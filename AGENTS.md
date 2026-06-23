# Repository Guidelines

## Project Structure & Module Organization

Friday is a multi-service workspace. The C# backend lives in `backend/`, with domain models, controllers, repositories, and services under `backend/src/Api/`. The Next.js dashboard lives in `webapp/`; routes are in `webapp/app/`, features in `webapp/features/`, server access in `webapp/server/`, and UI primitives in `webapp/components/ui/`. The Java bot is in `bot/src/main/java/`, with JUnit tests in `bot/src/test/java/`. The Python FastAPI SLM service is in `slm/`. Schema and runtime orchestration live in `database/schema.sql` and `infra/docker-compose.yml`.

## Build, Test, and Development Commands

- `docker compose --env-file .env -f infra/docker-compose.yml up`: start the stack.
- `dotnet restore backend && dotnet build backend`: restore and build backend.
- `dotnet run --project backend`: run the backend API.
- `npm install --prefix webapp`: install dashboard deps.
- `npm run dev --prefix webapp`: run the dashboard.
- `npm run build --prefix webapp`: build production assets.
- `npm run lint --prefix webapp`: run the Next.js lint script.
- `mvn -f bot/pom.xml test`: run Java bot tests.
- `pip install -r slm/requirements.txt && uvicorn slm.main:app --host 0.0.0.0 --port 8090`: run the SLM API.

## Coding Style & Naming Conventions

Use two-space indentation in C#, Java, TypeScript, and CSS. C# types use `PascalCase`, interfaces use `IName`, and API JSON is configured as `snake_case`. Java packages stay under `edu.uprm.friday.bot`; classes use `PascalCase`, and test methods use descriptive camelCase names. TypeScript components use `PascalCase`; hooks use `use-name.ts` or `useName` exports. Keep shared UI in `webapp/components/ui/` and feature code in `webapp/features/`.

## Testing Guidelines

Java bot tests use JUnit 5 and the `*Test.java` pattern under `bot/src/test/java/`. Add tests near the package changed and run `mvn -f bot/pom.xml test` before submitting bot work. Backend, webapp, and SLM test suites are not currently defined; when adding them, document the command and keep tests close to the service.

## Commit & Pull Request Guidelines

Recent history mixes conventional commits such as `feat: ...` with imperative summaries such as `Refactor ...`. Prefer concise, imperative messages and use a prefix (`feat:`, `fix:`, `refactor:`, `test:`) when it fits. Pull requests should include a short description, affected service paths, test commands run, linked issues, and UI screenshots.

## Security & Configuration Tips

Do not commit `.env`; use `.env-example` as the template. Required local secrets include `POSTGRES_PASSWORD`, `DISCORD_BOT_TOKEN`, `DISCORD_CLIENT_ID`, and `DISCORD_GUILD_ID`. Keep generated build outputs such as `bin/`, `obj/`, `.next/`, and `node_modules/` out of reviews.
