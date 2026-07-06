param(
    [string]$GatewayBaseUrl = "http://localhost:5005",
    [int]$Iterations = 20
)

$ErrorActionPreference = "Stop"

Write-Host "Step 3 validation: load-balancing proof via gateway"
Write-Host "Gateway: $GatewayBaseUrl"

$targetUrl = "$GatewayBaseUrl/api/gift"
$headers = @{ "x-correlation-id" = [guid]::NewGuid().ToString() }

$instances = @()

for ($i = 1; $i -le $Iterations; $i++) {
    try {
        $response = Invoke-WebRequest -Uri $targetUrl -Headers $headers -Method GET
        $instanceId = $response.Headers["X-Instance-Id"]
        if (-not $instanceId) { $instanceId = "<missing>" }
        $instances += $instanceId
        Write-Host ("[{0}] status={1} instance={2}" -f $i, [int]$response.StatusCode, $instanceId)
    }
    catch {
        Write-Host ("[{0}] request failed: {1}" -f $i, $_.Exception.Message)
    }
}

$unique = $instances | Sort-Object -Unique
Write-Host ""
Write-Host "Unique instance ids:"
$unique | ForEach-Object { Write-Host " - $_" }

if ($unique.Count -ge 2) {
    Write-Host ""
    Write-Host "PASS: Load balancing is visible (2+ distinct instance ids)."
    exit 0
}

Write-Host ""
Write-Host "WARN: Could not prove 2+ distinct instance ids."
exit 1
