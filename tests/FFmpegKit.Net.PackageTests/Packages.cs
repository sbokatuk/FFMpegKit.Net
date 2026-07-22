using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace FFmpegKit.Net.PackageTests;

/// <summary>
/// Locates the packed .nupkg files under test.
/// </summary>
/// <remarks>
/// Scoped to the <c>Full</c> variant only - the repository builds all eight, but package-shape
/// checks are identical in kind across variants (only the licence expression and native payload
/// size differ, both of which are exercised here too), so exhaustively repeating them eight times
/// over would mostly restate <see cref="IsGpl"/>. Set <c>FFMPEGKIT_ARTIFACTS</c> to point at a
/// different <c>./artifacts</c> directory than the repository root's.
/// </remarks>
public static class Packages
{
    public const string Variant = "Full";

    /// <summary>Target frameworks every variant-parameterized package must carry.</summary>
    public static readonly string[] AndroidTargetFrameworks =
    [
        "net8.0-android34.0", "net9.0-android35.0", "net10.0-android36.0",
    ];

    public static readonly string[] IosTargetFrameworks =
    [
        "net8.0-ios18.0", "net9.0-ios18.0", "net10.0-ios26.0",
    ];

    /// <summary>The xcframeworks the iOS binding ships: FFmpegKit's own plus the seven FFmpeg libraries.</summary>
    public static readonly string[] ExpectedXcFrameworks =
    [
        "ffmpegkit", "libavcodec", "libavdevice", "libavfilter",
        "libavformat", "libavutil", "libswresample", "libswscale",
    ];

    public static bool IsGpl(string variant = Variant) => variant.EndsWith("Gpl", StringComparison.Ordinal);

    public static string NativeLicense(string variant = Variant) => IsGpl(variant) ? "GPL-3.0-only" : "LGPL-3.0-only";

    public static string LicenseExpression(string variant = Variant) => $"MIT AND {NativeLicense(variant)}";

    public static string AndroidPackageId => $"FFmpegKit.Net.{Variant}.Android";
    public static string IosPackageId => $"FFmpegKit.Net.{Variant}.iOS";
    public static string CrossPlatformPackageId => $"FFmpegKit.Net.{Variant}";
    public static string MauiPackageId => $"FFmpegKit.Net.{Variant}.Maui";

    /// <summary>Identifies the simulator slice of an xcframework from its directory name.</summary>
    public static bool IsSimulatorSlice(string slice) => slice.Contains("simulator", StringComparison.Ordinal);

    /// <summary>Whether a slice directory name denotes an iOS slice at all.</summary>
    public static bool IsIosSlice(string slice) => slice.StartsWith("ios-", StringComparison.Ordinal);

    public static string ArtifactsDirectory { get; } = ResolveArtifactsDirectory();

    public static string FindPackage(string packageId, string extension = ".nupkg")
    {
        // "FFmpegKit.Net.Full.*" also matches "FFmpegKit.Net.Full.Android.<version>", since
        // "Full" is a prefix of "Full.Android" - so the character right after "{packageId}."
        // must start the version (a digit), not another id segment (a letter).
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

    public static XDocument ReadNuspec(ZipArchive package, string packageId)
    {
        var entry = package.GetEntry($"{packageId}.nuspec");
        Assert.NotNull(entry);

        using var stream = entry!.Open();
        return XDocument.Load(stream);
    }

    /// <summary>Reads a package entry fully into memory so it can be seeked.</summary>
    public static MemoryStream ReadEntry(ZipArchive package, string entryName)
    {
        var entry = package.GetEntry(entryName);
        Assert.True(entry is not null, $"Package has no entry '{entryName}'.");

        var buffer = new MemoryStream();
        using (var stream = entry!.Open())
        {
            stream.CopyTo(buffer);
        }

        buffer.Position = 0;
        return buffer;
    }

    /// <summary>
    /// The iOS binding's native payload for a target framework - the xcframeworks, zipped into a
    /// binding resource package that sits beside the assembly (CompressBindingResourcePackage).
    /// </summary>
    public static ZipArchive OpenNativePayload(ZipArchive package, string targetFramework) =>
        new(ReadEntry(package, $"lib/{targetFramework}/{Packages.IosPackageId}.resources.zip"));

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
