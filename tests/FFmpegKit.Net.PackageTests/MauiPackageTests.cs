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

            // Deliberate per-band floors, not the packing machine's workload version - one CI
            // image packed a 9.0.120 floor and broke every consumer pinning an older Controls
            // patch. Must match FFmpegKitMauiControlsVersion in FFmpegKit.Net.Maui.csproj.
            var expectedControlsFloor = tfm.Split('.')[0] switch
            {
                "net8" => "8.0.100",
                "net9" => "9.0.30",
                _ => "10.0.0",
            };
            Assert.Equal(expectedControlsFloor, Packages.DependencyVersion(nuspec, tfm, "Microsoft.Maui.Controls"));
        }
    }

    [Fact]
    public void Package_ships_the_licence_texts_it_declares()
    {
        using var package = Packages.OpenPackage(Packages.MauiPackageId);

        var bindings = Packages.ReadEntryText(package, "licenses/LICENSE");
        Assert.Contains("MIT License", bindings, StringComparison.OrdinalIgnoreCase);

        var expectedFile = $"licenses/{Packages.NativeLicense().Replace("-only", string.Empty)}.txt";
        var native = Packages.ReadEntryText(package, expectedFile);

        var expectedTitle = Packages.IsGpl()
            ? "GNU GENERAL PUBLIC LICENSE"
            : "GNU LESSER GENERAL PUBLIC LICENSE";
        Assert.StartsWith(expectedTitle, native.TrimStart(), StringComparison.Ordinal);
    }
}
