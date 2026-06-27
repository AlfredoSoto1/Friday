import axios from "axios";

import type {
  ApiEnvelope,
  ServerChannel,
  ServerMember,
  ServerRole,
  TeamOverview,
} from "@/features/server/server-types";

const backendUrl =
  process.env.NEXT_PUBLIC_API_BASE_URL ??
  "http://localhost:8080";

const client = axios.create({
  baseURL: `${backendUrl}/api/v1/bot`,
  timeout: 10000,
});

function unwrap<T>(body: ApiEnvelope<T> | T): T {
  if (body && typeof body === "object" && "status" in body) {
    const envelope = body as ApiEnvelope<T>;

    if (envelope.status === "error" || !envelope.data) {
      throw new Error(envelope.error?.message ?? "Backend request failed.");
    }

    return envelope.data;
  }

  return body as T;
}

export async function getTeamOverview(guildId: string): Promise<TeamOverview[]> {
  const [rolesResponse, channelsResponse, membersResponse] = await Promise.all([
    client.get<ApiEnvelope<ServerRole[]> | ServerRole[]>(
      `/servers/${guildId}/roles`
    ),
    client.get<ApiEnvelope<ServerChannel[]> | ServerChannel[]>(
      `/servers/${guildId}/channels`
    ),
    client.get<ApiEnvelope<ServerMember[]> | ServerMember[]>(
      `/servers/${guildId}/members`
    ),
  ]);
  const roles = unwrap(rolesResponse.data);
  const channels = unwrap(channelsResponse.data);
  const members = unwrap(membersResponse.data);

  return roles.map((role) => {
    const discordRoleId = role.discord_role_id ?? "";
    const channel = channels.find((item) => (
      item.name.toLowerCase() === role.name.toLowerCase()
    ));
    const memberCount = members.filter((member) => (
      member.role_ids.includes(discordRoleId)
    )).length;

    return {
      id: role.role_id,
      name: role.name,
      discordRoleId,
      color: `#${(role.color ?? 0x5865f2).toString(16).padStart(6, "0")}`,
      channelName: channel?.name ?? "Not linked",
      memberCount,
    };
  });
}
