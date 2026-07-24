namespace Ffmpegkit.Net;

/// <summary>FFmpeg log levels, shared across both platforms.</summary>
/// <remarks>
/// The platform bindings project FFmpegKit's native <c>Level</c> as a Java enum on Android and an
/// <c>NSInteger</c>-backed type on iOS - neither is a plain C# enum, so neither can be used in a
/// <c>switch</c> directly. This is the common shape both <c>Platforms/Android</c> and
/// <c>Platforms/iOS</c> convert to and from.
/// </remarks>
public enum FFmpegLogLevel
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
