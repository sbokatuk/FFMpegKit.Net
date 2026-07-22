using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ffmpegkit.Ios
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

			return RunAsync (callback => ExecuteAsync (command, callback), cancellationToken);
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

			return RunAsync (callback => ExecuteWithArgumentsAsync (arguments, callback), cancellationToken);
		}

		/// <summary>
		/// Bridges one of FFmpegKit's completion-callback overloads onto a task, and wires the
		/// token to the session's own cancellation.
		/// </summary>
		static Task<FFmpegSession> RunAsync (
			Func<FFmpegSessionCompleteCallback, FFmpegSession> start,
			CancellationToken cancellationToken)
		{
			// RunContinuationsAsynchronously so awaiting code never resumes on FFmpegKit's
			// internal callback thread, which would tie up the session machinery for as long as
			// the continuation runs.
			var completion = new TaskCompletionSource<FFmpegSession> (TaskCreationOptions.RunContinuationsAsynchronously);

			var session = start (completed => completion.TrySetResult (completed));

			if (cancellationToken.CanBeCanceled) {
				// Registered after the session exists, so there is something to cancel. If the
				// command already finished, the continuation below disposes the registration
				// immediately and the token is simply never acted on.
				var registration = cancellationToken.Register (session.Cancel);

				completion.Task.ContinueWith (
					_ => registration.Dispose (),
					CancellationToken.None,
					TaskContinuationOptions.ExecuteSynchronously,
					TaskScheduler.Default);
			}

			return completion.Task;
		}
	}
}
