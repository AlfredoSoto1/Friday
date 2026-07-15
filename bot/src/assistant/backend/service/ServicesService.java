package assistant.backend.service;

import assistant.backend.BackendClient;
import assistant.backend.dto.*;
import com.fasterxml.jackson.databind.JsonNode;
import java.util.*;
import java.util.stream.StreamSupport;

public final class ServicesService {
  public List<ServiceDTO> getContacts(String search) {
    return BackendClient.getData("/api/v1/inelicom/contacts?limit=25" + (search.isBlank() ? "" : "&search=" + java.net.URLEncoder.encode(search, java.nio.charset.StandardCharsets.UTF_8))).stream()
        .flatMap(contacts -> StreamSupport.stream(contacts.spliterator(), false))
        .map(this::map)
        .toList();
  }

  public ServiceDTO getContact(int contactId) {
    return BackendClient.getData("/api/v1/inelicom/contacts/" + contactId)
        .map(this::map)
        .orElseGet(() -> unavailable(contactId));
  }

  private ServiceDTO unavailable(int contactId) {
    ServiceDTO d = new ServiceDTO();
    d.setId(contactId);
    d.setName("Contact information");
    d.setDescription("La información de este contacto no está configurada en el backend.");
    d.setAvailability("");
    d.setDepartmentAbbreviation("");
    d.setBuildingName("");
    d.setBuildingCode("");
    ContactDTO c = new ContactDTO();
    c.setEmail("");
    d.setContact(c);
    return d;
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
