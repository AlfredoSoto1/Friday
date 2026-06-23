"use client";

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import type { BotChannel } from "@/features/dashboard/admin-types";

export function BotChannelsCard({ channels }: { channels: BotChannel[] }) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Channels</CardTitle>
        <CardDescription>{channels.length} channels synced from Discord</CardDescription>
      </CardHeader>
      <CardContent>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Name</TableHead>
              <TableHead>Discord ID</TableHead>
              <TableHead>Type</TableHead>
              <TableHead>Position</TableHead>
              <TableHead>NSFW</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {channels.map((channel) => (
              <TableRow key={channel.channel_id}>
                <TableCell>{channel.name}</TableCell>
                <TableCell>{channel.discord_channel_id}</TableCell>
                <TableCell>{channel.type}</TableCell>
                <TableCell>{channel.position}</TableCell>
                <TableCell>{channel.nsfw ? "Yes" : "No"}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </CardContent>
    </Card>
  );
}
