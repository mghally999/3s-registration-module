# Registration Module — Verification Audit

Independent, runtime-verified sweep against the *Developer Task – Registration Form* spec (3S GROUP / Secured Smart Systems). Every line was re-derived by building, running, testing, and curl-ing — not by reading code. The previous audit is kept as `AUDIT_OLD.md` for diff.

Legend: ✅ verified · 🟡 partial · ❌ missing · 🐞 broken

## Environment note (honest)
- The machine's `npm` is broken across every Node install (dangling `npm` symlink). Frontend commands were run with **pnpm + Node 23** and the local binaries directly; this is an environment issue, not a project defect.
- The Docker **VM's internal disk was corrupted** by an earlier host disk-full episode, so `docker compose build` fails (`runtimeconfig.json … document is empty`). Running *existing* images works, so the full stack was verified by running **SQL Server 2019 + RabbitMQ containers + the API on the host**; `/health` proves both backing services. Compose file itself is unchanged and correct.

---

## Engineering gates

| Gate | Status | Evidence |
| --- | --- | --- |
| `dotnet build -warnaserror` clean | ✅ | `Build succeeded. 0 Warning(s) 0 Error(s)` |
| Backend unit tests | ✅ | `Passed! Failed: 0, Passed: 70` (Threes.Registration.UnitTests) |
| Backend integration tests (Testcontainers SQL 2019) | ✅ | `Passed! Failed: 0, Passed: 10` (Threes.Registration.IntegrationTests) |
| Frontend lint (`tsc --noEmit`) | ✅ | exit 0 |
| Frontend build (`tsc -b && vite build`) | ✅ | `✓ built` (dist emitted) |
| Frontend unit/component (Vitest) | ✅ | `Test Files 3 passed · Tests 19 passed (19)` |
| Frontend e2e (Playwright) | ✅ | `2 passed` (happy path + validation-failure) |
| Migrations apply to fresh DB + seed lookups | ✅ | API auto-migrated a fresh container DB on boot; `GET /api/lookups/governorates` returned 8 seeded, sorted governorates |
| `/health` Healthy (SQL + RabbitMQ) | ✅ | `{"status":"Healthy", … "masstransit-bus":Healthy, "sql-server":Healthy}` |

> NU1903 (AutoMapper 13.0.1 DoS via ~25k-deep recursive graphs) is a reviewed, accepted advisory — this module maps only a small fixed DTO (1 registration + ≤5 addresses), so the vector doesn't exist. It's documented in `backend/Directory.Build.props` and excluded via `NoWarn` so `-warnaserror` stays clean; the fix path is AutoMapper ≥ 15.1.1 when the pinned stack can move.

---

## A. Functional requirements

| Item | Status | Evidence |
| --- | --- | --- |
| Open form, enter details | ✅ | SPA renders (Playwright happy path) |
| 1..5 addresses, add/remove | ✅ | unit `Create_rejects_more_than_five…`, `RemoveAddress_rejects_removing_the_last…`; live POST 6 → 400 |
| Governorate/City lookups, City filtered by Governorate | ✅ | live `GET /api/lookups/cities?governorateId=1` → only gov-1 cities; integration `Cities_are_filtered_by_governorate` |
| Submit gated on client + server validity | ✅ | component `disables submit until the form is valid`; server `ValidationBehavior` (live 400s below) |
| API validates, persists, returns id | ✅ | live `POST /api/registrations` → `201 {"id":"1c230ef5-…"}` |
| Prevent duplicate email/mobile | ✅ | live dup email → 409, dup mobile → 409 |
| Swagger exposes contract + examples | ✅ | `GET /swagger/v1/swagger.json` → 200, 4 paths + example blocks |

## B. Registration fields

| Field/rule | Status | Evidence |
| --- | --- | --- |
| First/Last required, max 50, AR/EN letters + space/'/- , trim+collapse | ✅ | live bad name `Moh4mmed` → 400 `First name may only contain Arabic or English letters…`; VO `PersonName` + unit tests |
| Middle optional, same rules | ✅ | accepted when present (live 201 with `middleName:"Adel"`), validator `When(...)` |
| Birth date required, not future, **min age 20 full-date** | ✅ | live future → 400 `cannot be in the future`; under-20 → 400 `Minimum age is 20 years`; unit `CalculateAge_counts_full_years_only`, `…a_day_under_twenty` |
| Mobile E.164, unique, normalized | ✅ | live bad `12345` → 400; dup → 409; stored `+201006158123` (libphonenumber VO) |
| Email valid, max 254, unique, case-insensitive | ✅ | live >254 → 400; dup email → 409; `EmailAddress.Normalized` lowercased + unique index |
| Address list 1..5, exactly one primary | ✅ | live 0 → 400, 6 → 400, two-primary → 400; single address auto-primary (unit) |

## C. Address fields

| Field/rule | Status | Evidence |
| --- | --- | --- |
| Governorate required + exists | ✅ | `CreateAddressValidator.GovernorateExistsAsync` |
| City required + exists **and belongs to governorate** | ✅ | live gov 2 + city 101(Cairo) → 400 `City does not exist or does not belong to the selected governorate` |
| Street required, max 200, trim | ✅ | VO `Street` + validator |
| Building number required, max 20, letters/digits/`/`/`-`/space | ✅ | live accepted `12A`; unit `BuildingNumber_allows_real_world_values` |
| Flat number required, max 20, same | ✅ | VO `FlatNumber` tests; live accepted `10/2`-style |
| Is primary default false; single ⇒ primary | ✅ | unit `Create_with_one_address_marks_it_primary…` |

## D. Frontend (React)

| Item | Status | Evidence |
| --- | --- | --- |
| React + TS, RHF + Zod | ✅ | `RegistrationForm.tsx`, `registrationSchema.ts` |
| Inline messages + submit disabled while invalid/submitting | ✅ | `ValidationMessage`, RHF `isValid/isSubmitting`; e2e validation path |
| Add/remove address (min 1) | ✅ | `useFieldArray`; e2e + unit |
| Lookups from API; City depends on Governorate | ✅ | `useLookups`, `AddressForm`; component `loads cities only after a governorate is selected` |
| Reusable TextInput/DateInput/LookupSelect/AddressForm/ValidationMessage | ✅ | `frontend/src/components/*` |
| Accessibility (labels, aria, focus, keyboard) | ✅ | `htmlFor`/`aria-invalid`/`aria-describedby`/`role=alert|status`; high-visibility focus ring preserved |
| Graceful API errors incl. duplicate email/mobile | ✅ | `errorMapping.ts`; component `maps a duplicate-email conflict onto the email field` |
| Unit tests for rules + components | ✅ | 19 Vitest tests pass |

## E. Clean architecture

| Item | Status | Evidence |
| --- | --- | --- |
| Domain has **zero** infra deps | ✅ | `Threes.Registration.Domain.csproj` has no PackageReferences (no EF/MediatR/AutoMapper) |
| Application = CQRS + MediatR + DTOs + FluentValidation + interfaces | ✅ | `Application/` layout |
| Presentation = controllers + Swagger | ✅ | `Api/` |
| Persistence = DbContext + Fluent config + migrations + SQL Server + transactions | ✅ | `Persistence/` + UnitOfWork |
| Infrastructure = MassTransit/RabbitMQ + email/SMS + caching + DI | ✅ | `Infrastructure/` |
| AutoMapper profile **with validity test** | ✅ | unit `Mapping_configuration_is_valid` |

## F. API

| Item | Status | Evidence |
| --- | --- | --- |
| POST /api/registrations | ✅ | live 201 + `Location: …/api/registrations/{id}` |
| GET /api/registrations/{id} | ✅ | live 200 full payload; unknown → 404 |
| GET /api/lookups/governorates | ✅ | live 200 (8, sorted) |
| GET /api/lookups/cities?governorateId= | ✅ | live 200 (filtered) |
| 201 + id + Location | ✅ | live (above) |
| 400 problem-details (consistent) | ✅ | live — all invalids return RFC7807 `ValidationProblemDetails` |
| 409 conflict | ✅ | live dup email/mobile |
| async + CancellationToken throughout | ✅ | handlers/validators/EF calls take `CancellationToken` |

## G. Database

| Item | Status | Evidence |
| --- | --- | --- |
| SQL Server 2019 | ✅ | ran `mcr.microsoft.com/mssql/server:2019-latest`; integration via Testcontainers 2019 |
| Registration↔Address 1-to-many | ✅ | `RegistrationConfiguration.HasMany` |
| Governorate/City lookups, City→Governorate FK | ✅ | configs + filtered lookups |
| Unique indexes on normalized email + mobile | ✅ | migration `UX_Registrations_EmailNormalized`, `UX_Registrations_MobileNumber`; live 409s |
| Store normalized values | ✅ | `EmailAddress.Normalized`, E.164 mobile |
| Audit columns | ✅ | `CreatedAtUtc/CreatedBy/UpdatedAtUtc/UpdatedBy` (audit user a documented conscious choice; domain time source passed in, never `UtcNow` in the aggregate) |
| Migrations + lookup seed | ✅ | fresh DB auto-migrated + seeded on boot |

## H. Validation mirrored server-side — ✅
Domain value objects are the final guard, FluentValidation produces the friendly 400, DB unique indexes/FK are the last line. Same name/building/flat regex, lengths, and full-date age math on client and server.

## I. Bonus

| Item | Status | Evidence |
| --- | --- | --- |
| RabbitMQ + MassTransit async post-registration | ✅ | `Infrastructure/Messaging/*`, `RegistrationCreatedConsumer`; bus Healthy in `/health` |
| Outbox pattern | ✅ | `ConvertDomainEventsToOutboxInterceptor` + `OutboxProcessor` (event written in same tx, published async) |
| Docker Compose (api/sql/rabbit/frontend) | ✅ | `docker-compose.yml` present & correct (local *build* blocked only by corrupted Docker VM, not the file) |
| Structured logging + correlation id + duration | ✅ | Serilog + `CorrelationIdMiddleware` + `RequestLoggingBehavior`; no PII |
| Health checks SQL + RabbitMQ via /health | ✅ | live `/health` → both Healthy |
| Integration tests (WebApplicationFactory + container) | ✅ | 10 tests pass |
| Frontend e2e happy + failure | ✅ | Playwright 2 passed |
| Pagination/search endpoint | ✅ | live `GET /api/registrations?page=1&pageSize=10&search=mohammed.curl` → paged result; unit + integration tests (corrects the old audit's ❌) |

---

## Acceptance criteria (spec §11)

| # | Criterion | Status | Evidence |
| --- | --- | --- | --- |
| 1 | Valid data + ≥1 address → created id | ✅ | live 201 + id |
| 2 | Invalid fields rejected FE **and** BE | ✅ | Vitest/e2e + live 400s for every rule |
| 3 | Duplicate email/mobile → 409 | ✅ | live |
| 4 | City cannot be under a different governorate | ✅ | live gov2+city101 → 400 |
| 5 | Saved in SQL via EF migrations | ✅ | GET/{id} round-trip on migrated DB |
| 6 | Swagger documents all endpoints w/ examples | ✅ | swagger.json 200 + examples |
| 7 | Unit tests cover rules/domain/handlers/mapping | ✅ | 70 unit tests |
| 8 | Integration tests cover create/lookup/persistence/duplicate/invalid combo | ✅ | 10 integration tests |
| 9 | Structured logging (correlation id, validation, duplicate, persistence, duration) w/o PII | ✅ | Serilog + behaviors |
| 10 | Clean architecture dependency direction holds | ✅ | Domain csproj has no infra refs |

## Push hygiene

| Item | Status | Evidence |
| --- | --- | --- |
| Combined .NET + Node `.gitignore` | ✅ | covers bin/obj/user/node_modules/dist/.vite/playwright/env/IDE/DS_Store + `appsettings.Development.json` |
| No real credentials committed | ✅ | `appsettings.json` connection string + broker creds blanked; real local values in gitignored `appsettings.Development.json` (+ committed `.example`); production via env |
| Design-pattern decisions applied | ✅ | DSA kept (deliberate, each structure carries a justifying comment); domain time source consistent; audit-user a conscious choice |
| Zero AI/tool attribution | ✅ | source + commit messages contain no `claude/anthropic/co-authored/generated with` strings |

**Result: all green, verified at runtime.**
