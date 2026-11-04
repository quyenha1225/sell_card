
param(
    [string]$ImageTag = 'sellcard:local',
    [string]$ContainerName = 'sellcard-web',
    [int]$Port = 8090,
    [string]$ConfigDir = 'C:\SellCardDocker\config'
)

$ErrorActionPreference = 'Stop'

# 1. Kiem tra Docker Windows Containers
$osType = & docker info --format '{{.OSType}}' 2>$null

if ($LASTEXITCODE -ne 0 -or $osType.Trim() -ne 'windows') {
    throw 'Docker must use Windows containers.'
}

# 2. Kiem tra Web.config rieng cua Docker
$webConfig = Join-Path $ConfigDir 'Web.config'

if (-not (Test-Path -LiteralPath $webConfig -PathType Leaf)) {
    throw "Missing Docker Web.config: $webConfig"
}

# 3. Kiem tra KeyDbContext ma khong hien mat khau
[xml]$cfg = Get-Content -LiteralPath $webConfig -Raw

$db = $cfg.SelectSingleNode(
    '/configuration/connectionStrings/add[@name="KeyDbContext"]'
)

if ($null -eq $db) {
    throw 'Missing KeyDbContext in Docker Web.config.'
}

$conn = $db.GetAttribute('connectionString')

if ([string]::IsNullOrWhiteSpace($conn)) {
    throw 'KeyDbContext connection string is empty.'
}

if ($conn -match '(?i)(integrated\s+security\s*=\s*(true|sspi)|trusted_connection\s*=\s*(true|yes)|server\s*=\s*\.\\SQLEXPRESS)') {
    throw 'Configure Docker to connect to SQL Server via host.docker.internal and SQL Authentication.'
}

# 4. Kiem tra container trung ten
$existing = @(& docker ps -a --format '{{.Names}}')

if ($existing -contains $ContainerName) {
    throw "Container '$ContainerName' already exists. Remove the old test container before running again."
}

# 5. Mount Web.config vao Windows Container
$mountSpec = "type=bind,source=$ConfigDir,target=C:\sellcard-config,readonly"

Write-Host 'Starting SellCard Docker container...'

# 6. Chay Docker: khong chi dinh Host IP tren Windows NAT
& docker run --detach `
    --isolation=hyperv `
    --name $ContainerName `
    --publish "${Port}:80" `
    --mount $mountSpec `
    $ImageTag

if ($LASTEXITCODE -ne 0) {
    throw 'Docker run failed.'
}

Write-Host ''
Write-Host 'SellCard Docker container started.'
Write-Host "Website: http://localhost:$Port/Home/Index"
Write-Host "Check logs: docker logs $ContainerName"
