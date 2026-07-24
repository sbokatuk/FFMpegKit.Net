using AppKit;
using Foundation;

namespace FFmpegKit.Net.DeviceTests;

/// <summary>
/// Host for the smoke tests on macOS. The target platform is the machine itself, so the runner
/// script launches the app bundle's executable from a terminal, reads stdout and greps for the
/// verdict line - no simulator or device involved.
/// </summary>
public static class Program
{
    private static void Main(string[] args)
    {
        NSApplication.Init();
        NSApplication.SharedApplication.Delegate = new AppDelegate();
        NSApplication.SharedApplication.Run();
    }
}

public sealed class AppDelegate : NSApplicationDelegate
{
    public override void DidFinishLaunching(NSNotification notification)
    {
        // Off the main thread: FFmpegKit dispatches some callbacks to the main queue, and the
        // checks block waiting on sessions - running them on the main thread would deadlock.
        Task.Run(RunAndReport);
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

        // Terminate so the runner script returns instead of waiting on a GUI app that never
        // shows a window.
        Environment.Exit(failures == 0 ? 0 : 1);
    }
}
