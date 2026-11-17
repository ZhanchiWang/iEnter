$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$assets = Join-Path (Split-Path $PSScriptRoot -Parent) 'assets'
New-Item -ItemType Directory -Path $assets -Force | Out-Null
function Render-Icon([int]$size) {
    $bmp = New-Object System.Drawing.Bitmap($size, $size)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::Transparent)
    $g.ScaleTransform(($size / 512.0), ($size / 512.0))
    $round = New-Object System.Drawing.Drawing2D.GraphicsPath
    $round.AddArc(16,16,224,224,180,90)
    $round.AddArc(272,16,224,224,270,90)
    $round.AddArc(272,272,224,224,0,90)
    $round.AddArc(16,272,224,224,90,90)
    $round.CloseFigure()
    $bg = New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml('#007AFF'))
    $g.FillPath($bg,$round)
    $arrow = New-Object System.Drawing.Drawing2D.GraphicsPath
    $arrow.AddLine(376,132,376,240)
    $arrow.AddBezier(376,240,376,275.346,347.346,304,312,304)
    $arrow.AddLine(312,304,136,304)
    $arrow.StartFigure()
    $arrow.AddLine(208,232,136,304)
    $arrow.AddLine(136,304,208,376)
    $pen = New-Object System.Drawing.Pen([System.Drawing.Color]::White,56)
    $pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $pen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
    $g.DrawPath($pen,$arrow)
    $pen.Dispose(); $arrow.Dispose(); $bg.Dispose(); $round.Dispose(); $g.Dispose()
    return $bmp
}
$preview = Render-Icon 1024
$preview.Save((Join-Path $assets 'auto-enter.png'),[System.Drawing.Imaging.ImageFormat]::Png)
$preview.Dispose()
$sizes = @(16,20,24,32,40,48,64,96,128,256)
$frames = New-Object 'System.Collections.Generic.List[byte[]]'
foreach ($size in $sizes) {
    $bmp = Render-Icon $size
    $stream = New-Object System.IO.MemoryStream
    $bmp.Save($stream,[System.Drawing.Imaging.ImageFormat]::Png)
    $frames.Add($stream.ToArray())
    $stream.Dispose(); $bmp.Dispose()
}
$file = [System.IO.File]::Create((Join-Path $assets 'auto-enter.ico'))
$writer = New-Object System.IO.BinaryWriter($file)
$writer.Write([UInt16]0); $writer.Write([UInt16]1); $writer.Write([UInt16]$sizes.Count)
$offset = 6 + 16 * $sizes.Count
for ($i=0; $i -lt $sizes.Count; $i++) {
    $s = if ($sizes[$i] -eq 256) { 0 } else { $sizes[$i] }
    $writer.Write([byte]$s); $writer.Write([byte]$s); $writer.Write([byte]0); $writer.Write([byte]0)
    $writer.Write([UInt16]1); $writer.Write([UInt16]32); $writer.Write([UInt32]$frames[$i].Length); $writer.Write([UInt32]$offset)
    $offset += $frames[$i].Length
}
foreach ($frame in $frames) { $writer.Write($frame) }
$writer.Dispose(); $file.Dispose()
Write-Output 'Created auto-enter.ico (10 native sizes, 16–256 px) and auto-enter.png (1024 px).'
