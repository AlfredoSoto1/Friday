import type {
  BuildingDto,
  DepartmentDto,
  FacultyDto,
  RoomDto,
} from "@/server/entities/inelicom";

export type ContentKind = "faculty" | "building" | "department" | "room";
export type ContentRecord = FacultyDto | BuildingDto | DepartmentDto | RoomDto;

export interface ContentData {
  faculties: FacultyDto[];
  buildings: BuildingDto[];
  departments: DepartmentDto[];
  rooms: RoomDto[];
}

export interface ContentFormValues {
  name: string;
  gpin: string;
  code: string;
  facultyId: number;
  buildingId: number;
  departmentId: number;
}
