[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$generator = Join-Path $PSScriptRoot 'Generate-LegacyCatalog.ps1'
& $generator -Check

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$allManifestPath = Join-Path $repositoryRoot 'docs\generated\legacy-query-manifest.csv'
$errorManifestPath = Join-Path $repositoryRoot 'docs\generated\legacy-error-query-manifest.csv'

$allRows = @(Import-Csv -LiteralPath $allManifestPath)
$errorRows = @(Import-Csv -LiteralPath $errorManifestPath)

if ($allRows.Count -ne 898) {
    throw "Expected 898 rows in the full CSV manifest, found $($allRows.Count)."
}
if ($errorRows.Count -ne 69) {
    throw "Expected 69 rows in the error CSV manifest, found $($errorRows.Count)."
}
if (@($allRows | Where-Object { $_.ErrorLike -eq 'true' }).Count -ne 69) {
    throw 'The full manifest does not contain exactly 69 error-like rows.'
}
if (@($errorRows | Where-Object { $_.ErrorLike -ne 'true' }).Count -ne 0) {
    throw 'The error manifest contains a row that is not marked error-like.'
}

$validKinds = @('SELECT', 'INSERT', 'UPDATE', 'DELETE', 'TRANSFORM', 'OTHER')
$invalidKinds = @($allRows | Where-Object { $_.Kind -notin $validKinds })
if ($invalidKinds.Count -ne 0) {
    throw "The full manifest contains $($invalidKinds.Count) invalid query kind values."
}

Write-Output 'Legacy catalogue validation passed.'

