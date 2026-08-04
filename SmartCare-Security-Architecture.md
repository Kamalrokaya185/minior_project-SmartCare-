# SmartCare — Security & Reliability Architecture

**Companion document to:** SmartCare-Architecture.md
**Audience:** Engineering leadership, security review, compliance stakeholders
**Posture:** Defense-in-depth, zero-trust between tenants, assume breach

---

## Table of Contents
1. STRIDE Threat Model
2. OWASP Top 10 Mitigation Map
3. Authorization Architecture (RBAC + Permissions + Resource + Tenant + Ownership)
4. Multi-Tenant Security
5. Authentication Security
6. API Security
7. Input Validation Strategy
8. Injection Protection
9. XSS Protection
10. CSRF Protection
11. Payment Security
12. Race Condition Prevention (Double-Booking)
13. Secure File Upload
14. Sensitive Data Classification & Protection
15. Logging & Audit
16. Monitoring & Incident Detection
17. Fraud Detection
18. Data Retention
19. Backup & Disaster Recovery
20. DevSecOps
21. Testing Strategy
22. Compliance Posture (HIPAA/GDPR-aligned)

---

## 1. STRIDE Threat Model

| Category | Attack Scenario | Business Impact | Mitigation |
|---|---|---|---|
| **Spoofing** | Attacker forges a JWT or steals a receptionist's token via a phishing page to impersonate them | Unauthorized appointment/payment actions under a real staff identity | Strong JWT signing (asymmetric RS256, not HS256, so the signing key never leaves the auth service), short token lifetime, device-bound refresh tokens, mandatory HTTPS, OTP step-up for sensitive roles |
| **Spoofing** | Attacker fabricates a `TenantId` claim or header to act as a different hospital | Full cross-tenant data breach | `TenantId` is only ever derived from a **signed JWT claim**, never from a client-supplied header/query param; token issued only after the user's real hospital membership is verified server-side at login |
| **Tampering** | Attacker modifies a payment amount or appointment fee in transit or via a replayed/edited request | Financial loss, billing disputes | TLS 1.2+ everywhere; server always recomputes price server-side from the doctor's current fee record — client-submitted amounts are **never trusted**, only used for display; payment amount is verified against the gateway's own confirmation, not the client's claim |
| **Tampering** | Attacker tampers with an appointment's status directly via API (e.g., force `Completed` without a real consultation) | Fraudulent billing, false medical records | Domain-level state machine rejects illegal transitions regardless of caller; only specific roles can trigger specific transitions (Doctor completes, Receptionist checks in) enforced by permission-based authorization, not just "any authenticated user" |
| **Repudiation** | A user denies having cancelled an appointment or approved a refund | Disputes, potential fraud, legal exposure in a healthcare/financial context | Immutable, append-only `AuditLogs` capturing actor, IP, device, timestamp, correlation ID for every state-changing action; digitally hashed log chaining considered for the highest-sensitivity actions (refunds, role changes) so tampering with historical logs is detectable |
| **Information Disclosure** | Attacker changes an `appointmentId` in the URL to view another patient's appointment (IDOR) | Massive HIPAA-style patient privacy breach | Resource-based authorization on every read: the handler loads the resource and verifies `resource.PatientId == currentUser.PatientId` (or tenant-equivalent) before returning it — **existence of the record is not proof of the right to see it** |
| **Information Disclosure** | Verbose exception messages/stack traces leak schema or internal details | Aids further attacks, leaks PII in error payloads | Global exception middleware returns sanitized `ProblemDetails`; full details only ever go to server-side structured logs, never to the client, and never in a non-Production environment shipped externally |
| **Denial of Service** | Attacker floods the login or booking endpoint with requests | Platform outage, hospital operational disruption (patients unable to book care) | Rate limiting (IP + account based), CDN/WAF in front of the API, autoscaling stateless API instances, background-job queuing so a burst of bookings doesn't overwhelm the payment gateway synchronously |
| **Denial of Service** | Attacker triggers expensive queries (e.g., unbounded search/export) repeatedly | Database resource exhaustion affecting all tenants | Mandatory pagination (max page size enforced server-side, not just client-suggested), query timeouts, per-tenant resource quotas on heavy operations (e.g., exports throttled and run as background jobs, not synchronous requests) |
| **Elevation of Privilege** | A Receptionist manipulates a request to grant themselves Hospital Admin permissions, or a Patient calls a Doctor-only endpoint | Full account/hospital compromise | Every endpoint enforces a permission-based policy server-side (never inferred from UI visibility); role/permission changes themselves require elevated approval and are audit-logged; privilege checks happen in the Application layer (pipeline behavior), not just in a controller attribute that could be missed on a new endpoint |

---

## 2. OWASP Top 10 (2021) Mitigation Map

| Risk | SmartCare Mitigation |
|---|---|
| **A01 – Broken Access Control** | Layered authorization: RBAC → Permission claims → Resource ownership → Tenant match, enforced centrally via a MediatR `AuthorizationBehavior` so no handler can accidentally skip it. Deny-by-default: an endpoint with no explicit policy is rejected, not allowed. |
| **A02 – Cryptographic Failures** | TLS everywhere; `decimal`-safe money handling; PBKDF2/Argon2id password hashing; envelope encryption for PII columns (National ID) with keys in a managed KMS/Key Vault, not in application config; no home-grown crypto. |
| **A03 – Injection** | EF Core parameterized queries exclusively; raw SQL banned outside a reviewed allow-list (and even then, parameterized); FluentValidation + Domain invariants as a second gate. Detailed in Section 8. |
| **A04 – Insecure Design** | Threat modeling performed at design time (this document); state machines instead of implicit status strings; least-privilege database roles; abuse-case thinking baked into the Appointment/Payment/Refund flows (e.g., refund calculation can't be manipulated client-side because it's entirely server-computed from stored policy + timestamps). |
| **A05 – Security Misconfiguration** | Infrastructure-as-Code with reviewed defaults (deny-by-default network rules, no default admin passwords), hardened container images (non-root user, minimal base image), environment-specific config validated at startup (fail fast if a required secret is missing rather than silently running insecurely). |
| **A06 – Vulnerable & Outdated Components** | Automated dependency scanning (Dependabot/Snyk) in CI, blocking merges on critical CVEs; pinned versions with a documented patch cadence rather than "latest" floating tags in production. |
| **A07 – Identification & Authentication Failures** | Refresh token rotation + reuse detection, account lockout with backoff, MFA/OTP for sensitive roles, no "security questions"-style weak recovery flows. Detailed in Section 5. |
| **A08 – Software & Data Integrity Failures** | CI/CD pipeline requires signed commits/protected branches for release tags; container images are scanned and signed before deployment; webhook payloads (payment gateway) are signature-verified, never trusted on content alone. |
| **A09 – Security Logging & Monitoring Failures** | Structured, correlated logging (Serilog) plus a dedicated immutable `AuditLogs` store; alerting thresholds defined for auth failures, refund spikes, and anomalous access patterns (Section 16). |
| **A10 – Server-Side Request Forgery (SSRF)** | Any outbound call the server makes based on user input (e.g., a future "import from URL" feature, webhook callback URLs) is restricted to an allow-list of domains, blocks requests to internal/private IP ranges (metadata endpoints like `169.254.169.254` explicitly denied), and uses a dedicated egress proxy with no access to internal network segments. |

---

## 3. Authorization Architecture

JWT alone answers "who are you," never "what can you do to this specific resource." SmartCare layers four independent checks, all of which must pass:

1. **Role-Based (RBAC):** coarse gate — is this user a Doctor/Receptionist/Patient/Super Admin at all. Implemented as a claim in the JWT, validated on every request.
2. **Permission-Based:** fine-grained action gate — e.g., `appointments.confirm`, `refunds.approve`. Stored as `RolePermissions` in the DB (not hardcoded), so a hospital can create a custom "Senior Receptionist" role with an extended permission set without a code deployment. A custom `[Authorize(Policy = "appointments.confirm")]` policy provider resolves this at request time.
3. **Resource-Based:** the handler loads the actual entity and checks a relationship, e.g. `appointment.PatientId == currentUser.PatientId` for a patient, or `appointment.HospitalId == currentUser.TenantId` for staff. **This is the direct answer to IDOR** — knowing or guessing a valid `AppointmentId` is insufficient; the record's ownership must match the caller.
4. **Tenant-Based:** every tenant-scoped query is additionally filtered by `TenantId` at the EF Core global-filter level (Section 4) — an independent, structural safety net beneath the three checks above, so even a bug in resource-based logic doesn't leak across hospitals.

**IDOR prevention, concretely:** SmartCare never says "if the ID exists, return it." The query for `GetAppointmentByIdQuery` is always shaped as *"get the appointment with this ID **that belongs to this caller/tenant**"* — a non-owned resource returns `404 Not Found` (not `403 Forbidden`, to avoid confirming the resource's existence to an attacker probing IDs).

**Alternatives considered:**
- *Claims-only authorization (JWT roles alone)* — rejected as the sole mechanism: it cannot express "this specific appointment belongs to this specific patient," which is precisely the IDOR case in the brief.
- *ACL tables per resource* (explicit grant per user per record) — powerful but overkill for SmartCare's ownership model, which is naturally hierarchical (tenant → hospital → patient/doctor) rather than needing arbitrary per-record sharing. Reserved as a future option if a "share appointment with a family member" feature is ever needed.

---

## 4. Multi-Tenant Security

- **Tenant resolution:** on login, the server determines the user's `TenantId` from the database (their `Hospital` membership) and embeds it as a signed claim in the JWT. The client never supplies `TenantId` for resolution — only for display.
- **Tenant validation:** every request's `TenantContext` is populated purely from the validated JWT claim; a middleware step runs before any handler executes and short-circuits with 401 if the claim is missing/malformed.
- **Global Query Filters:** every tenant-scoped `DbSet` has `modelBuilder.Entity<T>().HasQueryFilter(e => e.TenantId == _tenantContext.TenantId)` configured once in the `DbContext`, so **every** LINQ query — including ones written by a developer who forgets tenancy exists — is automatically scoped. This converts "developer discipline" (unreliable at scale) into "structural guarantee."
- **PostgreSQL Row-Level Security (RLS):** a second, database-level layer independent of the application code. A Postgres session variable (`app.current_tenant`) is set per-connection/request, and RLS policies on every tenant table enforce `tenant_id = current_setting('app.current_tenant')::uuid`. This protects against the scenario where a bug, a raw-SQL escape hatch, or a compromised admin tool bypasses EF Core entirely — the database itself refuses to return other tenants' rows.
- **Cross-tenant attack prevention, concretely:**
  - A malicious/compromised hospital admin cannot enumerate other hospitals' patient IDs because list endpoints are always tenant-filtered server-side, never accepting a `tenantId` parameter from the client.
  - Even if an attacker obtains a valid JWT for Hospital A and tries to pass Hospital B's resource ID, the resource-based + tenant-based checks (Section 3) both fail independently.
  - Background jobs and reporting pipelines that legitimately need cross-tenant access (Super Admin analytics) run under an explicit "platform" context that bypasses the per-tenant RLS policy via a distinct, tightly audited database role — never by disabling RLS globally.

**Trade-off acknowledged:** RLS adds a small per-query overhead and requires session-variable discipline in connection pooling (must be set per logical request, not assumed to persist across pooled connections) — mitigated by setting it explicitly at the start of every unit of work rather than relying on connection state.

---

## 5. Authentication Security

- **JWT access tokens:** short-lived (10–15 min), signed RS256, contain `UserId`, `TenantId`, `Role`, permission claims — no PII beyond what's needed for authorization decisions.
- **Refresh Token Rotation:** every refresh exchange issues a brand-new refresh token and invalidates the old one immediately (one-time use).
- **Refresh Token Reuse Detection:** if a refresh token that was already rotated/invalidated is presented again, this is a strong signal of token theft (an attacker using a stolen, already-superseded token) — the entire token **family** is revoked and the user is force-logged-out on all devices, with a security alert triggered.
- **Token Revocation:** refresh tokens are stored (hashed) with status flags, allowing immediate server-side revocation — critical for "logout everywhere" and admin-forced logout on suspected compromise.
- **Device Sessions:** each refresh token is tied to a device/session fingerprint (user agent + rough device ID), allowing a "manage your sessions" screen and targeted single-device logout, not just all-or-nothing.
- **Logout from all devices:** revokes every active refresh token family for the user in one action.
- **Password Policy:** minimum length/complexity enforced server-side (never client-only), checked against a breached-password list (e.g., Have I Been Pwned k-anonymity API) to reject known-compromised passwords rather than just requiring "1 uppercase, 1 symbol" theater.
- **Password Expiration:** deliberately **not** enforced by default — modern NIST guidance (SP 800-63B) argues forced periodic rotation *reduces* security (users pick weaker, predictable variants). Recommendation: skip mandatory rotation; rely on breach-detection-triggered forced resets instead. Left as an optional, hospital-configurable policy for organizations with a compliance mandate requiring it.
- **Email Verification:** required before first login; unverified accounts cannot book/pay.
- **OTP Verification:** required as a step-up for sensitive actions (Super Admin operations, refund approval, first login from a new device).
- **MFA (future):** the auth data model already supports a `UserMfaMethods` table so TOTP/authenticator-app MFA is additive, not a redesign.

---

## 6. API Security

- **Rate Limiting & IP Throttling:** ASP.NET Core Rate Limiting middleware — stricter fixed-window limits on `/auth/*` endpoints, more generous token-bucket limits on general API traffic; limits keyed by IP **and** account to catch both distributed and single-account abuse.
- **API Versioning:** URL-segment (`/api/v1/...`) as established in the base architecture — old versions can be security-patched or deprecated on a clear timeline.
- **Request/Response Validation:** requests validated via FluentValidation before reaching handlers; responses shaped through explicit DTOs (never returning EF Core entities directly) so internal fields (e.g., password hash, internal notes) can never accidentally leak through model binding.
- **Correlation IDs:** every request gets a `TraceId`/`CorrelationId` (generated if not supplied) propagated through logs, background jobs, and even outbound calls to the payment gateway — essential for tracing a single suspicious transaction end-to-end during an investigation.
- **Idempotency Keys:** required on all payment-initiating and refund-initiating endpoints (client-generated `Idempotency-Key` header); the server stores the key with the resulting response for a window (e.g., 24h) and returns the original result on retry instead of re-executing — prevents duplicate charges from network retries or double-clicks.
- **API Documentation Security:** Swagger/OpenAPI UI is **disabled or authentication-gated in production** — publicly exposing full schema details aids attackers; available in staging/internal environments only.
- **CORS Configuration:** explicit allow-list of known frontend origins (no wildcard `*` in production, especially since credentials/tokens are involved); preflight caching tuned for performance without being overly permissive.
- **Security Headers:** HSTS, `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Content-Security-Policy`, `Referrer-Policy` applied via middleware to every response.

---

## 7. Input Validation Strategy

Client-side validation is treated purely as UX — **every** validation is re-enforced server-side:

- **DTO Validation:** FluentValidation validators for every Command/Query, run automatically via a pipeline behavior before a handler executes — type, length, format, required-field checks happen here.
- **Domain Validation:** deeper business invariants (e.g., "an appointment slot cannot be in the past," "a refund percentage must be 0–100") are enforced inside the Domain layer's aggregates/value objects, so they can never be bypassed even by a future internal caller that skips the Application layer's DTO validation.
- **File Validation:** covered in depth in Section 13.
- **SQL Parameters:** never built via string concatenation; EF Core LINQ or parameterized `FormattableString`-based raw SQL only when unavoidable.
- **JSON Validation:** strict deserialization (unknown fields ignored or rejected depending on endpoint sensitivity), payload size limits enforced by Kestrel to prevent oversized-payload DoS.
- **HTML Encoding:** any user-supplied free text that is ever rendered (review comments, notes) is HTML-encoded at output time, not just "sanitized" at input time (encoding at output is the more robust, context-aware defense — see Section 9).

---

## 8. Injection Protection

| Injection Type | Prevention |
|---|---|
| **SQL Injection** | EF Core parameterized queries by default; a project-wide lint/code-review rule bans string-concatenated SQL; the few raw-SQL exceptions (complex reporting queries) go through parameterized `FormattableString` interpolation, never `string.Format`, and are code-reviewed specifically for this. |
| **NoSQL Injection (future-proofing)** | If a NoSQL store (e.g., for logs/analytics) is added later, the same principle applies: query builders/ODMs with parameter binding, never constructing query documents via raw string/JSON concatenation from user input. |
| **Command Injection** | The application never shells out to OS commands based on user input; if a future feature requires it (e.g., a file conversion tool), input is strictly allow-listed and passed as discrete arguments to a process API, never through a shell interpreter. |
| **LDAP Injection** | Not applicable today (no LDAP integration); if enterprise SSO/LDAP is added later, inputs used in LDAP filters will be escaped using the standard LDAP escaping rules, never concatenated raw. |
| **XML Injection / XXE** | Any XML parsing (e.g., a future insurance-claim XML integration) uses parsers with DTD processing and external entity resolution **disabled by default** — this is the standard, non-negotiable XXE mitigation. |
| **CRLF Injection** | Header values built from user input (rare, but e.g., a custom filename in `Content-Disposition`) are validated/stripped of `\r\n` before being placed in response headers, preventing HTTP response splitting. |

---

## 9. XSS Protection

- **Stored XSS** (e.g., a malicious `<script>` in a review comment persisted and later rendered to other users): backend HTML-encodes all user-generated content at the point of rendering/serialization; the API itself, being JSON-only, doesn't render HTML — but the **contract** with the frontend is that any field flagged as free text is treated as untrusted and encoded by the frontend before insertion into the DOM.
- **Reflected XSS** (payload in a query param echoed back in an error message or search-results page): API never echoes raw, unencoded user input back in HTML responses (it doesn't serve HTML at all); if a future server-rendered admin panel exists, all interpolated values are auto-encoded by the templating engine (e.g., Razor's default HTML encoding — raw `Html.Raw()` banned for user content).
- **DOM XSS:** primarily a frontend responsibility — never insert user data into the DOM via `innerHTML`/`dangerouslySetInnerHTML`/similar without sanitization; use framework-safe binding (React's default text interpolation, which encodes automatically) instead of manual DOM manipulation.
- **Backend responsibility:** provide clean, correctly-typed JSON and a strict `Content-Security-Policy` header so that even if an encoding gap exists, injected scripts have a reduced ability to execute or exfiltrate data (e.g., disallowing inline scripts, restricting allowed script/style sources).
- **Frontend responsibility:** encode on output, use the framework's default escaping (avoid raw HTML injection APIs), and apply a client-side sanitizer library (e.g., DOMPurify) specifically for any feature that intentionally allows limited rich text (unlikely in v1, but relevant if free-text doctor notes ever support formatting).

---

## 10. CSRF Protection

- **Why JWT-based APIs carry lower CSRF risk:** CSRF exploits the browser's automatic inclusion of cookies on cross-site requests. SmartCare's API expects the JWT access token in an `Authorization: Bearer` header, which a malicious third-party site **cannot** force a browser to attach automatically (unlike cookies) — this alone neutralizes classic CSRF for the primary auth path.
- **When CSRF protection is still required:** if refresh tokens (or any session-identifying value) are ever stored in cookies for XSS-mitigation reasons (`HttpOnly` cookies are a legitimate, common pattern for refresh tokens specifically), then CSRF protection **is** required for any cookie-authenticated endpoint, since the "no automatic cookie attachment" argument no longer applies.
- **Recommendation for SmartCare:** access tokens in memory (Bearer header) + refresh tokens in an `HttpOnly`, `Secure`, `SameSite=Strict` cookie for web clients. `SameSite=Strict` alone blocks the vast majority of CSRF vectors for the refresh endpoint; as defense-in-depth, the refresh endpoint additionally requires a custom header (e.g., `X-Requested-With`) that simple cross-site form submissions cannot set, functioning as a lightweight anti-CSRF check without the full complexity of a traditional anti-forgery token scheme (appropriate given it's a token-issuing endpoint, not a full HTML form-based app).
- Native mobile clients are unaffected by CSRF entirely (no browser, no cookies) and use the Bearer-token flow exclusively.

---

## 11. Payment Security

- **PCI Scope Reduction (the most important decision):** SmartCare **never** touches raw card data. Card number, CVV, and any banking PIN are entered directly into the payment gateway's hosted field / SDK (e.g., Stripe Elements-equivalent), which returns only a **tokenized reference**. SmartCare's database stores only that gateway token/transaction reference, `PaymentStatus`, and amount — this keeps SmartCare largely out of PCI-DSS scope (SAQ-A-level) rather than inheriting the full compliance burden of handling cardholder data directly.
- **Gateway Verification:** every payment's status is confirmed by **querying the gateway's API directly** (or via a signed webhook, see below) — the client's "payment succeeded" message is treated as a hint to refresh state, never as proof.
- **Webhook Signature Validation:** every incoming gateway webhook is verified against the gateway's provided HMAC signature (using the raw request body, before any deserialization) — an unsigned or invalid-signature webhook is rejected outright. This prevents an attacker from forging a "payment succeeded" webhook to unlock a free appointment.
- **Idempotent Payments:** payment-initiation endpoints require an idempotency key (Section 6); additionally, a `Payments` row is created in a `Pending` state **before** calling the gateway, and the gateway's transaction reference is later reconciled against it — a retried request finds the existing pending/completed record instead of creating a duplicate charge.
- **Duplicate Payment Prevention:** a unique constraint on `(AppointmentId)` in `Payments` for non-failed statuses ensures one successful payment per appointment at the database level, beyond just idempotency-key deduplication.
- **Refund Security:** refund requests require permission-based authorization (`refunds.approve`, typically Receptionist/Hospital Admin, with an amount threshold that escalates to Super Admin approval) and are computed server-side via `RefundCalculationService` (Section 10 of the base document) — never a client-submitted amount.
- **Never stored:** card numbers, CVV, PINs, full magnetic-stripe/track data. Only gateway-issued references, last-4 digits (if returned for display, e.g., "Visa ending 4242" — itself provided by the gateway, not derived by SmartCare), and transaction metadata.

---

## 12. Race Condition Prevention (Double-Booking)

**Options:**

- **Optimistic Concurrency (row version/`xmin` check):** cheap, no locks held, works well for low-contention updates (e.g., editing a doctor's profile) — but for a genuinely hot slot (many patients hitting "book" on the same popular doctor's 9am opening simultaneously), it just converts the race into a flurry of retries/conflicts rather than gracefully serializing them.
- **Pessimistic Locking (`SELECT ... FOR UPDATE`):** guarantees correctness by serializing access to the contested row, but holding a DB lock across the full booking flow (which may include a synchronous payment-gateway round-trip) risks long lock waits and reduced throughput.
- **Distributed Locking (Redis/Redlock):** serializes the *business operation* (booking a specific slot) rather than a DB row, and — crucially — the lock scope can be kept short (just the "reserve the slot" decision), with the slower payment step happening *after* the slot is provisionally reserved, not while holding the lock.
- **Unique Database Constraint** (`UNIQUE(DoctorId, SlotStart) WHERE Status NOT IN ('Cancelled','Rejected')`): the ultimate, unbypassable safety net — even if application-level locking has a bug or a race window, the database itself will reject a second insert for the same slot.

**Recommendation:** a **combination**, layered:
1. A short-lived **Redis distributed lock** on the specific `(DoctorId, SlotId)` key wraps only the "reserve" decision — fast, released within milliseconds, before any payment call.
2. The reservation itself is written inside a **database transaction**.
3. A **unique constraint** on the slot/appointment table is the non-negotiable final backstop, catching anything the lock layer might miss under edge cases (e.g., a lock service outage/failover window).
4. Payment happens **after** a slot is provisionally reserved (short-hold, e.g., 5–10 minutes, auto-released by a background sweep if unpaid) — so the expensive/slow external call never happens inside the contested critical section.

This layered approach is standard for high-contention "reserve a limited resource" problems (the same pattern used for concert ticket booking) and avoids both the correctness gaps of optimistic-only approaches and the throughput cost of holding pessimistic locks across slow I/O.

---

## 13. Secure File Upload

(Applies now to any profile photos/documents, and is the foundation for future prescriptions/lab reports.)

- **Allowed Extensions:** strict allow-list (e.g., `.jpg`, `.png`, `.pdf` for documents) — never a block-list, since block-lists are always incomplete.
- **MIME Validation:** validate the **actual file content** (magic-byte/signature check), not just the `Content-Type` header or extension, which are trivially spoofed by an attacker renaming a `.exe` to `.pdf`.
- **Virus Scanning:** every upload is scanned (e.g., ClamAV or a cloud provider's malware-scanning service) asynchronously before being marked available; files are quarantined until the scan clears.
- **Maximum File Size:** enforced both at the reverse proxy/gateway level and application level, preventing storage exhaustion and slow-loris-style upload DoS.
- **Random File Names:** stored filenames are server-generated GUIDs, never the client-supplied name (prevents path traversal via crafted filenames like `../../etc/passwd` and avoids leaking any information embedded in the original filename).
- **Secure Storage:** files live in blob storage **outside the web root**, never directly served by the web server's static file path; the storage bucket itself is private (no public read).
- **Download Authorization:** every download goes through an authenticated endpoint that performs the same resource/tenant/ownership checks as any other resource (Section 3), then issues a short-lived signed URL — direct, unauthenticated blob URLs are never exposed.
- **Never allow executable files:** `.exe`, `.sh`, `.bat`, `.dll`, `.js` (as an uploadable "document"), and similar are always rejected regardless of stated purpose.

---

## 14. Sensitive Data Classification & Protection

| Data Class | Examples | Protection |
|---|---|---|
| **Critical / Regulated** | Passwords, National ID, medical history/notes (future EMR) | Passwords: salted hash (Argon2id/PBKDF2), never encrypted-and-reversible. National ID / medical data: column-level encryption at rest (EF Core value converters + KMS-managed keys), access logged on every read (Section 15), restricted by permission claims beyond normal role access. |
| **Financial** | Payment gateway references, refund records | Never raw card data (Section 11); gateway references treated as sensitive-but-not-secret (can't be used to charge a card directly, but still access-controlled and audit-logged). |
| **Personal / Contact** | Phone numbers, email addresses | Encrypted at rest where regulation/hospital policy demands it; always transmitted over TLS; masked in logs (e.g., `j***@example.com`) so operational logs don't become a secondary PII store. |
| **Operational / Low Sensitivity** | Appointment times, department names, review star ratings | Standard access control, no special encryption needed beyond platform-wide TLS/at-rest disk encryption. |

- **Encryption in Transit:** TLS 1.2+ enforced everywhere (API, DB connections, cache connections, third-party integrations); HTTP disabled entirely in production.
- **Encryption at Rest:** managed disk/volume encryption for the database and blob storage by default (cloud-provider level), **plus** application-level column encryption for the "Critical/Regulated" tier specifically — a stolen disk snapshot alone should not expose National IDs or future medical notes even if at the infrastructure layer.
- **Hashing:** one-way, salted hashing exclusively for passwords — never reversible encryption for anything used as a credential.
- **Key Rotation:** encryption keys (for column-level encryption) are versioned and rotated on a defined schedule (e.g., annually, or immediately on suspected compromise) via the KMS/Key Vault, with old-key-decrypt/new-key-encrypt support so rotation doesn't require a disruptive full data re-migration in one step — new writes use the new key, old data is re-encrypted opportunistically or via a background migration job.

---

## 15. Logging & Audit

- **Immutability:** `AuditLogs` is append-only at the database-permission level — the application's runtime DB role has `INSERT` but no `UPDATE`/`DELETE` grant on this table; only a separate, break-glass administrative role (used for legitimate retention-policy purges only, itself logged) can remove rows, and only after a retention period expires.
- **Tracked events (minimum):** Login/Logout (success and failure), Appointment status changes, Payment actions, Refunds, Role/Permission changes, Failed login attempts, Data exports, and access to sensitive records (e.g., a Receptionist opening a patient's full profile — access itself is logged, not just modifications, which is a common HIPAA-style audit expectation).
- **Log fields:** `UserId`, `TenantId`, `IPAddress`, `DeviceFingerprint`, `TimestampUtc`, `CorrelationId`, `Action`, `EntityType`/`EntityId`, and a before/after snapshot for the specific fields that changed (not a full entity dump, to avoid the log itself becoming an oversized secondary PII store).
- **Separation from operational logs:** operational/Serilog logs (for debugging, performance) live in a different store with a shorter retention than the compliance-grade `AuditLogs` table, which is retained per the data-retention policy (Section 18) regardless of operational log rotation.

---

## 16. Monitoring & Incident Detection

| Signal | Detection Approach | Alert Action |
|---|---|---|
| Failed logins | Counter per account/IP over a rolling window | Threshold breach → temporary lockout + security team alert |
| Suspicious login location | Compare login IP geolocation to the user's recent login history | Step-up OTP challenge; notify the user via email of a new-location login |
| High cancellation rate | Per-patient or per-hospital rate vs. historical baseline | Feeds the Fraud Risk Score (Section 17); dashboard flag for hospital admin |
| High refund rate | Per-hospital refund $ / total payment $ ratio vs. baseline | Alert to Super Admin fraud-review queue |
| Brute force attempts | Rate limiter rejection counts on `/auth/*` | Auto-block at WAF/edge after sustained abuse from an IP/subnet |
| API abuse | Rate-limit rejections, abnormal request patterns (e.g., sequential ID enumeration) | Alert + optional temporary IP ban |
| Slow queries | APM/query-duration histograms (Section 12 of base doc, `PerformanceBehavior`) | Alert on p95/p99 threshold breach; feeds capacity planning |
| High CPU/Memory | Standard infrastructure metrics (container/orchestrator level) | Autoscale trigger; alert if sustained beyond autoscale ceiling |
| Payment failures | Failure-rate spike per gateway/per hospital | Alert — could indicate a gateway outage or a fraud attempt pattern (e.g., card testing) |

**Alerting strategy:** tiered severity (info → dashboard only; warning → on-call Slack/Teams notification; critical → paging) via a standard observability stack (Application Insights/Datadog/Grafana+Prometheus, cloud-agnostic choice deferred to deployment target), with alert rules defined as code (versioned, reviewed) rather than configured ad hoc in a UI.

---

## 17. Fraud Detection

A dedicated, additive module (`SmartCare.Domain.Fraud` / `SmartCare.Application.Fraud`) that **scores rather than blocks**, per the requirement:

**Signals monitored:**
- Multiple accounts sharing a device fingerprint, phone number, or payment method (duplicate-detection joins across `Patients`, without necessarily merging or blocking them automatically).
- Repeated No-Show patterns feeding into (and correlated with, but distinct from) the Attendance Score.
- Excessive booking velocity (e.g., many bookings across many doctors/hospitals in a short window — a bot/scalping pattern).
- Suspicious refund request patterns (e.g., booking → immediate cancellation right at the edge of the refund window, repeated).
- Bot-like request timing/headers (server-side heuristics; a CAPTCHA challenge can be triggered for public, unauthenticated endpoints like registration/search if velocity crosses a threshold).

**Output: a `FraudRiskScore` (0–100) per patient**, computed by a background job/event-driven recalculation (similar pattern to Attendance Score in the base architecture) — **never** an automatic ban. Consumers of the score:
- Booking flow may require additional verification (OTP, deposit) above a risk threshold.
- Super Admin gets a **review queue** for high-risk accounts, with the actual decision (warn, restrict, suspend) remaining a human action, logged and reversible — avoiding false-positive harm to legitimate patients (e.g., a patient who genuinely had a family emergency causing repeated cancellations shouldn't be auto-banned).

---

## 18. Data Retention

- **Soft Delete:** default for all clinically/financially relevant entities (Patients, Doctors, Appointments, Hospitals) — an `IsDeleted`/`DeletedAtUtc` flag hides the record from normal queries (via the same global-filter mechanism used for tenancy) without destroying it.
- **Hard Delete:** reserved for genuinely non-sensitive, low-value data (e.g., an expired, never-verified OTP code) or executed only via an explicit, audited "right to erasure" workflow (GDPR-style), which itself is logged and subject to legal-hold checks (e.g., a patient's erasure request cannot silently delete records still needed for an open billing dispute or legal retention requirement — this is resolved by anonymization rather than deletion where retention is legally mandated).
- **Never permanently deleted (per requirement):** Payments, Appointments, Audit Logs — enforced both by database constraints (`ON DELETE RESTRICT` from the base document) and by application-level policy (no exposed "hard delete" command exists for these aggregates at all).
- **Archive Strategy:** old, closed-out data (e.g., appointments older than N years with no open disputes) is moved to a cheaper "archive" storage tier/table (or cold object storage as exported records) rather than kept in the hot operational tables indefinitely — improves query performance on the active dataset while preserving the record for compliance.

---

## 19. Backup & Disaster Recovery

- **Daily automated backups** at minimum, with **Point-in-Time Recovery (PITR)** enabled via PostgreSQL WAL archiving — allows restoring to any point within the retention window (e.g., "5 minutes before the bad migration ran"), not just to the last nightly snapshot.
- **Backup Encryption:** backups encrypted at rest with the same rigor as production data (managed KMS keys), and access to restore from a backup is itself a permissioned, audited action.
- **Backup Verification:** automated periodic restore-and-validate jobs (restoring a backup into an isolated environment and running integrity checks) — an unverified backup is not a real backup; this catches silent corruption before it's needed in an actual emergency.
- **Recovery Testing:** scheduled disaster-recovery drills (at minimum annually, ideally quarterly) simulating full-region loss, validating that the documented recovery runbook actually works end-to-end, not just in theory.
- **Failover Strategy:** a warm/standby replica in a secondary availability zone (and, budget permitting, a secondary region) for the database; stateless API containers redeploy trivially in a failover region since they hold no local state.
- **RTO/RPO targets (recommended starting point for SmartCare, to be refined with the business):**
  - **RPO (Recovery Point Objective): ≤ 5 minutes** — achievable via continuous WAL streaming to the standby/PITR archive, meaning at most a few minutes of transactions could be lost in a worst-case failure.
  - **RTO (Recovery Time Objective): ≤ 1 hour** for a full regional failover in early stages (single active hospital), tightening toward **≤ 15 minutes** as the platform matures and justifies investment in automated failover orchestration rather than a manual runbook.

---

## 20. DevSecOps

- **Static Code Analysis (SAST):** Roslyn analyzers + a dedicated security-focused SAST tool (e.g., SonarQube/Semgrep) run on every PR, blocking merge on high-severity findings.
- **Dependency Vulnerability Scanning:** Dependabot/Snyk/`dotnet list package --vulnerable` integrated into CI, with a policy of no critical/high CVEs shipped to production without an explicit, time-boxed exception.
- **Secret Scanning:** pre-commit hooks and CI-level scanning (e.g., gitleaks/GitHub secret scanning) to catch accidentally committed connection strings, API keys, or JWT signing keys before they ever reach a shared branch.
- **Security Testing (DAST):** automated dynamic scanning (e.g., OWASP ZAP) against a staging environment as part of the pipeline, catching runtime issues static analysis can't (misconfigured headers, live auth bypass attempts).
- **Container Image Scanning:** every built image scanned (Trivy/Grype) for OS-level and dependency-level vulnerabilities before push to the registry; base images pinned and updated on a regular cadence rather than "latest".
- **CI/CD Security:** least-privilege pipeline service accounts (a deploy pipeline shouldn't have standing production database admin rights), signed/protected release branches, mandatory PR review + passing security gates before merge to `main`, secrets injected at deploy time from a vault rather than stored in pipeline config.
- **Infrastructure as Code Security:** IaC (Terraform/Bicep) scanned (e.g., Checkov/tfsec) for misconfigurations (public S3-equivalent buckets, overly permissive security groups) before apply; all infra changes go through the same PR review process as application code.

---

## 21. Testing Strategy

| Test Type | What It Validates |
|---|---|
| **Unit Tests** | Domain invariants and Application handler logic in isolation (e.g., "an Appointment cannot transition from Pending directly to Completed") — fast, no external dependencies, run on every commit. |
| **Integration Tests** | Real interaction with PostgreSQL (via Testcontainers) — EF Core mappings, migrations, global query filters, and RLS policies actually behave as designed against a real database engine, not an in-memory fake that can hide provider-specific bugs. |
| **Functional Tests** | End-to-end API behavior through real HTTP calls against a running test instance — full request → auth → handler → DB → response pipeline, including middleware (tenant resolution, exception handling). |
| **API (Contract) Tests** | Ensures the API's request/response shape matches its documented contract (OpenAPI spec) — catches breaking changes before they reach frontend/mobile/third-party integrators. |
| **Performance Tests** | Measures response times and resource usage under expected normal load — establishes baselines and catches regressions per release. |
| **Load Tests** | Validates behavior under expected **peak** load (e.g., a hospital's morning booking rush) — confirms autoscaling and connection pooling hold up, not just "does it work," but "does it work at scale." |
| **Stress Tests** | Pushes well beyond expected peak to find the actual breaking point and confirm **graceful degradation** (proper error responses, no data corruption) rather than catastrophic failure. |
| **Security Tests** | Automated checks for the specific mitigations in this document — e.g., an automated test that asserts Patient A's token cannot fetch Patient B's appointment (IDOR regression test), that a forged/unsigned webhook is rejected, that rate limits actually trigger. |
| **Penetration Tests** | Periodic (at minimum annually, and before major releases/compliance milestones) third-party manual penetration testing — automated tooling and internal tests don't replace an adversarial human tester probing business-logic-level flaws (e.g., creative abuse of the refund policy engine) that automated scanners typically miss. |

---

## 22. Compliance Posture (HIPAA/GDPR-Aligned)

SmartCare is designed to **evolve into** formal HIPAA/GDPR compliance without architectural rework, without over-engineering for certifications not yet pursued:

- **Data Privacy & Minimization:** only data genuinely needed for the current feature set is collected (e.g., no speculative "just in case" medical fields before EMR is actually built); each new field added to a patient record should be justifiable against an actual use case.
- **Consent Management:** notification preferences and any future data-sharing (e.g., with an insurance module) are modeled as explicit, timestamped consent records — not implied consent — so a compliance audit can show exactly what a user agreed to and when.
- **Least Privilege:** already embedded architecturally (Section 3's layered authorization, narrow database role permissions) — this is a HIPAA "minimum necessary" principle applied consistently, not a separate bolt-on.
- **Data Subject Rights (GDPR-style):** the soft-delete/anonymization approach (Section 18) gives a concrete mechanism for "right to erasure" requests that doesn't conflict with financial/legal retention obligations; audit logs of who accessed a given patient's record support "right to access" transparency requests.
- **Business Associate / Data Processing Agreements:** architecturally, this means every third-party integration (payment gateway, SMS provider, cloud host) is chosen with an eye toward whether they offer a BAA (HIPAA) / DPA (GDPR) — a procurement/legal concern the architecture supports by keeping third-party integrations behind clean Infrastructure adapters (Section 3 of the base document), so swapping a non-compliant vendor for a compliant one later is a contained change.
- **Audit Requirements:** the immutable `AuditLogs` design (Section 15) directly satisfies the kind of "who accessed what, when" trail that both HIPAA and GDPR audits expect.
- **Recommendation:** treat formal HIPAA/GDPR **certification** as a business/legal milestone to pursue when SmartCare targets specific markets requiring it (e.g., US hospitals), rather than claiming compliance prematurely — but build (as this document does) so that reaching certification is a **process and documentation** effort layered on top of already-sound architecture, not a re-architecture.

---

## Summary of Key Security Recommendations

1. **Never trust a single layer.** Every sensitive operation is checked at multiple independent layers (JWT → RBAC → Permission → Resource ownership → Tenant filter → RLS) so that one bypassed control doesn't equal a breach.
2. **Server is the source of truth for money and state.** Prices, refund amounts, and appointment status transitions are always computed/validated server-side, never trusted from the client — this single principle prevents a large share of the "Tampering" and "Payment Security" risk categories.
3. **PCI scope is minimized by design** — SmartCare never touches raw card data, only gateway tokens.
4. **Double-booking is solved with layered concurrency control** (short-lived distributed lock → transaction → unique constraint), not any single mechanism alone.
5. **Fraud detection scores; humans decide.** No automatic account bans — this protects legitimate patients from false positives while still surfacing risk to Super Admins.
6. **Audit logs are immutable and comprehensive**, satisfying both incident-response needs and future HIPAA/GDPR audit expectations without waiting for a compliance mandate to force the issue.
7. **Compliance is a natural extension of good architecture here**, not a bolt-on — least privilege, data minimization, and consent tracking are already load-bearing parts of the design.

---

*This document should be read alongside SmartCare-Architecture.md. Recommended next step: a formal threat-model workshop with the engineering team to validate this STRIDE analysis against the actual implementation as the Domain and Application layers are built, followed by incorporating the Section 21 security test suite into the CI pipeline from the very first sprint — retrofitting security tests after the fact is far more expensive than building them alongside the first vertical slice.*
