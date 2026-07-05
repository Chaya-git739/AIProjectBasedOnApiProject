# ADR: Inventory — Key‑Value Store (Redis)

Status: Proposed

Context
-------
Inventory operations require very low latency for reservations and fast atomic decrements/increments. The system will need to handle high throughput and transient state changes.

Decision
--------
Use Redis as the primary store for inventory counts and reservations (with persistence snapshots or AOF as needed).

Rationale
---------
- Redis provides atomic counter operations (`DECRBY`) and supports Lua scripts for multi-key atomic operations.
- Extremely low latency and high throughput make it suitable for stock reservation hot paths.

Consequences
------------
- Redis is eventually consistent for distributed setups; must design idempotent consumers and compensation for failures.
- Operational concern: persistence and recovery strategy must be chosen (RDB/AOF) to match durability needs.

CAP/Consistency
---------------
- BASE: favors Availability and Partition tolerance with eventual consistency; application-level idempotency and compensating transactions required.

תרגום לעברית
----------------
- תאור: מלאי דורש השהיה נמוכה מאוד עבור הזמנות ושמירה על מונים אטומיים.
- החלטה: שימוש ב‑Redis לאחסון מונה המלאי ושמירת הזמנות זמניות.
- נימוקים: Redis תומך בפקודות אטומיות ובלש skripts כדי לבצע מבצעי מונה במצב אטומי.
- השלכות: עקביות בסופו של דבר ברירת מחדל; יש לתכנן אסטרטגיית שימור ו‑idempotency באפליקציה.
