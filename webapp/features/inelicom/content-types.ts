import type { BuildingDto, FacultyDto } from "@/server/entities/inelicom";

export type ContentKind = "faculty" | "building";
export type ContentRecord = FacultyDto | BuildingDto;

export interface ContentData {
  faculties: FacultyDto[];
  buildings: BuildingDto[];
}

export interface ContentFormValues {
  name: string;
  gpin: string;
  facultyId: number;
  buildingId: number;
}
