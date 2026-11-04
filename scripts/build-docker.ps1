param(
    [string] $PublishDir = 'C:\JenkinsDeploy\BBEcom',
    [string] $ImageTag = 'sellcard:local'
)
$ErrorActionPreference = 'Stop'

$osType = (& docker info --format '{{.OSType}}' 2>$null)
if ($LASTEXITCODE -ne 0 -or $osType.Trim() -ne 'windows') {
    throw 'Docker Engine must be running in Windows containers mode (docker info -> windows).'
}

$publishDll = Join-Path $PublishDir 'bin\BBEcom.dll'
if (-not (Test-Path -LiteralPath $publishDll -PathType Leaf)) {
    throw "Missing Jenkins Publish output: $publishDll"
}

$dockerDir = Join-Path $PSScriptRoot '..\deploy\docker'
$context = Join-Path $env:TEMP ("SellCardDockerContext_" + [guid]::NewGuid().ToString('N'))
$siteDir = Join-Path $context 'site'
New-Item -ItemType Directory -Path $siteDir -Force | Out-Null

try {
    # Stage site without runtime config or likely mutable/secret data.
    # Uploaded images and persistent data must be mounted separately, not baked into images.
    & robocopy.exe $PublishDir $siteDir /E /R:2 /W:2 /NP `
        /XF Web.config Web.Release.config Web.Debug.config .env *.pfx *.pem *.key `
        /XD (Join-Path $PublishDir 'App_Data') (Join-Path $PublishDir 'Content\Admin\ckfinder\userfiles')

    if ($LASTEXITCODE -ge 8) {
        throw "Robocopy staging failed with exit code $LASTEXITCODE"
    }
    $global:LASTEXITCODE = 0

    if (Test-Path (Join-Path $siteDir 'Web.config')) {
        throw 'SECURITY: Web.config must never be present in Docker build context.'
    }

    Copy-Item (Join-Path $dockerDir 'Dockerfile') (Join-Path $context 'Dockerfile')
    Copy-Item (Join-Path $dockerDir 'start.ps1') (Join-Path $context 'start.ps1')

    & docker build --tag $ImageTag $context
    if ($LASTEXITCODE -ne 0) {
        throw "docker build failed with exit code $LASTEXITCODE"
    }
    Write-Host "Docker image built: $ImageTag"
}
finally {
    if (Test-Path -LiteralPath $context) {
        Remove-Item -LiteralPath $context -Recurse -Force -ErrorAction SilentlyContinue
    }
}
