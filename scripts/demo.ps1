param(
    [string]$ApiUrl = $(if ($env:API_URL) { $env:API_URL } else { "http://localhost:5080" }),
    [string]$SimulatorUrl = $(if ($env:SIMULATOR_URL) { $env:SIMULATOR_URL } else { "http://localhost:5090" }),
    [string]$Base = "USD",
    [string]$Target = "EUR"
)

$ErrorActionPreference = "Stop"

function Write-Step([string]$Title) {
    Write-Host ""
    Write-Host "=== $Title ===" -ForegroundColor Cyan
}

function Set-ProviderMode([string]$Mode) {
    $body = @{ mode = $Mode } | ConvertTo-Json
    $status = Invoke-RestMethod -Method Put -Uri "$SimulatorUrl/admin/behavior" -Body $body -ContentType "application/json"
    Write-Host "Provider mode -> $($status.mode)" -ForegroundColor Magenta
}

function Get-ProviderRequests {
    (Invoke-RestMethod -Uri "$SimulatorUrl/admin/behavior").requestsReceived
}

function Get-Circuit {
    Invoke-RestMethod -Uri "$ApiUrl/api/circuit-breaker"
}

function Invoke-Api([string]$Uri) {
    try {
        $response = Invoke-WebRequest -Uri $Uri -UseBasicParsing
        return [pscustomobject]@{ StatusCode = [int]$response.StatusCode; Content = $response.Content }
    }
    catch [System.Net.WebException] {
        $failed = $_.Exception.Response
        $reader = New-Object System.IO.StreamReader($failed.GetResponseStream())
        return [pscustomobject]@{ StatusCode = [int]$failed.StatusCode; Content = $reader.ReadToEnd() }
    }
}

function Invoke-Rate([int]$Times) {
    for ($i = 1; $i -le $Times; $i++) {
        $response = Invoke-Api "$ApiUrl/api/exchange-rates?base=$Base&target=$Target"
        $result = $response.Content | ConvertFrom-Json
        $color = switch ($result.source) { "Live" { "Green" } "Fallback" { "Yellow" } default { "Red" } }
        $reason = if ($result.failureReason) { " reason=$($result.failureReason)" } else { "" }
        Write-Host ("  #{0,-2} HTTP {1} source={2,-11} rate={3,-10} circuit={4}{5}" -f $i, $response.StatusCode, $result.source, $result.rate, $result.circuitState, $reason) -ForegroundColor $color
    }
}

$policy = (Get-Circuit).policy
$breakSeconds = [TimeSpan]::Parse($policy.breakDuration).TotalSeconds

Invoke-RestMethod -Method Post -Uri "$ApiUrl/api/circuit-breaker/reset" | Out-Null

Write-Step "1. Healthy provider: circuit CLOSED, every call reaches the provider"
Set-ProviderMode "Healthy"
Invoke-Rate 3

Write-Step "2. Provider starts failing: failures are counted until the threshold opens the circuit"
Set-ProviderMode "Failing"
Invoke-Rate 4

Write-Step "3. Circuit OPEN: calls fail fast, the provider is NOT called, fallback is served"
$before = Get-ProviderRequests
Invoke-Rate 5
$after = Get-ProviderRequests
Write-Host "Requests received by the provider during this step: $($after - $before)" -ForegroundColor Magenta

Write-Step "4. Provider recovers, waiting ${breakSeconds}s of break duration"
Set-ProviderMode "Healthy"
Start-Sleep -Seconds ($breakSeconds + 1)

Write-Step "5. HALF-OPEN: a trial call reaches the provider and closes the circuit"
Invoke-Rate 2

Write-Step "Circuit events (newest first)"
(Get-Circuit).recentEvents | Select-Object -First 15 | ForEach-Object {
    Write-Host ("  {0:HH:mm:ss} {1,-18} [{2}] {3}" -f ([DateTimeOffset]$_.occurredAt).LocalDateTime, $_.type, $_.stateAfter, $_.detail)
}

Write-Step "Metrics"
(Get-Circuit).metrics | Format-List
