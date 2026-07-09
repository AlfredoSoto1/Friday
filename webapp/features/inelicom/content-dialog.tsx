"use client";

import { useEffect, useState } from "react";
import { Pencil, Plus, Save } from "lucide-react";

import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import type {
  ContentData,
  ContentFormValues,
  ContentKind,
  ContentRecord,
} from "@/features/inelicom/content-types";

interface ContentDialogProps {
  kind: ContentKind;
  data: ContentData;
  current?: ContentRecord;
  onSave: (
    kind: ContentKind,
    values: ContentFormValues,
    current?: ContentRecord
  ) => Promise<boolean>;
}

const emptyValues: ContentFormValues = {
  name: "",
  gpin: "",
  facultyId: 0,
  buildingId: 0,
};

export function ContentDialog({
  kind,
  data,
  current,
  onSave,
}: ContentDialogProps): React.ReactElement {
  const [open, setOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [values, setValues] = useState<ContentFormValues>(emptyValues);

  useEffect((): void => {
    if (!open) {
      return;
    }

    setValues({
      name: current?.name ?? "",
      gpin: current && "gpin" in current ? current.gpin : "",
      facultyId: current && "facultyId" in current
        ? current.facultyId
        : data.faculties[0]?.facultyId ?? 0,
      buildingId: current && "buildingId" in current
        ? current.buildingId
        : data.buildings[0]?.buildingId ?? 0,
    });
  }, [current, data, open]);

  async function save(): Promise<void> {
    setSaving(true);
    const succeeded = await onSave(kind, values, current);
    setSaving(false);

    if (succeeded) {
      setOpen(false);
    }
  }

  const label = `${current ? "Edit" : "Add"} ${kind}`;
  const canSave = Boolean(
    values.name.trim()
    && (kind !== "building" || values.gpin.trim())
    && (kind !== "department" || (values.facultyId && values.buildingId))
  );

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button
          variant={current ? "ghost" : "default"}
          size={current ? "icon-sm" : "sm"}
          disabled={
            kind === "department" && (!data.faculties.length || !data.buildings.length)
          }
        >
          {current ? <Pencil /> : <Plus />}
          {current ? <span className="sr-only">{label}</span> : label}
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{label}</DialogTitle>
          <DialogDescription>
            Changes are saved directly to the INEL/ICOM catalog.
          </DialogDescription>
        </DialogHeader>
        <FieldGroup>
          <Field>
            <FieldLabel htmlFor={`${kind}-name`}>Name</FieldLabel>
            <Input
              id={`${kind}-name`}
              value={values.name}
              onChange={(event): void => {
                setValues((currentValues) => ({
                  ...currentValues,
                  name: event.target.value,
                }));
              }}
            />
          </Field>
          {kind === "building" ? (
            <Field>
              <FieldLabel htmlFor="building-gpin">GPIN</FieldLabel>
              <Input
                id="building-gpin"
                value={values.gpin}
                onChange={(event): void => {
                  setValues((currentValues) => ({
                    ...currentValues,
                    gpin: event.target.value,
                  }));
                }}
              />
            </Field>
          ) : null}
          {kind === "department" ? (
            <RelationSelect
              label="Faculty"
              value={values.facultyId}
              options={data.faculties.map((faculty) => ({
                id: faculty.facultyId,
                name: faculty.name,
              }))}
              onChange={(facultyId): void => {
                setValues((currentValues) => ({ ...currentValues, facultyId }));
              }}
            />
          ) : null}
          {kind === "department" ? (
            <RelationSelect
              label="Building"
              value={values.buildingId}
              options={data.buildings.map((building) => ({
                id: building.buildingId,
                name: building.name,
              }))}
              onChange={(buildingId): void => {
                setValues((currentValues) => ({ ...currentValues, buildingId }));
              }}
            />
          ) : null}
        </FieldGroup>
        <DialogFooter showCloseButton>
          <Button
            onClick={save}
            disabled={!canSave || saving}
          >
            <Save />
            Save
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

interface RelationSelectProps {
  label: string;
  value: number;
  options: Array<{ id: number; name: string }>;
  onChange: (value: number) => void;
}

function RelationSelect({
  label,
  value,
  options,
  onChange,
}: RelationSelectProps): React.ReactElement {
  return (
    <Field>
      <FieldLabel>{label}</FieldLabel>
      <Select
        value={value ? value.toString() : ""}
        onValueChange={(nextValue): void => {
          onChange(Number(nextValue));
        }}
      >
        <SelectTrigger className="w-full">
          <SelectValue placeholder={`Select ${label.toLowerCase()}`} />
        </SelectTrigger>
        <SelectContent>
          {options.map((option) => (
            <SelectItem key={option.id} value={option.id.toString()}>
              {option.name}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </Field>
  );
}
