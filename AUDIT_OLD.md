# Registration Module — Audit against specification

Legend: ✅ present & verified · 🟡 partial · ❌ missing · 🐞 broken

Stack in use (verified, not guessed):
- backend: **.NET 8** (`net8.0`), EF Core **8.0.6**, MediatR **12.2.0**, FluentValidation **11.9.2**, AutoMapper **13.0.1**, MassTransit **8.2.2**, Serilog **8.0.1**, Swashbuckle **6.6.2**.
- frontend: **React 18.3** + **TypeScript 5.5**, **React Hook Form 7.52** + **Zod 3.23**, Vite 5, Vitest 2, Playwright.
- db: **SQL Server 2019** (provider `Microsoft.EntityFrameworkCore.SqlServer`).

Evidence commands are run from `backend/` (with `export PATH="$HOME/.dotnet:$PATH"`) and `frontend/`.

---

## A. Functional requirements

| Item | Status | Location | Evidence |
| --- | --- | --- | --- |
| Open form, enter personal details | ✅ | `frontend/src/components/RegistrationForm.tsx` | frontend build + component test pass |
| 1..5 addresses, add/remove | ✅ | `RegistrationForm.tsx` (useFieldArray), `AddressForm.tsx`; domain `AddressBook` (min 1 / max 5) | unit test `Create_rejects_more_than_five_addresses`, `RemoveAddress_rejects_removing_the_last_address` |
| Governorate/City lookups, City filtered by Governorate | ✅ | `LookupsController`, `useLookups.ts`, `AddressForm.tsx` | integration `Cities_are_filtered_by_governorate`; component `loads cities only after a governorate is selected` |
| Submit only when client + server valid | ✅ | RHF `isValid` gates submit; backend `ValidationBehavior` | component `disables submit until the form is valid` |
| API validates, persists, returns id | ✅ | `CreateRegistrationCommandHandler` | integration `Create_then_get_round_trips_the_registration` |
| Prevent duplicate email/mobile | ✅ | handler pre-check + `UnitOfWork` unique-violation → `ConflictException` | integration `Duplicate_email…409`, `Duplicate_mobile…409` |
| Swagger exposes contract + examples | ✅ | `Program.cs` (Swagger), `Swagger/Examples/CreateRegistrationRequestExample.cs` | served at `/swagger`; example provider registered |

## B. Registration fields & validation

| Field/rule | Status | Location | Evidence |
| --- | --- | --- | --- |
| First name required, max 50, AR/EN letters + space/hyphen/apostrophe, trim+collapse | ✅ | domain `ValueObjects/PersonName.cs`; app `Validation/InputRules.cs`, `CreateRegistrationCommandValidator` | unit `PersonName_*`, `Name_with_digits_is_rejected` |
| Middle name optional, same rules | ✅ | `PersonName` + validator `When(MiddleName not empty)` | mapping test maps middle name |
| Last name required, same rules | ✅ | same as first name | unit tests |
| Birth date required, not future, min age 20 **accurate** | ✅ | domain `ValueObjects/BirthDate.cs` (`CalculateAge`) | unit `CalculateAge_counts_full_years_only`, `…a_day_under_twenty`, `…future` |
| Mobile required, E.164, unique, stored normalized | ✅ | `MobileNumber` VO + `Infrastructure/Phone/LibPhoneNumberNormalizer.cs`; owned column + unique index | unit `MobileNumber_*`; validator `Local_mobile_format_is_accepted` |
| Email required, valid, max 254, unique, case-insensitive | ✅ | `EmailAddress` VO (Value + Normalized); owned cols + unique index on normalized | unit `EmailAddress_keeps_original_but_normalizes_lowercase` |
| Address list required, 1..5, one primary | ✅ | `Registration.SetInitialAddresses`, validator | unit `Create_rejects_two_primary_addresses`, `…zero_addresses`, `…more_than_five` |

## C. Address fields & validation

| Field/rule | Status | Location | Evidence |
| --- | --- | --- | --- |
| Governorate required + must exist | ✅ | `CreateAddressValidator` (`GovernorateExistsAsync`) | unit `LookupCacheTests`, integration |
| City required + exists + belongs to governorate | ✅ | `CreateAddressValidator` (`CityBelongsToGovernorateAsync`) | unit `City_under_the_wrong_governorate_is_rejected`; integration `City_under_the_wrong_governorate…400` |
| Street required, max 200, trim | ✅ | `ValueObjects/Street.cs` + validator | unit/value-object |
| Building number required, max 20, letters/digits/`/`/`-`/space | ✅ | `ValueObjects/BuildingNumber.cs` | unit `BuildingNumber_allows_real_world_values`, `…rejects_disallowed_characters` |
| Flat number required, max 20, same allowance | ✅ | `ValueObjects/FlatNumber.cs` | value-object tests |
| Is primary optional, default false, single address ⇒ primary | ✅ | `Registration.SetInitialAddresses` | unit `Create_with_one_address_marks_it_primary…` |

## D. Frontend requirements

| Item | Status | Location |
| --- | --- | --- |
| React + TypeScript | ✅ | `frontend/` (Vite + TS) |
| RHF + Zod schema | ✅ | `RegistrationForm.tsx`, `validation/registrationSchema.ts` |
| Inline messages + submit disabled while invalid/submitting | ✅ | `ValidationMessage.tsx`, RHF `isValid`/`isSubmitting` |
| Add/remove address, min 1 | ✅ | `RegistrationForm.tsx` useFieldArray |
| Load lookups; City depends on Governorate | ✅ | `useLookups.ts`, `AddressForm.tsx` |
| Reusable TextInput/DateInput/LookupSelect/AddressForm/ValidationMessage | ✅ | `frontend/src/components/*` |
| Accessibility (labels, aria, focus) | ✅ | components use `htmlFor`/`aria-invalid`/`aria-describedby`; `styles.css` focus states |
| Graceful API errors incl. duplicate email/mobile | ✅ | `api/errorMapping.ts`, `RegistrationForm.tsx` |
| Unit tests for rules + component behavior | ✅ | `frontend/tests/*` (19 tests) |

## E. Backend architecture (clean architecture)

| Item | Status | Evidence |
| --- | --- | --- |
| Domain has no EF/MediatR/AutoMapper/infra deps | ✅ | `Threes.Registration.Domain.csproj` has **zero** PackageReferences; verified by grep (see §Evidence runs) |
| Application: CQRS + MediatR + DTOs + FluentValidation + interfaces | ✅ | `Application/` layout |
| Presentation: controllers + Swagger + mapping | ✅ | `Api/` |
| Persistence: DbContext + Fluent config + migrations + SQL Server + transactions | ✅ | `Persistence/` |
| Infrastructure: MassTransit/RabbitMQ + email/SMS + caching + DI | ✅ | `Infrastructure/` |
| Mapping: AutoMapper profile + validity test | ✅ | `Common/Mapping/RegistrationMappingProfile.cs`; test `Mapping_configuration_is_valid` |

## F. API requirements

| Item | Status | Location |
| --- | --- | --- |
| POST /api/registrations | ✅ | `RegistrationsController.Create` |
| GET /api/registrations/{id} | ✅ | `RegistrationsController.GetById` |
| GET /api/lookups/governorates | ✅ | `LookupsController.GetGovernorates` |
| GET /api/lookups/cities?governorateId= | ✅ | `LookupsController.GetCities` |
| 201 + id + Location | ✅ | `CreatedAtAction(...)`; integration asserts `Location` header |
| 400 problem-details | ✅ | `GlobalExceptionHandler` → `ValidationProblemDetails` |
| 409 duplicate | ✅ | `ConflictException` → 409 |
| async + CancellationToken throughout | ✅ | handlers/validators/EF calls all take `CancellationToken` |

## G. Database requirements

| Item | Status | Location |
| --- | --- | --- |
| SQL Server 2019 | ✅ | `AddPersistence` UseSqlServer; compose `mssql/server:2019` |
| Registration ↔ Address one-to-many | ✅ | `RegistrationConfiguration` HasMany |
| Governorate/City lookups, City→Governorate FK | ✅ | `GovernorateConfiguration`, `CityConfiguration` |
| Unique indexes on normalized email + mobile | ✅ | migration `UX_Registrations_EmailNormalized`, `UX_Registrations_MobileNumber` |
| Store normalized values | ✅ | `EmailAddress.Normalized` column; mobile stored E.164 |
| Audit columns | ✅ | `CreatedAtUtc/CreatedBy/UpdatedAtUtc/UpdatedBy` |
| Migrations + lookup seed | ✅ | `Migrations/*_InitialCreate` + `LookupSeedData` |

## H. Validation requirements (server mirrors client) — ✅ (see B/C; backend value objects are the final guard, FluentValidation the friendly 400, DB the last line).

## I. Bonus items

| Item | Status | Location |
| --- | --- | --- |
| RabbitMQ + MassTransit async post-registration | ✅ | `Infrastructure/Messaging/*`, consumer `RegistrationCreatedConsumer` |
| Outbox pattern | ✅ | `Persistence/Interceptors/ConvertDomainEventsToOutboxInterceptor.cs` + `OutboxProcessor` |
| Docker Compose (api/sql/rabbit/frontend) | ✅ | `docker-compose.yml` |
| Structured logging + correlation id + duration | ✅ | Serilog, `CorrelationIdMiddleware`, `RequestLoggingBehavior` |
| Health checks SQL + RabbitMQ via /health | ✅ | `AddSqlServer` + MassTransit bus health check |
| Integration tests (WebApplicationFactory + container) | ✅ | `IntegrationTests` (Testcontainers MsSql) |
| Frontend e2e happy + failure | ✅ | `frontend/e2e/registration.spec.ts` |
| Pagination/search endpoint | ❌ | NOT implemented — see "Open items" below |

---

## Open items (honest gaps)
- **I — pagination/search endpoint for registrations**: not implemented. The spec lists it as "if listing registrations is needed"; listing was not otherwise required. Flagged here rather than silently skipped. (Can be added as `GET /api/registrations?page=&pageSize=&search=`.)

## Definition-of-Done evidence (this session's runs; being re-run fresh for current proof)
1. Submit valid + get id — integration `Create_then_get_round_trips_the_registration` ✅
2. Invalid rejected FE + BE — frontend schema tests + integration `Invalid_field…400` ✅
3. Duplicate email/mobile 409 — integration ✅
4. City under wrong governorate rejected — integration ✅
5. Saved in SQL via migrations — integration (real container) ✅
6. Swagger examples — `/swagger` + example provider ✅
7. Unit tests cover rules/domain/handlers/mapping — 67 backend unit tests ✅
8. Integration tests cover create/lookup/persistence/duplicate/invalid combo — 9 integration tests ✅
9. Structured logging w/o PII — Serilog + behaviors, domain-only email logging ✅
10. Dependency direction holds — Domain csproj has no infra refs ✅

Engineering gates: `dotnet build` clean; backend 67 unit + 9 integration pass; frontend 19 tests pass + prod build; migration applies to fresh DB + seeds; bonus implemented except pagination (flagged).
