namespace Ffmpegkit.Net.Maui;

public static partial class FFmpegKitMaui
{
    // iOS has no Storage Access Framework equivalent - MAUI always hands back a real,
    // FFmpeg-readable file path here, so this is a pass-through.
    public static partial Task<string> GetInputArgumentAsync(FileResult file)
    {
        ArgumentNullException.ThrowIfNull(file);

        return Task.FromResult(file.FullPath);
    }
}
