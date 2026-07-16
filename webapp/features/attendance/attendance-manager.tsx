"use client";

import { useMemo, useState } from "react";
import {
  CheckCircle2,
  Download,
  FileSpreadsheet,
  TriangleAlert,
} from "lucide-react";

import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@/components/ui/input";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

const weekdays = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday"] as const;
type Weekday = typeof weekdays[number];

interface CsvRecord {
  row: string[];
  email: string;
}

interface ParsedCsv {
  fileName: string;
  fileNames: string[];
  headers: string[];
  records: CsvRecord[];
  emailColumnIndex: number | null;
}

interface AttendanceResult {
  present: CsvRecord[];
  missing: CsvRecord[];
  unknown: CsvRecord[];
}

function parseCsvText(text: string): string[][] {
  const rows: string[][] = [];
  let row: string[] = [];
  let field = "";
  let inQuotes = false;

  for (let index = 0; index < text.length; index++) {
    const current = text[index];

    if (current === '"') {
      if (inQuotes && text[index + 1] === '"') {
        field += '"';
        index++;
      } else {
        inQuotes = !inQuotes;
      }
    } else if (current === "," && !inQuotes) {
      row.push(field.trim());
      field = "";
    } else if ((current === "\n" || current === "\r") && !inQuotes) {
      if (current === "\r" && text[index + 1] === "\n") index++;
      row.push(field.trim());
      if (row.some((value) => value.length > 0)) rows.push(row);
      row = [];
      field = "";
    } else {
      field += current;
    }
  }

  if (field.length > 0 || row.length > 0) {
    row.push(field.trim());
    if (row.some((value) => value.length > 0)) rows.push(row);
  }

  return rows;
}

const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

function emailFromRow(row: string[], emailColumnIndex: number | null): string {
  const detectedValue = emailColumnIndex !== null ? row[emailColumnIndex] ?? "" : "";
  if (emailPattern.test(detectedValue.trim())) {
    return normalizeEmail(detectedValue);
  }

  return normalizeEmail(
    row.find((value) => emailPattern.test(value.trim())) ?? ""
  );
}

function normalizeEmail(value: string): string {
  return value.trim().toLowerCase();
}

function detectEmailColumn(headers: string[]): number | null {
  const index = headers.findIndex((header) => {
    const normalized = header
      .toLowerCase()
      .normalize("NFD")
      .replace(/[\u0300-\u036f]/g, "")
      .replace(/[_-]/g, " ")
      .trim();
    return normalized === "email" ||
      normalized.includes("email") ||
      normalized.includes("correo") ||
      normalized.includes("account") ||
      normalized.includes("cuenta");
  });

  return index >= 0 ? index : null;
}

async function parseCsvFile(file: File, allowEmpty = false): Promise<ParsedCsv> {
  const rows = parseCsvText(await file.text());
  if (rows.length < 1 || (!allowEmpty && rows.length < 2)) {
    throw new Error(`${file.name} must include headers and at least one student.`);
  }

  const headers = rows[0];
  const emailColumnIndex = detectEmailColumn(headers);
  const records = rows.slice(1)
    .map((row) => ({ row, email: emailFromRow(row, emailColumnIndex) }))
    .filter((record) => record.email.length > 0);

  if (!records.length && !allowEmpty) {
    throw new Error(`${file.name} does not include any rows with a valid email.`);
  }

  return { fileName: file.name, fileNames: [file.name], headers, records, emailColumnIndex };
}

function csvEscape(value: string): string {
  if (/[",\n\r]/.test(value)) {
    return `"${value.replaceAll('"', '""')}"`;
  }

  return value;
}

function downloadFile(fileName: string, content: string, type: string): void {
  downloadBlob(fileName, new Blob([content], { type }));
}

function downloadBlob(fileName: string, blob: Blob): void {
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName;
  link.click();
  URL.revokeObjectURL(url);
}

function combineMasterCsvs(files: ParsedCsv[]): ParsedCsv {
  const [first, ...rest] = files;
  return {
    fileName: files.map((file) => file.fileName).join(", "),
    fileNames: files.flatMap((file) => file.fileNames),
    headers: first.headers,
    emailColumnIndex: first.emailColumnIndex,
    records: [first, ...rest].flatMap((file) => file.records),
  };
}

function alignUnknownSubsetRow(master: ParsedCsv, record: CsvRecord): string[] {
  if (record.row.length === master.headers.length) {
    return record.row;
  }

  const row = Array.from({ length: master.headers.length }, () => "");
  if (master.emailColumnIndex !== null) {
    row[master.emailColumnIndex] = record.email;
  }

  return row;
}

function buildMissingCsv(
  master: ParsedCsv,
  missing: CsvRecord[],
  unknown: CsvRecord[]
): string {
  const unknownRows = unknown.map((record) => alignUnknownSubsetRow(master, record));
  const lines = [master.headers, ...missing.map((record) => record.row), ...unknownRows];
  return lines
    .map((row) => master.headers.map((_, index) => csvEscape(row[index] ?? "")).join(","))
    .join("\n");
}

function xmlEscape(value: string): string {
  return value
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;");
}

function columnName(index: number): string {
  let name = "";
  let value = index + 1;

  while (value > 0) {
    const remainder = (value - 1) % 26;
    name = String.fromCharCode(65 + remainder) + name;
    value = Math.floor((value - 1) / 26);
  }

  return name;
}

function worksheetCell(value: string, rowIndex: number, columnIndex: number): string {
  const reference = `${columnName(columnIndex)}${rowIndex}`;
  return `<c r="${reference}" t="inlineStr"><is><t>${xmlEscape(value)}</t></is></c>`;
}

function buildWorksheet(rows: string[][]): string {
  const rowXml = rows.map((row, rowIndex) => {
    const excelRow = rowIndex + 1;
    const cells = row.map((cell, columnIndex) => worksheetCell(cell, excelRow, columnIndex)).join("");
    return `<row r="${excelRow}">${cells}</row>`;
  }).join("");

  return `<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetData>${rowXml}</sheetData></worksheet>`;
}

const crcTable = Array.from({ length: 256 }, (_, index) => {
  let value = index;
  for (let bit = 0; bit < 8; bit++) {
    value = value & 1 ? 0xedb88320 ^ (value >>> 1) : value >>> 1;
  }
  return value >>> 0;
});

function crc32(data: Uint8Array): number {
  let crc = 0xffffffff;
  for (const byte of data) {
    crc = crcTable[(crc ^ byte) & 0xff] ^ (crc >>> 8);
  }
  return (crc ^ 0xffffffff) >>> 0;
}

function writeUint16(target: Uint8Array, offset: number, value: number): void {
  target[offset] = value & 0xff;
  target[offset + 1] = (value >>> 8) & 0xff;
}

function writeUint32(target: Uint8Array, offset: number, value: number): void {
  target[offset] = value & 0xff;
  target[offset + 1] = (value >>> 8) & 0xff;
  target[offset + 2] = (value >>> 16) & 0xff;
  target[offset + 3] = (value >>> 24) & 0xff;
}

function concatArrays(parts: Uint8Array[]): Uint8Array {
  const total = parts.reduce((sum, part) => sum + part.length, 0);
  const output = new Uint8Array(total);
  let offset = 0;

  for (const part of parts) {
    output.set(part, offset);
    offset += part.length;
  }

  return output;
}

function createZip(files: Array<{ name: string; content: string }>): Uint8Array {
  const encoder = new TextEncoder();
  const localParts: Uint8Array[] = [];
  const centralParts: Uint8Array[] = [];
  let offset = 0;

  for (const file of files) {
    const nameBytes = encoder.encode(file.name);
    const contentBytes = encoder.encode(file.content);
    const checksum = crc32(contentBytes);
    const local = new Uint8Array(30 + nameBytes.length);

    writeUint32(local, 0, 0x04034b50);
    writeUint16(local, 4, 20);
    writeUint16(local, 6, 0);
    writeUint16(local, 8, 0);
    writeUint16(local, 10, 0);
    writeUint16(local, 12, 0);
    writeUint32(local, 14, checksum);
    writeUint32(local, 18, contentBytes.length);
    writeUint32(local, 22, contentBytes.length);
    writeUint16(local, 26, nameBytes.length);
    writeUint16(local, 28, 0);
    local.set(nameBytes, 30);
    localParts.push(local, contentBytes);

    const central = new Uint8Array(46 + nameBytes.length);
    writeUint32(central, 0, 0x02014b50);
    writeUint16(central, 4, 20);
    writeUint16(central, 6, 20);
    writeUint16(central, 8, 0);
    writeUint16(central, 10, 0);
    writeUint16(central, 12, 0);
    writeUint16(central, 14, 0);
    writeUint32(central, 16, checksum);
    writeUint32(central, 20, contentBytes.length);
    writeUint32(central, 24, contentBytes.length);
    writeUint16(central, 28, nameBytes.length);
    writeUint16(central, 30, 0);
    writeUint16(central, 32, 0);
    writeUint16(central, 34, 0);
    writeUint16(central, 36, 0);
    writeUint32(central, 38, 0);
    writeUint32(central, 42, offset);
    central.set(nameBytes, 46);
    centralParts.push(central);

    offset += local.length + contentBytes.length;
  }

  const centralDirectory = concatArrays(centralParts);
  const end = new Uint8Array(22);
  writeUint32(end, 0, 0x06054b50);
  writeUint16(end, 4, 0);
  writeUint16(end, 6, 0);
  writeUint16(end, 8, files.length);
  writeUint16(end, 10, files.length);
  writeUint32(end, 12, centralDirectory.length);
  writeUint32(end, 16, offset);
  writeUint16(end, 20, 0);

  return concatArrays([...localParts, centralDirectory, end]);
}

function buildPresentWorkbook(present: CsvRecord[], selectedDays: Weekday[]): Blob {
  const daySet = new Set(selectedDays);
  const rows = [
    ["Email", ...weekdays],
    ...present.map((record) => [
      record.email,
      ...weekdays.map((day) => daySet.has(day) ? "✓" : ""),
    ]),
  ];
  const workbook = createZip([
    {
      name: "[Content_Types].xml",
      content: '<?xml version="1.0" encoding="UTF-8" standalone="yes"?><Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="xml" ContentType="application/xml"/><Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/><Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/></Types>',
    },
    {
      name: "_rels/.rels",
      content: '<?xml version="1.0" encoding="UTF-8" standalone="yes"?><Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/></Relationships>',
    },
    {
      name: "xl/workbook.xml",
      content: '<?xml version="1.0" encoding="UTF-8" standalone="yes"?><workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><sheets><sheet name="Present" sheetId="1" r:id="rId1"/></sheets></workbook>',
    },
    {
      name: "xl/_rels/workbook.xml.rels",
      content: '<?xml version="1.0" encoding="UTF-8" standalone="yes"?><Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/></Relationships>',
    },
    { name: "xl/worksheets/sheet1.xml", content: buildWorksheet(rows) },
  ]);

  return new Blob([workbook], {
    type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
  });
}

export function AttendanceManager(): React.ReactElement {
  const [master, setMaster] = useState<ParsedCsv | null>(null);
  const [subset, setSubset] = useState<ParsedCsv | null>(null);
  const [selectedDays, setSelectedDays] = useState<Weekday[]>([]);
  const [error, setError] = useState("");

  const result = useMemo<AttendanceResult | null>(() => {
    if (!master || !subset) return null;

    const subsetByEmail = new Map(subset.records.map((record) => [record.email, record]));
    const masterByEmail = new Map(master.records.map((record) => [record.email, record]));
    const present = master.records.filter((record) => subsetByEmail.has(record.email));
    const missing = master.records.filter((record) => !subsetByEmail.has(record.email));
    const unknown = subset.records.filter((record) => !masterByEmail.has(record.email));

    return { present, missing, unknown };
  }, [master, subset]);

  async function handleMasterFiles(fileList?: FileList | null): Promise<void> {
    const files = Array.from(fileList ?? []);
    if (!files.length) return;
    if (files.some((file) => !file.name.toLowerCase().endsWith(".csv"))) {
      setError("Upload CSV files only.");
      return;
    }

    try {
      setMaster(combineMasterCsvs(await Promise.all(files.map((file) => parseCsvFile(file)))));
      setError("");
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : "Could not parse CSV files.");
    }
  }

  async function handleSubsetFile(file?: File | null): Promise<void> {
    if (!file) return;
    if (!file.name.toLowerCase().endsWith(".csv")) {
      setError("Upload CSV files only.");
      return;
    }

    try {
      setSubset(await parseCsvFile(file, true));
      setError("");
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : "Could not parse CSV file.");
    }
  }

  function toggleDay(day: Weekday, checked: boolean): void {
    setSelectedDays((current) => (
      checked ? [...current, day] : current.filter((item) => item !== day)
    ));
  }

  function downloadMissing(): void {
    if (!master || !result) return;
    downloadFile(
      "missing-students.csv",
      buildMissingCsv(master, result.missing, result.unknown),
      "text/csv;charset=utf-8"
    );
  }

  function downloadPresent(): void {
    if (!result) return;
    downloadBlob(
      "present-students.xlsx",
      buildPresentWorkbook(result.present, selectedDays)
    );
  }

  return (
    <div className="grid min-w-0 gap-6">
      {error ? (
        <Alert variant="destructive">
          <TriangleAlert />
          <AlertTitle>Attendance import failed</AlertTitle>
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      ) : null}

      <div className="grid gap-6 lg:grid-cols-2">
        <Card className="rounded-md border-border bg-card shadow-panel">
          <CardHeader>
            <CardTitle>Master student list</CardTitle>
            <CardDescription>Upload one or more complete CSV lists, such as ICOM and INEL.</CardDescription>
          </CardHeader>
          <CardContent className="grid gap-3">
            <Input
              type="file"
              accept=".csv,text/csv"
              multiple
              onChange={(event) => void handleMasterFiles(event.target.files)}
            />
            {master ? (
              <Badge variant="secondary" className="w-fit">
                <FileSpreadsheet />
                {master.records.length} students from {master.fileNames.length} file{master.fileNames.length === 1 ? "" : "s"}
              </Badge>
            ) : null}
          </CardContent>
        </Card>

        <Card className="rounded-md border-border bg-card shadow-panel">
          <CardHeader>
            <CardTitle>Attendance subset</CardTitle>
            <CardDescription>Upload the CSV list of students present for the selected days.</CardDescription>
          </CardHeader>
          <CardContent className="grid gap-3">
            <Input
              type="file"
              accept=".csv,text/csv"
              onChange={(event) => void handleSubsetFile(event.target.files?.[0])}
            />
            {subset ? (
              <Badge variant="secondary" className="w-fit">
                <FileSpreadsheet />
                {subset.records.length} students from {subset.fileName}
              </Badge>
            ) : null}
          </CardContent>
        </Card>
      </div>

      <Card className="rounded-md border-border bg-card shadow-panel">
        <CardHeader>
          <CardTitle>Attendance days</CardTitle>
          <CardDescription>Selected days are marked in the present-students Excel export.</CardDescription>
        </CardHeader>
        <CardContent className="flex flex-wrap gap-4">
          {weekdays.map((day) => (
            <label key={day} className="flex items-center gap-2 text-sm font-medium">
              <Checkbox
                checked={selectedDays.includes(day)}
                onCheckedChange={(checked) => toggleDay(day, checked === true)}
              />
              {day}
            </label>
          ))}
        </CardContent>
      </Card>

      {result ? (
        <div className="grid gap-6 lg:grid-cols-[22rem_minmax(0,1fr)]">
          <Card className="rounded-md border-border bg-card shadow-panel">
            <CardHeader>
              <CardTitle>Summary</CardTitle>
              <CardDescription>Intersection between the master list and attendance subset.</CardDescription>
            </CardHeader>
            <CardContent className="grid gap-4">
              <div className="grid grid-cols-2 gap-3">
                <div className="rounded-md border bg-background/40 p-3">
                  <p className="text-xs text-muted-foreground">Present</p>
                  <p className="text-2xl font-semibold">{result.present.length}</p>
                </div>
                <div className="rounded-md border bg-background/40 p-3">
                  <p className="text-xs text-muted-foreground">Missing</p>
                  <p className="text-2xl font-semibold">{result.missing.length}</p>
                </div>
              </div>
              {result.unknown.length ? (
                <Alert>
                  <TriangleAlert />
                  <AlertTitle>Unmatched subset rows</AlertTitle>
                  <AlertDescription>
                    {result.unknown.length} attendance rows were not found in the master list.
                  </AlertDescription>
                </Alert>
              ) : (
                <Alert>
                  <CheckCircle2 />
                  <AlertTitle>Subset matched</AlertTitle>
                  <AlertDescription>Every attendance row was found in the master list.</AlertDescription>
                </Alert>
              )}
              <div className="flex flex-wrap gap-2">
                <Button variant="outline" onClick={downloadMissing}>
                  <Download />
                  Missing CSV
                </Button>
                <Button onClick={downloadPresent}>
                  <Download />
                  Present Excel
                </Button>
              </div>
            </CardContent>
          </Card>

          <Card className="min-w-0 rounded-md border-border bg-card shadow-panel">
            <CardHeader>
              <CardTitle>Present students</CardTitle>
              <CardDescription>First 25 intersected students by email.</CardDescription>
            </CardHeader>
            <CardContent>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Email</TableHead>
                    {weekdays.map((day) => (
                      <TableHead key={day}>{day}</TableHead>
                    ))}
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {result.present.slice(0, 25).map((record) => (
                    <TableRow key={record.email}>
                      <TableCell>{record.email}</TableCell>
                      {weekdays.map((day) => (
                        <TableCell key={day}>{selectedDays.includes(day) ? "✓" : ""}</TableCell>
                      ))}
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </div>
      ) : null}
    </div>
  );
}
