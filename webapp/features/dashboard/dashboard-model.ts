import type { DashboardDataEntity } from "@/server/entities/dashboard";

export type HealthTone = "ok" | "warn";

export interface StatusSummary {
  label: string;
  value: string;
  ok: boolean;
}

export interface RuntimeMetric {
  label: string;
  value: string;
}

export function getDashboardModel(data: DashboardDataEntity) {
  const schemas = data.catalog?.schemas ?? [];
  const tableCount = schemas.reduce(
    (count, schema) => count + schema.tables.length,
    0
  );
  const rowCount = schemas.reduce(
    (count, schema) =>
      count + schema.tables.reduce((rows, table) => rows + table.rows, 0),
    0
  );
  const backendOk = data.status?.status === "ok";
  const databaseOk = data.status?.database === "connected";

  return {
    backendOk,
    databaseOk,
    schemas,
    tableCount,
    rowCount,
    healthTone: backendOk && databaseOk ? ("ok" as const) : ("warn" as const),
    healthLabel: backendOk && databaseOk ? "online" : "needs attention",
    statuses: [
      {
        label: "Backend",
        value: backendOk ? "Online" : "Unavailable",
        ok: backendOk,
      },
      {
        label: "Database",
        value: databaseOk ? "Connected" : "Unavailable",
        ok: databaseOk,
      },
      {
        label: "Discord bot",
        value: "Configured by token",
        ok: true,
      },
      {
        label: "SLM",
        value: "Ollama endpoint",
        ok: true,
      },
    ] satisfies StatusSummary[],
    runtimeMetrics: [
      {
        label: "Backend API",
        value: process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:8080",
      },
      {
        label: "Schemas",
        value: String(schemas.length),
      },
      {
        label: "Tables",
        value: String(tableCount),
      },
      {
        label: "Rows",
        value: rowCount.toLocaleString("en-US"),
      },
      {
        label: "Theme",
        value: "Dark",
      },
    ] satisfies RuntimeMetric[],
  };
}
