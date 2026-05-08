# docformatter-update.ps1
# Pulls the latest released docformatter.exe from GitHub and replaces the local binary.
# Lives next to docformatter.exe in %USERPROFILE%\bin.

[CmdletBinding()]
param(
    [switch]$Force  # download even when local version already matches latest tag
)

$ErrorActionPreference = 'Stop'
$ProgressPreference    = 'SilentlyContinue'

$Repo    = 'Gilcemir/markup_helper'
$BinDir  = Split-Path -Parent $MyInvocation.MyCommand.Path
$ExePath = Join-Path $BinDir 'docformatter.exe'

function Get-LocalVersion {
    param([string]$Exe)
    if (-not (Test-Path -LiteralPath $Exe)) { return $null }
    try {
        $raw = & $Exe --version 2>$null
    } catch {
        return $null
    }
    if (-not $raw) { return $null }
    $line = ($raw -split "`r?`n" | Where-Object { $_ -match '\S' } | Select-Object -First 1).Trim()
    # strip metadata after '+', e.g. "0.2.0+abc1234" -> "0.2.0"
    return ($line -split '\+', 2)[0].Trim()
}

function Get-LatestRelease {
    param([string]$Repo)
    $api = "https://api.github.com/repos/$Repo/releases/latest"
    $headers = @{ 'User-Agent' = 'docformatter-update' }
    $rel = Invoke-RestMethod -Uri $api -Headers $headers
    $tag = "$($rel.tag_name)"
    $version = $tag.TrimStart('v')
    $asset = $rel.assets | Where-Object { $_.name -eq 'docformatter.exe' }
    if (-not $asset) { throw "docformatter.exe not found in release $tag" }
    return [pscustomobject]@{
        Tag     = $tag
        Version = $version
        Url     = $asset.browser_download_url
    }
}

Write-Host '== docformatter update =='
$local = Get-LocalVersion -Exe $ExePath
if ($local) { Write-Host "Local version:  $local" } else { Write-Host 'Local version:  (not installed or unreadable)' }

Write-Host 'Checking GitHub for latest release...'
$latest = Get-LatestRelease -Repo $Repo
Write-Host "Latest release: $($latest.Version)  (tag $($latest.Tag))"

if (-not $Force -and $local -and ($local -ieq $latest.Version)) {
    Write-Host 'Already up to date.'
    return
}

$tmp = [IO.Path]::Combine([IO.Path]::GetTempPath(), "docformatter.exe.$([Guid]::NewGuid()).download")
Write-Host "Downloading $($latest.Tag) ..."
Invoke-WebRequest -Uri $latest.Url -OutFile $tmp -UseBasicParsing

try {
    Move-Item -LiteralPath $tmp -Destination $ExePath -Force
} catch {
    Remove-Item -LiteralPath $tmp -ErrorAction SilentlyContinue
    Write-Host ''
    Write-Host 'Could not replace docformatter.exe. It is probably running.' -ForegroundColor Yellow
    Write-Host 'Close any terminal/process using docformatter and re-run docformatter-update.' -ForegroundColor Yellow
    throw
}

$newLocal = Get-LocalVersion -Exe $ExePath
Write-Host ''
Write-Host "Updated to $newLocal." -ForegroundColor Green
