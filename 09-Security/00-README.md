# 09. Security

Notes covering authentication, authorization, OWASP threats, cryptography, and secrets management for a senior backend engineer. Azure-first, .NET-centric, tied directly to resume experience (Managed Identity, RBAC/JIT platform, CVE remediation at scale).

| #  | Note | Priority | What it covers |
| -- | ---- | -------- | -------------- |
| 01 | [OAuth2, OIDC, JWT and Session Security](01-OAuth2-OIDC-JWT-and-Session-Security.md) | P0 | Grant types, PKCE, JWT anatomy + validation, Managed Identity, token storage |
| 02 | [Authorization, RBAC, ABAC and Least Privilege](02-Authorization-RBAC-ABAC-and-Least-Privilege.md) | P0 | Role/attribute/policy models, multi-tenant isolation, IDOR, JIT access, OPA |
| 03 | [OWASP Top 10 and Application Security](03-OWASP-Top-10-and-Application-Security.md) | P0 | SQL injection, XSS, CSRF, CORS, SSRF, security headers, secure code review checklist |
| 04 | [Cryptography, Secrets and Supply Chain](04-Cryptography-Secrets-and-Supply-Chain.md) | P0 | Encryption, TLS, password storage, Key Vault, CVE remediation at scale, STRIDE |

**Study order:** 01 → 02 → 03 → 04
**Time to revise:** ~60 minutes
