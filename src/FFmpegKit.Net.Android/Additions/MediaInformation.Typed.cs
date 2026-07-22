using System;
using System.Collections.Generic;

namespace Ffmpegkit.Droid
{
	public partial class MediaInformation
	{
		/// <summary>Media duration, or null when the container does not report one.</summary>
		/// <remarks>
		/// Typed counterpart of <see cref="Duration"/>, which is the raw FFprobe string. Parse that
		/// yourself and you inherit a culture bug: <c>double.Parse("12.345000")</c> yields
		/// 12,345,000 under a German locale and throws under a French one.
		/// </remarks>
		public TimeSpan? DurationOrNull =>
			MediaValues.Seconds (Duration) is { } seconds
				? TimeSpan.FromSeconds (seconds)
				: null;

		/// <summary>Overall bit rate in bits per second, or null when not reported.</summary>
		public long? BitrateBps => MediaValues.Integer (Bitrate);

		/// <summary>File size in bytes, or null when not reported.</summary>
		public long? SizeBytes => MediaValues.Integer (Size);

		/// <summary>Start time, or null when not reported.</summary>
		public TimeSpan? StartTimeOrNull =>
			MediaValues.Seconds (StartTime) is { } seconds
				? TimeSpan.FromSeconds (seconds)
				: null;

		/// <summary>Container metadata tags (title, artist, creation_time, ...), keyed case-insensitively.</summary>
		/// <remarks>Typed counterpart of <see cref="Tags"/>, which is a Java <c>JSONObject</c>.</remarks>
		public IReadOnlyDictionary<string, string> TagValues => MediaValues.ToDictionary (Tags);
	}

	public partial class StreamInformation
	{
		/// <summary>Frame width in pixels, or null for non-video streams.</summary>
		/// <remarks>Typed counterpart of <see cref="Width"/>, which is a boxed Java <c>Long</c>.</remarks>
		public int? PixelWidth => MediaValues.Int32 (Width);

		/// <summary>Frame height in pixels, or null for non-video streams.</summary>
		public int? PixelHeight => MediaValues.Int32 (Height);

		/// <summary>Stream index, or null when not reported.</summary>
		public int? IndexOrNull => MediaValues.Int32 (Index);

		/// <summary>Stream bit rate in bits per second, or null when not reported.</summary>
		public long? BitrateBps => MediaValues.Integer (Bitrate);

		/// <summary>Audio sample rate in Hz, or null for non-audio streams.</summary>
		public int? SampleRateHz =>
			MediaValues.Integer (SampleRate) is { } rate ? checked ((int)rate) : null;

		/// <summary>Average frame rate in frames per second, or null when not reported.</summary>
		/// <remarks>
		/// <see cref="AverageFrameRate"/> is a rational string such as <c>"30000/1001"</c>, which is
		/// how FFprobe expresses rates like 29.97 exactly. This evaluates it.
		/// </remarks>
		public double? AverageFrameRateFps => MediaValues.Rational (AverageFrameRate);

		/// <summary>Real frame rate in frames per second, or null when not reported.</summary>
		public double? RealFrameRateFps => MediaValues.Rational (RealFrameRate);

		/// <summary>True when this is a video stream.</summary>
		public bool IsVideo => string.Equals (Type, "video", StringComparison.OrdinalIgnoreCase);

		/// <summary>True when this is an audio stream.</summary>
		public bool IsAudio => string.Equals (Type, "audio", StringComparison.OrdinalIgnoreCase);

		/// <summary>Stream metadata tags, keyed case-insensitively.</summary>
		public IReadOnlyDictionary<string, string> TagValues => MediaValues.ToDictionary (Tags);
	}
}
