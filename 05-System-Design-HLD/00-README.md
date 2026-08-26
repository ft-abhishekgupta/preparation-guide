# 05. System Design — High Level Design

The biggest differentiator at Senior/Staff/Tech Lead level. Master the framework, building blocks, and math before anything else.

| # | Note | Priority | What it covers |
|---|------|----------|----------------|
| 01 | [System Design Interview Framework](01-System-Design-Interview-Framework.md) | P0 | 7-step framework, time budget, clarifying questions, scoring rubric |
| 02 | [Scalability, Availability and Reliability](02-Scalability-Availability-and-Reliability.md) | P0 | Scaling strategies, SLO math, availability nines, redundancy, fault tolerance |
| 03 | [CAP, Consistency Models and Replication](03-CAP-Consistency-Models-and-Replication.md) | P0 | CAP/PACELC, consistency spectrum, replication patterns, consensus, Cosmos DB levels |
| 04 | [Back-of-the-Envelope Estimation](04-Back-of-the-Envelope-Estimation.md) | P0 | Latency numbers, QPS/storage/bandwidth math, 3 worked examples |
| 05 | [Load Balancing, API Gateway and Service Discovery](05-Load-Balancing-API-Gateway-and-Service-Discovery.md) | P0 | L4/L7 LB, algorithms, Azure Front Door vs AGW, gateway patterns, BFF, service discovery |
| 06 | [Message Queues and Pub-Sub](06-Message-Queues-and-Pub-Sub.md) | P0 | Kafka vs Service Bus vs Event Hubs, delivery semantics, idempotency, outbox pattern |
| 07 | [Event-Driven Architecture, CQRS and Event Sourcing](07-Event-Driven-Architecture-CQRS-and-Event-Sourcing.md) | P0 | EDA patterns, CQRS, event sourcing, saga, domain events |
| 08 | [Consistent Hashing, Sharding and Rate Limiting](08-Consistent-Hashing-Sharding-and-Rate-Limiting.md) | P0 | Consistent hashing, hot-key mitigation, rate limiting algorithms |
| 09 | [Distributed Systems Patterns](09-Distributed-Systems-Patterns.md) | P0 | Circuit breaker, bulkhead, retry, distributed locks, leader election |
| 10 | [Microservices Architecture](10-Microservices-Architecture.md) | P1 | Service decomposition, inter-service communication, service mesh |
| 11 | [Storage, Search and Job Scheduling](11-Storage-Search-and-Job-Scheduling.md) | P1 | Object storage, search engines, distributed job schedulers |
| 12 | [Geo-Distributed and Location-Based Systems](12-Geo-Distributed-and-Location-Based-Systems.md) | P1 | Multi-region, geo-routing, quad-trees, geohash |

**Study order:** 01 → 04 → 02 → 03 → 05 → 06 → 09 → 07 → 08 → 10 → 11 → 12
**Time to revise:** ~4–5 hours first pass; ~90 min revision pass

**Case studies:** See [`../06-System-Design-Case-Studies/00-README.md`](../06-System-Design-Case-Studies/00-README.md) for end-to-end design walkthroughs (News Feed, AI Certification Platform, Rate Limiter, etc.)
