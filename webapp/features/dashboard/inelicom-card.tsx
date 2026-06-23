"use client";

import { RecordDialog } from "@/components/custom/record-dialog";
import { RecordTable } from "@/components/custom/record-table";
import type { EditableRecord } from "@/components/custom/record-fields";
import { Card, CardAction, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import type { AdminRecord } from "@/features/dashboard/admin-types";
import { inelicomResources, type ResourceConfig } from "@/features/dashboard/dashboard-config";

export function InelicomCard({
  resource,
  records,
  onResourceChange,
  onSave,
  onDelete,
}: {
  resource: ResourceConfig;
  records: AdminRecord[];
  onResourceChange: (key: string) => void;
  onSave: (values: EditableRecord, current?: EditableRecord) => Promise<void>;
  onDelete: (row: EditableRecord) => Promise<void>;
}) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Inelicom data</CardTitle>
        <CardDescription>{records.length} records loaded</CardDescription>
        <CardAction>
          <div className="flex gap-2">
            <Select value={resource.key} onValueChange={onResourceChange}>
              <SelectTrigger className="w-44">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {inelicomResources.map((item) => (
                  <SelectItem key={item.key} value={item.key}>
                    {item.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <RecordDialog title={`New ${resource.label}`} fields={resource.fields} onSave={onSave} />
          </div>
        </CardAction>
      </CardHeader>
      <CardContent>
        <RecordTable
          rows={records as EditableRecord[]}
          columns={resource.columns}
          idKey={resource.idKey}
          fields={resource.fields}
          onSave={onSave}
          onDelete={onDelete}
        />
      </CardContent>
    </Card>
  );
}
