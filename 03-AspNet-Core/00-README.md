# 03. ASP.NET Core

P0 area. Six files covering the full production-grade ASP.NET Core stack on .NET 8 minimal hosting model — from Kestrel internals to rate limiting. Assume reader can build 5K-RPS APIs; go deep on tradeoffs and failure modes.

| #  | Note | Priority | What it covers |
|----|------|----------|----------------|
| 01 | [Request Pipeline, Middleware & Filters](01-Request-Pipeline-Middleware-and-Filters.md) | P0 | Kestrel, WebApplication, middleware chain, MVC filters, ordering pitfalls |
| 02 | [Dependency Injection & Lifetimes](02-Dependency-Injection-and-Lifetimes.md) | P0 | IoC, Singleton/Scoped/Transient, captive deps, IOptions, IHttpClientFactory |
| 03 | [Authentication — JWT, OAuth 2.0, OIDC](03-Authentication-JWT-OAuth-OIDC.md) | P0 | JWT anatomy, signing, OAuth grant types, OIDC, Entra ID, mTLS |
| 04 | [Authorization — Policies, Roles & Claims](04-Authorization-Policies-Roles-Claims.md) | P0 | Role/claims/policy/resource-based authz, RBAC vs ABAC, multi-tenant |
| 05 | [API Design — Routing, Binding, Validation & Versioning](05-API-Design-Routing-Binding-Validation-and-Versioning.md) | P0 | REST conventions, Minimal API vs Controllers, Problem Details, rate limiting |
| 06 | [Configuration, Logging, Health & Background Services](06-Configuration-Logging-Health-and-Background-Services.md) | P0 | Config providers, structured logging, OpenTelemetry, health checks, hosted services |

**Study order:** 01 → 02 → 03 → 04 → 05 → 06
**Time to revise:** ~90 minutes (15 min/file)
