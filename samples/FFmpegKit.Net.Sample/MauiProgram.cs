using CommunityToolkit.Maui;
using Ffmpegkit.Net.Maui;
using Microsoft.Extensions.Logging;

namespace FFmpegKit.Net.Sample;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseFFmpegKit()
			.UseMauiCommunityToolkit()
#if NET10_0_OR_GREATER
			// MediaElement 10.0.0 made the Android foreground-service opt-in a required argument.
			// This sample previews a local file while in the foreground, and opting in would also
			// require FOREGROUND_SERVICE_MEDIA_PLAYBACK in the manifest.
			.UseMauiCommunityToolkitMediaElement(isAndroidForegroundServiceEnabled: false)
#else
			.UseMauiCommunityToolkitMediaElement()
#endif
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// UseFFmpegKit() above registered IFFmpegKit; registering the page lets Shell create it
		// through the service provider, so MainPage takes the interface as a constructor
		// dependency instead of calling the static classes.
		builder.Services.AddTransient<MainPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
