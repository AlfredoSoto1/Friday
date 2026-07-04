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
export type DistributionMode = "balanced" | "randomized" | "manual";

export interface TeamGroup {
  id: number;
  name: string;
  color: string;
  studentIds: number[];
}

export const TEAM_COLOR_SWATCHES = [
  "#5865f2",
  "#3fb950",
  "#d29922",
  "#f85149",
  "#58a6ff",
  "#bc8cff",
  "#f778ba",
  "#39c5cf",
] as const;

export interface TeamCardProps {
  team: TeamGroup;
  members: Student[];
  otherTeams: TeamGroup[];
  editMode: boolean;
  onRename: (teamId: number, name: string) => void;
  onRecolor: (teamId: number, color: string) => void;
  onMoveStudent: (studentId: number, toTeamId: number | null) => void;
}
