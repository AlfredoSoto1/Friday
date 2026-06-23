"use client";

import { Trash2 } from "lucide-react";

import { RecordDialog } from "@/components/custom/record-dialog";
import type { EditableRecord } from "@/components/custom/record-fields";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardAction, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import type { BotRole } from "@/features/dashboard/admin-types";
import { roleFields } from "@/features/dashboard/dashboard-config";
import { toEditableRecord } from "@/features/dashboard/dashboard-utils";

export function BotRolesCard({
  roles,
  onSave,
  onDelete,
}: {
  roles: BotRole[];
  onSave: (values: EditableRecord, current?: BotRole) => Promise<void>;
  onDelete: (role: BotRole) => Promise<void>;
}) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Roles</CardTitle>
        <CardDescription>{roles.length} roles synced</CardDescription>
        <CardAction>
          <RecordDialog title="New role" fields={roleFields} onSave={(values) => onSave(values)} />
        </CardAction>
      </CardHeader>
      <CardContent>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Name</TableHead>
              <TableHead>Discord ID</TableHead>
              <TableHead>Position</TableHead>
              <TableHead>Flags</TableHead>
              <TableHead className="w-24">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {roles.map((role) => (
              <TableRow key={role.role_id}>
                <TableCell>{role.name}</TableCell>
                <TableCell>{role.discord_role_id}</TableCell>
                <TableCell>{role.position}</TableCell>
                <TableCell><RoleFlags role={role} /></TableCell>
                <TableCell>
                  <div className="flex gap-1">
                    <RecordDialog title="Edit role" fields={roleFields} current={toEditableRecord(role)} onSave={(values) => onSave(values, role)} />
                    <Button variant="destructive" size="icon-sm" onClick={() => void onDelete(role)}>
                      <Trash2 className="h-4 w-4" />
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

function RoleFlags({ role }: { role: BotRole }) {
  return (
    <div className="flex gap-1">
      {role.managed && <Badge variant="outline">Managed</Badge>}
      {role.mentionable && <Badge variant="outline">Mentionable</Badge>}
      {role.hoisted && <Badge variant="outline">Hoisted</Badge>}
    </div>
  );
}
