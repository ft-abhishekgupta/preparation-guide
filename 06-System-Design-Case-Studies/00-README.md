# 06. System Design Case Studies

End-to-end designs for 12 classic interview problems. Each file follows the full case-study template: requirements → scale estimation → API → data model → architecture → deep dives → bottlenecks → scaling → extensions → interview Q&A → quick recap. Apply the framework from [`../05-System-Design-HLD/01-System-Design-Interview-Framework.md`](../05-System-Design-HLD/01-System-Design-Interview-Framework.md) to every design.

| # | File | Priority | Core Concepts | Description |
|---|------|----------|---------------|-------------|
| 01 | [URL Shortener](01-URL-Shortener.md) | P0 | Hashing, KV store, caching, ID gen | Encode long URLs to short codes; handle redirects at scale |
| 02 | [Rate Limiter](02-Rate-Limiter.md) | P0 | Token bucket, Redis atomics, distributed counters | Throttle API traffic per user/IP/key at gateway or service layer |
| 03 | [Notification System](03-Notification-System.md) | P0 | Fan-out, queues, multi-channel, idempotency | Deliver push/SMS/email/in-app notifications to millions of users |
| 04 | [Chat Application](04-Chat-Application.md) | P0 | WebSockets, presence, fan-out, Cassandra ordering | 1:1 and group messaging with real-time delivery and offline sync |
| 05 | [News Feed](05-News-Feed.md) | P0 | Push/pull/hybrid fan-out, Redis timeline, ranking | Personalised content feed — mirrors the Xbox Publisher News Feed Platform |
| 06 | [Video Streaming Platform](06-Video-Streaming-Platform.md) | P0 | Transcoding, ABR/HLS/DASH, CDN, DRM | Netflix/YouTube-style VOD and live streaming at global scale |
| 07 | [Ride-Hailing Dispatch](07-Ride-Hailing-Dispatch.md) | P1 | Geo-index, quad-tree, matching, CQRS | Real-time driver matching with location tracking (Uber/Lyft style) |
| 08 | [Collaborative Document Editor](08-Collaborative-Document-Editor.md) | P1 | OT/CRDT, WebSockets, conflict resolution | Google Docs style concurrent editing |
| 09 | [Payment and Commerce Platform](09-Payment-and-Commerce-Platform.md) | P1 | Saga, idempotency, eventual consistency, ledger | Checkout, payment processing, and sales campaign authoring |
| 10 | [Distributed File Storage](10-Distributed-File-Storage.md) | P1 | Chunking, erasure coding, consistent hashing | S3/Dropbox-style file storage with deduplication |
| 11 | [Search Engine and Autocomplete](11-Search-Engine-and-Autocomplete.md) | P1 | Inverted index, trie, ranking, typeahead | Web search indexing and sub-50 ms autocomplete |
| 12 | [AI Content Moderation Pipeline](12-AI-Content-Moderation-Pipeline.md) | P1 | Event-driven, embedding, LLM, idempotency | Mirrors the Xbox AI Content Certification Platform |

---

## Concept Coverage Matrix

| Case Study | Sharding | Caching | Queues | WebSockets | Consistency | Geo-Index | CDN |
|------------|----------|---------|--------|------------|-------------|-----------|-----|
| URL Shortener | ✓ | ✓ | ✓ | | eventual | | |
| Rate Limiter | ✓ | ✓ | | | strong/atomic | | |
| Notification System | | ✓ | ✓ | | at-least-once | | |
| Chat Application | ✓ | ✓ | ✓ | ✓ | causal | | |
| News Feed | ✓ | ✓ | ✓ | ✓ | eventual | | ✓ |
| Video Streaming | ✓ | ✓ | ✓ | | eventual | | ✓ |
| Ride-Hailing | ✓ | ✓ | ✓ | ✓ | strong (matching) | ✓ | |
| Collab Editor | | ✓ | ✓ | ✓ | causal/OT | | |
| Payment Platform | ✓ | ✓ | ✓ | | strong | | |
| File Storage | ✓ | ✓ | ✓ | | eventual | | ✓ |
| Search/Autocomplete | ✓ | ✓ | ✓ | | eventual | | ✓ |
| AI Moderation | | ✓ | ✓ | | at-least-once | | |

---

**Study order:** 01 → 02 → 05 (resume-relevant) → 04 → 03 → 06 → 07–12 as time allows.
**Time to revise:** ~20–30 min per file (first pass); ~5 min per file (re-revision via Quick Recap).
