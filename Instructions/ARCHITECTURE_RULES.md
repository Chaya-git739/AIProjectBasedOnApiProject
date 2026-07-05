# Architecture & Code Quality Rules

## Layered Architecture

This project follows a strict layered architecture. Every service must respect these boundaries:

### Layer Order
```
Controller → Service (BLL) → Repository (DAL) → DbContext / External Store
```

### Rules
- Controllers call **only** service interfaces (`IXxxService`). Never call a repository or DbContext directly from a controller.
- Services call **only** repository interfaces (`IXxxRepository`). Never inject or use `DbContext` inside a service class.
- Repositories are the **only** layer that touches `DbContext`, raw SQL, or any external store (Redis, HTTP client to another DB, etc.).
- HTTP calls to other microservices belong in the **service layer**, not in controllers and not in repositories.
- Every repository must have a matching interface (`IXxxRepository`) defined in the `Services/` folder so the service layer depends on the abstraction, not the EF implementation.

### Naming Conventions
| Layer | Interface | Implementation |
|---|---|---|
| Repository | `IXxxRepository` | `EfXxxRepository` |
| Service | `IXxxService` | `XxxService` |
| Controller | — | `XxxController` |

### Repository Responsibilities
A repository has exactly one job: **talk to the database**. Nothing else.

- Repositories only contain: `Add`, `Update`, `Delete`, `GetById`, `GetAll`, and focused query methods.
- Repositories must **not** contain: conditional logic, upsert decisions, existence checks that drive business flow, or any code that decides *what* to persist based on state.
- Upsert patterns (check if exists → create if not → return id) are **business logic** and belong in the service layer.
- A repository method must do exactly one thing — if a method name needs the word "or" (e.g. `GetOrCreate`), it belongs in the service, not the repository.
- Services orchestrate multiple repository calls to fulfill a business operation. Repositories never call other repositories.

### Dependency Injection
- Register all interfaces and their implementations in `Program.cs`.
- Use `AddScoped` for EF-backed repositories and services.
- Never use `new` to instantiate a service or repository inside another class — always inject via constructor.

---

## No Duplicated Code

- If the same logic appears in more than one place, extract it into a shared service, helper, or extension method.
- Do not copy-paste validation logic between controller and service — validation belongs in the service layer; the controller may do only model-state checks (`ModelState.IsValid`).
- Do not define the same DTO or model in multiple projects — if a contract is shared across services, define it once and reference it, or align the shape explicitly.
- Anonymous objects (`new { ... }`) must not be used as return types across layer boundaries — define a named class or record.
- Static mutable state (e.g. `static List<T>`) is forbidden as a persistence mechanism — use a repository backed by a real store.

---

## General
- Every new entity that needs persistence must have: a model class, a repository interface, an EF repository implementation, and a `DbSet<T>` registered in the relevant `DbContext`.
- Migrations must be added whenever a model or `DbContext` changes.
- Notification or side-effect calls (email, cache invalidation) must be fire-and-forget from the service layer and must never cause the primary operation to fail.
