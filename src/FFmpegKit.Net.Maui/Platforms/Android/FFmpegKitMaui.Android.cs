using DroidConfig = Ffmpegkit.Droid.FFmpegKitConfig;
using AndroidUri = Android.Net.Uri;

namespace Ffmpegkit.Net.Maui;

public static partial class FFmpegKitMaui
{
    public static partial Task<string> GetInputArgumentAsync(FileResult file)
    {
        ArgumentNullException.ThrowIfNull(file);

        // MAUI's FilePicker copies into the cache and returns a real path; this only fires for a
        // content:// URI that reached FullPath directly, e.g. from a share intent.
        if (!file.FullPath.StartsWith("content://", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(file.FullPath);

        var uri = AndroidUri.Parse(file.FullPath)
            ?? throw new InvalidOperationException($"'{file.FullPath}' is not a valid content URI.");

        return Task.FromResult(DroidConfig.GetSafParameterForRead(uri));
    }
}
