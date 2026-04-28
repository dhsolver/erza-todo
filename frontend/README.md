# Frontend (React + TypeScript + Vite)

Task management frontend for the Ezra take-home project.

## Prerequisites

- Node.js 20+
- Backend running at `https://localhost:7287`

## Run Locally

```bash
npm install
npm run dev
```

Open `http://localhost:5173`.

## Build

```bash
npm run build
```

## Tests

```bash
npm run test:run
```

Test stack:
- Vitest
- React Testing Library
- JSDOM

## API/Proxy Notes

- Dev server proxies `/api` and `/health` to backend (`https://localhost:7287`) in `vite.config.ts`.
- UI is componentized into focused pieces under `src/components/`.
- If backend auth is enabled, unauthenticated requests to protected todo endpoints will fail with `401`.
