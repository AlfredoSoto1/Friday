"use client";

import { useState } from "react";
import { FileSpreadsheet, Upload, X } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardAction,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Item, ItemActions, ItemContent, ItemDescription, ItemMedia, ItemTitle } from "@/components/ui/item";
import { Label } from "@/components/ui/label";
import { Spinner } from "@/components/ui/spinner";
import type { RosterFile } from "@/features/roster/roster-types";

const ACCEPTED_EXTENSIONS = [".csv", ".xlsx", ".txt"];

interface RosterUploadCardProps {
  file: RosterFile | null;
  studentCount: number;
  loading: boolean;
  parseError: string;
  onFileSelected: (file: File) => void;
  onRemoveFile: () => void;
}

function isAcceptedFile(file: File): boolean {
  const lowerName = file.name.toLowerCase();
  return ACCEPTED_EXTENSIONS.some((extension) => lowerName.endsWith(extension));
}

function formatFileSize(bytes: number): string {
  if (bytes < 1024) {
    return `${bytes} B`;
  }

  return `${(bytes / 1024).toFixed(1)} KB`;
}

export function RosterUploadCard({
  file,
  studentCount,
  loading,
  parseError,
  onFileSelected,
  onRemoveFile,
}: RosterUploadCardProps): React.ReactElement {
  const [dragActive, setDragActive] = useState(false);
  const [typeError, setTypeError] = useState("");

  function handleFile(candidate?: File | null): void {
    if (!candidate) {
      return;
    }

    if (!isAcceptedFile(candidate)) {
      setTypeError("Upload a CSV, Excel (.xlsx), or TXT file.");
      return;
    }

    setTypeError("");
    onFileSelected(candidate);
  }

  const error = typeError || parseError;

  return (
    <Card className="min-w-0 rounded-md border-border bg-card shadow-panel">
      <CardHeader>
        <CardTitle>Student list</CardTitle>
        <CardDescription>
          Upload the roster you want to split into teams.
        </CardDescription>
        <CardAction>
          <FileSpreadsheet className="size-5 text-muted-foreground" />
        </CardAction>
      </CardHeader>
      <CardContent className="space-y-3">
        <Input
          id="roster-file-input"
          type="file"
          accept=".csv,.xlsx,.txt,text/csv,text/plain"
          className="hidden"
          disabled={loading}
          onChange={(event): void => {
            handleFile(event.target.files?.[0]);
            event.target.value = "";
          }}
        />

        {file ? (
          <Item variant="outline" className="items-start gap-3 p-3">
            <ItemMedia variant="icon" className="size-10 shrink-0 rounded-md bg-muted text-muted-foreground">
              <FileSpreadsheet className="size-5" />
            </ItemMedia>
            <ItemContent className="min-w-0">
              <ItemTitle className="truncate">{file.name}</ItemTitle>
              <ItemDescription>{formatFileSize(file.size)}</ItemDescription>
            </ItemContent>
            <ItemActions className="shrink-0 gap-2">
              {loading ? (
                <Badge variant="secondary">
                  <Spinner />
                  Parsing
                </Badge>
              ) : (
                <Badge variant="secondary">{studentCount} students</Badge>
              )}
              <Button asChild variant="outline" size="sm" disabled={loading}>
                <Label htmlFor="roster-file-input" className="cursor-pointer">
                  <Upload />
                  Replace
                </Label>
              </Button>
              <Button variant="ghost" size="icon-sm" onClick={onRemoveFile} disabled={loading}>
                <X />
                <span className="sr-only">Remove file</span>
              </Button>
            </ItemActions>
          </Item>
        ) : (
          <Label
            htmlFor="roster-file-input"
            onDragOver={(event): void => {
              event.preventDefault();
              setDragActive(true);
            }}
            onDragLeave={(): void => {
              setDragActive(false);
            }}
            onDrop={(event): void => {
              event.preventDefault();
              setDragActive(false);
              handleFile(event.dataTransfer.files?.[0]);
            }}
            className={`flex cursor-pointer flex-col items-center justify-center gap-2 rounded-md border border-dashed py-10 text-center transition-colors ${
              dragActive ? "border-primary bg-primary/5" : "border-border hover:bg-muted/40"
            }`}
          >
            <Upload className="size-6 text-muted-foreground" />
            <span className="text-sm font-medium">Click to upload or drag a file here</span>
            <span className="text-xs text-muted-foreground">CSV, Excel (.xlsx), or TXT</span>
          </Label>
        )}

        {error ? (
          <p className="text-xs text-destructive">{error}</p>
        ) : null}
      </CardContent>
    </Card>
  );
}
