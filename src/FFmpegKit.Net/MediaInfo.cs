namespace Ffmpegkit.Net;

/// <summary>
/// Parsed FFprobe output for one media file, with the same typed accessors on every platform.
/// </summary>
/// <remarks>
/// FFprobe reports numbers as invariant-format strings (Android also boxes some as Java
/// <c>Long</c>s; iOS keeps everything as <c>NSString</c>) rather than as numbers, and both
/// bindings expose that faithfully. Parsing them yourself is a live bug -
/// <c>double.Parse("12.345000")</c> returns 12,345,000 under a German locale and throws under a
/// French one - so this is built once, invariantly, in <c>Platforms/Android</c> / <c>Platforms/iOS</c>,
/// and null rather than thrown when FFprobe omits a field, which it does routinely depending on
/// the container.
/// </remarks>
/// <param name="Format">Short container name, e.g. <c>"mov,mp4,m4a,3gp,3g2,mj2"</c>.</param>
/// <param name="LongFormat">Human-readable container description.</param>
/// <param name="Duration">Media duration, or null when the container does not report one.</param>
/// <param name="StartTime">Start time, or null when not reported.</param>
/// <param name="SizeBytes">File size in bytes, or null when not reported.</param>
/// <param name="BitrateBps">Overall bit rate in bits per second, or null when not reported.</param>
/// <param name="Tags">Container metadata tags (title, artist, creation_time, ...), keyed case-insensitively.</param>
/// <param name="Streams">Every stream FFprobe found, in container order.</param>
public sealed record MediaInfo(
    string? Format,
    string? LongFormat,
    TimeSpan? Duration,
    TimeSpan? StartTime,
    long? SizeBytes,
    long? BitrateBps,
    IReadOnlyDictionary<string, string> Tags,
    IReadOnlyList<StreamInfo> Streams);

/// <summary>One stream (video, audio, subtitle, ...) within a probed media file.</summary>
/// <param name="Index">Stream index within the container, or null when not reported.</param>
/// <param name="Type">FFprobe's stream type string, e.g. <c>"video"</c> or <c>"audio"</c>.</param>
/// <param name="Codec">Short codec name, e.g. <c>"h264"</c>.</param>
/// <param name="CodecLong">Human-readable codec description.</param>
/// <param name="PixelWidth">Frame width in pixels, or null for non-video streams.</param>
/// <param name="PixelHeight">Frame height in pixels, or null for non-video streams.</param>
/// <param name="BitrateBps">Stream bit rate in bits per second, or null when not reported.</param>
/// <param name="SampleRateHz">Audio sample rate in Hz, or null for non-audio streams.</param>
/// <param name="AverageFrameRateFps">
/// Average frame rate, evaluated from FFprobe's rational string (e.g. <c>"30000/1001"</c>), or
/// null when not reported.
/// </param>
/// <param name="RealFrameRateFps">Real (as opposed to average) frame rate, or null when not reported.</param>
/// <param name="Tags">Stream metadata tags, keyed case-insensitively.</param>
public sealed record StreamInfo(
    int? Index,
    string? Type,
    string? Codec,
    string? CodecLong,
    int? PixelWidth,
    int? PixelHeight,
    long? BitrateBps,
    int? SampleRateHz,
    double? AverageFrameRateFps,
    double? RealFrameRateFps,
    IReadOnlyDictionary<string, string> Tags)
{
    /// <summary>True when this is a video stream.</summary>
    public bool IsVideo => string.Equals(Type, "video", StringComparison.OrdinalIgnoreCase);

    /// <summary>True when this is an audio stream.</summary>
    public bool IsAudio => string.Equals(Type, "audio", StringComparison.OrdinalIgnoreCase);
}
