using System.Globalization;
using Ffmpegkit.Net;

namespace FFmpegKit.Net.UnitTests;

/// <summary>
/// The invariant-parsing rules FFprobe values depend on. The culture-hostile cases run under a
/// German culture on purpose - the whole point of <see cref="MediaValues"/> is that
/// <c>double.Parse("12.345000")</c> reads as twelve million under de-DE.
/// </summary>
public class MediaValuesTests
{
    [Theory]
    [InlineData("12.345000", 12.345)]
    [InlineData("0.000000", 0)]
    [InlineData("30", 30)]
    public void Number_parses_invariantly(string value, double expected)
    {
        Assert.Equal(expected, MediaValues.Number(value)!.Value, 6);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("N/A")]
    public void Number_returns_null_for_absent_or_unparseable_values(string? value)
    {
        Assert.Null(MediaValues.Number(value));
    }

    [Fact]
    public void Number_ignores_the_ambient_culture()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            Assert.Equal(12.345, MediaValues.Number("12.345000")!.Value, 6);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Theory]
    [InlineData("128000", 128000L)]
    [InlineData("0", 0L)]
    public void Integer_parses_invariantly(string value, long expected)
    {
        Assert.Equal(expected, MediaValues.Integer(value));
    }

    [Theory]
    [InlineData("12.5")]
    [InlineData("N/A")]
    [InlineData(null)]
    public void Integer_returns_null_for_non_integers(string? value)
    {
        Assert.Null(MediaValues.Integer(value));
    }

    [Fact]
    public void Seconds_converts_to_a_timespan()
    {
        Assert.Equal(TimeSpan.FromSeconds(12.345), MediaValues.Seconds("12.345000")!.Value);
    }

    [Fact]
    public void Seconds_returns_null_when_absent()
    {
        Assert.Null(MediaValues.Seconds(null));
    }

    [Theory]
    [InlineData("30/1", 30)]
    [InlineData("30000/1001", 29.97002997)]
    [InlineData("25", 25)]
    public void Rational_evaluates_ffprobe_rates(string value, double expected)
    {
        Assert.Equal(expected, MediaValues.Rational(value)!.Value, 6);
    }

    [Theory]
    [InlineData("0/0")]
    [InlineData("30/0")]
    [InlineData("x/1")]
    [InlineData("")]
    [InlineData(null)]
    public void Rational_returns_null_for_unknown_rates(string? value)
    {
        // "0/0" is FFprobe's way of saying it does not know; a zero denominator must never throw.
        Assert.Null(MediaValues.Rational(value));
    }
}
