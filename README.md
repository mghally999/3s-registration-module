# 3S Registration

A registration module: a React + TypeScript form backed by a .NET 8 REST API built with clean architecture, CQRS, FluentValidation, EF Core / SQL Server 2019, RabbitMQ (MassTransit) with the outbox pattern, structured logging, health checks, and a full test suite.

This implements the "Developer Task - Registration Form" spec end to end, including the bonus items.

## What it does

A user fills in personal details (name, birth date, mobile, email) plus one to five addresses, where City is filtered by the selected Governorate. The API validates everything again on the server, stores it in SQL Server, prevents duplicate email/mobile, and returns the created id. After the registration commits, an integration event is published (via the outbox) for async post-processing like a welcome email/SMS.

## Solution layout

```
backend/
  src/
    Threes.Registration.Domain          entities, value objects, domain events, invariants (no ef/mediatr/automapper)
    Threes.Registration.Application      cqrs commands/queries, mediatr handlers, validators, automapper, behaviors
    Threes.Registration.Persistence      ef core dbcontext, fluent config, migrations, seed, outbox interceptor
    Threes.Registration.Infrastructure   masstransit/rabbitmq, outbox processor, lookup cache, clock, phone normalizer
    Threes.Registration.Api              controllers, swagger, problem-details, correlation id, serilog, health checks
  tests/
    Threes.Registration.UnitTests        domain, validators, handlers, mapping, data structures, algorithms
    Threes.Registration.IntegrationTests webapplicationfactory + testcontainers sql server 2019
frontend/
  src/                                   react + ts, react-hook-form + zod, reusable components, lookup hooks
  tests/                                 vitest + testing-library unit/component tests
  e2e/                                   playwright happy-path + validation-failure path
docker-compose.yml                       sql server + rabbitmq + api + frontend
```

The dependency direction is enforced by project references: Domain depends on nothing, Application depends on Domain, Persistence/Infrastructure depend inward, and only the Api host references everything to wire it together.

## Running it

### Everything in Docker (recommended)

```bash
docker compose up --build
```

Then:

- frontend: http://localhost:3000
- api + swagger: http://localhost:8080/swagger
- health: http://localhost:8080/health
- rabbitmq dashboard: http://localhost:15672 (guest / guest)

The api waits for SQL Server and RabbitMQ to be healthy, then applies migrations (which include the lookup seed) on startup.

### Running locally without Docker

Backend (needs the .NET 8 SDK and a SQL Server reachable on the connection string in `backend/src/Threes.Registration.Api/appsettings.json`):

```bash
cd backend
dotnet run --project src/Threes.Registration.Api
```

If you do not want RabbitMQ locally, set `Messaging:UseInMemory` to `true` and MassTransit uses its in-memory transport.

For local runs the connection string and broker credentials are **not** committed — copy the template and fill in your values (the real file is gitignored):

```bash
cp backend/src/Threes.Registration.Api/appsettings.Development.example.json \
   backend/src/Threes.Registration.Api/appsettings.Development.json
```

Frontend:

```bash
cd frontend
npm install
npm run dev      # http://localhost:5173, proxies /api to http://localhost:8080
```

## UI

The frontend is a formal, institutional design: a centered card on a calm canvas with a restrained bronze-gold accent and a transitional serif (Source Serif 4) for headings. Fields lay out on a responsive two-column CSS grid (first/last, mobile/email, governorate/city, building/flat side by side) that collapses to a single column under ~720px. It has tactile focus/hover/invalid input states, a staggered entrance that respects `prefers-reduced-motion`, an accessible success/error banner plus a corner toast, and keeps every label/`aria-*` wiring intact. Before/after screenshots (desktop + mobile) are in [`frontend/docs/`](frontend/docs/).

## Deployment

See [DEPLOYMENT.md](DEPLOYMENT.md) — frontend on Vercel, API + SQL Server + RabbitMQ on Railway, with the exact environment variables and the links to share.

## API

| Method | Route | Purpose |
| ------ | ----- | ------- |
| POST | `/api/registrations` | create a registration (201 + Location, 400 validation, 409 duplicate) |
| GET | `/api/registrations/{id}` | get a registration by id (404 if missing) |
| GET | `/api/registrations?page={p}&pageSize={n}&search={term}` | paged list of registrations, optionally filtered by email/mobile |
| GET | `/api/lookups/governorates` | active governorates, sorted by name |
| GET | `/api/lookups/cities?governorateId={id}` | cities for a governorate, sorted by name |

Errors use a consistent RFC7807 problem-details shape. Validation errors come back as a `ValidationProblemDetails` keyed by field; a duplicate returns 409 with a `field` hint so the frontend highlights the right input. Swagger shows a ready-to-run example request body.

## Validation

Every rule lives on the server as the source of truth, with the same rules mirrored on the client for instant feedback:

- names: required (first/last), optional middle, max 50, Arabic or English letters with single space / hyphen / apostrophe between letters, trimmed and space-collapsed.
- birth date: not in the future, minimum age 20 calculated accurately against today (not just the year difference).
- mobile: normalized to E.164 with libphonenumber and stored/compared in that form; unique.
- email: valid, max 254, unique and compared case-insensitively via a normalized column.
- addresses: 1 to 5, exactly one primary (a single address is always primary); city must belong to the chosen governorate.

On the backend the value objects in the Domain layer are the final guard (they throw if anyone bypasses validation), FluentValidation produces the friendly 400s, and the database adds unique indexes plus foreign keys as the last line.

## Data structures and algorithms

These are used where they actually fit, not for show:

- **Doubly linked list** (`Domain/Common/Collections/DoublyLinkedList`): backs the aggregate's `AddressBook`. Addresses are an ordered, mutable sequence (add at the tail, remove from the middle, re-point the primary by looking at neighbours), which is exactly what a doubly linked list is good at.
- **Singly linked list** (`SinglyLinkedList`): holds the domain events an aggregate raises. They are only appended and then drained once, so a tail-pointer append is all that is needed.
- **Hashmap** (`Infrastructure/Lookups/LookupCache`): a `Dictionary<int, CityBucket>` from governorate id to its cities, so "cities for governorate X" is O(1).
- **Merge sort** (`Application/Common/Algorithms/MergeSort`): a stable, recursive sort used to order governorates and cities by name before they are served and cached.
- **Binary search** (`Application/Common/Algorithms/BinarySearch`): each governorate's cities are also kept sorted by id, so the "does this city belong to this governorate" check that runs on every submitted address is an O(log n) probe.

## Messaging and the outbox

When a registration is saved, the `RegistrationCreatedDomainEvent` it raises is converted into an `OutboxMessage` row inside the same database transaction (a SaveChanges interceptor). A background `OutboxProcessor` then reads unprocessed rows and publishes them through MassTransit/RabbitMQ, where a consumer does the welcome email/SMS. This keeps the core create transaction consistent and independent of broker availability: if RabbitMQ is down the registration still succeeds and the event is delivered later.

## Observability

- Serilog structured logging with a correlation id (read from `X-Correlation-ID` or generated) attached to every log line for a request, plus per-request duration. Request bodies and personal data are never logged.
- `/health` reports SQL Server and the RabbitMQ bus.

## Tests

Backend:

```bash
cd backend
dotnet test                                                   # everything
dotnet test tests/Threes.Registration.UnitTests               # fast, no docker
dotnet test tests/Threes.Registration.IntegrationTests        # needs docker (testcontainers sql server 2019)
```

Unit tests cover the validation rules, domain/aggregate behavior, the command handler, the AutoMapper configuration, and the custom data structures and algorithms. Integration tests spin up the real api against a throwaway SQL Server 2019 container and cover create + get, duplicate email/mobile (409), invalid governorate/city combinations (400), and lookup filtering.

Frontend:

```bash
cd frontend
npm test         # vitest unit + component tests
npm run e2e      # playwright happy-path + validation-failure path
```

## Notes / deliberate choices

- Manual mapping is used in the write path (the command handler builds the aggregate through its factory so the domain rules run) and AutoMapper is used for the read DTO; the mapping profile is asserted valid in a test.
- The lookup tables are cached in memory because they change rarely and are read on every form load and every address validation. The cache invalidates on demand.
- Audit columns (`CreatedAtUtc`, `CreatedBy`, `UpdatedAtUtc`, `UpdatedBy`) are included on the registration.
