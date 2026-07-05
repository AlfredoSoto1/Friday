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
            .setTitle("🚀  " + safe(project.getName(), "Proyecto estudiantil"))
            .setDescription(
                safe(
                    project.getDescription(),
                    "Innovación creada por nuestra comunidad universitaria."))
            .addField("Enfoque", "Investigación  •  Diseño  •  Colaboración", false);

    String contact = contact(project.getEmail(), project.getWebsite());
    if (!contact.isBlank()) embed.addField("🔗 Conecta con el proyecto", contact, false);

    String social = social(project.getPlatforms(), project.getUrlhandle());
    if (!social.isBlank()) embed.addField("📣 Canales oficiales", social, false);
    return embed.build();
  }

  public MessageEmbed buildNotFound(int color, String query) {
    return new EmbedBuilder()
        .setColor(color)
        .setTitle("🔎 Proyecto no encontrado")
        .setDescription(
            "No encontramos un proyecto llamado **"
                + safe(query, "ese proyecto")
                + "**.\n"
                + "Verifica el nombre e inténtalo nuevamente.")
        .build();
  }

  private String contact(String email, String website) {
    StringBuilder result = new StringBuilder();
    if (present(email)) result.append("✉️  ").append(email.trim());
    if (present(website)) append(result, "🌐  [Visitar sitio oficial](" + website.trim() + ")");
    return result.toString();
  }

  private String social(List<String> platforms, List<String> handles) {
    StringBuilder result = new StringBuilder();
    if (platforms == null || handles == null) return "";
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
