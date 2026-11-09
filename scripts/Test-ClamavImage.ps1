#requires -Version 7.5
[CmdletBinding()]
param(
    [string]$Image = 'arzenal-clamav:debian12',
    [string]$Platform
)

$ErrorActionPreference = 'Stop'
$scannerProbeDirectory = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../Arzenal-Store-TestApi/Fixtures/ClamAV')).Path
$scannerRunArguments = @('run', '--rm', '-d', '--read-only', '--tmpfs', '/tmp', '--cap-drop', 'ALL',
    '--security-opt', 'no-new-privileges:true', '--mount', "type=bind,source=$scannerProbeDirectory,target=/var/lib/clamav,readonly",
    '--entrypoint', '/usr/bin/tini')
if ($Platform) { $scannerRunArguments += @('--platform', $Platform) }
$scannerRunArguments += @($Image, '-g', '--', 'clamd', '--config-file=/etc/clamav/clamd.conf', '--foreground')
$scannerContainerId = & docker @scannerRunArguments
if ($LASTEXITCODE -ne 0 -or $scannerContainerId -notmatch '^[0-9a-f]{64}$') { throw 'Le conteneur ClamAV de test ne démarre pas.' }
try {
    $scannerReady = $false
    for ($scannerAttempt = 0; $scannerAttempt -lt 20; $scannerAttempt++) {
        $null = & docker exec $scannerContainerId clamdscan --config-file=/etc/clamav/clamd.conf --ping=1 2>$null
        if ($LASTEXITCODE -eq 0) { $scannerReady = $true; break }
        Start-Sleep -Milliseconds 500
    }
    if (-not $scannerReady) { & docker logs $scannerContainerId; throw 'Le healthcheck ClamAV ne répond pas.' }
    $scannerResult = & docker exec $scannerContainerId clamdscan --config-file=/etc/clamav/clamd.conf /var/lib/clamav/probe.txt
    if ($LASTEXITCODE -ne 1 -or ($scannerResult -join "`n") -notmatch 'Arzenal.InertProbe') {
        throw 'La détection de la signature de test inerte a échoué.'
    }
    Write-Output "ClamAV OK : utilisateur non-root, configuration de production, healthcheck et signature inerte ($Image)."
} finally { & docker stop $scannerContainerId | Out-Null }
