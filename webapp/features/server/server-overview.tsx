"use client";

import { useEffect, useState } from "react";
import { Hash, Shield, TriangleAlert, Users } from "lucide-react";

import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Empty, EmptyDescription, EmptyHeader, EmptyTitle } from "@/components/ui/empty";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Table, TableBody, TableCell, TableHead, TableHeader, TableRow,
} from "@/components/ui/table";
import { getTeamOverview } from "@/features/server/server-api";
import type { TeamOverview } from "@/features/server/server-types";
import { UploadSettings } from "@/features/server/upload-settings";

const curriculumPrograms = ["INEL", "ICOM", "INSO", "CIIC"] as const;
const bannerTypes = ["Rules banner", "Help banner", "Welcome banner"] as const;

export function ServerOverview({ guildId }: { guildId: string }) {
  const [teams, setTeams] = useState<TeamOverview[]>([]);
  const [loading, setLoading] = useState(Boolean(guildId));
  const [error, setError] = useState("");

  useEffect(() => {
    if (!guildId) {
      return;
    }

    setLoading(true);
    getTeamOverview(guildId)
      .then((result) => {
        setTeams(result);
        setError("");
      })
      .catch((reason: unknown) => {
        setError(reason instanceof Error ? reason.message : "Could not load server teams.");
      })
      .finally(() => {
        setLoading(false);
      });
  }, [guildId]);

  if (!guildId) {
    return (
      <Empty className="border">
        <EmptyHeader>
          <EmptyTitle>No server selected</EmptyTitle>
          <EmptyDescription>Return to the dashboard and choose a Discord server.</EmptyDescription>
        </EmptyHeader>
      </Empty>
    );
  }

  return (
    <>
      <Card className="rounded-md border-border bg-card shadow-panel">
        <CardHeader>
          <CardTitle>Teams and groups</CardTitle>
          <CardDescription>Discord roles, linked channels, and assigned members.</CardDescription>
        </CardHeader>
        <CardContent>
          {error ? (
            <Alert variant="destructive">
              <TriangleAlert />
              <AlertTitle>Could not load server content</AlertTitle>
              <AlertDescription>{error}</AlertDescription>
            </Alert>
          ) : null}
          {loading ? (
            <div className="space-y-3">
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-14 w-full" />
              <Skeleton className="h-14 w-full" />
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Role</TableHead>
                  <TableHead>Discord role ID</TableHead>
                  <TableHead>Linked channel</TableHead>
                  <TableHead className="text-right">Members</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {teams.map((team) => (
                  <TableRow key={team.id}>
                    <TableCell>
                      <Badge variant="outline" style={{ borderColor: team.color, color: team.color }}>
                        <Shield />
                        {team.name}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-muted-foreground">
                      {team.discordRoleId || "Not synced"}
                    </TableCell>
                    <TableCell>
                      <span className="inline-flex items-center gap-1.5">
                        <Hash className="size-4 text-muted-foreground" />
                        {team.channelName}
                      </span>
                    </TableCell>
                    <TableCell className="text-right">
                      <Badge variant="secondary"><Users />{team.memberCount}</Badge>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>
      <div className="grid gap-6 xl:grid-cols-2">
        <UploadSettings guildId={guildId} kind="curriculum" title="Curriculums" description="PDF curriculums available to the Discord bot." options={curriculumPrograms} />
        <UploadSettings guildId={guildId} kind="banner" title="Command banners" description="Images used in rules, help, and welcome embeds." options={bannerTypes} />
      </div>
    </>
  );
}
