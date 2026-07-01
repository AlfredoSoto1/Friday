"use client";

import { ArrowDownAZ, ArrowUpAZ, SlidersHorizontal } from "lucide-react";

import {
  Card,
  CardAction,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { ToggleGroup, ToggleGroupItem } from "@/components/ui/toggle-group";
import type { SortDirection, SortField } from "@/features/roster/roster-types";

const SORT_FIELDS: Array<{ value: SortField; label: string }> = [
  { value: "name", label: "Name" },
  { value: "studentId", label: "Student ID" },
  { value: "major", label: "Major" },
  { value: "year", label: "Class year" },
  { value: "gpa", label: "GPA" },
];

interface SortControlCardProps {
  sortField: SortField;
  sortDirection: SortDirection;
  onSortFieldChange: (field: SortField) => void;
  onSortDirectionChange: (direction: SortDirection) => void;
  disabled: boolean;
}

export function SortControlCard({
  sortField,
  sortDirection,
  onSortFieldChange,
  onSortDirectionChange,
  disabled,
}: SortControlCardProps): React.ReactElement {
  return (
    <Card className="min-w-0 rounded-md border-border bg-card shadow-panel">
      <CardHeader>
        <CardTitle>Sorting control panel</CardTitle>
        <CardDescription>
          Choose how students are ordered before they&apos;re split into teams.
        </CardDescription>
        <CardAction>
          <SlidersHorizontal className="size-5 text-muted-foreground" />
        </CardAction>
      </CardHeader>
      <CardContent>
        <FieldGroup className="gap-4 sm:flex-row sm:items-end">
          <Field className="sm:max-w-56">
            <FieldLabel htmlFor="roster-sort-field">Sort by</FieldLabel>
            <Select
              value={sortField}
              onValueChange={(value): void => onSortFieldChange(value as SortField)}
              disabled={disabled}
            >
              <SelectTrigger id="roster-sort-field" className="w-full">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {SORT_FIELDS.map((option) => (
                  <SelectItem key={option.value} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </Field>

          <Field className="sm:max-w-56">
            <FieldLabel>Order</FieldLabel>
            <ToggleGroup
              type="single"
              variant="outline"
              value={sortDirection}
              onValueChange={(value): void => {
                if (value) {
                  onSortDirectionChange(value as SortDirection);
                }
              }}
              disabled={disabled}
            >
              <ToggleGroupItem value="asc" className="gap-1.5">
                <ArrowDownAZ />
                Ascending
              </ToggleGroupItem>
              <ToggleGroupItem value="desc" className="gap-1.5">
                <ArrowUpAZ />
                Descending
              </ToggleGroupItem>
            </ToggleGroup>
          </Field>
        </FieldGroup>
      </CardContent>
    </Card>
  );
}
