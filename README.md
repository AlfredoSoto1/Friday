# Friday

Discord bot, backend, dashboard, and SLM workspace for incoming university student support.

## Local Environment

All credentials and local runtime settings live in `.env`. The file is ignored by Git. Use `.env-example` as the template for future machines.

Required secret values:

- `POSTGRES_PASSWORD`
- `DISCORD_BOT_TOKEN`
- `DISCORD_CLIENT_ID`
- `DISCORD_GUILD_ID`

## Devcontainer

Open the repository in VS Code and reopen it in the devcontainer. The devcontainer starts:

- `so-workspace`: C#, Java, Python, Node/TypeScript, Next.js, React tooling, GitHub CLI, and VS Code extensions.
- `so-db`: PostgreSQL 16 Alpine on port `5432`.

The repository is mounted at `/workspaces/friday`.

The devcontainer compose publishes service ports directly and connects both services through the `so-network` bridge network. Named volumes keep `backend/bin`, `webapp/node_modules`, and `webapp/.next` inside Docker volumes. Lifecycle scripts in `.devcontainer/scripts` install the PostgreSQL client on create and fix volume permissions on start.

## Runtime Compose

From the repository root:

```bash
docker compose --env-file .env -f infra/docker-compose.yml up
```

Services:

- `db`: PostgreSQL 16 Alpine, initialized from `database/schema.sql`.
- `app`: C# backend and Next.js dashboard.
- `agent`: Python Discord bot and SLM API.

Default local URLs:

- Backend: `http://localhost:8080`
- Webapp: `http://localhost:3000`
- SLM: `http://localhost:8090`

## App Commands

Backend:

```bash
dotnet run --project backend
```

Bot:

```bash
python -m bot.main
```

SLM:

```bash
uvicorn slm.main:app --host 0.0.0.0 --port 8090
```

Webapp:

```bash
npm install --prefix webapp
npm run dev --prefix webapp
```
