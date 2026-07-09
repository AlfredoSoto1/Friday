"use client";

import { useState } from "react";
import { CheckCircle2, Loader2, TriangleAlert, Upload } from "lucide-react";

import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { NativeSelect, NativeSelectOption } from "@/components/ui/native-select";
import type {
  CsvImportResultDto,
  InelicomCsvImportKind,
} from "@/server/entities/inelicom";
import { InelicomApi } from "@/server/webservices/inelicom-webservice";

const importOptions: Array<{
  kind: InelicomCsvImportKind;
  label: string;
  columns: string;
}> = [
  {
    kind: "buildings",
    label: "Google pins / rooms",
    columns: "code, name, gpin",
  },
  {
    kind: "faculties",
    label: "Faculty profiles",
    columns: "ext, web, phone, facebook, email, office, name, job_entitlement, description, abreviation, instagram",
  },
  {
    kind: "projects",
    label: "Projects",
    columns: "web, facebook, instagram, email, name, description",
  },
  {
    kind: "organizations",
    label: "Organizations",
    columns: "name, description, email, facebook, instagram, twitter_x, web",
  },
];

export function CsvImportManager(): React.ReactElement {
  const [kind, setKind] = useState<InelicomCsvImportKind>("buildings");
  const [file, setFile] = useState<File | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [result, setResult] = useState<CsvImportResultDto | null>(null);

  const selected = importOptions.find((option) => option.kind === kind)
    ?? importOptions[0];

  async function submitImport(event: React.FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();
    if (!file) {
      setError("Choose a CSV file before importing.");
      setResult(null);
      return;
    }

    setLoading(true);
    setError("");
    setResult(null);

    const response = await InelicomApi.importCsv(kind, file);
    if (response.isFailure) {
      setError(response.error.message);
      setLoading(false);
      return;
    }

    setResult(response.value);
    setLoading(false);
  }

  return (
    <div className="grid min-w-0 gap-6 lg:grid-cols-[minmax(0,1fr)_22rem]">
      <Card className="rounded-md border-border bg-card shadow-panel">
        <CardHeader>
          <CardTitle>Import dataset</CardTitle>
          <CardDescription>
            Upload content CSVs for Google pins/rooms, projects, organizations, or faculty.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form className="grid gap-5" onSubmit={submitImport}>
            <label className="grid gap-2 text-sm font-medium">
              Dataset
              <NativeSelect
                className="w-full"
                value={kind}
                onChange={(event) => (
                  setKind(event.target.value as InelicomCsvImportKind)
                )}
              >
                {importOptions.map((option) => (
                  <NativeSelectOption key={option.kind} value={option.kind}>
                    {option.label}
                  </NativeSelectOption>
                ))}
              </NativeSelect>
            </label>
            <label className="grid gap-2 text-sm font-medium">
              CSV file
              <Input
                type="file"
                accept=".csv,text/csv"
                onChange={(event) => setFile(event.target.files?.[0] ?? null)}
              />
            </label>
            <Button className="w-fit" type="submit" disabled={loading}>
              {loading ? <Loader2 className="animate-spin" /> : <Upload />}
              Import dataset
            </Button>
          </form>
        </CardContent>
      </Card>

      <div className="grid content-start gap-4">
        <Card className="rounded-md border-border bg-card shadow-panel">
          <CardHeader>
            <CardTitle className="text-base">Expected columns</CardTitle>
            <CardDescription>{selected.label}</CardDescription>
          </CardHeader>
          <CardContent>
            <p className="break-words text-sm text-muted-foreground">
              {selected.columns}
            </p>
          </CardContent>
        </Card>

        {error ? (
          <Alert variant="destructive">
            <TriangleAlert />
            <AlertTitle>Import failed</AlertTitle>
            <AlertDescription>{error}</AlertDescription>
          </Alert>
        ) : null}

        {result ? (
          <Alert>
            <CheckCircle2 />
            <AlertTitle>Import complete</AlertTitle>
            <AlertDescription>
              {result.inserted} inserted, {result.updated} updated, {result.skipped} skipped.
              {result.errors.length > 0 ? ` ${result.errors.join(" ")}` : ""}
            </AlertDescription>
          </Alert>
        ) : null}
      </div>
    </div>
  );
}
