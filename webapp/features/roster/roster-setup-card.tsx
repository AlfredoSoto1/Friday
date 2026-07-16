"use client";

import { useState } from "react";
import {
  Dices,
  FileSpreadsheet,
  Minus,
  Plus,
  Sparkles,
  Upload,
  X,
} from "lucide-react";

import { Badge } from "@/components/ui/badge";
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
import {
  Item,
  ItemActions,
  ItemContent,
  ItemDescription,
  ItemMedia,
  ItemTitle,
} from "@/components/ui/item";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Spinner } from "@/components/ui/spinner";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { SortControls } from "@/features/roster/sort-control-card";
import type {
  DistributionMode,
  RosterFile,
  SortDirection,
  SortField,
  StudentProgram,
} from "@/features/roster/roster-types";

const ACCEPTED_EXTENSIONS = [".csv", ".xlsx", ".txt"];
const MIN_TEAMS = 2;
const MAX_TEAMS = 12;

interface RosterSetupCardProps {
  file: RosterFile | null;
  studentCount: number;
  loading: boolean;
  onFileSelected: (file: File) => void;
  onRemoveFile: () => void;
  program: StudentProgram | "";
  onProgramChange: (program: StudentProgram) => void;
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

function isAcceptedFile(file: File): boolean {
  const lowerName = file.name.toLowerCase();
  return ACCEPTED_EXTENSIONS.some((extension) => lowerName.endsWith(extension));
}

function formatFileSize(bytes: number): string {
  if (bytes < 1024) {
    return bytes + " B";
  }

  return (bytes / 1024).toFixed(1) + " KB";
}

export function RosterSetupCard({
  file,
  studentCount,
  loading,
  onFileSelected,
  onRemoveFile,
  program,
  onProgramChange,
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
}: RosterSetupCardProps): React.ReactElement {
  const [dragActive, setDragActive] = useState(false);
  const [typeError, setTypeError] = useState("");
  const perTeam = teamCount > 0 ? Math.floor(studentCount / teamCount) : 0;

  function handleFile(candidate?: File | null): void {
    if (!candidate) {
      return;
    }

    if (!isAcceptedFile(candidate)) {
      setTypeError("Upload a CSV, Excel (.xlsx), or TXT file.");
      return;
    }

    setTypeError("");
    onFileSelected(candidate);
  }

  function changeCount(next: number): void {
    onTeamCountChange(Math.min(MAX_TEAMS, Math.max(MIN_TEAMS, next)));
  }

  return (
    <Card className="min-w-0 rounded-md border-border bg-card shadow-panel">
      <CardHeader>
        <CardTitle>Student list and team setup</CardTitle>
        <CardDescription>
          Upload a roster, configure its ordering, and generate the team distribution.
        </CardDescription>
        <CardAction>
          <FileSpreadsheet className="size-5 text-muted-foreground" />
        </CardAction>
      </CardHeader>
      <CardContent className="grid gap-6 lg:grid-cols-2">
        <div className="min-w-0 space-y-2">
          <Input
            id="roster-file-input"
            type="file"
            accept=".csv,.xlsx,.txt,text/csv,text/plain"
            className="hidden"
            disabled={loading}
            onChange={(event): void => {
              handleFile(event.target.files?.[0]);
              event.target.value = "";
            }}
          />

          {file ? (
            <Item variant="outline" className="items-start gap-3 p-3">
              <ItemMedia
                variant="icon"
                className="size-10 shrink-0 rounded-md bg-muted text-muted-foreground"
              >
                <FileSpreadsheet className="size-5" />
              </ItemMedia>
              <ItemContent className="min-w-0">
                <ItemTitle className="truncate">{file.name}</ItemTitle>
                <ItemDescription>{formatFileSize(file.size)}</ItemDescription>
              </ItemContent>
              <ItemActions className="shrink-0 gap-2">
                {loading ? (
                  <Badge variant="secondary">
                    <Spinner />
                    Parsing
                  </Badge>
                ) : (
                  <Badge variant="secondary">{studentCount} students</Badge>
                )}
                <Button asChild variant="outline" size="sm" disabled={loading}>
                  <Label htmlFor="roster-file-input" className="cursor-pointer">
                    <Upload />
                    Replace
                  </Label>
                </Button>
                <Button
                  variant="ghost"
                  size="icon-sm"
                  onClick={onRemoveFile}
                  disabled={loading}
                >
                  <X />
                  <span className="sr-only">Remove file</span>
                </Button>
              </ItemActions>
            </Item>
          ) : (
            <Label
              htmlFor="roster-file-input"
              onDragOver={(event): void => {
                event.preventDefault();
                setDragActive(true);
              }}
              onDragLeave={(): void => {
                setDragActive(false);
              }}
              onDrop={(event): void => {
                event.preventDefault();
                setDragActive(false);
                handleFile(event.dataTransfer.files?.[0]);
              }}
              className={
                "flex min-h-40 cursor-pointer flex-col items-center justify-center gap-2 " +
                "rounded-md border border-dashed text-center transition-colors " +
                (dragActive
                  ? "border-primary bg-primary/5"
                  : "border-border hover:bg-muted/40")
              }
            >
              <Upload className="size-6 text-muted-foreground" />
              <span className="text-sm font-medium">Click to upload or drag a file here</span>
              <span className="text-xs text-muted-foreground">CSV, Excel (.xlsx), or TXT</span>
            </Label>
          )}

          {typeError ? (
            <p className="text-xs text-destructive">{typeError}</p>
          ) : null}
        </div>

        <div className="space-y-2">
          <SortControls
            sortField={sortField}
            sortDirection={sortDirection}
            onSortFieldChange={onSortFieldChange}
            onSortDirectionChange={onSortDirectionChange}
            disabled={disabled}
          />
          <FieldGroup className="gap-2">
            <Field>
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
                {studentCount
                  ? "About " + perTeam + " students per team"
                  : "Upload a list first"}
              </FieldDescription>
            </Field>
            <Field>
              <FieldLabel>Program</FieldLabel>
              <Select
                value={program}
                onValueChange={(value): void => {
                  onProgramChange(value as StudentProgram);
                }}
                disabled={loading}
              >
                <SelectTrigger className="w-full">
                  <SelectValue placeholder="Select a program" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="INEL">INEL</SelectItem>
                  <SelectItem value="ICOM">ICOM</SelectItem>
                  <SelectItem value="INSO">INSO</SelectItem>
                  <SelectItem value="CIIC">CIIC</SelectItem>
                </SelectContent>
              </Select>
              <FieldDescription>
                Applied to every student when teams are generated
              </FieldDescription>
            </Field>
            <Field className="sm:col-span-2 xl:col-span-1">
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
                </TabsList>
              </Tabs>
            </Field>
          </FieldGroup>
        </div>
      </CardContent>
      <CardFooter className="justify-end border-t pt-4">
        <Button onClick={onGenerate} disabled={disabled || !program}>
          <Sparkles />
          {hasTeams ? "Generate teams again" : "Generate teams"}
        </Button>
      </CardFooter>
    </Card>
  );
}
