import axios from "axios";

import { EnvelopeResult, keysToSnake } from "@/lib/webservices";
import { getBackendUrl } from "@/server/webservices/backend-url";
import type {
  CreateDiscordServerDto,
  DashboardContentDto,
  DiscordServerDto,
  SetDiscordServerEnabledDto,
} from "@/server/entities/dashboard";

const backendUrl = getBackendUrl();

const client = axios.create({
  baseURL: `${backendUrl}/api/v1/dashboard`,
  timeout: 10000,
});

export class DashboardWebservice {
  static async getDashboard(): Promise<EnvelopeResult<DashboardContentDto>> {
    return client.get("")
      .then((response) => EnvelopeResult.fromObject<DashboardContentDto>(response.data))
      .catch((error: unknown) => EnvelopeResult.fromError<DashboardContentDto>(error));
  }

  static async createServer(
    data: CreateDiscordServerDto
  ): Promise<EnvelopeResult<DiscordServerDto>> {
    return client.post("/servers", keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<DiscordServerDto>(response.data))
      .catch((error: unknown) => EnvelopeResult.fromError<DiscordServerDto>(error));
  }

  static async setServerEnabled(
    serverId: number,
    data: SetDiscordServerEnabledDto
  ): Promise<EnvelopeResult<DiscordServerDto>> {
    return client.patch(`/servers/${serverId}/enabled`, keysToSnake(data))
      .then((response) => EnvelopeResult.fromObject<DiscordServerDto>(response.data))
      .catch((error: unknown) => EnvelopeResult.fromError<DiscordServerDto>(error));
  }

  static async deleteServer(serverId: number): Promise<EnvelopeResult<boolean>> {
    return client.delete(`/servers/${serverId}`)
      .then((response) => EnvelopeResult.fromObject<boolean>(response.data))
      .catch((error: unknown) => EnvelopeResult.fromError<boolean>(error));
  }
}
