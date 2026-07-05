---
name: migration-planner-decomposer
description: "Use when planning a migration from the monolith into staged microservice work items. Produces the next concrete slice, dependency order, cut lines, risks, and verification steps for Authentication/Users, Catalog/Gifts/Categories/Donors, Order/Ticket/Raffle/Winner, and Notifications/Email/Cache."
---

# Migration Planner / Decomposer

Use this skill to break the monolith into the next smallest safe migration slice.

## Planning goals

Produce a migration plan that:
- identifies the next concrete slice to extract
- shows dependencies that must be resolved first
- defines cut lines for code, data, and communication
- keeps each step small enough to verify independently
- favors a strangler-style migration over a big-bang rewrite
- preserves existing behavior, response shapes, and business rules unless a contract change is explicitly approved

## First implementation slice

Start with the service boundary map and ownership rules. Use [service boundary map](./references/service-boundary-map.md) to identify which controllers, BLL classes, DAL classes, shared models, and cross-service dependencies belong to each target service before any extraction begins.

## Next implementation slice

After Authentication/Users is separated, use [order raffle boundary map](./references/order-raffle-slice/boundary-map.md) to prepare the Order/Ticket/Raffle/Winner extraction. Keep the user-id-in-token flow stable while identifying the cut lines for checkout, order persistence, raffle execution, and winner persistence.

## Service sequence

Use this target order unless a dependency forces a different cut:

1. Authentication/Users
2. Catalog/Gifts/Categories/Donors
3. Order/Ticket/Raffle/Winner
4. Notifications/Email/Cache

## Planning method

For any requested migration slice, decompose it into:

1. **Next slice**
   - The smallest practical piece of work that can be completed and validated
   - Example: move login endpoints and JWT creation before user admin flows

2. **Dependencies**
   - Direct code dependencies
   - Shared models or data access
   - Required APIs, events, or message contracts
   - Database ownership and migration dependencies

3. **Cut lines**
   - What moves now
   - What stays in the monolith for the moment
   - Which interfaces or abstractions must be introduced first
   - Whether the monolith should remain on its existing implementation until the extracted service is independently validated before any call-site swap

4. **Risks**
   - Cross-service coupling
   - Shared database access
   - Circular dependencies
   - Missing contracts or versioning gaps

5. **Verification**
   - Build or compile checks
   - Endpoint smoke tests
   - Contract tests
   - Data consistency checks

## Recommended decomposition rules

- Extract one bounded context at a time.
- Keep authentication separate from business workflows.
- Keep catalog data ownership separate from order execution.
- Keep notifications event-driven if possible.
- Do not split a workflow across services without an explicit contract.
- Do not remove fields, validation rules, token claims, or endpoint behavior unless the contract change is called out and verified.
- Verify that the extracted slice produces the same observable behavior before and after the move unless the change is intentional.

## Output format

When producing a plan, return:

- **Next slice**: the next concrete migration step
- **Why this slice**: short reason it is the best cut
- **Dependencies**: what must exist before it can move
- **Cut lines**: what moves and what remains
- **Risks**: the main failure modes
- **Verification**: how to prove the slice is safe
- **Follow-up slice**: the next likely step after this one

## Good planning tone

Be practical and incremental. Prefer the smallest safe cut that reduces coupling without forcing the entire architecture to change at once.