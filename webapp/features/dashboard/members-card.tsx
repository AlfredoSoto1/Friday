"use client";

import { Check, X } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import type { BotMember, BotRole } from "@/features/dashboard/admin-types";

export function MembersCard({ members, roles }: { members: BotMember[]; roles: BotRole[] }) {
  const roleNames = new Map(roles.map((role) => [role.discord_role_id, role.name]));

  return (
    <Card>
      <CardHeader>
        <CardTitle>Server users</CardTitle>
        <CardDescription>{members.length} members registered</CardDescription>
      </CardHeader>
      <CardContent>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>User</TableHead>
              <TableHead>Discord</TableHead>
              <TableHead>Level</TableHead>
              <TableHead>Roles</TableHead>
              <TableHead>Verified</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {members.map((member) => (
              <TableRow key={member.server_user_id}>
                <TableCell>{member.fullname}</TableCell>
                <TableCell>{member.discord_user_id}</TableCell>
                <TableCell>{member.level}</TableCell>
                <TableCell>
                  <div className="flex flex-wrap gap-1">
                    {member.role_ids.map((roleId) => (
                      <Badge key={roleId} variant="outline">
                        {roleNames.get(roleId) ?? roleId}
                      </Badge>
                    ))}
                  </div>
                </TableCell>
                <TableCell>
                  {member.verified ? <Check className="h-4 w-4 text-success" /> : <X className="h-4 w-4 text-muted-foreground" />}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </CardContent>
    </Card>
  );
}
