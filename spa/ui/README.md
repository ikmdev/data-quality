# PIQI Data Quality UI

Standalone React SPA for PIQI data quality operations and batch testing.

## Quick Start

```bash
npm install
npm run dev
```

Runs on http://localhost:5173

Set API base URL in `.env`:

```bash
VITE_PIQI_API_BASE_URL=http://localhost:5026
```

## Project Structure

- **src/pages** — Page components (Submit, Batch, Dashboard, Checks)
- **src/components** — Reusable UI components
- **src/styles** — Global styles and design tokens
- **src/app** — App shell, routing, API clients, and hooks

## Build

```bash
npm run build
```

## Features (Planned)

- [x] Submit single JSON message to PIQI `ScoreAuditMessage`
- [x] Parse batch CSV/XLS/XLSX files in browser
- [x] Submit backend queued batch jobs (`SubmitBatchJob`)
- [x] Poll job status (`BatchJobStatus/:jobId`) and render run summary
- [x] Display assessment checks returned by completed batch jobs
- [x] Filter and paginate checks table

## API Contract

UI uses these backend endpoints under `${VITE_PIQI_API_BASE_URL}/PIQI`:

- `POST /ScoreMessage`
- `POST /ScoreAuditMessage`
- `POST /SubmitBatchJob`
- `GET /BatchJobStatus/:jobId`
- `POST /BatchAssessmentResults` (used server-side by worker flow)

## Notes

- TypeScript is configured with `noEmit`, so source-of-truth files are `.ts/.tsx` only.
- Batch scoring and assessment extraction are executed server-side in the API worker.
