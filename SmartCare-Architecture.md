# SmartCare — Enterprise Architecture Design Document

**Prepared as:** Principal Architecture Blueprint
**Stack:** ASP.NET Core (.NET 9), Clean Architecture, DDD, CQRS, PostgreSQL, EF Core
**Scope:** Multi-tenant SaaS healthcare platform (single-hospital launch → thousands of hospitals)

---

## Table of Contents

1. Architectural Philosophy & Guiding Decisions
2. Multi-Tenancy Strategy
3. Clean Architecture Layering & Solution Structure
4. Domain-Driven Design Model
5. Database Design
6. CQRS, Repository & Unit of Work Strategy
7. API Design
8. Security Architecture
9. Appointment & Attendance-Score Engine
10. Refund & Cancellation Policy Engine
11. Notification Architecture
12. Logging, Monitoring & Auditing
13. Performance & Scalability Strategy
14. Deployment Architecture
15. Future-Proofing Roadmap

Each decision below follows the same format: **Options → Trade-offs → Recommendation**, as requested.

---

## 1. Architectural Philosophy & Guiding Decisions

Before touching layers or tables, three foundational decisions shape everything else:

| Decision | Options Considered | Recommendation | Why |
|---|---|---|---|
| Architecture style | Layered (N-Tier), Clean Architecture, Modular Monolith, Microservices | **Clean Architecture inside a Modular Monolith** | Microservices add operational cost (service mesh, distributed transactions, observability overhead) that is unjustified at launch. Clean Architecture with strict module boundaries lets you **extract services later** (Payments, Notifications, EMR) without a rewrite — you get the modularity benefit without the Day-1 tax. |
| Tenancy readiness | Retrofit later vs. design-in from Day 1 | **Design-in from Day 1** | Retrofitting `TenantId` into every table, query, and cache key after the first hospital goes live is one of the most expensive mistakes in SaaS engineering. It is added now at near-zero marginal cost. |
| Query/Command split | Plain CRUD services vs. full CQRS+Event Sourcing vs. CQRS-lite | **CQRS-lite (MediatR, same database)** | Full event sourcing is overkill for v1 and complicates auditing in a way that actually fights the "Audit Log" requirement rather than helping it. CQRS-lite gives read/write separation, thin controllers, and a natural home for validation/pipeline behaviors, without the operational burden of separate read/write stores. Can evolve into true read-replicas later. |

**Guiding principles applied throughout:**
- Dependency Rule: dependencies always point **inward** (Presentation/Infrastructure → Application → Domain). Domain has zero external references.
- Every module is a vertical slice (Appointments, Payments, etc.) so it can later become its own microservice/bounded context with minimal friction.
- Nothing hospital-specific is hardcoded; everything is configuration or data (schedule rules, refund rules, notification channels).

---

## 2. Multi-Tenancy Strategy

### Options

**A. Separate Database per Hospital**
- ✅ Strongest isolation, easiest per-tenant backup/restore, easiest to satisfy a hospital that demands "our data lives alone" (common in healthcare procurement).
- ❌ Expensive at scale (thousands of hospitals = thousands of databases), harder connection pooling, migrations must run N times, cross-tenant analytics (Super Admin dashboards) become a distributed query problem.

**B. Shared Database / Separate Schema**
- ✅ Better isolation than fully shared tables; per-tenant schema means you *could* still restore one tenant's schema independently.
- ❌ PostgreSQL/most ORMs handle dynamic schema-per-tenant awkwardly with EF Core migrations; connection/schema-switching adds complexity; still doesn't scale cleanly to "thousands" without significant tooling investment.

**C. Shared Database / Shared Schema with `TenantId` discriminator**
- ✅ Cheapest to run, easiest to migrate (one schema, one migration), easiest for Super Admin cross-tenant analytics, scales to thousands of tenants on a handful of database servers with sharding added later only if needed.
- ❌ Requires rigorous discipline: every query MUST be tenant-filtered or you get data leaks. Noisy-neighbor risk (one hospital's heavy load can affect another) unless mitigated.

### Recommendation: **Shared Database / Shared Schema**, with a global query filter and a path to hybrid isolation later.

Rationale specific to SmartCare:
- The stated goal is "thousands of hospitals" — only the shared-schema model realistically operates at that scale without a dedicated platform team.
- Data-leak risk is mitigated architecturally, not by developer discipline alone:
  - **EF Core Global Query Filters** (`HasQueryFilter(e => e.TenantId == _currentTenant.Id)`) applied to every tenant-scoped entity at the `DbContext` level — impossible to forget in a new query.
  - A **`ITenantContext`** service resolved per-request from the JWT claims (never from a client-supplied header alone) sets the ambient tenant.
  - PostgreSQL **Row-Level Security (RLS)** as a defense-in-depth second layer — even a bypassed EF filter (e.g. raw SQL) is blocked at the database level.
  - Every table that is tenant-owned includes `TenantId` as the **leading column in composite indexes**, so tenant isolation is also a performance win (index locality).
- **Escape hatch for large/enterprise hospitals:** the design keeps `TenantId` as a real column (not baked into table names), which means a specific large hospital can later be migrated to its own database or schema (option A/B) via a data-movement job, without changing application code — the `ITenantContext`/connection-resolution layer simply routes that tenant differently. This gives you a **hybrid model on demand** rather than an all-or-nothing choice today.
- `Super Admin` platform-level entities (Hospitals, Subscriptions, Platform Config) live in a separate **non-tenant "Platform" schema**, since they are not owned by any single tenant.

---

## 3. Clean Architecture Layering & Solution Structure

### Layer Responsibilities

**Domain Layer (`SmartCare.Domain`)**
- Pure C#, zero external dependencies (not even EF Core).
- Contains Entities, Value Objects, Aggregates, Domain Events, Domain Exceptions, Enumerations (as classed enums, not raw `enum`, to support behavior like `AppointmentStatus.CanTransitionTo(...)`), and Repository *interfaces* (not implementations).
- Enforces invariants: an `Appointment` cannot be marked `Completed` unless it was `CheckedIn` first — that rule lives here, not in a service.

**Application Layer (`SmartCare.Application`)**
- Orchestrates use cases via CQRS **Commands** and **Queries** (MediatR).
- Contains: Command/Query handlers, DTOs, Validators (FluentValidation), interfaces for infrastructure concerns it needs (`IEmailSender`, `IPaymentGateway`, `IDateTimeProvider`, `ICurrentUserService`, `ITenantContext`), Pipeline Behaviors (validation, logging, transaction, caching), AutoMapper/Mapster profiles.
- Depends only on Domain. Knows *nothing* about EF Core, HTTP, or PostgreSQL — it depends on abstractions it defines, which Infrastructure fulfills (Dependency Inversion).

**Infrastructure Layer (`SmartCare.Infrastructure`)**
- Implements Application's interfaces: EF Core `DbContext` + repository implementations, `Unit of Work`, external service integrations (email/SMS providers, payment gateway SDKs, JWT token generation, Redis cache, file storage).
- This is the *only* layer allowed to know about PostgreSQL, EF Core, Serilog sinks, third-party SDKs.
- Split further into `SmartCare.Infrastructure.Persistence`, `SmartCare.Infrastructure.Identity`, `SmartCare.Infrastructure.Notifications`, `SmartCare.Infrastructure.Payments` — so a future swap (e.g., Twilio → different SMS provider) touches one small project.

**Presentation Layer (`SmartCare.WebApi`)**
- ASP.NET Core Web API. Thin controllers that only translate HTTP → MediatR command/query → HTTP response.
- Owns: Middleware (exception handling, tenant resolution, rate limiting), API versioning, Swagger/OpenAPI, authentication/authorization wiring, health checks.
- No business logic — a controller action is typically 3–5 lines.

**Shared/Common Layer (`SmartCare.SharedKernel`)**
- Cross-cutting primitives with no business meaning of their own: `Result<T>` pattern, `PagedList<T>`, base `Entity`/`AggregateRoot`/`ValueObject` classes, `DomainEvent` base class, custom `Guard` clauses, common constants.
- Referenced by Domain and Application (it's "beneath" them conceptually, not a violation of the dependency rule since it contains no business rules).

### Dependency Direction

```
SmartCare.WebApi  ──────────┐
SmartCare.Infrastructure ───┼──► SmartCare.Application ──► SmartCare.Domain ──► SmartCare.SharedKernel
                             │
   (Composition Root wires everything together in Program.cs)
```

Only `SmartCare.WebApi` (the composition root) references `Infrastructure` directly for DI registration. Infrastructure never references WebApi. This ensures Domain and Application remain fully unit-testable in isolation, and Infrastructure can be replaced (e.g., swap PostgreSQL for another provider) without touching business logic.

### Solution Structure

```
SmartCare.sln
│
├── src/
│   ├── Core/
│   │   ├── SmartCare.Domain/
│   │   │   ├── Common/                  (AggregateRoot, Entity, ValueObject, DomainEvent)
│   │   │   ├── Tenancy/                 (Hospital, Subscription, TenantId VO)
│   │   │   ├── Identity/                (User, Role, Permission)
│   │   │   ├── Appointments/            (Appointment aggregate, AppointmentStatus, Slot VO)
│   │   │   ├── Doctors/
│   │   │   ├── Patients/
│   │   │   ├── Departments/
│   │   │   ├── Payments/
│   │   │   ├── Reviews/
│   │   │   ├── Attendance/
│   │   │   ├── Notifications/
│   │   │   └── Exceptions/
│   │   │
│   │   └── SmartCare.Application/
│   │       ├── Common/
│   │       │   ├── Behaviors/           (ValidationBehavior, LoggingBehavior, TransactionBehavior)
│   │       │   ├── Interfaces/           (IApplicationDbContext, IEmailSender, ITenantContext...)
│   │       │   └── Models/               (Result, PagedList, ApiResponse)
│   │       ├── Hospitals/{Commands,Queries}
│   │       ├── Appointments/{Commands,Queries}
│   │       ├── Doctors/{Commands,Queries}
│   │       ├── Patients/{Commands,Queries}
│   │       ├── Payments/{Commands,Queries}
│   │       ├── Notifications/
│   │       └── ...one folder per bounded context, vertical-slice style
│   │
│   ├── Infrastructure/
│   │   ├── SmartCare.Infrastructure.Persistence/  (DbContext, Configurations, Migrations, Repos, UoW)
│   │   ├── SmartCare.Infrastructure.Identity/     (JWT, refresh tokens, password hashing)
│   │   ├── SmartCare.Infrastructure.Notifications/(Email, SMS, Push adapters)
│   │   ├── SmartCare.Infrastructure.Payments/     (Gateway adapters, refund processor)
│   │   └── SmartCare.Infrastructure.BackgroundJobs/ (Hangfire/Quartz jobs)
│   │
│   ├── Presentation/
│   │   └── SmartCare.WebApi/
│   │       ├── Controllers/v1/
│   │       ├── Middleware/               (TenantResolution, ExceptionHandling, RateLimiting)
│   │       ├── Filters/
│   │       ├── Extensions/               (DI registration per layer)
│   │       └── Program.cs
│   │
│   └── SharedKernel/
│       └── SmartCare.SharedKernel/
│
└── tests/
    ├── SmartCare.Domain.UnitTests/
    ├── SmartCare.Application.UnitTests/
    ├── SmartCare.Infrastructure.IntegrationTests/
    └── SmartCare.WebApi.FunctionalTests/
```

---

## 4. Domain-Driven Design Model

### Bounded Contexts

1. **Identity & Access** — Users, Roles, Permissions, Refresh Tokens
2. **Tenancy & Subscription** — Hospitals, Subscription Plans, Billing Cycles
3. **Clinical Directory** — Departments, Doctors, Doctor Schedules, Leave
4. **Patient Management** — Patients, Medical Profile (minimal now, EMR-ready later)
5. **Scheduling & Appointments** — Appointments, Queue, Slots
6. **Payments & Refunds** — Payments, Refunds, Cancellation Policies
7. **Reputation** — Reviews, Ratings, Attendance Score
8. **Notifications** — Templates, Delivery Log
9. **Platform Administration** — Super Admin, Audit Log, System Configuration

### Aggregates, Entities, Value Objects (key examples)

**Aggregate: `Appointment`** (Aggregate Root: `Appointment`)
- Entities inside: none needed separately (kept small deliberately — an Appointment doesn't need child entities like "AppointmentLine").
- Value Objects: `TimeSlot` (Start, End — immutable, validates End > Start), `AppointmentStatus` (smart enum with legal-transition rules), `Money` (Amount + Currency, used for fee snapshot).
- Invariants enforced inside the aggregate: cannot transition `Pending → Completed` directly; cannot check-in before Confirmed; cannot double-book a `TimeSlot` for the same doctor (enforced via a domain service + DB unique constraint as a second guard).
- Domain Events raised: `AppointmentBookedEvent`, `AppointmentConfirmedEvent`, `AppointmentCancelledEvent`, `AppointmentCompletedEvent`, `AppointmentNoShowEvent`. These events are what the **Notification module** and **Attendance Score module** subscribe to — this is how modules stay decoupled (Appointments module has zero knowledge that Attendance Score exists).

**Aggregate: `Hospital`**
- Value Objects: `Address`, `ContactInfo`, `SubscriptionTier`.
- Owns the `CancellationPolicy` value object (window, refund %, penalty amount) — configurable per hospital as required.

**Aggregate: `DoctorSchedule`**
- Entity: `ScheduleSlot` (child entity, since slots have identity and lifecycle within a schedule but aren't meaningful outside it).
- Value Object: `TimeSlot`, `RecurrenceRule`.
- Invariant: slots cannot overlap for the same doctor.

**Aggregate: `Patient`**
- Value Objects: `AttendanceScore` (a calculated, bounded value object 0–100 with its own rules for how it moves), `EmergencyContact`.

**Aggregate: `Payment`**
- Value Objects: `Money`, `PaymentStatus`.
- Raises `PaymentVerifiedEvent`, `RefundIssuedEvent`.

**Domain Services** (logic that doesn't belong to one aggregate):
- `SlotAvailabilityService` — checks a doctor's schedule + existing appointments to compute free slots (spans Doctor + Appointment aggregates).
- `RefundCalculationService` — applies a hospital's `CancellationPolicy` against an appointment's cancellation time to compute refund amount (spans Hospital + Appointment + Payment).
- `AttendanceScoreService` — recalculates score based on Completed/NoShow/Cancelled history.

**Specifications** (using the Specification pattern for reusable, composable query logic):
- `DoctorAvailableOnDateSpec`, `UpcomingAppointmentsForPatientSpec`, `HospitalActiveSubscriptionSpec` — used both for filtering in queries and validating business rules, avoiding logic duplication between the read (CQRS query) side and the write (command) side.

**Repositories** (interfaces in Domain, one per Aggregate Root only — never per entity):
- `IAppointmentRepository`, `IHospitalRepository`, `IDoctorRepository`, `IPatientRepository`, `IPaymentRepository`. Child entities (e.g., `ScheduleSlot`) are never exposed via their own repository — they're only reachable through their aggregate root, preserving invariants.

---

## 5. Database Design (Conceptual — no SQL yet, as requested)

### Core Tables and Why They Exist

| Table | Purpose | Key Relationships |
|---|---|---|
| `Hospitals` | One row per tenant; central anchor for all tenant-scoped data | Parent to almost everything via `TenantId` |
| `Subscriptions` | Tracks plan, billing cycle, status per hospital | 1:N from Hospital |
| `Users` | Single identity table for all human actors (Super Admin, Receptionist, Doctor, Patient) with a `UserType` discriminator | Referenced by `Doctors`, `Patients`, `Receptionists` as 1:1 extension tables |
| `Roles` / `Permissions` / `RolePermissions` / `UserRoles` | Normalized RBAC + fine-grained permission model | Many-to-many join tables |
| `Departments` | Hospital's clinical departments (Cardiology, etc.) | N:1 to Hospital |
| `Doctors` | Extension of `Users` with clinical attributes | 1:1 with Users, N:1 with Hospital & Department |
| `DoctorSchedules` | Recurring/one-off availability windows | N:1 to Doctor |
| `ScheduleSlots` | Materialized bookable slots (generated from schedule rules) | N:1 to DoctorSchedule |
| `Patients` | Extension of `Users` with patient-specific data | 1:1 with Users |
| `Appointments` | Central transactional table | N:1 to Patient, Doctor, Hospital, Department; 1:1 to Payment |
| `Payments` | Payment attempts and outcomes | 1:1 to Appointment |
| `Refunds` | Refund transactions, separate from Payments for clean audit trail | N:1 to Payment |
| `CancellationPolicies` | Per-hospital configurable refund rules | 1:1 or 1:N to Hospital (versioned, so historical appointments use the policy that was active at booking time) |
| `Reviews` | Doctor/Hospital ratings | N:1 to Appointment (one review per completed appointment) |
| `AttendanceScores` | Current + historical score snapshots per patient | 1:1 current, 1:N history, to Patient |
| `NotificationTemplates` / `NotificationLogs` | Template content + delivery audit trail | N:1 to Hospital (templates can be overridden per-hospital, default at platform level) |
| `AuditLogs` | Immutable record of sensitive actions | Polymorphic reference (EntityType + EntityId) to any auditable table |
| `RefreshTokens` | Token rotation/revocation tracking | N:1 to User |
| `SystemConfigurations` | Platform-wide and per-hospital feature flags/settings | Nullable `TenantId` (null = platform default) |

### Why `Users` is unified rather than four separate tables
A single `Users` table with a `UserType` discriminator (Doctor/Patient/Receptionist/SuperAdmin) plus **thin extension tables** (`Doctors`, `Patients`, `Receptionists` holding only role-specific columns) avoids duplicating authentication concerns (password hash, email verification, refresh tokens, lockout state) four times. This is the standard "Class Table Inheritance" pattern and keeps the Identity bounded context genuinely unified — one login endpoint, one password policy, one MFA implementation, regardless of user type.

### Indexing Strategy
- Every tenant-scoped table: composite index leading with `TenantId` (e.g., `(TenantId, DoctorId, StartTime)` on `Appointments`) — this is both the tenancy safety net and the performance-critical index since almost every query filters by tenant first.
- `Appointments`: unique constraint on `(DoctorId, TimeSlotStart)` where status is not Cancelled/Rejected — prevents double-booking at the database level as a last line of defense beyond domain logic.
- `Users.Email`: unique index (globally, since login is platform-wide, not per-tenant — a doctor could theoretically... though in v1 a user belongs to one hospital; documented as a constraint to revisit if a doctor ever works across hospitals).
- `RefreshTokens.TokenHash`: unique index for O(1) lookup and immediate revocation checks.
- `AuditLogs`: index on `(EntityType, EntityId)` and `(TenantId, CreatedAt)` for both entity-history lookups and time-range compliance queries.

### Constraints & Cascade Rules
- `Appointments → Patient/Doctor/Hospital`: `ON DELETE RESTRICT` — you must never hard-delete a Patient/Doctor/Hospital that has appointment history; use **soft delete** (`IsDeleted` + `DeletedAtUtc`) everywhere in this domain, both for audit/legal (medical record retention laws) and referential safety.
- `Payments → Appointment`: `ON DELETE RESTRICT`, same reasoning — financial records are never hard-deleted.
- `RefreshTokens → User`: `ON DELETE CASCADE` — tokens are meaningless without the user and carry no independent audit value.
- `ScheduleSlots → DoctorSchedule`: `ON DELETE CASCADE` at the schedule level, but a slot that already has a linked Appointment is protected by application logic (cannot delete a schedule with future confirmed bookings — business rule, not a DB constraint, since the correct response is a validation error, not silent DB rejection).
- All monetary columns use `decimal(18,2)`, never `float`/`double` — required for correctness in a payments domain.
- All timestamps stored as `timestamptz` (UTC) — critical since hospitals may span time zones; conversion to local time happens only at presentation.

### Normalization
Third normal form throughout, with two deliberate, documented exceptions:
- `Appointments.FeeAtBooking` (a `Money` snapshot) duplicates the doctor's consultation fee at time of booking. This is *intentional* denormalization: consultation fees change over time, and an appointment's price must never retroactively change — this is an audit/legal requirement, not a normalization failure.
- `CancellationPolicies` are versioned rather than updated in place, so historical appointments always resolve refunds against the policy that was active when they were booked.

---

## 6. CQRS, Repository & Unit of Work Strategy

### CQRS — where it helps, where it doesn't
- **Commands** (`BookAppointmentCommand`, `ConfirmPaymentCommand`, `CancelAppointmentCommand`) go through full validation, domain invariant checks, and raise domain events. Handled by MediatR `IRequestHandler`.
- **Queries** (`GetDoctorScheduleQuery`, `GetHospitalDashboardQuery`) bypass the repository/aggregate layer entirely and use **lightweight read models** (Dapper or EF Core `AsNoTracking` projections directly to DTOs) for performance — no reason to hydrate a full `Appointment` aggregate just to list appointments in a grid.
- This is "CQRS-lite": same database, same schema, different code paths for read vs. write. Full event sourcing / separate read database is explicitly **not** recommended for v1 — it would slow delivery without a corresponding scale requirement yet. The design doesn't block adding it later per-module (e.g., Analytics could get a dedicated read replica first).

### Repository Pattern — used, but narrowly
- One repository per **Aggregate Root only**, returning/persisting whole aggregates for command handlers.
- Repositories do **not** support ad-hoc filtering (`GetByFilter(Expression<...>)` anti-pattern) — that leaks query concerns into Domain. Complex reads are queries, not repository methods.
- Justification: repositories exist here specifically to (a) keep the Domain layer's repository *interfaces* free of EF Core, and (b) provide a seam for the Unit of Work / transactional consistency boundary — not as a blanket "always abstract the database" rule.

### Unit of Work
- A single `IUnitOfWork` wraps the `DbContext`'s `SaveChangesAsync`, ensuring an aggregate + its raised domain events + any secondary writes commit atomically.
- A `TransactionBehavior` (MediatR pipeline behavior) wraps every Command handler in a DB transaction automatically, and dispatches domain events **after** successful commit (using the "collect-then-dispatch" pattern) — so a `NotificationSent` side effect never fires for a transaction that ultimately rolled back.

---

## 7. API Design

- **Versioning:** URL-segment versioning (`/api/v1/appointments`) via `Asp.Versioning.Http` — explicit and cache-friendly, easier for hospital IT teams and third-party integrators to reason about than header-based versioning.
- **Response envelope:** a consistent `ApiResponse<T>` wrapper: `{ success, data, message, errors, traceId }` — makes client-side error handling uniform across hundreds of endpoints and gives you a `traceId` correlated to Serilog for support debugging.
- **Error handling:** RFC 7807 `ProblemDetails` under the hood, mapped into the `ApiResponse` envelope by global exception-handling middleware; domain exceptions (e.g., `InvalidAppointmentTransitionException`) map to 409/422, validation failures (FluentValidation) map to 400 with a field-error dictionary, not-found to 404, auth failures to 401/403.
- **Validation:** FluentValidation validators run automatically via a `ValidationBehavior` pipeline step before a command handler ever executes — controllers never validate manually.
- **Pagination:** cursor-based for high-volume, frequently-changing lists (e.g., appointment queues) to avoid page-drift; offset-based (`page`/`pageSize`) for smaller, admin-style lists (e.g., Department list) where simplicity wins. Every paged response includes `totalCount`, `hasNextPage`.
- **Filtering/Sorting/Searching:** a standard query-string contract (`?filter=status:Confirmed&sort=-startTime&search=John`) parsed into a shared `QueryParameters` object at the Application layer, translated into `IQueryable` projections — one implementation reused across all list endpoints rather than bespoke filter logic per controller.
- **RESTful organization:** resource-oriented (`/hospitals/{id}/doctors`, `/doctors/{id}/schedules`, `/appointments/{id}/status`) with actions that aren't naturally RESTful (e.g., "check in a patient") modeled as a `POST /appointments/{id}/check-in` sub-resource action rather than a verb-polluted URL.

---

## 8. Security Architecture

Mapped directly to the requirement list:

- **Authentication:** JWT access tokens (short-lived, 15 min) + rotating refresh tokens (stored hashed, one-time-use — reuse of an old refresh token revokes the entire token family, a strong signal of theft).
- **Authorization:** Role-Based (Super Admin / Receptionist / Doctor / Patient) layered with **Permission-Based** claims (e.g., `appointments.confirm`, `reports.view`) so a hospital can create custom receptionist sub-roles later without code changes — roles are just named bundles of permissions, stored in the DB, not hardcoded in `[Authorize(Roles=...)]` attributes (a custom `[Authorize(Permission = "...")]` policy provider is used instead).
- **Tenant-aware authorization:** every authorization check also validates the resource's `TenantId` matches the caller's tenant claim — prevents "confused deputy" cross-tenant access even if an ID is guessed.
- **Password hashing:** ASP.NET Core Identity's PBKDF2 (or Argon2id via a custom hasher) — never custom-rolled hashing.
- **Email verification & OTP:** required before first login; OTP (TOTP or SMS-based) available as a step-up for sensitive actions (Super Admin actions, refund approval).
- **Rate limiting & brute-force protection:** ASP.NET Core built-in Rate Limiting middleware (fixed-window per IP+endpoint for public endpoints, stricter on `/auth/login`), combined with **account lockout** after N failed attempts (exponential backoff).
- **Injection/XSS/CSRF:** EF Core parameterized queries eliminate SQL injection by default (raw SQL is banned outside reviewed, parameterized exceptions); output encoding handled by the API returning JSON only (XSS is primarily a frontend concern, but all user-supplied text fields — review comments, notes — are HTML-encoded before storage/display); CSRF is largely moot for a token-based (not cookie-session) API, but if refresh tokens are ever stored in cookies, `SameSite=Strict` + `HttpOnly` + double-submit token is applied.
- **Input validation:** FluentValidation at the Application boundary; Domain-layer invariants as a second, non-bypassable layer (defense in depth — even a buggy handler can't create an invalid `Appointment`).
- **Sensitive data encryption:** column-level encryption (via EF Core value converters) for fields like national ID numbers; TLS in transit everywhere; PostgreSQL `pgcrypto` or transparent disk encryption at rest.
- **Secure file upload:** (for future prescriptions/lab reports) — strict content-type allow-listing, virus scanning hook, storage outside the web root (blob storage), signed short-lived URLs for retrieval.
- **Audit logs:** every state-changing command writes an immutable `AuditLog` entry (who, what, when, before/after snapshot for sensitive fields) via a MediatR pipeline behavior — not something developers have to remember to call manually.
- **Secrets management:** no secrets in `appsettings.json`; Azure Key Vault / AWS Secrets Manager / HashiCorp Vault in production, `dotnet user-secrets` locally.
- **Security headers & HTTPS:** HSTS, `X-Content-Type-Options`, `X-Frame-Options`, CSP via middleware; HTTPS redirection enforced, HTTP disabled entirely in production.
- **Token revocation & session management:** refresh token table supports immediate revocation (logout-everywhere, admin-forced logout on suspicious activity); access tokens are short-lived enough that revocation lag is acceptable.
- **GDPR-like privacy:** data export/delete request workflow, PII minimization (only collect what's needed), consent tracking for notification channels, data retention policy documented per data category (financial records retained longer than, say, notification logs).
- **Least privilege:** database roles used by the app have no `DROP`/`ALTER` rights in production; a separate migration-runner identity has elevated rights and is never used by the running application.

---

## 9. Appointment & Attendance-Score Engine

The state machine (Pending → Confirmed → Checked In → In Consultation → Completed / Cancelled / Rescheduled / No Show / Rejected / Expired) is implemented as a **smart enum with an explicit transition table** inside the `Appointment` aggregate — any attempt to transition outside the allowed graph throws a domain exception before it ever reaches the database. An `Expired` background job (Hangfire recurring job) sweeps `Pending` appointments whose payment window has lapsed.

**Attendance Score** is deliberately **decoupled from public ratings**, exactly as required:
- Public `Reviews`/`Ratings` are unaffected by cancellations — patients rate *service quality*, not booking behavior.
- `AttendanceScoreService` listens to `AppointmentCompletedEvent`, `AppointmentNoShowEvent`, `AppointmentCancelledEvent` (via MediatR `INotificationHandler`, decoupled from the Appointments module) and adjusts an internal 0–100 score.
- The score feeds three consumers, all reading the score rather than recalculating it: (1) **Deposit requirement** (low-score patients may be required to pay a higher upfront deposit — read by the Payments module at booking time), (2) **Penalty amount** (feeds `RefundCalculationService`), (3) **Booking priority/fraud flagging** (a very low score can trigger a Super Admin fraud-review queue instead of auto-approval).
- This is a clean example of **event-driven decoupling between bounded contexts** rather than the Appointments module directly calling into Payments/Reputation code.

---

## 10. Refund & Cancellation Policy Engine

Each `Hospital` owns a versioned `CancellationPolicy` value object: `{ WindowHours, RefundPercentage, PenaltyAmount, EffectiveFrom }`. At cancellation time, `RefundCalculationService`:
1. Loads the policy version that was **active at booking time** (not today's policy — fairness and legal defensibility).
2. Computes elapsed time between now and the appointment slot.
3. Applies tiered logic (e.g., >24h = 100% refund, 6–24h = 50%, <6h = 0% + penalty) — the tiers themselves are configurable data, not hardcoded thresholds, satisfying "each hospital defines its own."
4. Emits a `RefundCalculatedEvent`, consumed by the Payments module to actually execute the refund through the gateway.

This keeps policy *configuration* (Hospital module), policy *evaluation* (a Domain Service straddling Hospital+Appointment), and refund *execution* (Payments/Infrastructure) cleanly separated.

---

## 11. Notification Architecture

A single `INotificationService.SendAsync(NotificationRequest)` abstraction in Application, with a **channel-strategy pattern** in Infrastructure:

```
INotificationChannel (interface)
 ├── EmailChannel        (SendGrid/SES adapter)
 ├── SmsChannel          (Twilio/local gateway adapter)
 ├── PushChannel         (future — FCM/APNs)
 └── WhatsAppChannel     (future — WhatsApp Business API)
```

- A `NotificationTemplate` (per hospital, falling back to a platform default) is resolved by `TemplateKey` + `Locale` (multi-language ready) and rendered before dispatch.
- Notifications are sent via a **background job queue** (Hangfire), not inline in the request thread — an appointment booking should never wait on an email provider's latency, and failures are retried with backoff.
- Every send/failure is written to `NotificationLogs` for audit and for the Super Admin's "global notification management" view.
- Domain events (`AppointmentConfirmedEvent`, etc.) are the trigger — the Appointments module never calls `INotificationService` directly; a dedicated `NotificationEventHandlers` project subscribes to domain events across contexts, keeping notification concerns entirely out of core business modules.

---

## 12. Logging, Monitoring & Auditing

- **Serilog** as the logging framework, structured (JSON) sinks to both console (for container log aggregation) and a centralized store (Seq / Elastic / Application Insights, cloud-agnostic via sink swap).
- **Request logging:** middleware logs method, path, status, duration, `TenantId`, `UserId`, `TraceId` for every request.
- **Error logging:** unhandled exceptions logged with full context by the global exception middleware before being translated to the client-safe `ProblemDetails` response — internal details never leak to the client.
- **Audit logging:** separate from operational logs — `AuditLogs` table (not just log files) since audit trails must be queryable, tenant-scoped, and tamper-evident (append-only, no update/delete permission granted to the app's DB role on this table).
- **Performance logging:** a `PerformanceBehavior` MediatR pipeline step logs any command/query exceeding a threshold (e.g., 500ms) with the handler name — cheap, automatic slow-query visibility without APM tooling required on day one, though APM (Application Insights/Datadog) is recommended once traffic justifies the cost.
- **Correlation:** every log line, API response, and background job carries the same `TraceId`, so a support engineer can follow one request end-to-end across sync and async work.

---

## 13. Performance & Scalability Strategy

- **Caching:** Redis (distributed, tenant-aware keys prefixed with `TenantId`) for read-heavy, slow-changing data — doctor schedules, hospital directory search, subscription status. Cache invalidation triggered by the relevant domain events (e.g., `DoctorScheduleUpdatedEvent` evicts that doctor's cached slots) rather than time-based expiry alone.
- **Lazy vs. Eager Loading:** EF Core lazy loading is **disabled globally** (it causes unpredictable N+1 queries in a multi-tenant system where "unpredictable" becomes "expensive at scale"). All loading is explicit via `.Include()` in repository/query code, or better, via projection directly to DTOs (avoiding loading full entity graphs at all for read paths).
- **Background jobs:** Hangfire (or Quartz.NET) for: appointment-expiry sweeps, notification dispatch, scheduled slot generation from recurring `DoctorSchedules`, subscription renewal reminders, nightly analytics aggregation.
- **Async everywhere:** all I/O-bound code (`DbContext`, HTTP calls to payment/notification providers) is `async`/`await` end-to-end; no blocking `.Result`/`.Wait()`.
- **Connection pooling:** PgBouncer in front of PostgreSQL in production for connection multiplexing across what will eventually be many app instances; EF Core's own pooled `DbContext` (`AddDbContextPool`) reduces per-request allocation overhead.
- **Database optimization:** read replicas for reporting/analytics queries once volume justifies it (keeps OLTP path fast); table partitioning on `Appointments` by date range once historical volume grows large (PostgreSQL native partitioning), transparent to the application layer.
- **Horizontal scalability:** the API is stateless (JWT, no server-side session) so it scales horizontally behind a load balancer trivially; Redis handles any needed shared state (rate-limit counters, distributed locks for slot-booking race conditions using `Redlock` to prevent two patients grabbing the same slot simultaneously under high concurrency).

---

## 14. Deployment Architecture

- **Docker:** each layer's composition root (`SmartCare.WebApi`) is containerized via a multi-stage Dockerfile (build stage with SDK, runtime stage with ASP.NET runtime image only — smaller attack surface and image size).
- **docker-compose** for local dev spins up: API, PostgreSQL, Redis, Seq (log viewer), and a mail-catcher (for email testing) — one command onboarding for new engineers.
- **Cloud-ready:** container is orchestration-agnostic (works on Azure Container Apps, AWS ECS/EKS, or plain Kubernetes) since no cloud-specific SDK is called directly from Application/Domain — cloud provider specifics (Key Vault, Blob Storage) are isolated behind Infrastructure interfaces, so moving cloud providers is an Infrastructure-layer change only.
- **CI/CD:** build → run unit + integration tests (integration tests spin up a real PostgreSQL via Testcontainers, not mocks, for realistic EF Core behavior) → build image → push to registry → deploy, with EF Core migrations run as a separate, explicit pipeline step (never `EnsureCreated()`/auto-migrate on app startup in production — migrations are a controlled, reviewable action).
- **Health checks:** `/health/live` and `/health/ready` endpoints (DB connectivity, Redis connectivity) for orchestrator readiness/liveness probes.

---

## 15. Future-Proofing Roadmap

How the above design absorbs each future item **without major redesign**:

| Future Feature | Why the current design already supports it |
|---|---|
| React frontend | API is already a pure JSON REST API with no server-rendered views — a React app is just a new consumer. |
| Mobile app | Same API; JWT auth already mobile-friendly (no cookie dependency). |
| Digital Prescriptions / EMR | New bounded context (`SmartCare.Domain.Clinical`) added as a new module/vertical slice; hooks into existing `Appointment.Completed` event; doesn't touch existing modules. |
| Laboratory / Pharmacy modules | Same pattern — new bounded contexts, new aggregates, referencing `Patient`/`Doctor` by ID only (no tight coupling). |
| Telemedicine / Video Consultation | `Appointment` aggregate already models a `ConsultationType` extension point (in-person/video) as a value object; video-provider integration is a new Infrastructure adapter behind an `IVideoSessionProvider` interface. |
| AI Assistant / Recommendations | Consumes existing read models/query side (CQRS read path) — analytics and recommendation services are natural read-only consumers of the same data without touching write-side invariants. |
| Insurance | New `InsurancePolicy` aggregate referenced from `Payment`, extending the existing Payments module rather than replacing it. |
| Ambulance / Wearables | New bounded contexts; wearables likely integrate via a new `IntegrationEvents`/webhook ingestion endpoint, following the same event-driven pattern already used internally. |
| Multi-language | `NotificationTemplate` and any patient-facing content already keyed by `Locale`; extending to full i18n is a content/config task, not an architecture change. |
| Multi-branch hospitals | `Hospital` aggregate can add a `Branch` child entity (a hospital "owns" branches) without changing the tenancy model — `TenantId` stays at the Hospital level, `BranchId` becomes an additional scoping dimension. |
| Machine Learning / Analytics | The read-side (CQRS queries + potential read replica) is already isolated from the transactional write path, so an ML pipeline can consume it without risking OLTP performance. |

---

## Summary of Key Recommendations

1. **Clean Architecture + Modular Monolith** now, with clear seams to peel off microservices later (Payments and Notifications are the most likely first candidates).
2. **Shared Database / Shared Schema** multi-tenancy with `TenantId` filtering enforced at the EF Core global-filter level *and* PostgreSQL RLS as defense-in-depth, keeping a documented escape hatch to isolate a large tenant later.
3. **CQRS-lite** via MediatR — commands go through the full domain/aggregate/validation pipeline; queries use lightweight projections. No event sourcing for v1.
4. **Repository pattern scoped strictly to aggregate roots**, paired with a Unit of Work that guarantees transactional consistency and safe, post-commit domain event dispatch.
5. **Attendance Score fully decoupled from public Ratings**, connected only via domain events — protecting the integrity of the public-facing review system while still enabling fraud/penalty controls internally.
6. **Security is layered, not single-point**: JWT+refresh, RBAC+permission claims, tenant-aware authorization checks, domain-level invariants, and immutable audit logs each catch what the layer before might miss.
7. Every future capability in the requirements list maps to an **additive bounded context or Infrastructure adapter**, not a change to existing core modules — which is the actual test of whether this architecture will hold up as SmartCare grows from one hospital to thousands.

---

*This document defines the architecture only, per the brief. The next step — once this design is reviewed and approved — would be to translate the Domain layer (entities, value objects, aggregate invariants) into actual C# code, followed by EF Core configurations and the Application layer's first vertical slice (recommended starting point: Hospital Registration → Doctor Management → Appointment Booking, since that is the critical path through the core workflow).*
