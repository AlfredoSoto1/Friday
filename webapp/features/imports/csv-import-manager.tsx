"use client";

import { useState } from "react";
import { CheckCircle2, FileSpreadsheet, Loader2, TriangleAlert, Upload, X } from "lucide-react";

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
import { Input } from "@/components/ui/input";
import { Item, ItemActions, ItemContent, ItemDescription, ItemMedia, ItemTitle } from "@/components/ui/item";
import { Label } from "@/components/ui/label";
import { NativeSelect, NativeSelectOption } from "@/components/ui/native-select";
import { Spinner } from "@/components/ui/spinner";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import type { CsvImportResultDto, InelicomCsvImportKind } from "@/server/entities/inelicom";
import { InelicomApi } from "@/server/webservices/inelicom-webservice";

const ACCEPTED_EXTENSIONS = [".csv"];
const importTargets: Array<{ kind: InelicomCsvImportKind; label: string }> = [
  { kind: "buildings", label: "Buildings" },
  { kind: "contacts", label: "Contacts" },
  { kind: "faculties", label: "Faculties" },
  { kind: "projects", label: "Projects" },
  { kind: "organizations", label: "Organizations" },
];

interface UploadedCsvTable {
  fileName: string;
  headers: string[];
  rows: string[][];
}

export function CsvImportManager(): React.ReactElement {
  const [kind, setKind] = useState<InelicomCsvImportKind>("faculties");
  const [uploading, setUploading] = useState(false);
  const [dragActive, setDragActive] = useState(false);
  const [file, setFile] = useState<File | null>(null);
  const [error, setError] = useState("");
  const [csvResult, setCsvResult] = useState<CsvImportResultDto | null>(null);
  const [uploadedCsv, setUploadedCsv] = useState<UploadedCsvTable | null>(null);

  function handleKindChange(nextKind: InelicomCsvImportKind): void {
    setKind(nextKind);
    setCsvResult(null);
    setUploadedCsv(null);
    setError("");
  }

  function handleFile(candidate?: File | null): void {
    if (!candidate) {
      return;
    }

    if (!isAcceptedFile(candidate)) {
      setError("Upload a CSV file.");
      setFile(null);
      setCsvResult(null);
      setUploadedCsv(null);
      return;
    }

    setError("");
    setCsvResult(null);
    setUploadedCsv(null);
    setFile(candidate);
  }

  async function submitCsvImport(event: React.FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();
    if (!file) {
      setError("Choose a CSV file to import.");
      return;
    }

    setUploading(true);
    setError("");
    setCsvResult(null);
    setUploadedCsv(null);

    let parsedCsv: UploadedCsvTable;
    try {
      parsedCsv = parseCsv(await file.text(), file.name);
    } catch (parseError) {
      setError(parseError instanceof Error ? parseError.message : "Unable to parse CSV file.");
      setUploading(false);
      return;
    }

    const response = await InelicomApi.importCsv(kind, file);
    if (response.isFailure) {
      setError(response.error.message);
      setUploading(false);
      return;
    }

    setCsvResult(response.value);
    setUploadedCsv(parsedCsv);
    setUploading(false);
  }

  return (
    <div className="grid min-w-0 gap-6">
      <Card className="rounded-md border-border bg-card shadow-panel">
        <CardHeader>
          <CardTitle>Import CSV</CardTitle>
          <CardDescription>
            Upload a CSV for buildings, contacts, faculties, projects, or organizations.
          </CardDescription>
          <CardAction>
            <FileSpreadsheet className="size-5 text-muted-foreground" />
          </CardAction>
        </CardHeader>
        <CardContent>
          <form className="grid gap-5" onSubmit={submitCsvImport}>
            <div className="grid gap-5 md:grid-cols-[minmax(16rem,24rem)_1fr]">
              <label className="grid h-fit gap-2 text-sm font-medium">
                Import target
                <NativeSelect
                  className="w-full"
                  value={kind}
                  disabled={uploading}
                  onChange={(event) => {
                    handleKindChange(event.target.value as InelicomCsvImportKind);
                  }}
                >
                  {importTargets.map((target) => (
                    <NativeSelectOption key={target.kind} value={target.kind}>
                      {target.label}
                    </NativeSelectOption>
                  ))}
                </NativeSelect>
              </label>

              <div className="min-w-0">
                <Input
                  id="csv-file-input"
                  type="file"
                  accept=".csv,text/csv"
                  className="hidden"
                  disabled={uploading}
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
                      {uploading ? (
                        <Badge variant="secondary">
                          <Spinner />
                          Importing
                        </Badge>
                      ) : null}
                      <Button asChild variant="outline" size="sm" disabled={uploading}>
                        <Label htmlFor="csv-file-input" className="cursor-pointer">
                          <Upload />
                          Replace
                        </Label>
                      </Button>
                      <Button
                        type="button"
                        variant="ghost"
                        size="icon-sm"
                        onClick={() => {
                          setFile(null);
                          setCsvResult(null);
                          setUploadedCsv(null);
                        }}
                        disabled={uploading}
                      >
                        <X />
                        <span className="sr-only">Remove file</span>
                      </Button>
                    </ItemActions>
                  </Item>
                ) : (
                  <Label
                    htmlFor="csv-file-input"
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
                    <span className="text-sm font-medium">Click to upload or drag a CSV here</span>
                    <span className="text-xs text-muted-foreground">CSV files only</span>
                  </Label>
                )}
              </div>
            </div>

            <Button className="w-fit" type="submit" disabled={uploading || !file}>
              {uploading ? <Loader2 className="animate-spin" /> : <Upload />}
              Import CSV
            </Button>
          </form>
        </CardContent>
      </Card>

      {error ? (
        <Alert variant="destructive">
          <TriangleAlert />
          <AlertTitle>Import failed</AlertTitle>
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      ) : null}

      {csvResult ? (
        <Alert>
          <CheckCircle2 />
          <AlertTitle>CSV import complete</AlertTitle>
          <AlertDescription>
            {csvResult.inserted} inserted, {csvResult.updated} updated, and {csvResult.skipped} skipped from {csvResult.fileName}.
            {csvResult.errors.length > 0 ? ` ${csvResult.errors.join(" ")}` : ""}
          </AlertDescription>
        </Alert>
      ) : null}

      {uploadedCsv ? (
        <Card className="min-w-0 rounded-md border-border bg-card shadow-panel">
          <CardHeader>
            <CardTitle className="text-base">Uploaded data</CardTitle>
            <CardDescription>
              {uploadedCsv.rows.length} rows from {uploadedCsv.fileName}.
            </CardDescription>
          </CardHeader>
          <CardContent className="min-w-0 overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  {uploadedCsv.headers.map((header, index) => (
                    <TableHead key={`${header}-${index}`} className="min-w-40">
                      {header || `Column ${index + 1}`}
                    </TableHead>
                  ))}
                </TableRow>
              </TableHeader>
              <TableBody>
                {uploadedCsv.rows.length > 0 ? (
                  uploadedCsv.rows.map((row, rowIndex) => (
                    <TableRow key={`${uploadedCsv.fileName}-${rowIndex}`}>
                      {uploadedCsv.headers.map((_, columnIndex) => (
                        <TableCell key={columnIndex} className="max-w-96 whitespace-normal break-words align-top">
                          {cellValue(row[columnIndex])}
                        </TableCell>
                      ))}
                    </TableRow>
                  ))
                ) : (
                  <TableRow>
                    <TableCell className="h-24 text-center text-muted-foreground" colSpan={uploadedCsv.headers.length}>
                      No uploaded rows to show.
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      ) : null}
    </div>
  );
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

function cellValue(value?: string): string {
  if (value === undefined || value.trim() === "") {
    return "-";
  }

  return value;
}

function parseCsv(csv: string, fileName: string): UploadedCsvTable {
  const records: string[][] = [];
  let row: string[] = [];
  let field = "";
  let inQuotes = false;

  for (let index = 0; index < csv.length; index++) {
    const current = csv[index];
    if (current === '"') {
      if (inQuotes && csv[index + 1] === '"') {
        field += '"';
        index++;
      } else {
        inQuotes = !inQuotes;
      }
    } else if (current === "," && !inQuotes) {
      row.push(field);
      field = "";
    } else if ((current === "\n" || current === "\r") && !inQuotes) {
      if (current === "\r" && csv[index + 1] === "\n") index++;
      row.push(field);
      field = "";
      if (row.some((value) => value.trim() !== "")) records.push(row);
      row = [];
    } else {
      field += current;
    }
  }

  if (inQuotes) {
    throw new Error("CSV has an unterminated quoted field.");
  }

  if (field.length > 0 || row.length > 0) {
    row.push(field);
    if (row.some((value) => value.trim() !== "")) records.push(row);
  }

  if (records.length === 0) {
    throw new Error("CSV file is empty.");
  }

  const headers = records[0].map((header) => header.trim());

  if (!headers.some((header) => header !== "")) {
    throw new Error("CSV is missing a header row.");
  }

  return {
    fileName,
    headers,
    rows: records.slice(1),
  };
}
