# Step 3 Validation Runbook (Gateway + BFF Boundary + Load Balancing)

## Preconditions
- Docker Desktop is running.
- From repository root: `C:\ProjectAIBasedApi\web--api-project\web--Api-Project`

## 1) Start required services
```powershell
docker compose up -d --build apigateway catalogservice catalogservice2 mongo
```

## 2) Smoke check gateway health
```powershell
Invoke-WebRequest -Uri "http://localhost:5005/health" -Method GET
```
Expected: HTTP 200 with gateway status payload.

## 3) Load balancing proof (2+ instances)
```powershell
powershell -ExecutionPolicy Bypass -File scripts/validate-step3.ps1
```
Expected:
- Repeated HTTP 200 responses
- At least 2 distinct `X-Instance-Id` values

## 4) Resilience proof (one replica down)
```powershell
powershell -ExecutionPolicy Bypass -File scripts/validate-step3-resilience.ps1
```
Expected:
- `catalogservice2` is stopped
- Requests still return success through gateway

## 5) Optional direct quick check
```powershell
1..10 | ForEach-Object {
  $r = Invoke-WebRequest -Uri "http://localhost:5005/api/gift" -Method GET
  "[$_] status=$($r.StatusCode) instance=$($r.Headers['X-Instance-Id'])"
}
```

## 6) Evidence to capture for submission
- Screenshot of gateway health (Step 2).
- Output showing rotating `X-Instance-Id` (Step 3).
- Output showing successful requests after one replica is stopped (Step 4).

## 7) Teardown
```powershell
docker compose stop apigateway catalogservice catalogservice2 mongo
```

## Troubleshooting
- If Docker connection fails with `dockerDesktopLinuxEngine` pipe error:
  - Start Docker Desktop manually.
  - Wait until engine status is Running.
  - Re-run Step 1.
- If `X-Instance-Id` is missing:
  - Verify CatalogService middleware changes are present.
- If all calls hit one instance only:
  - Verify gateway catalog cluster has 2 destinations and RoundRobin policy.
