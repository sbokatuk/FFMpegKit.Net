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

| Package | What it is | Target frameworks |
| --- | --- | --- |
| `FFmpegKit.Net.<Variant>.Maui` | MAUI app-builder wiring and a FilePicker helper | net8.0, net9.0, net10.0 (android + ios) |
| `FFmpegKit.Net.<Variant>` | The cross-platform client: `FFmpegKit`, `FFprobeKit`, `FFmpegKitConfig` | net8.0, net9.0, net10.0 (android + ios) |
| `FFmpegKit.Net.<Variant>.Android` | The raw binding to the native FFmpegKit Android SDK | `net8.0-android34.0`, `net9.0-android35.0`, `net10.0-android36.0` |
| `FFmpegKit.Net.<Variant>.iOS` | The raw binding to the native FFmpegKit iOS SDK | `net8.0-ios18.0`, `net9.0-ios18.0`, `net10.0-ios26.0` |

Each package pulls in the one below it, so a single reference is enough. Drop to a platform
binding directly when you need something the cross-platform API does not expose - the full SDK
surface is there under `Ffmpegkit.Droid.*` (Android) and `Ffmpegkit.Ios.*` (iOS).

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

Both platforms bind FFmpegKit's own API whole - `FFmpegKit`, `FFprobeKit`, `FFmpegKitConfig`,
`MediaInformation` and the rest - not just the entry points this package's cross-platform layer
happens to cover.

**Android** binds the community-maintained [`ffmpegkit-maintained/ffmpeg`][android-fork] `.aar`,
via Maven Central. Keeps the upstream `com.arthenica.ffmpegkit` Java API, so the binding's
`Ffmpegkit.Droid` namespace is unaffected by the switch from the archived original.

**iOS** binds the [`sk3llo/ffmpeg_kit_flutter`][ios-fork] `.xcframework` release - the only
upstream still publishing all eight variants for Apple platforms. See
[docs/BUILD.md](docs/BUILD.md) for the full comparison of available native sources and why
neither binding uses the archived `arthenica/ffmpeg-kit` directly.

Android and iOS currently track the same FFmpeg version (8.1.2), but each binding's own binding
revision - and the FFmpegKit release it names its download after - advances independently; see
[Directory.Build.props](Directory.Build.props).

**Mac Catalyst is not supported.** Neither native source publishes a Catalyst slice.

## Building

See [docs/BUILD.md](docs/BUILD.md). In short:

```sh
src/FFmpegKit.Net.Android/Jars/FetchJars.sh     # downloads the .aar/.jar files
build/FetchXcFrameworks.sh                      # downloads the xcframeworks (macOS only)
build/BuildNugets.sh                            # packs everything into ./artifacts
```

## License

> This section describes what upstream states. It is not legal advice - if the distinction
> matters for your product, get it reviewed.

The C# binding and client code in this repository is MIT. **The published NuGet packages are
not** - each one embeds native FFmpeg binaries that carry their own copyleft terms:

| Variant suffix | Native license | SPDX expression |
| --- | --- | --- |
| `Audio`, `Full`, `Https`, `Min`, `Video` | LGPL-3.0 | `MIT AND LGPL-3.0-only` |
| `FullGpl`, `HttpsGpl`, `MinGpl` | **GPL-3.0** | `MIT AND GPL-3.0-only` |

The `-Gpl` variants enable `x264`, `x265`, `xvid` and `vidstab`, which are GPL - upstream keeps
them as separate artifacts specifically so they never contaminate the LGPL ones. **If your app is
closed-source, use a non-GPL variant.** The cross-platform and MAUI packages (no `.Android` or
`.iOS` in the id) carry no native payload of their own, but declare the same expression as
whichever binding they resolve to for a given target framework - that is what a consumer actually
ends up shipping transitively.

Every package ships the texts it is covered by under `licenses/` - `LICENSE` (MIT) and
`LGPL-3.0.txt` or `GPL-3.0.txt` (the native binaries). The same texts are in this repository under
[`licenses/`](licenses).

[android-fork]: https://github.com/ffmpegkit-maintained/ffmpeg
[ios-fork]: https://github.com/sk3llo/ffmpeg_kit_flutter
