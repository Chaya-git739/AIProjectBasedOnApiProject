# ADR: AuthenticationService — Relational Database (SQL Server)

Status: Accepted

Context
-------
User records, credentials and roles require uniqueness, constraints, and auditability. Current code uses EF Core and expects constrained schema behavior.

Decision
--------
Use a relational database (SQL Server) for `AuthenticationService`.

Rationale
---------
- Authentication data benefits from unique constraints (email), indexing and transactions.
- EF Core migrations and relational constraints simplify schema evolution and integrity.

Consequences
------------
- Strong consistency for user records (ACID).
- Service owns its database; no direct external access by other services.

CAP/Consistency
---------------
- Prioritize Consistency over Availability for user identity correctness.

תרגום לעברית
----------------
- תאור: רשומות משתמשים, סיסמאות ותפקידים דורשות ייחודיות, הגבלות ויכולת ביקורת.
- החלטה: שימוש בבסיס נתונים יחסתי (SQL Server) עבור `AuthenticationService`.
- נימוקים: unique constraints (כמו אימייל), אינדוקסים וטרנזקציות; EF Core מקל על מיגרציות ושמירה על שלמות הנתונים.
- השלכות: קונסיסטנטיות חזקה (ACID) ועלויות תפעוליות נוספות.
