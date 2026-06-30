import type { Distribution } from "@/features/roster/roster-distribution";
import type {
  DistributionMode,
  RosterFile,
  SortDirection,
  SortField,
  Student,
} from "@/features/roster/roster-types";

export interface StoredRoster {
  file: RosterFile;
  students: Student[];
  sortField: SortField;
  sortDirection: SortDirection;
  teamCount: number;
  distributionMode: DistributionMode;
  distribution: Distribution | null;
}

const STORAGE_KEY = "friday:roster";

export function loadStoredRoster(): StoredRoster | null {
  const raw = window.localStorage.getItem(STORAGE_KEY);

  if (!raw) {
    return null;
  }

  return JSON.parse(raw) as StoredRoster;
}

export function saveStoredRoster(roster: StoredRoster): void {
  window.localStorage.setItem(STORAGE_KEY, JSON.stringify(roster));
}
