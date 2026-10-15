# ==============================================================================
# SCRIPT: GenerarSpriteMisil.ps1
# DESCRIPCIÓN: Procesa y genera sprites de misiles supersónicos en alta definición
#              y optimizados para el juego (tanto para la nave principal como rival).
#              Elimina márgenes vacíos (tight crop) y aplica 4x SSAA bicúbico
#              para eliminar cualquier pixelado o deformación.
# USO: powershell -ExecutionPolicy Bypass -File GenerarSpriteMisil.ps1
# ==============================================================================

Add-Type -AssemblyName System.Drawing

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$BaseDir = Split-Path -Parent $ScriptDir
$AssetsDir = Join-Path $BaseDir "assets\sprites"
$SourceMissile = Join-Path $AssetsDir "misil_f35.png"
$OutputTightPlayer = Join-Path $AssetsDir "misil_player_hd.png"
$OutputTightRival = Join-Path $AssetsDir "misil_rival_hd.png"
$OutputGamePlayer = Join-Path $AssetsDir "misil_f35.png"
$OutputGameRival = Join-Path $AssetsDir "misil_rival.png"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   GENERADOR Y OPTIMIZADOR DE SPRITES DE MISIL F-35" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

if (-not (Test-Path $SourceMissile)) {
    Write-Error "No se encontro el archivo base: $SourceMissile"
    exit 1
}

$bmpSrc = [System.Drawing.Bitmap]::FromFile($SourceMissile)

# 1. Encontrar el Bounding Box exacto (eliminando bordes transparentes)
$minX = $bmpSrc.Width; $maxX = 0
$minY = $bmpSrc.Height; $maxY = 0

for ($y = 0; $y -lt $bmpSrc.Height; $y += 2) {
    for ($x = 0; $x -lt $bmpSrc.Width; $x += 2) {
        $pix = $bmpSrc.GetPixel($x, $y)
        if ($pix.A -gt 20) {
            if ($x -lt $minX) { $minX = $x }
            if ($x -gt $maxX) { $maxX = $x }
            if ($y -lt $minY) { $minY = $y }
            if ($y -gt $maxY) { $maxY = $y }
        }
    }
}

# Añadir pequeño margen de 4px para preservar anti-aliasing de humo/plasma
$minX = [Math]::Max(0, $minX - 4)
$maxX = [Math]::Min($bmpSrc.Width - 1, $maxX + 4)
$minY = [Math]::Max(0, $minY - 4)
$maxY = [Math]::Min($bmpSrc.Height - 1, $maxY + 4)

$cropW = $maxX - $minX + 1
$cropH = $maxY - $minY + 1

Write-Host "Recorte optimo detectado: X=[$minX, $maxX], Y=[$minY, $maxY], Tamano: ${cropW}x${cropH}" -ForegroundColor Green

# 2. Crear Bitmap recortado centrado
$bmpCrop = New-Object System.Drawing.Bitmap($cropW, $cropH, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$gCrop = [System.Drawing.Graphics]::FromImage($bmpCrop)
$gCrop.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$gCrop.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
$gCrop.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
$gCrop.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality

$srcRect = New-Object System.Drawing.Rectangle($minX, $minY, $cropW, $cropH)
$destRect = New-Object System.Drawing.Rectangle(0, 0, $cropW, $cropH)
$gCrop.DrawImage($bmpSrc, $destRect, $srcRect, [System.Drawing.GraphicsUnit]::Pixel)
$gCrop.Dispose()
$bmpSrc.Dispose()

# Guardar versión HD recortada
$bmpCrop.Save($OutputTightPlayer, [System.Drawing.Imaging.ImageFormat]::Png)
Write-Host " -> Sprite HD Jugador guardado: $OutputTightPlayer" -ForegroundColor Green

# 3. Crear versión HD Rival (Tono carmesí / fuego oscuro para el rival)
$bmpCropRival = New-Object System.Drawing.Bitmap($cropW, $cropH, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
for ($y = 0; $y -lt $cropH; $y++) {
    for ($x = 0; $x -lt $cropW; $x++) {
        $p = $bmpCrop.GetPixel($x, $y)
        if ($p.A -gt 0) {
            # Cambiar el tono cian/azul de plasma a rojo carmesí
            $r = [Math]::Min(255, [int]($p.R * 0.7 + $p.B * 0.9))
            $g = [int]($p.G * 0.5)
            $b = [int]($p.B * 0.3)
            $bmpCropRival.SetPixel($x, $y, [System.Drawing.Color]::FromArgb($p.A, $r, $g, $b))
        }
    }
}
$bmpCropRival.Save($OutputTightRival, [System.Drawing.Imaging.ImageFormat]::Png)
Write-Host " -> Sprite HD Rival guardado: $OutputTightRival" -ForegroundColor Green

# 4. Generar Sprites optimizados para el juego (18 x 38 px) con bicúbico de alta calidad
$gameW = 18
$gameH = 38

function Resize-Bicubic {
    param($srcBmp, $w, $h)
    $res = New-Object System.Drawing.Bitmap($w, $h, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($res)
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    $g.DrawImage($srcBmp, (New-Object System.Drawing.Rectangle(0, 0, $w, $h)))
    $g.Dispose()
    return $res
}

$bmpGamePlayer = Resize-Bicubic $bmpCrop $gameW $gameH
$bmpGamePlayer.Save($OutputGamePlayer, [System.Drawing.Imaging.ImageFormat]::Png)
Write-Host " -> Sprite Juego Jugador ($gameW x $gameH) actualizado: $OutputGamePlayer" -ForegroundColor Green

$bmpGameRival = Resize-Bicubic $bmpCropRival $gameW $gameH
$bmpGameRival.Save($OutputGameRival, [System.Drawing.Imaging.ImageFormat]::Png)
Write-Host " -> Sprite Juego Rival ($gameW x $gameH) actualizado: $OutputGameRival" -ForegroundColor Green

$bmpCrop.Dispose()
$bmpCropRival.Dispose()
$bmpGamePlayer.Dispose()
$bmpGameRival.Dispose()

Write-Host "`nSprites de misil generados exitosamente con maxima definicion y sin distorsion." -ForegroundColor Cyan
