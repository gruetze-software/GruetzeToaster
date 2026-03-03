# Setup: Zielplattformen und Projektdaten
$projectName = "gruetzetoaster" # Muss mit AssemblyName in csproj übereinstimmen
$publishRoot = "publish_output"

$targets = @(
    @{ rid = "win-x64";    name = "Windows";    ext = ".exe" },
    @{ rid = "linux-x64";  name = "Linux";      ext = "" },
    @{ rid = "osx-arm64";  name = "macOS_ARM";   ext = "" },
    @{ rid = "osx-x64";    name = "macOS_Intel"; ext = "" }
)

# Vorbereitung: Alten Output löschen
if (Test-Path $publishRoot) { Remove-Item -Recurse -Force $publishRoot }
New-Item -ItemType Directory -Path $publishRoot

foreach ($target in $targets) {
    $runtime = $target.rid
    $platformName = $target.name
    $exportPath = "$publishRoot/$platformName"

    Write-Host "--- Baue Release für $platformName ($runtime) ---" -ForegroundColor Cyan
    
    # 1. dotnet publish ausführen
    # --self-contained true: Damit es auch ohne installiertes .NET auf dem c54u läuft
    # -p:PublishSingleFile=true: Alles in eine einzige Datei packen
    dotnet publish -c Release `
                   -r $runtime `
                   --self-contained true `
                   -p:PublishSingleFile=true `
                   -p:IncludeNativeLibrariesForSelfExtract=true `
                   -o $exportPath

    if ($LASTEXITCODE -eq 0) 
    {
        # 2. Spezifische Anpassung für Windows (.exe zu .scr)
        if ($platformName -eq "Windows") 
        {
            $oldPath = "$exportPath/$projectName$($target.ext)"
            $newPath = "$exportPath/GruetzeToaster.scr"
            
            if (Test-Path $oldPath) {
                # Wir nutzen jetzt die Variable $newPath für den vollen Zielpfad
                Move-Item -Path $oldPath -Destination $newPath -Force
                Write-Host "SUCCESS: Windows-Datei wurde zu $newPath verschoben/umbenannt." -ForegroundColor Green
            }
        }
        elseif ($platformName -eq "Linux") 
        {
            Write-Host "Erstelle .deb Paket für Linux..." -ForegroundColor Cyan
            
            $staging = "$publishRoot/linux_pkg"
            $binDir = "$staging/usr/bin"
            $controlDir = "$staging/DEBIAN"

            # Ordnerstruktur erstellen
            New-Item -ItemType Directory -Path $binDir -Force
            New-Item -ItemType Directory -Path $controlDir -Force

            # Dateien kopieren
            Copy-Item "$exportPath/$projectName" -Destination "$binDir/$projectName"
            Copy-Item "debian_template/DEBIAN/control" -Destination "$controlDir/control"

            # Rechte setzen (Wichtig für Linux!)
            # Hinweis: Unter Windows benötigt dies WSL oder ein Tool wie bsdtar
            Write-Host "HINWEIS: Führe 'dpkg-deb --build $staging' auf einem Linux-System aus." -ForegroundColor Yellow
        }
        elseif ($platformName -like "macOS*") 
        {
            Write-Host "Erstelle .app Bundle für $platformName..." -ForegroundColor Cyan
            
            $appFolder = "$exportPath/GruetzeToaster.app"
            $contentsDir = "$appFolder/Contents"
            $macosDir = "$contentsDir/MacOS"
            $resourcesDir = "$contentsDir/Resources"

            # 1. Verzeichnisstruktur für macOS .app Bundle erstellen
            New-Item -ItemType Directory -Path $macosDir -Force
            New-Item -ItemType Directory -Path $resourcesDir -Force

            # 2. Die ausführbare Datei in den MacOS-Ordner verschieben
            # Wir nehmen die Datei, die 'dotnet publish' gerade erzeugt hat
            Move-Item -Path "$exportPath/$projectName" -Destination "$macosDir/$projectName" -Force

            # 3. Info.plist kopieren (Stelle sicher, dass die Datei im Projektordner liegt)
            if (Test-Path "macOS_template/Info.plist") {
                Copy-Item "macOS_template/Info.plist" -Destination "$contentsDir/Info.plist"
            }

            # 4. Icon kopieren (falls vorhanden)
            if (Test-Path "Assets/icon.icns") {
                Copy-Item "Assets/icon.icns" -Destination "$resourcesDir/icon.icns"
            }
        }
        Write-Host "FERTIG: $platformName Build liegt in $exportPath" -ForegroundColor Green
    } 
    else 
    {
        Write-Host "FEHLER: Build für $platformName fehlgeschlagen!" -ForegroundColor Red
    }
}

Write-Host "`nAlle Builds abgeschlossen. Die Dateien befinden sich im Ordner: $publishRoot" -ForegroundColor Yellow