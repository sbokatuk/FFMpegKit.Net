namespace Ffmpegkit.Net;

/// <summary>
/// Thrown by <see cref="FFmpegSessionResult.EnsureSuccess"/> when a command ran and failed.
/// </summary>
/// <remarks>
/// The message ends with the tail of the session's console output, so the FFmpeg error text
/// lands directly in logs and test failures; the untruncated output stays available via
/// <see cref="Result"/>.
/// </remarks>
public sealed class FFmpegExecutionException : Exception
{
    // Enough for FFmpeg's closing error lines without turning the exception message into the
    // whole transcript of a long run.
    private const int MessageOutputTailLength = 2000;

    /// <summary>Creates the exception for a failed session's outcome.</summary>
    public FFmpegExecutionException(FFmpegSessionResult result)
        : base(BuildMessage(result ?? throw new ArgumentNullException(nameof(result))))
    {
        Result = result;
    }

    /// <summary>
    /// The failed session's outcome, including the full
    /// <see cref="FFmpegSessionResult.Output"/>.
    /// </summary>
    public FFmpegSessionResult Result { get; }

    private static string BuildMessage(FFmpegSessionResult result)
    {
        var message =
            $"FFmpeg session {result.SessionId} failed with return code " +
            $"{result.ReturnCode?.ToString() ?? "<none>"}.";

        // A session that never ran has no output; its explanation is the native stack trace.
        var detail = string.IsNullOrWhiteSpace(result.Output) ? result.FailStackTrace : result.Output;
        if (string.IsNullOrWhiteSpace(detail))
        {
            return message;
        }

        detail = detail.Trim();
        if (detail.Length > MessageOutputTailLength)
        {
            detail = "…" + detail[^MessageOutputTailLength..];
        }

        return message + Environment.NewLine + detail;
    }
}
