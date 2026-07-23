using MacFFprobeKit = Ffmpegkit.Mac.FFprobeKit;
using MacMediaInformation = Ffmpegkit.Mac.MediaInformation;
using MacStreamInformation = Ffmpegkit.Mac.StreamInformation;

namespace Ffmpegkit.Net;

public static partial class FFprobeKit
{
    public static partial async Task<MediaInfo?> GetMediaInformationAsync(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var session = await MacFFprobeKit.GetMediaInformationAsync(path).ConfigureAwait(false);

        return session.MediaInformation is { } info ? ToShared(info) : null;
    }

    // Unlike Android, Ffmpegkit.Mac has no Additions equivalent of MediaInformation.Typed.cs, so
    // the invariant string parsing happens here directly (via the shared MediaValues) rather than
    // through typed accessors the binding already provides.
    private static MediaInfo ToShared(MacMediaInformation info) =>
        new(
            info.Format,
            info.LongFormat,
            MediaValues.Seconds(info.Duration),
            MediaValues.Seconds(info.StartTime),
            MediaValues.Integer(info.Size),
            MediaValues.Integer(info.Bitrate),
            MediaValuesMac.ToDictionary(info.Tags),
            info.Streams?.Select(ToShared).ToList() ?? []);

    private static StreamInfo ToShared(MacStreamInformation stream) =>
        new(
            MediaValuesMac.Int32(stream.Index),
            stream.Type,
            stream.Codec,
            stream.CodecLong,
            MediaValuesMac.Int32(stream.Width),
            MediaValuesMac.Int32(stream.Height),
            MediaValues.Integer(stream.Bitrate),
            MediaValues.Integer(stream.SampleRate) is { } rate ? checked((int)rate) : null,
            MediaValues.Rational(stream.AverageFrameRate),
            MediaValues.Rational(stream.RealFrameRate),
            MediaValuesMac.ToDictionary(stream.Tags));
}
