namespace FFmpegKit.Net.PackageTests;

/// <summary>
/// Shape checks for the packed FFmpegKit.Net.Full package - the cross-platform client.
/// </summary>
/// <remarks>
/// This package carries no native payload and does not embed either binding: its whole job is to
/// depend on the right *external* one - FFmpegKit.Net.Full.Android / .iOS, published to nuget.org
/// from sbokatuk/FFmpegKit.Android and sbokatuk/FFmpegKit.iOS - per target framework, at the exact
/// version pinned in Directory.Build.props. That is what these tests verify: the assembly is
/// present, and the nuspec's per-framework dependency groups point at the right package and
/// version rather than, say, a different variant or a stale pin.
/// </remarks>
public class CrossPlatformPackageTests
{
    [Fact]
    public void Package_carries_an_assembly_for_every_android_and_ios_target_framework()
    {
        using var package = Packages.OpenPackage(Packages.CrossPlatformPackageId);

        foreach (var tfm in Packages.AndroidTargetFrameworks.Concat(Packages.IosTargetFrameworks))
        {
            var expected = $"lib/{tfm}/{Packages.CrossPlatformPackageId}.dll";
            Assert.True(package.GetEntry(expected) is not null, $"{Packages.CrossPlatformPackageId} is missing '{expected}'.");
        }
    }

    [Fact]
    public void Every_android_target_framework_depends_on_the_pinned_android_binding_version()
    {
        using var package = Packages.OpenPackage(Packages.CrossPlatformPackageId);
        var nuspec = Packages.ReadNuspec(package, Packages.CrossPlatformPackageId);

        foreach (var tfm in Packages.AndroidTargetFrameworks)
        {
            var ids = Packages.DependencyIds(nuspec, tfm).ToList();

            Assert.Contains(Packages.AndroidPackageId, ids);
            Assert.DoesNotContain(Packages.IosPackageId, ids);
            Assert.Equal(Packages.AndroidPackageVersion, Packages.DependencyVersion(nuspec, tfm, Packages.AndroidPackageId));
        }
    }

    [Fact]
    public void Every_ios_target_framework_depends_on_the_pinned_ios_binding_version()
    {
        using var package = Packages.OpenPackage(Packages.CrossPlatformPackageId);
        var nuspec = Packages.ReadNuspec(package, Packages.CrossPlatformPackageId);

        foreach (var tfm in Packages.IosTargetFrameworks)
        {
            var ids = Packages.DependencyIds(nuspec, tfm).ToList();

            Assert.Contains(Packages.IosPackageId, ids);
            Assert.DoesNotContain(Packages.AndroidPackageId, ids);
            Assert.Equal(Packages.IosPackageVersion, Packages.DependencyVersion(nuspec, tfm, Packages.IosPackageId));
        }
    }

    [Fact]
    public void Nuspec_declares_the_matching_license_expression()
    {
        using var package = Packages.OpenPackage(Packages.CrossPlatformPackageId);
        var nuspec = Packages.ReadNuspec(package, Packages.CrossPlatformPackageId);

        var ns = nuspec.Root!.Name.Namespace;
        var expression = nuspec.Root!.Element(ns + "metadata")?.Element(ns + "license")?.Value;

        // No native payload of its own, but the licence still follows the binding it pulls in -
        // that is what a consumer actually ends up shipping transitively.
        Assert.Equal(Packages.LicenseExpression(), expression);
    }

    [Fact]
    public void Package_ships_the_licence_texts_it_declares()
    {
        using var package = Packages.OpenPackage(Packages.CrossPlatformPackageId);

        var bindings = Packages.ReadEntryText(package, "licenses/LICENSE");
        Assert.Contains("MIT License", bindings, StringComparison.OrdinalIgnoreCase);

        // The GPL and LGPL texts differ only subtly at a glance - the LGPL is titled "GNU LESSER
        // GENERAL PUBLIC LICENSE" - so assert both the file name and the title, to catch the two
        // being swapped as well as a variant being mapped to the wrong licence.
        var expectedFile = $"licenses/{Packages.NativeLicense().Replace("-only", string.Empty)}.txt";
        var expectedTitle = Packages.IsGpl()
            ? "GNU GENERAL PUBLIC LICENSE"
            : "GNU LESSER GENERAL PUBLIC LICENSE";

        var native = Packages.ReadEntryText(package, expectedFile);

        Assert.StartsWith(expectedTitle, native.TrimStart(), StringComparison.Ordinal);
        Assert.Contains("Version 3", native, StringComparison.Ordinal);
    }
}
