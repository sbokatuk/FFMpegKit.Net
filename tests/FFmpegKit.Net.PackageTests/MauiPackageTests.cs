namespace FFmpegKit.Net.PackageTests;

/// <summary>Shape checks for the packed FFmpegKit.Net.Full.Maui package.</summary>
public class MauiPackageTests
{
    [Fact]
    public void Package_carries_an_assembly_for_every_android_and_ios_target_framework()
    {
        using var package = Packages.OpenPackage(Packages.MauiPackageId);

        foreach (var tfm in Packages.AndroidTargetFrameworks.Concat(Packages.IosTargetFrameworks))
        {
            var expected = $"lib/{tfm}/{Packages.MauiPackageId}.dll";
            Assert.True(package.GetEntry(expected) is not null, $"{Packages.MauiPackageId} is missing '{expected}'.");
        }
    }

    [Fact]
    public void Every_target_framework_depends_on_the_cross_platform_client()
    {
        using var package = Packages.OpenPackage(Packages.MauiPackageId);
        var nuspec = Packages.ReadNuspec(package, Packages.MauiPackageId);

        foreach (var tfm in Packages.AndroidTargetFrameworks.Concat(Packages.IosTargetFrameworks))
        {
            var ids = Packages.DependencyIds(nuspec, tfm).ToList();

            Assert.Contains(Packages.CrossPlatformPackageId, ids);
            Assert.Contains("Microsoft.Maui.Controls", ids);
        }
    }
}
