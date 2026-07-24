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
			.UseMauiCommunityToolkitMediaElement()
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
