---
name: architecture-boundary-reviewer
description: "Use when reviewing a planned or completed change to verify it still matches the intended microservice split, ownership boundaries, and communication pattern. Checks service ownership, cross-service dependencies, data access, API/event contracts, and migration safety for the Authentication/Users, Catalog/Gifts/Categories/Donors, Order/Ticket/Raffle/Winner, and Notifications/Email/Cache services."
---

# Architecture Boundary Reviewer

Use this skill to review whether a change respects the target microservice boundaries.

## Review goals

Check that the change:
- stays inside one bounded context, or has a clearly defined cross-service contract
- does not introduce direct DAL, DbContext, or table access across service boundaries
- uses HTTP, messaging, or events instead of internal code reuse across services
- keeps ownership of data and business rules in the correct service
- does not mix notification, cache, user, catalog, and order concerns in the same implementation slice

## Service boundaries

Treat these as the target ownership map:

- Authentication/Users
- Catalog/Gifts/Categories/Donors
- Order/Ticket/Raffle/Winner
- Notifications/Email/Cache

## Review checklist

For each change, verify:

1. **Boundary fit**
   - Is the code change entirely within one service boundary?
   - If it crosses a boundary, is the interaction an explicit contract rather than direct dependency?

2. **Ownership**
   - Does the target service own the data, workflow, and validation logic it is changing?
   - Is any rule being implemented in the wrong service?

3. **Dependency direction**
   - Are dependencies flowing inward toward the owning service?
   - Is there any direct reference to another service's DAL, repository, entity model, or internal helper?

4. **Communication pattern**
   - If the change needs another service, is it using an API client, message, or event?
   - Is the contract small, explicit, and versionable?

5. **Migration safety**
   - Does the change preserve the current monolith until the target service is ready?
   - Does it avoid creating a partial split that would be hard to reverse?

6. **Data consistency**
   - Is the source of truth clear?
   - Are cache updates, email delivery, and order/winner transitions handled through the correct service?

## Output format

When reviewing a change, return:

- **Verdict**: pass, caution, or fail
- **Why**: short explanation
- **Boundary issues**: list any violations or risky couplings
- **Required fixes**: concrete changes needed before merge
- **Next check**: the next file, test, or contract to inspect

## When to flag a problem

Flag it if the change:
- reaches into another service's DAL or DbContext
- shares entities or models across services as if they were common domain objects
- introduces synchronous coupling where an event is more appropriate
- places notification or cache logic inside core business services without a contract boundary
- changes multiple service responsibilities at once without an explicit migration step

## Good review tone

Be strict about boundaries and explicit about risk. Prefer small, local fixes over large redesigns unless the change is clearly violating the target architecture.