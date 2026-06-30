import { Users } from "lucide-react";

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Empty, EmptyDescription, EmptyHeader, EmptyTitle } from "@/components/ui/empty";
import { ItemContent, ItemMedia, ItemTitle } from "@/components/ui/item";
import { BackToServerButton } from "@/features/roster/back-to-server-button";
import { RosterManager } from "@/features/roster/roster-manager";

interface RosterPageProps {
  searchParams: Promise<{
    guildId?: string;
    name?: string;
  }>;
}

export default async function RosterPage({
  searchParams,
}: RosterPageProps): Promise<React.ReactElement> {
  const params = await searchParams;
  const guildId = params.guildId ?? "";
  const name = params.name ?? "Discord server";

  return (
    <Card className="min-h-screen w-full min-w-0 overflow-x-hidden rounded-none bg-transparent py-0 ring-0">
      <CardHeader className="border-b border-border bg-background/80 px-0 py-4">
        <div className="mx-auto flex w-full max-w-7xl items-center justify-between gap-4 px-4 sm:px-6">
          <div className="flex min-w-0 items-center gap-4">
            <ItemMedia
              variant="icon"
              className="size-9 shrink-0 rounded-md bg-discord text-white"
            >
              <Users className="size-5" />
            </ItemMedia>
            <ItemContent className="min-w-0">
              <ItemTitle className="text-lg">Friday</ItemTitle>
              <CardDescription className="truncate">
                Team distribution for {name}
              </CardDescription>
            </ItemContent>
          </div>
          <BackToServerButton />
        </div>
      </CardHeader>
      <CardContent className="mx-auto grid w-full min-w-0 max-w-7xl gap-6 px-4 py-8 sm:px-6">
        <Card className="rounded-md border-border bg-card shadow-panel">
          <CardHeader>
            <CardTitle className="text-3xl">Upload student list</CardTitle>
            <CardDescription>
              Import a roster, sort it the way you want, and split it into balanced teams.
            </CardDescription>
          </CardHeader>
        </Card>
        {guildId ? (
          <RosterManager guildId={guildId} />
        ) : (
          <Empty className="border">
            <EmptyHeader>
              <EmptyTitle>No server selected</EmptyTitle>
              <EmptyDescription>
                Return to the dashboard and choose a Discord server.
              </EmptyDescription>
            </EmptyHeader>
          </Empty>
        )}
      </CardContent>
    </Card>
  );
}
