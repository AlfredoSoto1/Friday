import { Activity, Bot, BrainCircuit, Database, Github, LayoutDashboard, Server } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Panel } from "@/components/ui/panel";

type BackendStatus = {
  status?: string;
  database?: string;
};

type TableSummary = {
  schema: string;
  table: string;
  rows: number;
};

type SchemaSummary = {
  name: string;
  tables: TableSummary[];
};

async function fetchJson<T>(path: string): Promise<T | null> {
  const backendUrl = process.env.BACKEND_URL ?? "http://localhost:8080";

  try {
    const response = await fetch(`${backendUrl}${path}`, { cache: "no-store" });
    if (!response.ok) {
      return null;
    }

    return (await response.json()) as T;
  } catch {
    return null;
  }
}

export default async function Home() {
  const [status, catalog] = await Promise.all([
    fetchJson<BackendStatus>("/api/status"),
    fetchJson<{ schemas: SchemaSummary[] }>("/api/catalog/summary")
  ]);

  const backendOk = status?.status === "ok";
  const databaseOk = status?.database === "connected";
  const schemas = catalog?.schemas ?? [];
  const tableCount = schemas.reduce((count, schema) => count + schema.tables.length, 0);

  return (
    <main className="min-h-screen">
      <header className="border-b border-border bg-background/80">
        <div className="mx-auto flex max-w-7xl items-center justify-between px-6 py-4">
          <div className="flex items-center gap-3">
            <div className="flex h-9 w-9 items-center justify-center rounded-md bg-discord text-white">
              <Bot className="h-5 w-5" />
            </div>
            <div>
              <h1 className="text-lg font-semibold tracking-normal">Friday</h1>
              <p className="text-sm text-muted">Incoming student operations</p>
            </div>
          </div>
          <div className="flex items-center gap-2 text-sm text-muted">
            <Github className="h-4 w-4" />
            <span>Dashboard</span>
          </div>
        </div>
      </header>

      <section className="mx-auto grid max-w-7xl gap-6 px-6 py-8 lg:grid-cols-[1fr_360px]">
        <div className="space-y-6">
          <div className="flex flex-wrap items-end justify-between gap-4">
            <div>
              <div className="mb-2 flex items-center gap-2 text-sm font-medium text-discord">
                <LayoutDashboard className="h-4 w-4" />
                Control center
              </div>
              <h2 className="text-3xl font-semibold tracking-normal">Services and database</h2>
            </div>
            <Badge tone={backendOk && databaseOk ? "ok" : "warn"}>
              {backendOk && databaseOk ? "online" : "needs attention"}
            </Badge>
          </div>

          <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
            <StatusPanel icon={Server} label="Backend" value={backendOk ? "Online" : "Unavailable"} ok={backendOk} />
            <StatusPanel icon={Database} label="Database" value={databaseOk ? "Connected" : "Unavailable"} ok={databaseOk} />
            <StatusPanel icon={Bot} label="Discord bot" value="Configured by token" ok />
            <StatusPanel icon={BrainCircuit} label="SLM" value="Ollama endpoint" ok />
          </div>

          <Panel className="overflow-hidden">
            <div className="border-b border-border px-4 py-3">
              <h3 className="text-sm font-semibold">Database schemas</h3>
            </div>
            <div className="divide-y divide-border">
              {schemas.length === 0 ? (
                <div className="px-4 py-8 text-sm text-muted">No schema data available.</div>
              ) : (
                schemas.map((schema) => (
                  <div key={schema.name} className="px-4 py-4">
                    <div className="mb-3 flex items-center justify-between gap-3">
                      <h4 className="font-medium">{schema.name}</h4>
                      <Badge>{schema.tables.length} tables</Badge>
                    </div>
                    <div className="grid gap-2 sm:grid-cols-2 xl:grid-cols-3">
                      {schema.tables.map((table) => (
                        <div key={`${schema.name}.${table.table}`} className="rounded-md border border-border bg-background px-3 py-2">
                          <div className="text-sm font-medium">{table.table}</div>
                          <div className="text-xs text-muted">{table.rows} rows</div>
                        </div>
                      ))}
                    </div>
                  </div>
                ))
              )}
            </div>
          </Panel>
        </div>

        <aside className="space-y-4">
          <Panel className="p-4">
            <div className="mb-4 flex items-center justify-between">
              <h3 className="text-sm font-semibold">Runtime</h3>
              <Activity className="h-4 w-4 text-success" />
            </div>
            <dl className="space-y-3 text-sm">
              <Metric label="Backend API" value={process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:8080"} />
              <Metric label="Schemas" value={String(schemas.length)} />
              <Metric label="Tables" value={String(tableCount)} />
              <Metric label="Theme" value="Dark" />
            </dl>
          </Panel>
        </aside>
      </section>
    </main>
  );
}

function StatusPanel({
  icon: Icon,
  label,
  value,
  ok
}: {
  icon: React.ComponentType<{ className?: string }>;
  label: string;
  value: string;
  ok: boolean;
}) {
  return (
    <Panel className="p-4">
      <div className="mb-5 flex items-center justify-between">
        <Icon className="h-5 w-5 text-muted" />
        <Badge tone={ok ? "ok" : "warn"}>{ok ? "ok" : "down"}</Badge>
      </div>
      <div className="text-sm text-muted">{label}</div>
      <div className="mt-1 text-base font-semibold">{value}</div>
    </Panel>
  );
}

function Metric({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex items-center justify-between gap-4 border-b border-border pb-2 last:border-b-0 last:pb-0">
      <dt className="text-muted">{label}</dt>
      <dd className="max-w-48 truncate font-medium">{value}</dd>
    </div>
  );
}
