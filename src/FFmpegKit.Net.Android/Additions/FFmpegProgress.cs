using System;

namespace Ffmpegkit.Droid
{
	/// <summary>
	/// A progress sample taken while an FFmpeg command runs.
	/// </summary>
	/// <param name="Position">How far into the output FFmpeg has written.</param>
	/// <param name="Percent">
	/// Fraction of the way through, 0 to 1, or null when the total duration is not known.
	/// </param>
	/// <param name="SizeBytes">Bytes written so far.</param>
	/// <param name="Bitrate">Current bit rate in kbit/s, or null when FFmpeg reports none.</param>
	/// <param name="Speed">Encoding speed relative to realtime; 2.0 means twice as fast as playback.</param>
	/// <param name="VideoFrameNumber">Frames encoded so far, 0 for audio-only work.</param>
	/// <param name="VideoFps">Current frames per second, 0 for audio-only work.</param>
	public sealed record FFmpegProgress (
		TimeSpan Position,
		double? Percent,
		long SizeBytes,
		double? Bitrate,
		double Speed,
		int VideoFrameNumber,
		float VideoFps)
	{
		/// <summary>Estimated time remaining, or null without a total duration or a usable speed.</summary>
		/// <remarks>
		/// Derived from <see cref="Speed"/>, which FFmpeg reports relative to realtime, so this is
		/// only as steady as the encode is. Treat it as an indication, not a countdown.
		/// </remarks>
		public TimeSpan? EstimatedTimeRemaining { get; init; }

		internal static FFmpegProgress From (Statistics statistics, TimeSpan? totalDuration)
		{
			// Statistics.Time is milliseconds into the output. FFmpeg can report a negative value
			// before the first frame is written.
			var position = TimeSpan.FromMilliseconds (Math.Max (statistics.Time, 0));

			double? percent = null;
			TimeSpan? remaining = null;

			if (totalDuration is { } total && total > TimeSpan.Zero) {
				percent = Math.Clamp (position.TotalMilliseconds / total.TotalMilliseconds, 0, 1);

				if (statistics.Speed > 0) {
					var outstanding = total - position;

					if (outstanding > TimeSpan.Zero)
						remaining = TimeSpan.FromSeconds (outstanding.TotalSeconds / statistics.Speed);
				}
			}

			// FFmpeg reports a bitrate of 0 (and occasionally a negative) before it has enough
			// output to measure one.
			var bitrate = statistics.Bitrate > 0 ? statistics.Bitrate : (double?)null;

			return new FFmpegProgress (
				position,
				percent,
				statistics.Size,
				bitrate,
				statistics.Speed,
				statistics.VideoFrameNumber,
				statistics.VideoFps) {
				EstimatedTimeRemaining = remaining,
			};
		}
	}
}
