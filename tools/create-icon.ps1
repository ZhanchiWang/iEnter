$ErrorActionPreference = 'Stop'
# Use the same .NET Framework drawing implementation as the desktop app.
if ($PSVersionTable.PSEdition -eq 'Core') {
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath
    if ($LASTEXITCODE -ne 0) { throw 'Icon rendering failed.' }
    return
}
Add-Type -AssemblyName System.Drawing
$assets = Join-Path (Split-Path $PSScriptRoot -Parent) 'assets'
New-Item -ItemType Directory -Path $assets -Force | Out-Null
Add-Type -Path (Join-Path (Split-Path $PSScriptRoot -Parent) 'src\IconArtwork.cs') -ReferencedAssemblies System.Drawing
function Render-Icon([int]$size) {
    $bmp = New-Object System.Drawing.Bitmap($size, $size)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.Clear([System.Drawing.Color]::Transparent)
    [TypelessAutoEnter.IconArtwork]::Draw($g, (New-Object System.Drawing.RectangleF(0,0,$size,$size)))
    $g.Dispose()
    return $bmp
}
$preview = Render-Icon 1024
$preview.Save((Join-Path $assets 'ienter.png'),[System.Drawing.Imaging.ImageFormat]::Png)
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
$file = [System.IO.File]::Create((Join-Path $assets 'ienter.ico'))
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
Write-Output 'Created ienter.ico (10 native sizes, 16–256 px) and ienter.png (1024 px).'
