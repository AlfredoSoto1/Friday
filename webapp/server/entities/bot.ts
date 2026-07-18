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
  position: number;
  name: string;
  memberCount: number;
}

export interface PrepaTeamExportRowDto {
  teamId: number;
  position: number;
  teamName: string;
  serverUserId?: number | null;
  firstName?: string | null;
  firstLastName?: string | null;
  secondLastName?: string | null;
  initial?: string | null;
  email?: string | null;
  program?: string | null;
}
