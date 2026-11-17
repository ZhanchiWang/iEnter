$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$exe = Join-Path $root 'dist\iEnter.exe'
if (-not (Test-Path -LiteralPath $exe)) { throw 'Run build.ps1 first.' }
$version = (Get-Item -LiteralPath $exe).VersionInfo.FileVersion -replace '\.0$', ''
$release = Join-Path $root 'release'
New-Item -ItemType Directory -Path $release -Force | Out-Null
$zip = Join-Path $release "iEnter-v$version-Windows-x64.zip"
$files = @('dist\iEnter.exe','dist\iEnter.exe.config','README.md','README.zh-CN.md','LICENSE','assets','docs') | ForEach-Object { Join-Path $root $_ }
Compress-Archive -LiteralPath $files -DestinationPath $zip -Force
Copy-Item -LiteralPath $exe -Destination (Join-Path $release 'iEnter.exe') -Force
Copy-Item -LiteralPath "$root\dist\iEnter.exe.config" -Destination "$release\iEnter.exe.config" -Force
$hashes = @($zip, "$release\iEnter.exe", "$release\iEnter.exe.config") | ForEach-Object { $hash=Get-FileHash -LiteralPath $_ -Algorithm SHA256; $hash.Hash.ToLowerInvariant() + '  ' + [System.IO.Path]::GetFileName($_) }
[System.IO.File]::WriteAllLines((Join-Path $release 'SHA256SUMS.txt'), $hashes, (New-Object System.Text.UTF8Encoding($false)))
Write-Output "Packaged: $zip"
