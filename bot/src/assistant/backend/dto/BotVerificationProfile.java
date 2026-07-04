package assistant.backend.dto;

public record BotVerificationProfile(
  boolean enabled,
  String title,
  String description,
  String buttonLabel,
  String channelId,
  String verifiedRoleId,
  String bannerUrl
) {
  public static BotVerificationProfile defaults() {
    return new BotVerificationProfile(
      true,
      "Bienvenido al servidor",
      "Presiona el boton de verificacion para confirmar tu correo institucional.",
      "Verify",
      null,
      null,
      null);
  }
}
