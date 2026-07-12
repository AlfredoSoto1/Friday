import type { BotRoleDto, BotTeamDto } from "@/server/entities/bot";
export interface Student {
  id: number;
  name: string;
  firstName: string;
  firstLastName: string;
  secondLastName: string;
  initial: string;
  personalEmail: string;
  institutionalEmail: string;
  program: "INEL" | "ICOM" | "INSO" | "CIIC";
}

export interface RosterFile {
  name: string;
  size: number;
  type: string;
}

export type SortField = "firstName" | "firstLastName" | "secondLastName";
export type SortDirection = "asc" | "desc";
export type DistributionMode = "balanced" | "randomized";

export interface TeamGroup {
  id: number;
  name: string;
  color: string;
  roleId: number | null;
  existingTeamId: number | null;
  appendMembers: boolean;
  createNewTeam: boolean;
  studentIds: number[];
}

export interface TeamCardProps {
  team: TeamGroup;
  members: Student[];
  otherTeams: TeamGroup[];
  roles: BotRoleDto[];
  existingTeams: BotTeamDto[];
  editMode: boolean;
  onRename: (teamId: number, name: string) => void;
  onRoleChange: (teamId: number, roleId: number) => void;
  onExistingTeamChange: (teamId: number, existingTeamId: number | null) => void;
  onCreateNewTeamChange: (teamId: number, createNewTeam: boolean) => void;
  onAppendMembersChange: (teamId: number, appendMembers: boolean) => void;
  onConfigurationOpen: () => void;
  onMoveStudent: (studentId: number, toTeamId: number | null) => void;
}
