using Ffmpegkit.Mac;

using MacConfig = Ffmpegkit.Mac.FFmpegKitConfig;

namespace Ffmpegkit.Net;

public static partial class FFmpegKitConfig
{
    public static partial FFmpegLogLevel GetLogLevel() => ToShared(MacConfig.GetLogLevel());

    public static partial void SetLogLevel(FFmpegLogLevel level) => MacConfig.SetLogLevel(ToNative(level));

    public static partial void EnableLogCallback(Action<string>? logCallback)
    {
        if (logCallback is null)
        {
            MacConfig.EnableLogCallback(null);
            return;
        }

        MacConfig.EnableLogCallback(log => logCallback(log.Message));
    }

    public static partial void ClearSessions() => MacConfig.ClearSessions();

    // Level is a plain C# enum on macOS (bound from a native NS_ENUM, same as on iOS), unlike
    // Android's Java enum, so this is an ordinary switch rather than the Equals-based comparison
    // Platforms/Android needs - but the underlying values still do not match FFmpegLogLevel's,
    // so a cast is wrong.
    private static FFmpegLogLevel ToShared(Level level) => level switch
    {
        Level.Quiet => FFmpegLogLevel.Quiet,
        Level.Panic => FFmpegLogLevel.Panic,
        Level.Fatal => FFmpegLogLevel.Fatal,
        Level.Error => FFmpegLogLevel.Error,
        Level.Warning => FFmpegLogLevel.Warning,
        Level.Info => FFmpegLogLevel.Info,
        Level.Verbose => FFmpegLogLevel.Verbose,
        Level.Debug => FFmpegLogLevel.Debug,
        Level.Trace => FFmpegLogLevel.Trace,
        Level.StdErr => FFmpegLogLevel.StdErr,
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Unrecognised FFmpeg log level."),
    };

    private static Level ToNative(FFmpegLogLevel level) => level switch
    {
        FFmpegLogLevel.Quiet => Level.Quiet,
        FFmpegLogLevel.Panic => Level.Panic,
        FFmpegLogLevel.Fatal => Level.Fatal,
        FFmpegLogLevel.Error => Level.Error,
        FFmpegLogLevel.Warning => Level.Warning,
        FFmpegLogLevel.Info => Level.Info,
        FFmpegLogLevel.Verbose => Level.Verbose,
        FFmpegLogLevel.Debug => Level.Debug,
        FFmpegLogLevel.Trace => Level.Trace,
        FFmpegLogLevel.StdErr => Level.StdErr,
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Unrecognised FFmpeg log level."),
    };
}
