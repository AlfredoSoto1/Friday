"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { Shield, TriangleAlert, Users } from "lucide-react";

import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardAction,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Empty, EmptyDescription, EmptyHeader, EmptyTitle } from "@/components/ui/empty";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import type { TeamOverview } from "@/features/server/server-types";
import { BotApi } from "@/server/webservices/bot-webservice";

interface ServerOverviewProps {
  serverId: number | null;
  guildId: string;
  serverName: string;
  enabled: boolean;
}

export function ServerOverview({
  serverId,
  guildId,
  serverName,
  enabled,
}: ServerOverviewProps): React.ReactElement {
  const [teams, setTeams] = useState<TeamOverview[]>([]);
  const [error, setError] = useState("");

  useEffect((): void => {
    const numericGuildId = Number(guildId);

    if (!guildId || !Number.isSafeInteger(numericGuildId)) {
      return;
    }

    Promise.all([
      BotApi.getGuildRoles(numericGuildId),
      BotApi.getGuildMembers(numericGuildId),
    ])
      .then(([rolesResult, membersResult]): void => {
        const failedResult = [
          rolesResult,
          membersResult,
        ].find((result) => result.isFailure);

        if (failedResult?.isFailure) {
          setError(failedResult.error.message);
          return;
        }

        const members = membersResult.value.items;
        const nextTeams = rolesResult.value.items.map((role): TeamOverview => {
          const discordRoleId = role.discordRoleId ?? "";
          const memberCount = members.filter((member): boolean => (
            member.roleIds.includes(discordRoleId)
          )).length;

          return {
            id: role.roleId,
            name: role.name,
            discordRoleId,
            color: `#${(role.color ?? 0x5865f2)
              .toString(16)
              .padStart(6, "0")}`,
            memberCount,
          };
        });

        setTeams(nextTeams);
        setError("");
      })
      .catch((reason: unknown): void => {
        setError(
          reason instanceof Error
            ? reason.message
            : "Could not load server teams."
        );
      });
  }, [guildId]);

  if (!guildId) {
    return (
      <Empty className="border">
        <EmptyHeader>
          <EmptyTitle>No server selected</EmptyTitle>
          <EmptyDescription>
            Return to the dashboard and choose a Discord server.
          </EmptyDescription>
        </EmptyHeader>
      </Empty>
    );
  }

  return (
    <>
      <Card className="min-w-0 rounded-md border-border bg-card shadow-panel">
        <CardHeader>
          <CardTitle>Teams and groups</CardTitle>
          <CardDescription>
            Discord roles and assigned members.
          </CardDescription>
          <CardAction>
            {teams.length ? (
              <Button size="sm">Edit</Button>
            ) : (
              <Button asChild size="sm">
                <Link
                  href={{
                    pathname: "/roster",
                    query: {
                      serverId: serverId?.toString() ?? "",
                      guildId,
                      name: serverName,
                      enabled: enabled.toString(),
                    },
                  }}
                >
                  Prepare student groups
                </Link>
              </Button>
            )}
          </CardAction>
        </CardHeader>
        <CardContent className="min-w-0">
          {error ? (
            <Alert variant="destructive">
              <TriangleAlert />
              <AlertTitle>Could not load server content</AlertTitle>
              <AlertDescription>{error}</AlertDescription>
            </Alert>
          ) : null}
          {teams.length ? (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Role</TableHead>
                  <TableHead>Discord role ID</TableHead>
                  <TableHead className="text-right">Members</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {teams.map((team) => (
                  <TableRow key={team.id}>
                    <TableCell>
                      <Badge
                        variant="outline"
                        style={{
                          borderColor: team.color,
                          color: team.color,
                        }}
                      >
                        <Shield />
                        {team.name}
                      </Badge>
                    </TableCell>
                    <TableCell className="max-w-48 truncate text-muted-foreground">
                      {team.discordRoleId || "Not synced"}
                    </TableCell>
                    <TableCell className="text-right">
                      <Badge variant="secondary">
                        <Users />
                        {team.memberCount}
                      </Badge>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          ) : null}
        </CardContent>
      </Card>
    </>
  );
}
