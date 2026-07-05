package assistant.embeds.contacts;

import assistant.backend.dto.ServiceDTO;
import net.dv8tion.jda.api.EmbedBuilder;
import net.dv8tion.jda.api.entities.MessageEmbed;

public final class ServicesEmbed {
  public MessageEmbed buildInfoPanel(int color, ServiceDTO service) {
    EmbedBuilder embed =
        new EmbedBuilder()
            .setColor(color)
            .setTitle("🏛️  " + safe(service.getName(), "University Service"))
            .setDescription(
                safe(service.getDescription(), "Contact information and available resources."));

    String location = location(service);
    if (!location.isBlank()) embed.addField("📍 Location", location, true);

    String availability = clean(service.getAvailability());
    if (!availability.isBlank()) embed.addField("🕒 Availability", availability, true);

    String contact = contact(service);
    if (!contact.isBlank()) embed.addField("🔗 Contact", contact, false);

    if (!service.getOffering().isEmpty())
      embed.addField("What they offer", "• " + String.join("\n• ", service.getOffering()), false);

    service
        .getAdditional()
        .forEach((name, values) -> embed.addField(name, "• " + String.join("\n• ", values), false));
    return embed.build();
  }

  private String location(ServiceDTO service) {
    String department = clean(service.getDepartmentAbbreviation());
    String building = clean(service.getBuildingName());
    String code = clean(service.getBuildingCode());
    StringBuilder value = new StringBuilder();
    if (!department.isBlank()) value.append("**Department:** ").append(department);
    if (!building.isBlank()) {
      if (!value.isEmpty()) value.append("\n");
      value.append("**Building:** ").append(building);
      if (!code.isBlank()) value.append(" (`").append(code.toUpperCase()).append("`)");
    }
    return value.toString();
  }

  private String contact(ServiceDTO service) {
    if (service.getContact() == null) return "";
    StringBuilder value = new StringBuilder();
    String email = clean(service.getContact().getEmail());
    if (!email.isBlank()) value.append("✉️  ").append(email);
    service
        .getContact()
        .getWebpages()
        .forEach(
            page ->
                append(
                    value,
                    "🌐  [" + safe(page.getDescription(), "Website") + "](" + page.getUrl() + ")"));
    service
        .getContact()
        .getSocialmedias()
        .forEach(social -> append(value, "• **" + social.getPlatform() + ":** " + social.getUrl()));
    return value.toString();
  }

  private void append(StringBuilder value, String line) {
    if (!value.isEmpty()) value.append("\n");
    value.append(line);
  }

  private String clean(String value) {
    return value == null ? "" : value.trim();
  }

  private String safe(String value, String fallback) {
    return clean(value).isBlank() ? fallback : value.trim();
  }
}
