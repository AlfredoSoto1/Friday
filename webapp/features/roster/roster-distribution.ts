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
    color: "#5865f2",
    roleId: null,
    existingTeamId: null,
    appendMembers: false,
    createNewTeam: true,
    studentIds: [],
  }));
}

function addStudent(teams: TeamGroup[], teamIndex: number, student: Student): void {
  teams[teamIndex] = {
    ...teams[teamIndex],
    studentIds: [...teams[teamIndex].studentIds, student.id],
  };
}

function leastPopulatedTeam(teams: TeamGroup[]): number {
  return teams.reduce((smallest, team, index) => (
    team.studentIds.length < teams[smallest].studentIds.length ? index : smallest
  ), 0);
}

function distributeBalanced(students: Student[], teams: TeamGroup[]): TeamGroup[] {
  const inel = students.filter((student) => student.program === "INEL");
  const icom = students.filter((student) => student.program === "ICOM");
  const otherPrograms = students.filter((student) => (
    student.program !== "INEL" && student.program !== "ICOM"
  ));
  const pairCount = Math.min(inel.length, icom.length);

  for (let index = 0; index < pairCount; index += 1) {
    const teamIndex = index % teams.length;
    addStudent(teams, teamIndex, inel[index]);
    addStudent(teams, teamIndex, icom[index]);
  }

  [...inel.slice(pairCount), ...icom.slice(pairCount), ...otherPrograms]
    .forEach((student) => addStudent(teams, leastPopulatedTeam(teams), student));

  return teams;
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
  const teams = createEmptyTeams(teamCount);

  if (mode === "balanced") {
    return { teams: distributeBalanced(students, teams), unassignedIds: [] };
  }

  const distributedTeams = shuffle(students).reduce<TeamGroup[]>((current, student, index) => {
    addStudent(current, index % teamCount, student);
    return current;
  }, teams);

  return { teams: distributedTeams, unassignedIds: [] };
}
