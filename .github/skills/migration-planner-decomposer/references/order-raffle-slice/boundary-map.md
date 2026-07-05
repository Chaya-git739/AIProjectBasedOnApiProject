# Order / Ticket / Raffle / Winner Boundary Map

This document defines the next implementation slice in the migration plan: the Order/Ticket/Raffle/Winner service boundary and its ownership rules.

## Target service

### Order/Ticket/Raffle/Winner
Owns:
- ticket purchase and order checkout
- order confirmation and history
- raffle execution
- winner selection and winner persistence
- user-facing order and winner read models
- notification trigger points for winner events

Likely source files:
- `WebApplication2/Controllers/OrderController.cs`
- `WebApplication2/Controllers/RaffleController.cs`
- `WebApplication2/Controllers/WinnerController.cs`
- `WebApplication2/BLL/OrderServiceBLL.cs`
- `WebApplication2/BLL/RaffleSarviceBLL.cs`
- `WebApplication2/BLL/WinnerBLL.cs`
- `WebApplication2/DAL/OrderDAL.cs`
- `WebApplication2/DAL/TicketDal.cs`
- `WebApplication2/DAL/WinnerDal.cs`
- `WebApplication2/Models/OrderModel.cs`
- `WebApplication2/Models/OrderTicketModel.cs`
- `WebApplication2/Models/WinnerModel.cs`
- `WebApplication2/Models/DTO/OrderDTO.cs`
- `WebApplication2/Models/DTO/TicketDTO.cs`
- `WebApplication2/Models/DTO/WinnerDTO.cs`
- `WebApplication2/Models/DTO/PurchaserDetailsDto.cs`

## Boundary rules

- The service must not call Authentication/Users DAL or BLL directly.
- User identity should come from the JWT claim or a contract-style user reference.
- The service may keep a `UserId` reference, but not user persistence ownership.
- Cross-service calls to Catalog or Notifications must use explicit contracts.
- Shared user models should not be treated as the source of truth for this service.
- Existing DTOs and response shapes should stay stable unless a contract change is approved.

## Current coupling to remove or replace

- `OrderController` reads the user id from the JWT and sends it into the order workflow. This is acceptable as a transitional boundary.
- `OrderDAL` still projects `User.Name` and `User.Email` from the shared user entity. This is a transitional read dependency that should later become a contract-only user snapshot or resolved read model.
- `RaffleSarviceBLL` currently relies on order data to build the raffle pool and uses winner email notification through the notification service.
- `WinnerController` currently handles winner persistence and notification together; this should eventually separate persistence from notification dispatch.

## First implementation slice

Implement this first:
1. map the controllers, BLL classes, DAL classes, and DTOs to the new Order/Ticket/Raffle/Winner service ownership
2. mark direct user-entity dependencies that must become a contract later
3. identify the cut lines for moving checkout, raffle execution, and winner persistence together
4. keep the current user-id-in-token flow stable during the transition

## Immediate follow-up

After this map is complete, the next step is to extract the Order/Ticket/Raffle/Winner service incrementally, starting with the checkout and order persistence workflow.