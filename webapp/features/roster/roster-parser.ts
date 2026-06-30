import { readSheet } from "read-excel-file/universal";

import type { Student } from "@/features/roster/roster-types";

export class RosterParseError extends Error {}

const MAJORS = ["ICOM", "INEL", "INSO", "CIIC", "Civil", "Mechanical", "Industrial"] as const;
const YEARS: Student["year"][] = ["Freshman", "Sophomore", "Junior", "Senior"];

const HEADER_ALIASES: Record<string, keyof Student> = {
  name: "name",
  "full name": "name",
  "student name": "name",
  "student id": "studentId",
  "studentid": "studentId",
  id: "studentId",
  major: "major",
  program: "major",
  department: "major",
  year: "year",
  "class year": "year",
  "academic year": "year",
  gpa: "gpa",
};

type ColumnMap = Partial<Record<keyof Student, number>>;

function randomItem<T>(items: readonly T[]): T {
  return items[Math.floor(Math.random() * items.length)];
}

function randomStudentId(index: number): string {
  return `802-${String(20 + (index % 6)).padStart(2, "0")}-${String(1000 + index).padStart(4, "0")}`;
}

function randomGpa(): number {
  return Math.round((2.4 + Math.random() * 1.6) * 100) / 100;
}

function normalizeYear(value: string): Student["year"] | null {
  const lower = value.trim().toLowerCase();

  if (lower.startsWith("fr")) return "Freshman";
  if (lower.startsWith("so")) return "Sophomore";
  if (lower.startsWith("ju")) return "Junior";
  if (lower.startsWith("se")) return "Senior";

  return null;
}

function cellToText(cell: unknown): string {
  if (cell === null || cell === undefined) {
    return "";
  }

  if (cell instanceof Date) {
    return cell.toLocaleDateString();
  }

  return String(cell).trim();
}

function detectHeader(row: string[]): ColumnMap | null {
  const columns: ColumnMap = {};

  row.forEach((cell, index) => {
    const key = HEADER_ALIASES[cell.toLowerCase()];
    if (key && columns[key] === undefined) {
      columns[key] = index;
    }
  });

  return columns.name !== undefined ? columns : null;
}

function buildStudent(id: number, row: string[], columns: ColumnMap): Student | null {
  const name = columns.name !== undefined ? row[columns.name] : row[0];

  if (!name) {
    return null;
  }

  const studentIdCell = columns.studentId !== undefined ? row[columns.studentId] : "";
  const majorCell = columns.major !== undefined ? row[columns.major] : "";
  const yearCell = columns.year !== undefined ? row[columns.year] : "";
  const gpaCell = columns.gpa !== undefined ? row[columns.gpa] : "";
  const parsedGpa = Number.parseFloat(gpaCell);

  return {
    id,
    name,
    studentId: studentIdCell || randomStudentId(id),
    major: majorCell || randomItem(MAJORS),
    year: normalizeYear(yearCell) ?? randomItem(YEARS),
    gpa: Number.isFinite(parsedGpa) ? Math.round(parsedGpa * 100) / 100 : randomGpa(),
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

  if (rows.length === 0) {
    throw new RosterParseError("This file doesn't contain any rows.");
  }

  const headerColumns = detectHeader(rows[0]);
  const dataRows = headerColumns ? rows.slice(1) : rows;
  const columns = headerColumns ?? { name: 0 };

  const students = dataRows
    .map((row, index) => buildStudent(index + 1, row, columns))
    .filter((student): student is Student => student !== null);

  if (students.length === 0) {
    throw new RosterParseError("No students were found in this file.");
  }

  return students;
}
