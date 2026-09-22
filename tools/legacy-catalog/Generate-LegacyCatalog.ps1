[CmdletBinding()]
param(
    [switch]$Check
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$expectedQueryCount = 898
$expectedErrorQueryCount = 69
$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$sourcePath = Join-Path $repositoryRoot 'docs\queries-sql.md'
$outputDirectory = Join-Path $repositoryRoot 'docs\generated'

function Get-QueryKind {
    param([Parameter(Mandatory = $true)][string]$Sql)

    $match = [regex]::Match(
        $Sql,
        '(?is)^\s*(?:PARAMETERS\b.*?;\s*)?(?<Kind>SELECT|INSERT|UPDATE|DELETE|TRANSFORM)\b'
    )

    if (-not $match.Success) {
        return 'OTHER'
    }

    return $match.Groups['Kind'].Value.ToUpperInvariant()
}

function Get-TargetDomain {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Sql
    )

    $searchText = "$Name`n$Sql"
    $rules = @(
        @{ Domain = 'reports-analysis'; Pattern = '(?i)AnalysisReportDefinition|ReportDefinition|ReportButton|ReportColumn|\btblAnaliza\b|\bANALIZA\b|\bIZVEST|\bREPORT' },
        @{ Domain = 'notices-payments'; Pattern = '(?i)\bNotice\b|PaymentOrder|OPOMEN|PLATNI' },
        @{ Domain = 'documents-email'; Pattern = '(?i)SentEmail|\bEmail\b|\bDocument\b|Attachment|\bMAIL' },
        @{ Domain = 'benefits-interest'; Pattern = '(?i)\bBenefit|\bInterest|KAMAT|POGOD' },
        @{ Domain = 'banking'; Pattern = '(?i)BankStatement|BankStatementLine|BankAccount|\bIZVOD|\bUPLAT' },
        @{ Domain = 'ledger'; Pattern = '(?i)LedgerEntry|JournalEntry|ChartOfAccounts|SubAccount|\bGK\b|\bNALOG|\bKONTO' },
        @{ Domain = 'suppliers'; Pattern = '(?i)SupplierInvoice|ExpenseInvoice|ExpenseTypeSubtype|ExpenseContract|RACUNDOB|DOBAVLJ|TROSKOV' },
        @{ Domain = 'billing'; Pattern = '(?i)InvoiceBatch|InvoiceLine|InvoiceUnit|\bInvoice\b|\bRACUN' },
        @{ Domain = 'master-data'; Pattern = '(?i)\bCompany\b|\bPartner\b|\bUnit\b|\bContract\b|\bAddress\b|BuildingEntrance|SKUPSTIN|KUPAC|OBJEKAT' },
        @{ Domain = 'platform-admin'; Pattern = '(?i)\bStaff\b|\bSetting\b|ShortList|Translation|SelectionBasket|\bEvent\b|\bImport' }
    )

    foreach ($rule in $rules) {
        if ($searchText -match $rule.Pattern) {
            return $rule.Domain
        }
    }

    return 'other'
}

function Get-PortStatus {
    param([Parameter(Mandatory = $true)][string]$Sql)

    # These dependencies need the missing Access UI/VBA source or a business mapping
    # for tables explicitly documented as absent from the current canonical model.
    $missingSourcePattern = @(
        '(?i)\bForms!',
        '(?i)\bReports!',
        '(?i)\bScreen\.',
        '(?i)\bTempVars!',
        '(?i)"\s*&\s*[A-Za-z_][A-Za-z0-9_]*\s*&\s*"',
        '(?i)\bExpenseInvoice\b',
        '(?i)\bExpenseTypeSubtype\b',
        '(?i)\bExpenseContract\b',
        '(?i)\bTroskovi_[A-Za-z0-9_]+'
    ) -join '|'

    if ($Sql -match $missingSourcePattern) {
        return 'blocked-by-missing-source'
    }

    return 'needs-port'
}

function ConvertTo-CsvField {
    param([AllowEmptyString()][string]$Value)

    if ($null -eq $Value) {
        $Value = ''
    }

    return '"' + $Value.Replace('"', '""') + '"'
}

function ConvertTo-MarkdownCell {
    param([AllowEmptyString()][string]$Value)

    if ($null -eq $Value) {
        return ''
    }

    return $Value.Replace('\', '\\').Replace('|', '\|').Replace("`r", '').Replace("`n", ' ')
}

function New-CsvContent {
    param([Parameter(Mandatory = $true)][object[]]$Rows)

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add('Name,Kind,ErrorLike,TargetDomain,Status,SourceReference')

    foreach ($row in $Rows) {
        $values = @(
            $row.Name,
            $row.Kind,
            $row.ErrorLike.ToString().ToLowerInvariant(),
            $row.TargetDomain,
            $row.Status,
            $row.SourceReference
        ) | ForEach-Object { ConvertTo-CsvField -Value $_ }
        $lines.Add(($values -join ','))
    }

    return ($lines -join "`n") + "`n"
}

function New-MarkdownContent {
    param(
        [Parameter(Mandatory = $true)][string]$Title,
        [Parameter(Mandatory = $true)][object[]]$Rows,
        [Parameter(Mandatory = $true)][string]$Description
    )

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add("# $Title")
    $lines.Add('')
    $lines.Add($Description)
    $lines.Add('')
    $lines.Add("Generated deterministically from ``docs/queries-sql.md``. Entries: **$($Rows.Count)**.")
    $lines.Add('')
    $lines.Add('| Name | Kind | Error-like | Target domain | Status | Source |')
    $lines.Add('|---|---|---:|---|---|---|')

    foreach ($row in $Rows) {
        $name = ConvertTo-MarkdownCell -Value $row.Name
        $kind = ConvertTo-MarkdownCell -Value $row.Kind
        $errorLike = $row.ErrorLike.ToString().ToLowerInvariant()
        $domain = ConvertTo-MarkdownCell -Value $row.TargetDomain
        $status = ConvertTo-MarkdownCell -Value $row.Status
        $lines.Add("| $name | $kind | $errorLike | $domain | $status | [L$($row.SourceLine)](../queries-sql.md#L$($row.SourceLine)) |")
    }

    return ($lines -join "`n") + "`n"
}

function Assert-ContentMatches {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$ExpectedContent
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Generated artifact is missing: $Path"
    }

    $actualContent = [System.IO.File]::ReadAllText($Path)
    if ($actualContent -ne $ExpectedContent) {
        throw "Generated artifact is stale: $Path. Run tools/legacy-catalog/Generate-LegacyCatalog.ps1."
    }
}

if (-not (Test-Path -LiteralPath $sourcePath)) {
    throw "Source file does not exist: $sourcePath"
}

$sourceText = [System.IO.File]::ReadAllText($sourcePath)
$queryPattern = '(?ms)^=====QUERY=====\r?\n(?<Name>[^\r\n]+)\r?\n-----SQL-----\r?\n(?<Sql>.*?)(?=^=====QUERY=====|\r?\n```\s*$)'
$matches = [regex]::Matches($sourceText, $queryPattern)

$rows = [System.Collections.Generic.List[object]]::new()
foreach ($match in $matches) {
    $name = $match.Groups['Name'].Value.Trim()
    $sql = $match.Groups['Sql'].Value.Trim()
    $prefix = $sourceText.Substring(0, $match.Groups['Name'].Index)
    $sourceLine = ([regex]::Matches($prefix, '\r\n|\n')).Count + 1
    $isErrorLike = $name.StartsWith('ERROR', [System.StringComparison]::OrdinalIgnoreCase)

    $rows.Add([pscustomobject][ordered]@{
        Name = $name
        Kind = Get-QueryKind -Sql $sql
        ErrorLike = $isErrorLike
        TargetDomain = Get-TargetDomain -Name $name -Sql $sql
        Status = Get-PortStatus -Sql $sql
        SourceReference = "docs/queries-sql.md:L$sourceLine"
        SourceLine = $sourceLine
    })
}

$errorRows = @($rows | Where-Object { $_.ErrorLike })
if ($rows.Count -ne $expectedQueryCount) {
    throw "Expected $expectedQueryCount queries, parsed $($rows.Count)."
}
if ($errorRows.Count -ne $expectedErrorQueryCount) {
    throw "Expected $expectedErrorQueryCount error-like queries, parsed $($errorRows.Count)."
}
if (@($rows | Where-Object { $_.Kind -eq 'OTHER' }).Count -gt 0) {
    throw 'At least one query could not be classified as SELECT/INSERT/UPDATE/DELETE/TRANSFORM.'
}

$artifacts = [ordered]@{
    (Join-Path $outputDirectory 'legacy-query-manifest.csv') = New-CsvContent -Rows $rows.ToArray()
    (Join-Path $outputDirectory 'legacy-query-manifest.md') = New-MarkdownContent -Title 'Legacy query manifest' -Rows $rows.ToArray() -Description 'Catalogue of all extracted Access QueryDef SQL blocks, with migration triage metadata.'
    (Join-Path $outputDirectory 'legacy-error-query-manifest.csv') = New-CsvContent -Rows $errorRows
    (Join-Path $outputDirectory 'legacy-error-query-manifest.md') = New-MarkdownContent -Title 'Legacy error-like query manifest' -Rows $errorRows -Description 'Subset whose query name begins with `ERROR` (case-insensitive); these queries encode consistency checks or their supporting/fix queries.'
}

if ($Check) {
    foreach ($artifact in $artifacts.GetEnumerator()) {
        Assert-ContentMatches -Path $artifact.Key -ExpectedContent $artifact.Value
    }
    Write-Output "Legacy catalogue is current: $($rows.Count) queries, $($errorRows.Count) error-like queries."
    exit 0
}

[System.IO.Directory]::CreateDirectory($outputDirectory) | Out-Null
$utf8WithoutBom = [System.Text.UTF8Encoding]::new($false)
foreach ($artifact in $artifacts.GetEnumerator()) {
    [System.IO.File]::WriteAllText($artifact.Key, $artifact.Value, $utf8WithoutBom)
}

$kindSummary = $rows | Group-Object -Property Kind | Sort-Object -Property Name | ForEach-Object { "$($_.Name)=$($_.Count)" }
$statusSummary = $rows | Group-Object -Property Status | Sort-Object -Property Name | ForEach-Object { "$($_.Name)=$($_.Count)" }
Write-Output "Generated legacy catalogue: $($rows.Count) queries, $($errorRows.Count) error-like queries."
Write-Output "Kinds: $($kindSummary -join ', ')"
Write-Output "Statuses: $($statusSummary -join ', ')"

