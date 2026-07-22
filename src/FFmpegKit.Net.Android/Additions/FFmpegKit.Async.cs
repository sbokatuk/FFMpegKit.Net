using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ffmpegkit.Droid
{
	public partial class FFmpegKit
	{
		/// <summary>Runs an FFmpeg command and awaits its completion.</summary>
		/// <param name="command">The FFmpeg command, as it would be typed after <c>ffmpeg</c>.</param>
		/// <param name="cancellationToken">Cancels the running session.</param>
		/// <returns>The completed session. Inspect <see cref="AbstractSession.ReturnCode"/> for the outcome.</returns>
		/// <remarks>
		/// The synchronous <see cref="Execute(string)"/> blocks the calling thread for the whole
		/// transcode, which on the UI thread means a frozen app. This wraps FFmpegKit's own
		/// asynchronous path so the command can simply be awaited.
		/// <para>
		/// A failing command completes the task normally with a non-success return code; it does
		/// not throw. Cancellation asks FFmpeg to stop, and the session then completes with a
		/// cancelled return code rather than raising <see cref="OperationCanceledException"/> —
		/// FFmpeg may still have written a partial output file.
		/// </para>
		/// </remarks>
		public static Task<FFmpegSession> ExecuteAsync (string command, CancellationToken cancellationToken = default)
		{
			if (command is null)
				throw new ArgumentNullException (nameof (command));

			return RunAsync (
				callback => ExecuteAsync (command, callback),
				cancellationToken);
		}

		/// <summary>Runs an FFmpeg command from pre-split arguments and awaits its completion.</summary>
		/// <remarks>
		/// Prefer this over <see cref="ExecuteAsync(string,CancellationToken)"/> when any argument
		/// may contain spaces or quotes, such as a file path: no quoting rules are involved.
		/// </remarks>
		public static Task<FFmpegSession> ExecuteWithArgumentsAsync (string[] arguments, CancellationToken cancellationToken = default)
		{
			if (arguments is null)
				throw new ArgumentNullException (nameof (arguments));

			return RunAsync (
				callback => ExecuteWithArgumentsAsync (arguments, callback),
				cancellationToken);
		}

		/// <summary>Runs an FFmpeg command, reporting progress, and awaits its completion.</summary>
		/// <param name="command">The FFmpeg command, as it would be typed after <c>ffmpeg</c>.</param>
		/// <param name="progress">Receives a sample each time FFmpeg reports statistics.</param>
		/// <param name="totalDuration">
		/// Duration of the material being processed. Supply it to get
		/// <see cref="FFmpegProgress.Percent"/> and an estimated time remaining; without it the
		/// other fields are still reported. <see cref="MediaInformation.DurationOrNull"/> from an
		/// <see cref="FFprobeKit.GetMediaInformationAsync(string)"/> call is the usual source.
		/// </param>
		/// <param name="cancellationToken">Cancels the running session.</param>
		/// <remarks>
		/// Progress is reported on an FFmpegKit worker thread, so marshal to the UI thread before
		/// touching UI. Statistics are per session here, not the global
		/// <see cref="FFmpegKitConfig.EnableStatisticsCallback(Action{Statistics})"/> hook, so
		/// concurrent commands do not report into each other's handlers.
		/// </remarks>
		public static Task<FFmpegSession> ExecuteAsync (
			string command,
			IProgress<FFmpegProgress> progress,
			TimeSpan? totalDuration = null,
			CancellationToken cancellationToken = default)
		{
			if (command is null)
				throw new ArgumentNullException (nameof (command));
			if (progress is null)
				throw new ArgumentNullException (nameof (progress));

			var statistics = new ActionStatisticsCallback (
				sample => progress.Report (FFmpegProgress.From (sample, totalDuration)));

			return RunAsync (
				callback => ExecuteAsync (command, callback, logCallback: null, statistics),
				cancellationToken);
		}

		private static Task<FFmpegSession> RunAsync (
			Func<IFFmpegSessionCompleteCallback, FFmpegSession> start,
			CancellationToken cancellationToken)
		{
			// RunContinuationsAsynchronously: the callback arrives on an FFmpegKit worker thread,
			// and continuations must not run there and block its executor.
			var completion = new TaskCompletionSource<FFmpegSession> (TaskCreationOptions.RunContinuationsAsynchronously);

			if (cancellationToken.IsCancellationRequested)
				return Task.FromCanceled<FFmpegSession> (cancellationToken);

			CancellationTokenRegistration registration = default;

			var callback = new ActionFFmpegSessionCompleteCallback (session => {
				registration.Dispose ();
				completion.TrySetResult (session);
			});

			var started = start (callback);

			if (cancellationToken.CanBeCanceled) {
				// Captured before registering: the session is needed to cancel just this command
				// rather than every session FFmpegKit is running.
				var sessionId = started.SessionId;
				registration = cancellationToken.Register (() => Cancel (sessionId));

				// The token may have been cancelled between the check above and registering.
				if (cancellationToken.IsCancellationRequested)
					Cancel (sessionId);
			}

			return completion.Task;
		}
	}
}
