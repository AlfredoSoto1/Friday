package assistant.app.embeds;

import assistant.backend.dto.BotCommandResponse;
import assistant.backend.dto.BotGuildProfile;
import net.dv8tion.jda.api.EmbedBuilder;
import net.dv8tion.jda.api.entities.MessageEmbed;

import java.time.Instant;

public final class EmbedFactory {
  public MessageEmbed commandEmbed(BotGuildProfile profile, BotCommandResponse response) {
    EmbedBuilder builder = base(profile, response.color())
      .setTitle(response.title())
      .setDescription(response.description());

    if (response.imageUrl() != null && !response.imageUrl().isBlank()) {
      builder.setImage(response.imageUrl());
    }
    if (response.url() != null && !response.url().isBlank()) {
      builder.setUrl(response.url());
    }
    return builder.build();
  }

  public MessageEmbed verificationEmbed(BotGuildProfile profile) {
    EmbedBuilder builder = base(profile, null)
      .setTitle(profile.verification().title())
      .setDescription(profile.verification().description())
      .addField("Pasos", "1. Presiona el boton de verificacion.\n2. Escribe tu correo institucional.\n3. Espera que Friday asigne tus roles.", false);

    if (profile.verification().bannerUrl() != null && !profile.verification().bannerUrl().isBlank()) {
      builder.setImage(profile.verification().bannerUrl());
    }
    return builder.build();
  }

  public MessageEmbed welcomeEmbed(BotGuildProfile profile) {
    EmbedBuilder builder = base(profile, null)
      .setTitle(profile.welcome().title())
      .setDescription(profile.welcome().description());

    if (profile.welcome().bannerUrl() != null && !profile.welcome().bannerUrl().isBlank()) {
      builder.setImage(profile.welcome().bannerUrl());
    }
    return builder.build();
  }

  public MessageEmbed levelUpEmbed(BotGuildProfile profile, String mention, int level) {
    return base(profile, null)
      .setTitle("Level up")
      .setDescription(mention + " reached level **" + level + "**.")
      .build();
  }

  public MessageEmbed gameEmbed(BotGuildProfile profile, String title, String description) {
    return base(profile, null)
      .setTitle(title)
      .setDescription(description)
      .build();
  }

  private EmbedBuilder base(BotGuildProfile profile, String overrideColor) {
    EmbedBuilder builder = new EmbedBuilder()
      .setColor(parseColor(overrideColor == null ? profile.theme().primaryColor() : overrideColor))
      .setTimestamp(Instant.now());

    if (profile.theme().thumbnailUrl() != null && !profile.theme().thumbnailUrl().isBlank()) {
      builder.setThumbnail(profile.theme().thumbnailUrl());
    }
    if (profile.theme().footerText() != null && !profile.theme().footerText().isBlank()) {
      builder.setFooter(profile.theme().footerText());
    }
    return builder;
  }

  private int parseColor(String value) {
    if (value == null || value.isBlank()) {
      return 0x2f80ed;
    }
    try {
      return Integer.parseInt(value.replace("#", ""), 16);
    } catch (NumberFormatException ignored) {
      return 0x2f80ed;
    }
  }
}
