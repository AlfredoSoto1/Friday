import { EnvelopeResult, keysToSnake, Paged } from "@/lib/webservices";
import axios from "axios";

import type {
  BotMemberDto,
  BotRoleDto,
  BotTeamDto,
} from "@/server/entities/bot";

const backendUrl = process.env.NEXT_PUBLIC_API_BASE_URL ??
  process.env.BACKEND_URL ?? "http://localhost:8080";

const BASE = `${backendUrl}/api/v1/bot`;

export class BotApi {
  static async getEnabledGuilds() : Promise<EnvelopeResult<Paged<any>>> {
    return axios.get(`${BASE}/servers`)
      .then((response) => EnvelopeResult.fromList<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async getGuildProfile(guildId: number) : Promise<EnvelopeResult<any>> {
    return axios.get(`${BASE}/servers/${guildId}/profile`)
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async updateGuildProfile(guildId: number, data: any) {
    return axios.put(`${BASE}/servers/${guildId}/profile`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async getUsers() {
    return axios.get(`${BASE}/users`)
      .then((response) => EnvelopeResult.fromList<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async getGuildMembers(
    guildId: number
  ): Promise<EnvelopeResult<Paged<BotMemberDto>>> {
    return axios.get(`${BASE}/servers/${guildId}/members`)
      .then((response) => (
        EnvelopeResult.fromList<BotMemberDto>(response.data)
      ))
      .catch((error: unknown) => (
        EnvelopeResult.fromError<Paged<BotMemberDto>>(error)
      ));
  }

  static async getGuildRoles(
    guildId: string | number
  ): Promise<EnvelopeResult<Paged<BotRoleDto>>> {
    return axios.get(`${BASE}/servers/${guildId}/roles`)
      .then((response) => (
        EnvelopeResult.fromList<BotRoleDto>(response.data)
      ))
      .catch((error: unknown) => (
        EnvelopeResult.fromError<Paged<BotRoleDto>>(error)
      ));
  }

  static async getGuildTeams(
    guildId: string | number
  ): Promise<EnvelopeResult<Paged<BotTeamDto>>> {
    return axios.get(`${BASE}/servers/${guildId}/teams`)
      .then((response) => (
        EnvelopeResult.fromList<BotTeamDto>(response.data)
      ))
      .catch((error: unknown) => (
        EnvelopeResult.fromError<Paged<BotTeamDto>>(error)
      ));
  }

  static async getCommandResponse(guildId: number, commandName: string) {
    return axios.get(`${BASE}/servers/${guildId}/commands/${commandName}`)
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async verifyMember(guildId: number, data: any) {
    return axios.post(`${BASE}/servers/${guildId}/members/verify`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async addXp(guildId: number, data: any) {
    return axios.post(`${BASE}/servers/${guildId}/members/xp`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }

  static async syncGuild(guildId: number, data: any) {
    return axios.post(`${BASE}/servers/${guildId}/sync`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<any>(response.data))
      .catch((error) => EnvelopeResult.fromError<any>(error));
  }
}
