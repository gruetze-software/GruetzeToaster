#!/bin/bash

# --- 1. Konfiguration ---
PROJ_FILE="./GruetzeToaster.csproj"
APP_NAME="GruetzeToaster"
DIST_DIR="dist"
RELEASE_DIR="$DIST_DIR/releases"

# Version dynamisch aus der csproj extrahieren
VERSION=$(grep -oPm1 "(?<=<Version>)[^<]+" $PROJ_FILE || echo "1.0.0")

echo "--- 🚀 Starting Build Process for $APP_NAME v$VERSION ---"

# Aufräumen und Release-Ordner erstellen
rm -rf $DIST_DIR
mkdir -p $RELEASE_DIR

# --- 2. Hilfsfunktionen ---

# Prüft, welches Pack-Programm installiert ist
pack_archive() {
    local target_zip="../../$1"
    if command -v zip >/dev/null 2>&1; then
        zip -r "$target_zip" .
    elif command -v 7z >/dev/null 2>&1; then
        7z a -tzip "$target_zip" ./*
    else
        echo "⚠️  ERROR: Neither 'zip' nor '7z' found! Archive could not be created."
        return 1
    fi
}

build_target() {
    local rid=$1
    local platform_name=$2
    local output_path="$DIST_DIR/$platform_name"
    
    echo -e "\n📦 Building for $platform_name ($rid)..."
    dotnet publish "$PROJ_FILE" -c Release -r "$rid" --self-contained true \
        -p:PublishSingleFile=true \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        -p:PublishReadyToRun=true \
        -o "$output_path"
    return $?
}

# --- 3. Plattform-Builds & Paketierung ---

# A. WINDOWS (x64)
if build_target "win-x64" "Windows"; then
    echo "🔧 Converting .exe to .scr..."
    mv "$DIST_DIR/Windows/GruetzeToaster.exe" "$DIST_DIR/Windows/GruetzeToaster.scr"
    (cd "$DIST_DIR/Windows" && pack_archive "$RELEASE_DIR/${APP_NAME}_v${VERSION}_Windows_x64.zip")
fi

# B. LINUX (x64)
if build_target "linux-x64" "Linux"; then
    echo "📦 Packaging Linux Binary..."
    (cd "$DIST_DIR/Linux" && pack_archive "$RELEASE_DIR/${APP_NAME}_v${VERSION}_Linux_x64.zip")
    
    if [[ "$OSTYPE" == "linux-gnu"* ]]; then
        echo "🏗️  Creating .deb package..."
        PKG_ROOT="$DIST_DIR/linux_pkg"
        mkdir -p "$PKG_ROOT/usr/local/bin" "$PKG_ROOT/DEBIAN"
        cp "$DIST_DIR/Linux/GruetzeToaster" "$PKG_ROOT/usr/local/bin/gruetzetoaster"
        echo -e "Package: gruetzetoaster\nVersion: $VERSION\nArchitecture: amd64\nMaintainer: Gruetze-Software\nDescription: Flying Toasters for Linux." > "$PKG_ROOT/DEBIAN/control"
        sudo chown -R root:root "$PKG_ROOT"
        dpkg-deb --build "$PKG_ROOT" "$RELEASE_DIR/${APP_NAME}_v${VERSION}_amd64.deb"
        sudo chown -R $USER:$USER "$DIST_DIR"
    fi
fi

# C. macOS Intel
if build_target "osx-x64" "macOS_Intel"; then
    echo "🍎 Creating macOS Intel App Bundle..."
    APP_PATH="$DIST_DIR/macOS_Intel/GruetzeToaster.app"
    mkdir -p "$APP_PATH/Contents/MacOS"
    mv "$DIST_DIR/macOS_Intel/GruetzeToaster" "$APP_PATH/Contents/MacOS/"
    # Info.plist Erstellung (gekürzt für Übersicht)
    echo '<?xml version="1.0" encoding="UTF-8"?><!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd"><plist version="1.0"><dict><key>CFBundleExecutable</key><string>GruetzeToaster</string><key>CFBundleName</key><string>GruetzeToaster</string><key>CFBundleVersion</key><string>'$VERSION'</string></dict></plist>' > "$APP_PATH/Contents/Info.plist"
    (cd "$DIST_DIR/macOS_Intel" && pack_archive "$RELEASE_DIR/${APP_NAME}_v${VERSION}_macOS_Intel.zip")
fi

# D. macOS Apple Silicon
if build_target "osx-arm64" "macOS_ARM"; then
    echo "🍎 Creating macOS ARM App Bundle..."
    APP_PATH="$DIST_DIR/macOS_ARM/GruetzeToaster.app"
    mkdir -p "$APP_PATH/Contents/MacOS"
    mv "$DIST_DIR/macOS_ARM/GruetzeToaster" "$APP_PATH/Contents/MacOS/"
    cp "$DIST_DIR/macOS_Intel/GruetzeToaster.app/Contents/Info.plist" "$APP_PATH/Contents/Info.plist"
    (cd "$DIST_DIR/macOS_ARM" && pack_archive "$RELEASE_DIR/${APP_NAME}_v${VERSION}_macOS_AppleSilicon.zip")
fi

echo -e "\n--- ✨ Build Complete! ---"
echo "Release assets are ready in: $RELEASE_DIR"
ls -lh "$RELEASE_DIR"