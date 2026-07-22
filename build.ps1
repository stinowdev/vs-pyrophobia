<#
.SYNOPSIS
    Builds the Pyrophobia mod and packages it into a release zip.

.DESCRIPTION
    Compiles the project in the chosen configuration, then zips the build
    output (DLL + modinfo.json + assets) into Releases/<modid>_<version>.zip.

.PARAMETER Configuration
    Build configuration. Defaults to Release.

.PARAMETER Deploy
    If set, also copies the packaged zip into the game's Mods folder so it can
    be tested immediately.

.EXAMPLE
    .\build.ps1
    .\build.ps1 -Deploy
#>
param(
    [string]$Configuration = "Release",
    [switch]$Deploy
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

# Read mod id + version from modinfo.json so there is a single source of truth.
$modinfo = Get-Content (Join-Path $root "resources\modinfo.json") -Raw | ConvertFrom-Json
$modId = $modinfo.modid
$version = $modinfo.version

Write-Host "Building $modId v$version ($Configuration)..." -ForegroundColor Cyan
dotnet build (Join-Path $root "Pyrophobia.csproj") -c $Configuration -p:Version=$version
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed." }

$outDir = Join-Path $root "bin\$Configuration"
$releaseDir = Join-Path $root "Releases"
New-Item -ItemType Directory -Force -Path $releaseDir | Out-Null

$zipPath = Join-Path $releaseDir "$($modId)_$($version).zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }

Write-Host "Packaging -> $zipPath" -ForegroundColor Cyan
$items = Get-ChildItem -Path $outDir | Where-Object { $_.Name -notmatch '\.(pdb|deps\.json)$' }
Compress-Archive -Path $items.FullName -DestinationPath $zipPath -Force

Write-Host "Created $zipPath" -ForegroundColor Green

if ($Deploy) {
    $gameDir = $env:VINTAGE_STORY
    if (-not $gameDir) { $gameDir = Join-Path $env:APPDATA "Vintagestory" }
    $modsDir = Join-Path (Join-Path $env:APPDATA "VintagestoryData") "Mods"
    if (-not (Test-Path $modsDir)) { $modsDir = Join-Path $gameDir "Mods" }
    New-Item -ItemType Directory -Force -Path $modsDir | Out-Null
    Copy-Item $zipPath $modsDir -Force
    Write-Host "Deployed to $modsDir" -ForegroundColor Green
}
