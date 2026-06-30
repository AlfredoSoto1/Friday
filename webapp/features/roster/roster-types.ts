export interface Student {
  id: number;
  name: string;
  studentId: string;
  major: string;
  year: "Freshman" | "Sophomore" | "Junior" | "Senior";
  gpa: number;
}

export interface RosterFile {
  name: string;
  size: number;
  type: string;
}

export type SortField = "name" | "studentId" | "major" | "year" | "gpa";
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
