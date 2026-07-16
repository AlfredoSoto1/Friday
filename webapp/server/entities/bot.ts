export interface BotRoleDto {
  roleId: number;
  guildId: number;
  discordRoleId?: string | null;
  name: string;
  color?: number | null;
  position?: number | null;
  managed: boolean;
  mentionable: boolean;
  hoisted: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface BotTeamDto {
  teamId: number;
  name: string;
  roleId?: number | null;
  roleIds?: number[];
  roleName?: string | null;
  memberCount: number;
}
