# Ezra — Task management (take-home)

Small production-style task API (**ASP.NET Core 8**, **EF Core**, **SQLite**) and **React + TypeScript** UI. Fits the Ezra full-stack developer brief: clear API boundaries, persistence, optimistic concurrency, filters, paging, health check, and focused tests.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) (LTS recommended)

## Run the backend

```bash
cd backend/Ezra.Api
dotnet run
```

- API + Swagger: `https://localhost:7287` (HTTP `http://localhost:5287` also configured)
- Health: `GET /health`
- SQLite file: `ezra-todos.db` in the API project directory (created on first run via `EnsureCreated`)

CORS allows the Vite dev origin (`http://localhost:5173`). Adjust `appsettings.json` → `Cors:AllowedOrigins` for other hosts.

## Run the frontend

```bash
cd frontend
npm install
npm run dev
```

Open `http://localhost:5173`. The Vite dev server **proxies** `/api` and `/health` to `https://localhost:7287`, so you normally do not need to configure HTTPS in the browser for the UI.

## Tests

```bash
cd backend/Ezra.Api.Tests
dotnet test
```

- **Unit tests** exercise `TodoService` with an EF Core in-memory database (fast, deterministic).
- **Integration tests** use `WebApplicationFactory` with in-memory EF to validate HTTP behavior (including `409` on optimistic concurrency conflict).

```bash
cd frontend
npm run test:run
```

- **Frontend unit tests** use Vitest + React Testing Library for component behavior (compose form events, task list actions, and empty-state rendering).

## API sketch

| Method | Path | Notes |
|--------|------|--------|
| `GET` | `/api/todos?page=&pageSize=&isCompleted=` | Paged list; `pageSize` capped at 100 |
| `GET` | `/api/todos/{id}` | Single todo |
| `POST` | `/api/todos` | Body: `title`, optional `description`, optional `dueAtUtc` |
| `PUT` | `/api/todos/{id}` | Body includes `version`; mismatch → `409` |
| `DELETE` | `/api/todos/{id}` | Idempotent from client perspective (`404` if missing) |

## Assumptions & trade-offs

- **SQLite + `EnsureCreated`** keeps onboarding friction low for reviewers. For a real deployment I would switch to **migrations** (`dotnet ef migrations add`) and automated `Migrate()` at startup (or a release job), plus backup/restore playbooks.
- **Optimistic concurrency** uses an integer `version` on each todo so two editors do not silently overwrite each other; the UI refreshes after some failures.
- **Authn/authz** are out of scope for this exercise; a production MVP would add at least API keys or OIDC, row-level tenancy, and rate limiting at the edge.
- **Global error handling** uses ASP.NET Core’s `ProblemDetails` pipeline; validation errors return standard `400` problem bodies.

## What I would add next

- Real **migrations** and a managed database for multi-instance hosting.
- **Authentication** and per-user task lists.
- **Observability**: structured logs, metrics, tracing, correlation IDs.
- **E2E tests** (Playwright) against a containerized stack in CI.
- **Input hardening** at the edge (WAF) and stricter payload size limits.

## Repo layout

- `backend/Ezra.Api` — Web API, EF Core, services, controllers  
- `backend/Ezra.Api.Tests` — xUnit tests  
- `frontend/` — Vite + React UI  

Submit by pushing this tree to GitHub and sharing the repository link.
