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
  email: "institutionalEmail",
  firstname: "firstName",
  "first name": "firstName",
  first_name: "firstName",
  name: "firstName",
  firstlastname: "firstLastName",
  "first lastname": "firstLastName",
  "first last name": "firstLastName",
  first_last_name: "firstLastName",
  "father last name": "firstLastName",
  secondlastname: "secondLastName",
  "second lastname": "secondLastName",
  "second last name": "secondLastName",
  second_last_name: "secondLastName",
  "mother last name": "secondLastName",
  initial: "initial",
  "personal email": "personalEmail",
  personalemail: "personalEmail",
  "institutional email": "institutionalEmail",
  institutionalemail: "institutionalEmail",
  "institutional account": "institutionalEmail",
  program: "program",
};

const EO_HEADER_ALIASES: Record<string, RosterColumn> = {
  email: "institutionalEmail",
  first_name: "firstName",
  first_last_name: "firstLastName",
  second_last_name: "secondLastName",
  initial: "initial",
  program: "program",
};

const REQUIRED_COLUMNS: RosterColumn[] = [
  "firstName",
  "firstLastName",
  "secondLastName",
  "initial",
];

function cellToText(cell: unknown): string {
  if (cell === null || cell === undefined) {
    return "";
  }

  return String(cell).trim();
}

function detectColumns(
  row: string[],
  aliases: Record<string, RosterColumn> = HEADER_ALIASES
): ColumnMap {
  return row.reduce<ColumnMap>((columns, cell, index) => {
    const key = aliases[cell.toLowerCase().trim()];
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
  columns: ColumnMap,
  isEO: boolean
): Student | null {
  function value(column: RosterColumn): string {
    const index = columns[column];
    return index === undefined ? "" : row[index]?.trim() ?? "";
  }

  const firstName = value("firstName");
  const firstLastName = value("firstLastName");
  const secondLastName = value("secondLastName");
  const initial = value("initial");
  const personalEmail = value("personalEmail");
  const institutionalEmail = value("institutionalEmail");
  const program = isEO ? parseProgram(value("program")) : null;

  if (!firstName || !firstLastName ||
      (!personalEmail && !institutionalEmail)) {
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

export async function parseRosterFile(
  file: File,
  isEO = false
): Promise<Student[]> {
  const rows = await readRows(file);

  if (rows.length < 2) {
    throw new RosterParseError("The roster must include headers and students.");
  }

  const detectedColumns = detectColumns(
    rows[0],
    isEO ? EO_HEADER_ALIASES : HEADER_ALIASES
  );
  const columns: ColumnMap = !isEO && rows[0][0]?.toUpperCase() === "NOMBRE"
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
  const requiredColumns: RosterColumn[] = isEO
    ? [...REQUIRED_COLUMNS, "program"]
    : REQUIRED_COLUMNS;
  const missing = requiredColumns.filter((column) => (
    columns[column] === undefined
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
      columns,
      isEO
    ))
    .filter((student): student is Student => student !== null);

  if (!students.length) {
    throw new RosterParseError("No valid students were found in this file.");
  }

  return students;
}
