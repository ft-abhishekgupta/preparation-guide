# 04. Databases

Covers relational + NoSQL fundamentals, Cosmos DB deep dive, indexing, transactions, data modeling, sharding, replication, and the CAP/consistency landscape. All SQL examples in T-SQL/ANSI SQL; app code in C#.

| # | Note | Priority | What it covers |
| - | ---- | -------- | -------------- |
| 01 | [SQL Joins, Subqueries, CTEs & Window Functions](01-SQL-Joins-Subqueries-CTEs-and-Window-Functions.md) | P0 | All join types, join algorithms, logical query order, aggregates, CTEs, recursive CTEs, window functions, set operators, NULL semantics, MERGE, classic SQL puzzles |
| 02 | [Indexing & Query Optimization](02-Indexing-and-Query-Optimization.md) | P0 | B+Tree internals, clustered vs non-clustered, covering indexes, SARGability, execution plans, parameter sniffing, N+1, slow-query triage |
| 03 | [Transactions, ACID, Isolation & Locking](03-Transactions-ACID-Isolation-and-Locking.md) | P0 | ACID, isolation levels, read phenomena, MVCC, lock types, deadlocks, optimistic concurrency, 2PC, saga, idempotency |
| 04 | [Data Modeling, Normalization & Denormalization](04-Data-Modeling-Normalization-and-Denormalization.md) | P0 | ER modeling, normal forms, denormalization trade-offs, hierarchical patterns, audit tables, migrations, multi-tenancy |
| 05 | [Sharding, Partitioning & Replication](05-Sharding-Partitioning-and-Replication.md) | P0 | Sharding strategies, consistent hashing, replication topologies, quorum, CDC, outbox, RPO/RTO |
| 06 | [NoSQL Landscape & CAP](06-NoSQL-Landscape-and-CAP.md) | P0 | CAP/PACELC, BASE vs ACID, consistency spectrum, five NoSQL families, DynamoDB patterns, Cassandra, Redis, LSM vs B-Tree |
| 07 | [Cosmos DB Deep Dive](07-Cosmos-DB-Deep-Dive.md) | P0 | Resource model, partition key design, RU model, consistency levels, global distribution, indexing, change feed, data modeling, migration playbook |

**Study order:** 01 → 02 → 03 → 04 → 05 → 06 → 07  
**Time to revise:** ~90 minutes (all); ~30 minutes (01–03 only for quick SQL refresh); ~20 minutes (07 alone for Cosmos DB deep-dive)
