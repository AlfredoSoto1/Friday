package assistant.backend.service;

import assistant.backend.BackendClient;
import assistant.backend.dto.*;
import com.fasterxml.jackson.databind.JsonNode;
import java.net.*;
import java.nio.charset.StandardCharsets;
import java.util.*;
import java.util.stream.StreamSupport;

public final class BuildingService {
  public List<BuildingDTO> getAllBuilding(int page, int size) {
    return nodes("/api/v1/inelicom/buildings?page_index=" + page + "&limit=" + size).stream()
        .map(this::building)
        .toList();
  }

  public Optional<BuildingDTO> findBuilding(String roomCode) {
    return getAllBuilding(0, 100).stream()
        .filter(
            b ->
                roomCode
                        .toLowerCase()
                        .startsWith(b.getCode() == null ? "" : b.getCode().toLowerCase())
                    || b.getName().equalsIgnoreCase(roomCode))
        .findFirst();
  }

  public int insertBuilding(BuildingDTO b) {
    return 0;
  }

  public Optional<BuildingDTO> updateBuilding(int id, BuildingDTO b) {
    return Optional.empty();
  }

  public Optional<BuildingDTO> deleteBuilding(int id) {
    return Optional.empty();
  }

  private List<JsonNode> nodes(String p) {
    return BackendClient.getData(p).map(n -> n).stream()
        .flatMap(n -> StreamSupport.stream(n.spliterator(), false))
        .toList();
  }

  private BuildingDTO building(JsonNode n) {
    BuildingDTO d = new BuildingDTO();
    d.setId(n.path("building_id").asInt());
    d.setName(n.path("name").asText());
    d.setCode(n.path("code").asText());
    d.setGpin(n.path("gpin").asText());
    return d;
  }

  private String enc(String s) {
    return URLEncoder.encode(s, StandardCharsets.UTF_8);
  }
}
