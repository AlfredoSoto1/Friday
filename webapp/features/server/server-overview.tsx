"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { Hash, Shield, TriangleAlert, Users } from "lucide-react";

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
import { UploadSettings } from "@/features/server/upload-settings";
import { BotApi } from "@/server/webservices/bot-webservice";

const curriculumPrograms = ["INEL", "ICOM", "INSO", "CIIC"] as const;
const bannerTypes = ["Rules banner", "Help banner", "Welcome banner"] as const;

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
      BotApi.getGuildChannels(numericGuildId),
      BotApi.getGuildMembers(numericGuildId),
    ])
      .then(([rolesResult, channelsResult, membersResult]): void => {
        const failedResult = [
          rolesResult,
          channelsResult,
          membersResult,
        ].find((result) => result.isFailure);

        if (failedResult?.isFailure) {
          setError(failedResult.error.message);
          return;
        }

        const channels = channelsResult.value.items;
        const members = membersResult.value.items;
        const nextTeams = rolesResult.value.items.map((role): TeamOverview => {
          const discordRoleId = role.discordRoleId ?? "";
          const channel = channels.find((item): boolean => (
            item.name.toLowerCase() === role.name.toLowerCase()
          ));
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
            channelName: channel?.name ?? "Not linked",
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
      })
;
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
            Discord roles, linked channels, and assigned members.
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
                  <TableHead>Linked channel</TableHead>
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
                    <TableCell>
                      <span className="inline-flex items-center gap-1.5">
                        <Hash className="size-4 text-muted-foreground" />
                        {team.channelName}
                      </span>
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
      <div className="grid min-w-0 gap-6 xl:grid-cols-2">
        <UploadSettings
          guildId={guildId}
          kind="curriculum"
          title="Curriculums"
          description="PDF curriculums available to the Discord bot."
          options={curriculumPrograms}
        />
        <UploadSettings
          guildId={guildId}
          kind="banner"
          title="Command banners"
          description="Images used in rules, help, and welcome embeds."
          options={bannerTypes}
        />
      </div>
    </>
  );
}
