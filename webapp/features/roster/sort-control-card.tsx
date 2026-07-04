"use client";

import { ArrowDownAZ, ArrowUpAZ } from "lucide-react";

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
  { value: "firstName", label: "Name" },
  { value: "firstLastName", label: "First Last Name" },
  { value: "secondLastName", label: "Second Last Name" },
];

interface SortControlsProps {
  sortField: SortField;
  sortDirection: SortDirection;
  onSortFieldChange: (field: SortField) => void;
  onSortDirectionChange: (direction: SortDirection) => void;
  disabled: boolean;
}

export function SortControls({
  sortField,
  sortDirection,
  onSortFieldChange,
  onSortDirectionChange,
  disabled,
}: SortControlsProps): React.ReactElement {
  return (
    <FieldGroup className="gap-4 sm:flex-row sm:items-end">
      <Field className="sm:max-w-56">
        <FieldLabel htmlFor="roster-sort-field">Sort by</FieldLabel>
        <Select
          value={sortField}
          onValueChange={(value): void => {
            onSortFieldChange(value as SortField);
          }}
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
  );
}
