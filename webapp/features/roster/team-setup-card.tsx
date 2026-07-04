"use client";

import { Dices, Minus, Plus, Sparkles, Users } from "lucide-react";

import { Button } from "@/components/ui/button";
import { ButtonGroup } from "@/components/ui/button-group";
import {
  Card,
  CardAction,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Field, FieldDescription, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { SortControls } from "@/features/roster/sort-control-card";
import type {
  DistributionMode,
  SortDirection,
  SortField,
} from "@/features/roster/roster-types";

const MIN_TEAMS = 2;
const MAX_TEAMS = 12;

interface TeamSetupCardProps {
  studentCount: number;
  teamCount: number;
  onTeamCountChange: (count: number) => void;
  distributionMode: DistributionMode;
  onDistributionModeChange: (mode: DistributionMode) => void;
  sortField: SortField;
  sortDirection: SortDirection;
  onSortFieldChange: (field: SortField) => void;
  onSortDirectionChange: (direction: SortDirection) => void;
  onGenerate: () => void;
  hasTeams: boolean;
  disabled: boolean;
}

export function TeamSetupCard({
  studentCount,
  teamCount,
  onTeamCountChange,
  distributionMode,
  onDistributionModeChange,
  sortField,
  sortDirection,
  onSortFieldChange,
  onSortDirectionChange,
  onGenerate,
  hasTeams,
  disabled,
}: TeamSetupCardProps): React.ReactElement {
  const perTeam = teamCount > 0 ? Math.floor(studentCount / teamCount) : 0;

  function changeCount(next: number): void {
    onTeamCountChange(Math.min(MAX_TEAMS, Math.max(MIN_TEAMS, next)));
  }

  return (
    <Card className="min-w-0 rounded-md border-border bg-card shadow-panel">
      <CardHeader>
        <CardTitle>Team setup</CardTitle>
        <CardDescription>
          Configure ordering and preview the distribution before generating teams.
        </CardDescription>
        <CardAction>
          <Users className="size-5 text-muted-foreground" />
        </CardAction>
      </CardHeader>
      <CardContent className="space-y-6">
        <SortControls
          sortField={sortField}
          sortDirection={sortDirection}
          onSortFieldChange={onSortFieldChange}
          onSortDirectionChange={onSortDirectionChange}
          disabled={disabled}
        />
        <FieldGroup className="gap-5 sm:flex-row sm:items-start">
          <Field className="sm:max-w-48">
            <FieldLabel htmlFor="roster-team-count">Number of teams</FieldLabel>
            <ButtonGroup>
              <Button
                type="button"
                variant="outline"
                size="icon"
                disabled={disabled || teamCount <= MIN_TEAMS}
                onClick={(): void => changeCount(teamCount - 1)}
              >
                <Minus />
                <span className="sr-only">Decrease team count</span>
              </Button>
              <Input
                id="roster-team-count"
                type="number"
                min={MIN_TEAMS}
                max={MAX_TEAMS}
                value={teamCount}
                disabled={disabled}
                className="text-center"
                onChange={(event): void => {
                  const parsed = Number(event.target.value);
                  if (Number.isInteger(parsed)) changeCount(parsed);
                }}
              />
              <Button
                type="button"
                variant="outline"
                size="icon"
                disabled={disabled || teamCount >= MAX_TEAMS}
                onClick={(): void => changeCount(teamCount + 1)}
              >
                <Plus />
                <span className="sr-only">Increase team count</span>
              </Button>
            </ButtonGroup>
            <FieldDescription>
              {studentCount ? `About ${perTeam} students per team` : "Upload a list first"}
            </FieldDescription>
          </Field>
          <Field>
            <FieldLabel>Distribution</FieldLabel>
            <Tabs
              value={distributionMode}
              onValueChange={(value): void => {
                onDistributionModeChange(value as DistributionMode);
              }}
            >
              <TabsList>
                <TabsTrigger value="balanced" disabled={disabled}>
                  <Sparkles /> Balanced
                </TabsTrigger>
                <TabsTrigger value="randomized" disabled={disabled}>
                  <Dices /> Randomized
                </TabsTrigger>
                <TabsTrigger value="manual" disabled={disabled}>
                  <Users /> Manual
                </TabsTrigger>
              </TabsList>
            </Tabs>
          </Field>
        </FieldGroup>
      </CardContent>
      <CardFooter className="justify-end border-t pt-4">
        <Button onClick={onGenerate} disabled={disabled}>
          <Sparkles />
          {hasTeams ? "Regenerate teams" : "Generate teams"}
        </Button>
      </CardFooter>
    </Card>
  );
}
