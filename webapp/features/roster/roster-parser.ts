import type { Student } from "@/features/roster/roster-types";

export class RosterParseError extends Error {}

type RosterColumn =
  | "firstName"
  | "firstLastName"
  | "secondLastName"
  | "initial"
  | "personalEmail"
  | "institutionalEmail";

type ColumnMap = Partial<Record<RosterColumn, number>>;

const HEADER_ALIASES: Record<string, RosterColumn> = {
  email: "institutionalEmail",
  first_name: "firstName",
  name: "firstName",
  first_last_name: "firstLastName",
  "father last name": "firstLastName",
  second_last_name: "secondLastName",
  "mother last name": "secondLastName",
  initial: "initial",
  "personal email": "personalEmail",
  "institutional email": "institutionalEmail",
  "institutional account": "institutionalEmail",
};

const LEGACY_PREPA_COLUMNS: ColumnMap = {
  firstLastName: 0,
  secondLastName: 1,
  firstName: 2,
  initial: 3,
  personalEmail: 4,
  institutionalEmail: 5,
};

const REQUIRED_COLUMNS: RosterColumn[] = [
  "firstName",
  "firstLastName",
  "secondLastName",
  "initial",
];

function normalizeHeader(value: string): string {
  return value.replace(/^\uFEFF/, "").trim().toLowerCase();
}

function parseCsv(text: string): string[][] {
  const rows: string[][] = [];
  let row: string[] = [];
  let field = "";
  let inQuotes = false;

  for (let index = 0; index < text.length; index += 1) {
    const character = text[index];

    if (character === '"') {
      if (inQuotes && text[index + 1] === '"') {
        field += '"';
        index += 1;
      } else {
        inQuotes = !inQuotes;
      }
    } else if (character === "," && !inQuotes) {
      row.push(field.trim());
      field = "";
    } else if ((character === "\n" || character === "\r") && !inQuotes) {
      if (character === "\r" && text[index + 1] === "\n") {
        index += 1;
      }
      row.push(field.trim());
      if (row.some(Boolean)) rows.push(row);
      row = [];
      field = "";
    } else {
      field += character;
    }
  }

  if (inQuotes) {
    throw new RosterParseError("The CSV contains an unterminated quoted field.");
  }

  row.push(field.trim());
  if (row.some(Boolean)) rows.push(row);
  return rows;
}

function detectColumns(header: string[]): ColumnMap | null {
  const normalized = header.map(normalizeHeader);
  const isLegacyPrepaLayout = normalized[0] === "nombre" &&
    normalized[4] === "email" &&
    normalized[5] === "inst email";

  if (isLegacyPrepaLayout) return LEGACY_PREPA_COLUMNS;

  const columns = normalized.reduce<ColumnMap>((detected, cell, index) => {
    const column = HEADER_ALIASES[cell];
    return column ? { ...detected, [column]: index } : detected;
  }, {});
  const hasNames = REQUIRED_COLUMNS.every((column) => (
    columns[column] !== undefined
  ));
  const hasEmail = columns.personalEmail !== undefined ||
    columns.institutionalEmail !== undefined;

  return hasNames && hasEmail ? columns : null;
}

function buildStudent(
  id: number,
  row: string[],
  columns: ColumnMap
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
  };
}

export async function parseRosterFile(file: File): Promise<Student[]> {
  if (!file.name.toLowerCase().endsWith(".csv")) {
    throw new RosterParseError("Upload a CSV file.");
  }

  const rows = parseCsv(await file.text());
  if (rows.length < 2) {
    throw new RosterParseError("The roster must include headers and students.");
  }

  const columns = detectColumns(rows[0]);
  if (!columns) {
    throw new RosterParseError(
      "Use the Lista_EO.csv or LISTA DE PREPAS CSV column layout."
    );
  }

  const students = rows.slice(1)
    .map((row, index) => buildStudent(index + 1, row, columns))
    .filter((student): student is Student => student !== null);

  if (!students.length) {
    throw new RosterParseError("No valid students were found in this file.");
  }

  return students;
}
