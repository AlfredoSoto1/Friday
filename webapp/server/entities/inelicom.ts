export interface FacultyDto {
  facultyId: number;
  name: string;
  createdAt: string;
}

export interface FacultyRequest {
  name: string;
}

export interface BuildingDto {
  buildingId: number;
  name: string;
  gpin: string;
  createdAt: string;
}

export interface BuildingRequest {
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
