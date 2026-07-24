# Building FFmpegKit.Net

Everything here is driven by the pins in [`Directory.Build.props`](../Directory.Build.props) and
run by the scripts in `build/`. CI runs exactly these scripts, so a green local run means the same
thing a green pipeline does.

This repository does **not** build the Android or iOS bindings - see
[sbokatuk/FFmpegKit.Android](https://github.com/sbokatuk/FFmpegKit.Android) and
[sbokatuk/FFmpegKit.iOS](https://github.com/sbokatuk/FFmpegKit.iOS) for that. It only builds the
cross-platform client and the MAUI package, which depend on the two bindings as ordinary NuGet
packages already published to nuget.org.

## Layout

```
Directory.Build.props        external binding pins, target frameworks, shared package metadata
global.json                  pins the .NET 9 SDK (the "net9 band")
NuGet.config                 nuget.org + ./artifacts, so tests and the sample consume packed packages
licenses/                    LGPL-3.0.txt / GPL-3.0.txt (native FFmpeg); packed into each package together with the root MIT LICENSE
build/
  BuildNugets.sh              two-pass pack + merge -> ./artifacts, for both projects and every variant
  merge-packages.py           combines the two SDK-band passes into one package per id
src/
  FFmpegKit.Net/               the cross-platform client (FFmpegKit, FFprobeKit, FFmpegKitConfig)
  FFmpegKit.Net.Maui/          the .NET MAUI package (app-builder wiring, FilePicker helper)
tests/
  FFmpegKit.Net.UnitTests/     fast desktop tests for the platform-neutral logic (parsing, progress math)
  FFmpegKit.Net.PackageTests/  inspects the packed .nupkg files for the Full variant
  FFmpegKit.Net.DeviceTests/   on-device e2e for the cross-platform API (one project, Android + iOS heads)
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
selects which variant of the external Android/iOS bindings a project depends on. Both projects get
their own `obj/<Variant>/` and `bin/<Variant>/` (see `Directory.Build.props`), because all eight
are built one after another in the same working copy and a shared intermediate output would let
one variant's resolved dependency leak into the next one's package.

Both packages are parameterized the same way even though their C# source never changes across
variants: `Ffmpegkit.Ios.FFmpegKit` and `Ffmpegkit.Droid.FFmpegKit` are identical across variants,
but each variant is a distinct external assembly (`FFmpegKit.Net.Video.iOS` vs
`FFmpegKit.Net.Full.iOS`, ...), so an assembly built against one is bound to that variant's
assembly identity.

## Why two passes

No single .NET SDK can build net8, net9 and net10 for a given platform - each SDK's workload
carries the current target framework and the previous one only. Verified support matrix:

| | SDK 9 band | SDK 10 band |
| --- | --- | --- |
| Android | `net8.0-android34.0`, `net9.0-android35.0` | `net10.0-android36.0` |
| iOS | `net8.0-ios18.0`, `net9.0-ios18.0` | `net10.0-ios26.0` |
| macOS | `net8.0-macos14.0`, `net9.0-macos15.0` | `net10.0-macos26.0` |

macOS is carried by the cross-platform client only - `FFmpegKit.Net.Maui` stays Android+iOS,
since MAUI has no `net*-macos` head (its "Mac" is Mac Catalyst, which no native FFmpegKit build
supports).

So `build/BuildNugets.sh` packs each project twice per variant - once under the SDK `global.json`
pins, once from a scratch directory whose own `global.json` selects the .NET 10 SDK - and
`build/merge-packages.py` copies the missing `lib/<tfm>` trees from the second package into the
first, copying the matching nuspec dependency groups across too (not just adding an empty group -
that would silently drop the merged-in framework's dependency on the external binding).

The platform version in each target framework is pinned deliberately, and matches what the
external Android/iOS packages themselves target - a mismatch fails restore rather than silently
producing a package with no payload for that framework.

## External binding pins

`Directory.Build.props` pins `FFmpegKitAndroidPackageVersion` and `FFmpegKitIosPackageVersion` to
an exact version each - Android and iOS binding repositories version independently and do not
track each other, so bumping one is not assumed to imply the other.

To find the current version of a given variant, use nuget.org's search API rather than the
flat-container versions list - the latter also returns older, unlisted releases, which can sort
after the current one under plain version-string ordering:

```sh
curl -s "https://azuresearch-usnc.nuget.org/query?q=packageid:ffmpegkit.net.full.android&prerelease=false" \
  | python3 -c "import json,sys; print(json.load(sys.stdin)['data'][0]['version'])"
```

## Packing

```sh
build/BuildNugets.sh                              # version from Directory.Build.props
build/BuildNugets.sh 8.1.2-beta.4                  # explicit package version
build/BuildNugets.sh 8.1.2-beta.4 8.1.2.4 8.1.2.1  # override the Android/iOS binding pins
```

Requires macOS: both packages multi-target Android and iOS together, so restoring either needs
the iOS workload regardless of which platform's code you are exercising.

Output lands in `./artifacts`, which `NuGet.config` exposes as a package source so the tests and
the sample app resolve the packages that were just built rather than whatever is on nuget.org.
FFmpegKit.Net.Maui depends on FFmpegKit.Net by `PackageReference` (this same repository), so it is
packed second, once the local feed already has the other.

## Testing

Three tiers, cheapest first:

```sh
dotnet test tests/FFmpegKit.Net.UnitTests                 # no artifacts needed
build/BuildNugets.sh                                      # produce ./artifacts for the rest
dotnet test tests/FFmpegKit.Net.PackageTests
```

**UnitTests** compile the platform-neutral sources (`MediaValues`, `FFmpegProgress`, the result
records) directly into a plain `net9.0` project - the packable project only targets
`net*-android`/`net*-ios`, so it cannot be project-referenced from a desktop test - and cover the
invariant-culture parsing and the progress clamping/ETA rules without any device in the loop.

**PackageTests** are scoped to the `Full` variant - see
[`Packages.cs`](../tests/FFmpegKit.Net.PackageTests/Packages.cs) for why repeating identical shape
checks across all eight variants adds little. Checks: an assembly for every target framework on
both platforms, the cross-platform and MAUI nuspecs' per-framework dependency groups pointing at
the right external package **and version** (not just the right id - a merge that silently dropped
the pin would still pass an id-only check), the license expression on every package matching what
`Full` actually ships (LGPL-3.0), and the licence texts themselves being packed.

**DeviceTests** run one shared set of smoke checks - encode, probe, progress, cancellation, log
callbacks - through `Ffmpegkit.Net` only, hosted by a plain Android activity or a plain iOS app.
They consume the packed `FFmpegKit.Net.<Variant>` NuGet from `./artifacts` (which is why the
project is not in the `.sln`: a fresh clone has no artifacts to restore). The same scripts CI
uses run them locally:

```sh
# Android: against a booted emulator or device (override the RID for arm64 hardware)
FFMPEGKIT_DEVICE_RID=android-arm64 ./.github/scripts/run-device-tests.sh Video 8.1.2.1 net9.0-android35.0

# iOS: boots a simulator itself
./.github/scripts/run-simulator-tests.sh Video 8.1.2.1 net9.0-ios18.0
```

## CI

Everything packs on macOS: both packages multi-target Android and iOS together, so restoring
either needs the iOS workload regardless of which platform's code is being exercised - there is no
Android-only, Linux-runner leg the way there is in the two binding repositories.
