## Data Integrity

## Overview
Rules implemented to prevent orphaned records and maintain data consistency.

---

## Relationships

| Relationship | Delete Behavior | Business Rule |
|-------------|----------------|---------------|
| User → Order | Restrict | User with confirmed orders cannot be deleted |
| Gift → OrderTicket | Restrict | Gift with draft or confirmed orders cannot be deleted |
| Gift → Winner | Restrict | Gift with winners cannot be deleted |
| User → Winner | Restrict | Protected by database constraint |
| Order → OrderTicket | Cascade | Order deletion removes related tickets |

---

## Business Rules

### User Deletion
- Uses Soft Delete (`IsDeleted`).
- Deletion is blocked when the user has confirmed orders.

### Gift Deletion
- Uses Soft Delete (`IsDeleted`).
- Deletion is blocked when the gift appears in:
  - Confirmed orders
  - Draft orders
  - Winner records

---


## Database Protection

```csharp
.OnDelete(DeleteBehavior.Restrict)