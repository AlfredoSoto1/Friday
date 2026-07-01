"use client";
import { useEffect, useState } from "react";
import { ArrowRightLeft, Check, Pencil, Users, X } from "lucide-react";
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
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Field, FieldLabel } from "@/components/ui/field";
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
  Popover,
  PopoverContent,
  PopoverHeader,
  PopoverTitle,
  PopoverTrigger,
} from "@/components/ui/popover";
import {
  TEAM_COLOR_SWATCHES,
  type TeamCardProps,
} from "@/features/roster/roster-types";
import { studentInitials } from "@/features/roster/student-initials";
import { cn } from "@/lib/utils";
export function TeamCard({
  team,
  members,
  otherTeams,
  editMode,
  onRename,
  onRecolor,
  onMoveStudent,
}: TeamCardProps): React.ReactElement {
  const [open, setOpen] = useState(false);
  const [draftName, setDraftName] = useState(team.name);
  useEffect((): void => {
    if (open) {
      setDraftName(team.name);
    }
  }, [open, team.name]);
  function commitName(): void {
    const trimmed = draftName.trim();
    if (trimmed) {
      onRename(team.id, trimmed);
    }
  }
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
          <Popover open={open} onOpenChange={setOpen}>
            <PopoverTrigger asChild>
              <Button variant="ghost" size="icon-sm">
                <Pencil />
                <span className="sr-only">Customize {team.name}</span>
              </Button>
            </PopoverTrigger>
            <PopoverContent>
              <PopoverHeader>
                <PopoverTitle>Customize team</PopoverTitle>
              </PopoverHeader>
              <Field>
                <FieldLabel htmlFor={`team-name-${team.id}`}>Team name</FieldLabel>
                <Input
                  id={`team-name-${team.id}`}
                  value={draftName}
                  onChange={(event): void => setDraftName(event.target.value)}
                  onBlur={commitName}
                  onKeyDown={(event): void => {
                    if (event.key === "Enter") {
                      commitName();
                    }
                  }}
                />
              </Field>
              <Field>
                <FieldLabel>Team color</FieldLabel>
                <div className="flex flex-wrap gap-2">
                  {TEAM_COLOR_SWATCHES.map((swatch) => (
                    <button
                      key={swatch}
                      type="button"
                      aria-label={`Use color ${swatch}`}
                      onClick={(): void => onRecolor(team.id, swatch)}
                      className="flex size-6 items-center justify-center rounded-full ring-1 ring-foreground/10"
                      style={{ backgroundColor: swatch }}
                    >
                      {team.color === swatch ? (
                        <Check className="size-3.5 text-white" />
                      ) : null}
                    </button>
                  ))}
                  <Input
                    type="color"
                    value={team.color}
                    onChange={(event): void => onRecolor(team.id, event.target.value)}
                    className="h-6 w-8 cursor-pointer p-0.5"
                    aria-label="Custom team color"
                  />
                </div>
              </Field>
            </PopoverContent>
          </Popover>
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
