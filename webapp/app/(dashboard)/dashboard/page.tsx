import { Bot } from "lucide-react";

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Item, ItemContent, ItemMedia, ItemTitle } from "@/components/ui/item";
import { DiscordServerManager } from "@/features/dashboard/discord-server-manager";
import { ServiceStatusGrid } from "@/features/dashboard/service-status-grid";
import { DashboardWebservice } from "@/server/webservices/dashboard-webservice";

export const dynamic = "force-dynamic";

export default async function DashboardPage() {
  const result = await DashboardWebservice.getDashboard();
  const servers = result.isSuccess ? result.value.servers : [];
  const error = result.isFailure ? result.error.message : undefined;

  return (
    <Card className="min-h-screen rounded-none bg-transparent py-0 ring-0">
      <CardHeader className="border-b border-border bg-background/80 px-6 py-4">
        <div className="mx-auto flex w-full max-w-7xl items-center justify-between gap-4 px-4 sm:px-6">
            <div className="flex min-w-0 items-center gap-4">
            <ItemMedia
              variant="icon"
              className="size-9 rounded-md bg-discord text-white"
            >
              <Bot className="size-5" />
            </ItemMedia>
            <ItemContent>
              <ItemTitle className="text-lg">Friday</ItemTitle>
              <CardDescription>Incoming student operations</CardDescription>
            </ItemContent>
          </div>
        </div>
      </CardHeader>
      <CardContent className="mx-auto grid w-full max-w-7xl gap-6 px-6 py-8">
        <Card className="rounded-md border-border bg-card shadow-panel">
          <CardHeader>
            <CardTitle className="text-3xl">Dashboard</CardTitle>
            <CardDescription>
              Manage Discord servers and review the local service stack.
            </CardDescription>
          </CardHeader>
        </Card>
        <ServiceStatusGrid />
        <DiscordServerManager initialServers={servers} initialError={error} />
      </CardContent>
    </Card>
  );
}
