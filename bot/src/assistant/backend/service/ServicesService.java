package assistant.backend.service;

import assistant.backend.BackendClient;
import assistant.backend.dto.*;
import com.fasterxml.jackson.databind.JsonNode;
import java.net.*;
import java.nio.charset.StandardCharsets;
import java.util.*;
import java.util.stream.StreamSupport;

public final class ServicesService {
  public List<String> getServiceNames(String contactType) {
    return contacts(null, contactType).stream().map(ServiceDTO::getName).toList();
  }

  public List<ServiceDTO> getAllServices(int p, int s, String contactType) {
    return contacts(null, contactType);
  }

  public ServiceDTO getService(String name, String contactType) {
    String lookup = contactType == null ? lookupName(name) : name;
    return contacts(lookup, contactType).stream().findFirst().orElseGet(() -> unavailable(name, contactType));
  }

  public ServiceDTO getServiceByContactType(String contactType) {
    return contacts(null, contactType).stream()
        .findFirst()
        .orElseGet(() -> unavailable("Contact information", contactType));
  }

  private String lookupName(String name) {
    String n = name.toLowerCase(Locale.ROOT);
    if (n.contains("consejer") || n.contains("dcsp")) return "DCSP";
    if (n.contains("transito") || n.contains("vigilancia") || n.contains("guardia"))
      return "Guardia";
    if (n.contains("asesor")) return "Asesoria";
    if (n.contains("asistencia")) return "Asistencia";
    return name;
  }

  private ServiceDTO unavailable(String name) {
    return unavailable(name, null);
  }

  private ServiceDTO unavailable(String name, String contactType) {
    ServiceDTO d = new ServiceDTO();
    d.setName(name);
    d.setDescription("La información de este contacto no está configurada en el backend.");
    d.setAvailability("");
    d.setDepartmentAbbreviation(contactType != null ? contactType : "");
    d.setBuildingName("");
    d.setBuildingCode("");
    ContactDTO c = new ContactDTO();
    c.setEmail("");
    d.setContact(c);
    return d;
  }

  private List<ServiceDTO> contacts(String search, String contactType) {
    String p =
        "/api/v1/inelicom/contacts?limit=25"
            + (search == null ? "" : "&search=" + URLEncoder.encode(search, StandardCharsets.UTF_8))
            + (contactType == null ? "" : "&contactType=" + contactType);
    return BackendClient.getData(p).map(n -> n).stream()
        .flatMap(n -> StreamSupport.stream(n.spliterator(), false))
        .map(this::map)
        .toList();
  }

  private ServiceDTO map(JsonNode n) {
    ServiceDTO d = new ServiceDTO();
    d.setId(n.path("contact_id").asInt());
    d.setName(n.path("name").asText());
    String description = n.path("description").asText();
    d.setDescription(description.isBlank() ? n.path("phone").asText() : description);
    Arrays.stream(n.path("services").asText().split(";"))
        .map(String::trim).filter(service -> !service.isEmpty()).forEach(d.getOffering()::add);
    d.setAvailability("");
    d.setDepartmentAbbreviation("");
    d.setBuildingName("");
    d.setBuildingCode("");
    ContactDTO c = new ContactDTO();
    c.setEmail(n.path("email").asText());
    c.setPhone(n.path("phone").asText());
    if (!n.path("website").asText().isBlank()) {
      WebpageDTO w = new WebpageDTO();
      w.setDescription("Website");
      w.setUrl(n.path("website").asText());
      c.addWebpages(w);
    }
    d.setContact(c);
    return d;
  }
}
