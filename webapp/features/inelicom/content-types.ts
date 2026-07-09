import type {
  BuildingDto,
  DepartmentDto,
  FacultyDto,
} from "@/server/entities/inelicom";

export type ContentKind = "faculty" | "building" | "department";
export type ContentRecord = FacultyDto | BuildingDto | DepartmentDto;

export interface ContentData {
  faculties: FacultyDto[];
  buildings: BuildingDto[];
  departments: DepartmentDto[];
}

export interface ContentFormValues {
  name: string;
  gpin: string;
  facultyId: number;
  buildingId: number;
}
