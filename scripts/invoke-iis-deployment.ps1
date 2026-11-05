# FileShare.None coordinates all deployments using this wrapper on this IIS host.
# Keep the lock file on disk: deleting it after release can introduce a race.
param(
    [string]$LockDirectory = 'C:\JenkinsBackups\BBEcom',
    [ValidateRange(1, 600)][int]$LockTimeoutSeconds = 120
)
$ErrorActionPreference = 'Stop'
New-Item -ItemType Directory -Path $lockDirectory -Force | Out-Null
$lockFile = Join-Path $lockDirectory 'iis-deployment.lock'
$deadline = [DateTime]::UtcNow.AddSeconds($LockTimeoutSeconds)
$handle = $null
try {
    Write-Host "Waiting for the local IIS deployment lock (maximum $LockTimeoutSeconds seconds)..."
    while ($null -eq $handle) {
        try {
            $handle = [IO.File]::Open($lockFile, [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
        } catch [IO.IOException] {
            $errorCode = $_.Exception.HResult -band 0xFFFF
            if ($errorCode -notin @(32, 33)) { throw }
            if ([DateTime]::UtcNow -ge $deadline) { throw 'Another deployment holds the IIS lock. Retry after it finishes.' }
            Start-Sleep -Seconds 3
        }
    }
    Write-Host 'Local IIS deployment lock acquired.'
    $global:LASTEXITCODE = 0
    & (Join-Path $PSScriptRoot 'deploy-iis.ps1')
    if ($LASTEXITCODE -ne 0) { throw "IIS deployment failed with exit code $LASTEXITCODE" }
} finally {
    if ($null -ne $handle) {
        $handle.Dispose()
        Write-Host 'Local IIS deployment lock released.'
    }
}
