export interface ApiEnvelope<T> {
  status: "success" | "error";
  error?: { message: string } | null;
  data?: T;
}

export interface ServerRole {
  role_id: number;
  discord_role_id?: string | null;
  name: string;
  color?: number | null;
}

export interface ServerChannel {
  channel_id: number;
  discord_channel_id: string;
  name: string;
  type: string;
}

export interface ServerMember {
  role_ids: string[];
}

export interface TeamOverview {
  id: number;
  name: string;
  discordRoleId: string;
  color: string;
  channelName: string;
  memberCount: number;
}
