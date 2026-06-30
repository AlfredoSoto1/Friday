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

function storageKey(guildId: string): string {
  return `friday:${guildId}:roster`;
}

export function loadStoredRoster(guildId: string): StoredRoster | null {
  const raw = window.localStorage.getItem(storageKey(guildId));

  if (!raw) {
    return null;
  }

  return JSON.parse(raw) as StoredRoster;
}

export function saveStoredRoster(guildId: string, roster: StoredRoster): void {
  window.localStorage.setItem(storageKey(guildId), JSON.stringify(roster));
}
