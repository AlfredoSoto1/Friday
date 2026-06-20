import axios from "axios";

import type {
  BackendStatusEntity,
  CatalogSummaryEntity,
  DashboardDataEntity,
} from "@/server/entities/dashboard";

const backendUrl =
  process.env.BACKEND_URL ??
  process.env.NEXT_PUBLIC_API_BASE_URL ??
  "http://localhost:8080";

const dashboardClient = axios.create({
  baseURL: backendUrl,
  timeout: 5000,
});

async function fetchEntity<T>(path: string): Promise<T | null> {
  try {
    const response = await dashboardClient.get<T>(path);

    return response.data;
  } catch {
    return null;
  }
}

export async function getDashboardData(): Promise<DashboardDataEntity> {
  const [status, catalog] = await Promise.all([
    fetchEntity<BackendStatusEntity>("/api/status"),
    fetchEntity<CatalogSummaryEntity>("/api/catalog/summary"),
  ]);

  return { status, catalog };
}
