"use client";

import { useEffect, useState } from "react";
import { Save } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card, CardAction, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import { Field, FieldGroup, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import type { BotChannel, BotRole, GuildProfile } from "@/features/dashboard/admin-types";

export function GuildSettingsCard({
  profile,
  roles,
  channels,
  onSave,
}: {
  profile: GuildProfile;
  roles: BotRole[];
  channels: BotChannel[];
  onSave: (profile: GuildProfile) => Promise<void>;
}) {
  const [values, setValues] = useState(profile);

  useEffect(() => setValues(profile), [profile]);

  return (
    <Card>
      <CardHeader>
        <CardTitle>Server settings</CardTitle>
        <CardDescription>{profile.guild_id}</CardDescription>
        <CardAction>
          <Button onClick={() => void onSave(values)}>
            <Save className="h-4 w-4" />
            Save
          </Button>
        </CardAction>
      </CardHeader>
      <CardContent className="space-y-4">
        <FieldGroup className="grid gap-4 md:grid-cols-2">
          <TextInput label="Name" value={values.name} onChange={(name) => setValues((previous) => ({ ...previous, name }))} />
          <Field orientation="horizontal">
            <Checkbox checked={values.enabled} onCheckedChange={(checked) => setValues((previous) => ({ ...previous, enabled: checked === true }))} />
            <FieldLabel>Enabled</FieldLabel>
          </Field>
          <TextInput label="Primary color" value={values.theme.primary_color} onChange={(primary_color) => setValues((previous) => ({ ...previous, theme: { ...previous.theme, primary_color } }))} />
          <TextInput label="Footer" value={values.theme.footer_text ?? ""} onChange={(footer_text) => setValues((previous) => ({ ...previous, theme: { ...previous.theme, footer_text } }))} />
          <TextInput label="Verification title" value={values.verification.title} onChange={(title) => setValues((previous) => ({ ...previous, verification: { ...previous.verification, title } }))} />
          <TextInput label="Verification button" value={values.verification.button_label} onChange={(button_label) => setValues((previous) => ({ ...previous, verification: { ...previous.verification, button_label } }))} />
        </FieldGroup>
        <TextAreaInput label="Verification description" value={values.verification.description} onChange={(description) => setValues((previous) => ({ ...previous, verification: { ...previous.verification, description } }))} />
        <FieldGroup className="grid gap-4 md:grid-cols-2">
          <SelectField label="Verification role" value={values.verification.verified_role_id ?? ""} items={roleItems(roles)} onChange={(verified_role_id) => setValues((previous) => ({ ...previous, verification: { ...previous.verification, verified_role_id } }))} />
          <SelectField label="Verification channel" value={values.verification.channel_id ?? ""} items={channelItems(channels)} onChange={(channel_id) => setValues((previous) => ({ ...previous, verification: { ...previous.verification, channel_id } }))} />
          <TextInput label="Welcome title" value={values.welcome.title} onChange={(title) => setValues((previous) => ({ ...previous, welcome: { ...previous.welcome, title } }))} />
          <SelectField label="Welcome channel" value={values.welcome.channel_id ?? ""} items={channelItems(channels)} onChange={(channel_id) => setValues((previous) => ({ ...previous, welcome: { ...previous.welcome, channel_id } }))} />
        </FieldGroup>
        <TextAreaInput label="Welcome description" value={values.welcome.description} onChange={(description) => setValues((previous) => ({ ...previous, welcome: { ...previous.welcome, description } }))} />
      </CardContent>
    </Card>
  );
}

function TextInput({ label, value, onChange }: { label: string; value: string; onChange: (value: string) => void }) {
  return <Field><FieldLabel>{label}</FieldLabel><Input value={value} onChange={(event) => onChange(event.target.value)} /></Field>;
}

function TextAreaInput({ label, value, onChange }: { label: string; value: string; onChange: (value: string) => void }) {
  return <Field><FieldLabel>{label}</FieldLabel><Textarea value={value} onChange={(event) => onChange(event.target.value)} /></Field>;
}

function SelectField({ label, value, items, onChange }: { label: string; value: string; items: { value: string; label: string }[]; onChange: (value: string) => void }) {
  return (
    <Field>
      <FieldLabel>{label}</FieldLabel>
      <Select value={value} onValueChange={onChange}>
        <SelectTrigger className="w-full"><SelectValue /></SelectTrigger>
        <SelectContent>{items.map((item) => <SelectItem key={item.value} value={item.value}>{item.label}</SelectItem>)}</SelectContent>
      </Select>
    </Field>
  );
}

function roleItems(roles: BotRole[]) {
  return roles.filter((role) => role.discord_role_id).map((role) => ({ value: role.discord_role_id ?? "", label: role.name }));
}

function channelItems(channels: BotChannel[]) {
  return channels.map((channel) => ({ value: channel.discord_channel_id, label: channel.name }));
}
