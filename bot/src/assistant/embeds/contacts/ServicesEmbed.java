package assistant.embeds.contacts;

import assistant.backend.dto.ServiceDTO;
import assistant.embeds.EmbedValues;
import java.util.List;
import java.util.stream.Collectors;
import net.dv8tion.jda.api.EmbedBuilder;
import net.dv8tion.jda.api.entities.MessageEmbed;

public final class ServicesEmbed {
  public MessageEmbed buildInfoPanel(int color, ServiceDTO service) {
    EmbedBuilder embed =
        new EmbedBuilder()
            .setColor(color)
            .setTitle("🏛️  " + EmbedValues.na(service.getName()))
            .setDescription(limit(EmbedValues.na(service.getDescription()), 4096));

    String location = location(service);
    embed.addField("📍 Location", location, true);

    embed.addField("🕒 Availability", EmbedValues.na(service.getAvailability()), true);

    embed.addField("🔗 Contact", contact(service), false);

    embed.addField("What they offer", list(service.getOffering()), false);

    if (service.getAdditional() != null) {
      service.getAdditional().forEach((name, values) -> embed.addField(
          EmbedValues.na(name), list(values), false));
    }
    return embed.build();
  }

  private String location(ServiceDTO service) {
    String department = EmbedValues.na(service.getDepartmentAbbreviation());
    String building = EmbedValues.na(service.getBuildingName());
    String code = EmbedValues.na(service.getBuildingCode());
    StringBuilder value = new StringBuilder();
    value.append("**Department:** ").append(department).append("\n");
    value.append("**Building:** ").append(building);
    value.append(" (`").append(code.toUpperCase()).append("`)");
    return value.toString();
  }

  private String contact(ServiceDTO service) {
    if (service.getContact() == null) return "N/A";
    StringBuilder value = new StringBuilder("✉️  ").append(EmbedValues.na(service.getContact().getEmail()));
    append(value, "☎️  " + EmbedValues.na(service.getContact().getPhone()));
    List<?> webpages = service.getContact().getWebpages();
    if (webpages == null || webpages.isEmpty()) append(value, "🌐  N/A");
    else service.getContact().getWebpages().forEach(page -> {
      if (page == null) {
        append(value, "🌐  N/A");
        return;
      }
      String url = EmbedValues.na(page == null ? null : page.getUrl());
      append(value, "N/A".equals(url)
          ? "🌐  N/A"
          : "🌐  [" + EmbedValues.na(page.getDescription()) + "](" + url + ")");
    });
    if (service.getContact().getSocialmedias() != null) {
      service.getContact().getSocialmedias().forEach(social -> append(
          value,
          "• **" + EmbedValues.na(social == null ? null : social.getPlatform())
              + ":** " + EmbedValues.na(social == null ? null : social.getUrl())));
    }
    return value.toString();
  }

  private String list(List<String> values) {
    if (values == null || values.isEmpty()) return "N/A";
    return limit(values.stream().map(EmbedValues::na)
        .collect(Collectors.joining("\n• ", "• ", "")), 1024);
  }

  private String list(String[] values) {
    if (values == null || values.length == 0) return "N/A";
    return limit(java.util.Arrays.stream(values)
        .map(EmbedValues::na)
        .collect(Collectors.joining("\n• ", "• ", "")), 1024);
  }

  private String limit(String value, int maxLength) {
    if (value.length() <= maxLength) return value;
    return value.substring(0, maxLength - 3) + "...";
  }

  private void append(StringBuilder value, String line) {
    if (!value.isEmpty()) value.append("\n");
    value.append(line);
  }

}
