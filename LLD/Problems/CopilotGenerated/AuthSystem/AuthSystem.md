# Login / Authentication System

A pluggable authentication service: register and log in users, hash passwords safely, issue
and validate session tokens, support multiple auth providers and MFA, and lock out brute
force attempts.

## Prompt

```
"Design an authentication system. Users register with email and password and log in to
receive a token used on subsequent requests.

login({ "email": "a@b.com", "password": "secret" })
  -> { "token": "...", "expiresAt": 1699999999 }

It should also support social login (Google, GitHub) and optional two-factor auth.
Focus on the auth flow, token lifecycle and security decisions."
```

## Questions

```
"Session tokens or JWTs?"
> Opaque session tokens stored server side - simpler to revoke

"Which login methods?"
> Email+password, plus OAuth providers, extensible

"Is MFA required?"
> Optional per user, TOTP

"What happens after repeated failed logins?"
> Lock the account temporarily

"Do we need password reset?"
> Yes, token-based

"Roles and permissions?"
> Simple role check is enough

"Can a user have multiple active sessions?"
> Yes, and they must be individually revocable

"How are passwords stored?"
> Salted hash with a slow KDF - never plaintext, never plain SHA-256
```

## Requirements

```
Requirements:
1. register(email, password) - reject duplicates and weak passwords
2. Passwords stored as salt + slow hash (bcrypt/argon2/PBKDF2), never reversible
3. login(email, password) returns a session token with an expiry
4. Multiple auth providers: password, OAuth - pluggable
5. If MFA is enabled, login returns a pending challenge; a valid TOTP completes it
6. validate(token) returns the user, or fails if expired/revoked
7. logout(token) revokes one session; logoutAll(userId) revokes every session
8. Lock the account for N minutes after K consecutive failures
9. Password reset by a single-use, time-bound token
10. authorize(token, role) for basic role checks

Out of scope:
- Real OAuth handshake with providers
- Distributed session store, persistence
- Device fingerprinting, risk scoring
- Email/SMS delivery mechanics
```

## Core Entities

```
AuthService       : Orchestrator - register / login / validate / logout
User              : Identity, credential hash, roles, MFA state
Credential        : salt + hash + algorithm
PasswordHasher    : Strategy - PBKDF2 / bcrypt / argon2
AuthProvider      : Strategy - password, OAuth
Session           : token + userId + expiry + revoked flag
SessionStore      : Token lifecycle
MfaProvider       : Strategy - TOTP
LoginAttemptTracker: Brute-force lockout
AuthResult        : Success / MfaRequired / Failure + reason
```

**Why these patterns**

| Concern | Pattern | Reason |
|---|---|---|
| Multiple login methods | Strategy (`AuthProvider`) | Add SAML/OAuth without touching the flow |
| Hash algorithm | Strategy (`PasswordHasher`) | Algorithms age; migration must be possible |
| Token lifecycle | Repository (`SessionStore`) | Swap in-memory for Redis unchanged |
| Result of login | Result object, not exception | `MFA_REQUIRED` is a normal outcome, not an error |

## Class Design

```
class AuthService:
    - users: Map<email, User>
    - providers: Map<string, AuthProvider>
    - hasher: PasswordHasher
    - sessions: SessionStore
    - attempts: LoginAttemptTracker
    - mfa: MfaProvider

    + register(email, password) -> User
    + login(providerName, credentials) -> AuthResult
    + verifyMfa(challengeId, code) -> AuthResult
    + validate(token) -> User | null
    + authorize(token, role) -> boolean
    + logout(token) -> void
    + logoutAll(userId) -> void
    + requestPasswordReset(email) -> string
    + resetPassword(resetToken, newPassword) -> void

class User:
    - id, email: string
    - credential: Credential
    - roles: Set<string>
    - mfaEnabled: boolean
    - mfaSecret: string | null
    - status: UserStatus      // ACTIVE, LOCKED, DISABLED

class Credential:
    - salt: string
    - hash: string
    - algorithm: string       // enables future migration

interface PasswordHasher:
    + hash(password) -> Credential
    + verify(password, credential) -> boolean

interface AuthProvider:
    + name() -> string
    + authenticate(credentials) -> User | null

class PasswordAuthProvider implements AuthProvider
class OAuthProvider implements AuthProvider

class Session:
    - token: string
    - userId: string
    - createdAt, expiresAt: long
    - revoked: boolean

    + isValid() -> boolean

interface SessionStore:
    + create(userId, ttlMs) -> Session
    + get(token) -> Session | null
    + revoke(token) -> void
    + revokeAllForUser(userId) -> void

class LoginAttemptTracker:
    - failures: Map<email, (count, lockedUntil)>

    + isLocked(email) -> boolean
    + recordFailure(email) -> void
    + reset(email) -> void

class AuthResult:
    - status: SUCCESS | MFA_REQUIRED | FAILURE
    - token: string | null
    - challengeId: string | null
    - reason: string | null
```

**Login flow**

```
login(email, password)
        |
        v
  account locked? --yes--> FAILURE("locked")
        | no
        v
  provider.authenticate() --fail--> recordFailure --> FAILURE("invalid credentials")
        | ok
        v
  attempts.reset()
        |
  mfaEnabled? --yes--> create challenge --> MFA_REQUIRED(challengeId)
        | no                                        |
        v                                    verifyMfa(code)
  sessions.create() ------------------------------->|
        |                                            v
        +-----------------> SUCCESS(token) <---------+
```

## Implementation

register

```
register(email, password)
    if users.containsKey(email)
        throw new UserExistsException()      // note: generic message to the caller

    validatePasswordStrength(password)       // length, character classes, breach list

    credential = hasher.hash(password)       // fresh random salt inside
    user = new User(generateId(), email, credential, roles: {"USER"})
    users[email] = user
    return user
```

login

```
login(providerName, credentials)
    email = credentials["email"]

    if attempts.isLocked(email)
        return AuthResult.failure("Account temporarily locked")

    provider = providers[providerName]
    user = provider.authenticate(credentials)

    if user == null
        attempts.recordFailure(email)
        return AuthResult.failure("Invalid credentials")   // never say which part was wrong

    if user.status != ACTIVE
        return AuthResult.failure("Account is not active")

    attempts.reset(email)

    if user.mfaEnabled
        challengeId = generateId()
        pendingChallenges[challengeId] = (user.id, now() + 5 minutes)
        return AuthResult.mfaRequired(challengeId)

    session = sessions.create(user.id, SESSION_TTL)
    return AuthResult.success(session.token)
```

Password verification - constant time

```
PasswordAuthProvider.authenticate(credentials)
    user = users.get(credentials["email"])

    if user == null
        hasher.verify(credentials["password"], DUMMY_CREDENTIAL)   // burn the same time
        return null                                                // no user enumeration

    return hasher.verify(credentials["password"], user.credential) ? user : null
```

Two deliberate choices: the dummy verify keeps response time flat whether or not the account
exists, and hash comparison uses a fixed-time equality check so timing cannot leak the hash.

Hashing

```
PBKDF2Hasher.hash(password)
    salt = randomBytes(16)
    hash = pbkdf2(password, salt, iterations: 100_000, keyLength: 32)
    return new Credential(base64(salt), base64(hash), "PBKDF2-SHA256-100000")

PBKDF2Hasher.verify(password, credential)
    salt = base64Decode(credential.salt)
    computed = pbkdf2(password, salt, iterations, keyLength)
    return constantTimeEquals(computed, base64Decode(credential.hash))
```

Slow by design: a fast hash (MD5/SHA) lets an attacker try billions of guesses per second
against a leaked table.

Session validation & lockout

```
validate(token)
    session = sessions.get(token)
    if session == null || !session.isValid(): return null
    return usersById[session.userId]

LoginAttemptTracker.recordFailure(email)
    state = failures.getOrDefault(email, (0, 0))
    state.count += 1
    if state.count >= maxAttempts
        state.lockedUntil = now() + lockoutMs
        state.count = 0
    failures[email] = state

isLocked(email)
    return failures.contains(email) && now() < failures[email].lockedUntil
```

## Code

Credential & Hasher

```cs
using System;
using System.Security.Cryptography;

public class Credential
{
    public string Salt { get; }
    public string Hash { get; }
    public string Algorithm { get; }

    public Credential(string salt, string hash, string algorithm)
    {
        Salt = salt;
        Hash = hash;
        Algorithm = algorithm;
    }
}

public interface IPasswordHasher
{
    Credential Hash(string password);
    bool Verify(string password, Credential credential);
}

public class Pbkdf2Hasher : IPasswordHasher
{
    private const int Iterations = 100_000;
    private const int KeyLength = 32;
    private const int SaltLength = 16;

    public Credential Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltLength);
        var hash = Derive(password, salt);
        return new Credential(Convert.ToBase64String(salt), Convert.ToBase64String(hash),
                              $"PBKDF2-SHA256-{Iterations}");
    }

    public bool Verify(string password, Credential credential)
    {
        var salt = Convert.FromBase64String(credential.Salt);
        var expected = Convert.FromBase64String(credential.Hash);
        var actual = Derive(password, salt);

        // Fixed-time comparison - a normal == leaks information through timing
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private static byte[] Derive(string password, byte[] salt)
        => Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeyLength);
}
```

User & Session

```cs
using System;
using System.Collections.Generic;

public enum UserStatus { Active, Locked, Disabled }

public class User
{
    public string Id { get; }
    public string Email { get; }
    public Credential Credential { get; set; }
    public HashSet<string> Roles { get; }
    public bool MfaEnabled { get; set; }
    public string MfaSecret { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;

    public User(string id, string email, Credential credential, HashSet<string> roles)
    {
        Id = id;
        Email = email;
        Credential = credential;
        Roles = roles;
    }
}

public class Session
{
    public string Token { get; }
    public string UserId { get; }
    public long CreatedAt { get; }
    public long ExpiresAt { get; }
    public bool Revoked { get; set; }

    public Session(string token, string userId, long expiresAt)
    {
        Token = token;
        UserId = userId;
        CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        ExpiresAt = expiresAt;
    }

    public bool IsValid()
        => !Revoked && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() < ExpiresAt;
}
```

SessionStore

```cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

public interface ISessionStore
{
    Session Create(string userId, long ttlMs);
    Session Get(string token);
    void Revoke(string token);
    void RevokeAllForUser(string userId);
}

public class InMemorySessionStore : ISessionStore
{
    private readonly Dictionary<string, Session> _sessions = new();

    public Session Create(string userId, long ttlMs)
    {
        // Cryptographic randomness - Guid/Random are predictable enough to be a real risk
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var session = new Session(token, userId,
                                  DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ttlMs);
        _sessions[token] = session;
        return session;
    }

    public Session Get(string token)
        => _sessions.TryGetValue(token, out var session) ? session : null;

    public void Revoke(string token)
    {
        if (_sessions.TryGetValue(token, out var session))
        {
            session.Revoked = true;
        }
    }

    public void RevokeAllForUser(string userId)
    {
        foreach (var session in _sessions.Values.Where(s => s.UserId == userId))
        {
            session.Revoked = true;
        }
    }
}
```

AuthProvider

```cs
using System.Collections.Generic;

public interface IAuthProvider
{
    string Name { get; }
    User Authenticate(Dictionary<string, string> credentials);
}

public class PasswordAuthProvider : IAuthProvider
{
    private static readonly Credential Dummy =
        new("ZHVtbXlzYWx0ZHVtbXk=", "ZHVtbXloYXNoZHVtbXloYXNo", "PBKDF2-SHA256-100000");

    private readonly Dictionary<string, User> _users;
    private readonly IPasswordHasher _hasher;

    public string Name => "password";

    public PasswordAuthProvider(Dictionary<string, User> users, IPasswordHasher hasher)
    {
        _users = users;
        _hasher = hasher;
    }

    public User Authenticate(Dictionary<string, string> credentials)
    {
        var email = credentials.GetValueOrDefault("email", "");
        var password = credentials.GetValueOrDefault("password", "");

        if (!_users.TryGetValue(email, out var user))
        {
            _hasher.Verify(password, Dummy);   // equalise timing, prevent user enumeration
            return null;
        }

        return _hasher.Verify(password, user.Credential) ? user : null;
    }
}
```

LoginAttemptTracker

```cs
using System;
using System.Collections.Generic;

public class LoginAttemptTracker
{
    private class State
    {
        public int Count;
        public long LockedUntil;
    }

    private readonly Dictionary<string, State> _failures = new();
    private readonly int _maxAttempts;
    private readonly long _lockoutMs;

    public LoginAttemptTracker(int maxAttempts, long lockoutMs)
    {
        _maxAttempts = maxAttempts;
        _lockoutMs = lockoutMs;
    }

    public bool IsLocked(string email)
        => _failures.TryGetValue(email, out var state)
           && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() < state.LockedUntil;

    public void RecordFailure(string email)
    {
        if (!_failures.TryGetValue(email, out var state))
        {
            state = new State();
            _failures[email] = state;
        }

        state.Count++;
        if (state.Count >= _maxAttempts)
        {
            state.LockedUntil = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + _lockoutMs;
            state.Count = 0;
        }
    }

    public void Reset(string email) => _failures.Remove(email);
}
```

AuthResult & AuthService

```cs
using System;
using System.Collections.Generic;

public enum AuthStatus { Success, MfaRequired, Failure }

public class AuthResult
{
    public AuthStatus Status { get; }
    public string Token { get; }
    public string ChallengeId { get; }
    public string Reason { get; }

    private AuthResult(AuthStatus status, string token, string challengeId, string reason)
    {
        Status = status;
        Token = token;
        ChallengeId = challengeId;
        Reason = reason;
    }

    public static AuthResult Success(string token) => new(AuthStatus.Success, token, null, null);
    public static AuthResult MfaRequired(string id) => new(AuthStatus.MfaRequired, null, id, null);
    public static AuthResult Failure(string reason) => new(AuthStatus.Failure, null, null, reason);
}

public class AuthService
{
    private const long SessionTtlMs = 24 * 60 * 60 * 1000;

    private readonly Dictionary<string, User> _usersByEmail = new();
    private readonly Dictionary<string, User> _usersById = new();
    private readonly Dictionary<string, IAuthProvider> _providers = new();
    private readonly Dictionary<string, (string UserId, long ExpiresAt)> _challenges = new();

    private readonly IPasswordHasher _hasher;
    private readonly ISessionStore _sessions;
    private readonly LoginAttemptTracker _attempts;
    private readonly Func<string, string, bool> _verifyTotp;

    public AuthService(IPasswordHasher hasher, ISessionStore sessions,
                       LoginAttemptTracker attempts, Func<string, string, bool> verifyTotp)
    {
        _hasher = hasher;
        _sessions = sessions;
        _attempts = attempts;
        _verifyTotp = verifyTotp;

        _providers["password"] = new PasswordAuthProvider(_usersByEmail, hasher);
    }

    public void RegisterProvider(IAuthProvider provider) => _providers[provider.Name] = provider;

    public User Register(string email, string password)
    {
        if (_usersByEmail.ContainsKey(email))
        {
            throw new InvalidOperationException("Registration failed");
        }

        if (password.Length < 8)
        {
            throw new ArgumentException("Password too weak");
        }

        var user = new User(Guid.NewGuid().ToString(), email, _hasher.Hash(password),
                            new HashSet<string> { "USER" });
        _usersByEmail[email] = user;
        _usersById[user.Id] = user;
        return user;
    }

    public AuthResult Login(string providerName, Dictionary<string, string> credentials)
    {
        var email = credentials.GetValueOrDefault("email", "");

        if (_attempts.IsLocked(email))
        {
            return AuthResult.Failure("Account temporarily locked");
        }

        if (!_providers.TryGetValue(providerName, out var provider))
        {
            return AuthResult.Failure("Unknown auth provider");
        }

        var user = provider.Authenticate(credentials);
        if (user == null)
        {
            _attempts.RecordFailure(email);
            return AuthResult.Failure("Invalid credentials");
        }

        if (user.Status != UserStatus.Active)
        {
            return AuthResult.Failure("Account is not active");
        }

        _attempts.Reset(email);

        if (user.MfaEnabled)
        {
            var challengeId = Guid.NewGuid().ToString();
            _challenges[challengeId] =
                (user.Id, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 5 * 60 * 1000);
            return AuthResult.MfaRequired(challengeId);
        }

        return AuthResult.Success(_sessions.Create(user.Id, SessionTtlMs).Token);
    }

    public AuthResult VerifyMfa(string challengeId, string code)
    {
        if (!_challenges.TryGetValue(challengeId, out var challenge)
            || DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() > challenge.ExpiresAt)
        {
            return AuthResult.Failure("Challenge expired");
        }

        var user = _usersById[challenge.UserId];
        if (!_verifyTotp(user.MfaSecret, code))
        {
            return AuthResult.Failure("Invalid code");
        }

        _challenges.Remove(challengeId);   // single use
        return AuthResult.Success(_sessions.Create(user.Id, SessionTtlMs).Token);
    }

    public User Validate(string token)
    {
        var session = _sessions.Get(token);
        return session is { } s && s.IsValid() ? _usersById[s.UserId] : null;
    }

    public bool Authorize(string token, string role)
    {
        var user = Validate(token);
        return user != null && user.Roles.Contains(role);
    }

    public void Logout(string token) => _sessions.Revoke(token);

    public void LogoutAll(string userId) => _sessions.RevokeAllForUser(userId);

    public void ChangePassword(string userId, string newPassword)
    {
        _usersById[userId].Credential = _hasher.Hash(newPassword);
        _sessions.RevokeAllForUser(userId);   // invalidate every existing session
    }
}
```

Usage

```cs
var auth = new AuthService(new Pbkdf2Hasher(), new InMemorySessionStore(),
                           new LoginAttemptTracker(maxAttempts: 5, lockoutMs: 15 * 60 * 1000),
                           verifyTotp: (secret, code) => TotpVerifier.Verify(secret, code));

auth.Register("a@b.com", "correct-horse-battery");
var result = auth.Login("password", new() { ["email"] = "a@b.com", ["password"] = "correct-horse-battery" });

if (result.Status == AuthStatus.Success)
{
    var user = auth.Validate(result.Token);
}
```

## Security Decisions Worth Stating in the Interview

| Decision | Reason |
|---|---|
| Slow KDF (PBKDF2/argon2), never SHA-256 alone | Makes offline cracking of a leaked DB expensive |
| Unique random salt per user | Kills rainbow tables and cross-user hash reuse |
| `FixedTimeEquals` for hash comparison | Prevents timing side channels |
| Dummy verify for unknown emails | Prevents user enumeration via response time |
| Generic "Invalid credentials" message | Does not reveal whether the email exists |
| `RandomNumberGenerator` for tokens | `Guid`/`Random` are predictable enough to be guessable |
| Opaque server-side sessions | Instantly revocable, unlike a stateless JWT |
| Revoke all sessions on password change | Locks out an attacker who already had a session |
| Lockout with time window | Blunts credential stuffing without permanent DoS on the user |

## Extensibility

### "How would you add Google login?"

Implement `IAuthProvider` with `Name = "google"`: exchange the OAuth code for a profile, then
look up or auto-create the user by verified email. `Login` needs no change - it only knows
the interface.

### "Would you use JWTs instead? What's the trade-off?"

JWT: stateless, no lookup per request, scales horizontally - but cannot be revoked before
expiry. Sessions: one lookup per request, instantly revocable. Common middle ground is a
short-lived JWT access token (5-15 min) plus a long-lived revocable refresh token - you get
statelessness with a bounded revocation window.

### "How do you migrate to a stronger hash algorithm?"

`Credential.Algorithm` already records what was used. On successful login, if the stored
algorithm is outdated, re-hash the plaintext (available only at that moment) with the new
algorithm and save. Users migrate transparently as they log in.

### "How do you implement password reset securely?"

Single-use token = random 32 bytes, stored **hashed**, 15-minute TTL, bound to the user.
Always return the same response whether or not the email exists. On successful reset,
invalidate the token and revoke all sessions.

### "How does this scale across servers?"

Swap `InMemorySessionStore` for a Redis-backed one with TTL on the key - the interface is
unchanged. Lockout counters also move to Redis so an attacker cannot spread guesses across
nodes to bypass the limit.

### "What if an attacker spreads attempts across many accounts?"

Per-account lockout does not catch that. Add a second layer keyed by IP/subnet, plus a
CAPTCHA after a global threshold. This is where the rate limiter design plugs in.
