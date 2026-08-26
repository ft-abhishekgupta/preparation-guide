# 08. Caching

Notes on every caching layer a backend engineer must know: in-process, distributed (Redis), CDN, and HTTP browser caching. Covers strategy selection, eviction, invalidation, stampede protection, and Azure-specific deployment.

| #  | Note | Priority | What it covers |
| -- | ---- | -------- | -------------- |
| 01 | [Caching Strategies and Patterns](01-Caching-Strategies-and-Patterns.md) | P0 | Cache-aside/write-through/write-behind, eviction, invalidation, stampede, hot key, avalanche |
| 02 | [Redis and Distributed Caching](02-Redis-and-Distributed-Caching.md) | P0 | Redis internals, data types, Cluster, Sentinel, Managed Identity, StackExchange.Redis, HybridCache |
| 03 | [CDN and Client-Side Caching](03-CDN-and-Client-Side-Caching.md) | P1 | HTTP Cache-Control, ETags, Azure Front Door, browser vs service-worker, ASP.NET OutputCaching |

**Study order:** 01 → 02 → 03
**Time to revise:** ~45 minutes
