import {
  TEAM_COLOR_SWATCHES,
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
  const sorted = [...students].sort((a, b) => {
    if (field === "gpa") {
      return a.gpa - b.gpa;
    }

    return a[field].localeCompare(b[field]);
  });

  return direction === "asc" ? sorted : sorted.reverse();
}

function shuffle(students: Student[]): Student[] {
  const shuffled = [...students];

  for (let i = shuffled.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [shuffled[i], shuffled[j]] = [shuffled[j], shuffled[i]];
  }

  return shuffled;
}

function createEmptyTeams(teamCount: number): TeamGroup[] {
  return Array.from({ length: teamCount }, (_, index) => ({
    id: index + 1,
    name: `Team ${index + 1}`,
    color: TEAM_COLOR_SWATCHES[index % TEAM_COLOR_SWATCHES.length],
    studentIds: [],
  }));
}

export interface Distribution {
  teams: TeamGroup[];
  unassignedIds: number[];
}

export function distributeStudents(
  students: Student[],
  teamCount: number,
  mode: DistributionMode,
  sortField: SortField,
  sortDirection: SortDirection
): Distribution {
  const teams = createEmptyTeams(teamCount);

  if (mode === "manual") {
    return { teams, unassignedIds: students.map((student) => student.id) };
  }

  const ordered = mode === "randomized"
    ? shuffle(students)
    : sortStudents(students, sortField, sortDirection);

  ordered.forEach((student, index) => {
    teams[index % teamCount].studentIds.push(student.id);
  });

  return { teams, unassignedIds: [] };
}
