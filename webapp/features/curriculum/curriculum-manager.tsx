"use client";

import { useEffect, useState } from "react";
import type { DragEvent } from "react";
import {
  Check,
  Download,
  FileText,
  Save,
  Trash2,
  TriangleAlert,
  Upload,
  X,
} from "lucide-react";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
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
import { cn } from "@/lib/utils";
import { Skeleton } from "@/components/ui/skeleton";
import { Spinner } from "@/components/ui/spinner";
import type { CurriculumDto } from "@/server/entities/curriculum";
import { CurriculumApi } from "@/server/webservices/curriculum-webservice";

interface ProgramInfo {
  code: string;
  name: string;
}

const PROGRAMS: ProgramInfo[] = [
  { code: "INEL", name: "Electrical Engineering" },
  { code: "ICOM", name: "Computer Engineering" },
];

function formatFileSize(bytes: number): string {
  if (bytes < 1024) {
    return `${bytes} B`;
  }

  return `${(bytes / 1024).toFixed(1)} KB`;
}

function isPdf(file: File): boolean {
  return file.type === "application/pdf" || file.name.toLowerCase().endsWith(".pdf");
}

export function CurriculumManager(): React.ReactElement {
  const [curriculums, setCurriculums] = useState<CurriculumDto[]>([]);
  const [pendingFiles, setPendingFiles] = useState<Record<string, File>>({});
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [saved, setSaved] = useState(false);
  const [error, setError] = useState("");

  useEffect((): void => {
    void loadCurriculums();
  }, []);

  async function loadCurriculums(): Promise<void> {
    setLoading(true);
    const result = await CurriculumApi.getCurriculums();

    if (result.isFailure) {
      setError(result.error.message);
      setLoading(false);
      return;
    }

    setCurriculums(result.value.items);
    setError("");
    setLoading(false);
  }

  function selectFile(program: string, file?: File): void {
    if (!file) {
      return;
    }

    if (!isPdf(file)) {
      setError(`${file.name} is not a PDF file.`);
      return;
    }

    setError("");
    setSaved(false);
    setPendingFiles((current) => ({ ...current, [program]: file }));
  }

  function clearPending(program: string): void {
    setPendingFiles((current) => {
      const next = { ...current };
      delete next[program];
      return next;
    });
  }

  async function saveChanges(): Promise<void> {
    const entries = Object.entries(pendingFiles);
    if (entries.length === 0) {
      return;
    }

    setSaving(true);
    setError("");

    const results = await Promise.all(
      entries.map(([program, file]) => (
        CurriculumApi.uploadCurriculum(program, file).then((result) => ({ program, result }))
      ))
    );

    const failed = results.filter(({ result }) => result.isFailure);
    const succeeded = results.filter(({ result }) => result.isSuccess);

    succeeded.forEach(({ program }) => clearPending(program));

    if (failed.length > 0) {
      setError(
        `Could not save: ${failed.map(({ program }) => program).join(", ")}. `
        + failed[0].result.error.message
      );
    } else {
      setSaved(true);
    }

    setSaving(false);
    await loadCurriculums();
  }

  async function deleteCurriculum(program: string): Promise<void> {
    setError("");
    const result = await CurriculumApi.deleteCurriculum(program);

    if (result.isFailure) {
      setError(result.error.message);
      return;
    }

    await loadCurriculums();
  }

  if (loading) {
    return (
      <div className="grid gap-3">
        <Skeleton className="h-24" />
        <Skeleton className="h-24" />
        <Skeleton className="h-24" />
        <Skeleton className="h-24" />
      </div>
    );
  }

  const hasPending = Object.keys(pendingFiles).length > 0;

  return (
    <Card className="min-w-0 rounded-md border-border bg-card shadow-panel">
      <CardHeader>
        <CardTitle>INEL/ICOM curriculums</CardTitle>
        <CardDescription>
          Upload the latest curriculum PDF for each program. The bot serves whatever is saved here.
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        {error ? (
          <Alert variant="destructive">
            <TriangleAlert />
            <AlertTitle>Curriculum action failed</AlertTitle>
            <AlertDescription>{error}</AlertDescription>
          </Alert>
        ) : null}

        <div className="grid gap-3">
          {PROGRAMS.map((program) => (
            <CurriculumProgramCard
              key={program.code}
              program={program}
              current={curriculums.find((item) => item.program === program.code)}
              pending={pendingFiles[program.code]}
              onSelectFile={(file): void => selectFile(program.code, file)}
              onClearPending={(): void => clearPending(program.code)}
              onDelete={(): void => {
                void deleteCurriculum(program.code);
              }}
            />
          ))}
        </div>
      </CardContent>
      <CardFooter className="flex flex-wrap items-center justify-between gap-4 border-t pt-4">
        <p className="text-xs text-muted-foreground">
          {hasPending
            ? `${Object.keys(pendingFiles).length} curriculum${Object.keys(pendingFiles).length > 1 ? "s" : ""} ready to save.`
            : "Choose a PDF for a program, then save to send it to the backend."}
        </p>
        <Button onClick={(): void => { void saveChanges(); }} disabled={!hasPending || saving}>
          {saving ? <Spinner /> : saved && !hasPending ? <Check /> : <Save />}
          {saving ? "Saving" : saved && !hasPending ? "Saved" : "Save changes"}
        </Button>
      </CardFooter>
    </Card>
  );
}

interface CurriculumProgramCardProps {
  program: ProgramInfo;
  current?: CurriculumDto;
  pending?: File;
  onSelectFile: (file?: File) => void;
  onClearPending: () => void;
  onDelete: () => void;
}

function CurriculumProgramCard({
  program,
  current,
  pending,
  onSelectFile,
  onClearPending,
  onDelete,
}: CurriculumProgramCardProps): React.ReactElement {
  const [isDragOver, setIsDragOver] = useState(false);
  const inputId = `curriculum-${program.code.toLowerCase()}`;

  function handleDragOver(event: DragEvent<HTMLDivElement>): void {
    event.preventDefault();
    setIsDragOver(true);
  }

  function handleDragLeave(event: DragEvent<HTMLDivElement>): void {
    event.preventDefault();
    setIsDragOver(false);
  }

  function handleDrop(event: DragEvent<HTMLDivElement>): void {
    event.preventDefault();
    setIsDragOver(false);
    onSelectFile(event.dataTransfer.files?.[0]);
  }

  return (
    <Item className="flex w-full min-w-0 items-start gap-3 rounded-md border bg-background/40 p-3">
      <Input
        id={inputId}
        type="file"
        accept=".pdf,application/pdf"
        className="hidden"
        onChange={(event): void => {
          onSelectFile(event.target.files?.[0]);
          event.target.value = "";
        }}
      />

      <ItemMedia
        variant="icon"
        className="size-10 shrink-0 rounded-md bg-muted text-muted-foreground"
      >
        <FileText className="size-5" />
      </ItemMedia>

      <div className="min-w-0 flex-1">
        <div className="flex min-w-0 items-start justify-between gap-3">
          <ItemContent className="min-w-0 flex-1">
            <div className="flex min-w-0 items-center gap-2">
              <ItemTitle className="min-w-0 truncate">
                {program.code}
                <span className="font-normal text-muted-foreground"> — {program.name}</span>
              </ItemTitle>
              {pending ? (
                <Badge variant="outline" className="shrink-0 border-warning/40 text-warning">
                  Pending save
                </Badge>
              ) : current ? (
                <Badge variant="outline" className="shrink-0 border-success/40 bg-success/10 text-success">
                  Saved
                </Badge>
              ) : null}
            </div>
            <ItemDescription className="min-w-0 truncate">
              {pending
                ? `${pending.name} (${formatFileSize(pending.size)}) — not saved yet`
                : current
                  ? `${current.fileName} (${formatFileSize(current.fileSize)}) — uploaded ${new Date(current.uploadedAt).toLocaleDateString()}`
                  : "No curriculum uploaded yet"}
            </ItemDescription>
          </ItemContent>

          <ItemActions className="shrink-0 gap-2">
            <Button asChild variant="outline" size="sm">
              <Label htmlFor={inputId} className="cursor-pointer">
                <Upload />
                {current || pending ? "Replace" : "Choose"}
              </Label>
            </Button>
            {current ? (
              <Button asChild variant="ghost" size="icon-sm">
                <a
                  href={CurriculumApi.fileUrl(program.code)}
                  target="_blank"
                  rel="noopener noreferrer"
                  download={current.fileName}
                >
                  <Download />
                  <span className="sr-only">Download {program.code} curriculum</span>
                </a>
              </Button>
            ) : null}
            {current ? (
              <Button variant="ghost" size="icon-sm" onClick={onDelete}>
                <Trash2 />
                <span className="sr-only">Delete {program.code} curriculum</span>
              </Button>
            ) : null}
          </ItemActions>
        </div>

        <div
          onDragOver={handleDragOver}
          onDragLeave={handleDragLeave}
          onDrop={handleDrop}
          className={cn(
            "relative mt-3 w-full min-w-0 overflow-hidden rounded-md transition-colors",
            isDragOver && "ring-2 ring-primary ring-offset-2 ring-offset-background"
          )}
        >
          {pending ? (
            <div className="flex items-center gap-3 rounded-md border border-dashed border-warning/40 bg-warning/5 p-3">
              <FileText className="size-8 shrink-0 text-warning" />
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm font-medium">{pending.name}</p>
                <p className="text-xs text-muted-foreground">
                  {formatFileSize(pending.size)} — drop another PDF to replace, or save to upload
                </p>
              </div>
              <Button variant="ghost" size="icon-sm" onClick={onClearPending}>
                <X />
                <span className="sr-only">Cancel pending {program.code} change</span>
              </Button>
            </div>
          ) : current ? (
            <div className="h-40 w-full min-w-0 overflow-hidden rounded-md border bg-muted">
              <object
                data={CurriculumApi.fileUrl(program.code)}
                type="application/pdf"
                aria-label={`${program.code} curriculum preview`}
                className="h-full w-full"
              >
                <div className="flex h-full items-center justify-center">
                  <FileText className="size-10 text-muted-foreground" />
                </div>
              </object>
            </div>
          ) : (
            <Label
              htmlFor={inputId}
              className={cn(
                "flex h-28 w-full cursor-pointer flex-col items-center justify-center gap-1 rounded-md border border-dashed p-3 text-center text-muted-foreground",
                isDragOver ? "border-primary bg-primary/5 text-primary" : "border-border"
              )}
            >
              <Upload className="size-5" />
              <span className="text-sm">Drag and drop a PDF here, or click to browse</span>
            </Label>
          )}
        </div>
      </div>
    </Item>
  );
}
