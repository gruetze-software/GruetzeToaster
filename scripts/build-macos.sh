#!/usr/bin/env bash
set -euo pipefail
ARCH="${1:-arm64}"
case "$ARCH" in x64|arm64) ;; *) echo "Usage: $0 [x64|arm64]" >&2; exit 2 ;; esac
[[ "$(uname -s)" == Darwin ]] || { echo "Requires macOS for codesign and ditto." >&2; exit 1; }
ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="$ROOT/GruetzeToaster/GruetzeToaster.csproj"
VERSION="$(dotnet msbuild "$PROJECT" -nologo -getProperty:Version)"
[[ "$VERSION" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]] || exit 1
mkdir -p "$ROOT/artifacts"
STAGE="$(mktemp -d "$ROOT/artifacts/stage-macos.XXXXXX")"
trap 'rm -rf -- "$STAGE"' EXIT
APP="$STAGE/GruetzeToaster.app"
mkdir -p "$APP/Contents/MacOS" "$APP/Contents/Resources"
dotnet publish "$PROJECT" -c Release -r "osx-$ARCH" --self-contained true \
  -p:PublishSingleFile=false -p:DebugType=None -o "$APP/Contents/MacOS"
cp "$ROOT/GruetzeToaster/Assets/app-icon.icns" "$APP/Contents/Resources/"
cp "$ROOT/README.md" "$ROOT/LICENSE" "$ROOT/THIRD-PARTY-NOTICES.md" "$APP/Contents/Resources/"
mkdir -p "$APP/Contents/Resources/docs"
cp "$ROOT/docs/ARTWORK-RIGHTS.md" "$APP/Contents/Resources/docs/"
cp "$ROOT/docs/artwork-prompts.json" "$APP/Contents/Resources/docs/"
cp "$ROOT/docs/PERFORMANCE.md" "$APP/Contents/Resources/docs/"
mkdir -p "$APP/Contents/Resources/docs/images" "$APP/Contents/Resources/GruetzeToaster/Assets"
cp "$ROOT/docs/images/screensaver.png" "$APP/Contents/Resources/docs/images/"
cp "$ROOT/GruetzeToaster/Assets/app-icon.png" "$APP/Contents/Resources/GruetzeToaster/Assets/"
cat > "$APP/Contents/Info.plist" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0"><dict>
<key>CFBundleName</key><string>GruetzeToaster</string>
<key>CFBundleDisplayName</key><string>GruetzeToaster</string>
<key>CFBundleExecutable</key><string>gruetzetoaster</string>
<key>CFBundleIdentifier</key><string>de.gruetze-software.GruetzeToaster</string>
<key>CFBundlePackageType</key><string>APPL</string>
<key>CFBundleShortVersionString</key><string>$VERSION</string>
<key>CFBundleVersion</key><string>$VERSION</string>
<key>CFBundleIconFile</key><string>app-icon.icns</string>
<key>NSHighResolutionCapable</key><true/>
<key>LSMinimumSystemVersion</key><string>14.0</string>
<key>NSPrincipalClass</key><string>NSApplication</string>
<key>NSHumanReadableCopyright</key><string>© 2026 Grütze-Software</string>
</dict></plist>
PLIST
chmod 755 "$APP/Contents/MacOS/gruetzetoaster"
find "$APP/Contents/MacOS" -type f -name '*.dylib' -exec codesign --force --sign - {} \;
codesign --force --deep --sign - "$APP"
codesign --verify --deep --strict "$APP"
plutil -lint "$APP/Contents/Info.plist"
if [[ "$(uname -m)" == "$ARCH" || ( "$(uname -m)" == x86_64 && "$ARCH" == x64 ) ]]; then
  "$APP/Contents/MacOS/gruetzetoaster" --help
fi
ditto -c -k --sequesterRsrc --keepParent "$APP" "$ROOT/artifacts/GruetzeToaster-$VERSION-macos-$ARCH.zip"
