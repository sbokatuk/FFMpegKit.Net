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

    [Fact]
    public void EnsureSuccess_returns_the_same_instance_on_success()
    {
        var result = new FFmpegSessionResult(SessionId: 7, Succeeded: true, Cancelled: false, ReturnCode: 0);

        Assert.Same(result, result.EnsureSuccess());
    }

    [Fact]
    public void EnsureSuccess_throws_OperationCanceledException_when_cancelled()
    {
        var result = new FFmpegSessionResult(SessionId: 7, Succeeded: false, Cancelled: true, ReturnCode: null);

        Assert.Throws<OperationCanceledException>(() => result.EnsureSuccess());
    }

    [Fact]
    public void EnsureSuccess_throws_with_the_output_tail_on_failure()
    {
        var result = new FFmpegSessionResult(SessionId: 7, Succeeded: false, Cancelled: false, ReturnCode: 1)
        {
            Command = "-i missing.mp4 -f null -",
            Output = "ffmpeg version n8.1.2\nmissing.mp4: No such file or directory",
        };

        var exception = Assert.Throws<FFmpegExecutionException>(() => result.EnsureSuccess());

        Assert.Same(result, exception.Result);
        Assert.Contains("return code 1", exception.Message);
        Assert.Contains("No such file or directory", exception.Message);
    }

    [Fact]
    public void Exception_message_truncates_long_output_to_its_tail()
    {
        // The full transcript stays on Result; the message carries only enough to read the error.
        var output = new string('a', 5000) + "THE END";
        var result = new FFmpegSessionResult(SessionId: 1, Succeeded: false, Cancelled: false, ReturnCode: 1)
        {
            Output = output,
        };

        var exception = new FFmpegExecutionException(result);

        Assert.True(exception.Message.Length < output.Length, "The message must not carry the whole transcript.");
        Assert.EndsWith("THE END", exception.Message);
        Assert.Equal(output, exception.Result.Output);
    }

    [Fact]
    public void Exception_message_falls_back_to_the_fail_stack_trace()
    {
        // A session that never ran has no output; its explanation is the native stack trace.
        var result = new FFmpegSessionResult(SessionId: 1, Succeeded: false, Cancelled: false, ReturnCode: null)
        {
            FailStackTrace = "com.arthenica.ffmpegkit native trace",
        };

        Assert.Contains("native trace", new FFmpegExecutionException(result).Message);
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
