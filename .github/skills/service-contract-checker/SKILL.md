---
name: service-contract-checker
description: "Use when verifying DTOs, API contracts, events, and cross-service calls during or after a microservice split. Checks that service boundaries are expressed through explicit contracts, that payloads are versionable, and that Authentication/Users, Catalog/Gifts/Categories/Donors, Order/Ticket/Raffle/Winner, and Notifications/Email/Cache interact through clear interfaces instead of shared internals."
---

# Service Contract Checker

Use this skill to validate the contract layer between services.

## Review goals

Check that the change:
- uses explicit DTOs or contract types at the boundary
- avoids leaking persistence entities across services
- keeps API, event, and message payloads small and versionable
- uses the right communication style for the dependency
- does not couple services through shared internal helpers or DAL classes

## Contract surfaces

Review these kinds of interaction:
- HTTP request and response DTOs
- event payloads
- message contracts
- client interfaces and generated API clients
- shared identifiers and reference data

## Service boundaries

Treat these as the service ownership map:

- Authentication/Users
- Catalog/Gifts/Categories/Donors
- Order/Ticket/Raffle/Winner
- Notifications/Email/Cache

## Review checklist

For each contract, verify:

1. **DTO shape**
   - Is the payload minimal and purpose-built?
   - Does it avoid exposing database entities or internal navigation properties?
   - Does it include only fields the consumer actually needs?

2. **Boundary direction**
   - Does the consumer depend on a public contract, not another service’s internals?
   - Is there any direct reference to another service’s repository, DAL, DbContext, or private model?

3. **Communication style**
   - Is HTTP appropriate for synchronous lookup or command flow?
   - Is an event or message better for asynchronous notification or state propagation?
   - Does the chosen style match the business need?

4. **Versioning**
   - Can the contract evolve without breaking consumers?
   - Are new fields additive and optional where possible?
   - Is there a clear versioning or compatibility strategy?

5. **Ownership**
   - Which service owns the source of truth for this data?
   - Is the contract a read model, a command, or an event, and is that role clear?

6. **Consistency and idempotency**
   - For events and messages, can they be retried safely?
   - Is there an idempotency key or a natural duplicate-handling strategy?
   - Does the consumer handle out-of-order delivery if applicable?

## When to flag a problem

Flag it if the contract:
- exposes database entities directly over the wire
- relies on a shared model assembly as if it were a shared domain boundary
- uses synchronous calls where a notification event is the better fit
- omits ownership or correlation information needed for downstream processing
- cannot evolve without breaking other services

## Output format

When reviewing a contract, return:

- **Verdict**: pass, caution, or fail
- **Why**: short explanation of the contract fit
- **Contract issues**: any DTO, event, or coupling problems
- **Required fixes**: concrete changes needed before merge
- **Compatibility notes**: what consumers must handle
- **Next contract to inspect**: the next interface, DTO, or event to review

## Good review tone

Be strict about contract clarity and compatibility. Prefer small explicit payloads and stable boundaries over convenient but leaky shared models.