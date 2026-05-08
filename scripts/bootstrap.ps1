# bootstrap.ps1
# First-time installation of `docformatter` on Windows.
#
# Run from any PowerShell prompt:
#   iwr -useb https://raw.githubusercontent.com/Gilcemir/markup_helper/main/scripts/bootstrap.ps1 | iex
#
# Idempotent: safe to re-run. Adds %USERPROFILE%\bin to User PATH only if missing.

$ErrorActionPreference = 'Stop'
$ProgressPreference    = 'SilentlyContinue'  # speeds up Invoke-WebRequest

$Repo       = 'Gilcemir/markup_helper'
$Branch     = 'main'
$BinDir     = Join-Path $env:USERPROFILE 'bin'
$ExePath    = Join-Path $BinDir 'docformatter.exe'
$UpdatePs1  = Join-Path $BinDir 'docformatter-update.ps1'
$UpdateCmd  = Join-Path $BinDir 'docformatter-update.cmd'

function Add-ToUserPath {
    param([string]$Dir)
    $current = [Environment]::GetEnvironmentVariable('Path', 'User')
    if (-not $current) { $current = '' }
    $entries = $current.Split(';', [StringSplitOptions]::RemoveEmptyEntries)
    foreach ($e in $entries) {
        if ([IO.Path]::GetFullPath($e.TrimEnd('\')) -ieq [IO.Path]::GetFullPath($Dir.TrimEnd('\'))) {
            Write-Host "PATH already contains $Dir"
            return $false
        }
    }
    $newPath = if ($current) { "$current;$Dir" } else { $Dir }
    [Environment]::SetEnvironmentVariable('Path', $newPath, 'User')
    Write-Host "Added $Dir to User PATH"
    return $true
}

function Get-LatestReleaseAssetUrl {
    param([string]$Repo, [string]$AssetName)
    $api = "https://api.github.com/repos/$Repo/releases/latest"
    $headers = @{ 'User-Agent' = 'docformatter-bootstrap' }
    $rel = Invoke-RestMethod -Uri $api -Headers $headers
    $asset = $rel.assets | Where-Object { $_.name -eq $AssetName }
    if (-not $asset) {
        throw "Asset '$AssetName' not found in release $($rel.tag_name)"
    }
    return @{ Url = $asset.browser_download_url; Tag = $rel.tag_name }
}

Write-Host '== docformatter bootstrap =='

if (-not (Test-Path -LiteralPath $BinDir)) {
    New-Item -ItemType Directory -Path $BinDir -Force | Out-Null
    Write-Host "Created $BinDir"
} else {
    Write-Host "Directory already exists: $BinDir"
}

[void](Add-ToUserPath -Dir $BinDir)

Write-Host 'Fetching latest release info...'
$release = Get-LatestReleaseAssetUrl -Repo $Repo -AssetName 'docformatter.exe'
Write-Host "Latest release: $($release.Tag)"

Write-Host "Downloading docformatter.exe -> $ExePath"
Invoke-WebRequest -Uri $release.Url -OutFile $ExePath -UseBasicParsing

$rawBase = "https://raw.githubusercontent.com/$Repo/$Branch/scripts"
Write-Host "Downloading docformatter-update.ps1 -> $UpdatePs1"
Invoke-WebRequest -Uri "$rawBase/docformatter-update.ps1" -OutFile $UpdatePs1 -UseBasicParsing
Write-Host "Downloading docformatter-update.cmd -> $UpdateCmd"
Invoke-WebRequest -Uri "$rawBase/docformatter-update.cmd" -OutFile $UpdateCmd -UseBasicParsing

Write-Host ''
Write-Host 'Done.'
Write-Host 'Open a NEW terminal (so it picks up the updated PATH) and run:'
Write-Host '  docformatter --version'
Write-Host '  docformatter --help'
Write-Host 'To update later:  docformatter-update'
