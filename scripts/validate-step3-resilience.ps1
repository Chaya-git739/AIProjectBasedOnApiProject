param(
    [string]$GatewayBaseUrl = "http://localhost:5005"
)

$ErrorActionPreference = "Stop"

Write-Host "Step 3 resilience validation: one catalog replica down"

Write-Host "Starting catalogservice2 to ensure both replicas are available before test..."
docker compose up -d catalogservice2 | Out-Host

Write-Host "Stopping catalogservice2..."
docker compose stop catalogservice2 | Out-Host

$targetUrl = "$GatewayBaseUrl/api/gift"
$headers = @{ "x-correlation-id" = [guid]::NewGuid().ToString() }

# Warm-up probes: allow gateway/backend health state to converge after replica stop.
$warmupPassed = $false
for ($i = 1; $i -le 8; $i++) {
    try {
        $null = Invoke-WebRequest -Uri $targetUrl -Headers $headers -Method GET
        $warmupPassed = $true
        break
    }
    catch {
        if ($i -eq 8) {
            Write-Host ("Warm-up failed after {0} attempts: {1}" -f $i, $_.Exception.Message)
            exit 1
        }
    }
}

if (-not $warmupPassed) {
    Write-Host "Warm-up did not converge."
    exit 1
}

for ($i = 1; $i -le 10; $i++) {
    try {
        $response = Invoke-WebRequest -Uri $targetUrl -Headers $headers -Method GET
        $instanceId = $response.Headers["X-Instance-Id"]
        if (-not $instanceId) { $instanceId = "<missing>" }
        Write-Host ("[{0}] status={1} instance={2}" -f $i, [int]$response.StatusCode, $instanceId)
    }
    catch {
        Write-Host ("[{0}] request failed: {1}" -f $i, $_.Exception.Message)
        exit 1
    }
}

Write-Host ""
Write-Host "PASS: Requests are still served after one replica stopped."
exit 0
