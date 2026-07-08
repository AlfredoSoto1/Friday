# Repository Guidelines

## Project Structure & Module Organization

Friday is a multi-service workspace with a .NET backend, Next.js dashboard, Java Discord bot, Python SLM API, PostgreSQL schema, and Docker Compose orchestration.

- `backend/`: .NET 9 API project at `backend/Friday.Backend.csproj`. API code lives under `backend/src/Api/`, with controllers in `Controllers/`, domain records/models in `Domain/`, Dapper repositories in `Repositories/`, and business logic in `Services/`. Shared helpers live in `backend/src/Utils/`.
- `webapp/`: Next.js dashboard. App Router routes live in `webapp/app/`; dashboard feature modules live in `webapp/features/`; server-side entities and webservice wrappers live in `webapp/server/`; shared shadcn/radix UI primitives live in `webapp/components/ui/`; custom shared components live in `webapp/components/custom/`; reusable hooks live in `webapp/hooks/`; widgets live in `webapp/widgets/`.
- `bot/`: Java Discord bot using JDA. The Maven source root is `bot/src`, not `bot/src/main/java`. Bot code is under `bot/src/assistant/`, with Discord bootstrap code in `app/`, backend clients and DTOs in `backend/`, slash commands in `commands/`, embed builders in `embeds/`, and keyword/member interactions in `interactions/`.
- `slm/`: Python FastAPI service, currently centered on `slm/main.py` with dependencies in `slm/requirements.txt`.
- `database/`: PostgreSQL schema in `database/schema.sql`, seed data in `database/seeds/`, images in `database/images/`, and attendance/import source files in `database/attendance/`.
- `infra/`: local runtime orchestration in `infra/docker-compose.yml`.

Keep generated outputs such as `backend/bin/`, `backend/obj/`, `bot/target/`, `webapp/.next/`, `webapp/node_modules/`, and Python `__pycache__/` out of reviews.

## Build, Test, and Development Commands

- `docker compose --env-file .env -f infra/docker-compose.yml up`: start the local stack.
- `dotnet restore backend/Friday.Backend.csproj`: restore backend dependencies.
- `dotnet build backend/Friday.Backend.csproj`: build the backend.
- `dotnet run --project backend/Friday.Backend.csproj`: run the backend API.
- `npm install --prefix webapp`: install dashboard dependencies.
- `npm run dev --prefix webapp`: run the dashboard with Next.js.
- `npm run build --prefix webapp`: build production dashboard assets.
- `npm run lint --prefix webapp`: run the configured Next.js lint script.
- `mvn -f bot/pom.xml test`: compile the bot and run JUnit tests.
- `pip install -r slm/requirements.txt && uvicorn slm.main:app --host 0.0.0.0 --port 8090`: run the SLM API.

## Coding Style & Naming Conventions

Use two-space indentation in C#, Java, TypeScript, TSX, CSS, and SQL files unless a surrounding file already uses a different local style. Prefer small, service-local changes that follow the existing module boundaries.

C# backend conventions:

- Use `PascalCase` for types, records, public members, controllers, services, and repository classes; use `camelCase` for locals and parameters; prefix interfaces with `I`.
- Keep API endpoints in controllers thin. Put business rules in `Services/Implementation` and data access in `Repositories/Implementation`.
- Follow the existing partial-class pattern for grouped service and repository behavior, such as `BotService.Roster.cs` and `InelicomRepository.Project.cs`.
- Keep domain models grouped by bounded area under `Domain/Bot`, `Domain/Dashboard`, and `Domain/Inelicom`.
- API JSON is expected to use `snake_case`; preserve that contract when adding DTOs or response shapes.

Java bot conventions:

- Packages stay under `assistant.*` to match the current Maven source root.
- Command classes live in `bot/src/assistant/commands/<category>/` and usually extend `InteractionModel` while implementing `CommandI`.
- Use JDA-native components for Discord interactions: slash command options, embeds, buttons, action rows, modals, and ephemeral replies where appropriate.
- Keep backend HTTP access in `assistant.backend.BackendClient` and service wrappers under `assistant.backend.service`; do not duplicate backend calls inside commands.
- Use `PascalCase` for classes and descriptive `camelCase` for methods and variables.

TypeScript and React conventions:

- Components use `PascalCase`; hooks use `useName` exports and live in `webapp/hooks/` when shared.
- Keep route entry points in `webapp/app/` lean. Put workflow logic and UI composition in `webapp/features/<area>/`.
- Keep backend-facing types in `webapp/server/entities/` and HTTP/webservice code in `webapp/server/webservices/`.
- Prefer the existing `@/` path aliases from `webapp/components.json` and `webapp/tsconfig.json`.
- Use shadcn/radix primitives from `webapp/components/ui/` before creating new low-level UI building blocks.

Python SLM conventions:

- Keep FastAPI route definitions and lightweight orchestration in `slm/main.py` until the service grows enough to justify modules.
- Use explicit request/response models when adding non-trivial endpoints.

## Frontend Design Principles

The dashboard is an operational tool for Discord/server and department data management. Favor dense, scannable, task-oriented screens over marketing-style layouts.

- Preserve the current dark theme tokens in `webapp/app/globals.css` and `webapp/tailwind.config.ts`: background `#0d1117`, panel/card `#161b22`, border `#30363d`, Discord primary `#5865f2`, and semantic success/warning/danger colors.
- Use shadcn/radix components and lucide-react icons for controls. Prefer recognizable icon buttons for tool actions and pair them with accessible labels or tooltips when meaning is not obvious.
- Use cards for repeated objects, forms, tables, status summaries, and contained tools. Avoid nesting cards inside cards.
- Keep data tables, upload flows, server settings, rosters, and content managers optimized for repeated admin use: clear labels, stable controls, predictable empty/loading/error states, and no decorative copy.
- Use tabs, dialogs, drawers, dropdown menus, selects, switches, checkboxes, sliders, and segmented/toggle controls for the interaction patterns users already expect.
- Keep responsive layouts stable with explicit grid tracks, min widths, aspect ratios, and sensible wrapping. Text must not overflow buttons, cards, tables, or sidebars.
- Avoid adding unrelated decorative gradients, large hero sections, or one-off color palettes. New UI should look native to the existing dashboard.

## Testing Guidelines

- Bot work: run `mvn -f bot/pom.xml test`. Add JUnit 5 tests under the bot test source tree when introducing testable bot behavior.
- Backend work: run `dotnet build backend/Friday.Backend.csproj`. If backend tests are added later, document the command here and keep tests close to the service code.
- Webapp work: run `npm run lint --prefix webapp` and `npm run build --prefix webapp` for UI or route changes when dependencies are available.
- SLM work: at minimum import-check or run the FastAPI service command after endpoint changes. Add and document a Python test command when a test suite is introduced.
- Database changes: update `database/schema.sql` and any relevant seed/import files together. Note migration or data-loading implications in the PR.

## Commit & Pull Request Guidelines

Recent history mixes conventional commits such as `feat: ...` with imperative summaries such as `Refactor ...`. Prefer concise, imperative messages and use a prefix (`feat:`, `fix:`, `refactor:`, `test:`, `docs:`) when it clarifies the change.

Pull requests should include:

- Short description of the behavior or documentation change.
- Affected service paths, such as `backend/`, `webapp/`, `bot/`, `slm/`, `database/`, or `infra/`.
- Test/build commands run and any commands that could not be run.
- Linked issues or context when available.
- UI screenshots or short screen recordings for visible dashboard changes.

## Security & Configuration Tips

Do not commit `.env`; use `.env-example` as the local template when present. Required local secrets include `POSTGRES_PASSWORD`, `DISCORD_BOT_TOKEN`, `DISCORD_CLIENT_ID`, and `DISCORD_GUILD_ID`. Treat database imports and attendance files as potentially sensitive; avoid exposing student data in logs, screenshots, PR descriptions, or generated fixtures.
