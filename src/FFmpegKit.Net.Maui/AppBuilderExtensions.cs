namespace Ffmpegkit.Net.Maui;

/// <summary>Wires the package into a MAUI app.</summary>
public static class AppBuilderExtensions
{
    /// <summary>
    /// Registers FFmpegKit.Net.Maui with the app. Neither platform binding needs an Activity or
    /// a handler the way a video-rendering SDK would, so this does not wire up anything today -
    /// it exists so the startup call is in place if a future FFmpegKit version needs one (for
    /// instance, a runtime permission check before writing output files), without every consumer
    /// having to add it retroactively.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.UseMauiApp&lt;App&gt;().UseFFmpegKit();
    /// </code>
    /// </example>
    public static MauiAppBuilder UseFFmpegKit(this MauiAppBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder;
    }
}
