"use client";

import { useState } from "react";
import { ArrowRightLeft, Pencil, Users, X } from "lucide-react";

import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardAction,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Field, FieldDescription, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import {
  Item,
  ItemActions,
  ItemContent,
  ItemDescription,
  ItemMedia,
  ItemTitle,
} from "@/components/ui/item";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import { type TeamCardProps } from "@/features/roster/roster-types";
import { studentInitials } from "@/features/roster/student-initials";
import { cn } from "@/lib/utils";

export function TeamCard({
  team,
  members,
  otherTeams,
  roles,
  existingTeams,
  onRoleChange,
  onExistingTeamChange,
  onCreateNewTeamChange,
  onAppendMembersChange,
  onConfigurationOpen,
  editMode,
  onRename,
  onMoveStudent,
}: TeamCardProps): React.ReactElement {
  const [open, setOpen] = useState(false);
  const [draftName, setDraftName] = useState(team.name);
  const [draftCreateNewTeam, setDraftCreateNewTeam] = useState(team.createNewTeam);
  const [draftExistingTeamId, setDraftExistingTeamId] = useState<number | null>(team.existingTeamId);
  const [draftRoleId, setDraftRoleId] = useState<number | null>(team.roleId);
  const [draftAppendMembers, setDraftAppendMembers] = useState(team.appendMembers);

  function resetDraft(): void {
    setDraftName(team.name);
    setDraftCreateNewTeam(team.createNewTeam);
    setDraftExistingTeamId(team.existingTeamId);
    setDraftRoleId(team.roleId);
    setDraftAppendMembers(team.appendMembers);
  }

  function confirm(): void {
    if (draftCreateNewTeam && draftName.trim()) onRename(team.id, draftName.trim());
    if (draftCreateNewTeam !== team.createNewTeam) {
      onCreateNewTeamChange(team.id, draftCreateNewTeam);
    }
    if (!draftCreateNewTeam && draftExistingTeamId !== null) {
      onExistingTeamChange(team.id, draftExistingTeamId);
    }
    if (draftRoleId !== null) onRoleChange(team.id, draftRoleId);
    if (!draftCreateNewTeam && draftExistingTeamId !== null) {
      onAppendMembersChange(team.id, draftAppendMembers);
    }
    setOpen(false);
  }

  const canConfirm = draftRoleId !== null && (
    draftCreateNewTeam ? Boolean(draftName.trim()) : draftExistingTeamId !== null
  );

  return (
    <Card
      className={cn(
        "min-w-0 gap-3 rounded-md border-border bg-card py-3 shadow-panel transition-colors",
        open && "ring-2 ring-primary"
      )}
      style={{ borderTopColor: team.color, borderTopWidth: 3 }}
    >
      <CardHeader className="px-3">
        <span
          aria-hidden
          className="size-3 shrink-0 rounded-full"
          style={{ backgroundColor: team.color }}
        />
        <CardTitle className="truncate">{team.name}</CardTitle>
        <CardAction className="flex items-center gap-1.5">
          <Badge variant="secondary">
            <Users />
            {members.length}
          </Badge>
          <Dialog
            open={open}
            onOpenChange={(nextOpen): void => {
              if (nextOpen) {
                resetDraft();
                onConfigurationOpen();
              }
              setOpen(nextOpen);
            }}
          >
            <DialogTrigger asChild>
              <Button variant="ghost" size="icon-sm">
                <Pencil />
                <span className="sr-only">Edit {team.name}</span>
              </Button>
            </DialogTrigger>
            <DialogContent className="sm:max-w-md">
              <DialogHeader>
                <DialogTitle>Configure team</DialogTitle>
                <DialogDescription>
                  Select the server role. Its Discord color is applied to this team automatically.
                </DialogDescription>
              </DialogHeader>
              <Field orientation="horizontal" className="items-center justify-between gap-3 rounded-md border p-3">
                <div className="space-y-1">
                  <FieldLabel htmlFor={`team-create-${team.id}`}>Create a new team</FieldLabel>
                  <FieldDescription>Turn this off to select an existing backend team.</FieldDescription>
                </div>
                <Switch
                  id={`team-create-${team.id}`}
                  checked={draftCreateNewTeam}
                  onCheckedChange={setDraftCreateNewTeam}
                />
              </Field>
              {draftCreateNewTeam ? (
                <Field>
                  <FieldLabel htmlFor={`team-name-${team.id}`}>New team name</FieldLabel>
                  <Input
                    id={`team-name-${team.id}`}
                    value={draftName}
                    onChange={(event): void => setDraftName(event.target.value)}
                  />
                </Field>
              ) : (
                <Field>
                  <FieldLabel>Existing server team</FieldLabel>
                  <Select
                    value={draftExistingTeamId?.toString()}
                    onValueChange={(value): void => {
                      const selectedTeam = existingTeams.find((existingTeam) => (
                        existingTeam.teamId === Number(value)
                      ));
                      setDraftExistingTeamId(Number(value));
                      setDraftRoleId(selectedTeam?.roleId ?? null);
                    }}
                    disabled={existingTeams.length === 0}
                  >
                    <SelectTrigger className="w-full">
                      <SelectValue placeholder="Select an existing team" />
                    </SelectTrigger>
                    <SelectContent>
                      {existingTeams.map((existingTeam) => (
                        <SelectItem key={existingTeam.teamId} value={existingTeam.teamId.toString()}>
                          {existingTeam.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FieldDescription>
                    {existingTeams.length ? "Choose a team already stored for this server." : "No existing teams are stored for this server."}
                  </FieldDescription>
                </Field>
              )}
              <Field>
                <FieldLabel>Discord role</FieldLabel>
                <Select
                  value={draftRoleId?.toString()}
                  onValueChange={(value): void => setDraftRoleId(Number(value))}
                  disabled={roles.length === 0}
                >
                  <SelectTrigger className="w-full">
                    <SelectValue placeholder={roles.length ? "Select a server role" : "Loading server roles"} />
                  </SelectTrigger>
                  <SelectContent>
                    {roles.map((role) => (
                      <SelectItem key={role.roleId} value={role.roleId.toString()}>
                        {role.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FieldDescription>
                  {roles.length ? "Roles are loaded from the selected Discord server." : "No server roles are available yet."}
                </FieldDescription>
              </Field>
              {!draftCreateNewTeam && draftExistingTeamId !== null ? (
                <Field orientation="horizontal" className="items-center justify-between gap-3 rounded-md border p-3">
                  <div className="space-y-1">
                    <FieldLabel htmlFor={`team-append-${team.id}`}>Append members</FieldLabel>
                    <FieldDescription>On keeps current members; off fully replaces the backend team.</FieldDescription>
                  </div>
                  <Switch
                    id={`team-append-${team.id}`}
                    checked={draftAppendMembers}
                    onCheckedChange={setDraftAppendMembers}
                  />
                </Field>
              ) : null}
              <DialogFooter>
                <Button variant="outline" onClick={(): void => setOpen(false)}>Cancel</Button>
                <Button onClick={confirm} disabled={!canConfirm}>OK</Button>
              </DialogFooter>
            </DialogContent>
          </Dialog>
        </CardAction>
      </CardHeader>
      <CardContent className="min-w-0 px-3">
        {members.length === 0 ? (
          <p className="rounded-md border border-dashed py-4 text-center text-xs text-muted-foreground">
            No students assigned yet
          </p>
        ) : (
          <div className="grid gap-1.5">
            {members.map((student) => (
              <Item key={student.id} size="sm" variant="muted" className="min-w-0 gap-2 px-2 py-1.5">
                <ItemMedia variant="default">
                  <Avatar size="sm">
                    <AvatarFallback>{studentInitials(student.name)}</AvatarFallback>
                  </Avatar>
                </ItemMedia>
                <ItemContent className="min-w-0">
                  <ItemTitle className="truncate text-xs">{student.name}</ItemTitle>
                  <ItemDescription className="truncate text-xs">
                    {student.institutionalEmail || student.personalEmail}
                  </ItemDescription>
                </ItemContent>
                {editMode ? (
                  <ItemActions>
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button variant="ghost" size="icon-xs">
                          <ArrowRightLeft />
                          <span className="sr-only">Move {student.name}</span>
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuLabel>Move to</DropdownMenuLabel>
                        {otherTeams.map((destination) => (
                          <DropdownMenuItem
                            key={destination.id}
                            onSelect={(): void => onMoveStudent(student.id, destination.id)}
                          >
                            <span
                              className="size-2 shrink-0 rounded-full"
                              style={{ backgroundColor: destination.color }}
                            />
                            {destination.name}
                          </DropdownMenuItem>
                        ))}
                        <DropdownMenuSeparator />
                        <DropdownMenuItem onSelect={(): void => onMoveStudent(student.id, null)}>
                          <X />
                          Unassign
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </ItemActions>
                ) : null}
              </Item>
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
