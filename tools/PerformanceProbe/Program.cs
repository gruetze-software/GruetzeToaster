using System.Diagnostics;
using System.Text.Json;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Media.Imaging;
using GruetzeToaster;

// A repeatable CPU/software-rendering workload, not a GPU/vsync or display-smoothness test.
AppBuilder.Configure<App>().UseSkia().UseHarfBuzz().WithInterFont()
    .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false }).SetupWithoutStarting();
if (args.Length > 0 && args[0] == "--screenshot")
{
    string screenshotPath = args.Length > 1 ? args[1] : "docs/images/screensaver.png";
    var window = new MainWindow(false, IntPtr.Zero, true,
        new() { ToasterCount = 40, ShowC64 = true, ShowAmiga = true }, new Random(42));
    window.Width = 1600;
    window.Height = 900;
    window.Show();
    window.UpdateLayout();
    window.AdvanceScene(0.25);
    window.UpdateLayout();
    using var target = new RenderTargetBitmap(new PixelSize(1600, 900), new Vector(96, 96));
    target.Render(window);
    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(screenshotPath))!);
    target.Save(screenshotPath, PngBitmapEncoderOptions.Default);
    window.Close();
    Console.WriteLine($"Application scene saved to {screenshotPath}");
    return;
}
string output = args.Length > 0 ? args[0] : "artifacts/performance/latest.json";
int iterations = args.Length > 1 ? int.Parse(args[1]) : 120;
var results = new List<object>();
foreach (int count in new[] { 35, 200 })
{
    var window = new MainWindow(false, IntPtr.Zero, true,
        new() { ToasterCount = count, ShowC64 = true, ShowAmiga = true }, new Random(42));
    window.Width = 1920;
    window.Height = 1080;
    window.Show();
    window.UpdateLayout();
    using var target = new RenderTargetBitmap(new PixelSize(1920, 1080), new Vector(96, 96));
    for (int i = 0; i < 20; i++)
    {
        window.AdvanceScene(1.0 / 60);
        window.UpdateLayout();
        target.Render(window);
    }
    var samples = new double[iterations];
    long allocated = GC.GetTotalAllocatedBytes(true);
    for (int i = 0; i < iterations; i++)
    {
        long start = Stopwatch.GetTimestamp();
        window.AdvanceScene(1.0 / 60);
        window.UpdateLayout();
        target.Render(window);
        samples[i] = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
    }
    allocated = GC.GetTotalAllocatedBytes(true) - allocated;
    Array.Sort(samples);
    var result = new
    {
        objects = count, width = 1920, height = 1080, iterations,
        medianMs = samples[samples.Length / 2],
        p95Ms = samples[(int)((samples.Length - 1) * .95)],
        maxMs = samples[^1],
        meanMs = samples.Average(),
        allocatedBytesPerFrame = allocated / iterations
    };
    results.Add(result);
    Console.WriteLine(JsonSerializer.Serialize(result));
    window.Close();
}
Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);
File.WriteAllText(output, JsonSerializer.Serialize(new
{
    environment = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
    workload = "Headless Skia CPU rendering; 1920x1080; C64 and Amiga enabled; random seed 42",
    results
}, new JsonSerializerOptions { WriteIndented = true }));
