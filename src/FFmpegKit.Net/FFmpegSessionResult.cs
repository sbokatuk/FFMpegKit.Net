namespace Ffmpegkit.Net;

/// <summary>
/// The outcome of a completed FFmpeg or FFprobe session, translated from whichever platform
/// binding actually ran it.
/// </summary>
/// <remarks>
/// <c>Ffmpegkit.Droid.AbstractSession</c>, <c>Ffmpegkit.Ios.AbstractSession</c> and
/// <c>Ffmpegkit.Mac.AbstractSession</c> are unrelated types with a parallel but not identical
/// surface - a Java return code object on one side, Objective-C ones on the others, each with
/// its own <c>IsValueSuccess</c>/<c>IsValueCancel</c> pair. This flattens all of them into one
/// shape so calling code does not need a platform check to
/// read the result of a command, including the session's console output in <see cref="Output"/>.
/// Reach for the platform session type directly (via <c>FFmpegKitConfig.GetSession</c> on any
/// platform, keyed by <see cref="SessionId"/>) for anything not exposed here, such as individual
/// <c>Log</c> entries with per-line severities.
/// </remarks>
/// <param name="SessionId">FFmpegKit's own id for the session that ran.</param>
/// <param name="Succeeded">True when the command completed and returned success.</param>
/// <param name="Cancelled">True when the command was stopped via the <c>CancellationToken</c>.</param>
/// <param name="ReturnCode">
/// The process return code, or null when the session has not produced one - which is only
/// expected while <see cref="Cancelled"/> is also true.
/// </param>
public sealed record FFmpegSessionResult(
    long SessionId,
    bool Succeeded,
    bool Cancelled,
    int? ReturnCode)
{
    /// <summary>True when the session neither succeeded nor was cancelled - i.e. it failed.</summary>
    public bool Failed => !Succeeded && !Cancelled;

    /// <summary>The command the session ran, as FFmpegKit recorded it.</summary>
    public string? Command { get; init; }

    /// <summary>
    /// Everything the session wrote to its log - the same text FFmpeg prints to a console,
    /// which for a failed command includes the reason it failed.
    /// </summary>
    /// <remarks>
    /// This is the whole session transcript, so it grows with the amount of logging the command
    /// produced; lower <see cref="FFmpegKitConfig.SetLogLevel"/> to shrink it for long runs.
    /// </remarks>
    public string? Output { get; init; }

    /// <summary>How long the session took to run, when FFmpegKit recorded a duration for it.</summary>
    public TimeSpan? Duration { get; init; }

    /// <summary>
    /// The stack trace FFmpegKit recorded when the session itself failed to run at all -
    /// distinct from a command that ran and returned an error, whose explanation is in
    /// <see cref="Output"/>. Null in every other case.
    /// </summary>
    public string? FailStackTrace { get; init; }

    /// <summary>Returns this result unchanged when the command succeeded; throws otherwise.</summary>
    /// <returns>The same instance, so a call can be chained onto an awaited execute.</returns>
    /// <remarks>
    /// The awaitable API never throws for a failing or cancelled command - results carry flags
    /// instead. This is the bridge for code that prefers exceptions:
    /// <c>(await FFmpegKit.ExecuteAsync(...)).EnsureSuccess()</c>.
    /// </remarks>
    /// <exception cref="OperationCanceledException">The session was cancelled.</exception>
    /// <exception cref="FFmpegExecutionException">
    /// The command failed. The exception message ends with the tail of <see cref="Output"/>;
    /// the full result stays available on <see cref="FFmpegExecutionException.Result"/>.
    /// </exception>
    public FFmpegSessionResult EnsureSuccess()
    {
        if (Succeeded)
        {
            return this;
        }

        if (Cancelled)
        {
            throw new OperationCanceledException($"FFmpeg session {SessionId} was cancelled.");
        }

        throw new FFmpegExecutionException(this);
    }
}
