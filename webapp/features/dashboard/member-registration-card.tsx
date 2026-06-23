"use client";

import { useState } from "react";
import { Upload, Users } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Textarea } from "@/components/ui/textarea";
import type { BotRole, RegisterMemberRequest } from "@/features/dashboard/admin-types";

export function MemberRegistrationCard({
  roles,
  onRegister,
  onUpload,
}: {
  roles: BotRole[];
  onRegister: (values: RegisterMemberRequest) => Promise<void>;
  onUpload: (file: File) => Promise<void>;
}) {
  const [values, setValues] = useState<RegisterMemberRequest>({
    email: "",
    fullname: "",
    username: "",
    discord_user_id: "-",
    fun_fact: "",
    discord_role_ids: [],
  });

  function toggleRole(roleId: string, checked: boolean) {
    setValues((previous) => ({
      ...previous,
      discord_role_ids: checked
        ? [...previous.discord_role_ids, roleId]
        : previous.discord_role_ids.filter((item) => item !== roleId),
    }));
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Member registration</CardTitle>
        <CardDescription>{roles.length} selectable roles</CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <FieldGroup className="grid gap-4 md:grid-cols-2">
          <TextInput label="Email" value={values.email} onChange={(email) => setValues((previous) => ({ ...previous, email }))} />
          <TextInput label="Full name" value={values.fullname} onChange={(fullname) => setValues((previous) => ({ ...previous, fullname }))} />
          <TextInput label="Username" value={values.username} onChange={(username) => setValues((previous) => ({ ...previous, username }))} />
          <TextInput label="Discord user ID" value={values.discord_user_id} onChange={(discord_user_id) => setValues((previous) => ({ ...previous, discord_user_id }))} />
        </FieldGroup>
        <Field>
          <FieldLabel>Fun fact</FieldLabel>
          <Textarea value={values.fun_fact ?? ""} onChange={(event) => setValues((previous) => ({ ...previous, fun_fact: event.target.value }))} />
        </Field>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="w-10">Set</TableHead>
              <TableHead>Role</TableHead>
              <TableHead>Discord ID</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {roles.map((role) => (
              <TableRow key={role.role_id}>
                <TableCell>
                  <Checkbox
                    checked={Boolean(role.discord_role_id && values.discord_role_ids.includes(role.discord_role_id))}
                    disabled={!role.discord_role_id}
                    onCheckedChange={(checked) => role.discord_role_id && toggleRole(role.discord_role_id, checked === true)}
                  />
                </TableCell>
                <TableCell>{role.name}</TableCell>
                <TableCell>{role.discord_role_id}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
        <div className="flex flex-wrap gap-2">
          <Button onClick={() => void onRegister(values)}>
            <Users className="h-4 w-4" />
            Register
          </Button>
          <Button asChild variant="outline">
            <FieldLabel className="h-8 cursor-pointer px-2.5">
              <Upload className="h-4 w-4" />
              CSV
              <Input type="file" accept=".csv,text/csv" className="sr-only" onChange={(event) => {
                const file = event.target.files?.[0];
                if (file) void onUpload(file);
              }} />
            </FieldLabel>
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}

function TextInput({ label, value, onChange }: { label: string; value: string; onChange: (value: string) => void }) {
  return (
    <Field>
      <FieldLabel>{label}</FieldLabel>
      <Input value={value} onChange={(event) => onChange(event.target.value)} />
    </Field>
  );
}
