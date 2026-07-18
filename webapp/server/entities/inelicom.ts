export interface FacultyDto {
  facultyId: number;
  name: string;
  extension?: string | null;
  web?: string | null;
  phone?: string | null;
  facebook?: string | null;
  email?: string | null;
  office?: string | null;
  jobEntitlement?: string | null;
  description?: string | null;
  abbreviation?: string | null;
  instagram?: string | null;
  createdAt: string;
}

export interface FacultyRequest {
  name: string;
  extension?: string | null;
  web?: string | null;
  phone?: string | null;
  facebook?: string | null;
  email?: string | null;
  office?: string | null;
  jobEntitlement?: string | null;
  description?: string | null;
  abbreviation?: string | null;
  instagram?: string | null;
}

export interface BuildingDto {
  buildingId: number;
  code?: string | null;
  name: string;
  gpin: string;
  createdAt: string;
}

export interface BuildingRequest {
  code?: string | null;
  name: string;
  gpin: string;
}

export interface GuildRosterStudentRequest {
  email: string;
  firstName: string;
  firstLastName: string;
  secondLastName: string;
  initial: string;
}

export interface GuildRosterTeamRequest {
  teamId?: number;
  name: string;
  roleIds: number[];
  appendMembers: boolean;
  students: GuildRosterStudentRequest[];
}

export interface SaveGuildRosterRequest {
  teams: GuildRosterTeamRequest[];
}

export interface SaveGuildRosterResult {
  studentCount: number;
  teamCount: number;
}

export type InelicomCsvImportKind =
  | "buildings"
  | "contacts"
  | "faculties"
  | "projects"
  | "organizations";

export interface CsvImportResultDto {
  kind: InelicomCsvImportKind;
  fileName: string;
  inserted: number;
  updated: number;
  skipped: number;
  errors: string[];
}
