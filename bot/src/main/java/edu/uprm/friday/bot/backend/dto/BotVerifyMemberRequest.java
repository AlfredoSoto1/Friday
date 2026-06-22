package edu.uprm.friday.bot.backend.dto;

public record BotVerifyMemberRequest(
  String discordUserId,
  String discordUsername,
  String email,
  String funFact
) {
}
