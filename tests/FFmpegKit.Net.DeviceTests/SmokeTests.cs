using System.Globalization;
using Ffmpegkit.Net;
// This assembly's own root namespace is 'FFmpegKit', which would otherwise shadow the type - the
// same collision the sample documents.
using FFmpeg = Ffmpegkit.Net.FFmpegKit;

namespace FFmpegKit.Net.DeviceTests;

/// <summary>A single on-device check. Throws to fail.</summary>
/// <param name="Name">Human readable name, reported by the host.</param>
/// <param name="Execute">Runs the check. Receives a writable working directory.</param>
public sealed record SmokeTest(string Name, Func<string, Task> Execute);

/// <summary>
/// End-to-end checks for the cross-platform API that only mean anything on a real device or
/// simulator: they load the native FFmpeg libraries out of the platform binding and run actual
/// FFmpeg commands - but exclusively through <c>Ffmpegkit.Net</c>, never the platform namespaces.
/// The same file compiles into both the Android and the iOS head, which is itself the claim under
/// test: one code path, no platform branches.
/// </summary>
public static class SmokeTests
{
    public static SmokeTest[] All =>
    [
        new("ffmpeg -version succeeds", VersionCommandSucceeds),
        new("encodes raw frames to mp4 from a command string", EncodesFromACommandString),
        new("encodes raw frames to mp4 from pre-split arguments", EncodesFromPreSplitArguments),
        new("reports typed media information", TypedMediaInformationIsParsed),
        new("parses media values regardless of locale", TypedValuesIgnoreAmbientCulture),
        new("failing command completes with Failed, not an exception", FailingCommandIsReportedAsFailure),
        new("reports progress while encoding", ProgressIsReported),
        new("cancels a running command", CancellationStopsACommand),
        new("delivers log output to a delegate", LogDelegateReceivesOutput),
        new("log level round-trips", LogLevelRoundTrips),
        new("clears session history", SessionHistoryClears),
    ];

    private static async Task VersionCommandSucceeds(string workingDirectory)
    {
        // Reaching a result at all proves the native libraries loaded on this platform.
        var result = await FFmpeg.ExecuteAsync("-version");

        Assert(result.Succeeded, $"'-version' failed with return code {result.ReturnCode?.ToString() ?? "<null>"}.");
        Assert(!result.Failed && !result.Cancelled, "Succeeded, Failed and Cancelled disagree.");
        Assert(result.Output?.Contains("ffmpeg version") == true, "Output did not carry the version banner.");
        Assert(result.Command?.Contains("-version") == true, $"Command was '{result.Command ?? "<null>"}'.");
        Assert(ReferenceEquals(result.EnsureSuccess(), result), "EnsureSuccess() must return the same instance.");
        Report($"session {result.SessionId} return code {result.ReturnCode}");
    }

    private static async Task EncodesFromACommandString(string workingDirectory)
    {
        var input = Path.Combine(workingDirectory, "input.raw");
        var output = Path.Combine(workingDirectory, "output.mp4");
        WriteRawFrames(input);
        File.Delete(output);

        // rawvideo in, mpeg4 out: both are always present regardless of which FFmpegKit variant
        // is under test, and the scale filter exercises libavfilter/libswscale on the way through.
        var result = await FFmpeg.ExecuteAsync(BuildEncodeCommand(input, output));

        AssertSuccess(result, "string-command encode");
        Assert(File.Exists(output), $"'{output}' was not produced.");
        Report($"encoded {new FileInfo(output).Length} bytes");
    }

    private static async Task EncodesFromPreSplitArguments(string workingDirectory)
    {
        var input = Path.Combine(workingDirectory, "input.raw");
        var output = Path.Combine(workingDirectory, "args.mp4");
        WriteRawFrames(input);
        File.Delete(output);

        var result = await FFmpeg.ExecuteWithArgumentsAsync(
        [
            "-y",
            "-f", "rawvideo",
            "-pixel_format", "rgb24",
            "-video_size", $"{FrameWidth}x{FrameHeight}",
            "-framerate", "10",
            "-i", input,
            "-vf", "scale=64:64",
            "-c:v", "mpeg4",
            output,
        ]);

        AssertSuccess(result, "pre-split encode");
        Assert(File.Exists(output), $"'{output}' was not produced.");
    }

    private static async Task TypedMediaInformationIsParsed(string workingDirectory)
    {
        var output = Path.Combine(workingDirectory, "output.mp4");
        Assert(File.Exists(output), "The encode check must run before this one.");

        var information = await FFprobeKit.GetMediaInformationAsync(output);
        Assert(information is not null, "FFprobe returned no media information.");

        Assert(information!.Duration is not null, "Duration did not parse.");
        Assert(information.Duration!.Value > TimeSpan.Zero, $"Duration parsed as {information.Duration}.");
        Assert(information.SizeBytes is > 0, $"SizeBytes was {information.SizeBytes?.ToString() ?? "<null>"}.");

        var video = information.Streams.FirstOrDefault(s => s.IsVideo);
        Assert(video is not null, "No stream reported IsVideo.");

        // The encode step scales to 64x64, so these are known values rather than merely non-null.
        Assert(video!.PixelWidth == 64, $"PixelWidth was {video.PixelWidth?.ToString() ?? "<null>"}.");
        Assert(video.PixelHeight == 64, $"PixelHeight was {video.PixelHeight?.ToString() ?? "<null>"}.");
        Assert(video.AverageFrameRateFps is > 0, $"AverageFrameRateFps was {video.AverageFrameRateFps?.ToString() ?? "<null>"}.");

        Report($"duration={information.Duration} {video.PixelWidth}x{video.PixelHeight} @{video.AverageFrameRateFps:0.##}fps codec={video.Codec}");
    }

    private static async Task TypedValuesIgnoreAmbientCulture(string workingDirectory)
    {
        var output = Path.Combine(workingDirectory, "output.mp4");
        Assert(File.Exists(output), "The encode check must run before this one.");

        var invariant = (await FFprobeKit.GetMediaInformationAsync(output))?.Duration;
        Assert(invariant is not null, "FFprobe returned no duration to compare.");

        var previous = CultureInfo.CurrentCulture;
        try
        {
            // The whole point of MediaValues. Under de-DE the dot in "12.345000" reads as a group
            // separator and double.Parse returns 12,345,000; under fr-FR it throws.
            foreach (var culture in new[] { "de-DE", "fr-FR" })
            {
                CultureInfo.CurrentCulture = new CultureInfo(culture);
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture;

                var parsed = (await FFprobeKit.GetMediaInformationAsync(output))?.Duration;
                Assert(
                    parsed == invariant,
                    $"Duration parsed as {parsed} under {culture} but {invariant} under {previous.Name}.");
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
            CultureInfo.DefaultThreadCurrentCulture = null;
        }

        Report($"duration stable across locales: {invariant}");
    }

    private static async Task FailingCommandIsReportedAsFailure(string workingDirectory)
    {
        // A wrapper that mis-marshalled return codes would make every command look successful,
        // which would quietly defeat every other check here.
        var result = await FFmpeg.ExecuteAsync(
            $"-i \"{Path.Combine(workingDirectory, "does-not-exist.mp4")}\" -f null -");

        Assert(result.Failed, "FFmpeg reported success for a command that should have failed.");
        Assert(!result.Succeeded && !result.Cancelled, "Succeeded, Failed and Cancelled disagree.");
        Assert(result.ReturnCode is not null and not 0, $"Return code was {result.ReturnCode?.ToString() ?? "<null>"}.");

        // The reason a command failed must be readable off the result itself - dropping into the
        // platform binding to fish out the session log is exactly what this layer is for.
        Assert(!string.IsNullOrEmpty(result.Output), "A failed command must carry its console output.");

        try
        {
            result.EnsureSuccess();
            Assert(false, "EnsureSuccess() did not throw for a failed session.");
        }
        catch (FFmpegExecutionException exception)
        {
            Assert(ReferenceEquals(exception.Result, result), "The exception must carry the failed result.");
            Assert(exception.Message.Contains($"return code {result.ReturnCode}"), "The exception message must name the return code.");
        }
    }

    private static async Task ProgressIsReported(string workingDirectory)
    {
        var input = Path.Combine(workingDirectory, "progress.raw");
        var output = Path.Combine(workingDirectory, "progress.mp4");
        WriteRawFrames(input, frameCount: 1800);
        File.Delete(output);

        // Not Progress<T>: that posts to a synchronization context and would need a drain wait.
        // This is called synchronously on FFmpegKit's callback thread, so once the session task
        // completes every sample has been recorded.
        var progress = new CollectingProgress();

        // 1800 frames at 30fps is 60 seconds of material, upscaled so the encode cannot finish
        // inside FFmpeg's first statistics interval - a CI emulator once encoded a lighter
        // version of this so fast that every reported position was still zero.
        var total = TimeSpan.FromSeconds(1800 / 30.0);
        var result = await FFmpeg.ExecuteAsync(
            $"-y -f rawvideo -pixel_format rgb24 -video_size {FrameWidth}x{FrameHeight} " +
            $"-framerate 30 -i \"{input}\" -vf scale=640:480 -c:v mpeg4 \"{output}\"",
            progress,
            total);

        AssertSuccess(result, "progress encode");
        Assert(result.Duration > TimeSpan.Zero, $"Session duration was {result.Duration?.ToString() ?? "<null>"}.");

        var captured = progress.Samples;
        Assert(captured.Length > 0, "No progress was reported.");
        Assert(captured.All(p => p.Percent is >= 0 and <= 1), "A percent fell outside 0..1.");
        Assert(captured.Any(p => p.Position > TimeSpan.Zero), "Position never advanced.");

        var last = captured[^1];
        Report($"{captured.Length} samples, last: {last.Percent:P0} at {last.Position}, frame {last.VideoFrameNumber}, speed {last.Speed:0.##}x");
    }

    private static async Task CancellationStopsACommand(string workingDirectory)
    {
        var input = Path.Combine(workingDirectory, "long.raw");
        var output = Path.Combine(workingDirectory, "cancelled.mp4");
        WriteRawFrames(input, frameCount: 4000);
        File.Delete(output);

        using var cancellation = new CancellationTokenSource();

        // Upscaling several thousand frames keeps FFmpeg busy long enough to cancel mid-run.
        var task = FFmpeg.ExecuteAsync(
            $"-y -f rawvideo -pixel_format rgb24 -video_size {FrameWidth}x{FrameHeight} " +
            $"-framerate 30 -i \"{input}\" -vf scale=1280:720 -c:v mpeg4 \"{output}\"",
            cancellation.Token);

        cancellation.CancelAfter(TimeSpan.FromMilliseconds(300));

        // Cancelling must complete the task normally rather than hang or throw...
        var completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(60)));
        Assert(completed == task, "The cancelled command never completed.");

        // ...and must actually have stopped FFmpeg. Several thousand frames upscaled to 720p
        // cannot finish in 300ms on any device this runs on, so a success here would mean the
        // token was ignored rather than that the work simply beat the timer.
        var result = await task;
        Assert(result.Cancelled, $"Expected Cancelled, got Succeeded={result.Succeeded} ReturnCode={result.ReturnCode?.ToString() ?? "<null>"}.");
        Assert(!result.Failed, "A cancelled session must not read as failed.");

        try
        {
            result.EnsureSuccess();
            Assert(false, "EnsureSuccess() did not throw for a cancelled session.");
        }
        catch (OperationCanceledException)
        {
            // The one TAP convention the flag-based API still honours on request.
        }

        Report($"cancelled session {result.SessionId}");
    }

    private static async Task LogDelegateReceivesOutput(string workingDirectory)
    {
        var lines = 0;

        FFmpegKitConfig.EnableLogCallback(_ => Interlocked.Increment(ref lines));
        try
        {
            await FFmpeg.ExecuteAsync("-version");

            // Log callbacks arrive on FFmpegKit's own thread and can lag the session that
            // produced them, so give them a moment rather than racing the delivery.
            var deadline = DateTime.UtcNow.AddSeconds(5);
            while (Volatile.Read(ref lines) == 0 && DateTime.UtcNow < deadline)
            {
                await Task.Delay(50);
            }
        }
        finally
        {
            FFmpegKitConfig.EnableLogCallback(null);
        }

        Assert(Volatile.Read(ref lines) > 0, "The log delegate never fired.");
        Report($"received {Volatile.Read(ref lines)} log lines");
    }

    private static Task LogLevelRoundTrips(string workingDirectory)
    {
        var original = FFmpegKitConfig.GetLogLevel();
        try
        {
            foreach (var level in new[] { FFmpegLogLevel.Warning, FFmpegLogLevel.Info })
            {
                FFmpegKitConfig.SetLogLevel(level);
                var read = FFmpegKitConfig.GetLogLevel();
                Assert(read == level, $"Set {level} but read back {read}.");
            }
        }
        finally
        {
            FFmpegKitConfig.SetLogLevel(original);
        }

        Report($"restored to {original}");
        return Task.CompletedTask;
    }

    private static async Task SessionHistoryClears(string workingDirectory)
    {
        // Mostly an "it does not throw and the library still works afterwards" check - the
        // platform session tables are not observable through the cross-platform surface.
        FFmpegKitConfig.ClearSessions();

        var result = await FFmpeg.ExecuteAsync("-version");
        Assert(result.Succeeded, "FFmpeg stopped working after ClearSessions().");
    }

    private const int FrameWidth = 32;
    private const int FrameHeight = 32;
    private const int FrameCount = 10;

    private static string BuildEncodeCommand(string input, string output) =>
        $"-y -f rawvideo -pixel_format rgb24 -video_size {FrameWidth}x{FrameHeight} " +
        $"-framerate 10 -i \"{input}\" -vf scale=64:64 -c:v mpeg4 \"{output}\"";

    /// <summary>Writes a handful of rgb24 frames so the encode tests need no bundled media.</summary>
    private static void WriteRawFrames(string path, int frameCount = FrameCount)
    {
        var frame = new byte[FrameWidth * FrameHeight * 3];
        using var stream = File.Create(path);

        for (var i = 0; i < frameCount; i++)
        {
            for (var pixel = 0; pixel < frame.Length; pixel += 3)
            {
                frame[pixel] = (byte)(i * 25);
                frame[pixel + 1] = (byte)(pixel % 256);
                frame[pixel + 2] = (byte)((pixel + i) % 256);
            }

            stream.Write(frame);
        }
    }

    private sealed class CollectingProgress : IProgress<FFmpegProgress>
    {
        private readonly List<FFmpegProgress> _samples = [];

        public FFmpegProgress[] Samples
        {
            get { lock (_samples) return [.. _samples]; }
        }

        public void Report(FFmpegProgress value)
        {
            lock (_samples) _samples.Add(value);
        }
    }

    private static void AssertSuccess(FFmpegSessionResult result, string what) =>
        Assert(
            result.Succeeded,
            $"'{what}' failed with return code {result.ReturnCode?.ToString() ?? "<null>"} (cancelled={result.Cancelled}).");

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new SmokeTestFailure(message);
        }
    }

    private static void Report(string message) => Reporter?.Invoke(message);

    /// <summary>Set by the host so checks can surface detail to the platform log stream.</summary>
    public static Action<string>? Reporter { get; set; }
}

public sealed class SmokeTestFailure(string message) : Exception(message);
