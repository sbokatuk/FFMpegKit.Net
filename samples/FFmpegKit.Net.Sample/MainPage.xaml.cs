using CommunityToolkit.Maui.Views;
using Ffmpegkit.Net;

namespace FFmpegKit.Net.Sample;

public partial class MainPage : ContentPage
{
	const string SampleAssetName = "sample.mp4";

	sealed record ConversionOption(string Name, string OutputFileName, Func<string, string, string> BuildCommand);

	static readonly ConversionOption[] ConversionOptions =
	[
		new("Resize to 160x120", "converted_resize.mp4",
			(input, output) => $"-y -i \"{input}\" -vf scale=160:120 -c:v mpeg4 -c:a aac \"{output}\""),
		new("Grayscale", "converted_grayscale.mp4",
			(input, output) => $"-y -i \"{input}\" -vf hue=s=0 -c:v mpeg4 -c:a aac \"{output}\""),
		new("Extract audio only (AAC)", "converted_audio.m4a",
			(input, output) => $"-y -i \"{input}\" -vn -c:a aac \"{output}\""),
	];

	string? _inputPath;
	TimeSpan? _sourceDuration;

	public MainPage()
	{
		InitializeComponent();
		ConversionPicker.ItemsSource = ConversionOptions.Select(o => o.Name).ToList();
		ConversionPicker.SelectedIndex = 0;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		if (_inputPath is not null)
			return;

		var inputPath = Path.Combine(FileSystem.CacheDirectory, SampleAssetName);

		using (var assetStream = await FileSystem.OpenAppPackageFileAsync(SampleAssetName))
		using (var fileStream = File.Create(inputPath))
		{
			await assetStream.CopyToAsync(fileStream);
		}

		_inputPath = inputPath;
		BeforePlayer.Source = MediaSource.FromFile(inputPath);

		// Ffmpegkit.Net.FFprobeKit hands back the parsed MediaInfo directly - no per-platform
		// session wrapper, and DurationOrNull/PixelWidth/... are already invariantly parsed on
		// both Android and iOS, so there is nothing platform-specific left to branch on here.
		var info = await FFprobeKit.GetMediaInformationAsync(inputPath);
		_sourceDuration = info?.Duration;

		var video = info?.Streams.FirstOrDefault(s => s.IsVideo);
		StatusLabel.Text = video is null
			? "Tap the button to run an FFmpeg conversion."
			: $"Source: {video.PixelWidth}x{video.PixelHeight}, {_sourceDuration?.TotalSeconds:0.##}s, {video.Codec}.";
	}

	async void OnConvertClicked(object sender, EventArgs e)
	{
		if (_inputPath is null || ConversionPicker.SelectedIndex < 0)
			return;

		var option = ConversionOptions[ConversionPicker.SelectedIndex];
		var inputPath = _inputPath;

		ConvertBtn.IsEnabled = false;
		Spinner.IsVisible = true;
		Spinner.IsRunning = true;
		ConversionProgress.Progress = 0;
		ConversionProgress.IsVisible = true;
		ProgressLabel.IsVisible = true;
		ProgressLabel.Text = "Starting...";
		StatusLabel.Text = "Converting...";

		AfterPlayer.Stop();
		AfterPlayer.Source = null;

		try
		{
			// Progress<T> marshals back to the thread that created it - the UI thread here -
			// so the handler can touch controls directly, on either platform.
			var progress = new Progress<FFmpegProgress>(p =>
			{
				ConversionProgress.Progress = p.Percent ?? 0;
				ProgressLabel.Text = p.Percent is { } percent
					? $"{percent:P0} · {p.Position:mm\\:ss} · {p.Speed:0.#}x"
					: $"{p.Position:mm\\:ss} · {p.Speed:0.#}x";
			});

			var (success, message, outputPath) = await RunConversionAsync(option, inputPath, progress, _sourceDuration);
			StatusLabel.Text = message;
			SemanticScreenReader.Announce(message);

			if (success)
				AfterPlayer.Source = MediaSource.FromFile(outputPath);
		}
		catch (Exception ex)
		{
			StatusLabel.Text = $"Unexpected error: {ex.Message}";
		}
		finally
		{
			Spinner.IsRunning = false;
			Spinner.IsVisible = false;
			ConversionProgress.IsVisible = false;
			ProgressLabel.IsVisible = false;
			ConvertBtn.IsEnabled = true;
		}
	}

	static async Task<(bool Success, string Message, string OutputPath)> RunConversionAsync(
		ConversionOption option,
		string inputPath,
		IProgress<FFmpegProgress> progress,
		TimeSpan? sourceDuration)
	{
		var outputPath = Path.Combine(FileSystem.CacheDirectory, option.OutputFileName);

		if (File.Exists(outputPath))
			File.Delete(outputPath);

		var command = option.BuildCommand(inputPath, outputPath);

		// Qualified, not just `using Ffmpegkit.Net`: this app's own namespace is
		// FFmpegKit.Net.Sample, and "FFmpegKit" as a bare identifier resolves to that implicit
		// outer namespace rather than the imported class - the exact ambiguity the bindings'
		// own Ffmpegkit.Droid/Ffmpegkit.Ios casing sidesteps for themselves, but which resurfaces
		// here because this app's root namespace also starts with a dotted "FFmpegKit" segment.
		//
		// One call, one code path for both platforms - no Task.Run, since ExecuteAsync hands the
		// work to FFmpegKit's own executor rather than blocking a thread pool thread for the
		// length of the transcode. The duration is what lets FFmpegKit report a percentage
		// rather than just a position.
		var result = await Ffmpegkit.Net.FFmpegKit.ExecuteAsync(command, progress, sourceDuration);

		if (result.Succeeded)
		{
			var outputSize = new FileInfo(outputPath).Length;
			return (true, $"Success! Converted video written to:\n{outputPath}\n({outputSize:N0} bytes)", outputPath);
		}

		var outcome = result.Cancelled ? "cancelled" : $"failed (return code {result.ReturnCode})";
		return (false, $"Conversion {outcome}.", outputPath);
	}
}
