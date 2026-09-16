# High Level Design (HLD)

High level design describes the overall architecture of a system: its components, how they interact, and the trade-offs that make it scalable, available and reliable.

## Contents

| #   | Topic                                                   | Notes                                                                    |
| --- | ------------------------------------------------------- | ------------------------------------------------------------------------ |
| 1   | [System Design Interview](SystemDesign/SystemDesign.md) | Delivery framework, requirements, core entities, APIs, deep dives        |
| 2   | [Concepts](Concepts/Concepts.md)                        | CAP theorem, consistency, availability, scaling, logging, error handling |
| 3   | [Building Blocks](BuildingBlocks/BuildingBlocks.md)     | Proxies, load balancers, controllers, repositories, middleware           |
| 4   | [API Design](ApiDesign/README.md)                       | REST, GraphQL, gRPC, authentication and authorization                    |
| 5   | [Caching](Caching/Caching.md)                           | Cache types, policies, invalidation, eviction, CDNs, deep dives          |
| 6   | [Database Scaling](DatabaseScaling/DatabaseScaling.md)  | Sharding, consistent hashing, replication, performance optimization      |
| 7   | [Asynchronous Systems](AsyncSystems/AsyncSystems.md)    | Background tasks, task queues, brokers                                   |

## Syllabus

| #     | Heading                           | What to cover                                                                                          |
| ----- | --------------------------------- | ------------------------------------------------------------------------------------------------------ |
| **1** | **System Design Fundamentals**    | Scalability, availability, reliability, latency, throughput, CAP, consistency, SLAs/SLOs               |
| **2** | **Networking & Communication**    | HTTP/HTTPS, TCP/UDP, DNS, REST, gRPC, WebSockets, load balancing                                       |
| **3** | **Data Storage**                  | SQL, NoSQL, indexing, transactions, replication, sharding, partitioning                                |
| **4** | **Caching & Performance**         | Cache strategies, Redis, CDN, cache invalidation, rate limiting, performance optimization              |
| **5** | **Distributed Systems**           | Consistency, replication, leader/follower, consensus concepts, distributed transactions, idempotency   |
| **6** | **Messaging & Async Systems**     | Queues, pub/sub, Kafka, event-driven architecture, retries, ordering, dead-letter queues               |
| **7** | **System Design Building Blocks** | API Gateway, Load Balancer, Service Discovery, Object Storage, Search, Scheduler, Workers              |
| **8** | **Design Problems & Patterns**    | URL Shortener, Rate Limiter, Notification System, Chat, Feed, File Storage, Payment, Uber-like systems |

## Hello Interview Done

1. Orientation
2. Foundations
3. Thinking in Scale
   1. Cache Quiz
   2. Sharding Quiz
   3. Consistent Hashing Quiz
   4. CAP
   5. CAP Quiz
   6. Numbers to know
   7. Numbers to know quiz
4. The Pattern
   1. Common Patterns
   2. Scaling Reads
      1. Practice Bitly
      2. Review Bitly
      3. Scaling Reads
      4. Scaling Reads Quiz
   3. Scaling Writes
      1. Scaling Writes
      2. Quiz
      3. Practice Ad Click
      4. Ad Click
   4. Real Time Updates
      1. Practice FB Live Comment
      2. FB Live Comment
      3. Real Time Updates (Video Only)
      4. Quiz
      5. Whatsapp
   5. Dealing with Contention
      1. Ticketmaster
      2. Dealing with contention
      3. Online Auction
   6. Multi-step Processes
      1. Notification System
      2. Multi Step Process
      3. Payment System
   7. Handling Large Blob
      1. Review Dropbox
      2. Handling Large Blob
      3. Youtube
   8. Managing Long Running Tasks
      1. Leetcode Review
   9. Proximity Based Service
