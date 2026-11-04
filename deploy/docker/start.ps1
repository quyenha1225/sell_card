$ErrorActionPreference = 'Stop'

$externalConfig = 'C:\sellcard-config\Web.config'
$targetConfig = 'C:\inetpub\wwwroot\Web.config'
$websiteDll = 'C:\inetpub\wwwroot\bin\BBEcom.dll'

if (-not (Test-Path -LiteralPath $websiteDll -PathType Leaf)) {
    throw 'Missing published website DLL: BBEcom.dll'
}
if (-not (Test-Path -LiteralPath $externalConfig -PathType Leaf)) {
    throw 'Missing external Web.config. Mount C:\SellCardDocker\config as C:\sellcard-config.'
}

# Never store runtime credentials in Docker build layers.
Copy-Item -LiteralPath $externalConfig -Destination $targetConfig -Force

Write-Host 'SellCard configuration loaded; starting IIS (w3svc)...'
& 'C:\ServiceMonitor.exe' 'w3svc'
exit $LASTEXITCODE
