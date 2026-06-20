import type { ComponentType } from "react";

import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { cn } from "@/lib/utils";

interface StatusCardProps {
  icon: ComponentType<{ className?: string }>;
  label: string;
  value: string;
  ok: boolean;
}

export function StatusCard({ icon: Icon, label, value, ok }: StatusCardProps) {
  return (
    <Card className="rounded-md border-border bg-card shadow-panel">
      <CardHeader className="flex-row items-center justify-between space-y-0">
        <Icon className="h-5 w-5 text-muted-foreground" />
        <Badge
          variant="outline"
          className={cn(
            "h-6 rounded-md px-2",
            ok
              ? "border-success/40 bg-success/10 text-success"
              : "border-warning/40 bg-warning/10 text-warning"
          )}
        >
          {ok ? "ok" : "down"}
        </Badge>
      </CardHeader>
      <CardContent>
        <div className="text-sm text-muted-foreground">{label}</div>
        <CardTitle className="mt-1 text-base">{value}</CardTitle>
      </CardContent>
    </Card>
  );
}
