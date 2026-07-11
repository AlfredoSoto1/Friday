"use client";

import { Pencil, Shuffle, UserRoundCheck } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardAction,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { sortStudents } from "@/features/roster/roster-distribution";
import { TeamCard } from "@/features/roster/team-card";
import type { BotRoleDto } from "@/server/entities/bot";
import type {
  SortDirection,
  SortField,
  Student,
  TeamGroup,
} from "@/features/roster/roster-types";

interface TeamGroupsGridProps {
  teams: TeamGroup[];
  roles: BotRoleDto[];
  existingTeamNames: string[];
  unassigned: Student[];
  studentsById: Map<number, Student>;
  editMode: boolean;
  sortField: SortField;
  sortDirection: SortDirection;
  onToggleEditMode: () => void;
  onRegenerate: () => void;
  onRename: (teamId: number, name: string) => void;
  onRecolor: (teamId: number, color: string) => void;
  onRoleChange: (teamId: number, roleId: number) => void;
  onMoveStudent: (studentId: number, toTeamId: number | null) => void;
}

export function TeamGroupsGrid({
  teams,
  roles,
  existingTeamNames,
  unassigned,
  studentsById,
  editMode,
  sortField,
  sortDirection,
  onToggleEditMode,
  onRegenerate,
  onRename,
  onRecolor,
  onRoleChange,
  onMoveStudent,
}: TeamGroupsGridProps): React.ReactElement {
  return (
    <Card className="min-w-0 rounded-md border-border bg-card shadow-panel">
      <CardHeader>
        <CardTitle>Teams</CardTitle>
        <CardDescription>
          Select a team to rename it or change its color. Turn on editing to move students between teams.
        </CardDescription>
        <CardAction className="flex items-center gap-2">
          <Button variant="outline" size="sm" onClick={onRegenerate}>
            <Shuffle />
            Regenerate
          </Button>
          <Button
            variant={editMode ? "default" : "outline"}
            size="sm"
            onClick={onToggleEditMode}
          >
            <Pencil />
            {editMode ? "Done editing" : "Edit groups"}
          </Button>
        </CardAction>
      </CardHeader>
      <CardContent className="min-w-0 space-y-4">
        {editMode && unassigned.length > 0 ? (
          <div className="rounded-md border border-dashed p-3">
            <p className="mb-2 flex items-center gap-1.5 text-xs font-medium text-muted-foreground">
              <UserRoundCheck className="size-3.5" />
              Unassigned students ({unassigned.length})
            </p>
            <div className="flex flex-wrap gap-1.5">
              {unassigned.map((student) => (
                <DropdownMenu key={student.id}>
                  <DropdownMenuTrigger asChild>
                    <Badge variant="outline" className="h-6 cursor-pointer">
                      {student.name}
                    </Badge>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="start">
                    <DropdownMenuLabel>Assign to</DropdownMenuLabel>
                    {teams.map((team) => (
                      <DropdownMenuItem
                        key={team.id}
                        onSelect={(): void => onMoveStudent(student.id, team.id)}
                      >
                        <span
                          className="size-2 shrink-0 rounded-full"
                          style={{ backgroundColor: team.color }}
                        />
                        {team.name}
                      </DropdownMenuItem>
                    ))}
                  </DropdownMenuContent>
                </DropdownMenu>
              ))}
            </div>
          </div>
        ) : null}

        <div className="grid min-w-0 gap-4 md:grid-cols-2 xl:grid-cols-3">
          {teams.map((team) => (
            <TeamCard
              key={team.id}
              team={team}
              members={sortStudents(
                team.studentIds
                  .map((id) => studentsById.get(id))
                  .filter((student): student is Student => Boolean(student)),
                sortField,
                sortDirection
              )}
              otherTeams={teams.filter((other) => other.id !== team.id)}
              editMode={editMode}
              onRename={onRename}
              onRecolor={onRecolor}
              roles={roles}
              existingTeamNames={existingTeamNames}
              onRoleChange={onRoleChange}
              onMoveStudent={onMoveStudent}
            />
          ))}
        </div>
      </CardContent>
    </Card>
  );
}
