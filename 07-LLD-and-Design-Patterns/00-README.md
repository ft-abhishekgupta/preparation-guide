# 07. Low Level Design and Design Patterns

Master LLD interview rounds, all 23 GoF patterns, Clean Architecture, DDD, and machine-coding case studies — written for a senior/staff/tech-lead bar.

| #  | Note | Priority | What it covers |
|----|------|----------|----------------|
| 01 | [LLD Interview Framework & UML](01-LLD-Interview-Framework-and-UML.md) | P0 | 45-min LLD process, UML notation, scoring rubric |
| 02 | [SOLID Applied & Clean Code](02-SOLID-Applied-and-Clean-Code.md) | P0 | Each principle with before/after C#, code smells, clean-code checklist |
| 03 | [Creational Patterns](03-Creational-Patterns.md) | P0 | Factory, Abstract Factory, Builder, Prototype, Singleton, Object Pool, DI |
| 04 | [Structural Patterns](04-Structural-Patterns.md) | P0 | Adapter, Bridge, Composite, Decorator, Facade, Flyweight, Proxy |
| 05 | [Behavioral Patterns](05-Behavioral-Patterns.md) | P0 | Strategy, Observer, Command, CoR, Mediator, State, Template, Iterator, Visitor, Memento, Null Object |
| 06 | [Clean/Onion/Hexagonal Architecture & DDD](06-Architecture-Clean-Onion-Hexagonal-and-DDD.md) | P0 | Architecture styles, DDD tactical & strategic, worked Order aggregate |
| 07 | [Repository, UoW, CQRS & API Design](07-Repository-UnitOfWork-CQRS-and-API-Design.md) | P0 | Repo/UoW patterns, MediatR CQRS, API contract design, thread-safe LLD checklist |
| 08 | [LLD Case Studies](08-LLD-Case-Studies.md) | P0 | 8 full designs: Parking Lot, Elevator, URL Shortener, Chat, KV Store, Rate Limiter, Splitwise, Vending Machine |

**Study order:** 01 → 02 → 03 → 04 → 05 → 06 → 07 → 08  
**Time to revise:** ~3–4 hrs first pass; ~90 min re-revision

---

## Pattern → Problem Quick Lookup (all 23 GoF)

| Category | Pattern | Problem it solves |
|----------|---------|-------------------|
| Creational | Factory Method | Decouple object creation; subclass decides concrete type |
| Creational | Abstract Factory | Create families of related objects without specifying concrete classes |
| Creational | Builder | Construct complex objects step-by-step; avoid telescoping constructors |
| Creational | Prototype | Clone expensive-to-create objects cheaply |
| Creational | Singleton | Guarantee one instance; global access point (use sparingly) |
| Structural | Adapter | Bridge incompatible interfaces without changing either side |
| Structural | Bridge | Separate abstraction from implementation so both vary independently |
| Structural | Composite | Treat individual objects and trees of objects uniformly |
| Structural | Decorator | Add behaviour at runtime without subclassing |
| Structural | Facade | Simplify a complex subsystem behind a single entry point |
| Structural | Flyweight | Share fine-grained objects to save memory (many small instances) |
| Structural | Proxy | Control access to an object (lazy init, cache, security, remote) |
| Behavioral | Chain of Responsibility | Pass request along a handler chain until one handles it |
| Behavioral | Command | Encapsulate a request as an object; support undo/queue/log |
| Behavioral | Interpreter | Grammar for a language; evaluate sentences (rare — DSLs, rules) |
| Behavioral | Iterator | Traverse a collection without exposing its internal structure |
| Behavioral | Mediator | Centralise communication between objects; reduce coupling |
| Behavioral | Memento | Capture and restore object state without violating encapsulation |
| Behavioral | Observer | Notify many dependents when one object changes |
| Behavioral | State | Change behaviour when internal state changes; eliminate if-chains |
| Behavioral | Strategy | Define a family of algorithms; swap them at runtime |
| Behavioral | Template Method | Define algorithm skeleton in base class; subclasses fill in steps |
| Behavioral | Visitor | Add operations to an object hierarchy without changing its classes |
