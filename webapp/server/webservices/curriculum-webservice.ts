import axios from "axios";

import { EnvelopeResult, Paged } from "@/lib/webservices";
import type { CurriculumDto } from "@/server/entities/curriculum";

const backendUrl = process.env.NEXT_PUBLIC_API_BASE_URL ??
  process.env.BACKEND_URL ?? "http://localhost:8080";

const BASE = `${backendUrl}/api/v1/inelicom/curriculums`;

export class CurriculumApi {
  static fileUrl(program: string): string {
    return `${BASE}/${encodeURIComponent(program)}/file`;
  }

  static async getCurriculums(): Promise<EnvelopeResult<Paged<CurriculumDto>>> {
    return axios.get(BASE)
      .then((response) => EnvelopeResult.fromList<CurriculumDto>(response.data))
      .catch((error: unknown) => (
        EnvelopeResult.fromError<Paged<CurriculumDto>>(error)
      ));
  }

  static async uploadCurriculum(
    program: string,
    file: File
  ): Promise<EnvelopeResult<CurriculumDto>> {
    const formData = new FormData();
    formData.append("file", file);

    return axios.put(`${BASE}/${encodeURIComponent(program)}`, formData, {
      headers: { "Content-Type": "multipart/form-data" },
    })
      .then((response) => EnvelopeResult.fromObject<CurriculumDto>(response.data))
      .catch((error: unknown) => EnvelopeResult.fromError<CurriculumDto>(error));
  }

  static async deleteCurriculum(program: string): Promise<EnvelopeResult<boolean>> {
    return axios.delete(`${BASE}/${encodeURIComponent(program)}`)
      .then((response) => EnvelopeResult.fromObject<boolean>(response.data))
      .catch((error: unknown) => EnvelopeResult.fromError<boolean>(error));
  }
}
