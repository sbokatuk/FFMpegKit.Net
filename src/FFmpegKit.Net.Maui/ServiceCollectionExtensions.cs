using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ffmpegkit.Net.Maui;

/// <summary>Registers FFmpegKit with a Microsoft.Extensions service collection.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IFFmpegKit"/> as a singleton <see cref="FFmpegKitClient"/>.
    /// </summary>
    /// <remarks>
    /// Idempotent: an existing <see cref="IFFmpegKit"/> registration - including a test fake -
    /// is left in place. The static <c>FFmpegKit</c>/<c>FFprobeKit</c> classes keep working
    /// regardless; this only adds the injectable surface.
    /// </remarks>
    public static IServiceCollection AddFFmpegKit(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IFFmpegKit, FFmpegKitClient>();
        return services;
    }
}
