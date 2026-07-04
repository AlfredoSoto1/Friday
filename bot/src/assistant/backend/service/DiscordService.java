package assistant.backend.service;

import assistant.app.model.InteractionState;
import assistant.app.model.MemberPosition;
import assistant.backend.BackendClient;
import assistant.backend.dto.*;
import com.fasterxml.jackson.databind.JsonNode;
import java.io.File;
import java.util.*;
import java.util.concurrent.ConcurrentHashMap;
import java.util.stream.StreamSupport;

public final class DiscordService {
  private final Map<Long, InteractionStateDTO> states = new ConcurrentHashMap<>();

  public File findFromAssets(String path) {
    for (String root : List.of("database", "../database")) {
      File asset = new File(root, path);
      if (asset.isFile()) return asset;
    }
    throw new IllegalArgumentException("Bot asset not found: " + path);
  }

  public List<String> getProfanities() {
    return BackendClient.getData("/api/v1/bot/profanities").stream()
        .flatMap(n -> StreamSupport.stream(n.spliterator(), false))
        .map(JsonNode::asText)
        .toList();
  }

  public List<DiscordServerDTO> getAllRegisteredDiscordServers(int page, int size) {
    return BackendClient.enabledGuilds().stream().map(g -> server(g.guildId())).toList();
  }

  public Optional<DiscordServerDTO> getRegisteredDiscordServer(long guildId) {
    return Optional.of(server(guildId));
  }

  public List<String> getEffectiveRoleNames() {
    return Arrays.stream(MemberPosition.values()).map(Enum::name).toList();
  }

  public List<DiscordRoleDTO> getAllRoles(int page, int size, long guildId) {
    return BackendClient.getData("/api/v1/bot/servers/" + guildId + "/roles").stream()
        .flatMap(n -> StreamSupport.stream(n.spliterator(), false))
        .map(n -> role(n, guildId))
        .toList();
  }

  public Optional<DiscordRoleDTO> getEffectiveRole(MemberPosition position, long guildId) {
    String wanted = position.name().replace("_", " ");
    return getAllRoles(0, 100, guildId).stream()
        .filter(
            r ->
                r.getName().equalsIgnoreCase(wanted)
                    || r.getEffectivename().equalsIgnoreCase(position.name()))
        .findFirst();
  }

  public int registerDiscordServer(DiscordServerDTO server) {
    return 0;
  }

  public int registerRole(DiscordRoleDTO role) {
    return 0;
  }

  public boolean cacheInteractionState(InteractionState type, long state, long server) {
    InteractionStateDTO dto = new InteractionStateDTO();
    dto.setType(type);
    dto.setState(state);
    dto.setServerId(server);
    return states.putIfAbsent(state, dto) == null;
  }

  public boolean deleteCacheInteractionState(long state, long server) {
    return states.remove(state) != null;
  }

  public List<InteractionStateDTO> getCacheInteractionState(InteractionState type, long server) {
    return states.values().stream()
        .filter(s -> s.getType() == type && s.getServerId() == server)
        .toList();
  }

  private DiscordServerDTO server(long id) {
    var p = BackendClient.guildProfile(id);
    DiscordServerDTO d = new DiscordServerDTO();
    d.setServerId(id);
    d.setDepartment(departmentName(p.departmentProfile(), p.name()));
    d.setColor(p.theme().primaryColor());
    return d;
  }

  private String departmentName(String profile, String fallback) {
    return "INEL_ICOM".equalsIgnoreCase(profile)
        ? "ECE"
        : profile == null || profile.isBlank() ? fallback : profile;
  }

  private DiscordRoleDTO role(JsonNode n, long guild) {
    DiscordRoleDTO d = new DiscordRoleDTO();
    d.setId(n.path("role_id").asInt());
    d.setServerid(guild);
    d.setRoleid(n.path("discord_role_id").asLong());
    d.setName(n.path("name").asText());
    d.setEffectivename(n.path("name").asText().toUpperCase().replace(" ", "_"));
    return d;
  }
}
