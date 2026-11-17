$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$exe = Join-Path $root 'dist\AutoEnter.exe'
if (-not (Test-Path -LiteralPath $exe)) { throw 'Run build.ps1 first.' }
$version = (Get-Item -LiteralPath $exe).VersionInfo.FileVersion -replace '\.0$', ''
$release = Join-Path $root 'release'
New-Item -ItemType Directory -Path $release -Force | Out-Null
$zip = Join-Path $release "Auto-Enter-v$version-Windows-x64.zip"
$files = @('dist\AutoEnter.exe','dist\AutoEnter.exe.config','README.md','README.zh-CN.md','LICENSE','assets','docs') | ForEach-Object { Join-Path $root $_ }
Compress-Archive -LiteralPath $files -DestinationPath $zip -Force
Copy-Item -LiteralPath $exe -Destination (Join-Path $release 'AutoEnter.exe') -Force
Copy-Item -LiteralPath "$root\dist\AutoEnter.exe.config" -Destination "$release\AutoEnter.exe.config" -Force
$hashes = @($zip, "$release\AutoEnter.exe", "$release\AutoEnter.exe.config") | ForEach-Object { $hash=Get-FileHash -LiteralPath $_ -Algorithm SHA256; $hash.Hash.ToLowerInvariant() + '  ' + [System.IO.Path]::GetFileName($_) }
[System.IO.File]::WriteAllLines((Join-Path $release 'SHA256SUMS.txt'), $hashes, (New-Object System.Text.UTF8Encoding($false)))
Write-Output "Packaged: $zip"
