"use client";

import { Bot, Database, Server } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { getDashboardModel, type HealthTone } from "@/features/dashboard/dashboard-model";
import { cn } from "@/lib/utils";
import type { DashboardDataEntity } from "@/server/entities/dashboard";
import { RuntimeCard } from "@/widgets/runtime-card";
import { SchemaCatalog } from "@/widgets/schema-catalog";
import { StatusCard } from "@/widgets/status-card";

const statusIcons = [Server, Database, Bot, Database];

export function OverviewPanel({ data }: { data: DashboardDataEntity }) {
  const model = getDashboardModel(data);

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <div className="mb-2 flex items-center gap-2 text-sm font-medium text-discord">
            <Server className="h-4 w-4" />
            Control center
          </div>
          <h2 className="text-3xl font-semibold tracking-normal">Services and database</h2>
        </div>
        <HealthBadge tone={model.healthTone}>{model.healthLabel}</HealthBadge>
      </div>

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        {model.statuses.map((status, index) => (
          <StatusCard key={status.label} icon={statusIcons[index]} label={status.label} value={status.value} ok={status.ok} />
        ))}
      </div>

      <div className="grid gap-6 lg:grid-cols-[1fr_360px]">
        <SchemaCatalog schemas={model.schemas} />
        <RuntimeCard metrics={model.runtimeMetrics} backendOk={model.backendOk} databaseOk={model.databaseOk} />
      </div>
    </div>
  );
}

function HealthBadge({ tone, children }: { tone: HealthTone; children: React.ReactNode }) {
  return (
    <Badge
      variant="outline"
      className={cn(
        "h-6 rounded-md px-2",
        tone === "ok" ? "border-success/40 bg-success/10 text-success" : "border-warning/40 bg-warning/10 text-warning"
      )}
    >
      {children}
    </Badge>
  );
}
