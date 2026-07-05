# ADR: OrderService — Relational Database (SQL Server)

Status: Accepted

Context
-------
Order data contains payments, totals, and lifecycle transitions that require atomic updates and transactional guarantees. The current codebase uses EF Core and expects relational schema support.

Decision
--------
Use a relational database (SQL Server in docker-compose) for `OrderService`.

Rationale
---------
- Financial operations require ACID semantics to avoid lost or duplicate charges.
- EF Core + SQL Server supports strong typing, foreign keys, and migrations which simplify evolving the order schema.
- Keeping orders relational reduces complexity for transactional reads/writes and reporting queries.

Consequences
------------
- Strong consistency and transactional safety for order operations (ACID).
- Service will own its database instance/schema; other services cannot directly query it.
- Requires operational effort for backups and migrations.

CAP/Consistency
---------------
- Prioritize Consistency and Partition tolerance handling via retries; availability may be reduced under partitioning but correctness is prioritized for money.

תרגום לעברית
----------------
- תאור: נתוני ההזמנה כוללים תשלומים, סכומים ומעברים במצב ההזמנה שדורשים עדכונים אטומיים והבטחות טרנזקציוניות.
- החלטה: שימוש בבסיס נתונים יחסתי (SQL Server) עבור `OrderService`.
- נימוקים: פעולות כספיות דורשות ACID כדי למנוע טעויות בחיוב; EF Core ו‑SQL Server מתאימים ל‑joins, foreign keys ומיגרציות.
- השלכות: קונסיסטנטיות חזקה ודרישות תפעוליות (גיבויים, מיגרציות); בסיס הבדיקה בבעלות השירות בלבד.
