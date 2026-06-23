import axios from "axios";

import type {
  AdminRecord,
  ApiEnvelope,
  ApiMeta,
  BackendStatusEntity,
  BotChannel,
  BotMember,
  BotRole,
  BotUser,
  CatalogSummaryEntity,
  DashboardDataEntity,
  GuildProfile,
  GuildSummary,
  RegisterMemberRequest,
} from "@/features/dashboard/admin-types";

const backendUrl =
  process.env.NEXT_PUBLIC_API_BASE_URL ??
  process.env.BACKEND_URL ??
  "http://localhost:8080";

const client = axios.create({
  baseURL: backendUrl,
  timeout: 10000,
});

function unwrap<T>(body: ApiEnvelope<T> | T): { data: T; meta: ApiMeta | null } {
  if (body && typeof body === "object" && "status" in body && "data" in body) {
    const envelope = body as ApiEnvelope<T>;
    if (envelope.status === "error") {
      throw new Error(envelope.error?.message ?? "Backend request failed.");
    }

    return { data: envelope.data as T, meta: envelope.meta ?? null };
  }

  return { data: body as T, meta: null };
}

export async function apiGet<T>(path: string): Promise<{ data: T; meta: ApiMeta | null }> {
  const response = await client.get<ApiEnvelope<T> | T>(path);
  return unwrap(response.data);
}

export async function apiSend<T>(
  method: "post" | "put" | "delete",
  path: string,
  payload?: unknown
): Promise<{ data: T; meta: ApiMeta | null }> {
  const response = await client.request<ApiEnvelope<T> | T>({
    method,
    url: path,
    data: payload,
  });

  return unwrap(response.data);
}

export async function getDashboardData(): Promise<DashboardDataEntity> {
  const [status, catalog] = await Promise.allSettled([
    apiGet<BackendStatusEntity>("/api/status"),
    apiGet<CatalogSummaryEntity>("/api/catalog/summary"),
  ]);

  return {
    status: status.status === "fulfilled" ? status.value.data : null,
    catalog: catalog.status === "fulfilled" ? catalog.value.data : null,
    meta: status.status === "fulfilled" ? status.value.meta : null,
  };
}

export async function listInelicom(entity: string): Promise<AdminRecord[]> {
  return (await apiGet<AdminRecord[]>(`/api/inelicom/${entity}?limit=100&pageIndex=0`)).data;
}

export async function createInelicom(entity: string, payload: AdminRecord): Promise<AdminRecord> {
  return (await apiSend<AdminRecord>("post", `/api/inelicom/${entity}`, payload)).data;
}

export async function updateInelicom(entity: string, id: number, payload: AdminRecord): Promise<AdminRecord> {
  return (await apiSend<AdminRecord>("put", `/api/inelicom/${entity}/${id}`, payload)).data;
}

export async function deleteInelicom(entity: string, id: number): Promise<boolean> {
  return (await apiSend<boolean>("delete", `/api/inelicom/${entity}/${id}`)).data;
}

export async function getGuilds(): Promise<GuildSummary[]> {
  return (await apiGet<GuildSummary[]>("/api/bot/servers")).data;
}

export async function getGuildProfile(guildId: number): Promise<GuildProfile> {
  return (await apiGet<GuildProfile>(`/api/bot/servers/${guildId}/profile`)).data;
}

export async function updateGuildProfile(guildId: number, payload: GuildProfile): Promise<GuildProfile> {
  return (await apiSend<GuildProfile>("put", `/api/bot/servers/${guildId}/profile`, payload)).data;
}

export async function getBotUsers(): Promise<BotUser[]> {
  return (await apiGet<BotUser[]>("/api/bot/users")).data;
}

export async function createBotUser(payload: Omit<BotUser, "user_id" | "created_at">): Promise<BotUser> {
  return (await apiSend<BotUser>("post", "/api/bot/users", payload)).data;
}

export async function updateBotUser(userId: number, payload: Omit<BotUser, "user_id" | "created_at">): Promise<BotUser> {
  return (await apiSend<BotUser>("put", `/api/bot/users/${userId}`, payload)).data;
}

export async function deleteBotUser(userId: number): Promise<boolean> {
  return (await apiSend<boolean>("delete", `/api/bot/users/${userId}`)).data;
}

export async function getGuildMembers(guildId: number): Promise<BotMember[]> {
  return (await apiGet<BotMember[]>(`/api/bot/servers/${guildId}/members`)).data;
}

export async function registerGuildMember(guildId: number, payload: RegisterMemberRequest): Promise<unknown> {
  return (await apiSend<unknown>("post", `/api/bot/servers/${guildId}/members`, payload)).data;
}

export async function registerGuildMembers(guildId: number, members: RegisterMemberRequest[]): Promise<unknown> {
  return (await apiSend<unknown>("post", `/api/bot/servers/${guildId}/members/bulk`, { members })).data;
}

export async function getGuildRoles(guildId: number): Promise<BotRole[]> {
  return (await apiGet<BotRole[]>(`/api/bot/servers/${guildId}/roles`)).data;
}

export async function createGuildRole(guildId: number, payload: Partial<BotRole>): Promise<BotRole> {
  return (await apiSend<BotRole>("post", `/api/bot/servers/${guildId}/roles`, payload)).data;
}

export async function updateGuildRole(guildId: number, roleId: number, payload: Partial<BotRole>): Promise<BotRole> {
  return (await apiSend<BotRole>("put", `/api/bot/servers/${guildId}/roles/${roleId}`, payload)).data;
}

export async function deleteGuildRole(guildId: number, roleId: number): Promise<boolean> {
  return (await apiSend<boolean>("delete", `/api/bot/servers/${guildId}/roles/${roleId}`)).data;
}

export async function getGuildChannels(guildId: number): Promise<BotChannel[]> {
  return (await apiGet<BotChannel[]>(`/api/bot/servers/${guildId}/channels`)).data;
}

export async function createGuildChannel(guildId: number, payload: Partial<BotChannel>): Promise<BotChannel> {
  return (await apiSend<BotChannel>("post", `/api/bot/servers/${guildId}/channels`, payload)).data;
}

export async function updateGuildChannel(guildId: number, channelId: number, payload: Partial<BotChannel>): Promise<BotChannel> {
  return (await apiSend<BotChannel>("put", `/api/bot/servers/${guildId}/channels/${channelId}`, payload)).data;
}

export async function deleteGuildChannel(guildId: number, channelId: number): Promise<boolean> {
  return (await apiSend<boolean>("delete", `/api/bot/servers/${guildId}/channels/${channelId}`)).data;
}
