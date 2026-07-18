import type { StudentProgram } from "@/features/roster/roster-types";
import type { BotRoleDto } from "@/server/entities/bot";

const STUDENT_PROGRAMS: StudentProgram[] = [
  "INEL",
  "ICOM",
  "INSO",
  "CIIC",
];

function isStudentProgram(value: string): value is StudentProgram {
  return STUDENT_PROGRAMS.includes(value as StudentProgram);
}

export function programFromRoleIds(
  roleIds: number[],
  roles: BotRoleDto[]
): StudentProgram | null {
  const programs = new Set(roles
    .filter((role) => roleIds.includes(role.roleId))
    .map((role) => role.name.trim().toUpperCase())
    .filter(isStudentProgram));
  return programs.size === 1 ? [...programs][0] : null;
}
