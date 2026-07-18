"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Download, Trash2, TriangleAlert } from "lucide-react";

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
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Field, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Spinner } from "@/components/ui/spinner";
import { buildPrepaTeamArchive } from "@/features/server/prepa-team-export";
import { BotApi } from "@/server/webservices/bot-webservice";
import { DashboardWebservice } from "@/server/webservices/dashboard-webservice";

interface ServerHeaderCardProps {
  serverId: number | null;
  guildId: string;
  name: string;
  enabled: boolean;
}

export function ServerHeaderCard({
  serverId,
  guildId,
  name,
  enabled,
}: ServerHeaderCardProps): React.ReactElement {
  const router = useRouter();
  const [confirmation, setConfirmation] = useState("");
  const [deleting, setDeleting] = useState(false);
  const [exporting, setExporting] = useState(false);
  const [error, setError] = useState("");
  const [exportError, setExportError] = useState("");
  const hasGuild = /^[0-9]+$/.test(guildId);

  async function exportPrepaTeams(): Promise<void> {
    if (!hasGuild) {
      return;
    }

    setExporting(true);
    setExportError("");
    const result = await BotApi.getGuildPrepaTeamExport(guildId);

    if (result.isFailure) {
      setExportError(result.error.message);
      setExporting(false);
      return;
    }

    try {
      const archive = await buildPrepaTeamArchive(result.value);
      const safeServerName = name
        .trim()
        .replace(/[<>:"/\\|?*]+/g, "_")
        .replace(/[. ]+$/g, "") || guildId;
      const url = URL.createObjectURL(archive);
      const link = document.createElement("a");
      link.href = url;
      link.download = `${safeServerName}-prepas.zip`;
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.setTimeout(() => {
        URL.revokeObjectURL(url);
      }, 0);
    } catch (exportFailure) {
      setExportError(exportFailure instanceof Error
        ? exportFailure.message
        : "Could not build the Prepa team archive.");
    } finally {
      setExporting(false);
    }
  }

  async function deleteServer(): Promise<void> {
    if (serverId === null || confirmation !== name) {
      return;
    }

    setDeleting(true);
    setError("");
    const result = await DashboardWebservice.deleteServer(serverId);

    if (result.isFailure) {
      setError(result.error.message);
      setDeleting(false);
      return;
    }

    router.replace("/dashboard");
    router.refresh();
  }

  return (
    <Card className="min-w-0 rounded-md border-border bg-card shadow-panel">
      <CardHeader>
        <div className="flex min-w-0 flex-wrap items-center gap-3">
          <CardTitle className="truncate text-3xl">{name}</CardTitle>
          <Badge
            variant="outline"
            className={
              enabled
                ? "border-success/40 bg-success/10 text-success"
                : "text-muted-foreground"
            }
          >
            {enabled ? "Active" : "Inactive"}
          </Badge>
        </div>
        <CardDescription className="truncate">
          Discord ID: {guildId || "No server selected"}
        </CardDescription>
        <CardAction className="flex flex-wrap items-center gap-2">
          <Button
            variant="outline"
            disabled={!hasGuild || exporting}
            onClick={exportPrepaTeams}
          >
            {exporting ? <Spinner /> : <Download />}
            Download Prepa teams
          </Button>
          <Dialog onOpenChange={() => {
            setConfirmation("");
            setError("");
          }}>
            <DialogTrigger asChild>
              <Button
                variant="destructive"
                disabled={serverId === null}
              >
                <Trash2 />
                Delete server
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Delete {name}?</DialogTitle>
                <DialogDescription>
                  This permanently removes the server and its related Discord data.
                  Type <strong>{name}</strong> to confirm.
                </DialogDescription>
              </DialogHeader>
              <Field>
                <FieldLabel htmlFor="server-delete-confirmation">
                  Server name
                </FieldLabel>
                <Input
                  id="server-delete-confirmation"
                  value={confirmation}
                  onChange={(event) => {
                    setConfirmation(event.target.value);
                  }}
                  autoComplete="off"
                />
              </Field>
              {error ? (
                <Alert variant="destructive">
                  <TriangleAlert />
                  <AlertTitle>Could not delete server</AlertTitle>
                  <AlertDescription>{error}</AlertDescription>
                </Alert>
              ) : null}
              <DialogFooter showCloseButton>
                <Button
                  variant="destructive"
                  disabled={confirmation !== name || deleting}
                  onClick={deleteServer}
                >
                  {deleting ? <Spinner /> : <Trash2 />}
                  Delete server
                </Button>
              </DialogFooter>
            </DialogContent>
          </Dialog>
        </CardAction>
      </CardHeader>
      <CardContent className={exportError ? undefined : "sr-only"}>
        {exportError ? (
          <Alert variant="destructive">
            <TriangleAlert />
            <AlertTitle>Could not export Prepa teams</AlertTitle>
            <AlertDescription>{exportError}</AlertDescription>
          </Alert>
        ) : "Server actions"}
      </CardContent>
    </Card>
  );
}
