# Building FFmpegKit.Net

Everything here is driven by the pins in [`Directory.Build.props`](../Directory.Build.props) and
run by the scripts in `build/` and `src/FFmpegKit.Net.Android/Jars/`. CI runs exactly these
scripts, so a green local run means the same thing a green pipeline does.

## Layout

```
Directory.Build.props        native version pins, target frameworks, shared package metadata
global.json                  pins the .NET 9 SDK (the "net9 band")
NuGet.config                 nuget.org + ./artifacts, so tests and the sample consume packed packages
licenses/                    MIT (bindings), LGPL-3.0.txt / GPL-3.0.txt (native FFmpeg)
build/
  BuildNugets.sh              two-pass pack + merge -> ./artifacts, for every project and variant
  merge-packages.py           combines the two SDK-band passes into one package per id
  FetchXcFrameworks.sh        downloads the iOS xcframeworks for all eight variants
  native-versions.tsv         maps the FFmpeg version to the Android FFmpegKit release that packages it
src/
  FFmpegKit.Net.Android/       the Android binding (Jars/FetchJars.sh, Transforms/, Additions/)
  FFmpegKit.Net.iOS/           the iOS binding (ApiDefinition.cs, Structs.cs, Additions/, libs/)
  FFmpegKit.Net/               the cross-platform client (FFmpegKit, FFprobeKit, FFmpegKitConfig)
  FFmpegKit.Net.Maui/          the .NET MAUI package (app-builder wiring, FilePicker helper)
tests/
  FFmpegKit.Net.PackageTests/  inspects the packed .nupkg files for the Full variant
samples/
  FFmpegKit.Net.Sample/        one MAUI sample exercising FFmpegKit.Net.Full.Maui on both platforms
```

The solution spans .NET 8/9/10 and both platforms, so no single SDK can build all of it at once -
`dotnet build FFmpegKit.Net.sln` will fail on whichever band the current SDK does not own. Build
individual projects, or use `build/BuildNugets.sh`, which handles the bands. The sample is
deliberately **not** in the `.sln`, so `dotnet build FFmpegKit.Net.sln` never requires the MAUI
workload.

## Why eight variants, and why per-project obj/bin

`FFmpegKitBuildType` (`Audio`, `Full`, `FullGpl`, `Https`, `HttpsGpl`, `Min`, `MinGpl`, `Video`)
selects which native FFmpegKit build a project links against. Every project whose output depends
on it - both bindings, the cross-platform client, the MAUI package - gets its own `obj/<Variant>/`
and `bin/<Variant>/` (see `Directory.Build.props`), because all eight are built one after another
in the same working copy and a shared intermediate output would let one variant's native payload
leak into the next one's package.

The cross-platform and MAUI packages are parameterized the same way even though their C# source
never changes across variants: `Ffmpegkit.Ios.FFmpegKit` and `Ffmpegkit.Droid.FFmpegKit` are
identical across variants, but each variant is a distinct assembly
(`FFmpegKit.Net.Video.iOS` vs `FFmpegKit.Net.Full.iOS`, ...), so a cross-platform assembly built
against one is bound to that variant's assembly identity.

## Why two passes

No single .NET SDK can build net8, net9 and net10 for a given platform - each SDK's workload
carries the current target framework and the previous one only. Verified support matrix:

| | SDK 9 band | SDK 10 band |
| --- | --- | --- |
| Android | `net8.0-android34.0`, `net9.0-android35.0` | `net10.0-android36.0` |
| iOS | `net8.0-ios18.0`, `net9.0-ios18.0` | `net10.0-ios26.0` |

So `build/BuildNugets.sh` packs every project twice per variant - once under the SDK
`global.json` pins, once from a scratch directory whose own `global.json` selects the .NET 10 SDK
- and `build/merge-packages.py` copies the missing `lib/<tfm>` trees from the second package into
the first, adding the matching nuspec dependency groups.

The platform version in each target framework is pinned deliberately. Bare `net8.0-android`
resolves to `android21.0`, and bare `net8.0-ios` resolves to a similarly unpinned default - both
produce a binding assembly with no native payload, or silently change the `lib/<tfm>/` folder
name a workload update would move.

## Native sources

### Android

`src/FFmpegKit.Net.Android/Jars/FetchJars.sh` downloads `.aar` files from
[`ffmpegkit-maintained/ffmpeg`][android-fork] on Maven Central - the only one of FFmpegKit's three
relevant repositories that still ships usable Android binaries (the original `arthenica/ffmpeg-kit`
is archived with its release assets deleted; its successor `ffmpeg-kit-next` ships source only).
It also fetches the two `smart-exception` jars `FFmpegKitConfig`'s static initialiser needs at
runtime, which are neither bundled in the `.aar` nor declared in its `.pom`.

```sh
src/FFmpegKit.Net.Android/Jars/FetchJars.sh              # version from Directory.Build.props
src/FFmpegKit.Net.Android/Jars/FetchJars.sh 8.2.0         # override, e.g. to try a newer build
```

Reads `FFmpegKitAndroidNativeVersion` from the root `Directory.Build.props` - the same property
the `.csproj` uses to pick the `.aar` - so the download and what the project expects cannot drift
apart.

### iOS

`build/FetchXcFrameworks.sh` downloads xcframeworks from [`sk3llo/ffmpeg_kit_flutter`][ios-fork] -
the only source still publishing all eight variants for Apple platforms (`arthenica/ffmpeg-kit`'s
releases carry zero assets now; `ffmpeg-kit-next` is source only;
[`ffmpegkit-maintained/ffmpeg`][android-fork], the Android source, ships no Apple binaries at all).

```sh
build/FetchXcFrameworks.sh                    # every variant, version from Directory.Build.props
build/FetchXcFrameworks.sh 8.1.2 Video        # override the version, fetch a single variant
```

Reads `FFmpegKitIosNativeVersion` the same way. Each release is tagged `<version>-<variant>` and
carries every xcframework plus a `checksums.json`; every download is verified against it. Upstream
also ships a macOS slice in each xcframework, which is stripped on download - it cannot be reached
from a `net*-ios` binding, but would still be embedded once per target framework, pushing the
`FullGpl` package close to nuget.org's 250 MB limit.

Requires macOS with Xcode - iOS builds have no cross-platform path, unlike the Android binding.

**Android and iOS do not track the same FFmpeg numbering scheme, and are not built from the same
upstream fork.** Each has its own `FFmpegKit*NativeVersion` in `Directory.Build.props`; do not
assume bumping one also covers the other.

### Regenerating the iOS binding

Only needed when bumping to a newer native FFmpegKit version. Written by hand rather than
generated with [Objective Sharpie](https://learn.microsoft.com/dotnet/communitytoolkit/maui/)
each time - the binding's surface is stable and sharpie's raw output needs more cleanup than it is
worth for an incremental bump - but sharpie is still how a large version jump gets reconciled:

```sh
mkdir -p Headers
cp -R src/FFmpegKit.Net.iOS/libs/Video/ffmpegkit.xcframework/ios-arm64_arm64e/ffmpegkit.framework/Headers/* Headers/

# FFmpegKit.h alone only pulls in a fraction of the API - binding just that is how a previous
# generation ended up missing FFmpegKitConfig, FFprobeKit and the MediaInformation types.
ls Headers/*.h | grep -v fftools | grep -v ffmpegkit_exception \
  | sed 's|Headers/|#import "|; s|$|"|' > Headers/FFmpegKitUmbrella.h

sharpie bind -output Binding -sdk iphoneos26.5 -scope Headers Headers/FFmpegKitUmbrella.h -c -I Headers
```

Reconcile `Binding/ApiDefinitions.cs` / `Binding/StructsAndEnums.cs` into
`src/FFmpegKit.Net.iOS/ApiDefinition.cs` / `Structs.cs`. Every `[Verify]` attribute sharpie emits
must be reviewed and removed. Sharpie emits the `Level` enum as `ulong` despite its negative
members; it has to stay `long`.

## Packing

```sh
build/BuildNugets.sh                          # version from Directory.Build.props
build/BuildNugets.sh 8.1.2-beta.4             # explicit package version
build/BuildNugets.sh 8.1.2-beta.4 android     # only the Android binding
build/BuildNugets.sh 8.1.2-beta.4 apple       # only the iOS binding + cross-platform + Maui
```

`apple` requires macOS. With it, the Android package for the variant being built must already be
in `./artifacts` - the cross-platform client depends on both bindings by `PackageReference`, so it
can only restore once both are present; CI downloads the Android package from the pack-android job
before running the macOS one.

Output lands in `./artifacts`, which `NuGet.config` exposes as a package source so the tests and
the sample app resolve the packages that were just built rather than whatever is on nuget.org.

## Testing

```sh
build/BuildNugets.sh                                      # produce ./artifacts first
dotnet test tests/FFmpegKit.Net.PackageTests
```

Scoped to the `Full` variant - see [`Packages.cs`](../tests/FFmpegKit.Net.PackageTests/Packages.cs)
for why repeating identical shape checks across all eight variants adds little. Checks: a binding
assembly and native payload for every target framework on both platforms, the iOS payload
identical across the three target frameworks it is merged into, only iOS device/simulator slices
present (no macOS), the cross-platform and MAUI nuspecs' per-framework dependency groups pointing
at the right platform package, and the license expression on every package matching what `Full`
actually ships (LGPL-3.0).

## CI

Mirrors the two-runner split each binding used on its own: Android needs only a JDK and the
Android SDK and packs on `ubuntu-latest`; iOS needs Xcode, and the cross-platform and MAUI
packages multi-target both platforms together, so all three pack on macOS. Publishing (when
configured) should use nuget.org [trusted publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing)
rather than a long-lived API key, exactly as both source bindings already do.

[android-fork]: https://github.com/ffmpegkit-maintained/ffmpeg
[ios-fork]: https://github.com/sk3llo/ffmpeg_kit_flutter
