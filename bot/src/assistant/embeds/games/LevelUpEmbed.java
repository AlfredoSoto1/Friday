package assistant.embeds.games;

import assistant.backend.dto.UserRankDTO;
import java.util.List;
import net.dv8tion.jda.api.EmbedBuilder;
import net.dv8tion.jda.api.entities.MessageEmbed;

public final class LevelUpEmbed {
  public MessageEmbed buildUserLevelUpCongrats(int color, String avatarUrl) {
    return new EmbedBuilder()
        .setColor(color)
        .setTitle("✨ Level Up")
        .setDescription("A new milestone has been reached.")
        .setThumbnail(avatarUrl)
        .build();
  }

  public MessageEmbed buildLeaderboard(int color, List<UserRankDTO> leaderboard) {
    EmbedBuilder embed =
        new EmbedBuilder()
            .setColor(color)
            .setTitle("🏆 Friday Leaderboard")
            .setDescription("The community’s current top performers.");

    if (leaderboard.isEmpty()) {
      return embed
          .setDescription("No ranking data is available yet. Be the first to earn XP!")
          .build();
    }

    for (int index = 0; index < leaderboard.size(); index++) {
      UserRankDTO rank = leaderboard.get(index);
      embed.addField(
          medal(index) + "  " + safe(rank.getUsername(), "Unknown member"),
          "**Level "
              + rank.getLevel()
              + "**  •  Rank #"
              + rank.getRank()
              + "\nNext milestone: `"
              + rank.getMilestone()
              + " XP`",
          false);
    }
    return embed.build();
  }

  public MessageEmbed buildLeaderboardPosition(int color, UserRankDTO userRank, String avatarUrl) {
    int milestone = Math.max(userRank.getMilestone(), 1);
    int progress = Math.clamp((int) (100.0 * userRank.getUserXP() / milestone), 0, 100);

    return new EmbedBuilder()
        .setColor(color)
        .setAuthor(safe(userRank.getUsername(), "Your profile"), null, avatarUrl)
        .setTitle("📈 Rank Overview")
        .setThumbnail(avatarUrl)
        .addField("Position", "#" + userRank.getRank(), true)
        .addField("Level", String.valueOf(userRank.getLevel()), true)
        .addField("Commands", String.valueOf(userRank.getCommandsUsed()), true)
        .addField(
            "Progress to next level", progressBar(progress) + "  **" + progress + "%**", false)
        .setFooter(userRank.getUserXP() + " / " + milestone + " XP")
        .build();
  }

  private String medal(int index) {
    return switch (index) {
      case 0 -> "🥇";
      case 1 -> "🥈";
      case 2 -> "🥉";
      default -> "`#" + (index + 1) + "`";
    };
  }

  private String progressBar(int percent) {
    int filled = (int) Math.ceil(10 * (percent / 100.0));
    return "▰".repeat(filled) + "▱".repeat(10 - filled);
  }

  private String safe(String value, String fallback) {
    return value == null || value.isBlank() ? fallback : value;
  }
}
