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

**2. API** — runs on http://localhost:5209 (Swagger at `/swagger`):

```bash
dotnet run --project presentation/BookCenter.API --launch-profile http
```

**3. Frontend** — runs on http://localhost:5173 and proxies `/api` to the API:

```bash
cd frontend
npm install
npm run dev
```

**Tests:**

```bash
dotnet test
```

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
