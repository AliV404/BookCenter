# BookCenter

A small library management system: books, authors, categories, publishers, readers and loans.

| Part | Stack |
| --- | --- |
| API | ASP.NET Core 8, EF Core 8 (SQL Server), FluentValidation |
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

## Running locally

**1. Database** — set your SQL Server connection string in
`presentation/BookCenter.API/appsettings.Development.json`, then apply migrations:

```bash
dotnet tool restore                  # installs dotnet-ef (pinned in dotnet-tools.json)
dotnet ef database update -p src/Infrastructure/BookCenter.Persistence -s presentation/BookCenter.API
```

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

## Accounts and roles

| Role | Can do |
| --- | --- |
| anonymous | browse the catalog (`GET` books, authors, categories, publishers), register, sign in |
| **Reader** | the above + see **their own** loans (`GET /api/borrow-records` is scoped to them) |
| **Admin** | everything: manage catalog, readers and loans |

- `POST /api/auth/register` creates a Reader account *and* a reader card. If staff already created a card
  with the same email, the account is linked to it instead.
- `POST /api/auth/login` returns `{ token, expiresAt, user }`; send `Authorization: Bearer <token>`.
  5 wrong passwords lock the account for 5 minutes. `GET /api/auth/me` returns the current user.
- Endpoints require a signed-in user **by default** (fallback policy); public ones opt out with `[AllowAnonymous]`.
- `CreatedBy` / `ModifiedBy` / `DeletedBy` record the signed-in user's email.
- If staff delete a reader card, that reader can no longer sign in.

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
