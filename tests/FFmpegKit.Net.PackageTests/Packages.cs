using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace FFmpegKit.Net.PackageTests;

/// <summary>
/// Locates the packed .nupkg files under test.
/// </summary>
/// <remarks>
/// Scoped to the <c>Full</c> variant only - the repository builds all eight, but package-shape
/// checks are identical in kind across variants (only the licence expression differs, which is
/// exercised here too), so exhaustively repeating them eight times over would mostly restate
/// <see cref="IsGpl"/>. Set <c>FFMPEGKIT_ARTIFACTS</c> to point at a different <c>./artifacts</c>
/// directory than the repository root's.
/// </remarks>
public static class Packages
{
    public const string Variant = "Full";

    /// <summary>
    /// The exact external binding versions pinned in Directory.Build.props
    /// (FFmpegKitAndroidPackageVersion / FFmpegKitIosPackageVersion). Kept here too so a pin bump
    /// that forgets to update the other is caught by <see cref="CrossPlatformPackageTests"/>
    /// rather than discovered at restore time by a consumer.
    /// </summary>
    public const string AndroidPackageVersion = "8.1.2.5";
    public const string IosPackageVersion = "8.1.2.3";
    public const string MacPackageVersion = "8.1.2.2";

    /// <summary>Target frameworks the cross-platform client must carry.</summary>
    public static readonly string[] AndroidTargetFrameworks =
    [
        "net8.0-android34.0", "net9.0-android35.0", "net10.0-android36.0",
    ];

    public static readonly string[] IosTargetFrameworks =
    [
        "net8.0-ios18.0", "net9.0-ios18.0", "net10.0-ios26.0",
    ];

    /// <summary>
    /// Client only: FFmpegKit.Net.Maui stays Android+iOS, since MAUI has no net*-macos head (its
    /// "Mac" is Mac Catalyst, for which no native FFmpegKit slice exists).
    /// </summary>
    public static readonly string[] MacTargetFrameworks =
    [
        "net8.0-macos14.0", "net9.0-macos15.0", "net10.0-macos26.0",
    ];

    public static bool IsGpl(string variant = Variant) => variant.EndsWith("Gpl", StringComparison.Ordinal);

    public static string NativeLicense(string variant = Variant) => IsGpl(variant) ? "GPL-3.0-only" : "LGPL-3.0-only";

    public static string LicenseExpression(string variant = Variant) => $"MIT AND {NativeLicense(variant)}";

    /// <summary>
    /// The external bindings this repository depends on but does not pack itself - published to
    /// nuget.org from sbokatuk/FFmpegKit.Android and sbokatuk/FFmpegKit.iOS.
    /// </summary>
    public static string AndroidPackageId => $"FFmpegKit.Net.{Variant}.Android";
    public static string IosPackageId => $"FFmpegKit.Net.{Variant}.iOS";
    public static string MacPackageId => $"FFmpegKit.Net.{Variant}.Mac";

    public static string CrossPlatformPackageId => $"FFmpegKit.Net.{Variant}";
    public static string MauiPackageId => $"FFmpegKit.Net.{Variant}.Maui";

    public static string ArtifactsDirectory { get; } = ResolveArtifactsDirectory();

    public static string FindPackage(string packageId, string extension = ".nupkg")
    {
        // "FFmpegKit.Net.Full.*" also matches "FFmpegKit.Net.Full.Maui.<version>", since "Full"
        // is a prefix of "Full.Maui" - so the character right after "{packageId}." must start
        // the version (a digit), not another id segment (a letter).
        var versionPattern = $"{Regex.Escape(packageId)}\\.\\d";

        var matches = Directory.Exists(ArtifactsDirectory)
            ? Directory.GetFiles(ArtifactsDirectory, $"{packageId}.*{extension}")
                .Where(f => Regex.IsMatch(Path.GetFileName(f), versionPattern))
                .Where(f => !f.EndsWith(".symbols.nupkg", StringComparison.OrdinalIgnoreCase))
                .ToArray()
            : [];

        Assert.True(
            matches.Length > 0,
            $"No {packageId}*{extension} found in '{ArtifactsDirectory}'. " +
            "Run build/BuildNugets.sh (or the CI pack step) first.");

        // A rebuilt working copy can leave several versions behind; test the newest.
        return matches.OrderByDescending(File.GetLastWriteTimeUtc).First();
    }

    public static ZipArchive OpenPackage(string packageId, string extension = ".nupkg") =>
        ZipFile.OpenRead(FindPackage(packageId, extension));

    public static string ReadEntryText(ZipArchive package, string entryName)
    {
        var entry = package.GetEntry(entryName);
        Assert.True(entry is not null, $"'{entryName}' is not packed.");

        using var reader = new StreamReader(entry!.Open());
        return reader.ReadToEnd();
    }

    public static XDocument ReadNuspec(ZipArchive package, string packageId)
    {
        var entry = package.GetEntry($"{packageId}.nuspec");
        Assert.NotNull(entry);

        using var stream = entry!.Open();
        return XDocument.Load(stream);
    }

    /// <summary>Every dependency group's target framework, read from a package's nuspec.</summary>
    public static IEnumerable<string> DependencyGroupFrameworks(XDocument nuspec)
    {
        var ns = nuspec.Root!.Name.Namespace;
        return nuspec.Descendants(ns + "group")
            .Select(g => g.Attribute("targetFramework")?.Value)
            .Where(tfm => tfm is not null)!;
    }

    /// <summary>Every dependency id declared under a target framework group, read from a package's nuspec.</summary>
    public static IEnumerable<string> DependencyIds(XDocument nuspec, string targetFramework)
    {
        var ns = nuspec.Root!.Name.Namespace;
        return nuspec.Descendants(ns + "group")
            .Where(g => g.Attribute("targetFramework")?.Value == targetFramework)
            .SelectMany(g => g.Elements(ns + "dependency"))
            .Select(d => d.Attribute("id")?.Value)
            .Where(id => id is not null)!;
    }

    /// <summary>
    /// The pinned version of one dependency under a target framework group, or null if that
    /// dependency is not declared there. The brackets ("[8.1.2.5]") NuGet writes for an exact
    /// version pin are stripped, since the tests compare against the plain version string.
    /// </summary>
    public static string? DependencyVersion(XDocument nuspec, string targetFramework, string dependencyId)
    {
        var ns = nuspec.Root!.Name.Namespace;
        var version = nuspec.Descendants(ns + "group")
            .Where(g => g.Attribute("targetFramework")?.Value == targetFramework)
            .SelectMany(g => g.Elements(ns + "dependency"))
            .FirstOrDefault(d => d.Attribute("id")?.Value == dependencyId)
            ?.Attribute("version")?.Value;

        return version?.Trim('[', ']');
    }

    private static string ResolveArtifactsDirectory()
    {
        if (Environment.GetEnvironmentVariable("FFMPEGKIT_ARTIFACTS") is { Length: > 0 } configured)
            return Path.GetFullPath(configured);

        // Walk up to the repository root (the directory holding global.json).
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "global.json")))
            directory = directory.Parent;

        return Path.Combine(directory?.FullName ?? AppContext.BaseDirectory, "artifacts");
    }
}
