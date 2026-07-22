using Ffmpegkit.Droid;

using DroidConfig = Ffmpegkit.Droid.FFmpegKitConfig;

namespace Ffmpegkit.Net;

public static partial class FFmpegKitConfig
{
    // Unlike iOS's LogLevel (a raw int wrapped by Additions/Ergonomics.cs into GetLogLevel/
    // SetLogLevel), the Android binding already exposes LogLevel as a Level-typed property
    // directly (com.arthenica.ffmpegkit.FFmpegKitConfig.getLogLevel/setLogLevel), so there is no
    // Additions wrapper to call here - just the native <-> shared enum conversion.
    public static partial FFmpegLogLevel GetLogLevel() => ToShared(DroidConfig.LogLevel);

    public static partial void SetLogLevel(FFmpegLogLevel level) => DroidConfig.LogLevel = ToNative(level);

    public static partial void EnableLogCallback(Action<string>? logCallback)
    {
        if (logCallback is null)
        {
            // The Additions overload (Action<Log>) throws on null; clearing the callback means
            // calling the raw generated overload directly.
            DroidConfig.EnableLogCallback((ILogCallback?)null);
            return;
        }

        DroidConfig.EnableLogCallback(log => logCallback(log.Message));
    }

    public static partial void ClearSessions() => DroidConfig.ClearSessions();

    // Level is a real Java enum (Java.Lang.Enum), not a C# one, so its constants cannot be used
    // in a switch and must be compared with Equals rather than == - see Additions/Enums.cs, which
    // this mirrors in the opposite direction (managed -> native rather than native -> managed).

    private static FFmpegLogLevel ToShared(Level level)
    {
        if (level.Equals(Level.AvLogQuiet)) return FFmpegLogLevel.Quiet;
        if (level.Equals(Level.AvLogPanic)) return FFmpegLogLevel.Panic;
        if (level.Equals(Level.AvLogFatal)) return FFmpegLogLevel.Fatal;
        if (level.Equals(Level.AvLogError)) return FFmpegLogLevel.Error;
        if (level.Equals(Level.AvLogWarning)) return FFmpegLogLevel.Warning;
        if (level.Equals(Level.AvLogInfo)) return FFmpegLogLevel.Info;
        if (level.Equals(Level.AvLogVerbose)) return FFmpegLogLevel.Verbose;
        if (level.Equals(Level.AvLogDebug)) return FFmpegLogLevel.Debug;
        if (level.Equals(Level.AvLogTrace)) return FFmpegLogLevel.Trace;
        if (level.Equals(Level.AvLogStderr)) return FFmpegLogLevel.StdErr;

        throw new ArgumentOutOfRangeException(nameof(level), level.ToString(), "Unrecognised FFmpeg log level.");
    }

    private static Level ToNative(FFmpegLogLevel level) => level switch
    {
        FFmpegLogLevel.Quiet => Level.AvLogQuiet,
        FFmpegLogLevel.Panic => Level.AvLogPanic,
        FFmpegLogLevel.Fatal => Level.AvLogFatal,
        FFmpegLogLevel.Error => Level.AvLogError,
        FFmpegLogLevel.Warning => Level.AvLogWarning,
        FFmpegLogLevel.Info => Level.AvLogInfo,
        FFmpegLogLevel.Verbose => Level.AvLogVerbose,
        FFmpegLogLevel.Debug => Level.AvLogDebug,
        FFmpegLogLevel.Trace => Level.AvLogTrace,
        FFmpegLogLevel.StdErr => Level.AvLogStderr,
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Unrecognised FFmpeg log level."),
    };
}
