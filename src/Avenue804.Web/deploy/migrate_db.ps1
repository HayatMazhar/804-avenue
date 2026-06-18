param(
    [string]$SourceCs = "Server=tcp:db51128.public.databaseasp.net,1433;Database=db51128;User Id=db51128;Password=Tz3!+9Joq#4L;TrustServerCertificate=True;Encrypt=True;Connect Timeout=60",
    [string]$TargetCs = "Server=tcp:db53033.public.databaseasp.net,1433;Database=db53033;User Id=db53033;Password=6Wm!-o4EZ5_q;TrustServerCertificate=True;Encrypt=True;Connect Timeout=60"
)

# Tables to copy in dependency order (parents before children)
$tables = @(
    'AspNetRoles',
    'AspNetUsers',
    'AspNetUserRoles',
    'AspNetUserClaims',
    'AspNetUserLogins',
    'AspNetUserTokens',
    'AspNetRoleClaims',
    'LookupCategories',
    'LookupValues',
    'UaeAreas',
    'AreaGuides',
    'SiteSettings',
    'ContentBlocks',
    'Developers',
    'Agents',
    'NewsletterSubscribers',
    'Testimonials',
    'PortfolioProjects',
    'ProjectProgressItems',
    'PropertyListings',
    'PropertyListingInquiries',
    'PropertyRatings',
    'SavedProperties',
    'SavedSearches',
    'Inquiries',
    'AmcRequests',
    'OwnerListingRequests',
    'ServiceQuoteRequests',
    'ServiceReports',
    'MaintenanceTickets',
    'AdminNotifications'
)

$src = New-Object System.Data.SqlClient.SqlConnection($SourceCs)
$tgt = New-Object System.Data.SqlClient.SqlConnection($TargetCs)
$src.Open()
$tgt.Open()
Write-Host "Connected to both DBs." -ForegroundColor Green

function Exec-NonQuery($conn, $sql) {
    $c = $conn.CreateCommand(); $c.CommandText = $sql; $c.CommandTimeout = 120; [void]$c.ExecuteNonQuery()
}

Write-Host "`n[1] Disabling FK constraints on target..." -ForegroundColor Yellow
Exec-NonQuery $tgt "EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'"

Write-Host "[2] Deleting existing rows on target (in reverse order)..." -ForegroundColor Yellow
$reverse = $tables.Clone(); [Array]::Reverse($reverse)
foreach ($t in $reverse) {
    try { Exec-NonQuery $tgt "DELETE FROM [$t]"; Write-Host "  cleared $t" -ForegroundColor DarkGray }
    catch { Write-Warning ("  failed delete {0}: {1}" -f $t, $_.Exception.Message) }
}

Write-Host "`n[3] Copying data EU -> USA..." -ForegroundColor Cyan
foreach ($t in $tables) {
    $srcCmd = $src.CreateCommand()
    $srcCmd.CommandText = "SELECT * FROM [$t]"
    $srcCmd.CommandTimeout = 120
    $reader = $srcCmd.ExecuteReader()

    # Detect identity column
    $hasIdentity = $false
    try {
        $idCmd = $tgt.CreateCommand()
        $idCmd.CommandText = "SELECT COUNT(*) FROM sys.identity_columns WHERE OBJECT_ID = OBJECT_ID('dbo.[$t]')"
        if ([int]$idCmd.ExecuteScalar() -gt 0) { $hasIdentity = $true }
    } catch {}

    $opts = if ($hasIdentity) {
        [System.Data.SqlClient.SqlBulkCopyOptions]::KeepIdentity -bor [System.Data.SqlClient.SqlBulkCopyOptions]::KeepNulls
    } else {
        [System.Data.SqlClient.SqlBulkCopyOptions]::KeepNulls
    }

    $bulk = New-Object System.Data.SqlClient.SqlBulkCopy($tgt, $opts, $null)
    $bulk.DestinationTableName = "[$t]"
    $bulk.BulkCopyTimeout      = 180
    $bulk.BatchSize            = 500
    for ($i = 0; $i -lt $reader.FieldCount; $i++) {
        $col = $reader.GetName($i)
        [void]$bulk.ColumnMappings.Add($col, $col)
    }

    try {
        $bulk.WriteToServer($reader)
        # RowsCopied not always available; fallback to a COUNT
        $reader.Close()
        $countCmd = $tgt.CreateCommand(); $countCmd.CommandText = "SELECT COUNT(*) FROM [$t]"
        $copied = $countCmd.ExecuteScalar()
        Write-Host ("  {0,-30} {1,6} rows" -f $t, $copied) -ForegroundColor Green
    } catch {
        Write-Warning ("  failed copy {0}: {1}" -f $t, $_.Exception.Message)
        if (-not $reader.IsClosed) { $reader.Close() }
    } finally {
        $bulk.Close()
    }
}

Write-Host "`n[4] Re-enabling FK constraints..." -ForegroundColor Yellow
try { Exec-NonQuery $tgt "EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'" }
catch { Write-Warning "Some constraints could not be re-enabled: $($_.Exception.Message)" }

$src.Close(); $tgt.Close()
Write-Host "`nMigration complete." -ForegroundColor Green
