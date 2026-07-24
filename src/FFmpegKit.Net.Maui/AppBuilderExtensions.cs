namespace Ffmpegkit.Net.Maui;

/// <summary>Wires the package into a MAUI app.</summary>
public static class AppBuilderExtensions
{
    /// <summary>
    /// Registers FFmpegKit with the app: <see cref="IFFmpegKit"/> becomes resolvable (as a
    /// singleton <see cref="FFmpegKitClient"/>), so pages and view models can take it as a
    /// constructor dependency instead of calling the static classes.
    /// </summary>
    /// <remarks>
    /// Registration is idempotent and never overwrites an existing <see cref="IFFmpegKit"/> -
    /// a fake registered by a test harness wins. Apps that prefer the static
    /// <c>FFmpegKit</c>/<c>FFprobeKit</c> API can skip this call entirely; nothing else in the
    /// package depends on it.
    /// </remarks>
    /// <example>
    /// <code>
    /// builder.UseMauiApp&lt;App&gt;().UseFFmpegKit();
    ///
    /// public MainPage(IFFmpegKit ffmpeg) { _ffmpeg = ffmpeg; }
    /// </code>
    /// </example>
    public static MauiAppBuilder UseFFmpegKit(this MauiAppBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddFFmpegKit();
        return builder;
    }
}
