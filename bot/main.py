import asyncio
import logging
import os

import discord
import httpx
from discord import app_commands
from dotenv import load_dotenv

load_dotenv()

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s %(levelname)s %(name)s %(message)s",
)

logger = logging.getLogger("friday.bot")

TOKEN = os.getenv("DISCORD_BOT_TOKEN", "")
GUILD_ID = os.getenv("DISCORD_GUILD_ID", "")
BACKEND_URL = os.getenv("BOT_BACKEND_URL", os.getenv("BACKEND_URL", "http://localhost:8080"))


class FridayBot(discord.Client):
    def __init__(self) -> None:
        intents = discord.Intents.default()
        super().__init__(intents=intents)
        self.tree = app_commands.CommandTree(self)

    async def setup_hook(self) -> None:
        register_commands(self)

        if GUILD_ID.isdigit():
            guild = discord.Object(id=int(GUILD_ID))
            self.tree.copy_global_to(guild=guild)
            await self.tree.sync(guild=guild)
            logger.info("Synced Discord commands for guild %s", GUILD_ID)
            return

        await self.tree.sync()
        logger.info("Synced global Discord commands")


async def backend_health() -> str:
    try:
        async with httpx.AsyncClient(timeout=5.0) as client:
            response = await client.get(f"{BACKEND_URL.rstrip('/')}/health")
            response.raise_for_status()
            return "backend ok"
    except Exception as exc:
        logger.warning("Backend health check failed: %s", exc)
        return "backend unavailable"


def register_commands(client: FridayBot) -> None:
    @client.tree.command(name="ping", description="Check whether Friday is online.")
    async def ping(interaction: discord.Interaction) -> None:
        backend_status = await backend_health()
        await interaction.response.send_message(f"pong ({backend_status})", ephemeral=True)


async def idle_without_token() -> None:
    logger.warning("DISCORD_BOT_TOKEN is not set. Bot will stay idle until a token is provided.")
    while True:
        await asyncio.sleep(3600)


async def main() -> None:
    if not TOKEN or TOKEN.startswith("replace-with-"):
        await idle_without_token()
        return

    client = FridayBot()
    async with client:
        await client.start(TOKEN)


if __name__ == "__main__":
    asyncio.run(main())
