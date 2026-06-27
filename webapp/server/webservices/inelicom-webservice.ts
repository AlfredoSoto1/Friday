import { EnvelopeResult, keysToSnake, Paged } from "@/lib/webservices";
import axios from "axios";

import type {
  BuildingDto,
  BuildingRequest,
  DepartmentDto,
  DepartmentRequest,
  FacultyDto,
  FacultyRequest,
  RoomDto,
  RoomRequest,
} from "@/server/entities/inelicom";

const backendUrl = process.env.NEXT_PUBLIC_API_BASE_URL ??
  process.env.BACKEND_URL ?? "http://localhost:8080";

const BASE = `${backendUrl}/api/v1/inelicom`;

export class InelicomApi {
  static async getBuildings(): Promise<EnvelopeResult<Paged<BuildingDto>>> {
    return axios.get(`${BASE}/buildings`)
      .then((response) => EnvelopeResult.fromList<BuildingDto>(response.data))
      .catch((error: unknown) => (
        EnvelopeResult.fromError<Paged<BuildingDto>>(error)
      ));
  }

  static async getBuilding(buildingId: number) : Promise<EnvelopeResult<any>> {
    return axios.get(`${BASE}/buildings/${buildingId}`)
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async createBuilding(
    data: BuildingRequest
  ): Promise<EnvelopeResult<BuildingDto>> {
    return axios.post(`${BASE}/buildings`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<BuildingDto>(response.data))
      .catch((error: unknown) => EnvelopeResult.fromError<BuildingDto>(error));
  }

  static async updateBuilding(
    buildingId: number,
    data: BuildingRequest
  ): Promise<EnvelopeResult<BuildingDto>> {
    return axios.put(`${BASE}/buildings/${buildingId}`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<BuildingDto>(response.data))
      .catch((error: unknown) => EnvelopeResult.fromError<BuildingDto>(error));
  }

  static async deleteBuilding(
    buildingId: number
  ): Promise<EnvelopeResult<boolean>> {
    return axios.delete(`${BASE}/buildings/${buildingId}`)
      .then((response) => EnvelopeResult.fromObject<boolean>(response.data))
      .catch((error: unknown) => EnvelopeResult.fromError<boolean>(error));
  }

  static async getContacts() : Promise<EnvelopeResult<Paged<any>>> {
    return axios.get(`${BASE}/contacts`)
      .then((response) => EnvelopeResult.fromList<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async getContact(contactId: number) : Promise<EnvelopeResult<any>> {
    return axios.get(`${BASE}/contacts/${contactId}`)
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async createContact(data: any) : Promise<EnvelopeResult<any>> {
    return axios.post(`${BASE}/contacts`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async updateContact(contactId: number, data: any) : Promise<EnvelopeResult<any>> {
    return axios.put(`${BASE}/contacts/${contactId}`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async deleteContact(contactId: number) : Promise<EnvelopeResult<any>> {
    return axios.delete(`${BASE}/contacts/${contactId}`)
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async getDepartments(): Promise<EnvelopeResult<Paged<DepartmentDto>>> {
    return axios.get(`${BASE}/departments`)
      .then((response) => EnvelopeResult.fromList<DepartmentDto>(response.data))
      .catch((error: unknown) => (
        EnvelopeResult.fromError<Paged<DepartmentDto>>(error)
      ));
  }

  static async getDepartment(departmentId: number) : Promise<EnvelopeResult<any>> {
    return axios.get(`${BASE}/departments/${departmentId}`)
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async createDepartment(
    data: DepartmentRequest
  ): Promise<EnvelopeResult<DepartmentDto>> {
    return axios.post(`${BASE}/departments`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<DepartmentDto>(response.data))
      .catch((error: unknown) => EnvelopeResult.fromError<DepartmentDto>(error));
  }

  static async updateDepartment(
    departmentId: number,
    data: DepartmentRequest
  ): Promise<EnvelopeResult<DepartmentDto>> {
    return axios.put(`${BASE}/departments/${departmentId}`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<DepartmentDto>(response.data))
      .catch((error: unknown) => EnvelopeResult.fromError<DepartmentDto>(error));
  }

  static async deleteDepartment(
    departmentId: number
  ): Promise<EnvelopeResult<boolean>> {
    return axios.delete(`${BASE}/departments/${departmentId}`)
      .then((response) => EnvelopeResult.fromObject<boolean>(response.data))
      .catch((error: unknown) => EnvelopeResult.fromError<boolean>(error));
  }

  static async getFaculties(): Promise<EnvelopeResult<Paged<FacultyDto>>> {
    return axios.get(`${BASE}/faculties`)
      .then((response) => EnvelopeResult.fromList<FacultyDto>(response.data))
      .catch((error: unknown) => (
        EnvelopeResult.fromError<Paged<FacultyDto>>(error)
      ));
  }

  static async getFaculty(facultyId: number) : Promise<EnvelopeResult<any>> {
    return axios.get(`${BASE}/faculties/${facultyId}`)
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async createFaculty(
    data: FacultyRequest
  ): Promise<EnvelopeResult<FacultyDto>> {
    return axios.post(`${BASE}/faculties`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<FacultyDto>(response.data))
      .catch((error: unknown) => EnvelopeResult.fromError<FacultyDto>(error));
  }

  static async updateFaculty(
    facultyId: number,
    data: FacultyRequest
  ): Promise<EnvelopeResult<FacultyDto>> {
    return axios.put(`${BASE}/faculties/${facultyId}`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<FacultyDto>(response.data))
      .catch((error: unknown) => EnvelopeResult.fromError<FacultyDto>(error));
  }

  static async deleteFaculty(
    facultyId: number
  ): Promise<EnvelopeResult<boolean>> {
    return axios.delete(`${BASE}/faculties/${facultyId}`)
      .then((response) => EnvelopeResult.fromObject<boolean>(response.data))
      .catch((error: unknown) => EnvelopeResult.fromError<boolean>(error));
  }

  static async getOrganizations() : Promise<EnvelopeResult<Paged<any>>> {
    return axios.get(`${BASE}/organizations`)
      .then((response) => EnvelopeResult.fromList<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async getOrganization(organizationId: number) : Promise<EnvelopeResult<any>> {
    return axios.get(`${BASE}/organizations/${organizationId}`)
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async createOrganization(data: any) : Promise<EnvelopeResult<any>> {
    return axios.post(`${BASE}/organizations`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async updateOrganization(organizationId: number, data: any) : Promise<EnvelopeResult<any>> {
    return axios.put(`${BASE}/organizations/${organizationId}`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async deleteOrganization(organizationId: number) : Promise<EnvelopeResult<any>> {
    return axios.delete(`${BASE}/organizations/${organizationId}`)
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async getProjects() : Promise<EnvelopeResult<Paged<any>>> {
    return axios.get(`${BASE}/projects`)
      .then((response) => EnvelopeResult.fromList<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async getProject(projectId: number) : Promise<EnvelopeResult<any>> {
    return axios.get(`${BASE}/projects/${projectId}`)
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async createProject(data: any) : Promise<EnvelopeResult<any>> {
    return axios.post(`${BASE}/projects`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async updateProject(projectId: number, data: any) : Promise<EnvelopeResult<any>> {
    return axios.put(`${BASE}/projects/${projectId}`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async deleteProject(projectId: number) : Promise<EnvelopeResult<any>> {
    return axios.delete(`${BASE}/projects/${projectId}`)
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async getRooms(): Promise<EnvelopeResult<Paged<RoomDto>>> {
    return axios.get(`${BASE}/rooms`)
      .then((response) => EnvelopeResult.fromList<RoomDto>(response.data))
      .catch((error: unknown) => (
        EnvelopeResult.fromError<Paged<RoomDto>>(error)
      ));
  }

  static async getRoom(roomId: number) : Promise<EnvelopeResult<any>> {
    return axios.get(`${BASE}/rooms/${roomId}`)
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async createRoom(
    data: RoomRequest
  ): Promise<EnvelopeResult<RoomDto>> {
    return axios.post(`${BASE}/rooms`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<RoomDto>(response.data))
      .catch((error: unknown) => EnvelopeResult.fromError<RoomDto>(error));
  }

  static async updateRoom(
    roomId: number,
    data: RoomRequest
  ): Promise<EnvelopeResult<RoomDto>> {
    return axios.put(`${BASE}/rooms/${roomId}`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<RoomDto>(response.data))
      .catch((error: unknown) => EnvelopeResult.fromError<RoomDto>(error));
  }

  static async deleteRoom(
    roomId: number
  ): Promise<EnvelopeResult<boolean>> {
    return axios.delete(`${BASE}/rooms/${roomId}`)
      .then((response) => EnvelopeResult.fromObject<boolean>(response.data))
      .catch((error: unknown) => EnvelopeResult.fromError<boolean>(error));
  }
}
