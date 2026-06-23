"use client";

import { Trash2 } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { RecordDialog } from "@/components/custom/record-dialog";
import type { EditableRecord, FieldConfig, RecordValue } from "@/components/custom/record-fields";

export function RecordTable({
  rows,
  columns,
  idKey,
  fields,
  onSave,
  onDelete,
}: {
  rows: EditableRecord[];
  columns: string[];
  idKey: string;
  fields: FieldConfig[];
  onSave: (values: EditableRecord, current?: EditableRecord) => Promise<void>;
  onDelete: (row: EditableRecord) => Promise<void>;
}) {
  return (
    <Table>
      <TableHeader>
        <TableRow>
          {columns.map((column) => (
            <TableHead key={column}>{labelFromKey(column)}</TableHead>
          ))}
          <TableHead className="w-24">Actions</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {rows.map((row) => (
          <TableRow key={String(row[idKey])}>
            {columns.map((column) => (
              <TableCell key={column} className="max-w-80 truncate">
                {formatValue(row[column])}
              </TableCell>
            ))}
            <TableCell>
              <div className="flex gap-1">
                <RecordDialog title="Edit record" fields={fields} current={row} onSave={onSave} />
                <Button variant="destructive" size="icon-sm" onClick={() => void onDelete(row)}>
                  <Trash2 className="h-4 w-4" />
                </Button>
              </div>
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}

function formatValue(value: RecordValue): string {
  if (value === null || value === undefined) return "";
  if (typeof value === "boolean") return value ? "Yes" : "No";
  return String(value);
}

function labelFromKey(key: string): string {
  return key
    .split("_")
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
    .join(" ");
}
