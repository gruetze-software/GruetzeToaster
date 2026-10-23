#!/usr/bin/env bash
set -euo pipefail
KIND="${1:-}"
ARCH="${2:-x64}"
case "$KIND" in flatpak|snap) ;; *) echo "Usage: $0 [flatpak|snap] [x64|arm64]" >&2; exit 2 ;; esac
case "$ARCH" in x64|arm64) ;; *) exit 2 ;; esac
ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
PAYLOAD="$ROOT/artifacts/$KIND-payload"
# Use a fresh payload to prevent obsolete native libraries from entering a package.
[[ ! -e "$PAYLOAD" ]] || { echo "Payload already exists: $PAYLOAD. Move it aside before rebuilding." >&2; exit 1; }
if [[ "$KIND" == flatpak ]]; then PREFIX="$PAYLOAD"; else PREFIX="$PAYLOAD/usr"; fi
dotnet publish "$ROOT/GruetzeToaster/GruetzeToaster.csproj" -c Release -r "linux-$ARCH" \
  --self-contained true -p:PublishSingleFile=false -p:DebugType=None -o "$PREFIX/lib/gruetzetoaster"
mkdir -p "$PREFIX/bin"
cat > "$PREFIX/bin/gruetzetoaster" <<'LAUNCH'
#!/bin/sh
APP_ROOT="$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)"
exec "$APP_ROOT/lib/gruetzetoaster/gruetzetoaster" "$@"
LAUNCH
chmod 755 "$PREFIX/bin/gruetzetoaster"
install -Dm644 "$ROOT/packaging/de.gruetze_software.GruetzeToaster.desktop" "$PREFIX/share/applications/de.gruetze_software.GruetzeToaster.desktop"
install -Dm644 "$ROOT/packaging/de.gruetze_software.GruetzeToaster.metainfo.xml" "$PREFIX/share/metainfo/de.gruetze_software.GruetzeToaster.metainfo.xml"
install -Dm644 "$ROOT/GruetzeToaster/Assets/app-icon.png" "$PREFIX/share/icons/hicolor/256x256/apps/de.gruetze_software.GruetzeToaster.png"
cp "$ROOT/LICENSE" "$ROOT/THIRD-PARTY-NOTICES.md" "$PREFIX/lib/gruetzetoaster/"
mkdir -p "$PREFIX/lib/gruetzetoaster/docs"
cp "$ROOT/docs/ARTWORK-RIGHTS.md" "$PREFIX/lib/gruetzetoaster/docs/"
cp "$ROOT/docs/artwork-prompts.json" "$PREFIX/lib/gruetzetoaster/docs/"
echo "Prepared local payload: $PAYLOAD"
