namespace Ffmpegkit.Net;

/// <summary>Probes media files, with the same awaitable API on Android, iOS and macOS.</summary>
/// <remarks>See <see cref="Net.FFmpegKit"/> for why this layer exists and what it deliberately does not cover.</remarks>
public static partial class FFprobeKit
{
    /// <summary>Probes a file and awaits the parsed media information.</summary>
    /// <param name="path">Path to the media file.</param>
    /// <returns>
    /// The parsed information, or null when the file could not be parsed. Check the platform
    /// session (via <c>Ffmpegkit.Droid.FFmpegKitConfig</c> / <c>Ffmpegkit.Ios.FFmpegKitConfig</c>,
    /// keyed by session id) for the failure reason.
    /// </returns>
    /// <remarks>
    /// No <see cref="CancellationToken"/> overload is offered. FFmpegKit exposes no cancellation
    /// for probe sessions on any platform, so a token could only abandon the wait while the
    /// probe kept running in the background. Probes are short; if you need to give up waiting,
    /// race the returned task against a delay yourself.
    /// </remarks>
    public static partial Task<MediaInfo?> GetMediaInformationAsync(string path);
}
