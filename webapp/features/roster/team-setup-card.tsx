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
import type { DistributionMode } from "@/features/roster/roster-types";

const MIN_TEAMS = 2;
const MAX_TEAMS = 12;

const DISTRIBUTION_DESCRIPTIONS: Record<DistributionMode, string> = {
  balanced: "Students are distributed evenly in sort order, so every team gets a mix from across the list.",
  randomized: "Students are shuffled at random and then split evenly across teams.",
  manual: "Teams start empty so you can assign every student yourself in edit mode.",
};

interface TeamSetupCardProps {
  studentCount: number;
  teamCount: number;
  onTeamCountChange: (count: number) => void;
  distributionMode: DistributionMode;
  onDistributionModeChange: (mode: DistributionMode) => void;
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
  onGenerate,
  hasTeams,
  disabled,
}: TeamSetupCardProps): React.ReactElement {
  const perTeam = teamCount > 0 ? Math.floor(studentCount / teamCount) : 0;
  const remainder = teamCount > 0 ? studentCount % teamCount : 0;

  function clampCount(next: number): number {
    return Math.min(MAX_TEAMS, Math.max(MIN_TEAMS, next));
  }

  return (
    <Card className="min-w-0 rounded-md border-border bg-card shadow-panel">
      <CardHeader>
        <CardTitle>Team setup</CardTitle>
        <CardDescription>
          Choose how many teams to create and how students are distributed.
        </CardDescription>
        <CardAction>
          <Users className="size-5 text-muted-foreground" />
        </CardAction>
      </CardHeader>
      <CardContent>
        <FieldGroup className="gap-5 sm:flex-row sm:items-start">
          <Field className="sm:max-w-48">
            <FieldLabel htmlFor="roster-team-count">Number of teams</FieldLabel>
            <ButtonGroup>
              <Button
                type="button"
                variant="outline"
                size="icon"
                disabled={disabled || teamCount <= MIN_TEAMS}
                onClick={(): void => onTeamCountChange(clampCount(teamCount - 1))}
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
                  if (Number.isInteger(parsed)) {
                    onTeamCountChange(clampCount(parsed));
                  }
                }}
              />
              <Button
                type="button"
                variant="outline"
                size="icon"
                disabled={disabled || teamCount >= MAX_TEAMS}
                onClick={(): void => onTeamCountChange(clampCount(teamCount + 1))}
              >
                <Plus />
                <span className="sr-only">Increase team count</span>
              </Button>
            </ButtonGroup>
            <FieldDescription>
              {studentCount > 0
                ? `~${perTeam} students per team${remainder ? ` (${remainder} team${remainder > 1 ? "s" : ""} get one extra)` : ""}`
                : "Upload a list to see team sizes"}
            </FieldDescription>
          </Field>

          <Field>
            <FieldLabel>Distribution</FieldLabel>
            <Tabs
              value={distributionMode}
              onValueChange={(value): void => onDistributionModeChange(value as DistributionMode)}
            >
              <TabsList>
                <TabsTrigger value="balanced" disabled={disabled}>
                  <Sparkles />
                  Balanced
                </TabsTrigger>
                <TabsTrigger value="randomized" disabled={disabled}>
                  <Dices />
                  Randomized
                </TabsTrigger>
                <TabsTrigger value="manual" disabled={disabled}>
                  <Users />
                  Manual
                </TabsTrigger>
              </TabsList>
            </Tabs>
            <FieldDescription>
              {DISTRIBUTION_DESCRIPTIONS[distributionMode]}
            </FieldDescription>
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
