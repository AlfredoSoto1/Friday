export interface ApiMeta {
  timestamp?: string;
  request_id?: string;
  processing_time_ms?: number;
  limit?: number;
  page_index?: number;
  total?: number;
  remaining?: number;
}

export interface ApiEnvelope<T> {
  status: "success" | "error";
  meta?: ApiMeta;
  error?: { code: number; message: string } | null;
  data?: T;
}

export interface BackendStatusEntity {
  status?: string;
  database?: string;
  checked_at?: string;
}

export interface TableSummaryEntity {
  schema: string;
  table: string;
  rows: number;
}

export interface SchemaSummaryEntity {
  name: string;
  tables: TableSummaryEntity[];
}

export interface CatalogSummaryEntity {
  schemas: SchemaSummaryEntity[];
}

export interface DashboardDataEntity {
  status: BackendStatusEntity | null;
  catalog: CatalogSummaryEntity | null;
  meta: ApiMeta | null;
}

export type RecordValue = string | number | boolean | null | undefined;
export type AdminRecord = Record<string, RecordValue>;

export interface GuildSummary {
  guild_id: number;
  name: string;
  enabled: boolean;
  created_at: string;
}

export interface GuildProfile extends GuildSummary {
  theme: {
    primary_color: string;
    thumbnail_url?: string | null;
    footer_text?: string | null;
  };
  verification: {
    title: string;
    description: string;
    button_label: string;
    channel_id?: string | null;
    verified_role_id?: string | null;
    banner_url?: string | null;
  };
  welcome: {
    title: string;
    description: string;
    channel_id?: string | null;
    banner_url?: string | null;
  };
}

export interface BotUser {
  user_id: number;
  email: string;
  fullname: string;
  username: string;
  created_at: string;
}

export interface BotMember {
  server_user_id: number;
  user_id: number;
  email: string;
  fullname: string;
  username: string;
  discord_user_id: string;
  verified: boolean;
  fun_fact?: string | null;
  xp: number;
  level: number;
  role_ids: string[];
  created_at: string;
  updated_at: string;
}

export interface BotRole {
  role_id: number;
  guild_id: number;
  discord_role_id?: string | null;
  name: string;
  color?: number | null;
  position?: number | null;
  managed: boolean;
  mentionable: boolean;
  hoisted: boolean;
  created_at: string;
  updated_at: string;
}

export interface BotChannel {
  channel_id: number;
  guild_id: number;
  discord_channel_id: string;
  parent_channel_id?: string | null;
  name: string;
  type: string;
  position?: number | null;
  topic?: string | null;
  nsfw: boolean;
  created_at: string;
  updated_at: string;
}

export interface RegisterMemberRequest {
  email: string;
  fullname: string;
  username: string;
  discord_user_id: string;
  fun_fact?: string | null;
  discord_role_ids: string[];
}
