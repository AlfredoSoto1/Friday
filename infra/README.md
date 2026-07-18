# Friday Docker deployment

Use a Raspberry Pi running a 64-bit operating system. A 4 GB model is recommended when building the images locally.

1. From the repository root, create the root environment file:

   ```bash
   cp .env-example .env
   ```

2. Edit the root `.env`. Keep the existing variable names and set at least:

   - `POSTGRES_PASSWORD`
   - `DISCORD_BOT_TOKEN`
   - `WEBAPP_URL=http://<pi-lan-ip>:3000`
   - `NEXT_PUBLIC_API_BASE_URL=http://<pi-lan-ip>:8080`
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `NODE_ENV=production`

3. Start the complete stack from the repository root:

   ```bash
   COMPOSE_PARALLEL_LIMIT=1 docker compose --env-file .env -f infra/docker-compose.yml up --build -d
   ```

The dashboard is available at `http://<pi-lan-ip>:3000` and the backend at `http://<pi-lan-ip>:8080`.

Compose starts PostgreSQL, the .NET backend, the Next.js webapp, and the Discord bot. Every service reads the same root `.env`; values are not repeated in the Compose file. PostgreSQL data is kept in a persistent volume.

## Raspberry Pi memory limits

| Service | Limit |
| --- | ---: |
| PostgreSQL | 256 MiB |
| Backend | 384 MiB |
| Webapp | 256 MiB |
| Bot | 512 MiB |
| **Maximum total** | **1.375 GiB** |

These values are maximum ceilings, not reserved memory. The bot receives the largest allowance and uses the low-pause G1 collector for responsive Discord interactions. The backend has room for imports and API bursts. The mostly idle dashboard has the smallest Node.js heap.

The services remain separate because all four processes are required even in a combined container. Separate containers add little memory overhead while allowing per-service limits and independent restarts. Multi-stage Dockerfiles keep compilers and SDKs out of the runtime images.
