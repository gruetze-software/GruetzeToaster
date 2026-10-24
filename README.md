# GruetzeToaster

<img src="https://raw.githubusercontent.com/gruetze-software/GruetzeToaster/v1.2.0/GruetzeToaster/Assets/app-icon.png" alt="Propeller toaster icon" width="96" height="96">

A retro screensaver by [Grütze-Software](https://www.gruetze-software.de), built with .NET 10 and Avalonia.
Turquoise propeller toasters, toast, floppy disks and vintage computers drift across your screen.

![GruetzeToaster](https://raw.githubusercontent.com/gruetze-software/GruetzeToaster/v1.2.0/docs/images/screensaver.png)

## Download

Get [version 1.2.0](https://github.com/gruetze-software/GruetzeToaster/releases/tag/v1.2.0).
All packages include the .NET runtime. Extract archives completely and keep their files together.

| System | Package |
| --- | --- |
| Windows x64 | `GruetzeToaster-1.2.0-windows-x64.zip` |
| Linux x64 / ARM64 | `GruetzeToaster-1.2.0-linux-x64` / `linux-arm64`, as `.deb` or `.tar.gz` |
| macOS Apple Silicon | `GruetzeToaster-1.2.0-macos-arm64.zip` |
| macOS Intel | `GruetzeToaster-1.2.0-macos-x64.zip` |

### Windows

Extract the ZIP to a permanent folder. Open `Settings.cmd`, `About.cmd`, `Windowed.cmd` or `Fullscreen.cmd`.
To register the screensaver, right-click `gruetzetoaster.scr` → **Install** (possibly under **Show more options**).
Keep the dependencies beside the SCR file. The app is not Authenticode-signed.

### macOS

The app bundle declares macOS 14 or later as its minimum. Select Apple Silicon or Intel according to
Apple menu → **About This Mac**. Extract the ZIP, drag **GruetzeToaster.app** into **Applications** and double-click it.

The bundles are ad-hoc signed, without Apple notarization. If a trusted download is blocked, attempt to open it,
then use **System Settings → Privacy & Security → Open Anyway**, following [Apple's instructions](https://support.apple.com/en-us/102445).

Close the running app before launching another mode from Terminal:

```sh
open -a /Applications/GruetzeToaster.app --args --settings
open -a /Applications/GruetzeToaster.app --args --about
open -a /Applications/GruetzeToaster.app --args --windowed
```

This is a standalone app, not a native `.saver` extension. It does not appear in macOS Screen Saver settings,
start automatically when idle, or lock the Mac.

### Linux

Install the DEB matching your architecture with `sudo apt install ./GruetzeToaster-1.2.0-linux-x64.deb`,
or extract the TAR.GZ and run `./gruetzetoaster --windowed`.
Use `gruetzetoaster --settings` after DEB installation. A glibc-based desktop and native graphics libraries are required.

Native Wayland is selected when `WAYLAND_DISPLAY` is set; use `--x11` to try XWayland instead.
Native Wayland currently uses one fullscreen window, with monitor selection controlled by the compositor.
Linux runs as a standalone app without automatic idle activation or locking. Snap/Flatpak store packages are not provided.

## Controls and settings

- With no arguments, the app starts fullscreen. Mouse movement, a click or a key exits; **F** toggles the performance display.
- `--windowed` runs in a normal window; **Escape** closes it. `--settings` and `--about` open the corresponding dialogs.
- Choose 1–200 objects per screen, speed and visibility of each of the six object types. All types are enabled by default.
- Optional small background stars are off by default. Disable every object type for background only.
- C64 and Amiga computers appear less often; the C64 light blinks and the Amiga slides a disk into its side drive.
- Saved choices are preserved. **Reset** enables all object types and turns stars off.

## Links and license

- [Grütze-Software website](https://www.gruetze-software.de)
- [Source code](https://github.com/gruetze-software/GruetzeToaster)
- [Report bugs or request features](https://github.com/gruetze-software/GruetzeToaster/issues)
- [Support development on Ko-fi](https://ko-fi.com/gruetzesoftware)

[MIT license](LICENSE) · [Third-party notices](THIRD-PARTY-NOTICES.md) · [Artwork provenance](docs/ARTWORK-RIGHTS.md)

To build from source, install the .NET 10 SDK and use `scripts/build-windows.ps1`,
`bash scripts/build-linux.sh [x64|arm64]`, or `bash scripts/build-macos.sh [arm64|x64]` on the respective platform.
