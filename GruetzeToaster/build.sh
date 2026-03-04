#!/bin/bash

# --- 1. Konfiguration ---
PROJ_FILE="./GruetzeToaster.csproj"
APP_NAME="GruetzeToaster"
DIST_DIR="dist"
RELEASE_DIR="$DIST_DIR/releases"

# Version dynamisch aus der csproj extrahieren
VERSION=$(grep -oPm1 "(?<=<Version>)[^<]+" $PROJ_FILE || echo "1.0.0")

echo "--- 🚀 Starting Build Process for $APP_NAME v$VERSION (WSL Mode) ---"

# Aufräumen und Ordner erstellen
rm -rf $DIST_DIR
mkdir -p $RELEASE_DIR

# --- 2. Hilfsfunktionen ---

pack_archive() {
    local target_zip="../../$1"
    if command -v zip >/dev/null 2>&1; then
        zip -q -r "$target_zip" .
    else
        echo "⚠️ ERROR: 'zip' not found! Install with: sudo apt install zip"
        return 1
    fi
}

build_target() {
    local rid=$1
    local platform_name=$2
    local output_path="$DIST_DIR/$platform_name"
    
    echo -e "\n📦 Building for $platform_name ($rid)..."
    
    # ReadyToRun=false verhindert den WSL/NTFS Pfad-Fehler
    dotnet publish "$PROJ_FILE" -c Release -r "$rid" --self-contained true \
        -p:PublishSingleFile=true \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        -p:PublishReadyToRun=false \
        -o "$output_path"
    
    return $?
}

# --- 3. Plattform-Builds ---

# WINDOWS
if build_target "win-x64" "Windows"; then
    mv "$DIST_DIR/Windows/GruetzeToaster.exe" "$DIST_DIR/Windows/GruetzeToaster.scr"
    (cd "$DIST_DIR/Windows" && pack_archive "$RELEASE_DIR/${APP_NAME}_v${VERSION}_Windows_x64.zip")
fi

# LINUX
if build_target "linux-x64" "Linux"; then
    echo "📦 Packaging Linux Binary..."
    (cd "$DIST_DIR/Linux" && pack_archive "$RELEASE_DIR/${APP_NAME}_v${VERSION}_Linux_x64.zip")
    
    if [[ "$OSTYPE" == "linux-gnu"* ]]; then
        echo "🏗️  Creating .deb package (WSL Safe Mode)..."
        
        # Wir nutzen /tmp (echtes Linux-Dateisystem), um NTFS-Rechte zu umgehen
        TMP_PKG="/tmp/gruetzetoaster_build"
        rm -rf "$TMP_PKG"
        mkdir -p "$TMP_PKG/usr/local/bin" "$TMP_PKG/DEBIAN"
        
        # Daten kopieren
        cp "$DIST_DIR/Linux/GruetzeToaster" "$TMP_PKG/usr/local/bin/gruetzetoaster"
        echo -e "Package: gruetzetoaster\nVersion: $VERSION\nArchitecture: amd64\nMaintainer: Gruetze-Software\nDescription: Flying Toasters Screensaver." > "$TMP_PKG/DEBIAN/control"
        
        # Hier funktionieren die Rechte garantiert:
        chmod -R 755 "$TMP_PKG"
        chmod 644 "$TMP_PKG/DEBIAN/control"
        
        # Paket im Linux-Dateisystem bauen
        dpkg-deb --build "$TMP_PKG" "$RELEASE_DIR/${APP_NAME}_v${VERSION}_amd64.deb"
        
        # Aufräumen
        rm -rf "$TMP_PKG"
    fi
fi

# macOS
for rid in "osx-x64:macOS_Intel" "osx-arm64:macOS_ARM"; do
    IFS=":" read -r RID NAME <<< "$rid"
    if build_target "$RID" "$NAME"; then
        APP_PATH="$DIST_DIR/$NAME/GruetzeToaster.app/Contents/MacOS"
        mkdir -p "$APP_PATH"
        mv "$DIST_DIR/$NAME/GruetzeToaster" "$APP_PATH/"
        echo '<?xml version="1.0" encoding="UTF-8"?><!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd"><plist version="1.0"><dict><key>CFBundleExecutable</key><string>GruetzeToaster</string><key>CFBundleName</key><string>GruetzeToaster</string><key>CFBundleVersion</key><string>'$VERSION'</string></dict></plist>' > "$DIST_DIR/$NAME/GruetzeToaster.app/Contents/Info.plist"
        (cd "$DIST_DIR/$NAME" && pack_archive "$RELEASE_DIR/${APP_NAME}_v${VERSION}_${NAME}.zip")
    fi
done

echo -e "\n--- ✨ Build Complete! ---"
ls -lh "$RELEASE_DIR"