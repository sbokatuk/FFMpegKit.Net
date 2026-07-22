namespace Ffmpegkit.Net.Maui;

/// <summary>MAUI-specific conveniences on top of <c>Ffmpegkit.Net</c>.</summary>
public static partial class FFmpegKitMaui
{
    /// <summary>Turns a file picked through MAUI into an argument FFmpeg can read from.</summary>
    /// <param name="file">A result from <c>FilePicker</c>, <c>MediaPicker</c>, or similar.</param>
    /// <returns>
    /// A path or argument to pass to FFmpeg in place of a file name. On Android this is already
    /// a complete argument if it came from Storage Access Framework - do not wrap it in quotes.
    /// </returns>
    /// <remarks>
    /// MAUI's <c>FilePicker</c> normally copies the picked file into the app's cache and hands
    /// back a real path, in which case this is a pass-through. It only does real work for a
    /// <c>content://</c> URI that reaches <c>FileResult.FullPath</c>
    /// directly - from a share intent or a picker that does not copy - which FFmpeg cannot open
    /// on Android 10+ without registering it via Storage Access Framework first. iOS has no
    /// equivalent indirection, so there this is always a pass-through.
    /// </remarks>
    public static partial Task<string> GetInputArgumentAsync(FileResult file);
}
