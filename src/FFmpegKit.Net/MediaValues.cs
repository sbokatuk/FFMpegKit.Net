using System.Globalization;

namespace Ffmpegkit.Net;

/// <summary>
/// Invariant parsing for the numeric values FFprobe reports as strings, shared by both
/// platforms' <see cref="MediaInfo"/> / <see cref="StreamInfo"/> conversion.
/// </summary>
/// <remarks>
/// FFprobe emits numbers in a fixed, invariant format - <c>"12.345000"</c>, <c>"30/1"</c> - and
/// always with a dot as the decimal separator. Parsing them with the ambient culture is wrong and
/// quietly so: on a German device <c>double.Parse("12.345000")</c> returns 12,345,000 because the
/// dot reads as a group separator, and on a French one it throws. Everything here parses
/// invariantly and returns null rather than throwing when a value is absent or unparseable -
/// FFprobe omits fields routinely depending on the container. Tag-dictionary flattening is
/// platform-specific (Android hands back a Java <c>JSONObject</c>, iOS an <c>NSDictionary</c>)
/// and lives in <c>Platforms/Android</c> / <c>Platforms/iOS</c> instead.
/// </remarks>
internal static class MediaValues
{
    internal static TimeSpan? Seconds(string? value) =>
        Number(value) is { } seconds ? TimeSpan.FromSeconds(seconds) : null;

    internal static long? Integer(string? value) =>
        long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;

    internal static double? Number(string? value) =>
        double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;

    /// <summary>Parses a rational such as "30/1" or "30000/1001", as FFprobe reports rates.</summary>
    internal static double? Rational(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var separator = value.IndexOf('/');

        if (separator < 0)
            return Number(value);

        var numerator = Number(value[..separator]);
        var denominator = Number(value[(separator + 1)..]);

        // "0/0" is FFprobe's way of saying it does not know.
        return numerator is null || denominator is null or 0 ? null : numerator / denominator;
    }
}
