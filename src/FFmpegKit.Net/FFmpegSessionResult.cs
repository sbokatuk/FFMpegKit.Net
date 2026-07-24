namespace Ffmpegkit.Net;

/// <summary>
/// The outcome of a completed FFmpeg or FFprobe session, translated from whichever platform
/// binding actually ran it.
/// </summary>
/// <remarks>
/// <c>Ffmpegkit.Droid.AbstractSession</c> and <c>Ffmpegkit.Ios.AbstractSession</c> are unrelated
/// types with a parallel but not identical surface - a Java return code object on one side, an
/// Objective-C one on the other, each with its own <c>IsValueSuccess</c>/<c>IsValueCancel</c>
/// pair. This flattens both into one shape so calling code does not need a platform check to
/// read the result of a command. Reach for the platform session type directly (via
/// <c>FFmpegKitConfig.GetSession</c> on either platform, keyed by <see cref="SessionId"/>) for
/// anything not exposed here, such as per-line logs.
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
}
