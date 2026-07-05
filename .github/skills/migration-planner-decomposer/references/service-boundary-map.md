# Service Boundary Map

This document defines the first implementation slice for the migration plan: service boundaries and ownership rules.

## Target services

### 1. Authentication/Users
Owns:
- account and login flow
- JWT/token generation
- user profile and user persistence
- user-related validation and authorization decisions

Likely source files:
- `WebApplication2/Controllers/AccountController.cs`
- `WebApplication2/BLL/UserServiceBLL.cs`
- `WebApplication2/DAL/UserDAL.cs`

### 2. Catalog/Gifts/Categories/Donors
Owns:
- gift management
- category management
- donor management
- catalog read/write operations

Likely source files:
- `WebApplication2/Controllers/GiftController.cs`
- `WebApplication2/Controllers/CategoryController.cs`
- `WebApplication2/Controllers/DonorController.cs`
- `WebApplication2/BLL/GiftServiceBLL.cs`
- `WebApplication2/BLL/CategoryServiceBLL.cs`
- `WebApplication2/BLL/DonorServiceBLL.cs`
- `WebApplication2/DAL/GiftDAL.cs`
- `WebApplication2/DAL/CategoryDAL.cs`
- `WebApplication2/DAL/DonorDAL.cs`

### 3. Order/Ticket/Raffle/Winner
Owns:
- ticket purchase flow
- raffle execution
- winner selection
- order orchestration and outcome tracking

Likely source files:
- `WebApplication2/Controllers/TicketController.cs`
- `WebApplication2/Controllers/RaffleController.cs`
- `WebApplication2/Controllers/WinnerController.cs`
- `WebApplication2/BLL/OrderServiceBLL.cs`
- `WebApplication2/BLL/RaffleSarviceBLL.cs`
- `WebApplication2/BLL/WinnerBLL.cs`
- `WebApplication2/DAL/OrderDAL.cs`
- `WebApplication2/DAL/TicketDal.cs`
- `WebApplication2/DAL/WinnerDal.cs`

### 4. Notifications/Email/Cache
Owns:
- email delivery
- cache behavior
- notification dispatch
- notification formatting and retry behavior

Likely source files:
- `WebApplication2/BLL/EmailService.cs`
- cache-related code once identified

## Boundary rules

- Each service owns its own data and business rules.
- Services must not call another service’s DAL or DbContext directly.
- Cross-service communication must use HTTP, events, or messages.
- Shared entity models are not service contracts.
- A service may expose only the DTOs or events required by consumers.
- Preserve existing endpoint behavior, DTO shape, and token/claim contents unless a contract change is explicitly planned and verified.

## First implementation slice

Implement this first:
1. map all controllers, BLL classes, DAL classes, and shared models to one of the four target services
2. mark any class that currently crosses boundaries
3. identify all cross-service dependencies that must become contracts
4. list the code and data cut lines for the first extraction phase

## Immediate follow-up

After this map is complete, the next step is to use it to extract the Authentication/Users service first because it has the lowest inbound coupling.