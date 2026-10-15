# ==============================================================================
# PROYECTO: CÓNDOR TÁCTICO (SIMULADOR DE COMBATE AÉREO 2D)
# SCRIPT: GenerarModeloB2Spirit.ps1
# DESCRIPCIÓN: Generador de renders de diagnóstico del bombardero B-2 Spirit
# ==============================================================================

Add-Type -AssemblyName System.Drawing
$csCode = Get-Content -Path "$PSScriptRoot\..\CondorTactico\Graphics\B2SpiritModel.cs" -Raw
Add-Type -TypeDefinition $csCode -ReferencedAssemblies System.Drawing, System.Drawing.Primitives, System.Runtime

$outUltra = Join-Path $PSScriptRoot "..\assets\renders\B2_Spirit_Render_UltraHD.png"
$outGame  = Join-Path $PSScriptRoot "..\assets\renders\B2_Spirit_Render_Game.png"
$outMisil = Join-Path $PSScriptRoot "..\assets\sprites\B2_Misil_HD.png"

$sw = [System.Diagnostics.Stopwatch]::StartNew()
$bmpUltra = [CondorTactico.B2SpiritModel]::DibujarB2Spirit(980, 700, 0, $false, $true, 2)
$bmpUltra.Save($outUltra, [System.Drawing.Imaging.ImageFormat]::Png)
$bmpUltra.Dispose()
Write-Host "B-2 Spirit UltraHD generado en $($sw.ElapsedMilliseconds) ms -> $outUltra"

$sw.Restart()
$bmpGame = [CondorTactico.B2SpiritModel]::DibujarB2Spirit(132, 92, 0, $false, $false, 4)
$bmpGame.Save($outGame, [System.Drawing.Imaging.ImageFormat]::Png)
$bmpGame.Dispose()
Write-Host "B-2 Spirit Game (4x SSAA) generado en $($sw.ElapsedMilliseconds) ms -> $outGame"

$bmpMisil = [CondorTactico.B2SpiritModel]::DibujarMisilB2(80, 160, 0, $false, 4)
$bmpMisil.Save($outMisil, [System.Drawing.Imaging.ImageFormat]::Png)
$bmpMisil.Dispose()
Write-Host "Misil AGM-158 B-2 generado -> $outMisil"
