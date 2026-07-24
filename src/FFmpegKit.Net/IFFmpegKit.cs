namespace Ffmpegkit.Net;

/// <summary>
/// The cross-platform FFmpegKit surface as an injectable service: everything the static
/// <see cref="FFmpegKit"/>, <see cref="FFprobeKit"/> and <see cref="FFmpegKitConfig"/> classes
/// offer, behind one interface that app code can take as a constructor dependency and tests can
/// fake.
/// </summary>
/// <remarks>
/// The static classes remain the API for code that has no container - this interface adds
/// nothing over them except injectability. Register the default implementation with
/// <c>builder.UseFFmpegKit()</c> / <c>services.AddFFmpegKit()</c> from the
/// <c>FFmpegKit.Net.*.Maui</c> package, or in any Microsoft.Extensions container with
/// <c>services.AddSingleton&lt;IFFmpegKit, FFmpegKitClient&gt;()</c>.
/// <para>
/// FFmpegKit itself is process-global native state: the log level, the log callback and the
/// session history are shared by every instance of this interface and by the static API alike.
/// </para>
/// </remarks>
public interface IFFmpegKit
{
    /// <inheritdoc cref="FFmpegKit.ExecuteAsync(string, CancellationToken)"/>
    Task<FFmpegSessionResult> ExecuteAsync(string command, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="FFmpegKit.ExecuteAsync(string, IProgress{FFmpegProgress}, TimeSpan?, CancellationToken)"/>
    Task<FFmpegSessionResult> ExecuteAsync(
        string command,
        IProgress<FFmpegProgress> progress,
        TimeSpan? totalDuration = null,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="FFmpegKit.ExecuteWithArgumentsAsync(string[], CancellationToken)"/>
    Task<FFmpegSessionResult> ExecuteWithArgumentsAsync(
        string[] arguments,
        CancellationToken cancellationToken = default);

    /// <inheritdoc cref="FFprobeKit.GetMediaInformationAsync(string)"/>
    Task<MediaInfo?> GetMediaInformationAsync(string path);

    /// <summary>The FFmpeg log level - process-global, like all FFmpegKit configuration.</summary>
    /// <remarks>Wraps <see cref="FFmpegKitConfig.GetLogLevel"/> / <see cref="FFmpegKitConfig.SetLogLevel"/>.</remarks>
    FFmpegLogLevel LogLevel { get; set; }

    /// <inheritdoc cref="FFmpegKitConfig.EnableLogCallback(Action{string}?)"/>
    void EnableLogCallback(Action<string>? logCallback);

    /// <inheritdoc cref="FFmpegKitConfig.ClearSessions"/>
    void ClearSessions();
}
