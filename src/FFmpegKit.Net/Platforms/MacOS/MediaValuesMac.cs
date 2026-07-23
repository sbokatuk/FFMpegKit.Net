using Foundation;

namespace Ffmpegkit.Net;

/// <summary>
/// The macOS-specific half of parsing FFprobe output: unwrapping <see cref="NSNumber"/> and
/// flattening <see cref="NSDictionary"/> tags. Numeric strings go through the shared
/// <see cref="MediaValues"/> parser, same as on Android.
/// </summary>
internal static class MediaValuesMac
{
    internal static int? Int32(NSNumber? value) => value?.Int32Value;

    internal static long? Int64(NSNumber? value) => value?.Int64Value;

    /// <summary>Flattens an NSDictionary into string values, as Android's JSONObject.TagValues does.</summary>
    internal static IReadOnlyDictionary<string, string> ToDictionary(NSDictionary? dictionary)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (dictionary is null)
            return values;

        foreach (var key in dictionary.Keys)
        {
            var keyText = key?.ToString();

            if (string.IsNullOrEmpty(keyText))
                continue;

            var value = dictionary[key]?.ToString();

            if (value is not null)
                values[keyText] = value;
        }

        return values;
    }
}
