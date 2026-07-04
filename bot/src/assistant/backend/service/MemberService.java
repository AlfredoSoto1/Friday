package assistant.backend.service;

import assistant.app.model.*;
import assistant.backend.BackendClient;
import assistant.backend.dto.*;
import com.fasterxml.jackson.databind.JsonNode;
import java.util.*;
import java.util.function.Consumer;
import java.util.stream.StreamSupport;

public final class MemberService {
  public void shutdownVerificationQueueService() {}

  public <T> void queueVerificationTask(T value, Consumer<T> task) {
    task.accept(value);
  }

  public boolean stampMemberPresence(MemberDTO m, long guild) {
    return true;
  }

  public List<EmailDTO> getEmails(int p, int s, long g) {
    return List.of();
  }

  public long memberCount(MemberRetrievement r, long g) {
    return members(g).size();
  }

  public List<MemberDTO> getAllMembers(int p, int s, MemberRetrievement r, long g) {
    return members(g).stream().skip((long) p * s).limit(s).toList();
  }

  public Optional<MemberDTO> getMember(String email) {
    return BackendClient.getData("/api/v1/bot/users").stream()
        .flatMap(n -> StreamSupport.stream(n.spliterator(), false))
        .filter(n -> n.path("email").asText().equalsIgnoreCase(email))
        .map(this::member)
        .findFirst();
  }

  public Optional<TeamDTO> getMemberTeam(String email, long g) {
    return Optional.empty();
  }

  public List<DiscordRoleDTO> getMemberRoles(String email, long g) {
    return List.of();
  }

  public List<PrepaOrientadorDTO> getPrepaOrientadores(String email, long g) {
    return List.of();
  }

  public boolean isOrientador(String email, long g) {
    return false;
  }

  public int addMember(MemberDTO m, MemberPosition p, long g, String t) {
    return 0;
  }

  public int addEOrientador(MemberDTO m, long g, String t) {
    return 0;
  }

  public int addPrepa(MemberDTO m, long g, String t) {
    return 0;
  }

  public boolean addMembers(List<MemberDTO> m, MemberPosition p, long g, String t) {
    return false;
  }

  public boolean deleteMembers(List<Integer> ids) {
    return false;
  }

  private List<MemberDTO> members(long g) {
    return BackendClient.getData("/api/v1/bot/servers/" + g + "/members").stream()
        .flatMap(n -> StreamSupport.stream(n.spliterator(), false))
        .map(this::member)
        .toList();
  }

  private MemberDTO member(JsonNode n) {
    MemberDTO d = new MemberDTO();
    d.setId(n.path("user_id").asInt());
    d.setEmail(n.path("email").asText());
    d.setUsername(n.path("username").asText());
    String[] names = n.path("fullname").asText().split(" ", 2);
    d.setFirstname(names.length > 0 ? names[0] : "");
    d.setLastname(names.length > 1 ? names[1] : "");
    d.setInitial("");
    d.setFunfact(n.path("fun_fact").asText());
    d.setVerified(n.path("verified").asBoolean());
    return d;
  }
}
