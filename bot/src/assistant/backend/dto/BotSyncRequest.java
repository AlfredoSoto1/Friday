package assistant.backend.dto;

import java.util.List;

public record BotSyncRequest(
    long guildId,
    String guildName,
    List<BotSyncRole> roles) {}
