# FFmpegKit.Net.Sample

A MAUI app that runs real FFmpeg conversions against the packed `FFmpegKit.Net.Full.Maui`
package - resize, grayscale and audio extraction - with a before/after video preview.

## Running it

Pick a conversion, tap **Convert sample video**, and compare the result against the bundled
sample clip. Progress is reported live via `IProgress<FFmpegProgress>` while the transcode runs,
**Cancel** stops it mid-run (the awaited call completes with `Cancelled` rather than throwing),
a success reports the session's `Duration`, and a failure prints the last line of the session's
`Output` - FFmpeg's own error message - straight off the result.

The sample consumes the packed `FFmpegKit.Net.Full.Maui` package from `../artifacts`, so pack
first:

```sh
./build/BuildNugets.sh                                     # from the repository root
dotnet build samples/FFmpegKit.Net.Sample -f net9.0-android35.0
dotnet build samples/FFmpegKit.Net.Sample -f net9.0-ios18.0
```

Pass `-p:FFmpegKitVersion=<version>` to build against a specific packed version rather than the
default (`VersionPrefix` from `Directory.Build.props`, i.e. the version this repository packs).

It references the `Full` (LGPL) variant deliberately - swapping to a `-Gpl` one would make the
sample itself GPL-3.0. Deliberately **not** in `FFmpegKit.Net.sln`, so that
`dotnet build FFmpegKit.Net.sln` never requires the MAUI workload.

## What is worth reading

The platform bindings do not look alike - Android cancels through a static
`FFmpegKit.Cancel(sessionId)` and reports progress via a Java listener interface, iOS calls
`session.Cancel()` on the session itself and reports progress via an Objective-C block - but
[`MainPage.xaml.cs`](FFmpegKit.Net.Sample/MainPage.xaml.cs) has none of that: one
`_ffmpeg.ExecuteAsync(command, progress, sourceDuration, token)` call, one code path, no
per-platform branch, thanks to [`FFmpegKit.Net`](../src/FFmpegKit.Net).

The page also shows the dependency-injection surface end to end: `UseFFmpegKit()` in
[`MauiProgram.cs`](FFmpegKit.Net.Sample/MauiProgram.cs) registers `IFFmpegKit`, the page is
`AddTransient`'d, and Shell hands the interface in through the constructor - so the page depends
on something a unit test can fake, and never touches the static classes.

| | Android | iOS |
| --- | --- | --- |
| Native namespace | `Ffmpegkit.Droid` | `Ffmpegkit.Ios` |
| Cancellation | static `FFmpegKit.Cancel(sessionId)` | `session.Cancel()` |
| Progress callback | Java `IStatisticsCallback` | Objective-C `StatisticsCallback` block |
| Probed media info | typed accessors already on the binding | parsed here via the shared `MediaValues` |

One thing that is easy to get wrong and is commented in the code: the app's own root namespace is
`FFmpegKit.Net.Sample`, which makes the bare identifier `FFmpegKit` ambiguous with the implicit
`FFmpegKit` namespace that declaration introduces - static calls would need qualifying as
`Ffmpegkit.Net.FFmpegKit.ExecuteAsync(...)` rather than just `FFmpegKit.ExecuteAsync(...)`.
Injecting `IFFmpegKit` sidesteps the collision entirely, which is one more reason this sample
prefers it; the comment beside the constructor spells it out.
