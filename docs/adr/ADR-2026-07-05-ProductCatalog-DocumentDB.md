# ADR: ProductCatalog — Document Database (MongoDB)

Status: Proposed

Context
-------
Product entities have variable attributes per category (e.g., electronics vs apparel). Schemas change often and denormalized product representations are useful for reads.

Decision
--------
Use a document database (MongoDB) for `ProductCatalogService`.

Rationale
---------
- Documents model variable product attributes naturally without expensive schema migrations.
- Fast reads of full product documents are enabled, and denormalized representations suit catalog queries.

Consequences
------------
- Eventual consistency unless configured otherwise; good read performance and schema flexibility.
- Need to plan for indexing and data validation at the application level.

CAP/Consistency
---------------
- BASE: favors Availability and Partition tolerance with eventual consistency; can configure readConcern/writeConcern for stronger guarantees when needed.

תרגום לעברית
----------------
- תאור: פריטים בקטלוג יכולים להכיל שדות שונים לפי קטגוריה ולכן מודל מסמך מתאים לגמישות בסכימה.
- החלטה: שימוש ב‑MongoDB עבור `ProductCatalogService`.
- נימוקים: אין צורך במיגרציות כבדות לשינויים בשדות; קריאת מסמך מלא מהירה ומייעלת תצוגות קטלוג.
- השלכות: עקביות בסופו של דבר ברירת מחדל; יש לתכנן אינדקסים ולבצע ולידציה ברמת האפליקציה.
