# BookCenter

[![CI](https://github.com/AliV404/BookCenter/actions/workflows/ci.yml/badge.svg)](https://github.com/AliV404/BookCenter/actions/workflows/ci.yml)

A small library management system: books, authors, categories, publishers, readers and loans.

| Part | Stack |
| --- | --- |
| API | ASP.NET Core 10, EF Core 10 (SQL Server), FluentValidation |
| Frontend | React 19 + TypeScript, Vite, React Router, TanStack Query |
| Tests | xUnit + `WebApplicationFactory` (EF InMemory) |

```
src/Core/BookCenter.Domain          entities (no dependencies)
src/Core/BookCenter.Application     DTOs, validators, service & repository interfaces, exceptions
src/Infrastructure/BookCenter.Persistence   EF Core DbContext, migrations, repositories, services
presentation/BookCenter.API         controllers, error handling, validation filter
tests/BookCenter.API.Tests          end-to-end API tests
frontend/                           React app
```

## Run everything with Docker

The quickest way to try it: SQL Server, the API and the web app in three containers.

```bash
cp .env.example .env        # then change the passwords and the JWT key in .env
sudo docker compose up -d --build
```

Open **http://localhost:8080** and sign in with `ADMIN_EMAIL` / `ADMIN_PASSWORD` from `.env`. The API applies
database migrations on start; data lives in the `db-data` volume (`docker compose down -v` deletes it).
nginx serves the frontend and forwards `/api` to the API, and `GET /api/health` reports whether the API can
reach its database. For a public server, put HTTPS in front of port 8080 (e.g. Caddy or a cloud load balancer).

## Continuous integration

`.github/workflows/ci.yml` runs on every push and pull request:

| Job | What it checks |
| --- | --- |
| API tests | all tests against a real SQL Server 2022, migrations in sync with the model, no vulnerable NuGet packages |
| Frontend | `npm run lint`, `npm run build`, `npm audit` |
| Docker Compose | builds and starts the whole stack, then checks health, admin sign-in and the web app |

## Running locally (for development)

**Prerequisites:** the [.NET 10 SDK](https://learn.microsoft.com/dotnet/core/install/linux) (Ubuntu:
`sudo apt install dotnet-sdk-10.0`), Node.js 20+, and Docker for SQL Server.

**1. Database:** start SQL Server 2022 in Docker (once; afterwards `sudo docker start bookcenter-sql`):

```bash
sudo docker run -d --name bookcenter-sql --restart unless-stopped -e ACCEPT_EULA=Y \
  -e "MSSQL_SA_PASSWORD=BookCenter_Dev_2026!" -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
```

`appsettings.Development.json` already points at it (database `BookCenterDB`). That password is only for
this local container; never use it for a real server. Then create or upgrade the schema:

```bash
dotnet tool restore                  # installs dotnet-ef (pinned in dotnet-tools.json)
dotnet ef database update -p src/Infrastructure/BookCenter.Persistence -s presentation/BookCenter.API
```

> **Upgrading a database created before Nov 2026?** The upgrade adds length limits and unique indexes.
> First run [`tools/pre-upgrade-check.sql`](tools/pre-upgrade-check.sql) against it (read-only; e.g. with
> `sudo docker exec -i bookcenter-sql /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P 'BookCenter_Dev_2026!' -d BookCenterDB < tools/pre-upgrade-check.sql`,
> or the SQL Server extension for VS Code). Fix any `BLOCKER` rows, then run the update. If a migration
> does fail, it is rolled back and nothing changes. `TotalCopies` is filled in from the available copies
> plus the books currently on loan.

**2. API** — runs on http://localhost:5209 (Swagger at `/swagger`; use **Authorize** with a token from `/api/auth/login`):

```bash
dotnet run --project presentation/BookCenter.API --launch-profile http
```

**3. Frontend** — runs on http://localhost:5173 and proxies `/api` to the API:

```bash
cd frontend
npm install
npm run dev
```

On first start the API creates the `Admin` and `Reader` roles and a development admin account from
`Seed` in `appsettings.Development.json` — **admin@bookcenter.local / Admin12345**. Change it, and never
reuse the development `Jwt:Key` outside your machine; in production set `Jwt__Key` (32+ chars) via an
environment variable or `dotnet user-secrets`.

**Tests:**

```bash
dotnet test
```

By default the tests use EF's InMemory database: fast, but it ignores SQL Server's constraints, unique
indexes and rowversion checks (the two concurrency tests are skipped). To run everything against real
SQL Server (each test class creates and drops its own database from the migrations):

```bash
# with the bookcenter-sql container from step 1 running:
export BOOKCENTER_TEST_SQLSERVER="Server=localhost,1433;User Id=sa;Password=BookCenter_Dev_2026!;TrustServerCertificate=True"
dotnet test
```

## Accounts and roles

| Role | Can do |
| --- | --- |
| anonymous | browse the catalog (`GET` books, authors, categories, publishers), register, sign in |
| **Reader** | the above + see **their own** loans (`GET /api/borrow-records` is scoped to them) |
| **Admin** | everything: manage catalog, readers, loans and user accounts |

- `POST /api/auth/register` creates a Reader account *and* a reader card. If staff already created a card
  with the same email, the account is linked to it instead.
- `POST /api/auth/login` returns `{ token, expiresAt, user }`; send `Authorization: Bearer <token>`.
  5 wrong passwords lock the account for 5 minutes. `GET /api/auth/me` returns the current user.
- Endpoints require a signed-in user **by default** (fallback policy); public ones opt out with `[AllowAnonymous]`.
- `CreatedBy` / `ModifiedBy` / `DeletedBy` record the signed-in user's email.
- If staff delete a reader card, that reader can no longer sign in. Creating a new card with the same
  email links it to their account again.
- `POST /api/auth/change-password` changes your own password and returns a fresh token.

### Managing accounts (Admin, `/api/users`, "Users" page)

| Action | Endpoint |
| --- | --- |
| list / search / filter by role | `GET /api/users?search=&role=Admin` |
| grant or remove admin rights | `PUT /api/users/{id}/admin` `{ "isAdmin": true }` |
| lock (until unlocked) / unlock | `POST /api/users/{id}/lock`, `POST /api/users/{id}/unlock` |
| set a new password | `POST /api/users/{id}/reset-password` `{ "newPassword": "..." }` |

- **Changes take effect immediately.** Each token carries the account's security stamp, which is
  checked on every request. Changing roles, locking, resetting the password, changing the reader's email
  or deleting their card rotates the stamp, so the user's existing sessions end at once instead of when
  the token expires.
- A reader's login email **is** their card's email: editing it on the Readers page changes how they sign in.
- Admins can't demote, lock or reset the password of their own account, so there is always at least one admin.
  An account without a reader card can't be demoted (it would be unable to sign in); lock it instead.

## API overview

All endpoints live under `/api`. Lists accept `?page=&pageSize=&search=` (pageSize max 100) and return
`{ items, totalCount, page, pageSize, totalPages }`. Errors are RFC 7807 ProblemDetails:
`400` validation (with per-field `errors`), `404` not found, `409` business-rule conflict.

| Resource | Endpoints |
| --- | --- |
| Books | `GET/POST /api/books`, `GET/PUT/DELETE /api/books/{id}` — filters: `authorId`, `categoryId`, `publisherId` |
| Authors, Categories, Publishers, Readers | same CRUD shape |
| Loans | `GET /api/borrow-records?status=active\|overdue\|returned&readerId=`, `POST /api/borrow-records`, `POST /api/borrow-records/{id}/return` |

### Business rules

- Deletes are **soft** (`IsDeleted` + a global query filter); loan history is never lost.
- Authors / categories / publishers with books, and books or readers with active loans, can't be deleted (409).
- Borrowing checks stock, reader existence and duplicate loans, and saves the loan + stock change in one
  transaction. A `rowversion` on `Books` turns concurrent borrows of the last copy into a 409 instead of
  negative stock.
- `TotalCopies` is the stock you own; `CopiesAvailable` is maintained by borrow/return.
