using Ffmpegkit.Net;

namespace FFmpegKit.Net.UnitTests;

/// <summary>
/// The clamping and null-handling rules in <see cref="FFmpegProgress.From"/>, which both
/// platforms' statistics callbacks funnel through.
/// </summary>
public class FFmpegProgressTests
{
    private static FFmpegProgress Sample(
        double timeMs = 5_000,
        long sizeBytes = 1_000,
        double bitrate = 800,
        double speed = 2.0,
        TimeSpan? totalDuration = null) =>
        FFmpegProgress.From(timeMs, sizeBytes, bitrate, speed, videoFrameNumber: 10, videoFps: 30, totalDuration);

    [Fact]
    public void Negative_pre_first_frame_time_is_clamped_to_zero()
    {
        var progress = Sample(timeMs: -250);

        Assert.Equal(TimeSpan.Zero, progress.Position);
    }

    [Fact]
    public void Percent_is_null_without_a_total_duration()
    {
        var progress = Sample(totalDuration: null);

        Assert.Null(progress.Percent);
        Assert.Null(progress.EstimatedTimeRemaining);
    }

    [Fact]
    public void Percent_is_the_fraction_of_the_total()
    {
        var progress = Sample(timeMs: 5_000, totalDuration: TimeSpan.FromSeconds(10));

        Assert.Equal(0.5, progress.Percent!.Value, 6);
    }

    [Fact]
    public void Percent_is_clamped_to_one_when_ffmpeg_overshoots_the_probed_duration()
    {
        var progress = Sample(timeMs: 12_000, totalDuration: TimeSpan.FromSeconds(10));

        Assert.Equal(1, progress.Percent!.Value);
        Assert.Null(progress.EstimatedTimeRemaining);
    }

    [Fact]
    public void Estimated_time_remaining_scales_with_speed()
    {
        // 5s of a 10s file left, encoding at 2x realtime -> 2.5s remaining.
        var progress = Sample(timeMs: 5_000, speed: 2.0, totalDuration: TimeSpan.FromSeconds(10));

        Assert.Equal(2.5, progress.EstimatedTimeRemaining!.Value.TotalSeconds, 6);
    }

    [Fact]
    public void Estimated_time_remaining_is_null_without_a_usable_speed()
    {
        var progress = Sample(speed: 0, totalDuration: TimeSpan.FromSeconds(10));

        Assert.Null(progress.EstimatedTimeRemaining);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Unmeasured_bitrate_is_reported_as_null(double bitrate)
    {
        Assert.Null(Sample(bitrate: bitrate).Bitrate);
    }

    [Fact]
    public void Measured_bitrate_is_passed_through()
    {
        Assert.Equal(800, Sample(bitrate: 800).Bitrate);
    }
}
