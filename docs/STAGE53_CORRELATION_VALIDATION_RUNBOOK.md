# Stage 5.3 Validation Runbook (Correlation ID Propagation)

## Scope
Validate that `x-correlation-id` is:
- accepted by the gateway
- echoed back to the client
- propagated across downstream services
- available for centralized logs in Seq

## Preconditions
- Docker Desktop is running.
- Run commands from repository root:
  `C:\ProjectAIBasedApi\web--api-project\web--Api-Project`

## 1) Start/refresh required services
```powershell
docker compose up -d --build apigateway authenticationservice orderservice catalogservice catalogservice2 notificationservice seq
```

## 2) Verify container health
```powershell
docker compose ps
```
Expected:
- `apigateway`, `authenticationservice`, `orderservice`, `catalogservice`, `catalogservice2`, `notificationservice` are `healthy`.

## 3) Gateway echo check with custom correlation id
```powershell
curl.exe -s -D - -o NUL -H "x-correlation-id: stage53-e2e-001" http://localhost:5005/api/gift
```
Expected headers include exactly:
- `x-correlation-id: stage53-e2e-001`
- `X-Instance-Id: <container-id>`

Pass criteria:
- Correlation header appears once and value matches request value.

## 4) Auto-generation check when header is missing
```powershell
curl.exe -s -D - -o NUL http://localhost:5005/api/gift
```
Expected:
- Response contains `x-correlation-id` with a non-empty generated value.

## 5) Multi-request consistency spot check
```powershell
1..5 | ForEach-Object {
  curl.exe -s -D - -o NUL -H "x-correlation-id: stage53-batch-00$_" http://localhost:5005/api/gift |
    Select-String "x-correlation-id|X-Instance-Id"
}
```
Expected:
- Every response includes the same `x-correlation-id` value sent in its request.
- `X-Instance-Id` may rotate because catalog is round-robin behind gateway.

## 6) Downstream propagation check via Seq
1. Open Seq UI: `http://localhost:5341`
2. Filter logs by one test correlation id from Step 3, for example:
   - `CorrelationId = 'stage53-e2e-001'`
3. Confirm entries are visible from more than one service in the request path (for example gateway and catalog).

Pass criteria:
- Same `CorrelationId` appears in logs across involved services.

## 6.1) Full saga trace example
Use a checkout request so the correlation ID crosses HTTP and RabbitMQ, not only a read request.

Example request:
```powershell
curl.exe -X POST "http://localhost:5005/orders/api/order/checkout" ^
  -H "Content-Type: application/json" ^
  -H "Authorization: Bearer <token>" ^
  -H "x-correlation-id: saga-order-1001" ^
  -d "{\"orderItems\":[{\"giftId\":1,\"quantity\":1}],\"isDraft\":false}"
```

Then filter Seq with:
- `CorrelationId = 'saga-order-1001'`

Expected trace for a successful saga:
- ApiGateway request log with `CorrelationId=saga-order-1001`
- OrderService log: `OrderPlaced event published for order 1001`
- CatalogService log: `[Saga] Received OrderPlaced for OrderId=1001 CorrelationId=saga-order-1001`
- CatalogService log: `[Saga] Published inventory.reserved for OrderId=1001`
- OrderService log: `[Saga] Received inventory result for OrderId=1001 Success=True CorrelationId=saga-order-1001`
- OrderService log: `[Saga] Order confirmed for OrderId=1001`
- OrderService log: `[Saga] Published order.status-changed for OrderId=1001 Status=Confirmed`
- NotificationService log: `[Saga] Handling order.status-changed for OrderId=1001 Status=Confirmed CorrelationId=saga-order-1001`
- NotificationService log: `[Saga] Notification sent for OrderId=1001 Status=Confirmed`

Broker validation expectation:
- The RabbitMQ messages for `order.placed`, `inventory.reserved`, and `order.status-changed` must carry `properties.correlationId = saga-order-1001` and header `x-correlation-id = saga-order-1001`.

Pass criteria:
- One value, `saga-order-1001`, is visible in every service involved in the saga.
- The same value is preserved in RabbitMQ message metadata between publish and consume steps.

## 7) Evidence to capture for submission
- `docker compose ps` showing healthy services.
- HTTP headers from Step 3 and Step 4.
- Seq screenshot showing the full successful checkout saga filtered by one correlation ID.
- Seq screenshot showing same `CorrelationId` across multiple services.

## 8) Teardown (optional)
```powershell
docker compose stop apigateway authenticationservice orderservice catalogservice catalogservice2 notificationservice seq
```

## Troubleshooting
- If PowerShell reports `run` command not found:
  - Do not prefix commands with `run`; execute commands directly.
- If `x-correlation-id` is duplicated:
  - Rebuild/restart gateway and re-check Stage 5.3 middleware in `ApiGateway/Program.cs`.
- If a service is not healthy:
  - `docker compose logs <service-name> --tail 100`
- If no logs appear in Seq:
  - Verify `Serilog__SeqUrl=http://seq:80` in compose service environment and ensure `seq` container is running.
