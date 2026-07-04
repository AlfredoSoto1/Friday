package assistant.backend.dto;

public record BotGuildSummary(long guildId, String name, boolean enabled, String createdAt) {
}
