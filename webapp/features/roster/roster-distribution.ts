import {
  type DistributionMode,
  type SortDirection,
  type SortField,
  type Student,
  type TeamGroup,
} from "@/features/roster/roster-types";

export function sortStudents(
  students: Student[],
  field: SortField,
  direction: SortDirection
): Student[] {
  const sorted = [...students].sort((first, second) => (
    first[field].localeCompare(second[field], undefined, {
      numeric: true,
      sensitivity: "base",
    })
  ));

  return direction === "asc" ? sorted : sorted.reverse();
}

function shuffle(students: Student[]): Student[] {
  return students
    .map((student) => ({ student, order: Math.random() }))
    .sort((first, second) => first.order - second.order)
    .map((item) => item.student);
}

function createEmptyTeams(teamCount: number): TeamGroup[] {
  return Array.from({ length: teamCount }, (_, index) => ({
    id: index + 1,
    name: `Team ${index + 1}`,
    roleIds: [],
    existingTeamId: null,
    appendMembers: false,
    createNewTeam: true,
    studentIds: [],
  }));
}

function distributeEvenly(students: Student[], teams: TeamGroup[]): TeamGroup[] {
  return students.reduce<TeamGroup[]>((current, student, index) => {
    const teamIndex = index % teams.length;
    current[teamIndex] = {
      ...current[teamIndex],
      studentIds: [...current[teamIndex].studentIds, student.id],
    };
    return current;
  }, teams);
}

export interface Distribution {
  teams: TeamGroup[];
  unassignedIds: number[];
}

export function distributeStudents(
  students: Student[],
  teamCount: number,
  mode: DistributionMode
): Distribution {
  const orderedStudents = mode === "randomized" ? shuffle(students) : students;
  return {
    teams: distributeEvenly(orderedStudents, createEmptyTeams(teamCount)),
    unassignedIds: [],
  };
}
