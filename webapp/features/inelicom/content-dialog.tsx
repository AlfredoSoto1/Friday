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
  );

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button
          variant={current ? "ghost" : "default"}
          size={current ? "icon-sm" : "sm"}
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
