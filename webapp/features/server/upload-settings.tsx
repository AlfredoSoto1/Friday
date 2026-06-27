"use client";

import { useEffect, useState } from "react";
import { FileImage, FileText, Save, Upload } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Card, CardAction, CardContent, CardDescription, CardHeader, CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

interface UploadSettingsProps {
  guildId: string;
  kind: "curriculum" | "banner";
  title: string;
  description: string;
  options: readonly string[];
}

export function UploadSettings({
  guildId, kind, title, description, options,
}: UploadSettingsProps) {
  const [files, setFiles] = useState<Record<string, string>>({});
  const [savedFiles, setSavedFiles] = useState<Record<string, string>>({});
  const [ready, setReady] = useState(false);
  const [saved, setSaved] = useState(false);

  useEffect(() => {
    const stored = window.localStorage.getItem(`friday:${guildId}:${kind}`);
    const parsed = stored ? JSON.parse(stored) : {};

    setFiles(parsed);
    setSavedFiles(parsed);
    setReady(true);
  }, [guildId, kind]);

  function selectFile(option: string, file?: File) {
    if (!file) {
      return;
    }

    setFiles((current) => ({ ...current, [option]: file.name }));
    setSaved(false);
  }

  function saveFiles() {
    window.localStorage.setItem(
      `friday:${guildId}:${kind}`,
      JSON.stringify(files)
    );
    setSavedFiles(files);
    setSaved(true);
  }

  const Icon = kind === "curriculum" ? FileText : FileImage;
  const accept = kind === "curriculum" ? "application/pdf" : "image/*";

  return (
    <Card className="rounded-md border-border bg-card shadow-panel">
      <CardHeader>
        <CardTitle>{title}</CardTitle>
        <CardDescription>{description}</CardDescription>
        <CardAction><Icon className="size-5 text-muted-foreground" /></CardAction>
      </CardHeader>
      <CardContent className="space-y-4">
        {options.map((option) => {
          const inputId = `${kind}-${option.toLowerCase().replace(" ", "-")}`;

          return (
            <Card key={option} size="sm" className="rounded-md bg-background/40">
              <CardHeader>
                <CardTitle>{option}</CardTitle>
                <CardDescription>{files[option] ?? `No ${kind} selected`}</CardDescription>
                <CardAction>
                  {savedFiles[option] ? <Badge variant="outline">Saved</Badge> : null}
                </CardAction>
              </CardHeader>
              <CardContent>
                <Input
                  id={inputId}
                  type="file"
                  accept={accept}
                  className="sr-only"
                  onChange={(event) => {
                    selectFile(option, event.target.files?.[0]);
                  }}
                />
                <Button asChild variant="outline" size="sm">
                  <Label htmlFor={inputId}>
                    <Upload />
                    {files[option] ? "Change file" : "Choose file"}
                  </Label>
                </Button>
              </CardContent>
            </Card>
          );
        })}
        <div className="flex items-center justify-between gap-4 border-t pt-4">
          <p className="text-xs text-muted-foreground">
            Saved in this browser until backend file storage is configured.
          </p>
          <Button onClick={saveFiles} disabled={!ready}>
            <Save />
            {saved ? "Saved" : "Save changes"}
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}
