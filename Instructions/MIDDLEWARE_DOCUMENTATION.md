# Middleware System (Exception Handling + Logging)

## Overview
This project uses two custom middlewares to handle requests globally:

- Exception Handling Middleware
- Request/Response Logging Middleware

Together they provide:
- Centralized error handling
- Consistent API responses
- Full request/response tracing
- Better debugging and monitoring

---

## 1. ExceptionHandlingMiddleware

Handles all unhandled exceptions in the application and converts them into consistent HTTP responses.

### Supported Exceptions

| Exception Type | Status Code |
|----------------|------------|
| BusinessException | 409 Conflict |
| ArgumentException | 400 Bad Request |
| UnauthorizedAccessException | 401 Unauthorized |
| KeyNotFoundException | 404 Not Found |
| Other Exceptions | 500 Internal Server Error |

### Behavior
- Captures all unhandled exceptions
- Logs the error using ILogger
- Returns structured JSON response:

```json id="ex-json"
{
  "statusCode": 400,
  "message": "Error message",
  "type": "ExceptionType"
}