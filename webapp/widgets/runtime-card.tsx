import { Activity } from "lucide-react";

import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import type { RuntimeMetric } from "@/features/dashboard/dashboard-model";

interface RuntimeCardProps {
  metrics: RuntimeMetric[];
  backendOk: boolean;
  databaseOk: boolean;
}

export function RuntimeCard({
  metrics,
  backendOk,
  databaseOk,
}: RuntimeCardProps) {
  const healthScore = [backendOk, databaseOk].filter(Boolean).length * 50;

  return (
    <Card className="rounded-md border-border bg-card shadow-panel">
      <CardHeader className="flex-row items-center justify-between space-y-0">
        <CardTitle className="text-sm">Runtime</CardTitle>
        <Activity className="h-4 w-4 text-success" />
      </CardHeader>
      <CardContent>
        <div className="mb-4 space-y-2">
          <div className="flex items-center justify-between text-xs text-muted-foreground">
            <span>Service health</span>
            <span>{healthScore}%</span>
          </div>
          <div className="h-2 overflow-hidden rounded-full bg-muted">
            <div
              className="h-full rounded-full bg-primary transition-all"
              style={{ width: `${healthScore}%` }}
            />
          </div>
        </div>
        <dl className="space-y-3 text-sm">
          {metrics.map((metric, index) => (
            <div key={metric.label}>
              {index > 0 ? <Separator className="mb-3" /> : null}
              <div className="flex items-center justify-between gap-4">
                <dt className="text-muted-foreground">{metric.label}</dt>
                <dd className="max-w-48 truncate font-medium">
                  {metric.value}
                </dd>
              </div>
            </div>
          ))}
        </dl>
      </CardContent>
    </Card>
  );
}
