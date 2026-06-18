param(
    [string]$FtpHost    = "site67559.siteasp.net",
    [int]   $FtpPort    = 21,
    [string]$FtpUser    = "site67559",
    [string]$FtpPass    = "3Tb+f-B6?H9p",
    [string]$RemoteRoot = "wwwroot",
    [string]$LocalRoot  = "$PSScriptRoot\publish"
)

$base    = "ftp://${FtpHost}:${FtpPort}/${RemoteRoot}"
$userArg = "${FtpUser}:${FtpPass}"
$created = @{}

function Ftp-MkDir {
    param([string]$remotePath)
    if ($created.ContainsKey($remotePath)) { return }
    curl.exe --silent --user $userArg --ftp-create-dirs `
             --quote "MKD $remotePath" `
             "$base/" 2>&1 | Out-Null
    $created[$remotePath] = $true
}

function Ensure-Path {
    param([string]$relDir)
    # relDir like "runtimes/win-x64/native"
    $parts = $relDir.Split('/')
    $cur   = ""
    foreach ($p in $parts) {
        if ($p -eq "") { continue }
        $cur = if ($cur -eq "") { $p } else { "$cur/$p" }
        Ftp-MkDir -remotePath $cur
    }
}

# ── Step 1: app_offline.htm to unlock DLLs ──────────────────────────────
Write-Host "Placing app_offline.htm (takes app pool offline)..." -ForegroundColor Yellow
$tmpOffline = [System.IO.Path]::GetTempFileName()
Set-Content $tmpOffline -Value "<!DOCTYPE html><html><body><h1>Deploying...</h1></body></html>" -Encoding UTF8
curl.exe --silent --user $userArg --upload-file $tmpOffline "$base/app_offline.htm"
if ($LASTEXITCODE -ne 0) { Write-Warning "app_offline.htm upload may have failed (exit $LASTEXITCODE)" }
Remove-Item $tmpOffline -Force
Start-Sleep -Seconds 4

# ── Step 2: Upload all files ─────────────────────────────────────────────
$files = Get-ChildItem -Path $LocalRoot -Recurse -File
$total = $files.Count
$i     = 0

Write-Host "Uploading $total files..." -ForegroundColor Cyan

foreach ($file in $files) {
    $i++
    $rel    = $file.FullName.Substring($LocalRoot.TrimEnd('\').Length).TrimStart('\').Replace('\', '/')
    $dir    = ($rel -split '/')[0..([Math]::Max(0, ($rel -split '/').Count - 2))] -join '/'
    $remote = "$base/$rel"

    if ($dir -and $dir -ne $rel) { Ensure-Path -relDir $dir }

    Write-Progress -Activity "FTP Upload" -Status "[$i/$total] $rel" -PercentComplete (($i / $total) * 100)

    $out = curl.exe --silent --user $userArg --ftp-create-dirs --upload-file $file.FullName $remote 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "FAILED [$LASTEXITCODE] $rel : $out"
    }
}

Write-Progress -Activity "FTP Upload" -Completed

# ── Step 3: Delete app_offline.htm ──────────────────────────────────────
Write-Host "Removing app_offline.htm (bringing site back online)..." -ForegroundColor Yellow
curl.exe --silent --user $userArg --quote "DELE app_offline.htm" "$base/" 2>&1 | Out-Null

Write-Host "`nDone. $i files uploaded." -ForegroundColor Green
Write-Host "Site: https://804avenue.runasp.net" -ForegroundColor Cyan
