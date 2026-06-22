package edu.uprm.friday.bot.backend.dto;

public record BotXpResult(String discordUserId, int xp, int level, boolean leveledUp) {
}
