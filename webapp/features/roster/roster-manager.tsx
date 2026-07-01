"use client";
import { useMemo, useState } from "react";
import { TriangleAlert } from "lucide-react";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { distributeStudents, type Distribution } from "@/features/roster/roster-distribution";
import { parseRosterFile, RosterParseError } from "@/features/roster/roster-parser";
import { RosterSaveCard } from "@/features/roster/roster-save-card";
import { RosterUploadCard } from "@/features/roster/roster-upload-card";
import { TeamGroupsGrid } from "@/features/roster/team-groups-grid";
import { TeamSetupCard } from "@/features/roster/team-setup-card";
import type {
  DistributionMode,
  RosterFile,
  SortDirection,
  SortField,
  Student,
} from "@/features/roster/roster-types";
import { saveGuildDistribution } from "@/features/roster/roster-persistence";
export function RosterManager({
  guildId,
}: {
  guildId: string;
}): React.ReactElement {
  const [file, setFile] = useState<RosterFile | null>(null);
  const [students, setStudents] = useState<Student[]>([]);
  const [loadingFile, setLoadingFile] = useState(false);
  const [error, setError] = useState("");
  const [sortField, setSortField] = useState<SortField>("firstName");
  const [sortDirection, setSortDirection] = useState<SortDirection>("asc");
  const [teamCount, setTeamCount] = useState(4);
  const [distributionMode, setDistributionMode] = useState<DistributionMode>("balanced");
  const [distribution, setDistribution] = useState<Distribution | null>(null);
  const [editMode, setEditMode] = useState(false);
  const [saving, setSaving] = useState(false);
  const [saved, setSaved] = useState(false);
  const studentsById = useMemo(
    (): Map<number, Student> => new Map(
      students.map((student) => [student.id, student])
    ),
    [students]
  );
  const unassignedStudents = useMemo((): Student[] => {
    return (distribution?.unassignedIds ?? [])
      .map((id) => studentsById.get(id))
      .filter((student): student is Student => Boolean(student));
  }, [distribution, studentsById]);
  async function selectFile(selected: File): Promise<void> {
    setLoadingFile(true);
    setError("");
    try {
      const parsed = await parseRosterFile(selected);
      setStudents(parsed);
      setFile({ name: selected.name, size: selected.size, type: selected.type });
      setDistribution(null);
      setEditMode(false);
      setSaved(false);
    } catch (reason) {
      setError(reason instanceof RosterParseError
        ? reason.message
        : "This file could not be read.");
    } finally {
      setLoadingFile(false);
    }
  }

  function removeFile(): void {
    setFile(null);
    setStudents([]);
    setDistribution(null);
    setEditMode(false);
    setSaved(false);
    setError("");
  }

  function generate(): void {
    setDistribution(distributeStudents(
      students, teamCount, distributionMode
    ));
    setEditMode(false);
    setSaved(false);
  }

  function rename(teamId: number, name: string): void {
    setDistribution((current) => current && ({
      ...current,
      teams: current.teams.map((team) => (
        team.id === teamId ? { ...team, name } : team
      )),
    }));
    setSaved(false);
  }

  function recolor(teamId: number, color: string): void {
    setDistribution((current) => current && ({
      ...current,
      teams: current.teams.map((team) => (
        team.id === teamId ? { ...team, color } : team
      )),
    }));
    setSaved(false);
  }

  function moveStudent(studentId: number, teamId: number | null): void {
    setDistribution((current) => {
      if (!current) return current;
      const teams = current.teams.map((team) => ({
        ...team,
        studentIds: team.studentIds.filter((id) => id !== studentId),
      }));
      const unassignedIds = current.unassignedIds.filter((id) => id !== studentId);
      const target = teams.find((team) => team.id === teamId);

      if (target) target.studentIds.push(studentId);
      else unassignedIds.push(studentId);
      return { teams, unassignedIds };
    });
    setSaved(false);
  }

  async function save(): Promise<void> {
    if (!distribution || distribution.unassignedIds.length || !guildId) return;
    setSaving(true);
    const result = await saveGuildDistribution(
      guildId, distribution, studentsById
    );
    setSaving(false);
    setSaved(result.isSuccess);
    setError(result.isFailure ? result.error.message : "");
  }

  const assignedCount = distribution?.teams.reduce(
    (total, team) => total + team.studentIds.length, 0
  ) ?? 0;

  return (
    <div className="grid min-w-0 gap-6">
      {error ? (
        <Alert variant="destructive">
          <TriangleAlert />
          <AlertTitle>Roster action failed</AlertTitle>
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      ) : null}
      <RosterUploadCard
        file={file}
        studentCount={students.length}
        loading={loadingFile}
        parseError=""
        onFileSelected={(selected): void => { void selectFile(selected); }}
        onRemoveFile={removeFile}
      />
      <TeamSetupCard
        studentCount={students.length}
        teamCount={teamCount}
        onTeamCountChange={setTeamCount}
        distributionMode={distributionMode}
        onDistributionModeChange={setDistributionMode}
        sortField={sortField}
        sortDirection={sortDirection}
        onSortFieldChange={setSortField}
        onSortDirectionChange={setSortDirection}
        onGenerate={generate}
        hasTeams={Boolean(distribution)}
        disabled={!students.length || loadingFile}
      />
      {distribution ? (
        <TeamGroupsGrid
          teams={distribution.teams}
          unassigned={unassignedStudents}
          studentsById={studentsById}
          editMode={editMode}
          sortField={sortField}
          sortDirection={sortDirection}
          onToggleEditMode={(): void => setEditMode((current) => !current)}
          onRegenerate={generate}
          onRename={rename}
          onRecolor={recolor}
          onMoveStudent={moveStudent}
        />
      ) : null}
      <RosterSaveCard
        assignedCount={assignedCount}
        studentCount={students.length}
        teamCount={distribution?.teams.length ?? 0}
        saving={saving}
        saved={saved}
        disabled={
          !guildId || !distribution || Boolean(distribution.unassignedIds.length)
        }
        onSave={(): void => { void save(); }}
      />
    </div>
  );
}
