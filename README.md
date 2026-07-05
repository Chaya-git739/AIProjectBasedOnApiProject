# WebApplication2 API

## Overview
WebApplication2 is an ASP.NET Core Web API project built with .NET 9.0. It provides a raffle/auction-style application backend with:
- Entity Framework Core for SQL Server data access
- JWT authentication
- Redis cache support
- Swagger / OpenAPI documentation
- Email notifications via MailKit
- AutoMapper for DTO mapping

## Repository Structure
- `WebApplication2/`
  - Main ASP.NET Core project
  - `Program.cs` contains startup and dependency injection configuration
  - `Controllers/` contains API controllers
  - `DAL/` contains Entity Framework data access code and repository classes
  - `BLL/` contains business logic services
  - `Models/` contains entity and DTO models
  - `Middlewares/` contains custom middleware components
  - `Mappings/` contains AutoMapper profiles
  - `wwwroot/` contains static assets
- `docker-compose.yml` defines local development containers for API, SQL Server, and Redis
- `WebApplication2.sln` is the Visual Studio solution file for the API

## Pruned Cleanup
Removed files that were not part of the active solution or required for the API:
- Root-level helper / duplicate C# files
- Unrelated `PasswordHasher` helper project and script
- Node `package-lock.json` files
- Build output folders `WebApplication2/bin` and `WebApplication2/obj`

## Getting Started
### Prerequisites
- .NET 9 SDK
- Docker Desktop (for Docker Compose)
- SQL Server credentials configured in `docker-compose.yml`

### Run locally with Docker Compose
From the repository root:
```powershell
docker-compose up --build
```

The API will be available at `http://locFalhost:5226
`.

### Run from Visual Studio
1. Open `WebApplication2.sln`
2. Set `WebApplication2` as the startup project
3. Build and run

## Configuration
Key configuration values are in `WebApplication2/appsettings.json`:
- `ConnectionStrings:DefaultConnection` — SQL Server database connection string
- `Jwt:SecretKey` — symmetric JWT signing key
- `Redis:Connection` — Redis endpoint
- `EmailSettings` — SMTP server and sender settings

## Development Notes
- Swagger UI is enabled by default and is available at `/swagger`
- The project automatically applies pending EF Core migrations at startup
- `WebApplication2/DAL/StoreContext.cs` is the active database context used by the app

## Recommended Next Steps
- Secure `Jwt:SecretKey` and email credentials outside source control
- Add unit and integration tests for controllers and services
- Remove hard-coded development values from production deployment
- Add a proper documentation folder for future architecture notes
