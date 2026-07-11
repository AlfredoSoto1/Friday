export interface ContactDto {
  contactId: number;
  name: string;
  email: string;
  phone: string;
  website: string;
  description: string;
  services: string;
  createdAt: string;
}

export interface ContactRequest {
  name: string;
  email: string;
  phone: string;
  website: string;
  description?: string;
  services?: string;
}

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

export interface ProjectDto {
  projectId: number;
  web?: string | null;
  facebook?: string | null;
  instagram?: string | null;
  email?: string | null;
  name: string;
  description: string;
  createdAt: string;
}

export interface OrganizationDto {
  organizationId: number;
  email?: string | null;
  facebook?: string | null;
  instagram?: string | null;
  twitterX?: string | null;
  web?: string | null;
  name: string;
  description: string;
  createdAt: string;
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
  roleId: number;
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

