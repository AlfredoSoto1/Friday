import {
  Bot,
  BrainCircuit,
  Database,
  Github,
  LayoutDashboard,
  Server,
} from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  getDashboardModel,
  type HealthTone,
} from "@/features/dashboard/dashboard-model";
import { cn } from "@/lib/utils";
import type { DashboardDataEntity } from "@/server/entities/dashboard";
import { RuntimeCard } from "@/widgets/runtime-card";
import { SchemaCatalog } from "@/widgets/schema-catalog";
import { StatusCard } from "@/widgets/status-card";

const statusIcons = [Server, Database, Bot, BrainCircuit];

interface DashboardViewProps {
  data: DashboardDataEntity;
}

export function DashboardView({ data }: DashboardViewProps) {
  const model = getDashboardModel(data);

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
              <p className="text-sm text-muted-foreground">
                Incoming student operations
              </p>
            </div>
          </div>
          <Button
            variant="ghost"
            size="sm"
            className="gap-2 text-muted-foreground hover:bg-muted hover:text-foreground"
          >
            <Github className="h-4 w-4" />
            Dashboard
          </Button>
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
              <h2 className="text-3xl font-semibold tracking-normal">
                Services and database
              </h2>
            </div>
            <HealthBadge tone={model.healthTone}>{model.healthLabel}</HealthBadge>
          </div>

          <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
            {model.statuses.map((status, index) => (
              <StatusCard
                key={status.label}
                icon={statusIcons[index]}
                label={status.label}
                value={status.value}
                ok={status.ok}
              />
            ))}
          </div>

          <SchemaCatalog schemas={model.schemas} />
        </div>

        <aside className="space-y-4">
          <RuntimeCard
            metrics={model.runtimeMetrics}
            backendOk={model.backendOk}
            databaseOk={model.databaseOk}
          />
        </aside>
      </section>
    </main>
  );
}

function HealthBadge({
  tone,
  children,
}: {
  tone: HealthTone;
  children: React.ReactNode;
}) {
  return (
    <Badge
      variant="outline"
      className={cn(
        "h-6 rounded-md px-2",
        tone === "ok"
          ? "border-success/40 bg-success/10 text-success"
          : "border-warning/40 bg-warning/10 text-warning"
      )}
    >
      {children}
    </Badge>
  );
}
