package assistant.backend.dto;

import java.util.List;

public record BotCommandResponse(
    String commandName,
    String title,
    String description,
    String imageUrl,
    String url,
    String color,
    boolean ephemeral,
    List<BotButtonDefinition> buttons) {
  public static BotCommandResponse defaultFor(String commandName) {
    return new BotCommandResponse(
        commandName,
        "/" + commandName,
        "/"
            + commandName
            + " is wired correctly. Configure this response in the backend for this server.",
        null,
        null,
        null,
        true,
        List.of());
  }
}
