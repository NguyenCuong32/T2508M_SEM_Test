# BattleGame – Developing Microsoft Azure Solutions (SET01)

Full-stack solution for the BattleGame exam using **Azure Functions (Node.js/TypeScript)** + **React**.

---

## Project Structure

```
Pram/
├── database/
│   ├── create-database.sql     # DDL: creates BATTLEGAME tables
│   └── seed-data.sql           # Sample data
│
├── backend/                    # Azure Functions API
│   ├── host.json
│   ├── local.settings.json     # Local env vars (do NOT commit to git)
│   ├── package.json
│   ├── tsconfig.json
│   └── src/
│       ├── config/             # DB connection pool
│       ├── models/             # TypeScript interfaces & DTOs
│       ├── repositories/       # SQL queries (data-access layer)
│       ├── services/           # Business logic & validation
│       ├── controllers/        # HTTP request/response handling
│       ├── functions/          # Azure Function entry points
│       └── shared/             # Shared helpers (response format)
│
├── frontend/                   # React + TypeScript SPA
│   ├── index.html
│   ├── vite.config.ts
│   ├── package.json
│   └── src/
│       ├── types/              # Shared TypeScript types
│       ├── services/           # API calls (axios)
│       ├── components/         # Reusable UI components
│       ├── pages/              # Page-level components
│       ├── App.tsx
│       └── main.tsx
│
├── DEPLOY.md                   # Step-by-step Azure deployment guide
└── README.md
```

---

## APIs

| # | Method | Route | Description |
|---|--------|-------|-------------|
| 1 | POST | `/api/registerplayer` | Register a new player |
| 2 | POST | `/api/createasset` | Create a new game asset |
| 3 | GET  | `/api/getassetsbyplayer` | Report: players with their assets |

### Request Bodies

**POST /api/registerplayer**
```json
{
  "PlayerName": "player1",
  "FullName": "Nguyen Van A",
  "Age": "20",
  "Level": 1,
  "Email": "player1@game.com"
}
```

**POST /api/createasset**
```json
{
  "AssetName": "Hero 1",
  "LevelRequire": 1
}
```

---

## Local Development

### Backend

```bash
cd backend
npm install
npm run build
func start
# API available at http://localhost:7071/api/
```

### Frontend

```bash
cd frontend
npm install
npm run dev
# App available at http://localhost:3000
```

> The Vite dev server proxies `/api/*` → `http://localhost:7071` automatically.

---

## Database

- **Database First** approach
- Engine: SQL Server (or Azure SQL)
- Run `database/create-database.sql` then `database/seed-data.sql`

---

## Deploy to Azure

See [DEPLOY.md](./DEPLOY.md) for the full step-by-step guide.
