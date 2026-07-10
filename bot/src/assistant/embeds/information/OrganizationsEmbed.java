package assistant.embeds.information;

import assistant.backend.dto.OrganizationDTO;
import assistant.embeds.EmbedValues;
import java.util.List;
import net.dv8tion.jda.api.EmbedBuilder;
import net.dv8tion.jda.api.entities.MessageEmbed;

public final class OrganizationsEmbed {
  public MessageEmbed buildOrganization(int color, OrganizationDTO organization) {
    EmbedBuilder embed =
        new EmbedBuilder()
            .setColor(color)
            .setTitle("🌐  " + EmbedValues.na(organization.getName()))
            .setDescription(EmbedValues.na(organization.getDescription()));

    String links = links(organization.getEmail(), organization.getWebsite());
    embed.addField("Get involved", links, false);

    String social = social(organization.getPlatforms(), organization.getUrlhandle());
    embed.addField("Follow their work", social, false);
    return embed.build();
  }

  private String links(String email, String website) {
    StringBuilder result = new StringBuilder("✉️  ").append(EmbedValues.na(email));
    if (present(website)) append(result, "🌐  [Official website](" + website.trim() + ")");
    else append(result, "🌐  N/A");
    return result.toString();
  }

  private String social(List<String> platforms, List<String> handles) {
    if (platforms == null || handles == null || platforms.isEmpty() || handles.isEmpty()) return "N/A";
    StringBuilder result = new StringBuilder();
    for (int index = 0; index < Math.min(platforms.size(), handles.size()); index++)
      append(result, "• **" + EmbedValues.na(platforms.get(index)) + ":** " + EmbedValues.na(handles.get(index)));
    return result.isEmpty() ? "N/A" : result.toString();
  }

  private void append(StringBuilder builder, String line) {
    if (!builder.isEmpty()) builder.append("\n");
    builder.append(line);
  }

  private boolean present(String value) {
    return value != null && !value.isBlank();
  }

}
