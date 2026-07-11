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
  roleName?: string | null;
  memberCount: number;
}

export interface BotMemberDto {
  serverUserId: number;
  userId: number;
  email: string;
  fullname: string;
  username: string;
  discordUserId: string;
  verified: boolean;
  funFact?: string | null;
  xp: number;
  level: number;
  roleIds: string[];
  createdAt: string;
  updatedAt: string;
}
