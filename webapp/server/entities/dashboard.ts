export interface DiscordServerDto {
  serverId: number;
  name: string;
  serverCode: string;
  enabled: boolean;
  createdAt: string;
}

export interface DashboardContentDto {
  servers: DiscordServerDto[];
}

export interface CreateDiscordServerDto {
  name: string;
  serverCode: string;
}

export interface SetDiscordServerEnabledDto {
  enabled: boolean;
}

export type {
  ApiMeta,
  BackendStatusEntity,
  CatalogSummaryEntity,
  DashboardDataEntity,
  SchemaSummaryEntity,
  TableSummaryEntity,
} from "@/features/dashboard/admin-types";
