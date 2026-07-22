namespace FFmpegKit.Net.PackageTests;

/// <summary>
/// Shape checks for the packed FFmpegKit.Net.Full package - the cross-platform client.
/// </summary>
/// <remarks>
/// Unlike the two bindings, this package carries no native payload of its own: its whole job is
/// to depend on the right platform binding per target framework, so that is what these tests
/// verify - the assembly is present, and the nuspec's per-framework dependency groups point at
/// FFmpegKit.Net.Full.Android / .iOS rather than, say, a different variant.
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
    public void Every_android_target_framework_depends_on_the_android_binding()
    {
        using var package = Packages.OpenPackage(Packages.CrossPlatformPackageId);
        var nuspec = Packages.ReadNuspec(package, Packages.CrossPlatformPackageId);

        foreach (var tfm in Packages.AndroidTargetFrameworks)
        {
            var ids = Packages.DependencyIds(nuspec, tfm).ToList();

            Assert.Contains(Packages.AndroidPackageId, ids);
            Assert.DoesNotContain(Packages.IosPackageId, ids);
        }
    }

    [Fact]
    public void Every_ios_target_framework_depends_on_the_ios_binding()
    {
        using var package = Packages.OpenPackage(Packages.CrossPlatformPackageId);
        var nuspec = Packages.ReadNuspec(package, Packages.CrossPlatformPackageId);

        foreach (var tfm in Packages.IosTargetFrameworks)
        {
            var ids = Packages.DependencyIds(nuspec, tfm).ToList();

            Assert.Contains(Packages.IosPackageId, ids);
            Assert.DoesNotContain(Packages.AndroidPackageId, ids);
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
}
