using Foundation;
using UIKit;

namespace FFmpegKit.Net.DeviceTests;

/// <summary>
/// Host for the on-simulator smoke tests. Runs every shared check on launch, reports the outcome
/// to stdout - which `simctl launch --console-pty` streams straight back to CI - and then exits
/// with a verdict line the runner script greps for.
/// </summary>
public static class Program
{
    private static void Main(string[] args) => UIApplication.Main(args, null, typeof(AppDelegate));
}

[Register(nameof(AppDelegate))]
public sealed class AppDelegate : UIApplicationDelegate
{
    public override UIWindow? Window { get; set; }

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        // A window is not strictly needed for a headless run, but iOS terminates an app that
        // never presents one, which would look like a crash rather than a test failure.
        var root = new UIViewController();
        root.View!.BackgroundColor = UIColor.SystemBackground;

        Window = new UIWindow(UIScreen.MainScreen.Bounds) { RootViewController = root };
        Window.MakeKeyAndVisible();

        // Off the UI thread: the checks run real FFmpeg commands and would otherwise trip the
        // watchdog before any of them finished.
        Task.Run(RunAndReport);

        return true;
    }

    private static async Task RunAndReport()
    {
        var workingDirectory = Path.Combine(Path.GetTempPath(), "ffmpegkit-net-e2e");
        Directory.CreateDirectory(workingDirectory);

        SmokeTests.Reporter = message => Console.WriteLine($"    {message}");

        var failures = 0;

        foreach (var test in SmokeTests.All)
        {
            try
            {
                await test.Execute(workingDirectory);
                Console.WriteLine($"PASS {test.Name}");
            }
            catch (Exception exception)
            {
                failures++;
                Console.WriteLine($"FAIL {test.Name}: {exception.Message}");
            }
        }

        Console.WriteLine(failures == 0
            ? "FFMPEGKIT_E2E_DONE PASS"
            : $"FFMPEGKIT_E2E_DONE FAIL ({failures} failed)");
        Console.Out.Flush();

        // Terminate so the runner's `simctl launch --console-pty` returns instead of hanging
        // until its timeout. Exiting from an iOS app is otherwise not something to imitate.
        Environment.Exit(failures == 0 ? 0 : 1);
    }
}
