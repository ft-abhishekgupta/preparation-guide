# 02. C# and .NET Deep Dive

Primary language — must be flawless. Covers language internals, OOP, concurrency, memory, and collections targeting .NET 8 (notes on .NET 9 where relevant). Every file is P0.

| # | Note | Priority | What it covers |
| - | ---- | -------- | -------------- |
| 01 | [Language Fundamentals & Type System](01-Language-Fundamentals-and-Type-System.md) | P0 | Value/ref types, structs/records, Span, pattern matching, equality, nullable |
| 02 | [OOP & SOLID](02-OOP-and-SOLID.md) | P0 | Four pillars, abstract class vs interface, SOLID with examples, coupling/cohesion |
| 03 | [Generics, Delegates, Events & Expression Trees](03-Generics-Delegates-Events-and-Expression-Trees.md) | P0 | Generics variance, delegates vs events, closures, expression trees, reflection, source generators |
| 04 | [LINQ](04-LINQ.md) | P0 | Deferred execution, IEnumerable vs IQueryable, operators, N+1, PLINQ, perf pitfalls |
| 05 | [Async/Await & TPL](05-Async-Await-and-TPL.md) | P0 | State machine, Task/ValueTask, SynchronizationContext, CancellationToken, Channel, IAsyncEnumerable |
| 06 | [Threading, Synchronization & Concurrent Collections](06-Threading-Synchronization-and-Concurrent-Collections.md) | P0 | Lock primitives, deadlock, race conditions, concurrent collections, AsyncLocal |
| 07 | [Memory Management & GC](07-Memory-Management-and-GC.md) | P0 | Generations, LOH/POH, IDisposable, finalizers, leaks, allocation reduction, diagnostics |
| 08 | [Collections & Complexity](08-Collections-and-Complexity.md) | P0 | Full BCL collection table with Big-O, frozen/immutable, interface hierarchy |

**Study order:** 01 → 02 → 03 → 04 → 05 → 06 → 07 → 08

**Time to revise:** ~120 minutes full pass; ~30 minutes Quick Recap sweep
