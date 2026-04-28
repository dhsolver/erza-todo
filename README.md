# Ezra — Task Management Take-Home

Full-stack task management app built with:

- Backend: ASP.NET Core 8 Web API + EF Core + SQLite
- Frontend: React + TypeScript + Vite

The current submission focuses on production-minded backend behavior: authentication, per-user data ownership, consistent DTO contracts, validation, and integration tests.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) (LTS recommended)

## Run Backend

```bash
cd backend/Ezra.Api
dotnet run
```

- API + Swagger: `https://localhost:7287`
- Health: `GET /health`
- SQLite file: `backend/Ezra.Api/ezra-todos.db` (created via `EnsureCreated` on startup)

### Auth Configuration

JWT signing key is required. Either:

- Set environment variable: `AUTH_SIGNING_KEY`
- Or set `Auth:SigningKey` in config (development value exists in `appsettings.Development.json`)

If missing, API startup fails intentionally.

## Run Frontend

```bash
cd frontend
npm install
npm run dev
```

Open `http://localhost:5173`.

The Vite dev server proxies `/api` and `/health` to `https://localhost:7287`.

## Tests

```bash
cd backend/Ezra.Api.Tests
dotnet test
```

- Backend unit tests validate service behavior with in-memory EF.
- Backend integration tests cover auth flow and protected endpoint behavior.

```bash
cd frontend
npm run test:run
```

- Frontend tests use Vitest + Testing Library.

## API Contract

### Auth

| Method | Path | Notes |
|--------|------|-------|
| `POST` | `/api/auth/register` | Registers user and returns JWT |
| `POST` | `/api/auth/login` | Logs in and returns JWT |

### Todos (Requires Bearer token)

| Method | Path | Notes |
|--------|------|-------|
| `GET` | `/api/todos` | Supports `page`, `pageSize`, `isCompleted`, `status`, `priority`, `search`, `sortBy`, `sortDir` |
| `GET` | `/api/todos/{id}` | Returns `404` if not found, `403` if owned by another user |
| `POST` | `/api/todos` | Create todo |
| `PUT` | `/api/todos/{id}` | Includes optimistic concurrency via `version`; returns `409` on mismatch |
| `DELETE` | `/api/todos/{id}` | Returns `404` or `403` appropriately |

## Validation and Error Handling

- DTOs use data annotations for required fields, length limits, and enum validation.
- Controllers return explicit `400`, `401`, `403`, `404`, and `409` outcomes.
- API uses `ProblemDetails` for error payload consistency.

## Notes and Trade-Offs

- SQLite + `EnsureCreated` keeps local setup fast. For production, use EF migrations and managed DB operations.
- JWT signing key is environment/config driven; do not commit real production secrets.
- Current frontend and backend are maintained in one repository for assignment delivery.

## Repository Layout

- `backend/Ezra.Api` — API, auth, persistence, services
- `backend/Ezra.Api.Tests` — xUnit unit/integration tests
- `frontend` — React/Vite UI

Submit by sharing this repository URL.
