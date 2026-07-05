package assistant.embeds.games;

import java.time.Instant;
import net.dv8tion.jda.api.EmbedBuilder;
import net.dv8tion.jda.api.entities.MessageEmbed;

public final class GameEmbed {
  public MessageEmbed result(
      String title, String description, int color, String playerName, String avatarUrl) {
    EmbedBuilder builder =
        new EmbedBuilder()
            .setColor(color)
            .setTitle(title)
            .setDescription(description)
            .setAuthor(playerName, null, avatarUrl)
            .setTimestamp(Instant.now());

    if (avatarUrl != null && !avatarUrl.isBlank()) builder.setThumbnail(avatarUrl);
    return builder.build();
  }

  public MessageEmbed resultWithFooter(
      String title,
      String description,
      int color,
      String playerName,
      String avatarUrl,
      String footer) {
    return new EmbedBuilder(result(title, description, color, playerName, avatarUrl))
        .setFooter(footer)
        .build();
  }
}
