import Link from "next/link";
import { ArrowLeft, Bot } from "lucide-react";

import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { ItemContent, ItemMedia, ItemTitle } from "@/components/ui/item";
import { ServerOverview } from "@/features/server/server-overview";

interface ServerPageProps {
  searchParams: Promise<{
    guildId?: string;
    name?: string;
  }>;
}

export default async function ServerPage({ searchParams }: ServerPageProps) {
  const { guildId = "", name = "Discord server" } = await searchParams;

  return (
    <Card className="min-h-screen rounded-none bg-transparent py-0 ring-0">
      <CardHeader className="border-b border-border bg-background/80 px-6 py-4">
        <div className="flex items-center justify-between gap-4">
          <div className="flex items-center gap-4">
            <ItemMedia variant="icon" className="size-9 rounded-md bg-discord text-white">
              <Bot className="size-5" />
            </ItemMedia>
            <ItemContent>
              <ItemTitle className="text-lg">Friday</ItemTitle>
              <CardDescription>Server configuration</CardDescription>
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
      <CardContent className="mx-auto grid w-full max-w-7xl gap-6 px-6 py-8">
        <Card className="rounded-md border-border bg-card shadow-panel">
          <CardHeader>
            <CardTitle className="text-3xl">{name}</CardTitle>
            <CardDescription>
              Discord ID: {guildId || "No server selected"}
            </CardDescription>
          </CardHeader>
        </Card>
        <ServerOverview guildId={guildId} />
      </CardContent>
    </Card>
  );
}
