import type { EnvelopeResult } from "@/lib/webservices";
import type { Distribution } from "@/features/roster/roster-distribution";
import { programFromRoleIds } from "@/features/roster/roster-program";
import type { Student } from "@/features/roster/roster-types";
import type { BotRoleDto } from "@/server/entities/bot";
import type { SaveGuildRosterResult } from "@/server/entities/inelicom";
import { InelicomApi } from "@/server/webservices/inelicom-webservice";

export function saveGuildDistribution(
  guildId: string,
  distribution: Distribution,
  studentsById: Map<number, Student>,
  roles: BotRoleDto[],
  isEO: boolean
): Promise<EnvelopeResult<SaveGuildRosterResult>> {
  const teams = distribution.teams.map((team) => {
    const teamProgram = programFromRoleIds(team.roleIds, roles);

    return {
      teamId: team.existingTeamId ?? undefined,
      name: team.name,
      roleIds: team.roleIds,
      appendMembers: team.appendMembers,
      students: team.studentIds.flatMap((studentId) => {
        const student = studentsById.get(studentId);
        const program = isEO ? (student?.program ?? teamProgram) : teamProgram;
        return student && program ? [{
          email: student.institutionalEmail || student.personalEmail,
          firstName: student.firstName,
          firstLastName: student.firstLastName,
          secondLastName: student.secondLastName,
          initial: student.initial,
          program,
        }] : [];
      }),
    };
  });

  return InelicomApi.saveGuildRoster(guildId, { teams });
}
