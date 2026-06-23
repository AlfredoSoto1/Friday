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
import {
  type EditableRecord,
  type FieldConfig,
  RecordFields,
  seedValues,
} from "@/components/custom/record-fields";

export function RecordDialog({
  title,
  fields,
  current,
  onSave,
}: {
  title: string;
  fields: FieldConfig[];
  current?: EditableRecord;
  onSave: (values: EditableRecord, current?: EditableRecord) => Promise<void>;
}) {
  const [open, setOpen] = useState(false);
  const [values, setValues] = useState<EditableRecord>(() => seedValues(fields, current));

  useEffect(() => {
    if (open) setValues(seedValues(fields, current));
  }, [current, fields, open]);

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button variant={current ? "ghost" : "default"} size={current ? "icon-sm" : "sm"}>
          {current ? <Pencil className="h-4 w-4" /> : <Plus className="h-4 w-4" />}
          {!current && "Add"}
        </Button>
      </DialogTrigger>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>{fields.length} editable fields</DialogDescription>
        </DialogHeader>
        <RecordFields
          fields={fields}
          values={values}
          onChange={(key, value) => setValues((previous) => ({ ...previous, [key]: value }))}
        />
        <DialogFooter>
          <Button
            onClick={async () => {
              await onSave(values, current);
              setOpen(false);
            }}
          >
            <Save className="h-4 w-4" />
            Save
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
