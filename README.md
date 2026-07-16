# Friday

Discord bot, backend, dashboard, and SLM workspace for incoming university student support.

## Local Environment

All credentials and local runtime settings live in `.env`. The file is ignored by Git. Use `.env-example` as the template for future machines.

```bash
cp .env-example .env
```

Required secret values:

- `POSTGRES_PASSWORD`
- `DISCORD_BOT_TOKEN`

The bot also reads `BACKEND_URL` and defaults to `http://localhost:8080`. The Discord application ID and server ID are copied from Discord during setup; they are not runtime secrets.

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

Default local URLs:

- Backend: `http://localhost:8080`
- Webapp: `http://localhost:3000`

The Java Discord bot and Python SLM are not services in the runtime Compose file and must be started separately when needed.

## App Commands

Backend:

```bash
dotnet run --project backend
```

Bot:

```bash
mvn -f bot/pom.xml package
BACKEND_URL=http://localhost:8080 java -jar bot/target/friday-bot-0.1.0-SNAPSHOT.jar
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

## Configure Friday for a Discord Server

Friday must know about a server before the bot can synchronize its roles. Complete the steps below in order.

### 1. Create and invite the Discord bot

1. Create an application in the [Discord Developer Portal](https://discord.com/developers/applications), open its **Bot** page, and create or reset the bot token.
2. Enable the **Server Members Intent** and **Message Content Intent** privileged gateway intents. The bot uses both when handling member and message events.
3. In **OAuth2 > URL Generator**, select the `bot` and `applications.commands` scopes. Grant these bot permissions:
   - View Channels
   - Send Messages
   - Embed Links
   - Attach Files
   - Read Message History
   - Manage Messages
   - Manage Roles
   - Manage Nicknames
4. Open the generated URL and add the bot to the target server.
5. Put the token in `.env` as `DISCORD_BOT_TOKEN`. Never commit or share this token.

The bot's Discord role must be above every program and team role it will grant. It must also be above the students whose nicknames it will update.

### 2. Prepare the server roles

Create the roles before synchronizing the server. Friday uses the role names below for administration and automatic roster assignments.

| Role | Purpose |
| --- | --- |
| `Administrator` | Authorizes the first `/assistant service:sync` command. Give this role to the operator performing setup. |
| `Bot Developer` | Authorizes later assistant administration commands. |
| `Moderator` | Required with `Bot Developer` when publishing the verification prompt. |
| `Prepa` | Assigned to every student imported from a roster. |
| `INEL`, `ICOM`, `INSO`, `CIIC` | Assigned from each student's program. Only the programs present in the uploaded roster are required. |
| Team roles | One or more Discord roles selected for each generated team. |

`Prepa` and program-role matching is case-insensitive, but the names must otherwise match. The `Bot Developer` lookup also accepts spaces, hyphens, or underscores. Keep the bot's role above all roles it needs to assign.

### 3. Start Friday

Start the database, backend, and dashboard:

```bash
docker compose --env-file .env -f infra/docker-compose.yml up
```

In a second terminal, build and start the bot:

```bash
mvn -f bot/pom.xml package
BACKEND_URL=http://localhost:8080 java -jar bot/target/friday-bot-0.1.0-SNAPSHOT.jar
```

Wait for the bot to appear online. On startup it registers the global and server-specific slash commands.

### 4. Register and synchronize the server

1. In Discord, enable **User Settings > Advanced > Developer Mode**. Right-click the server and select **Copy Server ID**.
2. Open `http://localhost:3000`, find **Discord servers**, and select **Add server**.
3. Enter the server name, paste the Discord server ID, select the owning department, and select **Add server**. New servers are active by default.
4. In Discord, run `/assistant service:sync` as a member with the `Administrator` role.
5. Confirm that the private response says `Synchronized N roles.` Open **View server** in the dashboard and verify that the team role selector contains the Discord roles.

Run the sync command again whenever Discord roles are added, renamed, deleted, recolored, or reordered. If the bot says the server is not registered, confirm that the dashboard's Discord ID exactly matches the copied server ID.

### 5. Prepare the student roster

The dashboard accepts `.csv`, `.xlsx`, and `.txt` files. The first row must contain the following columns:

| Column | Requirement |
| --- | --- |
| `first name` | Required and non-empty. `name` and `firstname` are also accepted. |
| `first last name` | Required and non-empty. `father last name` and `firstlastname` are also accepted. |
| `second last name` | Required header; the value may be empty. `mother last name` and `secondlastname` are also accepted. |
| `initial` | Required header; the value may be empty. |
| `personal email` or `institutional email` | At least one email column and value are required. If both have values, Friday stores the institutional email. |
| `program` | Use `INEL`, `ICOM`, `INSO`, or `CIIC`. The supported official program descriptions/codes are normalized automatically. |

Example CSV:

```csv
first name,first last name,second last name,initial,personal email,institutional email,program
Ana,Rivera,Soto,M,ana@example.com,ana.rivera@upr.edu,INEL
Luis,Perez,Ortiz,J,luis@example.com,luis.perez@upr.edu,ICOM
```

For an INEL or ICOM-only file, the `program` column may be omitted when the filename contains `INEL` or `ICOM`. Check the parsed student count before continuing because rows without a first name, first last name, usable email, or supported program are skipped. Treat roster files as sensitive student data.

### 6. Generate teams and save role assignments

1. In the dashboard, select **View server** for the synchronized server.
2. Under **Student list and team setup**, upload the roster.
3. Choose between 2 and 12 teams, select **Balanced** or **Randomized**, and select **Generate teams**.
4. Use the pencil button on every team to configure it:
   - Keep **Create a new team** enabled and enter a name, or disable it to update an existing backend team.
   - Select at least one synchronized Discord role. Every selected role is assigned to the team's students; the first role supplies the team's display color and primary role.
   - When updating an existing team, enable **Append members** to retain its current members. Leave it disabled to replace that team's stored membership.
5. Select **Edit groups** to move students between teams. Every student must be assigned before the distribution can be saved.
6. Select **Save team distribution**.

Saving imports the students and stores their `Prepa`, program, and selected team-role assignments in the backend. The save fails if `Prepa` or any program role used by the roster was not created and synchronized.

### 7. Publish verification and apply the roles

Roster upload does not immediately change Discord members. It prepares the roles that Friday grants when each student verifies their email.

1. Copy the ID of the text channel that should contain the verification prompt.
2. Run `/assistant-verification channel:<channel-id>` as a member with the `Administrator` or synchronized `Bot Developer` role.
3. Confirm that the bot publishes the verification embed. This command requires synchronized `Moderator` and `Bot Developer` roles and permission to send messages and embeds in the selected channel.
4. Each student selects **verify** and enters the email stored from the roster. When the email matches, Friday links the Discord account, updates the student's nickname, and grants the `Prepa`, program, and team roles stored for that student.

If verification succeeds but roles are not granted, confirm that the bot has **Manage Roles** and that its highest role is above all assigned roles. If a synchronized role is later changed in Discord, run `/assistant service:sync` again before changing or importing the roster.

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
