param([string]$InputFile = 'TestResults\SellCard.trx', [string]$OutputFile = 'TestResults\junit.xml')
$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $InputFile)) { throw 'Test runner did not produce a TRX report.' }
[xml]$trx = Get-Content -LiteralPath $InputFile -Raw
$results = @($trx.SelectNodes('//*[local-name()="UnitTestResult"]'))
if ($results.Count -eq 0) { throw 'No tests executed; release is blocked.' }
$doc = New-Object System.Xml.XmlDocument
$suite = $doc.CreateElement('testsuite')
$suite.SetAttribute('name', 'SellCard.MSTest')
$suite.SetAttribute('tests', [string]$results.Count)
$failed = 0
$skipped = 0
foreach ($result in $results) {
    $test = $doc.CreateElement('testcase')
    $test.SetAttribute('name', $result.GetAttribute('testName'))
    $test.SetAttribute('classname', 'BBEcom.Tests')
    $duration = [TimeSpan]::Zero
    if ([TimeSpan]::TryParse($result.GetAttribute('duration'), [ref]$duration)) {
        $test.SetAttribute('time', $duration.TotalSeconds.ToString([Globalization.CultureInfo]::InvariantCulture))
    }
    $outcome = $result.GetAttribute('outcome')
    if ($outcome -eq 'NotExecuted') {
        $skipped++
        [void]$test.AppendChild($doc.CreateElement('skipped'))
    } elseif ($outcome -ne 'Passed') {
        $failed++
        $failure = $doc.CreateElement('failure')
        $failure.SetAttribute('message', "MSTest outcome: $outcome")
        $failure.InnerText = $result.InnerText
        [void]$test.AppendChild($failure)
    }
    [void]$suite.AppendChild($test)
}
$suite.SetAttribute('failures', [string]$failed)
$suite.SetAttribute('skipped', [string]$skipped)
[void]$doc.AppendChild($suite)
$doc.Save([IO.Path]::GetFullPath($OutputFile))
Write-Host "Tests: $($results.Count), failed: $failed, skipped: $skipped"
if ($failed -gt 0 -or $skipped -gt 0) { throw 'All discovered tests must pass before release.' }
