using System;

namespace Ffmpegkit.Droid
{
	// Adapters adapting a delegate to FFmpegKit's single-method Java callback interfaces.
	//
	// The generated interfaces cannot accept a lambda: C# converts lambdas to delegates, not to
	// interfaces, so without these every caller declares a class to run one line of code. Each
	// adapter derives from Java.Lang.Object because the instance is handed to Java and needs a
	// peer that survives the crossing, and each implements exactly one interface so that JNI
	// dispatch stays unambiguous.

	internal sealed class ActionFFmpegSessionCompleteCallback : Java.Lang.Object, IFFmpegSessionCompleteCallback
	{
		private readonly Action<FFmpegSession> _handler;

		internal ActionFFmpegSessionCompleteCallback (Action<FFmpegSession> handler) =>
			_handler = handler ?? throw new ArgumentNullException (nameof (handler));

		public void Apply (FFmpegSession session) => _handler (session);
	}

	internal sealed class ActionFFprobeSessionCompleteCallback : Java.Lang.Object, IFFprobeSessionCompleteCallback
	{
		private readonly Action<FFprobeSession> _handler;

		internal ActionFFprobeSessionCompleteCallback (Action<FFprobeSession> handler) =>
			_handler = handler ?? throw new ArgumentNullException (nameof (handler));

		public void Apply (FFprobeSession session) => _handler (session);
	}

	internal sealed class ActionMediaInformationSessionCompleteCallback : Java.Lang.Object, IMediaInformationSessionCompleteCallback
	{
		private readonly Action<MediaInformationSession> _handler;

		internal ActionMediaInformationSessionCompleteCallback (Action<MediaInformationSession> handler) =>
			_handler = handler ?? throw new ArgumentNullException (nameof (handler));

		public void Apply (MediaInformationSession session) => _handler (session);
	}

	internal sealed class ActionLogCallback : Java.Lang.Object, ILogCallback
	{
		private readonly Action<Log> _handler;

		internal ActionLogCallback (Action<Log> handler) =>
			_handler = handler ?? throw new ArgumentNullException (nameof (handler));

		public void Apply (Log log) => _handler (log);
	}

	internal sealed class ActionStatisticsCallback : Java.Lang.Object, IStatisticsCallback
	{
		private readonly Action<Statistics> _handler;

		internal ActionStatisticsCallback (Action<Statistics> handler) =>
			_handler = handler ?? throw new ArgumentNullException (nameof (handler));

		public void Apply (Statistics statistics) => _handler (statistics);
	}
}
