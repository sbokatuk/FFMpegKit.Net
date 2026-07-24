using DroidFFprobeKit = Ffmpegkit.Droid.FFprobeKit;
using DroidMediaInformation = Ffmpegkit.Droid.MediaInformation;
using DroidStreamInformation = Ffmpegkit.Droid.StreamInformation;

namespace Ffmpegkit.Net;

public static partial class FFprobeKit
{
    public static partial async Task<MediaInfo?> GetMediaInformationAsync(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var session = await DroidFFprobeKit.GetMediaInformationAsync(path).ConfigureAwait(false);

        return session.MediaInformation is { } info ? ToShared(info) : null;
    }

    // Ffmpegkit.Droid.MediaInformation / StreamInformation already carry typed accessors
    // (Additions/MediaInformation.Typed.cs) that parse FFprobe's invariant numeric strings, so
    // this is a field copy rather than a re-parse - unlike the iOS half, which has no equivalent
    // Additions type and uses the shared Ffmpegkit.Net.MediaValues parser directly.
    private static MediaInfo ToShared(DroidMediaInformation info) =>
        new(
            info.Format,
            info.LongFormat,
            info.DurationOrNull,
            info.StartTimeOrNull,
            info.SizeBytes,
            info.BitrateBps,
            info.TagValues,
            info.Streams?.Select(ToShared).ToList() ?? []);

    private static StreamInfo ToShared(DroidStreamInformation stream) =>
        new(
            stream.IndexOrNull,
            stream.Type,
            stream.Codec,
            stream.CodecLong,
            stream.PixelWidth,
            stream.PixelHeight,
            stream.BitrateBps,
            stream.SampleRateHz,
            stream.AverageFrameRateFps,
            stream.RealFrameRateFps,
            stream.TagValues);
}
