"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Trash2, TriangleAlert } from "lucide-react";

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
  const [error, setError] = useState("");

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
        <CardAction>
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
      <CardContent className="sr-only">Server actions</CardContent>
    </Card>
  );
}
