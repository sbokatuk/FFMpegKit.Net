using IosFFprobeKit = Ffmpegkit.Ios.FFprobeKit;
using IosMediaInformation = Ffmpegkit.Ios.MediaInformation;
using IosStreamInformation = Ffmpegkit.Ios.StreamInformation;

namespace Ffmpegkit.Net;

public static partial class FFprobeKit
{
    public static partial async Task<MediaInfo?> GetMediaInformationAsync(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var session = await IosFFprobeKit.GetMediaInformationAsync(path).ConfigureAwait(false);

        return session.MediaInformation is { } info ? ToShared(info) : null;
    }

    // Unlike Android, Ffmpegkit.Ios has no Additions equivalent of MediaInformation.Typed.cs, so
    // the invariant string parsing happens here directly (via the shared MediaValues) rather than
    // through typed accessors the binding already provides.
    private static MediaInfo ToShared(IosMediaInformation info) =>
        new(
            info.Format,
            info.LongFormat,
            MediaValues.Seconds(info.Duration),
            MediaValues.Seconds(info.StartTime),
            MediaValues.Integer(info.Size),
            MediaValues.Integer(info.Bitrate),
            MediaValuesIos.ToDictionary(info.Tags),
            info.Streams?.Select(ToShared).ToList() ?? []);

    private static StreamInfo ToShared(IosStreamInformation stream) =>
        new(
            MediaValuesIos.Int32(stream.Index),
            stream.Type,
            stream.Codec,
            stream.CodecLong,
            MediaValuesIos.Int32(stream.Width),
            MediaValuesIos.Int32(stream.Height),
            MediaValues.Integer(stream.Bitrate),
            MediaValues.Integer(stream.SampleRate) is { } rate ? checked((int)rate) : null,
            MediaValues.Rational(stream.AverageFrameRate),
            MediaValues.Rational(stream.RealFrameRate),
            MediaValuesIos.ToDictionary(stream.Tags));
}
