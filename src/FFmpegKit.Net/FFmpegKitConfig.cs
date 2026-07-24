namespace Ffmpegkit.Net;

/// <summary>Global FFmpegKit configuration shared across all platforms.</summary>
/// <remarks>
/// A small slice of <c>Ffmpegkit.Droid.FFmpegKitConfig</c> / <c>Ffmpegkit.Ios.FFmpegKitConfig</c> -
/// enough for the common case of routing FFmpeg's own log lines somewhere and bounding the memory
/// a long-running app spends on session history. Reach for the platform type directly for
/// anything else (Storage Access Framework helpers, per-session statistics history, ...).
/// </remarks>
public static partial class FFmpegKitConfig
{
    /// <summary>Returns the current FFmpeg log level.</summary>
    public static partial FFmpegLogLevel GetLogLevel();

    /// <summary>Sets the FFmpeg log level.</summary>
    public static partial void SetLogLevel(FFmpegLogLevel level);

    /// <summary>
    /// Routes FFmpeg log output to a delegate.
    /// </summary>
    /// <remarks>
    /// The callback runs on an FFmpegKit worker thread - marshal to the UI thread before touching
    /// UI. It is held by FFmpegKit until replaced, so anything it captures stays alive too; avoid
    /// capturing a view or activity. Pass null to stop routing logs through this callback.
    /// </remarks>
    public static partial void EnableLogCallback(Action<string>? logCallback);

    /// <summary>
    /// Drops every session FFmpegKit is holding in memory, each with its full log output.
    /// </summary>
    /// <remarks>
    /// FFmpegKit keeps every session up to its history size limit. An app running many
    /// conversions accumulates them with no obvious cause until this is called between batches.
    /// </remarks>
    public static partial void ClearSessions();
}
