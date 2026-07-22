using System;

namespace Ffmpegkit.Ios
{
	public partial class FFmpegKitConfig
	{
		/// <summary>The current log level, as the <see cref="Ffmpegkit.Ios.Level"/> enum.</summary>
		/// <remarks>
		/// The bound <see cref="LogLevel"/> is an <see cref="int"/>, faithfully to the native
		/// <c>+ (int)getLogLevel</c>, even though the values it accepts are exactly the members of
		/// an enum the binding already exposes. These two wrap it so callers do not pass integers
		/// whose meaning is only documented elsewhere.
		/// </remarks>
		public static Level GetLogLevel () => (Level) LogLevel;

		/// <inheritdoc cref="GetLogLevel"/>
		public static void SetLogLevel (Level level) => LogLevel = (int) level;

		/// <summary>Whether this is a long term support build of FFmpegKit.</summary>
		/// <remarks>
		/// The bound <see cref="IsLTSBuild"/> returns <see cref="int"/> because the native
		/// signature is <c>+ (int)isLTSBuild</c>, so testing it reads <c>IsLTSBuild != 0</c>.
		/// </remarks>
		public static bool IsLtsBuild => IsLTSBuild != 0;
	}

	/// <summary>
	/// Small conveniences over the generated binding. These exist as extension methods rather
	/// than partial members because the names they want - <c>Level</c> in particular - collide
	/// with types and generated properties inside the classes themselves.
	/// </summary>
	public static class FFmpegKitExtensions
	{
		/// <summary>Whether the session finished successfully.</summary>
		/// <remarks>
		/// A session that has not completed has no return code at all, so the bare
		/// <c>session.ReturnCode.IsValueSuccess</c> throws rather than answering false. This
		/// treats "no return code yet" as not successful.
		/// </remarks>
		public static bool Succeeded (this AbstractSession session)
		{
			if (session is null)
				throw new ArgumentNullException (nameof (session));

			return session.ReturnCode is { IsValueSuccess: true };
		}

		/// <summary>Whether the session was cancelled.</summary>
		public static bool Cancelled (this AbstractSession session)
		{
			if (session is null)
				throw new ArgumentNullException (nameof (session));

			return session.ReturnCode is { IsValueCancel: true };
		}

		/// <summary>The severity of this log line, as the <see cref="Ffmpegkit.Ios.Level"/> enum.</summary>
		/// <remarks>
		/// <see cref="Log.Level"/> is an <see cref="int"/>; the enum cannot be exposed under that
		/// name on the class itself, because the member and the type would collide.
		/// </remarks>
		public static Level Severity (this Log log)
		{
			if (log is null)
				throw new ArgumentNullException (nameof (log));

			return (Level) log.Level;
		}
	}
}
