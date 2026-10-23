# Performance

The scene uses one custom drawing control, shared downsampled textures, offscreen culling and
time-based movement. It does not create controls or change layout for individual objects each frame.
The propeller rotates continuously while the disk uses cosine easing. C64 light states are prepared once.
The retained decoded texture pixel data is tested to stay below 2 MiB; this is not total process or GPU memory.

## Local comparison (2026-10-23)

Windows, .NET 10.0.12, headless Skia software rendering at 1920×1080, C64 and Amiga enabled,
random seed 42. Each case warms up for 20 frames and measures 240 frames at simulated 60 Hz.
The baseline is the previous per-object control renderer, measured before replacement in this local workspace.

| Objects | Mean before → after | p95 before → after | Managed allocations/frame before → after |
| --- | --- | --- | --- |
| 35 | 5.04 → 3.13 ms | 7.34 → 5.68 ms | 10,098 → 1,240 bytes |
| 200 | 13.36 → 7.03 ms | 19.27 → 10.22 ms | 48,932 → 1,445 bytes |

This workload measures CPU drawing and managed allocations, including scene advancement and layout checks.
It does not measure GPU presentation, vsync, monitor refresh consistency or battery use. Machine load affects
timings; these results do not guarantee a frame rate on another computer. Raw local results are in the ignored
`artifacts/performance/before.json` and `after.json` files.

Run the current workload from the repository root:

```sh
dotnet run --project tools/PerformanceProbe -c Release -- artifacts/performance/latest.json 240
```

For display testing, compare 35 and 200 objects at 1080p and 4K, with the optional computers enabled,
on weak hardware and multiple monitors. Press F to show callback FPS, rolling p95 and maximum frame
intervals. Average FPS alone can hide stalls. Check CPU and memory separately and compare native Wayland,
X11 and macOS on their actual desktops. Headless tests do not validate those display backends.

## README and release screenshot

`docs/images/screensaver.png` is a 1600×900 capture of the application's scene, with 40 objects,
both optional computers enabled, no FPS overlay and random seed 42. It is rendered by the actual
application through Avalonia Headless/Skia, without desktop chrome or an image-editor composition.
Recreate it after changing artwork:

```sh
dotnet run --project tools/PerformanceProbe -c Release -- --screenshot docs/images/screensaver.png
```

The Windows build script includes the same image in its output for release use.
