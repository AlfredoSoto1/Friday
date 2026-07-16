package assistant.backend.dto;

import com.fasterxml.jackson.annotation.JsonProperty;
import java.util.List;

public record BotVerifyMemberResult(
    boolean verified,
    String message,
    @JsonProperty("role_ids") List<String> roleIds) {

  public BotVerifyMemberResult {
    roleIds = roleIds == null ? List.of() : List.copyOf(roleIds);
  }
}
