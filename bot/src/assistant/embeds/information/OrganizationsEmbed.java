package assistant.embeds.information;

import assistant.backend.dto.OrganizationDTO;
import java.util.List;
import net.dv8tion.jda.api.EmbedBuilder;
import net.dv8tion.jda.api.entities.MessageEmbed;

public final class OrganizationsEmbed {
  public MessageEmbed buildOrganization(int color, OrganizationDTO organization) {
    EmbedBuilder embed =
        new EmbedBuilder()
            .setColor(color)
            .setTitle("🌐  " + safe(organization.getName(), "Student Organization"))
            .setDescription(safe(organization.getDescription(), "Meet this campus community."));

    String links = links(organization.getEmail(), organization.getWebsite());
    if (!links.isBlank()) embed.addField("Get involved", links, false);

    String social = social(organization.getPlatforms(), organization.getUrlhandle());
    if (!social.isBlank()) embed.addField("Follow their work", social, false);
    return embed.build();
  }

  private String links(String email, String website) {
    StringBuilder result = new StringBuilder();
    if (present(email)) result.append("✉️  ").append(email.trim());
    if (present(website)) append(result, "🌐  [Official website](" + website.trim() + ")");
    return result.toString();
  }

  private String social(List<String> platforms, List<String> handles) {
    StringBuilder result = new StringBuilder();
    for (int index = 0; index < Math.min(platforms.size(), handles.size()); index++)
      append(result, "• **" + platforms.get(index) + ":** " + handles.get(index));
    return result.toString();
  }

  private void append(StringBuilder builder, String line) {
    if (!builder.isEmpty()) builder.append("\n");
    builder.append(line);
  }

  private boolean present(String value) {
    return value != null && !value.isBlank();
  }

  private String safe(String value, String fallback) {
    return present(value) ? value.trim() : fallback;
  }
}
