package assistant.embeds.information;

import assistant.backend.dto.ProjectDTO;
import assistant.embeds.EmbedValues;
import java.util.List;
import net.dv8tion.jda.api.EmbedBuilder;
import net.dv8tion.jda.api.entities.MessageEmbed;

public final class ProjectsEmbed {
  public MessageEmbed buildProject(int color, ProjectDTO project) {
    EmbedBuilder embed =
        new EmbedBuilder()
            .setColor(color)
            .setTitle("🚀  " + EmbedValues.na(project.getName()))
            .setDescription(
                EmbedValues.na(project.getDescription()))
            .addField("Enfoque", "Investigación  •  Diseño  •  Colaboración", false);

    String contact = contact(project.getEmail(), project.getWebsite());
    embed.addField("🔗 Conecta con el proyecto", contact, false);

    String social = social(project.getPlatforms(), project.getUrlhandle());
    embed.addField("📣 Canales oficiales", social, false);
    return embed.build();
  }

  public MessageEmbed buildNotFound(int color, String query) {
    return new EmbedBuilder()
        .setColor(color)
        .setTitle("🔎 Proyecto no encontrado")
        .setDescription(
            "No encontramos un proyecto llamado **"
                + EmbedValues.na(query)
                + "**.\n"
                + "Verifica el nombre e inténtalo nuevamente.")
        .build();
  }

  private String contact(String email, String website) {
    StringBuilder result = new StringBuilder("✉️  ").append(EmbedValues.na(email));
    if (present(website)) append(result, "🌐  [Visitar sitio oficial](" + website.trim() + ")");
    else append(result, "🌐  N/A");
    return result.toString();
  }

  private String social(List<String> platforms, List<String> handles) {
    StringBuilder result = new StringBuilder();
    if (platforms == null || handles == null || platforms.isEmpty() || handles.isEmpty()) return "N/A";
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
