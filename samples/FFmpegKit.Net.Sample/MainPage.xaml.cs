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

	readonly IFFmpegKit _ffmpeg;

	string? _inputPath;
	TimeSpan? _sourceDuration;
	CancellationTokenSource? _cancellation;

	// IFFmpegKit is registered by UseFFmpegKit() in MauiProgram.cs and injected here by Shell
	// through the service provider (the page itself is AddTransient'd there). Depending on the
	// interface keeps the page unit-testable with a fake, and sidesteps a naming trap the statics
	// have in this particular app: its root namespace starts with a dotted "FFmpegKit" segment,
	// so a bare FFmpegKit.ExecuteAsync(...) would resolve the namespace rather than the class and
	// need qualifying as Ffmpegkit.Net.FFmpegKit.ExecuteAsync(...). The statics remain fully
	// supported for apps without a container - see the README's namespace note.
	public MainPage(IFFmpegKit ffmpeg)
	{
		_ffmpeg = ffmpeg;
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

		// The probe hands back the parsed MediaInfo directly - no per-platform session wrapper,
		// and DurationOrNull/PixelWidth/... are already invariantly parsed on every platform, so
		// there is nothing platform-specific left to branch on here.
		var info = await _ffmpeg.GetMediaInformationAsync(inputPath);
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
		CancelBtn.IsEnabled = true;
		Spinner.IsVisible = true;
		Spinner.IsRunning = true;
		ConversionProgress.Progress = 0;
		ConversionProgress.IsVisible = true;
		ProgressLabel.IsVisible = true;
		ProgressLabel.Text = "Starting...";
		StatusLabel.Text = "Converting...";

		AfterPlayer.Stop();
		AfterPlayer.Source = null;

		_cancellation = new CancellationTokenSource();

		try
		{
			// Progress<T> marshals back to the thread that created it - the UI thread here -
			// so the handler can touch controls directly, on every platform.
			var progress = new Progress<FFmpegProgress>(p =>
			{
				ConversionProgress.Progress = p.Percent ?? 0;
				ProgressLabel.Text = p.Percent is { } percent
					? $"{percent:P0} · {p.Position:mm\\:ss} · {p.Speed:0.#}x"
					: $"{p.Position:mm\\:ss} · {p.Speed:0.#}x";
			});

			var (success, message, outputPath) =
				await RunConversionAsync(option, inputPath, progress, _sourceDuration, _cancellation.Token);
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
			_cancellation.Dispose();
			_cancellation = null;
			Spinner.IsRunning = false;
			Spinner.IsVisible = false;
			ConversionProgress.IsVisible = false;
			ProgressLabel.IsVisible = false;
			CancelBtn.IsEnabled = false;
			ConvertBtn.IsEnabled = true;
		}
	}

	void OnCancelClicked(object sender, EventArgs e)
	{
		// Cancellation is co-operative: FFmpeg stops as soon as it notices, and the awaited call
		// then completes with Cancelled=true rather than throwing.
		_cancellation?.Cancel();
		StatusLabel.Text = "Cancelling...";
	}

	async Task<(bool Success, string Message, string OutputPath)> RunConversionAsync(
		ConversionOption option,
		string inputPath,
		IProgress<FFmpegProgress> progress,
		TimeSpan? sourceDuration,
		CancellationToken cancellationToken)
	{
		var outputPath = Path.Combine(FileSystem.CacheDirectory, option.OutputFileName);

		if (File.Exists(outputPath))
			File.Delete(outputPath);

		var command = option.BuildCommand(inputPath, outputPath);

		// One call, one code path for every platform - no Task.Run, since ExecuteAsync hands the
		// work to FFmpegKit's own executor rather than blocking a thread pool thread for the
		// length of the transcode. The duration is what lets FFmpegKit report a percentage
		// rather than just a position. Prefer exceptions over flags? Chain .EnsureSuccess() onto
		// the await instead of checking Succeeded below.
		var result = await _ffmpeg.ExecuteAsync(command, progress, sourceDuration, cancellationToken);

		if (result.Succeeded)
		{
			var outputSize = new FileInfo(outputPath).Length;
			return (true,
				$"Success in {result.Duration?.TotalSeconds:0.#}s! Converted video written to:\n{outputPath}\n({outputSize:N0} bytes)",
				outputPath);
		}

		if (result.Cancelled)
			return (false, "Conversion cancelled.", outputPath);

		// The reason lives on the result: Output is the session's console transcript, and its
		// last line is FFmpeg's own error message.
		return (false,
			$"Conversion failed (return code {result.ReturnCode}).\n{LastLine(result.Output)}",
			outputPath);
	}

	static string LastLine(string? output) =>
		output?.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) is { Length: > 0 } lines
			? lines[^1]
			: "(no output captured)";
}
