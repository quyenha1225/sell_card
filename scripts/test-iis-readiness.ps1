# Readiness checks run on the Jenkins node before asking for release approval.
$ErrorActionPreference = 'Stop'
Get-Command robocopy.exe -ErrorAction Stop | Out-Null
foreach ($scriptName in @('deploy-iis.ps1', 'invoke-iis-deployment.ps1')) {
    $scriptPath = Join-Path $PSScriptRoot $scriptName
    if (-not (Test-Path -LiteralPath $scriptPath -PathType Leaf)) { throw "Missing deployment script: $scriptName" }
    $parseErrors = $null
    $tokens = $null
    [void][Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors.Count) { throw "Invalid PowerShell syntax: $scriptName" }
}
if ($env:IIS_DIR -ne 'C:\inetpub\wwwroot\BBEcom') { throw 'Unexpected IIS target. Update the guarded deployment scripts when moving the site.' }
foreach ($relative in @('Web.config', 'bin\BBEcom.dll', 'Global.asax')) {
    if (-not (Test-Path -LiteralPath (Join-Path $env:PUBLISH_DIR $relative) -PathType Leaf)) {
        throw "Published website is incomplete: $relative"
    }
}
if (-not (Test-Path -LiteralPath (Join-Path $env:IIS_DIR 'Web.config') -PathType Leaf)) {
    throw 'Private IIS Web.config is missing. Configure the existing IIS site first.'
}
if (Test-Path -LiteralPath (Join-Path $env:IIS_DIR 'app_offline.htm')) {
    throw 'IIS is already in maintenance mode. Resolve it before approving another release.'
}
if ((Get-Service W3SVC -ErrorAction Stop).Status -ne 'Running') { throw 'IIS W3SVC is not running.' }
try {
    $response = Invoke-WebRequest -Uri $env:WEBSITE_URL -UseBasicParsing -TimeoutSec 20 -MaximumRedirection 0
    if ($response.StatusCode -ne 200) { throw 'Expected HTTP 200.' }
} catch {
    throw 'The current IIS website does not return HTTP 200 without redirects. Check WEBSITE_URL, IIS bindings, Web.config and SQL connectivity before release.'
}
Write-Host 'IIS readiness passed: published files, deployment scripts, current configuration, service and HTTP 200.'
Write-Host 'Write permissions will be exercised when acquiring the deployment lock and copying the approved release.'
