# Build and package Windows installer (Inno Setup).
# Requires: Windows, .NET SDK with net48, Inno Setup 6 (iscc.exe on PATH or default install).

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
Set-Location $Root

Write-Host "==> Building Release net48..."
dotnet build "$Root\AssistenteDeReparo.sln" -c Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$Exe = Join-Path $Root "src\AssistenteDeReparo\bin\Release\net48\AssistenteDeReparo.exe"
if (-not (Test-Path $Exe)) {
    Write-Error "Build output not found: $Exe"
    exit 1
}

$Iscc = $null
$Candidates = @(
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles}\Inno Setup 6\ISCC.exe",
    "ISCC.exe"
)
foreach ($c in $Candidates) {
    if (Get-Command $c -ErrorAction SilentlyContinue) { $Iscc = (Get-Command $c).Source; break }
    if (Test-Path $c) { $Iscc = $c; break }
}

if (-not $Iscc) {
    Write-Error "Inno Setup 6 (ISCC.exe) not found. Install from https://jrsoftware.org/isinfo.php"
    exit 1
}

New-Item -ItemType Directory -Force -Path (Join-Path $Root "dist") | Out-Null

Write-Host "==> Compiling installer with $Iscc ..."
& $Iscc (Join-Path $Root "installer\AssistenteDeReparo.iss")
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "==> Done. See dist\"
Get-ChildItem (Join-Path $Root "dist") | Format-Table Name, Length, LastWriteTime
