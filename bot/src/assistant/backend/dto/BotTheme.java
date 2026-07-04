package assistant.backend.dto;

public record BotTheme(String primaryColor, String thumbnailUrl, String footerText) {
  public static BotTheme defaults() {
    return new BotTheme("2f80ed", null, "Friday");
  }
}
