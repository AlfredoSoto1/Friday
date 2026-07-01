"use client";

import { useState } from "react";
import Link from "next/link";
import { ArrowRight, Plus, Server, Trash2, TriangleAlert } from "lucide-react";

import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
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
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import {
  Empty,
  EmptyDescription,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
} from "@/components/ui/empty";
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import {
  Item,
  ItemActions,
  ItemContent,
  ItemDescription,
  ItemGroup,
  ItemMedia,
  ItemTitle,
} from "@/components/ui/item";
import { Spinner } from "@/components/ui/spinner";
import { Switch } from "@/components/ui/switch";
import type { DiscordServerDto } from "@/server/entities/dashboard";
import { DashboardWebservice } from "@/server/webservices/dashboard-webservice";

interface DiscordServerManagerProps {
  initialServers: DiscordServerDto[];
  initialError?: string;
}

export function DiscordServerManager({
  initialServers,
  initialError,
}: DiscordServerManagerProps) {
  const [servers, setServers] = useState(initialServers);
  const [name, setName] = useState("");
  const [serverCode, setServerCode] = useState("");
  const [dialogOpen, setDialogOpen] = useState(false);
  const [pendingServerId, setPendingServerId] = useState<number | null>(null);
  const [creating, setCreating] = useState(false);
  const [error, setError] = useState(initialError ?? "");

  async function createServer() {
    if (!name.trim() || !serverCode.trim()) {
      setError("Server name and Discord server ID are required.");
      return;
    }

    setCreating(true);
    setError("");
    const result = await DashboardWebservice.createServer({
      name: name.trim(),
      serverCode: serverCode.trim(),
    });
    setCreating(false);

    if (result.isFailure) {
      setError(result.error.message);
      return;
    }

    setServers((current) => [result.value, ...current]);
    setName("");
    setServerCode("");
    setDialogOpen(false);
  }

  async function setEnabled(server: DiscordServerDto, enabled: boolean) {
    setPendingServerId(server.serverId);
    setError("");
    const result = await DashboardWebservice.setServerEnabled(
      server.serverId,
      { enabled }
    );
    setPendingServerId(null);

    if (result.isFailure) {
      setError(result.error.message);
      return;
    }

    setServers((current) =>
      current.map((item) =>
        item.serverId === server.serverId ? result.value : item
      )
    );
  }

  async function deleteServer(server: DiscordServerDto) {
    setPendingServerId(server.serverId);
    setError("");
    const result = await DashboardWebservice.deleteServer(server.serverId);
    setPendingServerId(null);

    if (result.isFailure) {
      setError(result.error.message);
      return;
    }

    setServers((current) =>
      current.filter((item) => item.serverId !== server.serverId)
    );
  }

  return (
    <Card className="rounded-md border-border bg-card shadow-panel">
      <CardHeader>
        <CardTitle>Discord servers</CardTitle>
        <CardDescription>
          {servers.length} {servers.length === 1 ? "server" : "servers"} registered
        </CardDescription>
        <CardAction className="flex flex-wrap justify-end gap-2">
          <Button asChild variant="outline" size="sm">
            <Link href="/inelicom">Update INEL/ICOM Content</Link>
          </Button>
          <Button asChild variant="outline" size="sm">
            <Link href="/roster">Upload Student List</Link>
          </Button>
          <Button variant="outline" size="sm">Edit Organizations</Button>
          <Button variant="outline" size="sm">Edit Projects</Button>
          <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
            <DialogTrigger asChild>
              <Button size="sm">
                <Plus />
                Add server
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Add Discord server</DialogTitle>
                <DialogDescription>
                  Register a Discord server with its name and guild ID.
                </DialogDescription>
              </DialogHeader>
              <FieldGroup>
                <Field>
                  <FieldLabel htmlFor="server-name">Server name</FieldLabel>
                  <Input
                    id="server-name"
                    value={name}
                    onChange={(event) => setName(event.target.value)}
                    placeholder="Friday Community"
                  />
                </Field>
                <Field>
                  <FieldLabel htmlFor="server-code">Discord server ID</FieldLabel>
                  <Input
                    id="server-code"
                    value={serverCode}
                    onChange={(event) => setServerCode(event.target.value)}
                    placeholder="100000000000000001"
                    inputMode="numeric"
                  />
                </Field>
              </FieldGroup>
              <DialogFooter showCloseButton>
                <Button onClick={createServer} disabled={creating}>
                  {creating ? <Spinner /> : <Plus />}
                  Add server
                </Button>
              </DialogFooter>
            </DialogContent>
          </Dialog>
        </CardAction>
      </CardHeader>
      <CardContent className="space-y-4">
        {error ? (
          <Alert variant="destructive">
            <TriangleAlert />
            <AlertTitle>Server action failed</AlertTitle>
            <AlertDescription>{error}</AlertDescription>
          </Alert>
        ) : null}
        {servers.length === 0 ? (
          <Empty className="border">
            <EmptyHeader>
              <EmptyMedia variant="icon">
                <Server />
              </EmptyMedia>
              <EmptyTitle>No Discord servers</EmptyTitle>
              <EmptyDescription>
                Add the first server to manage it from this dashboard.
              </EmptyDescription>
            </EmptyHeader>
          </Empty>
        ) : (
          <ItemGroup className="gap-2">
            {servers.map((server) => (
              <Item key={server.serverId} variant="outline">
                <ItemMedia variant="icon" className="size-9 rounded-md bg-discord/10 text-discord">
                  <Server />
                </ItemMedia>
                <ItemContent>
                  <ItemTitle>
                    {server.name}
                    <Badge
                      variant="outline"
                      className={
                        server.enabled
                          ? "border-success/40 bg-success/10 text-success"
                          : "text-muted-foreground"
                      }
                    >
                      {server.enabled ? "Active" : "Inactive"}
                    </Badge>
                  </ItemTitle>
                  <ItemDescription>
                    Discord ID: {server.serverCode}
                  </ItemDescription>
                </ItemContent>
                <ItemActions>
                  {pendingServerId === server.serverId ? <Spinner /> : null}
                  <Button asChild variant="outline" size="sm">
                    <Link
                      href={{
                        pathname: "/server",
                        query: {
                          serverId: server.serverId.toString(),
                          guildId: server.serverCode,
                          name: server.name,
                          enabled: server.enabled.toString(),
                        },
                      }}
                    >
                      View server
                      <ArrowRight />
                    </Link>
                  </Button>
                  <Switch
                    checked={server.enabled}
                    onCheckedChange={(enabled) => setEnabled(server, enabled)}
                    disabled={pendingServerId === server.serverId}
                    aria-label={`${server.enabled ? "Deactivate" : "Activate"} ${server.name}`}
                  />
                  <AlertDialog>
                    <AlertDialogTrigger asChild>
                      <Button
                        variant="ghost"
                        size="icon-sm"
                        aria-label={`Remove ${server.name}`}
                        disabled={pendingServerId === server.serverId}
                      >
                        <Trash2 />
                      </Button>
                    </AlertDialogTrigger>
                    <AlertDialogContent>
                      <AlertDialogHeader>
                        <AlertDialogTitle>Remove {server.name}?</AlertDialogTitle>
                        <AlertDialogDescription>
                          This permanently removes the server and its related Discord data.
                        </AlertDialogDescription>
                      </AlertDialogHeader>
                      <AlertDialogFooter>
                        <AlertDialogCancel>Cancel</AlertDialogCancel>
                        <AlertDialogAction
                          variant="destructive"
                          onClick={() => deleteServer(server)}
                        >
                          Remove server
                        </AlertDialogAction>
                      </AlertDialogFooter>
                    </AlertDialogContent>
                  </AlertDialog>
                </ItemActions>
              </Item>
            ))}
          </ItemGroup>
        )}
      </CardContent>
    </Card>
  );
}
