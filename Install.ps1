param([string]$AppDataDirectory=(Join-Path $env:LOCALAPPDATA 'EDDiscovery'))
$ErrorActionPreference='Stop'
if(Get-Process EDDiscovery -ErrorAction SilentlyContinue){throw 'Close EDDiscovery before installing the DLL.'}
$source=Join-Path $PSScriptRoot 'RouteTrackerCommander.dll'
if(-not (Test-Path -LiteralPath $source)){throw 'RouteTrackerCommander.dll is missing beside Install.ps1.'}
$directory=Join-Path $AppDataDirectory 'DLL'
New-Item -ItemType Directory -Path $directory -Force | Out-Null
$destination=Join-Path $directory 'RouteTrackerCommander.dll'
if(Test-Path -LiteralPath $destination){
 $backup=Join-Path $AppDataDirectory ('RouteTrackerCommander-backups\'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
 New-Item -ItemType Directory -Path $backup -Force | Out-Null
 Copy-Item -LiteralPath $destination -Destination $backup
 Write-Output ('Previous DLL backed up: '+$backup)
}
Unblock-File -LiteralPath $source -ErrorAction SilentlyContinue
Copy-Item -LiteralPath $source -Destination $destination -Force
if((Get-FileHash -LiteralPath $source).Hash -ne (Get-FileHash -LiteralPath $destination).Hash){throw 'Installed DLL checksum differs.'}
Write-Output ('Installed: '+$destination)
Write-Output 'Start EDDiscovery, allow the extension and add the Route Tracker - Commander panel.'
