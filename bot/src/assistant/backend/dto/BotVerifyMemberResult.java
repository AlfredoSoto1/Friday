package assistant.backend.dto;

import java.util.List;

public record BotVerifyMemberResult(boolean verified, String message, List<String> roleIds) {
}
