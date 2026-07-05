# ADR: Notification — Event Log (Kafka) + Small Document Store

Status: Proposed

Context
-------
Notifications require durable delivery and replay capability for retries and auditing. Storing an append‑only event log supports delivery guarantees and replay for downstream consumers.

Decision
--------
Use an event log (Kafka) for message delivery and a small document store (MongoDB) for storing notification history and templates.

Rationale
---------
- Kafka provides durable, ordered, and replayable topics suitable for sagas and at-least-once delivery.
- A document store holds templates and history for queries and presentation.

Consequences
------------
- Consumers must be idempotent and handle duplicate messages.
- Operational overhead for running Kafka; may use managed alternative if available.

CAP/Consistency
---------------
- Event log provides strong ordering and durability; eventual consistency applies to consumer-side state.

תרגום לעברית
----------------
- תאור: הודעות דורשות הספקה עמידה ויכולת השמעה מחדש לצורך retries וביקורת.
- החלטה: שימוש ב‑Kafka כ‑event log ובמסד מסמכים קטן (MongoDB) לאחסון היסטוריית ההודעות ותבניות.
- נימוקים: Kafka נותן יכולת השמעה מחדש, סדר והבטחה עמידה; מסד מסמכים מתאים לאחסון היסטוריה ושחזורים.
- השלכות: יש להבטיח idempotency בצרכנים; עלות תפעולית גבוהה יותר (או שימוש במנהל מנוהל).
