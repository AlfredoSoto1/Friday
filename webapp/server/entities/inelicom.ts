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

export interface DepartmentDto {
  departmentId: number;
  name: string;
  facultyId: number;
  buildingId: number;
  createdAt: string;
}

export interface DepartmentRequest {
  name: string;
  facultyId: number;
  buildingId: number;
}

export interface RoomDto {
  roomId: number;
  code: string;
  name: string;
  buildingId: number;
  departmentId: number;
  createdAt: string;
}

export interface RoomRequest {
  code: string;
  name: string;
  buildingId: number;
  departmentId: number;
}

export interface GuildRosterStudentRequest {
  email: string;
  firstName: string;
  firstLastName: string;
  secondLastName: string;
  initial: string;
  program: "INEL" | "ICOM" | "INSO" | "CIIC";
}

export interface GuildRosterTeamRequest {
  name: string;
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
