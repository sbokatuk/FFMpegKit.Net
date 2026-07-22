namespace FFmpegKit.Net.PackageTests;

/// <summary>Shape checks for the packed FFmpegKit.Net.Full.iOS package.</summary>
public class IosPackageTests
{
    /// <summary>
    /// The payload checks decompress tens of megabytes and the payload is identical across
    /// target frameworks (see <see cref="Native_payload_is_the_same_across_target_frameworks"/>),
    /// so the heavier checks below run against one rather than all three.
    /// </summary>
    private const string PayloadTargetFramework = "net8.0-ios18.0";

    [Fact]
    public void Package_carries_a_binding_assembly_for_every_target_framework()
    {
        using var package = Packages.OpenPackage(Packages.IosPackageId);

        foreach (var tfm in Packages.IosTargetFrameworks)
        {
            var expected = $"lib/{tfm}/{Packages.IosPackageId}.dll";
            Assert.True(package.GetEntry(expected) is not null, $"{Packages.IosPackageId} is missing '{expected}'.");
        }
    }

    [Fact]
    public void Package_carries_the_native_payload_for_every_target_framework()
    {
        using var package = Packages.OpenPackage(Packages.IosPackageId);

        foreach (var tfm in Packages.IosTargetFrameworks)
        {
            var entry = package.GetEntry($"lib/{tfm}/{Packages.IosPackageId}.resources.zip");

            Assert.True(
                entry is not null,
                $"{Packages.IosPackageId} is missing 'lib/{tfm}/{Packages.IosPackageId}.resources.zip'. " +
                "Has CompressBindingResourcePackage been unset?");

            // The native payload is tens of megabytes; anything tiny means an empty placeholder.
            Assert.True(entry!.Length > 10_000_000, $"'{entry.FullName}' is only {entry.Length} bytes.");
        }
    }

    [Fact]
    public void Native_payload_is_the_same_across_target_frameworks()
    {
        using var package = Packages.OpenPackage(Packages.IosPackageId);

        // net8/net9 come from the .NET 9 SDK pass and net10 from the .NET 10 one, then
        // merge-packages.py grafts the net10 lib/ tree into the other package - this is where a
        // mismatched graft (wrong variant, wrong native version) would be caught.
        //
        // Compared by logical content, not bytes: each pass re-zips the payload with its own
        // timestamps, so the same frameworks legitimately produce different CRCs.
        var manifests = new List<(string Tfm, List<(string Name, long Length)> Entries)>();

        foreach (var tfm in Packages.IosTargetFrameworks)
        {
            using var payload = Packages.OpenNativePayload(package, tfm);
            manifests.Add((tfm, payload.Entries
                .Select(e => (e.FullName, e.Length))
                .OrderBy(e => e.FullName, StringComparer.Ordinal)
                .ToList()));
        }

        var reference = manifests[0];
        foreach (var (tfm, entries) in manifests.Skip(1))
        {
            Assert.True(
                reference.Entries.SequenceEqual(entries),
                $"The native payload for {tfm} differs from {reference.Tfm} in {Packages.IosPackageId}.");
        }
    }

    [Fact]
    public void Native_payload_carries_every_xcframework()
    {
        using var package = Packages.OpenPackage(Packages.IosPackageId);
        using var payload = Packages.OpenNativePayload(package, PayloadTargetFramework);

        var present = payload.Entries
            .Select(e => e.FullName.Split('/')[0])
            .Where(name => name.EndsWith(".xcframework", StringComparison.Ordinal))
            .Distinct()
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();

        var expected = Packages.ExpectedXcFrameworks
            .Select(name => $"{name}.xcframework")
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(expected, present);
    }

    [Fact]
    public void Native_payload_carries_ios_slices_only()
    {
        using var package = Packages.OpenPackage(Packages.IosPackageId);
        using var payload = Packages.OpenNativePayload(package, PayloadTargetFramework);

        foreach (var framework in Packages.ExpectedXcFrameworks)
        {
            // parts.Length > 2 (not just > 1) excludes the xcframework's own top-level
            // Info.plist - "ffmpegkit.xcframework/Info.plist" splits to only 2 parts and is not
            // a slice; a real slice always nests further, e.g. "ios-arm64/ffmpegkit.framework/...".
            var slices = payload.Entries
                .Select(e => e.FullName.Split('/'))
                .Where(parts => parts.Length > 2 && parts[0] == $"{framework}.xcframework")
                .Select(parts => parts[1])
                .Distinct()
                .ToList();

            // Device and simulator, and nothing else. A macOS slice creeping back in would inflate
            // the package by roughly a third for no benefit - it cannot be reached from a
            // net*-ios binding. Checked by shape rather than by name: upstream has renamed the
            // device slice between releases (ios-arm64_arm64e -> ios-arm64).
            Assert.All(slices, slice => Assert.True(
                Packages.IsIosSlice(slice),
                $"{framework}.xcframework carries a non-iOS slice '{slice}'."));

            Assert.Single(slices, Packages.IsSimulatorSlice);
            Assert.Single(slices, slice => !Packages.IsSimulatorSlice(slice));
        }
    }

    [Fact]
    public void Nuspec_declares_the_matching_license_expression()
    {
        using var package = Packages.OpenPackage(Packages.IosPackageId);
        var nuspec = Packages.ReadNuspec(package, Packages.IosPackageId);

        var ns = nuspec.Root!.Name.Namespace;
        var expression = nuspec.Root!.Element(ns + "metadata")?.Element(ns + "license")?.Value;

        Assert.Equal(Packages.LicenseExpression(), expression);
    }
}
