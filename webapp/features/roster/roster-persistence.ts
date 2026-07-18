import type { EnvelopeResult } from "@/lib/webservices";
import type { Distribution } from "@/features/roster/roster-distribution";
import type { Student } from "@/features/roster/roster-types";
import type {
  SaveGuildRosterRequest,
  SaveGuildRosterResult,
} from "@/server/entities/inelicom";
import { InelicomApi } from "@/server/webservices/inelicom-webservice";

export function saveGuildDistribution(
  guildId: string,
  distribution: Distribution,
  studentsById: Map<number, Student>
): Promise<EnvelopeResult<SaveGuildRosterResult>> {
  const request: SaveGuildRosterRequest = {
    teams: distribution.teams.map((team) => ({
      teamId: team.existingTeamId ?? undefined,
      name: team.name,
      roleIds: team.roleIds,
      appendMembers: team.appendMembers,
      students: team.studentIds.flatMap((studentId) => {
        const student = studentsById.get(studentId);
        return student ? [{
          email: student.institutionalEmail || student.personalEmail,
          firstName: student.firstName,
          firstLastName: student.firstLastName,
          secondLastName: student.secondLastName,
          initial: student.initial,
        }] : [];
      }),
    })),
  };

  return InelicomApi.saveGuildRoster(guildId, request);
}
