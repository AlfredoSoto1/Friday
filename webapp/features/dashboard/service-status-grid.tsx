import { Bot, BrainCircuit, Database, Server } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardAction,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";

const services = [
  { name: "Backend", detail: "API service", icon: Server },
  { name: "SLM", detail: "Language model service", icon: BrainCircuit },
  { name: "Bot", detail: "Discord bot", icon: Bot },
  { name: "Database", detail: "PostgreSQL", icon: Database },
] as const;

export function ServiceStatusGrid() {
  return (
    <Card className="rounded-md border-border bg-card shadow-panel">
      <CardHeader>
        <CardTitle>Service status</CardTitle>
        <CardDescription>Static placeholder values for the local stack.</CardDescription>
        <CardAction>
          <Badge variant="outline">Placeholder</Badge>
        </CardAction>
      </CardHeader>
      <CardContent className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
        {services.map(({ name, detail, icon: Icon }) => (
          <Card key={name} size="sm" className="rounded-md bg-background/40">
            <CardHeader>
              <CardTitle>{name}</CardTitle>
              <CardDescription>{detail}</CardDescription>
              <CardAction>
                <Icon className="size-4 text-muted-foreground" />
              </CardAction>
            </CardHeader>
            <CardContent>
              <Badge
                variant="outline"
                className="border-success/40 bg-success/10 text-success"
              >
                Operational
              </Badge>
            </CardContent>
          </Card>
        ))}
      </CardContent>
    </Card>
  );
}
