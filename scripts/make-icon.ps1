#Requires -Version 5.1
<#
.SYNOPSIS
    Gera o icone do Pulso (app.ico, Pulso.app.ico e assets/icon.png).

.DESCRIPTION
    Desenha o hexagono com a linha de pulso sobre um fundo escuro e grava
    um ICO multi-resolucao (16 a 256 px) usado pelo executavel e pela marca.
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$root = Split-Path -Parent $PSScriptRoot
$src = Join-Path $root 'src/Pulso'
$assets = Join-Path $root 'assets'

$ink = [System.Drawing.Color]::FromArgb(11, 14, 10)
$accentSoft = [System.Drawing.Color]::FromArgb(233, 199, 122)
$accentDeep = [System.Drawing.Color]::FromArgb(146, 108, 40)
$bgTop = [System.Drawing.Color]::FromArgb(26, 33, 22)
$bgBottom = [System.Drawing.Color]::FromArgb(9, 11, 8)
$border = [System.Drawing.Color]::FromArgb(190, 201, 162, 74)

function New-IconBitmap([int]$size) {
    $bmp = New-Object System.Drawing.Bitmap $size, $size, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    $g.Clear([System.Drawing.Color]::Transparent)

    $pad = [Math]::Max(0.5, $size * 0.015)
    $radius = [Math]::Max(2.0, $size * 0.2)
    $rect = New-Object System.Drawing.RectangleF $pad, $pad, ($size - 2 * $pad), ($size - 2 * $pad)

    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = $radius * 2
    $path.AddArc($rect.X, $rect.Y, $d, $d, 180, 90)
    $path.AddArc($rect.Right - $d, $rect.Y, $d, $d, 270, 90)
    $path.AddArc($rect.Right - $d, $rect.Bottom - $d, $d, $d, 0, 90)
    $path.AddArc($rect.X, $rect.Bottom - $d, $d, $d, 90, 90)
    $path.CloseFigure()

    $bg = New-Object System.Drawing.Drawing2D.LinearGradientBrush $rect, $bgTop, $bgBottom, 65.0
    $g.FillPath($bg, $path)
    $bg.Dispose()

    $penBorder = New-Object System.Drawing.Pen $border, ([Math]::Max(1.0, $size * 0.014))
    $g.DrawPath($penBorder, $path)
    $penBorder.Dispose()
    $path.Dispose()

    $inset = $size * 0.16
    $left = $inset
    $top = $inset
    $right = $size - $inset
    $bottom = $size - $inset
    $cx = $size / 2.0
    $cy = $size / 2.0
    $points = @(
        (New-Object System.Drawing.PointF $cx, $top),
        (New-Object System.Drawing.PointF $right, ($top + ($bottom - $top) * 0.25)),
        (New-Object System.Drawing.PointF $right, ($top + ($bottom - $top) * 0.75)),
        (New-Object System.Drawing.PointF $cx, $bottom),
        (New-Object System.Drawing.PointF $left, ($top + ($bottom - $top) * 0.75)),
        (New-Object System.Drawing.PointF $left, ($top + ($bottom - $top) * 0.25))
    )
    $hex = New-Object System.Drawing.Drawing2D.GraphicsPath
    $hex.AddPolygon($points)
    $hexRect = New-Object System.Drawing.RectangleF $left, $top, ($right - $left), ($bottom - $top)
    $fill = New-Object System.Drawing.Drawing2D.LinearGradientBrush $hexRect, $accentSoft, $accentDeep, 60.0
    $g.FillPath($fill, $hex)
    $fill.Dispose()
    $penHex = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(150, $ink)), ([Math]::Max(1.0, $size * 0.012))
    $g.DrawPath($penHex, $hex)
    $penHex.Dispose()
    $hex.Dispose()

    $w = $right - $left
    $h = $bottom - $top
    $pulse = @(
        (New-Object System.Drawing.PointF ($cx - $w * 0.3), $cy),
        (New-Object System.Drawing.PointF ($cx - $w * 0.13), $cy),
        (New-Object System.Drawing.PointF $cx, ($cy - $h * 0.2)),
        (New-Object System.Drawing.PointF ($cx + $w * 0.14), ($cy + $h * 0.14)),
        (New-Object System.Drawing.PointF ($cx + $w * 0.3), $cy)
    )
    $penPulse = New-Object System.Drawing.Pen $ink, ([Math]::Max(1.2, $size * 0.028))
    $penPulse.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $penPulse.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $penPulse.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $g.DrawLines($penPulse, $pulse)
    $penPulse.Dispose()

    $g.Dispose()
    return $bmp
}

function Get-DibBytes([System.Drawing.Bitmap]$bmp) {
    $size = $bmp.Width
    $rect = New-Object System.Drawing.Rectangle 0, 0, $size, $size
    $locked = $bmp.LockBits($rect, [System.Drawing.Imaging.ImageLockMode]::ReadOnly, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $stride = $locked.Stride
    $raw = New-Object byte[] ($stride * $size)
    [System.Runtime.InteropServices.Marshal]::Copy($locked.Scan0, $raw, 0, $raw.Length)
    $bmp.UnlockBits($locked)

    $maskStride = [int]([Math]::Ceiling($size / 32.0) * 4)
    $maskSize = $maskStride * $size
    $imageSize = $size * $size * 4

    $ms = New-Object System.IO.MemoryStream
    $bw = New-Object System.IO.BinaryWriter $ms
    $bw.Write([UInt32]40)
    $bw.Write([Int32]$size)
    $bw.Write([Int32]($size * 2))
    $bw.Write([UInt16]1)
    $bw.Write([UInt16]32)
    $bw.Write([UInt32]0)
    $bw.Write([UInt32]$imageSize)
    $bw.Write([Int32]0)
    $bw.Write([Int32]0)
    $bw.Write([UInt32]0)
    $bw.Write([UInt32]0)
    for ($y = $size - 1; $y -ge 0; $y--) {
        $bw.Write($raw, $y * $stride, $size * 4)
    }
    $bw.Write((New-Object byte[] $maskSize))
    $bw.Flush()
    $bytes = $ms.ToArray()
    $bw.Dispose()
    $ms.Dispose()
    return $bytes
}

$sizes = @(16, 24, 32, 48, 64, 128, 256)
$images = @()
foreach ($size in $sizes) {
    $bmp = New-IconBitmap $size
    $bytes = Get-DibBytes $bmp
    $images += ,@($size, $bytes)
    $bmp.Dispose()
}

$ms = New-Object System.IO.MemoryStream
$bw = New-Object System.IO.BinaryWriter $ms
$bw.Write([UInt16]0)
$bw.Write([UInt16]1)
$bw.Write([UInt16]$sizes.Count)
$offset = 6 + 16 * $sizes.Count
foreach ($entry in $images) {
    $size = [int]$entry[0]
    $data = [byte[]]$entry[1]
    $dim = if ($size -ge 256) { 0 } else { $size }
    $bw.Write([Byte]$dim)
    $bw.Write([Byte]$dim)
    $bw.Write([Byte]0)
    $bw.Write([Byte]0)
    $bw.Write([UInt16]1)
    $bw.Write([UInt16]32)
    $bw.Write([UInt32]$data.Length)
    $bw.Write([UInt32]$offset)
    $offset += $data.Length
}
foreach ($entry in $images) {
    $bw.Write([byte[]]$entry[1])
}
$bw.Flush()
$icoBytes = $ms.ToArray()
$bw.Dispose()
$ms.Dispose()

[System.IO.File]::WriteAllBytes((Join-Path $src 'app.ico'), $icoBytes)
[System.IO.File]::WriteAllBytes((Join-Path $src 'Pulso.app.ico'), $icoBytes)

$icon256 = New-IconBitmap 256
$icon256.Save((Join-Path $assets 'icon.png'), [System.Drawing.Imaging.ImageFormat]::Png)
$icon256.Dispose()

Write-Host ("Icone gerado: {0} bytes, {1} resolucoes." -f $icoBytes.Length, $sizes.Count)
