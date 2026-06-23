"use client";

import { Checkbox } from "@/components/ui/checkbox";
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";

export type RecordValue = string | number | boolean | null | undefined;
export type EditableRecord = Record<string, RecordValue>;

export interface FieldConfig {
  key: string;
  label: string;
  type?: "text" | "number" | "textarea" | "checkbox";
}

export function RecordFields({
  fields,
  values,
  onChange,
}: {
  fields: FieldConfig[];
  values: EditableRecord;
  onChange: (key: string, value: RecordValue) => void;
}) {
  return (
    <FieldGroup>
      {fields.map((field) => (
        <EditorField
          key={field.key}
          field={field}
          value={values[field.key]}
          onChange={(value) => onChange(field.key, value)}
        />
      ))}
    </FieldGroup>
  );
}

function EditorField({
  field,
  value,
  onChange,
}: {
  field: FieldConfig;
  value: RecordValue;
  onChange: (value: RecordValue) => void;
}) {
  if (field.type === "checkbox") {
    return (
      <Field orientation="horizontal">
        <Checkbox checked={Boolean(value)} onCheckedChange={(checked) => onChange(checked === true)} />
        <FieldLabel>{field.label}</FieldLabel>
      </Field>
    );
  }

  return (
    <Field>
      <FieldLabel>{field.label}</FieldLabel>
      {field.type === "textarea" ? (
        <Textarea value={String(value ?? "")} onChange={(event) => onChange(event.target.value)} />
      ) : (
        <Input
          type={field.type === "number" ? "number" : "text"}
          value={String(value ?? "")}
          onChange={(event) => onChange(field.type === "number" ? numberOrNull(event.target.value) : event.target.value)}
        />
      )}
    </Field>
  );
}

export function seedValues(fields: FieldConfig[], current?: EditableRecord): EditableRecord {
  return Object.fromEntries(fields.map((field) => [field.key, current?.[field.key] ?? defaultForField(field)]));
}

export function normalizeRecord(values: EditableRecord, fields: FieldConfig[]): EditableRecord {
  return Object.fromEntries(
    fields.map((field) => {
      const value = values[field.key];
      if (field.type === "number") return [field.key, numberOrNull(value)];
      if (field.type === "checkbox") return [field.key, Boolean(value)];
      return [field.key, value === "" ? null : value];
    })
  );
}

export function numberOrNull(value: RecordValue): number | null {
  if (value === null || value === undefined || value === "") return null;
  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : null;
}

function defaultForField(field: FieldConfig): RecordValue {
  if (field.type === "checkbox") return false;
  if (field.type === "number") return null;
  return "";
}
