namespace FFmpegKit.Net.PackageTests;

/// <summary>Shape checks for the packed FFmpegKit.Net.Full.Android package.</summary>
public class AndroidPackageTests
{
    [Fact]
    public void Package_carries_a_binding_assembly_for_every_target_framework()
    {
        using var package = Packages.OpenPackage(Packages.AndroidPackageId);

        foreach (var tfm in Packages.AndroidTargetFrameworks)
        {
            var expected = $"lib/{tfm}/{Packages.AndroidPackageId}.dll";
            Assert.True(package.GetEntry(expected) is not null, $"{Packages.AndroidPackageId} is missing '{expected}'.");
        }
    }

    [Fact]
    public void Nuspec_declares_the_matching_license_expression()
    {
        using var package = Packages.OpenPackage(Packages.AndroidPackageId);
        var nuspec = Packages.ReadNuspec(package, Packages.AndroidPackageId);

        var ns = nuspec.Root!.Name.Namespace;
        var expression = nuspec.Root!.Element(ns + "metadata")?.Element(ns + "license")?.Value;

        Assert.Equal(Packages.LicenseExpression(), expression);
    }

    [Fact]
    public void Package_ships_both_license_texts()
    {
        using var package = Packages.OpenPackage(Packages.AndroidPackageId);

        // Full is LGPL-3.0-only; the -Gpl variants carry GPL-3.0.txt instead - see Packages.IsGpl.
        Assert.True(package.GetEntry("licenses/LICENSE") is not null, "Package is missing the MIT licence text.");
        Assert.True(package.GetEntry("licenses/LGPL-3.0.txt") is not null, "Package is missing the LGPL-3.0 licence text.");
    }
}
