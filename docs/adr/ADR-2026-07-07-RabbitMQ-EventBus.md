# ADR: RabbitMQ Event Bus for Order Saga

Status: Accepted

Context
-------
The project needs asynchronous messaging for the order saga so that OrderService, CatalogService, and NotificationService can coordinate without direct synchronous coupling.
The broker must support topic routing, durable queues, retries, and dead-letter handling.

Decision
--------
Use RabbitMQ as the event bus for saga choreography and service-to-service notifications.

Rationale
---------
- RabbitMQ fits the project size and the course requirements well: it is simple to run in docker-compose, easy to inspect, and sufficient for topic-based event routing.
- Topic exchanges map cleanly to the saga event flow: `order.placed`, `inventory.reserved`, `inventory.rejected`, `inventory.release-requested`, and `order.status-changed`.
- Durable queues, dead-letter queues, and retry headers provide a clear implementation of at-least-once delivery with compensating actions.
- Compared with Kafka, RabbitMQ has lower operational overhead for this project and is easier to explain in a short architecture presentation.

Consequences
------------
- Consumers must be idempotent because delivery is at-least-once.
- Replay and long-term retention are weaker than Kafka, but the saga only needs short-lived workflow messaging.
- Each service owns its own message handling and cannot assume in-order global processing.

CAP/Consistency
---------------
- RabbitMQ provides durable delivery and decoupling, but the workflow remains eventually consistent across services.
- The system relies on idempotent consumers, correlation IDs, and compensating transactions rather than distributed ACID transactions.

Comparison with Kafka
---------------------
- Kafka is better for high-volume streaming, replay, and event history.
- RabbitMQ is a better fit here because the project needs a focused saga broker, not a full event-stream platform.
- Kafka would add more operational and conceptual overhead without improving the core learning objective for this project.

תרגום לעברית
----------------
- תאור: המערכת צריכה מסרים אסינכרוניים כדי לתאם בין שירותי ההזמנה, הקטלוג וההתראות בלי תלות סינכרונית הדוקה.
- החלטה: שימוש ב‑RabbitMQ כ‑event bus עבור ה‑saga והודעות בין השירותים.
- נימוקים: RabbitMQ קל להרצה ב‑docker-compose, מתאים ל‑topic routing, ותומך ב‑durable queues, retries ו‑dead-letter queues.
- השלכות: יש צורך ב‑idempotency וב‑compensation; אין כאן replay היסטורי כמו ב‑Kafka, אבל זה מספיק ל‑workflow של הפרויקט.