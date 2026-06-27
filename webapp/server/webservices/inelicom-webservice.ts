import { EnvelopeResult, keysToSnake, Paged } from "@/lib/webservices";
import axios from "axios";

const backendUrl = process.env.NEXT_PUBLIC_API_BASE_URL ??
  process.env.BACKEND_URL ?? "http://localhost:8080";

const BASE = `${backendUrl}/api/v1/inelicom`;

export class InelicomApi {
  static async getBuildings() : Promise<EnvelopeResult<Paged<any>>> {
    return axios.get(`${BASE}/buildings`)
      .then((response) => EnvelopeResult.fromList<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async getBuilding(buildingId: number) : Promise<EnvelopeResult<any>> {
    return axios.get(`${BASE}/buildings/${buildingId}`)
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async createBuilding(data: any) : Promise<EnvelopeResult<any>> {
    return axios.post(`${BASE}/buildings`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async updateBuilding(buildingId: number, data: any) : Promise<EnvelopeResult<any>> {
    return axios.put(`${BASE}/buildings/${buildingId}`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async deleteBuilding(buildingId: number) : Promise<EnvelopeResult<any>> {
    return axios.delete(`${BASE}/buildings/${buildingId}`)
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
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

  static async getGetDepartments() : Promise<EnvelopeResult<Paged<any>>> {
    return axios.get(`${BASE}/departments`)
      .then((response) => EnvelopeResult.fromList<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async getDepartment(departmentId: number) : Promise<EnvelopeResult<any>> {
    return axios.get(`${BASE}/departments/${departmentId}`)
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async createDepartment(data: any) : Promise<EnvelopeResult<any>> {
    return axios.post(`${BASE}/departments`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async updateDepartment(departmentId: number, data: any) : Promise<EnvelopeResult<any>> {
    return axios.put(`${BASE}/departments/${departmentId}`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async deleteDepartment(departmentId: number) : Promise<EnvelopeResult<any>> {
    return axios.delete(`${BASE}/departments/${departmentId}`)
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async getFaculties() : Promise<EnvelopeResult<Paged<any>>> {
    return axios.get(`${BASE}/faculties`)
      .then((response) => EnvelopeResult.fromList<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async getFaculty(facultyId: number) : Promise<EnvelopeResult<any>> {
    return axios.get(`${BASE}/faculties/${facultyId}`)
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async createFaculty(data: any) : Promise<EnvelopeResult<any>> {
    return axios.post(`${BASE}/faculties`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async updateFaculty(facultyId: number, data: any) : Promise<EnvelopeResult<any>> {
    return axios.put(`${BASE}/faculties/${facultyId}`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async deleteFaculty(facultyId: number) : Promise<EnvelopeResult<any>> {
    return axios.delete(`${BASE}/faculties/${facultyId}`)
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
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

  static async getRooms() : Promise<EnvelopeResult<Paged<any>>> {
    return axios.get(`${BASE}/rooms`)
      .then((response) => EnvelopeResult.fromList<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async getRoom(roomId: number) : Promise<EnvelopeResult<any>> {
    return axios.get(`${BASE}/rooms/${roomId}`)
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async createRoom(data: any) : Promise<EnvelopeResult<any>> {
    return axios.post(`${BASE}/rooms`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async updateRoom(roomId: number, data: any) : Promise<EnvelopeResult<any>> {
    return axios.put(`${BASE}/rooms/${roomId}`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async deleteRoom(roomId: number) : Promise<EnvelopeResult<any>> {
    return axios.delete(`${BASE}/rooms/${roomId}`)
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }
}
