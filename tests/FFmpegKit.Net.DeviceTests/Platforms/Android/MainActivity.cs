using Android.App;
using Android.OS;
using Android.Widget;
using AndroidLog = Android.Util.Log;

namespace FFmpegKit.Net.DeviceTests;

/// <summary>
/// Runs the shared smoke checks on launch and reports the outcome to logcat under a fixed tag.
/// CI starts the activity with `adb shell am start`, tails logcat for <see cref="DoneMarker"/>
/// and fails the job on FAIL or on timeout.
/// </summary>
/// <remarks>
/// Name is pinned so CI can launch it as
/// <c>com.sbokatuk.ffmpegkit.net.devicetests/.MainActivity</c>. Without it the manifest gets a
/// generated <c>crc64…</c> class name that changes between builds.
/// </remarks>
[Activity(
    Name = "com.sbokatuk.ffmpegkit.net.devicetests.MainActivity",
    Label = "FFmpegKit.Net Device Tests",
    MainLauncher = true,
    Exported = true)]
public class MainActivity : Activity
{
    public const string Tag = "FFmpegKitNetE2E";
    public const string DoneMarker = "FFMPEGKIT_E2E_DONE";

    private TextView? _output;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        _output = new TextView(this) { Text = "Running FFmpegKit.Net smoke tests…" };
        SetContentView(_output);

        // The checks run real FFmpeg commands, so keep them off the UI thread.
        _ = Task.Run(RunSmokeTests);
    }

    private async Task RunSmokeTests()
    {
        var workingDirectory = CacheDir?.AbsolutePath ?? Path.GetTempPath();
        var passed = 0;
        var failed = 0;

        AndroidLog.Info(Tag, $"starting smoke tests in {workingDirectory}");

        foreach (var test in SmokeTests.All)
        {
            SmokeTests.Reporter = detail => AndroidLog.Info(Tag, $"  {test.Name}: {detail}");

            try
            {
                await test.Execute(workingDirectory);
                passed++;
                AndroidLog.Info(Tag, $"PASS {test.Name}");
            }
            catch (Exception ex)
            {
                failed++;
                AndroidLog.Error(Tag, $"FAIL {test.Name}: {ex}");
            }
            finally
            {
                SmokeTests.Reporter = null;
            }
        }

        var verdict = failed == 0 ? "PASS" : "FAIL";
        AndroidLog.Info(Tag, $"{DoneMarker} {verdict} passed={passed} failed={failed}");

        RunOnUiThread(() => _output!.Text = $"{verdict}: {passed} passed, {failed} failed");
    }
}
