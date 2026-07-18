import { EnvelopeResult, Paged } from "@/lib/webservices";
import axios from "axios";

import type { BotRoleDto, BotTeamDto } from "@/server/entities/bot";
import { getBackendUrl } from "@/server/webservices/backend-url";

const backendUrl = getBackendUrl();

const BASE = `${backendUrl}/api/v1/bot`;

export class BotApi {
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
}
