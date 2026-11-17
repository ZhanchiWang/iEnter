param([string]$OutputDirectory = 'dist')
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $csc)) { throw '.NET Framework 4.x compiler not found.' }
$out = Join-Path $root $OutputDirectory
New-Item -ItemType Directory -Path $out -Force | Out-Null
$sources = Get-ChildItem -LiteralPath (Join-Path $root 'src') -Filter '*.cs' | ForEach-Object FullName
& $csc /nologo /target:winexe /platform:x64 /optimize+ /utf8output /warn:4 /win32icon:"$root\assets\auto-enter.ico" /win32manifest:"$root\src\app.manifest" /resource:"$root\assets\auto-enter.ico",AutoEnter.Icon /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Runtime.Serialization.dll /out:"$out\AutoEnter.exe" $sources
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
Copy-Item -LiteralPath "$root\src\App.config" -Destination "$out\AutoEnter.exe.config" -Force
Write-Output "Built: $out\AutoEnter.exe"
