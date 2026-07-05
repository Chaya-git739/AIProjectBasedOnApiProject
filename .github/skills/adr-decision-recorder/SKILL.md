---
name: adr-decision-recorder
description: "Use when you need to record and review architecture decisions during a microservice migration. Captures ownership rules, service boundaries, contract choices, data split decisions, and migration tradeoffs for Authentication/Users, Catalog/Gifts/Categories/Donors, Order/Ticket/Raffle/Winner, and Notifications/Email/Cache."
---

# ADR / Decision Recorder

Use this skill to capture the decisions that keep the migration consistent across multiple steps.

## Purpose

This skill helps you record why a choice was made so the migration does not drift over time.

Use it to document decisions such as:
- which service owns a domain
- whether a model stays shared temporarily
- whether a boundary uses HTTP, events, or messages
- when a contract can change
- when a dependency must be removed
- whether a database split is required now or later

## When to use

Use this skill when:
- a migration step is complete and you need to lock in the decision
- you are about to make an architecture choice that affects multiple services
- you need to compare a new change against a previous decision
- you want to keep the plan aligned across future steps

## What to record

For each decision, capture:
- the date or stage
- the problem being solved
- the decision taken
- the reason for the decision
- what options were rejected
- the impact on the next migration slice
- any follow-up work or verification required

## Recommended decision categories

- **Service ownership**
- **Contract shape**
- **Data ownership**
- **Cross-service communication**
- **Shared model temporary use**
- **Database split timing**
- **Migration cutover rule**

## Output format

When recording or reviewing a decision, return:

- **Decision**: the choice that was made
- **Why**: short reason for the choice
- **Rejected options**: what was not chosen and why
- **Impact**: what changes next
- **Follow-up**: the next action or check

## Good decision rules

- Keep decisions short and explicit.
- Prefer one decision per topic.
- Do not mix multiple service boundaries into one vague note.
- If a decision changes a previous one, say so clearly.
- If the change affects contract or behavior, call out the verification needed.

## Good usage pattern

Use this skill after the planner or reviewer has identified a stable slice. It is the place to write down the rule so later steps can follow it without re-arguing the architecture.