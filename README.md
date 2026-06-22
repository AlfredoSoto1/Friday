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

- `so-workspace`: .NET, Git, GitHub CLI, Java, Python, Node/TypeScript, Next.js, React tooling, PostgreSQL client tools, and VS Code extensions.
- `so-db`: PostgreSQL 16 Alpine on port `5432`.

The repository is mounted at `/workspaces/friday`.

The devcontainer compose publishes service ports directly and connects both services through the `so-network` bridge network. Named volumes keep `backend/bin`, `webapp/node_modules`, and `webapp/.next` inside Docker volumes. Lifecycle scripts in `.devcontainer/scripts` install the PostgreSQL client and curl on create, then fix volume permissions on start.

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

## Usage

### Application Commands

| Command                         | Description                                                                    |
| ------------------------------- | ------------------------------------------------------------------------------ |
| `/calendario`                   | Links to the academic calendar of UPRM.                                        |
| `/estudiante-orientador`        | Lists all EO's in the server by department.                                    |
| `/rules`                        | Lists server rules.                                                            |
| `/guia-prepistica`              | Provides a PDF guide on prepas.                                                |
| `/faculty`                      | Displays detailed information about faculty members.                           |
| `/ls_projects`                  | Provides information on ongoing projects and research.                         |
| `/ls_student_orgs`              | Lists student organizations and their activities.                              |
| `/salon`                        | Finds a building on campus based on its code.                                  |
| `/lab`                          | Finds a lab on campus based on its code.                                       |
| `/links`                        | Provides useful links related to studies and resources.                        |
| `/made-web`                     | Links to the MADE (Media Arts and Digital Entertainment) website.              |
| `/contact-dcsp`                 | Provides information about the Department of Computer Science and Engineering. |
| `/contact-department`           | Displays information about specific departments at UPRM.                       |
| `/contact-decanato-estudiantes` | Provides contact information for the Dean of Students.                         |
| `/contact-guardia-univ`         | Provides contact information for campus security.                              |
| `/contact-asesoria-academica`   | Provides guidance on academic matters and advisories.                          |
| `/contact-asistencia-economica` | Provides information about financial aid and economic assistance.              |
| `/curriculo`                    | Provides general information about academic curricula.                         |
| `/guia-prepistica`              | Provides a guide for incoming freshmen.                                        |
| `/faq`                          | Answers frequently asked questions about the Discord server.                   |
| `/help`                         | Displays a comprehensive help menu for navigating the bot's commands.          |
| `/map`                          | Provides a link to the UPRM campus map.                                        |
| `/rules`                        | Displays the rules and guidelines for the Discord server.                      |
