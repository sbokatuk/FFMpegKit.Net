namespace Ffmpegkit.Net;

/// <summary>
/// The default <see cref="IFFmpegKit"/>: a stateless instance facade over the static API.
/// </summary>
/// <remarks>
/// Every instance delegates to the same process-global FFmpegKit, so singleton, scoped and
/// transient registrations behave identically - singleton is the natural choice. Holds no
/// resources and needs no disposal.
/// </remarks>
public sealed class FFmpegKitClient : IFFmpegKit
{
    /// <inheritdoc/>
    public Task<FFmpegSessionResult> ExecuteAsync(string command, CancellationToken cancellationToken = default) =>
        FFmpegKit.ExecuteAsync(command, cancellationToken);

    /// <inheritdoc/>
    public Task<FFmpegSessionResult> ExecuteAsync(
        string command,
        IProgress<FFmpegProgress> progress,
        TimeSpan? totalDuration = null,
        CancellationToken cancellationToken = default) =>
        FFmpegKit.ExecuteAsync(command, progress, totalDuration, cancellationToken);

    /// <inheritdoc/>
    public Task<FFmpegSessionResult> ExecuteWithArgumentsAsync(
        string[] arguments,
        CancellationToken cancellationToken = default) =>
        FFmpegKit.ExecuteWithArgumentsAsync(arguments, cancellationToken);

    /// <inheritdoc/>
    public Task<MediaInfo?> GetMediaInformationAsync(string path) =>
        FFprobeKit.GetMediaInformationAsync(path);

    /// <inheritdoc/>
    public FFmpegLogLevel LogLevel
    {
        get => FFmpegKitConfig.GetLogLevel();
        set => FFmpegKitConfig.SetLogLevel(value);
    }

    /// <inheritdoc/>
    public void EnableLogCallback(Action<string>? logCallback) =>
        FFmpegKitConfig.EnableLogCallback(logCallback);

    /// <inheritdoc/>
    public void ClearSessions() =>
        FFmpegKitConfig.ClearSessions();
}
