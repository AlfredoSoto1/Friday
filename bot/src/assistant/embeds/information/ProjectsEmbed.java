package assistant.embeds.information;

import assistant.backend.dto.ProjectDTO;
import java.util.List;
import net.dv8tion.jda.api.EmbedBuilder;
import net.dv8tion.jda.api.entities.MessageEmbed;

public final class ProjectsEmbed {
  public MessageEmbed buildProject(int color, ProjectDTO project) {
    EmbedBuilder embed =
        new EmbedBuilder()
            .setColor(color)
            .setTitle("🚀  " + safe(project.getName(), "Student Project"))
            .setDescription(safe(project.getDescription(), "Explore this student-led project."));

    String links = links(project.getEmail(), project.getWebsite());
    if (!links.isBlank()) embed.addField("Connect", links, false);

    String social = social(project.getPlatforms(), project.getUrlhandle());
    if (!social.isBlank()) embed.addField("Social channels", social, false);
    return embed.build();
  }

  private String links(String email, String website) {
    StringBuilder result = new StringBuilder();
    if (present(email)) result.append("✉️  ").append(email.trim());
    if (present(website)) append(result, "🌐  [Visit website](" + website.trim() + ")");
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
