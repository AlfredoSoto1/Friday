package assistant.embeds;

public final class EmbedValues {
  private EmbedValues() {}

  public static String na(String value) {
    return value == null || value.isBlank() ? "N/A" : value.trim();
  }
}
