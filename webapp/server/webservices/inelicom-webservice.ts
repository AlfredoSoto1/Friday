import axios from "axios";

import { EnvelopeResult, keysToSnake, Paged } from "@/lib/webservices";
import type {
  BuildingDto,
  BuildingRequest,
  ContactDto,
  ContactRequest,
  CsvImportResultDto,
  FacultyDto,
  FacultyRequest,
  InelicomCsvImportKind,
  OrganizationDto,
  ProjectDto,
  SaveGuildRosterRequest,
  SaveGuildRosterResult,
} from "@/server/entities/inelicom";

const backendUrl = process.env.NEXT_PUBLIC_API_BASE_URL
  ?? process.env.BACKEND_URL
  ?? "http://localhost:8080";
const BASE = `${backendUrl}/api/v1/inelicom`;

export class InelicomApi {
  static async getContacts(): Promise<EnvelopeResult<Paged<ContactDto>>> {
    return axios.get(`${BASE}/contacts`)
      .then((response) => EnvelopeResult.fromList<ContactDto>(response.data))
      .catch((error: unknown) => (
        EnvelopeResult.fromError<Paged<ContactDto>>(error)
      ));
  }

  static async createContact(
    data: ContactRequest
  ): Promise<EnvelopeResult<ContactDto>> {
    return axios.post(`${BASE}/contacts`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<ContactDto>(response.data))
      .catch((error: unknown) => EnvelopeResult.fromError<ContactDto>(error));
  }

  static async updateContact(
    contactId: number,
    data: ContactRequest
  ): Promise<EnvelopeResult<ContactDto>> {
    return axios.put(`${BASE}/contacts/${contactId}`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<ContactDto>(response.data))
      .catch((error: unknown) => EnvelopeResult.fromError<ContactDto>(error));
  }

  static async deleteContact(
    contactId: number
  ): Promise<EnvelopeResult<boolean>> {
    return axios.delete(`${BASE}/contacts/${contactId}`)
      .then((response) => EnvelopeResult.fromObject<boolean>(response.data))
      .catch((error: unknown) => EnvelopeResult.fromError<boolean>(error));
  }

  static async getBuildings(): Promise<EnvelopeResult<Paged<BuildingDto>>> {
    return axios.get(`${BASE}/buildings`)
      .then((response) => EnvelopeResult.fromList<BuildingDto>(response.data))
      .catch((error: unknown) => (
        EnvelopeResult.fromError<Paged<BuildingDto>>(error)
      ));
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

  static async getFaculties(): Promise<EnvelopeResult<Paged<FacultyDto>>> {
    return axios.get(`${BASE}/faculties`)
      .then((response) => EnvelopeResult.fromList<FacultyDto>(response.data))
      .catch((error: unknown) => (
        EnvelopeResult.fromError<Paged<FacultyDto>>(error)
      ));
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


  static async getProjects(): Promise<EnvelopeResult<Paged<ProjectDto>>> {
    return axios.get(`${BASE}/projects`)
      .then((response) => EnvelopeResult.fromList<ProjectDto>(response.data))
      .catch((error: unknown) => (
        EnvelopeResult.fromError<Paged<ProjectDto>>(error)
      ));
  }

  static async getOrganizations(): Promise<EnvelopeResult<Paged<OrganizationDto>>> {
    return axios.get(`${BASE}/organizations`)
      .then((response) => EnvelopeResult.fromList<OrganizationDto>(response.data))
      .catch((error: unknown) => (
        EnvelopeResult.fromError<Paged<OrganizationDto>>(error)
      ));
  }

  static async importCsv(
    kind: InelicomCsvImportKind,
    file: File,
    append = false
  ): Promise<EnvelopeResult<CsvImportResultDto>> {
    const formData = new FormData();
    formData.append("file", file);
    formData.append("append", String(append));

    return axios.post(`${BASE}/imports/${kind}`, formData)
      .then((response) => (
        EnvelopeResult.fromObject<CsvImportResultDto>(response.data)
      ))
      .catch((error: unknown) => (
        EnvelopeResult.fromError<CsvImportResultDto>(error)
      ));
  }

  static async saveGuildRoster(
    guildId: string,
    data: SaveGuildRosterRequest
  ): Promise<EnvelopeResult<SaveGuildRosterResult>> {
    return axios.put(
      `${backendUrl}/api/v1/bot/servers/${guildId}/roster`,
      keysToSnake(data)
    )
      .then((response) => (
        EnvelopeResult.fromObject<SaveGuildRosterResult>(response.data)
      ))
      .catch((error: unknown) => (
        EnvelopeResult.fromError<SaveGuildRosterResult>(error)
      ));
  }
}
