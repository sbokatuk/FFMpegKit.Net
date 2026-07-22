using System;

namespace Ffmpegkit.Droid
{
	/// <summary>Managed counterpart of the Java <c>SessionState</c> enum.</summary>
	public enum FFmpegKitSessionState
	{
		Created,
		Running,
		Failed,
		Completed,
	}

	/// <summary>Managed counterpart of the Java <c>Level</c> enum (FFmpeg log levels).</summary>
	public enum FFmpegKitLogLevel
	{
		Quiet,
		Panic,
		Fatal,
		Error,
		Warning,
		Info,
		Verbose,
		Debug,
		Trace,
		StdErr,
	}

	/// <summary>
	/// Converts FFmpegKit's Java enums into managed enums.
	/// </summary>
	/// <remarks>
	/// <see cref="SessionState"/> and <see cref="Level"/> are real Java enums, so the binding
	/// projects them as <see cref="Java.Lang.Enum"/> subclasses rather than C# enums. They must
	/// stay that way — the Java objects are what get passed back across JNI — but as a result they
	/// cannot be used in a <c>switch</c>, and comparing them with <c>==</c> compares managed peer
	/// references, which are not guaranteed to be the same instance for the same Java constant.
	/// Converting once at the boundary avoids both problems.
	/// </remarks>
	public static class FFmpegKitEnumExtensions
	{
		/// <summary>Converts a Java <see cref="SessionState"/> to <see cref="FFmpegKitSessionState"/>.</summary>
		/// <exception cref="ArgumentOutOfRangeException">The state is not one FFmpegKit defines.</exception>
		public static FFmpegKitSessionState ToManaged (this SessionState state)
		{
			if (state is null)
				throw new ArgumentNullException (nameof (state));

			// Compared by Java identity rather than by C# reference: Equals crosses to Java, where
			// enum constants are singletons, so it is correct even across distinct managed peers.
			if (state.Equals (SessionState.Created)) return FFmpegKitSessionState.Created;
			if (state.Equals (SessionState.Running)) return FFmpegKitSessionState.Running;
			if (state.Equals (SessionState.Failed)) return FFmpegKitSessionState.Failed;
			if (state.Equals (SessionState.Completed)) return FFmpegKitSessionState.Completed;

			throw new ArgumentOutOfRangeException (nameof (state), state.ToString (), "Unrecognised FFmpegKit session state.");
		}

		/// <summary>Converts a Java <see cref="Level"/> to <see cref="FFmpegKitLogLevel"/>.</summary>
		/// <exception cref="ArgumentOutOfRangeException">The level is not one FFmpegKit defines.</exception>
		public static FFmpegKitLogLevel ToManaged (this Level level)
		{
			if (level is null)
				throw new ArgumentNullException (nameof (level));

			if (level.Equals (Level.AvLogQuiet)) return FFmpegKitLogLevel.Quiet;
			if (level.Equals (Level.AvLogPanic)) return FFmpegKitLogLevel.Panic;
			if (level.Equals (Level.AvLogFatal)) return FFmpegKitLogLevel.Fatal;
			if (level.Equals (Level.AvLogError)) return FFmpegKitLogLevel.Error;
			if (level.Equals (Level.AvLogWarning)) return FFmpegKitLogLevel.Warning;
			if (level.Equals (Level.AvLogInfo)) return FFmpegKitLogLevel.Info;
			if (level.Equals (Level.AvLogVerbose)) return FFmpegKitLogLevel.Verbose;
			if (level.Equals (Level.AvLogDebug)) return FFmpegKitLogLevel.Debug;
			if (level.Equals (Level.AvLogTrace)) return FFmpegKitLogLevel.Trace;
			if (level.Equals (Level.AvLogStderr)) return FFmpegKitLogLevel.StdErr;

			throw new ArgumentOutOfRangeException (nameof (level), level.ToString (), "Unrecognised FFmpeg log level.");
		}

		/// <summary>True when the session finished successfully.</summary>
		/// <remarks>Shorthand for the <c>State</c> / <c>ReturnCode</c> pair that has to be checked together.</remarks>
		public static bool Succeeded (this AbstractSession session)
		{
			if (session is null)
				throw new ArgumentNullException (nameof (session));

			return session.State.ToManaged () == FFmpegKitSessionState.Completed
				&& session.ReturnCode is { IsValueSuccess: true };
		}
	}
}
