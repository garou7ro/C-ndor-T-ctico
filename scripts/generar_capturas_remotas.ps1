# ==============================================================================
# SCRIPT DE AUTOMATIZACION: CAPTURAS Y TELEMETRIA REMOTA (TOTALMENTE AISLADO)
# NO MODIFICA EL CODIGO DEL JUEGO CONDOR TACTICO
# ==============================================================================

param(
    [string]$Destino = "C:\Users\kreiv\.gemini\antigravity\brain\2289d014-2fa2-4ce2-9148-a37fa94f326e"
)

Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "[CONDOR TACTICO] GENERADOR AISLADO DE CAPTURAS REMOTAS" -ForegroundColor Yellow
Write-Host "Directorio objetivo: $Destino" -ForegroundColor Gray
Write-Host "==================================================================" -ForegroundColor Cyan

$raiz = Resolve-Path (Join-Path $PSScriptRoot "..")
$proyectoCapturador = Join-Path $raiz "tools\CapturadorRemoto\CapturadorRemoto.csproj"

if (-not (Test-Path $proyectoCapturador)) {
    Write-Error "No se encontro el proyecto del capturador en: $proyectoCapturador"
    exit 1
}

if (-not (Test-Path $Destino)) {
    New-Item -ItemType Directory -Path $Destino -Force | Out-Null
}

Write-Host "`n[1/3] Ejecutando motor de telemetria y renderizado fuera de pantalla..." -ForegroundColor Green
dotnet run --project $proyectoCapturador -- $Destino

# Buscar ffmpeg en PATH o en Winget
$ffmpegExe = $null
if (Get-Command "ffmpeg" -ErrorAction SilentlyContinue) {
    $ffmpegExe = "ffmpeg"
} else {
    $wingetFfmpeg = Get-ChildItem -Path "$env:LOCALAPPDATA\Microsoft\WinGet\Packages" -Filter "ffmpeg.exe" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($wingetFfmpeg) {
        $ffmpegExe = $wingetFfmpeg.FullName
    }
}

$dirFrames = Join-Path $Destino "frames_temp"
$videoMp4 = Join-Path $Destino "video_combate_tactico.mp4"
$animacionGif = Join-Path $Destino "animacion_combate_tactico.gif"

if ($ffmpegExe -and (Test-Path $dirFrames)) {
    Write-Host "`n[2/3] Codificando video MP4 (H.264 @ 30 FPS)..." -ForegroundColor Green
    & $ffmpegExe -y -loglevel error -framerate 30 -i "$dirFrames\frame_%03d.png" -c:v libx264 -pix_fmt yuv420p "$videoMp4"

    Write-Host "`n[3/3] Generando animacion GIF optimizada (15 FPS Lanczos)..." -ForegroundColor Green
    & $ffmpegExe -y -loglevel error -i "$videoMp4" -vf "fps=15,scale=680:-1:flags=lanczos,split[s0][s1];[s0]palettegen[p];[s1][p]paletteuse" "$animacionGif"

    # Limpiar fotogramas temporales
    Remove-Item -Path $dirFrames -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "`nFotogramas temporales liberados." -ForegroundColor DarkGray
} else {
    Write-Warning "FFmpeg no encontrado. Se mantendran los fotogramas PNG individuales en: $dirFrames"
}

Write-Host "`n[OK] Proceso finalizado. Archivos generados en: $Destino" -ForegroundColor Cyan
Get-ChildItem -Path $Destino | Where-Object { $_.Extension -in '.png', '.gif', '.mp4' } | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize
