# Called only after the Jenkins approval gate, while holding the deployment lock.
$ErrorActionPreference = 'Stop'
$source = [IO.Path]::GetFullPath($env:PUBLISH_DIR).TrimEnd('\')
$target = [IO.Path]::GetFullPath($env:IIS_DIR).TrimEnd('\')
if ($target -ne 'C:\inetpub\wwwroot\BBEcom') { throw 'Unexpected IIS deployment directory.' }
if (-not $source.StartsWith(([IO.Path]::GetFullPath($env:WORKSPACE).TrimEnd('\') + '\release\'), [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Publish directory must be inside the workspace release directory.'
}
foreach ($path in @($source, $target)) {
    if (-not (Test-Path -LiteralPath $path -PathType Container)) { throw "Missing directory: $path" }
    $root = Get-Item -LiteralPath $path
    if (($root.Attributes -band [IO.FileAttributes]::ReparsePoint) -or
        (Get-ChildItem -LiteralPath $path -Recurse -Force | Where-Object { $_.Attributes -band [IO.FileAttributes]::ReparsePoint })) {
        throw 'Reparse points are not supported in deployment directories.'
    }
}
foreach ($file in @('bin\BBEcom.dll', 'Global.asax')) {
    if (-not (Test-Path -LiteralPath (Join-Path $source $file) -PathType Leaf)) { throw "Missing release file: $file" }
}
if (-not (Test-Path -LiteralPath (Join-Path $target 'Web.config') -PathType Leaf)) {
    throw 'Configure the private IIS Web.config before deploying.'
}
$offline = Join-Path $target 'app_offline.htm'
if (Test-Path -LiteralPath $offline) { throw 'Website is already offline; resolve the existing maintenance operation first.' }

# Recheck after approval and after waiting for another job to release the lock.
& git fetch origin $env:BUILD_BRANCH
if ($LASTEXITCODE -ne 0) { throw 'Cannot verify release branch.' }
$latest = & git rev-parse FETCH_HEAD
if ($LASTEXITCODE -ne 0 -or $latest.Trim() -ne $env:RELEASE_COMMIT) {
    throw 'A newer commit exists. Build and approve the latest release instead.'
}

function Test-MutableFile([string]$RelativePath) {
    return $RelativePath -match '(?i)(^|\\)(Web\.config|app_offline\.htm)$|(^|\\)(App_Data|userfiles|uploads)(\\|$)'
}
function Copy-Website([string]$From, [string]$To) {
    & robocopy.exe $From $To /E /R:5 /W:2 /NP /NFL /NDL /XF Web.config app_offline.htm /XD App_Data userfiles uploads
    if ($LASTEXITCODE -ge 8) { throw "Website copy failed: robocopy exit $LASTEXITCODE" }
    $global:LASTEXITCODE = 0
}
function Assert-Healthy {
    for ($attempt = 1; $attempt -le 5; $attempt++) {
        try {
            $response = Invoke-WebRequest -Uri $env:WEBSITE_URL -UseBasicParsing -TimeoutSec 20 -MaximumRedirection 0
            if ($response.StatusCode -ne 200) { throw 'Expected HTTP 200.' }
            Write-Host 'Website check passed: HTTP 200 (without redirects).'
            return
        } catch {
            if ($attempt -eq 5) { throw 'Website health verification failed.' }
            Start-Sleep -Seconds 3
        }
    }
}

Assert-Healthy
# GUID prevents different jobs/build numbers from sharing a backup directory.
$backup = Join-Path 'C:\JenkinsBackups\BBEcom' ([guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $backup | Out-Null
$newFiles = @()
foreach ($file in Get-ChildItem -LiteralPath $source -Recurse -File) {
    $relative = $file.FullName.Substring($source.Length + 1)
    if (-not (Test-MutableFile $relative) -and -not (Test-Path -LiteralPath (Join-Path $target $relative))) {
        $newFiles += $relative
    }
}
$copyStarted = $false
$keepOffline = $false
try {
    Set-Content -LiteralPath $offline -Value 'Website is being updated.' -Encoding ASCII
    Start-Sleep -Seconds 3
    # Keep a complete backup, including the private configuration, on the agent only.
    & robocopy.exe $target $backup /E /R:2 /W:2 /NP /NFL /NDL /XF app_offline.htm
    if ($LASTEXITCODE -ge 8) { throw 'Backup failed; deployment was not started.' }
    $global:LASTEXITCODE = 0
    $copyStarted = $true
    Copy-Website $source $target
    $expected = (Get-FileHash -LiteralPath (Join-Path $source 'bin\BBEcom.dll')).Hash
    $actual = (Get-FileHash -LiteralPath (Join-Path $target 'bin\BBEcom.dll')).Hash
    if ($expected -ne $actual) { throw 'Deployed application DLL does not match the approved release.' }
    Remove-Item -LiteralPath $offline -Force
    Assert-Healthy
    Write-Host "Deployment verified. Commit: $env:RELEASE_COMMIT; approver: $env:APPROVED_BY; backup: $backup"
} catch {
    $deploymentError = $_
    if ($copyStarted) {
        try {
            Set-Content -LiteralPath $offline -Value 'Restoring previous release.' -Encoding ASCII
            Start-Sleep -Seconds 3
            Copy-Website $backup $target
            foreach ($relative in $newFiles) {
                $candidate = [IO.Path]::GetFullPath((Join-Path $target $relative))
                if (-not $candidate.StartsWith(($target + '\'), [StringComparison]::OrdinalIgnoreCase)) {
                    throw 'Rollback path escaped the IIS directory.'
                }
                if (Test-Path -LiteralPath $candidate -PathType Leaf) { Remove-Item -LiteralPath $candidate -Force }
            }
            Remove-Item -LiteralPath $offline -Force
            Assert-Healthy
            Write-Host 'Previous application files restored and HTTP check passed. Database and uploads were not restored.'
        } catch {
            $keepOffline = $true
            Set-Content -LiteralPath $offline -Value 'Maintenance required. Contact the administrator.' -Encoding ASCII
            Write-Host "Rollback failed. Site remains offline. Protected backup: $backup"
        }
    }
    throw $deploymentError
} finally {
    if (-not $keepOffline -and (Test-Path -LiteralPath $offline)) { Remove-Item -LiteralPath $offline -Force }
}
$global:LASTEXITCODE = 0
