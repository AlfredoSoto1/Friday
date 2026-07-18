import { Workbook } from "exceljs";
import JSZip from "jszip";

import type { PrepaTeamExportRowDto } from "@/server/entities/bot";

interface ExportMember {
  serverUserId: number;
  firstName: string;
  lastNames: string;
  initial: string;
  email: string;
  program: string;
}

interface ExportTeam {
  position: number;
  name: string;
  members: ExportMember[];
}

const worksheetColumns = [
  { header: "First name", key: "firstName", width: 18 },
  { header: "Lastnames", key: "lastNames", width: 28 },
  { header: "Initial", key: "initial", width: 12 },
  { header: "Email", key: "email", width: 34 },
  { header: "Program", key: "program", width: 12 },
];

function toMember(row: PrepaTeamExportRowDto): ExportMember | null {
  if (row.serverUserId === null || row.serverUserId === undefined) {
    return null;
  }

  return {
    serverUserId: row.serverUserId,
    firstName: row.firstName?.trim() ?? "",
    lastNames: [row.firstLastName, row.secondLastName]
      .filter((value): value is string => Boolean(value?.trim()))
      .map((value) => value.trim())
      .join(" "),
    initial: row.initial?.trim() ?? "",
    email: row.email?.trim() ?? "",
    program: row.program?.trim() ?? "",
  };
}

function compareMembers(left: ExportMember, right: ExportMember): number {
  return left.lastNames.localeCompare(right.lastNames, undefined, {
    sensitivity: "base",
  }) || left.firstName.localeCompare(right.firstName, undefined, {
    sensitivity: "base",
  });
}

function groupTeams(rows: PrepaTeamExportRowDto[]): ExportTeam[] {
  const teams = new Map<number, ExportTeam>();

  rows.forEach((row) => {
    const team = teams.get(row.teamId) ?? {
      position: row.position,
      name: row.teamName,
      members: [],
    };
    const member = toMember(row);
    if (member) {
      team.members.push(member);
    }
    teams.set(row.teamId, team);
  });

  return Array.from(teams.values())
    .sort((left, right) => left.position - right.position)
    .map((team) => ({
      ...team,
      members: team.members.sort(compareMembers),
    }));
}

function sanitizeFileName(name: string): string {
  const sanitized = name
    .trim()
    .replace(/[<>:"/\\|?*]+/g, "_")
    .replace(/[. ]+$/g, "");
  return sanitized || "Team";
}

async function createWorkbook(members: ExportMember[]): Promise<ArrayBuffer> {
  const workbook = new Workbook();
  const worksheet = workbook.addWorksheet("Prepas");
  worksheet.columns = worksheetColumns;
  worksheet.addRows(members.map((member) => ({
    firstName: member.firstName,
    lastNames: member.lastNames,
    initial: member.initial,
    email: member.email,
    program: member.program,
  })));
  worksheet.getRow(1).font = { bold: true };
  worksheet.getRow(1).alignment = { vertical: "middle" };
  worksheet.views = [{ state: "frozen", ySplit: 1 }];
  worksheet.autoFilter = "A1:E1";

  return workbook.xlsx.writeBuffer();
}

export async function buildPrepaTeamArchive(
  rows: PrepaTeamExportRowDto[]
): Promise<Blob> {
  const teams = groupTeams(rows);
  const zip = new JSZip();

  for (const team of teams) {
    const position = team.position.toString().padStart(2, "0");
    const workbook = await createWorkbook(team.members);
    zip.file(`${position} - ${sanitizeFileName(team.name)}.xlsx`, workbook);
  }

  const masterMembers = Array.from(
    new Map(
      teams
        .flatMap((team) => team.members)
        .map((member) => [member.serverUserId, member])
    ).values()
  ).sort(compareMembers);
  zip.file("Master Prepas.xlsx", await createWorkbook(masterMembers));

  return zip.generateAsync({
    type: "blob",
    compression: "DEFLATE",
    compressionOptions: { level: 6 },
    mimeType: "application/zip",
  });
}
