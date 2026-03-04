## 🚀 GruetzeToaster Screensaver
A high-performance, retro-themed screensaver inspired by the legendary After Dark "Flying Toasters" of the 1990s. Built with Avalonia UI and .NET 9, this project is a modern tribute to the most iconic screensaver of the 16-bit era.

<img width="1403" height="948" alt="image" src="https://github.com/user-attachments/assets/54e4912e-eaa2-49c2-9e96-533b113f2b42" />

## ✨ Features

- **90s Nostalgia:** Authentic pixel art style with "nearest neighbor" rendering to keep every pixel sharp.
- **Parallax Depth Effect:** Multi-layered animation with varying speeds and opacities to create a sense of vast, flying space.
- **Customization:** Adjust toaster count (from a lone scout to a chaotic breakfast swarm) and animation speed.

## 🖥️ Platform Support

### Windows ✅ Full Screensaver Integration
Fully compatible with the Windows Screensaver protocol (`.scr`):
- **Mini-Preview** inside the Windows display settings monitor icon
- **Settings Dialog** — native configuration window
- **Fullscreen Mode** — smooth execution across all monitors

### Linux ✅ Standalone App
Runs as a fullscreen app. No XScreenSaver integration — start it manually from the terminal:
```bash
./GruetzeToaster
```
A `.deb` package is provided for Debian/Ubuntu-based distributions.

### macOS ⚠️ Standalone App (no system screensaver)
Runs as a fullscreen `.app` — **not** a native macOS screensaver. Apple's screensaver API requires an Objective-C bridge that is incompatible with pure .NET applications.

Double-click `GruetzeToaster.app` to launch it in fullscreen. Press `ESC` or move the mouse to exit.

> Builds are provided for both Intel (`x64`) and Apple Silicon (`arm64`) Macs.

