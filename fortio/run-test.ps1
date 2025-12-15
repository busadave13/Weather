<#
.SYNOPSIS
    Runs Fortio load tests against the Weather API with constant QPS.

.DESCRIPTION
    Wrapper script for Fortio load testing with configurable QPS and duration.
    Uses Fortio's constant QPS mode for precise request rate control.
    Provides pretty-printed results with load shedding statistics.

.PARAMETER Qps
    Queries (requests) per second to generate. Default: 100

.PARAMETER Duration
    Test duration (e.g., 30s, 1m, 5m). Default: 30s

.PARAMETER Url
    Target URL to test. Default: http://localhost:5081/api/weather

.PARAMETER MockeryMocks
    Value for X-Mockery-Mocks header. Default: windsensor/success, temperaturesensor/success, precipitationsensor/success

.PARAMETER Connections
    Number of concurrent connections. Default: 8

.PARAMETER UseDocker
    Run Fortio via Docker instead of local installation. Default: true

.EXAMPLE
    .\run-test.ps1 -Qps 50 -Duration 10s

.EXAMPLE
    .\run-test.ps1 -Qps 200 -Duration 1m
#>

param(
    [Parameter(HelpMessage = "Queries per second to generate")]
    [int]$Qps = 100,

    [Parameter(HelpMessage = "Test duration (e.g., 30s, 1m, 5m)")]
    [string]$Duration = "30s",

    [Parameter(HelpMessage = "Target URL to test")]
    [string]$Url = "http://localhost:5081/api/weather",

    [Parameter(HelpMessage = "X-Mockery-Mocks header value")]
    [string]$MockeryMocks = "windsensor/success, temperaturesensor/success, precipitationsensor/success",

    [Parameter(HelpMessage = "Number of concurrent connections")]
    [int]$Connections = 8,

    [Parameter(HelpMessage = "Run Fortio via Docker (default: true)")]
    [switch]$UseDocker = $true
)

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ResultFile = Join-Path $ScriptDir "fortio-result.json"

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "     Fortio Load Test - Weather API" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Target URL:    $Url"
Write-Host "QPS:           $Qps requests/second"
Write-Host "Duration:      $Duration"
Write-Host "Connections:   $Connections"
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Running load test..." -ForegroundColor Yellow
Write-Host ""

try {
    $TempOutputFile = Join-Path $ScriptDir "fortio-output.txt"
    
    if ($UseDocker) {
        $DockerUrl = $Url -replace "localhost", "host.docker.internal"
        
        # Run fortio and save output to file
        docker run --rm `
            fortio/fortio load `
            -qps $Qps `
            -t $Duration `
            -c $Connections `
            -payload "" `
            -json - `
            -H "X-Mockery-Mocks: $MockeryMocks" `
            $DockerUrl 2>&1 | Out-File -FilePath $TempOutputFile -Encoding utf8
    } else {
        if (-not (Get-Command fortio -ErrorAction SilentlyContinue)) {
            Write-Host "Fortio is not installed." -ForegroundColor Red
            exit 1
        }

        fortio load `
            -qps $Qps `
            -t $Duration `
            -c $Connections `
            -payload "" `
            -json - `
            -H "X-Mockery-Mocks: $MockeryMocks" `
            $Url 2>&1 | Out-File -FilePath $TempOutputFile -Encoding utf8
    }

    # Read file as single string
    $allContent = Get-Content -Path $TempOutputFile -Raw
    
    # Find the main JSON block containing "RunType"
    # Look for { at start of line, then find matching } by counting all braces
    $lines = $allContent -split "`r?`n"
    $jsonStartIndex = -1
    $jsonEndIndex = -1
    
    # First, find the line that starts with just "{"
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i] -match '^\s*\{\s*$') {
            # Check if next line contains "RunType" - this is our JSON block
            if ($i + 1 -lt $lines.Count -and $lines[$i + 1] -match '"RunType"') {
                $jsonStartIndex = $i
                break
            }
        }
    }
    
    if ($jsonStartIndex -ge 0) {
        # Now count braces to find the end
        $braceCount = 0
        for ($i = $jsonStartIndex; $i -lt $lines.Count; $i++) {
            $line = $lines[$i]
            # Count all { and } in the line
            $opens = ([regex]::Matches($line, '\{')).Count
            $closes = ([regex]::Matches($line, '\}')).Count
            $braceCount += $opens - $closes
            
            if ($braceCount -eq 0 -and $i -gt $jsonStartIndex) {
                $jsonEndIndex = $i
                break
            }
        }
    }
    
    if ($jsonStartIndex -ge 0 -and $jsonEndIndex -gt $jsonStartIndex) {
        $jsonLines = $lines[$jsonStartIndex..$jsonEndIndex]
        $jsonContent = $jsonLines -join "`n"
        $result = $jsonContent | ConvertFrom-Json
        
        # Extract metrics
        $totalRequests = 0
        $code200 = 0
        $code503 = 0
        $otherErrors = 0
        
        foreach ($code in $result.RetCodes.PSObject.Properties) {
            $count = [int]$code.Value
            $totalRequests += $count
            
            switch ($code.Name) {
                "200" { $code200 = $count }
                "503" { $code503 = $count }
                default { $otherErrors += $count }
            }
        }
        
        $actualQps = [math]::Round($result.ActualQPS, 1)
        $avgLatency = if ($result.DurationHistogram.Avg) { [math]::Round($result.DurationHistogram.Avg * 1000, 2) } else { 0 }
        $maxLatency = if ($result.DurationHistogram.Max) { [math]::Round($result.DurationHistogram.Max * 1000, 2) } else { 0 }
        
        # Safely extract percentiles
        $p50 = 0; $p90 = 0; $p99 = 0
        if ($result.DurationHistogram.Percentiles) {
            foreach ($pct in $result.DurationHistogram.Percentiles) {
                $val = [math]::Round($pct.Value * 1000, 2)
                switch ([int]$pct.Percentile) {
                    50 { $p50 = $val }
                    90 { $p90 = $val }
                    99 { $p99 = $val }
                }
            }
        }
        
        $pct200 = if ($totalRequests -gt 0) { [math]::Round(($code200 / $totalRequests) * 100, 1) } else { 0 }
        $pct503 = if ($totalRequests -gt 0) { [math]::Round(($code503 / $totalRequests) * 100, 1) } else { 0 }
        $pctErrors = if ($totalRequests -gt 0) { [math]::Round(($otherErrors / $totalRequests) * 100, 1) } else { 0 }
        
        # Print pretty results
        Write-Host ""
        Write-Host "============================================" -ForegroundColor Green
        Write-Host "           LOAD TEST RESULTS" -ForegroundColor Green
        Write-Host "============================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "  Total Requests:    $totalRequests"
        Write-Host "  Actual QPS:        $actualQps"
        Write-Host ""
        Write-Host "  HTTP Status Codes:" -ForegroundColor White
        
        if ($code200 -gt 0) {
            Write-Host "    $(([char]0x2713)) 200 OK:        $code200 ($pct200%)" -ForegroundColor Green
        }
        if ($code503 -gt 0) {
            Write-Host "    $(([char]0x26A0)) 503 Shed:      $code503 ($pct503%)" -ForegroundColor Yellow
        }
        if ($otherErrors -gt 0) {
            Write-Host "    $(([char]0x2717)) Errors:       $otherErrors ($pctErrors%)" -ForegroundColor Red
        }
        
        Write-Host ""
        Write-Host "  Latency (ms):" -ForegroundColor White
        Write-Host "    Avg:    $avgLatency"
        Write-Host "    P50:    $p50"
        Write-Host "    P90:    $p90"
        Write-Host "    P99:    $p99"
        Write-Host "    Max:    $maxLatency"
        Write-Host ""
        
        # Load shedding summary
        if ($code503 -gt 0) {
            Write-Host "  ============================================" -ForegroundColor Yellow
            Write-Host "  LOAD SHEDDING:  $pct503% of requests rejected" -ForegroundColor Yellow
            Write-Host "  ============================================" -ForegroundColor Yellow
        } else {
            Write-Host "  ============================================" -ForegroundColor Green
            Write-Host "  NO LOAD SHEDDING - All requests succeeded" -ForegroundColor Green
            Write-Host "  ============================================" -ForegroundColor Green
        }
        Write-Host ""
    } else {
        Write-Host "Could not parse Fortio output" -ForegroundColor Red
        Write-Host "JSON start index: $jsonStartIndex, JSON end index: $jsonEndIndex" -ForegroundColor Red
        if (Test-Path $TempOutputFile) {
            Write-Host "Output file exists. First 10 lines:" -ForegroundColor Yellow
            Get-Content -Path $TempOutputFile | Select-Object -First 10
        }
    }
}
finally {
    if (Test-Path $TempOutputFile) {
        Remove-Item $TempOutputFile -Force
    }
    if (Test-Path $ResultFile) {
        Remove-Item $ResultFile -Force
    }
}
