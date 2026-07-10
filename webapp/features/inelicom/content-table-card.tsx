"use client";

import { Trash2 } from "lucide-react";

import { Button } from "@/components/ui/button";
import {
  Card,
  CardAction,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { ContentDialog } from "@/features/inelicom/content-dialog";
import type {
  ContentData,
  ContentFormValues,
  ContentKind,
  ContentRecord,
} from "@/features/inelicom/content-types";

interface ContentTableCardProps {
  kind: ContentKind;
  title: string;
  description: string;
  records: ContentRecord[];
  data: ContentData;
  onSave: (
    kind: ContentKind,
    values: ContentFormValues,
    current?: ContentRecord
  ) => Promise<boolean>;
  onDelete: (kind: ContentKind, current: ContentRecord) => Promise<void>;
}

export function ContentTableCard({
  kind,
  title,
  description,
  records,
  data,
  onSave,
  onDelete,
}: ContentTableCardProps): React.ReactElement {
  return (
    <Card className="min-w-0 rounded-md border-border bg-card shadow-panel">
      <CardHeader>
        <CardTitle>{title}</CardTitle>
        <CardDescription>{description}</CardDescription>
        <CardAction>
          <ContentDialog
            kind={kind}
            data={data}
            onSave={onSave}
          />
        </CardAction>
      </CardHeader>
      <CardContent className="min-w-0">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Name</TableHead>
              {kind === "building" ? <TableHead>GPIN</TableHead> : null}
              {kind === "department" ? <TableHead>Faculty</TableHead> : null}
              {kind === "department" ? <TableHead>Building</TableHead> : null}
              <TableHead className="w-20">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {records.map((record) => (
              <TableRow key={recordId(kind, record)}>
                <TableCell className="font-medium">{record.name}</TableCell>
                {kind === "building" ? (
                  <TableCell>{"gpin" in record ? record.gpin : ""}</TableCell>
                ) : null}
                {kind === "department" ? (
                  <TableCell>{relationName(data, "faculty", record)}</TableCell>
                ) : null}
                {kind === "department" ? (
                  <TableCell>{relationName(data, "building", record)}</TableCell>
                ) : null}
                <TableCell>
                  <div className="flex gap-1">
                    <ContentDialog
                      kind={kind}
                      data={data}
                      current={record}
                      onSave={onSave}
                    />
                    <Button
                      variant="destructive"
                      size="icon-sm"
                      onClick={(): void => {
                        void onDelete(kind, record);
                      }}
                    >
                      <Trash2 />
                      <span className="sr-only">Delete {record.name}</span>
                    </Button>
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </CardContent>
    </Card>
  );
}

function recordId(kind: ContentKind, record: ContentRecord): number {
  if (kind === "faculty" && "facultyId" in record) return record.facultyId;
  if (kind === "building" && "buildingId" in record) return record.buildingId;
  return 0;
}

function relationName(
  data: ContentData,
  relation: "faculty" | "building",
  record: ContentRecord
): string {
  if (relation === "faculty" && "facultyId" in record) {
    return data.faculties.find((item) => item.facultyId === record.facultyId)?.name ?? "Unknown";
  }
  if (relation === "building" && "buildingId" in record) {
    return data.buildings.find((item) => item.buildingId === record.buildingId)?.name ?? "Unknown";
  }
  return "Unknown";
}
