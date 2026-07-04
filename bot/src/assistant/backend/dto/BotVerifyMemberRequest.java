package assistant.backend.dto;

public record BotVerifyMemberRequest(String discordUserId, String email, String funFact) {}
