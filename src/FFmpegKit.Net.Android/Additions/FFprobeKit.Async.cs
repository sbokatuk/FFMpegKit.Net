using System;
using System.Threading.Tasks;

namespace Ffmpegkit.Droid
{
	public partial class FFprobeKit
	{
		/// <summary>Probes a file and awaits the parsed media information.</summary>
		/// <param name="path">Path to the media file.</param>
		/// <returns>
		/// The completed session. <see cref="MediaInformationSession.MediaInformation"/> is null
		/// when the file could not be parsed; check <see cref="AbstractSession.ReturnCode"/>.
		/// </returns>
		/// <remarks>
		/// No <c>CancellationToken</c> overload is offered: FFmpegKit exposes no cancellation for
		/// probe sessions, so a token could only abandon the wait while the probe kept running.
		/// Probes are short; if you need to give up waiting, call <c>WaitAsync</c> on the returned
		/// task yourself, and note the session still completes in the background.
		/// </remarks>
		public static Task<MediaInformationSession> GetMediaInformationAsync (string path)
		{
			if (path is null)
				throw new ArgumentNullException (nameof (path));

			var completion = new TaskCompletionSource<MediaInformationSession> (TaskCreationOptions.RunContinuationsAsynchronously);

			GetMediaInformationAsync (
				path,
				new ActionMediaInformationSessionCompleteCallback (session => completion.TrySetResult (session)));

			return completion.Task;
		}

		/// <summary>Runs an FFprobe command and awaits its completion.</summary>
		/// <param name="command">The FFprobe command, as it would be typed after <c>ffprobe</c>.</param>
		/// <remarks>See <see cref="GetMediaInformationAsync(string)"/> for why there is no cancellation overload.</remarks>
		public static Task<FFprobeSession> ExecuteAsync (string command)
		{
			if (command is null)
				throw new ArgumentNullException (nameof (command));

			var completion = new TaskCompletionSource<FFprobeSession> (TaskCreationOptions.RunContinuationsAsynchronously);

			ExecuteAsync (
				command,
				new ActionFFprobeSessionCompleteCallback (session => completion.TrySetResult (session)));

			return completion.Task;
		}
	}
}
