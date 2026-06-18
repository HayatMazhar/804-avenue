param(
    [Parameter(Mandatory=$true)][string[]]$RelPaths,
    [string]$FtpHost    = "site67559.siteasp.net",
    [int]   $FtpPort    = 21,
    [string]$FtpUser    = "site67559",
    [string]$FtpPass    = "3Tb+f-B6?H9p",
    [string]$RemoteRoot = "wwwroot",
    [string]$LocalRoot  = ""
)

$base    = "ftp://${FtpHost}:${FtpPort}/${RemoteRoot}"
$userArg = "${FtpUser}:${FtpPass}"
if ([string]::IsNullOrWhiteSpace($LocalRoot)) {
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
    if ([string]::IsNullOrWhiteSpace($scriptDir)) { $scriptDir = (Get-Location).Path }
    $LocalRoot = Join-Path $scriptDir "publish"
}

Write-Host "Placing app_offline.htm..." -ForegroundColor Yellow
$tmpOffline = [System.IO.Path]::GetTempFileName()
Set-Content $tmpOffline -Value "<!DOCTYPE html><html><body><h1>Updating...</h1></body></html>" -Encoding UTF8
curl.exe --silent --user $userArg --upload-file $tmpOffline "$base/app_offline.htm"
Remove-Item $tmpOffline -Force
Start-Sleep -Seconds 2

$i = 0
foreach ($rel in $RelPaths) {
    $i++
    $local = Join-Path $LocalRoot $rel.Replace('/', '\')
    if (-not (Test-Path $local)) { Write-Warning "SKIP (missing): $rel"; continue }
    Write-Host "[$i/$($RelPaths.Count)] $rel" -ForegroundColor Cyan
    $out = curl.exe --silent --user $userArg --ftp-create-dirs --upload-file $local "$base/$rel" 2>&1
    if ($LASTEXITCODE -ne 0) { Write-Warning "FAILED [$LASTEXITCODE]: $out" }
}

Write-Host "Removing app_offline.htm..." -ForegroundColor Yellow
$ftpReq = [System.Net.FtpWebRequest]::Create("ftp://${FtpHost}:${FtpPort}/${RemoteRoot}/app_offline.htm")
$ftpReq.Credentials = New-Object System.Net.NetworkCredential($FtpUser, $FtpPass)
$ftpReq.Method = [System.Net.WebRequestMethods+Ftp]::DeleteFile
try { $resp = $ftpReq.GetResponse(); Write-Host "Deleted: $($resp.StatusDescription)" -ForegroundColor Green; $resp.Close() }
catch { Write-Warning "Delete failed: $($_.Exception.Message)" }

Write-Host "Done." -ForegroundColor Green
