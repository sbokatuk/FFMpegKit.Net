using System;
using System.Collections.Generic;
using System.Globalization;

namespace Ffmpegkit.Droid
{
	/// <summary>
	/// Parsing for the values FFprobe reports as strings.
	/// </summary>
	/// <remarks>
	/// FFprobe emits numbers in a fixed, invariant format - <c>"12.345000"</c>, <c>"30/1"</c> -
	/// and always with a dot as the decimal separator. Parsing them with the ambient culture is
	/// wrong and quietly so: on a German device <c>double.Parse("12.345000")</c> returns
	/// 12,345,000 because the dot reads as a group separator, and on a French one it throws.
	/// Everything here parses invariantly, and returns null rather than throwing when a value is
	/// absent or unparseable - FFprobe omits fields routinely depending on the container.
	/// </remarks>
	internal static class MediaValues
	{
		internal static double? Seconds (string value) => Number (value);

		internal static long? Integer (string value) =>
			long.TryParse (value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
				? parsed
				: null;

		internal static double? Number (string value) =>
			double.TryParse (value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
				? parsed
				: null;

		internal static int? Int32 (Java.Lang.Long value) =>
			value is null ? null : checked ((int)value.LongValue ());

		/// <summary>Parses a rational such as "30/1" or "30000/1001", as FFprobe reports rates.</summary>
		internal static double? Rational (string value)
		{
			if (string.IsNullOrWhiteSpace (value))
				return null;

			var separator = value.IndexOf ('/');

			if (separator < 0)
				return Number (value);

			var numerator = Number (value.Substring (0, separator));
			var denominator = Number (value.Substring (separator + 1));

			// "0/0" is FFprobe's way of saying it does not know.
			return numerator is null || denominator is null or 0 ? null : numerator / denominator;
		}

		/// <summary>Flattens a JSON object into string values, so callers need no Java JSON type.</summary>
		internal static IReadOnlyDictionary<string, string> ToDictionary (Org.Json.JSONObject json)
		{
			var values = new Dictionary<string, string> (StringComparer.OrdinalIgnoreCase);

			if (json is null)
				return values;

			var keys = json.Keys ();

			while (keys.HasNext) {
				// The iterator yields Java.Lang.Object, so the key comes back across JNI boxed.
				var key = keys.Next ()?.ToString ();

				if (string.IsNullOrEmpty (key))
					continue;

				var value = json.OptString (key, null);

				if (value is not null)
					values[key] = value;
			}

			return values;
		}
	}
}
