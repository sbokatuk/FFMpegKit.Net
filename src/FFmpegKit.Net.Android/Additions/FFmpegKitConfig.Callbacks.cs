using System;

namespace Ffmpegkit.Droid
{
	public partial class FFmpegKitConfig
	{
		/// <summary>Routes FFmpeg log output to a delegate.</summary>
		/// <remarks>
		/// Overload of <see cref="EnableLogCallback(ILogCallback)"/> that takes a delegate, so a
		/// lambda can be used instead of a class implementing the interface. The callback runs on
		/// an FFmpegKit worker thread — marshal to the UI thread before touching UI.
		/// <para>
		/// The delegate is held by FFmpegKit until it is replaced, so anything it captures stays
		/// alive too — avoid capturing an Activity. There is no Disable method: clear it by
		/// calling <c>EnableLogCallback((ILogCallback)null)</c>.
		/// </para>
		/// </remarks>
		public static void EnableLogCallback (Action<Log> logCallback)
		{
			if (logCallback is null)
				throw new ArgumentNullException (nameof (logCallback));

			EnableLogCallback (new ActionLogCallback (logCallback));
		}

		/// <summary>Routes FFmpeg progress statistics to a delegate.</summary>
		/// <remarks>
		/// Overload of <see cref="EnableStatisticsCallback(IStatisticsCallback)"/> taking a
		/// delegate. Fires frequently during a transcode; see
		/// <see cref="EnableLogCallback(Action{Log})"/> for the threading and lifetime caveats.
		/// </remarks>
		public static void EnableStatisticsCallback (Action<Statistics> statisticsCallback)
		{
			if (statisticsCallback is null)
				throw new ArgumentNullException (nameof (statisticsCallback));

			EnableStatisticsCallback (new ActionStatisticsCallback (statisticsCallback));
		}
	}
}
