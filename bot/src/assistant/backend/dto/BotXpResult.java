package assistant.backend.dto;

public record BotXpResult(String discordUserId, int xp, int level, boolean leveledUp) {
}
