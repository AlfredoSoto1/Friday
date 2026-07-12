"use client";

import { useEffect, useMemo, useState } from "react";
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
import type { BotRoleDto, BotTeamDto } from "@/server/entities/bot";
import { BotApi } from "@/server/webservices/bot-webservice";

function roleColor(role: BotRoleDto | undefined): string {
  const color = role?.color ?? 0;
  return color > 0 ? `#${color.toString(16).padStart(6, "0")}` : "#5865f2";
}

export function RosterManager({ guildId }: { guildId: string }): React.ReactElement {
  const [file, setFile] = useState<RosterFile | null>(null);
  const [students, setStudents] = useState<Student[]>([]);
  const [roles, setRoles] = useState<BotRoleDto[]>([]);
  const [existingTeams, setExistingTeams] = useState<BotTeamDto[]>([]);
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

  async function refreshGuildConfiguration(): Promise<void> {
    if (!/^[0-9]+$/.test(guildId)) {
      setError("Select a valid Discord server before configuring teams.");
      return;
    }

    const [rolesResult, teamsResult] = await Promise.all([
      BotApi.getGuildRoles(guildId),
      BotApi.getGuildTeams(guildId),
    ]);
    const errors: string[] = [];

    if (rolesResult.isSuccess) setRoles(rolesResult.value.items);
    else errors.push(`Could not load server roles: ${rolesResult.error.message}`);

    if (teamsResult.isSuccess) setExistingTeams(teamsResult.value.items);
    else errors.push(`Could not load existing teams: ${teamsResult.error.message}`);

    setError(errors.join(" "));
  }

  useEffect((): void => {
    setRoles([]);
    setExistingTeams([]);
    void refreshGuildConfiguration();
  }, [guildId]);

  const studentsById = useMemo(
    (): Map<number, Student> => new Map(students.map((student) => [student.id, student])),
    [students]
  );
  const unassignedStudents = useMemo((): Student[] => (
    (distribution?.unassignedIds ?? [])
      .map((id) => studentsById.get(id))
      .filter((student): student is Student => Boolean(student))
  ), [distribution, studentsById]);
  useEffect((): void => {
    if (!roles.length) return;
    setDistribution((current) => current && ({
      ...current,
      teams: current.teams.map((team) => (
        team.roleId === null
          ? team
          : { ...team, color: roleColor(roles.find((role) => role.roleId === team.roleId)) }
      )),
    }));
  }, [roles]);


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
      setError(reason instanceof RosterParseError ? reason.message : "This file could not be read.");
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
    setDistribution(distributeStudents(students, teamCount, distributionMode));
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

  function setCreateNewTeam(teamId: number, createNewTeam: boolean): void {
    setDistribution((current) => current && ({
      ...current,
      teams: current.teams.map((team) => (
        team.id === teamId ? {
          ...team,
          createNewTeam,
          existingTeamId: createNewTeam ? null : team.existingTeamId,
          appendMembers: createNewTeam ? false : team.appendMembers,
        } : team
      )),
    }));
    setSaved(false);
  }

  function selectExistingTeam(teamId: number, existingTeamId: number | null): void {
    const selectedTeam = existingTeams.find((team) => team.teamId === existingTeamId);
    if (!selectedTeam) return;

    setDistribution((current) => {
      if (!current) return current;
      if (current.teams.some((team) => (
        team.id !== teamId && team.existingTeamId === selectedTeam.teamId
      ))) {
        setError("Each existing server team can only be used once in a distribution.");
        return current;
      }

      return {
        ...current,
        teams: current.teams.map((team) => (
          team.id === teamId ? {
            ...team,
            name: selectedTeam.name,
            roleId: selectedTeam.roleId ?? null,
            color: roleColor(roles.find((role) => role.roleId === selectedTeam.roleId)),
            existingTeamId: selectedTeam.teamId,
            appendMembers: false,
            createNewTeam: false,
          } : team
        )),
      };
    });
    setSaved(false);
  }

  function selectRole(teamId: number, roleId: number): void {
    const selectedRole = roles.find((role) => role.roleId === roleId);
    setDistribution((current) => current && ({
      ...current,
      teams: current.teams.map((team) => (
        team.id === teamId ? { ...team, roleId, color: roleColor(selectedRole) } : team
      )),
    }));
    setSaved(false);
  }

  function setAppendMembers(teamId: number, appendMembers: boolean): void {
    setDistribution((current) => current && ({
      ...current,
      teams: current.teams.map((team) => (
        team.id === teamId ? { ...team, appendMembers } : team
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
    const validRoleIds = new Set(roles.map((role) => role.roleId));
    const invalidTeam = distribution?.teams.some((team) => (
      !team.name.trim() ||
      team.roleId === null ||
      !validRoleIds.has(team.roleId) ||
      (!team.createNewTeam && team.existingTeamId === null)
    ));
    if (!distribution || distribution.unassignedIds.length || !guildId || invalidTeam) {
      setError("Select a valid server role and, when not creating a new team, an existing server team.");
      return;
    }

    setSaving(true);
    const result = await saveGuildDistribution(guildId, distribution, studentsById);
    setSaving(false);
    setSaved(result.isSuccess);
    setError(result.isFailure ? result.error.message : "");
    if (result.isSuccess) {
      await refreshGuildConfiguration();
    }
  }

  const assignedCount = distribution?.teams.reduce(
    (total, team) => total + team.studentIds.length, 0
  ) ?? 0;
  const saveDisabled = !guildId || !distribution || Boolean(distribution.unassignedIds.length) || (
    distribution?.teams.some((team) => (
      team.roleId === null || (!team.createNewTeam && team.existingTeamId === null)
    )) ?? false
  );

  return (
    <div className="grid min-w-0 gap-6 pb-28">
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
          onRename={rename}
          roles={roles}
          existingTeams={existingTeams}
          onRoleChange={selectRole}
          onExistingTeamChange={selectExistingTeam}
          onCreateNewTeamChange={setCreateNewTeam}
          onAppendMembersChange={setAppendMembers}
          onConfigurationOpen={(): void => { void refreshGuildConfiguration(); }}
          onMoveStudent={moveStudent}
        />
      ) : null}
      <RosterSaveCard
        assignedCount={assignedCount}
        studentCount={students.length}
        teamCount={distribution?.teams.length ?? 0}
        saving={saving}
        saved={saved}
        disabled={saveDisabled}
        onSave={(): void => { void save(); }}
      />
    </div>
  );
}
