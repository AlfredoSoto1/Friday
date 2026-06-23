package edu.uprm.friday.bot.backend.dto;

public record BotVerifyMemberRequest(
  String discordUserId,
  String email,
  String funFact
) {
}
