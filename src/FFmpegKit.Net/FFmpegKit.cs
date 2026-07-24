namespace Ffmpegkit.Net;

/// <summary>
/// Runs FFmpeg commands, with the same awaitable API on Android and iOS.
/// </summary>
/// <remarks>
/// The platform bindings do not resemble each other in the small print - Android cancels a
/// session through a static <c>Cancel(sessionId)</c>, iOS calls <c>Cancel()</c> on the session
/// object itself, and the two <c>Statistics</c>/<c>MediaInformation</c> types are unrelated
/// generated classes with a parallel but not identical shape. This is the layer that hides that:
/// each member below is declared once here and implemented once per platform under
/// <c>Platforms/Android</c> / <c>Platforms/iOS</c> (an "extended" C# partial method - the
/// implementing declaration lives in whichever platform half actually compiles).
/// <para>
/// Reach for <c>Ffmpegkit.Droid.FFmpegKit</c> (Android) or <c>Ffmpegkit.Ios.FFmpegKit</c> (iOS)
/// directly when you need something not exposed here - both are still fully available; this
/// package only adds a shared layer on top; see <see cref="Net.FFmpegKitConfig"/> for the
/// equivalent over <c>FFmpegKitConfig</c> and <see cref="Net.FFprobeKit"/> for probing.
/// </para>
/// </remarks>
public static partial class FFmpegKit
{
    /// <summary>Runs an FFmpeg command and awaits its completion.</summary>
    /// <param name="command">The FFmpeg command, as it would be typed after <c>ffmpeg</c>.</param>
    /// <param name="cancellationToken">Cancels the running session.</param>
    /// <returns>The completed session's outcome.</returns>
    /// <remarks>
    /// A failing command completes the task normally with <see cref="FFmpegSessionResult.Failed"/>
    /// true; it does not throw. Cancellation asks FFmpeg to stop, and the session then completes
    /// with <see cref="FFmpegSessionResult.Cancelled"/> true rather than raising
    /// <see cref="OperationCanceledException"/> - FFmpeg may still have written a partial output file.
    /// </remarks>
    public static partial Task<FFmpegSessionResult> ExecuteAsync(
        string command,
        CancellationToken cancellationToken = default);

    /// <summary>Runs an FFmpeg command, reporting progress, and awaits its completion.</summary>
    /// <param name="command">The FFmpeg command, as it would be typed after <c>ffmpeg</c>.</param>
    /// <param name="progress">Receives a sample each time FFmpeg reports statistics.</param>
    /// <param name="totalDuration">
    /// Duration of the material being processed. Supply it to get
    /// <see cref="FFmpegProgress.Percent"/> and an estimated time remaining; without it the other
    /// fields are still reported. <see cref="MediaInfo.Duration"/> from
    /// <see cref="Net.FFprobeKit.GetMediaInformationAsync"/> is the usual source.
    /// </param>
    /// <param name="cancellationToken">Cancels the running session.</param>
    /// <remarks>Progress is reported on an FFmpegKit worker thread; marshal to the UI thread before touching UI.</remarks>
    public static partial Task<FFmpegSessionResult> ExecuteAsync(
        string command,
        IProgress<FFmpegProgress> progress,
        TimeSpan? totalDuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>Runs an FFmpeg command from pre-split arguments and awaits its completion.</summary>
    /// <remarks>
    /// Prefer this over <see cref="ExecuteAsync(string,CancellationToken)"/> when any argument
    /// may contain spaces or quotes, such as a file path: no quoting rules are involved.
    /// </remarks>
    public static partial Task<FFmpegSessionResult> ExecuteWithArgumentsAsync(
        string[] arguments,
        CancellationToken cancellationToken = default);
}
