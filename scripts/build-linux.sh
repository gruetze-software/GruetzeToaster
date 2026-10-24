#!/usr/bin/env bash
set -euo pipefail
ARCH="${1:-x64}"
case "$ARCH" in x64) DEB_ARCH=amd64 ;; arm64) DEB_ARCH=arm64 ;; *) echo "Usage: $0 [x64|arm64]" >&2; exit 2 ;; esac
ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT="$ROOT/GruetzeToaster/GruetzeToaster.csproj"
VERSION="$(dotnet msbuild "$PROJECT" -nologo -getProperty:Version)"
[[ "$VERSION" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]] || { echo "Invalid version" >&2; exit 1; }
mkdir -p "$ROOT/artifacts"
STAGE="$(mktemp -d "$ROOT/artifacts/stage-linux.XXXXXX")"
trap 'rm -rf -- "$STAGE"' EXIT
PUBLISH="$STAGE/publish"
# Keep native libraries beside the executable: no writable/executable extraction cache needed.
dotnet publish "$PROJECT" -c Release -r "linux-$ARCH" --self-contained true \
  -p:PublishSingleFile=false -p:DebugType=None -o "$PUBLISH"
cp "$ROOT/README.md" "$ROOT/LICENSE" "$ROOT/THIRD-PARTY-NOTICES.md" "$PUBLISH/"
mkdir -p "$PUBLISH/docs"
cp "$ROOT/docs/ARTWORK-RIGHTS.md" "$PUBLISH/docs/"
cp "$ROOT/docs/artwork-prompts.json" "$PUBLISH/docs/"
cp "$ROOT/docs/PERFORMANCE.md" "$PUBLISH/docs/"
tar -czf "$ROOT/artifacts/GruetzeToaster-$VERSION-linux-$ARCH.tar.gz" -C "$PUBLISH" .
PKG="$STAGE/package"
mkdir -p "$PKG/usr/lib/gruetzetoaster" "$PKG/usr/bin" "$PKG/DEBIAN"
cp -R "$PUBLISH/." "$PKG/usr/lib/gruetzetoaster/"
printf '#!/bin/sh\nexec /usr/lib/gruetzetoaster/gruetzetoaster "$@"\n' > "$PKG/usr/bin/gruetzetoaster"
chmod 755 "$PKG/usr/bin/gruetzetoaster" "$PKG/usr/lib/gruetzetoaster/gruetzetoaster"
install -Dm644 "$ROOT/packaging/de.gruetze_software.GruetzeToaster.desktop" "$PKG/usr/share/applications/de.gruetze_software.GruetzeToaster.desktop"
# The project owner's logo computer is the app icon.
install -Dm644 "$ROOT/GruetzeToaster/Assets/app-icon.png" "$PKG/usr/share/icons/hicolor/256x256/apps/de.gruetze_software.GruetzeToaster.png"
install -Dm644 "$ROOT/packaging/de.gruetze_software.GruetzeToaster.metainfo.xml" "$PKG/usr/share/metainfo/de.gruetze_software.GruetzeToaster.metainfo.xml"
cat > "$PKG/DEBIAN/control" <<CONTROL
Package: gruetzetoaster
Version: $VERSION
Section: games
Priority: optional
Architecture: $DEB_ARCH
Maintainer: Christoph Schaefer <schaefer.christoph@posteo.de>
Depends: libc6, libgcc-s1, libstdc++6, zlib1g, libfontconfig1, libice6, libsm6, libx11-6, libwayland-client0, libwayland-cursor0, libwayland-egl1, libxkbcommon0, libegl1, libgles2, libicu74 | libicu72 | libicu76 | libicu78, libssl3t64 | libssl3
Homepage: https://github.com/gruetze-software/GruetzeToaster
Description: Retro flying toaster animation
 Configurable fullscreen or windowed animation. Does not lock the session.
CONTROL
dpkg-deb --root-owner-group --build "$PKG" "$ROOT/artifacts/GruetzeToaster-$VERSION-linux-$ARCH.deb"
