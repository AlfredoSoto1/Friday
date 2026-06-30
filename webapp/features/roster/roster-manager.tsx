"use client";

import { useEffect, useMemo, useState } from "react";
import { Check, Save, TriangleAlert } from "lucide-react";

import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Spinner } from "@/components/ui/spinner";
import { distributeStudents, type Distribution } from "@/features/roster/roster-distribution";
import { parseRosterFile, RosterParseError } from "@/features/roster/roster-parser";
import { loadStoredRoster, saveStoredRoster } from "@/features/roster/roster-storage";
import { RosterUploadCard } from "@/features/roster/roster-upload-card";
import { SortControlCard } from "@/features/roster/sort-control-card";
import { TeamGroupsGrid } from "@/features/roster/team-groups-grid";
import { TeamSetupCard } from "@/features/roster/team-setup-card";
import type {
  DistributionMode,
  RosterFile,
  SortDirection,
  SortField,
  Student,
} from "@/features/roster/roster-types";

interface RosterManagerProps {
  guildId: string;
}

export function RosterManager({ guildId }: RosterManagerProps): React.ReactElement {
  const [ready, setReady] = useState(false);
  const [file, setFile] = useState<RosterFile | null>(null);
  const [students, setStudents] = useState<Student[]>([]);
  const [loadingFile, setLoadingFile] = useState(false);
  const [parseError, setParseError] = useState("");
  const [sortField, setSortField] = useState<SortField>("name");
  const [sortDirection, setSortDirection] = useState<SortDirection>("asc");
  const [teamCount, setTeamCount] = useState(4);
  const [distributionMode, setDistributionMode] = useState<DistributionMode>("balanced");
  const [distribution, setDistribution] = useState<Distribution | null>(null);
  const [editMode, setEditMode] = useState(false);
  const [saved, setSaved] = useState(false);
  const [storageError, setStorageError] = useState("");

  useEffect((): void => {
    try {
      const stored = loadStoredRoster(guildId);

      if (stored) {
        setFile(stored.file);
        setStudents(stored.students);
        setSortField(stored.sortField);
        setSortDirection(stored.sortDirection);
        setTeamCount(stored.teamCount);
        setDistributionMode(stored.distributionMode);
        setDistribution(stored.distribution);
        setSaved(true);
      }
    } catch {
      setStorageError("The saved roster for this server could not be read.");
    } finally {
      setReady(true);
    }
  }, [guildId]);

  const studentsById = useMemo(
    (): Map<number, Student> => new Map(students.map((student) => [student.id, student])),
    [students]
  );

  const unassignedStudents = useMemo((): Student[] => {
    if (!distribution) {
      return [];
    }

    return distribution.unassignedIds
      .map((id) => studentsById.get(id))
      .filter((student): student is Student => Boolean(student));
  }, [distribution, studentsById]);

  async function handleFileSelected(selected: File): Promise<void> {
    setLoadingFile(true);
    setParseError("");

    try {
      const parsed = await parseRosterFile(selected);
      setStudents(parsed);
      setFile({ name: selected.name, size: selected.size, type: selected.type });
      setDistribution(null);
      setEditMode(false);
      setSaved(false);
    } catch (error) {
      setParseError(
        error instanceof RosterParseError
          ? error.message
          : "This file could not be read. Try a different CSV, Excel, or TXT file."
      );
    } finally {
      setLoadingFile(false);
    }
  }

  function handleRemoveFile(): void {
    setFile(null);
    setStudents([]);
    setParseError("");
    setDistribution(null);
    setEditMode(false);
    setSaved(false);
  }

  function handleGenerate(): void {
    setDistribution(
      distributeStudents(students, teamCount, distributionMode, sortField, sortDirection)
    );
    setEditMode(false);
    setSaved(false);
  }

  function handleRename(teamId: number, name: string): void {
    setDistribution((current) => current && {
      ...current,
      teams: current.teams.map((team) => (team.id === teamId ? { ...team, name } : team)),
    });
    setSaved(false);
  }

  function handleRecolor(teamId: number, color: string): void {
    setDistribution((current) => current && {
      ...current,
      teams: current.teams.map((team) => (team.id === teamId ? { ...team, color } : team)),
    });
    setSaved(false);
  }

  function handleMoveStudent(studentId: number, toTeamId: number | null): void {
    setDistribution((current) => {
      if (!current) {
        return current;
      }

      const teams = current.teams.map((team) => ({
        ...team,
        studentIds: team.studentIds.filter((id) => id !== studentId),
      }));
      const unassignedIds = current.unassignedIds.filter((id) => id !== studentId);

      if (toTeamId === null) {
        unassignedIds.push(studentId);
      } else {
        const target = teams.find((team) => team.id === toTeamId);
        target?.studentIds.push(studentId);
      }

      return { teams, unassignedIds };
    });
    setSaved(false);
  }

  function handleSave(): void {
    if (!file || !distribution) {
      return;
    }

    try {
      saveStoredRoster(guildId, {
        file,
        students,
        sortField,
        sortDirection,
        teamCount,
        distributionMode,
        distribution,
      });
      setSaved(true);
      setStorageError("");
    } catch {
      setStorageError("This roster is too large to save in browser storage.");
    }
  }

  const assignedCount = distribution
    ? distribution.teams.reduce((total, team) => total + team.studentIds.length, 0)
    : 0;

  return (
    <div className="grid min-w-0 gap-6">
      {storageError ? (
        <Alert variant="destructive">
          <TriangleAlert />
          <AlertTitle>Could not save roster</AlertTitle>
          <AlertDescription>{storageError}</AlertDescription>
        </Alert>
      ) : null}

      <RosterUploadCard
        file={file}
        studentCount={students.length}
        loading={loadingFile}
        parseError={parseError}
        onFileSelected={(selected): void => {
          void handleFileSelected(selected);
        }}
        onRemoveFile={handleRemoveFile}
      />

      <SortControlCard
        sortField={sortField}
        sortDirection={sortDirection}
        onSortFieldChange={setSortField}
        onSortDirectionChange={setSortDirection}
        disabled={students.length === 0}
      />

      <TeamSetupCard
        studentCount={students.length}
        teamCount={teamCount}
        onTeamCountChange={setTeamCount}
        distributionMode={distributionMode}
        onDistributionModeChange={setDistributionMode}
        onGenerate={handleGenerate}
        hasTeams={Boolean(distribution)}
        disabled={students.length === 0 || loadingFile}
      />

      {distribution ? (
        <TeamGroupsGrid
          teams={distribution.teams}
          unassigned={unassignedStudents}
          studentsById={studentsById}
          editMode={editMode}
          onToggleEditMode={(): void => setEditMode((current) => !current)}
          onRegenerate={handleGenerate}
          onRename={handleRename}
          onRecolor={handleRecolor}
          onMoveStudent={handleMoveStudent}
        />
      ) : null}

      <Card className="min-w-0 rounded-md border-border bg-card shadow-panel">
        <CardContent className="flex flex-wrap items-center justify-between gap-4">
          <p className="text-sm text-muted-foreground">
            {distribution
              ? `${assignedCount}/${students.length} students assigned across ${distribution.teams.length} teams.`
              : "Upload a list and generate teams before saving."}
          </p>
          <Button onClick={handleSave} disabled={!ready || !distribution || loadingFile}>
            {loadingFile ? <Spinner /> : saved ? <Check /> : <Save />}
            {saved ? "Saved" : "Save team distribution"}
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}
