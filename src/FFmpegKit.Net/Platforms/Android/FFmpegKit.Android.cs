using Ffmpegkit.Droid;

using DroidFFmpegKit = Ffmpegkit.Droid.FFmpegKit;
using DroidProgress = Ffmpegkit.Droid.FFmpegProgress;

namespace Ffmpegkit.Net;

public static partial class FFmpegKit
{
    // Built on top of Ffmpegkit.Droid's own Additions (FFmpegKit.Async.cs), which already thread
    // a CancellationToken through FFmpegKit's completion callback via a TaskCompletionSource -
    // there is no reason to duplicate that plumbing here, only to translate its result.

    public static partial async Task<FFmpegSessionResult> ExecuteAsync(
        string command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var session = await DroidFFmpegKit.ExecuteAsync(command, cancellationToken).ConfigureAwait(false);
        return ToResult(session);
    }

    public static partial async Task<FFmpegSessionResult> ExecuteAsync(
        string command,
        IProgress<FFmpegProgress> progress,
        TimeSpan? totalDuration,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(progress);

        // Ffmpegkit.Droid.FFmpegProgress (the Additions type) already computes Percent and
        // EstimatedTimeRemaining, so this is a field copy rather than the raw-Statistics
        // conversion the iOS half needs - Ffmpegkit.Ios has no equivalent Additions type to hand
        // that work off to.
        var adapter = new Progress<DroidProgress>(sample => progress.Report(ToShared(sample)));

        var session = await DroidFFmpegKit.ExecuteAsync(command, adapter, totalDuration, cancellationToken)
            .ConfigureAwait(false);

        return ToResult(session);
    }

    public static partial async Task<FFmpegSessionResult> ExecuteWithArgumentsAsync(
        string[] arguments,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        var session = await DroidFFmpegKit.ExecuteWithArgumentsAsync(arguments, cancellationToken)
            .ConfigureAwait(false);

        return ToResult(session);
    }

    internal static FFmpegSessionResult ToResult(AbstractSession session) =>
        new(
            session.SessionId,
            session.ReturnCode is { IsValueSuccess: true },
            session.ReturnCode is { IsValueCancel: true },
            session.ReturnCode?.Value);

    private static FFmpegProgress ToShared(DroidProgress sample) =>
        new(
            sample.Position,
            sample.Percent,
            sample.SizeBytes,
            sample.Bitrate,
            sample.Speed,
            sample.VideoFrameNumber,
            sample.VideoFps)
        {
            EstimatedTimeRemaining = sample.EstimatedTimeRemaining,
        };
}
