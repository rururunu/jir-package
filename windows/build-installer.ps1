param(
    [string]$Configuration = "release",
    [string]$Version = "0.1.0"
)

$ErrorActionPreference = "Stop"

$Root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$Dist = Join-Path $Root "dist"
$BuildDir = Join-Path $Dist "build"
$Out = Join-Path $Dist "jir-$Version-windows-x64-gui-setup.exe"

Write-Host "Building jir ($Configuration)..." -ForegroundColor Cyan
Push-Location $Root
cargo build --release
$Metadata = cargo metadata --no-deps --format-version 1 | ConvertFrom-Json
Pop-Location

$TargetDir = $Metadata.target_directory
$BuiltExe = Join-Path $TargetDir "$Configuration\jir-cli.exe"

if (!(Test-Path $BuiltExe)) {
    throw "Built executable not found: $BuiltExe"
}

New-Item -ItemType Directory -Path $Dist -Force | Out-Null
if (Test-Path $BuildDir) {
    Remove-Item $BuildDir -Recurse -Force
}
New-Item -ItemType Directory -Path $BuildDir -Force | Out-Null

$Csc = (Get-Command csc.exe -ErrorAction SilentlyContinue).Source
if (-not $Csc) {
    $Csc = Get-ChildItem "$env:WINDIR\Microsoft.NET\Framework64" -Filter csc.exe -Recurse -ErrorAction SilentlyContinue |
        Sort-Object FullName -Descending |
        Select-Object -First 1 -ExpandProperty FullName
}
if (-not $Csc) {
    throw "csc.exe was not found. Cannot build Windows GUI installer."
}

if (Test-Path $Out) {
    try {
        Remove-Item $Out -Force
    } catch {
        $stamp = Get-Date -Format "yyyyMMdd-HHmmss"
        $Out = Join-Path $Dist "jir-$Version-windows-x64-gui-setup-$stamp.exe"
        Write-Host "Existing installer is locked. Using new output path:" -ForegroundColor Yellow
        Write-Host "  $Out"
    }
}

Write-Host "Building standalone GUI installer..." -ForegroundColor Cyan
$Uninstaller = Join-Path $BuildDir "uninstall.exe"
if (Test-Path $Uninstaller) {
    Remove-Item $Uninstaller -Force
}
& $Csc `
    /nologo `
    /target:winexe `
    "/out:$Uninstaller" `
    /reference:System.Windows.Forms.dll `
    /reference:System.Drawing.dll `
    (Join-Path $PSScriptRoot "JirUninstall.cs")

if (!(Test-Path $Uninstaller)) {
    throw "Uninstaller was not created: $Uninstaller"
}

& $Csc `
    /nologo `
    /target:winexe `
    "/out:$Out" `
    /reference:System.Windows.Forms.dll `
    /reference:System.Drawing.dll `
    "/resource:$BuiltExe,jir.exe" `
    "/resource:$Uninstaller,uninstall.exe" `
    (Join-Path $PSScriptRoot "JirSetup.cs")

if (!(Test-Path $Out)) {
    throw "Installer was not created: $Out"
}

Remove-Item $BuildDir -Recurse -Force

Write-Host ""
Write-Host "Installer created:" -ForegroundColor Green
Write-Host "  $Out"

