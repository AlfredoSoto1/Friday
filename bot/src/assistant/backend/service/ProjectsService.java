package assistant.backend.service;

import assistant.backend.BackendClient;
import assistant.backend.dto.ProjectDTO;
import com.fasterxml.jackson.databind.JsonNode;
import java.net.URLEncoder;
import java.nio.charset.StandardCharsets;
import java.util.List;
import java.util.Optional;
import java.util.stream.StreamSupport;

public final class ProjectsService {
  public List<ProjectDTO> getProjects(String search) {
    return projects(0, 25, search);
  }

  public Optional<ProjectDTO> getProject(String name) {
    return projects(0, 25, name).stream()
        .filter(p -> p.getName().equalsIgnoreCase(name))
        .findFirst();
  }

  private List<ProjectDTO> projects(int page, int size, String search) {
    String path = "/api/v1/inelicom/projects?page_index=" + page + "&limit=" + size;
    if (search != null) path += "&search=" + URLEncoder.encode(search, StandardCharsets.UTF_8);
    return BackendClient.getData(path).map(data -> data).stream()
        .flatMap(items -> StreamSupport.stream(items.spliterator(), false))
        .map(this::map)
        .toList();
  }

  private ProjectDTO map(JsonNode node) {
    ProjectDTO dto = new ProjectDTO();
    dto.setId(node.path("project_id").asInt());
    dto.setName(node.path("name").asText());
    dto.setDescription(node.path("description").asText());
    return dto;
  }
}
