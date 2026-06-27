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

export interface BotChannelDto {
  channelId: number;
  guildId: number;
  discordChannelId: string;
  parentChannelId?: string | null;
  name: string;
  type: string;
  position?: number | null;
  topic?: string | null;
  nsfw: boolean;
  createdAt: string;
  updatedAt: string;
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
