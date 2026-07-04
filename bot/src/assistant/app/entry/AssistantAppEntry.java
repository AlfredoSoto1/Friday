package assistant.app.entry;

import assistant.backend.BackendClient;
import assistant.app.discord.BotConfig;
import assistant.app.discord.ECEAssistant;

public final class AssistantAppEntry {
  private AssistantAppEntry() {
  }

  public static void main(String[] args) throws InterruptedException {
    BotConfig config = BotConfig.load();

    if (!config.hasUsableToken()) {
      System.err.println("DISCORD_BOT_TOKEN is not set. Bot will stay idle until a token is provided.");
      Thread.currentThread().join();
      return;
    }

    BackendClient.configure(config.backendUrl());
    new ECEAssistant(config.discordBotToken()).start();
  }
}
