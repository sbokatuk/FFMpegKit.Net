using Ffmpegkit.Mac;

using MacFFmpegKit = Ffmpegkit.Mac.FFmpegKit;
using MacStatistics = Ffmpegkit.Mac.Statistics;

namespace Ffmpegkit.Net;

public static partial class FFmpegKit
{
    // ExecuteAsync/ExecuteWithArgumentsAsync are built on Ffmpegkit.Mac's own Additions
    // (FFmpegKit.Async.cs), which already thread a CancellationToken through FFmpegKit's
    // completion callback via a TaskCompletionSource.

    public static partial async Task<FFmpegSessionResult> ExecuteAsync(
        string command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var session = await MacFFmpegKit.ExecuteAsync(command, cancellationToken).ConfigureAwait(false);
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

        // Ffmpegkit.Mac has no Additions equivalent of the Android progress overload, so this
        // calls the raw generated binding directly - its statistics callback is a plain
        // Objective-C block bound as a C# delegate, unlike Android's Java listener interface,
        // so no adapter object is needed - and reports through the shared FFmpegProgress
        // factory that builds a sample from primitive numbers.
        var completion = new TaskCompletionSource<FFmpegSession>(TaskCreationOptions.RunContinuationsAsynchronously);
        CancellationTokenRegistration registration = default;

        var session = MacFFmpegKit.ExecuteAsync(
            command,
            completeCallback: completed =>
            {
                registration.Dispose();
                completion.TrySetResult(completed);
            },
            // Unlike EnableLogCallback, this overload's logCallback parameter is not
            // [NullAllowed] in ApiDefinition.cs, so the generated binding throws
            // ArgumentNullException at runtime for a literal null despite the C# delegate type
            // accepting one at compile time - a no-op delegate is required, not null.
            logCallback: _ => { },
            statisticsCallback: statistics => progress.Report(ToShared(statistics, totalDuration)));

        if (cancellationToken.CanBeCanceled)
        {
            // Captured before registering: the session is needed to cancel just this command.
            registration = cancellationToken.Register(session.Cancel);

            // The token may already be cancelled by the time the session exists.
            if (cancellationToken.IsCancellationRequested)
                session.Cancel();
        }

        var completedSession = await completion.Task.ConfigureAwait(false);
        return ToResult(completedSession);
    }

    public static partial async Task<FFmpegSessionResult> ExecuteWithArgumentsAsync(
        string[] arguments,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        var session = await MacFFmpegKit.ExecuteWithArgumentsAsync(arguments, cancellationToken).ConfigureAwait(false);
        return ToResult(session);
    }

    internal static FFmpegSessionResult ToResult(AbstractSession session) =>
        new(
            session.SessionId,
            session.ReturnCode is { IsValueSuccess: true },
            session.ReturnCode is { IsValueCancel: true },
            session.ReturnCode?.Value);

    private static FFmpegProgress ToShared(MacStatistics statistics, TimeSpan? totalDuration) =>
        FFmpegProgress.From(
            statistics.Time,
            statistics.Size,
            statistics.Bitrate,
            statistics.Speed,
            statistics.VideoFrameNumber,
            statistics.VideoFps,
            totalDuration);
}
