param([string]$InputHelperDll, [string]$Executable = 'dist\iEnter.exe')
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$exe = Join-Path $root $Executable
$process = Start-Process -FilePath $exe -ArgumentList @('--self-test','test-results') -WorkingDirectory $root -WindowStyle Hidden -Wait -PassThru
Get-Content -LiteralPath (Join-Path $root 'test-results\self-test.txt')
if ($process.ExitCode -ne 0) { throw 'Self-test failed.' }
if ($InputHelperDll) {
    $dll = (Resolve-Path -LiteralPath $InputHelperDll).Path
    $process = Start-Process -FilePath $exe -ArgumentList @('--integration-test','test-results',('"' + $dll + '"')) -WorkingDirectory $root -WindowStyle Hidden -Wait -PassThru
    Get-Content -LiteralPath (Join-Path $root 'test-results\integration-test.txt')
    if ($process.ExitCode -ne 0) { throw 'Integration test failed.' }
}
