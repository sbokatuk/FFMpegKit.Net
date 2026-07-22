# FFmpegKit.Net

.NET bindings for the native **FFmpegKit** library, with one API across Android and iOS.
Run FFmpeg and FFprobe commands from C#, in .NET MAUI or plain .NET for Android / iOS.

```sh
dotnet add package FFmpegKit.Net.Full.Maui   # MAUI apps: adds the app-builder wiring
dotnet add package FFmpegKit.Net.Full        # everything else
```

```csharp
using Ffmpegkit.Net;

var session = await FFmpegKit.ExecuteAsync("-i input.mov -c:v libx264 output.mp4");

if (session.Succeeded)
    Console.WriteLine("done");
```

## Packages

Every package below comes in eight variants - substitute `Full` for `Audio`, `FullGpl`, `Https`,
`HttpsGpl`, `Min`, `MinGpl` or `Video` depending on which FFmpeg build you need. See
[License](#license) before picking a `-Gpl` one.

| Package | What it is | Built by |
| --- | --- | --- |
| `FFmpegKit.Net.<Variant>.Maui` | MAUI app-builder wiring and a FilePicker helper | this repository |
| `FFmpegKit.Net.<Variant>` | The cross-platform client: `FFmpegKit`, `FFprobeKit`, `FFmpegKitConfig` | this repository |
| `FFmpegKit.Net.<Variant>.Android` | The raw binding to the native FFmpegKit Android SDK | [sbokatuk/FFmpegKit.Android][ffmpegkit-android] |
| `FFmpegKit.Net.<Variant>.iOS` | The raw binding to the native FFmpegKit iOS SDK | [sbokatuk/FFmpegKit.iOS][ffmpegkit-ios] |

Each package pulls in the one below it, so a single reference is enough. This repository only
builds the top two - the cross-platform client and the MAUI package - and depends on the platform
bindings as ordinary NuGet packages already published to nuget.org, pinned to an exact version in
[Directory.Build.props](Directory.Build.props) (`FFmpegKitAndroidPackageVersion` /
`FFmpegKitIosPackageVersion`). Drop to a platform binding directly when you need something the
cross-platform API does not expose - the full SDK surface is there under `Ffmpegkit.Droid.*`
(Android) and `Ffmpegkit.Ios.*` (iOS).

## Why there is a cross-platform layer

The two bindings are faithful, independent projections of FFmpegKit's own Java and Objective-C
APIs, and they do not resemble each other in the small print: Android cancels a session through a
static `FFmpegKit.Cancel(sessionId)`, iOS calls `session.Cancel()` on the session itself; the two
`Statistics` and `MediaInformation` types are unrelated generated classes with a parallel but not
identical shape (Android boxes some FFprobe numbers as Java `Long`s, iOS keeps everything as
`NSString`). `FFmpegKit.Net` is the layer that hides that, so an app targeting both platforms is
not the one writing the adapter between `Ffmpegkit.Droid` and `Ffmpegkit.Ios` by hand.

The sample is the evidence: [`samples/FFmpegKit.Net.Sample`](samples) runs the same
`MainPage.xaml.cs` against both platforms, with no per-platform code at all.

## What is bound

Both platform bindings bind FFmpegKit's own API whole - `FFmpegKit`, `FFprobeKit`,
`FFmpegKitConfig`, `MediaInformation` and the rest - not just the entry points this repository's
cross-platform layer happens to cover. Where the native binaries come from, how each binding
versions itself, and how to build them from source are questions for their own repositories:
[sbokatuk/FFmpegKit.Android][ffmpegkit-android] and [sbokatuk/FFmpegKit.iOS][ffmpegkit-ios].

Android and iOS binding versions are pinned independently in
[Directory.Build.props](Directory.Build.props) and do not track each other - see that file's
comments before assuming one implies the other.

**Mac Catalyst is not supported.** Neither binding publishes a Catalyst slice.

## Building

See [docs/BUILD.md](docs/BUILD.md). In short:

```sh
build/BuildNugets.sh    # packs the cross-platform client and the MAUI package into ./artifacts
```

Requires macOS: both packages multi-target Android and iOS together, so restoring either needs
the iOS workload regardless of which platform's code you are exercising.

## License

> This section describes what upstream states. It is not legal advice - if the distinction
> matters for your product, get it reviewed.

The C# code in this repository is MIT. **The published NuGet packages are not** - each one
transitively pulls in a platform binding that embeds native FFmpeg binaries carrying their own
copyleft terms:

| Variant suffix | Native license | SPDX expression |
| --- | --- | --- |
| `Audio`, `Full`, `Https`, `Min`, `Video` | LGPL-3.0 | `MIT AND LGPL-3.0-only` |
| `FullGpl`, `HttpsGpl`, `MinGpl` | **GPL-3.0** | `MIT AND GPL-3.0-only` |

The `-Gpl` variants enable `x264`, `x265`, `xvid` and `vidstab`, which are GPL - upstream keeps
them as separate artifacts specifically so they never contaminate the LGPL ones. **If your app is
closed-source, use a non-GPL variant.** Neither package built in this repository embeds any native
payload of its own, but each declares the same expression as whichever binding it resolves to for
a given target framework - that is what a consumer actually ends up shipping transitively.

[ffmpegkit-android]: https://github.com/sbokatuk/FFmpegKit.Android
[ffmpegkit-ios]: https://github.com/sbokatuk/FFmpegKit.iOS
