import { readSheet } from "read-excel-file/universal";

import type { Student } from "@/features/roster/roster-types";

export class RosterParseError extends Error {}

type RosterColumn =
  | "firstName"
  | "firstLastName"
  | "secondLastName"
  | "initial"
  | "personalEmail"
  | "institutionalEmail"
  | "program";

type ColumnMap = Partial<Record<RosterColumn, number>>;

const HEADER_ALIASES: Record<string, RosterColumn> = {
  firstname: "firstName",
  "first name": "firstName",
  name: "firstName",
  firstlastname: "firstLastName",
  "first lastname": "firstLastName",
  "first last name": "firstLastName",
  "father last name": "firstLastName",
  secondlastname: "secondLastName",
  "second lastname": "secondLastName",
  "second last name": "secondLastName",
  "mother last name": "secondLastName",
  initial: "initial",
  "personal email": "personalEmail",
  personalemail: "personalEmail",
  "institutional email": "institutionalEmail",
  institutionalemail: "institutionalEmail",
  "institutional account": "institutionalEmail",
  program: "program",
};

const REQUIRED_COLUMNS: RosterColumn[] = [
  "firstName",
  "firstLastName",
  "secondLastName",
  "initial",
  "program",
];

function cellToText(cell: unknown): string {
  if (cell === null || cell === undefined) {
    return "";
  }

  return String(cell).trim();
}

function detectColumns(row: string[]): ColumnMap {
  return row.reduce<ColumnMap>((columns, cell, index) => {
    const key = HEADER_ALIASES[cell.toLowerCase().trim()];
    return key ? { ...columns, [key]: index } : columns;
  }, {});
}

function parseProgram(value: string): Student["program"] | null {
  const program = value.trim().toUpperCase();

  if (program.startsWith("0502") || program === "INEL") return "INEL";
  if (program.startsWith("0507") || program === "ICOM") return "ICOM";
  if (program.includes("SOFTWARE") || program === "INSO") return "INSO";
  if (program.includes("COMPUTER SCIENCE") || program === "CIIC") return "CIIC";
  return null;
}

function buildStudent(
  id: number,
  row: string[],
  columns: Required<ColumnMap>,
  programFallback: Student["program"] | null
): Student | null {
  const firstName = row[columns.firstName]?.trim();
  const firstLastName = row[columns.firstLastName]?.trim();
  const secondLastName = row[columns.secondLastName]?.trim();
  const initial = row[columns.initial]?.trim();
  const personalEmail = row[columns.personalEmail]?.trim();
  const institutionalEmail = row[columns.institutionalEmail]?.trim();
  const program = parseProgram(row[columns.program] ?? "") ?? programFallback;

  if (!firstName || !firstLastName ||
      (!personalEmail && !institutionalEmail) || !program) {
    return null;
  }

  return {
    id,
    name: [firstName, initial, firstLastName, secondLastName]
      .filter(Boolean)
      .join(" "),
    firstName,
    firstLastName,
    secondLastName,
    initial,
    personalEmail,
    institutionalEmail,
    program,
  };
}

function parseDelimitedText(text: string): string[][] {
  return text
    .split(/\r?\n/)
    .map((line) => line.trim())
    .filter((line) => line.length > 0)
    .map((line) => line.split(",").map((cell) => cell.trim()));
}

async function readRows(file: File): Promise<string[][]> {
  if (file.name.toLowerCase().endsWith(".xlsx")) {
    const sheet = await readSheet(file);
    return sheet.map((row) => row.map(cellToText));
  }

  return parseDelimitedText(await file.text());
}

export async function parseRosterFile(file: File): Promise<Student[]> {
  const rows = await readRows(file);

  if (rows.length < 2) {
    throw new RosterParseError("The roster must include headers and students.");
  }

  const detectedColumns = detectColumns(rows[0]);
  const columns: ColumnMap = rows[0][0]?.toUpperCase() === "NOMBRE"
    ? {
        firstLastName: 0,
        secondLastName: 1,
        firstName: 2,
        initial: 3,
        personalEmail: 4,
        institutionalEmail: 5,
        program: 6,
      }
    : detectedColumns;
  const fileName = file.name.toUpperCase();
  const programFallback: Student["program"] | null = fileName.includes("ICOM")
    ? "ICOM"
    : fileName.includes("INEL") ? "INEL" : null;
  const missing = REQUIRED_COLUMNS.filter((column) => (
    columns[column] === undefined && (column !== "program" || !programFallback)
  ));

  if (missing.length ||
      (columns.personalEmail === undefined &&
       columns.institutionalEmail === undefined)) {
    throw new RosterParseError(
      `Missing required columns: ${missing.join(", ") || "email"}.`
    );
  }

  const students = rows.slice(1)
    .map((row, index) => buildStudent(
      index + 1,
      row,
      columns as Required<ColumnMap>,
      programFallback
    ))
    .filter((student): student is Student => student !== null);

  if (!students.length) {
    throw new RosterParseError("No valid students were found in this file.");
  }

  return students;
}
