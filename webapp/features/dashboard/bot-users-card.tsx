"use client";

import { Trash2 } from "lucide-react";

import { RecordDialog } from "@/components/custom/record-dialog";
import type { EditableRecord } from "@/components/custom/record-fields";
import { Button } from "@/components/ui/button";
import { Card, CardAction, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import type { BotUser } from "@/features/dashboard/admin-types";
import { userFields } from "@/features/dashboard/dashboard-config";
import { toEditableRecord } from "@/features/dashboard/dashboard-utils";

export function BotUsersCard({
  users,
  onSave,
  onDelete,
}: {
  users: BotUser[];
  onSave: (values: EditableRecord, current?: BotUser) => Promise<void>;
  onDelete: (user: BotUser) => Promise<void>;
}) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Users</CardTitle>
        <CardDescription>{users.length} user records</CardDescription>
        <CardAction>
          <RecordDialog title="New user" fields={userFields} onSave={(values) => onSave(values)} />
        </CardAction>
      </CardHeader>
      <CardContent>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Email</TableHead>
              <TableHead>Full name</TableHead>
              <TableHead>Username</TableHead>
              <TableHead className="w-24">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {users.map((user) => (
              <TableRow key={user.user_id}>
                <TableCell>{user.email}</TableCell>
                <TableCell>{user.fullname}</TableCell>
                <TableCell>{user.username}</TableCell>
                <TableCell>
                  <div className="flex gap-1">
                    <RecordDialog
                      title="Edit user"
                      fields={userFields}
                      current={toEditableRecord(user)}
                      onSave={(values) => onSave(values, user)}
                    />
                    <Button variant="destructive" size="icon-sm" onClick={() => void onDelete(user)}>
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
