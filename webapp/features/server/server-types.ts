export interface TeamOverview {
  id: number;
  name: string;
  discordRoleId: string;
  color: string;
  channelName: string;
  memberCount: number;
}

export interface StoredUpload {
  name: string;
  type: string;
  dataUrl: string;
}
