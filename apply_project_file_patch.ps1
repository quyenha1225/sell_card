param(
    [string]$ProjectRoot = $PSScriptRoot
)

$ErrorActionPreference = "Stop"
$projectFile = Join-Path $ProjectRoot "BBEcom.csproj"

if (-not (Test-Path $projectFile)) {
    Write-Host "Khong tim thay BBEcom.csproj tai: $ProjectRoot" -ForegroundColor Red
    Write-Host "Hay giai nen ZIP vao dung thu muc goc cua project roi chay lai." -ForegroundColor Yellow
    exit 1
}

$searchViewEntry = '    <Content Include="Views\Product\Search.cshtml" />'
$content = [System.IO.File]::ReadAllText($projectFile)

if ($content.Contains('Views\Product\Search.cshtml')) {
    Write-Host "Search.cshtml da co trong BBEcom.csproj. Khong can sua them." -ForegroundColor Green
    exit 0
}

$backupFile = "$projectFile.backup_before_search_patch"
Copy-Item $projectFile $backupFile -Force

$anchor = '    <Content Include="Views\Product\Category.cshtml" />'
if (-not $content.Contains($anchor)) {
    Write-Host "Khong tim thay dong Category.cshtml trong BBEcom.csproj." -ForegroundColor Red
    Write-Host "Hay them thu cong dong sau vao ItemGroup chua cac Views:" -ForegroundColor Yellow
    Write-Host $searchViewEntry
    exit 1
}

$content = $content.Replace($anchor, "$anchor`r`n$searchViewEntry")
[System.IO.File]::WriteAllText($projectFile, $content, [System.Text.UTF8Encoding]::new($true))

Write-Host "Da them Views\Product\Search.cshtml vao BBEcom.csproj." -ForegroundColor Green
Write-Host "Ban sao luu: $backupFile" -ForegroundColor Cyan
Write-Host "Bay gio mo lai solution, Clean Solution va Rebuild Solution." -ForegroundColor Cyan
