"use client";

import { useEffect, useState } from "react";
import Image from "next/image";
import { FileImage, Save, Upload } from "lucide-react";

import { Alert, AlertDescription } from "@/components/ui/alert";
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
import { Input } from "@/components/ui/input";
import {
  Item,
  ItemActions,
  ItemContent,
  ItemDescription,
  ItemMedia,
  ItemTitle,
} from "@/components/ui/item";
import { Label } from "@/components/ui/label";
import type { StoredUpload } from "@/features/server/server-types";

interface UploadSettingsProps {
  guildId: string;
  kind: "banner";
  title: string;
  description: string;
  options: readonly string[];
}

type UploadMap = Record<string, StoredUpload>;

export function UploadSettings({
  guildId,
  kind,
  title,
  description,
  options,
}: UploadSettingsProps): React.ReactElement {
  const [files, setFiles] = useState<UploadMap>({});
  const [savedFiles, setSavedFiles] = useState<UploadMap>({});
  const [ready, setReady] = useState(false);
  const [saved, setSaved] = useState(false);
  const [error, setError] = useState("");

  useEffect((): void => {
    const stored = window.localStorage.getItem(`friday:${guildId}:${kind}`);

    try {
      const parsed = stored ? JSON.parse(stored) as UploadMap : {};
      setFiles(parsed);
      setSavedFiles(parsed);
    } catch {
      setError("The saved upload information could not be read.");
    } finally {
      setReady(true);
    }
  }, [guildId, kind]);

  function selectFile(option: string, file?: File): void {
    if (!file) {
      return;
    }

    const reader = new FileReader();

    reader.onload = (): void => {
      const dataUrl = reader.result;

      if (typeof dataUrl !== "string") {
        setError("The selected file could not be previewed.");
        return;
      }

      setFiles((current): UploadMap => ({
        ...current,
        [option]: {
          name: file.name,
          type: file.type,
          dataUrl,
        },
      }));

      setError("");
      setSaved(false);
    };

    reader.onerror = (): void => {
      setError("The selected file could not be read.");
    };

    reader.readAsDataURL(file);
  }

  function saveFiles(): void {
    try {
      window.localStorage.setItem(
        `friday:${guildId}:${kind}`,
        JSON.stringify(files)
      );
      setSavedFiles(files);
      setSaved(true);
      setError("");
    } catch {
      setSaved(false);
      setError("The files are too large for browser storage.");
    }
  }

  const accept = "image/*";

  return (
    <Card className="w-full min-w-0 overflow-hidden rounded-md border-border bg-card shadow-panel">
      <CardHeader className="min-w-0">
        <CardTitle className="min-w-0 truncate">{title}</CardTitle>
        <CardDescription className="min-w-0 truncate">
          {description}
        </CardDescription>
        <CardAction className="shrink-0">
          <FileImage className="size-5 text-muted-foreground" />
        </CardAction>
      </CardHeader>

      <CardContent className="min-w-0 space-y-4 overflow-hidden">
        {error ? (
          <Alert variant="destructive">
            <AlertDescription>{error}</AlertDescription>
          </Alert>
        ) : null}

        <UploadItems
          options={options}
          files={files}
          savedFiles={savedFiles}
          kind={kind}
          selectFile={selectFile}
          accept={accept}
        />

        <div className="flex min-w-0 flex-wrap items-center justify-between gap-4 border-t pt-4">
          <p className="min-w-0 break-words text-xs text-muted-foreground">
            Saved in this browser until backend file storage is configured.
          </p>

          <Button onClick={saveFiles} disabled={!ready} className="shrink-0">
            <Save />
            {saved ? "Saved" : "Save changes"}
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}

function UploadItems({
  options,
  files,
  savedFiles,
  kind,
  selectFile,
  accept,
}: {
  options: readonly string[];
  files: UploadMap;
  savedFiles: UploadMap;
  kind: "banner";
  selectFile: (option: string, file?: File) => void;
  accept: string;
}) {
  return (
    <div className="grid gap-3">
      {options.map((option) => {
        const inputId = `${kind}-${option.toLowerCase().replace(" ", "-")}`;
        const upload = files[option];

        return (
          <Item
            key={option}
            className="flex w-full min-w-0 items-start gap-3 rounded-md border bg-background/40 p-3"
          >
            <Input
              id={inputId}
              type="file"
              accept={accept}
              className="hidden"
              onChange={(event): void => {
                selectFile(option, event.target.files?.[0]);
              }}
            />

            <ItemMedia
              variant="icon"
              className="size-10 shrink-0 rounded-md bg-muted text-muted-foreground"
            >
              <FileImage className="size-5" />
            </ItemMedia>

            <div className="min-w-0 flex-1">
              <div className="flex min-w-0 items-start justify-between gap-3">
                <ItemContent className="min-w-0 flex-1">
                  <div className="flex min-w-0 items-center gap-2">
                    <ItemTitle className="min-w-0 truncate">
                      {option}
                    </ItemTitle>

                    {savedFiles[option] ? (
                      <Badge variant="outline" className="shrink-0">
                        Saved
                      </Badge>
                    ) : null}
                  </div>

                  <ItemDescription className="min-w-0 truncate">
                    {upload?.name ?? `No ${kind} selected`}
                  </ItemDescription>
                </ItemContent>

                <ItemActions className="shrink-0">
                  <Button asChild variant="outline" size="sm">
                    <Label htmlFor={inputId} className="cursor-pointer">
                      <Upload />
                      {upload ? "Change" : "Choose"}
                    </Label>
                  </Button>
                </ItemActions>
              </div>

              {upload ? (
                <div className="relative mt-3 h-32 w-full min-w-0 overflow-hidden rounded-md border bg-muted">
                  <Image
                    src={upload.dataUrl}
                    alt={`${option} preview`}
                    fill
                    unoptimized
                    className="object-cover"
                  />
                </div>
              ) : null}
            </div>
          </Item>
        );
      })}
    </div>
  );
}
