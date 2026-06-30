import Link from "next/link";
import { ArrowLeft, Bot } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader } from "@/components/ui/card";
import { ItemContent, ItemMedia, ItemTitle } from "@/components/ui/item";
import { ServerHeaderCard } from "@/features/server/server-header-card";
import { ServerOverview } from "@/features/server/server-overview";

interface ServerPageProps {
  searchParams: Promise<{
    serverId?: string;
    guildId?: string;
    name?: string;
    enabled?: string;
  }>;
}

export default async function ServerPage({
  searchParams,
}: ServerPageProps): Promise<React.ReactElement> {
  const params = await searchParams;
  const guildId = params.guildId ?? "";
  const name = params.name ?? "Discord server";
  const parsedServerId = params.serverId
    ? Number(params.serverId)
    : Number.NaN;
  const serverId = Number.isInteger(parsedServerId)
    ? parsedServerId
    : null;
  const enabled = params.enabled === "true";

  return (
    <div>
      <Card className="min-h-screen w-full min-w-0 overflow-x-hidden rounded-none bg-transparent py-0 ring-0">
        <CardHeader className="border-b border-border bg-background/80 px-0 py-4">
          <div className="mx-auto flex w-full max-w-7xl items-center justify-between gap-4 px-4 sm:px-6">
            <div className="flex min-w-0 items-center gap-4">
              <ItemMedia
                variant="icon"
                className="size-9 shrink-0 rounded-md bg-discord text-white"
              >
                <Bot className="size-5" />
              </ItemMedia>
              <ItemContent className="min-w-0">
                <ItemTitle className="text-lg">Friday</ItemTitle>
                <CardDescription className="truncate">
                  Server configuration
                </CardDescription>
              </ItemContent>
            </div>
            <Button asChild variant="outline" size="sm">
              <Link href="/dashboard">
                <ArrowLeft />
                Dashboard
              </Link>
            </Button>
          </div>
        </CardHeader>
        <CardContent className="mx-auto grid w-full min-w-0 max-w-7xl gap-6 px-4 py-8 sm:px-6">
          <ServerHeaderCard
            serverId={serverId}
            guildId={guildId}
            name={name}
            enabled={enabled}
          />
          <ServerOverview guildId={guildId} name={name} />
        </CardContent>
      </Card>
    </div>
  );
}
