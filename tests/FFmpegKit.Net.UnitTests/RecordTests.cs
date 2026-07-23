using Ffmpegkit.Net;

namespace FFmpegKit.Net.UnitTests;

/// <summary>The small computed members on the shared result records.</summary>
public class RecordTests
{
    [Theory]
    [InlineData(true, false, false)]  // succeeded
    [InlineData(false, true, false)]  // cancelled
    [InlineData(false, false, true)]  // failed
    public void Failed_means_neither_succeeded_nor_cancelled(bool succeeded, bool cancelled, bool expectedFailed)
    {
        var result = new FFmpegSessionResult(SessionId: 1, succeeded, cancelled, ReturnCode: succeeded ? 0 : 1);

        Assert.Equal(expectedFailed, result.Failed);
    }

    [Theory]
    [InlineData("video", true, false)]
    [InlineData("Video", true, false)]
    [InlineData("audio", false, true)]
    [InlineData("subtitle", false, false)]
    [InlineData(null, false, false)]
    public void Stream_type_flags_compare_case_insensitively(string? type, bool isVideo, bool isAudio)
    {
        var stream = new StreamInfo(
            Index: 0, type, Codec: null, CodecLong: null, PixelWidth: null, PixelHeight: null,
            BitrateBps: null, SampleRateHz: null, AverageFrameRateFps: null, RealFrameRateFps: null,
            Tags: new Dictionary<string, string>());

        Assert.Equal(isVideo, stream.IsVideo);
        Assert.Equal(isAudio, stream.IsAudio);
    }
}
