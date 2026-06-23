import type { EditableRecord, FieldConfig } from "@/components/custom/record-fields";
import type { AdminRecord, RegisterMemberRequest } from "@/features/dashboard/admin-types";

export function toEditableRecord<T extends object>(value: T): EditableRecord {
  return value as unknown as EditableRecord;
}

export function normalizeAdminRecord(values: EditableRecord, fields: FieldConfig[]): AdminRecord {
  return Object.fromEntries(
    fields.map((field) => {
      const value = values[field.key];
      if (field.type === "number") return [field.key, numberOrNull(value)];
      if (field.type === "checkbox") return [field.key, Boolean(value)];
      return [field.key, value === "" ? null : value];
    })
  );
}

export function numberOrNull(value: unknown): number | null {
  if (value === null || value === undefined || value === "") return null;
  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : null;
}

export function parseMembersCsv(text: string): RegisterMemberRequest[] {
  const rows = text
    .split(/\r?\n/)
    .map((row) => row.trim())
    .filter(Boolean)
    .map(parseCsvLine);

  const [headers = [], ...values] = rows;
  return values.map((row) => {
    const record = Object.fromEntries(headers.map((header, index) => [header, row[index] ?? ""]));
    return {
      email: record.email ?? "",
      fullname: record.fullname ?? record.full_name ?? "",
      username: record.username ?? "",
      discord_user_id: record.discord_user_id ?? record.discordUserId ?? "-",
      fun_fact: record.fun_fact ?? record.funfact ?? "",
      discord_role_ids: (record.discord_role_ids ?? record.roles ?? "")
        .split(/[|;]/)
        .map((role) => role.trim())
        .filter(Boolean),
    };
  });
}

function parseCsvLine(line: string): string[] {
  const values: string[] = [];
  let current = "";
  let quoted = false;

  for (let index = 0; index < line.length; index += 1) {
    const char = line[index];
    const next = line[index + 1];
    if (char === '"' && next === '"') {
      current += '"';
      index += 1;
    } else if (char === '"') {
      quoted = !quoted;
    } else if (char === "," && !quoted) {
      values.push(current.trim());
      current = "";
    } else {
      current += char;
    }
  }

  values.push(current.trim());
  return values;
}
