export type DiscordServerDepartmentProfile = "INEL_ICOM" | "INSO_CIIC";

export interface DiscordServerDto {
  serverId: number;
  name: string;
  serverCode: string;
  departmentProfile: DiscordServerDepartmentProfile;
  enabled: boolean;
  createdAt: string;
}

export interface DashboardContentDto {
  servers: DiscordServerDto[];
}

export interface CreateDiscordServerDto {
  name: string;
  serverCode: string;
  departmentProfile: DiscordServerDepartmentProfile;
}

export interface SetDiscordServerEnabledDto {
  enabled: boolean;
}
