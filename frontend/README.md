# Frontend (React + TypeScript + Vite)

Production-style task management UI for the Ezra take-home assignment.

## Prerequisites

- Node.js 20+
- Backend API running at `https://localhost:7287` (or update Vite proxy target)

## Run locally

```bash
npm install
npm run dev
```

Open `http://localhost:5173`.

## Build

```bash
npm run build
```

## Unit tests

```bash
npm run test:run
```

Test stack:
- Vitest
- React Testing Library
- JSDOM

## Notes

- Dev server proxies `/api` and `/health` to backend (`https://localhost:7287`) in `vite.config.ts`.
- UI is componentized into focused pieces under `src/components/`.
