"use client";

import { useEffect, useMemo, useState } from "react";
import {
  Bot,
  Check,
  Database,
  Pencil,
  Plus,
  RefreshCcw,
  Save,
  Server,
  Trash2,
  Upload,
  Users,
  X,
} from "lucide-react";

import {
  createBotUser,
  createGuildChannel,
  createGuildRole,
  createInelicom,
  deleteBotUser,
  deleteGuildChannel,
  deleteGuildRole,
  deleteInelicom,
  getBotUsers,
  getDashboardData,
  getGuildChannels,
  getGuildMembers,
  getGuildProfile,
  getGuildRoles,
  getGuilds,
  listInelicom,
  registerGuildMember,
  registerGuildMembers,
  updateBotUser,
  updateGuildChannel,
  updateGuildProfile,
  updateGuildRole,
  updateInelicom,
} from "@/features/dashboard/admin-api";
import type {
  AdminRecord,
  BotChannel,
  BotMember,
  BotRole,
  BotUser,
  DashboardDataEntity,
  GuildProfile,
  GuildSummary,
  RecordValue,
  RegisterMemberRequest,
} from "@/features/dashboard/admin-types";
import { getDashboardModel, type HealthTone } from "@/features/dashboard/dashboard-model";
import { cn } from "@/lib/utils";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardAction, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
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
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Textarea } from "@/components/ui/textarea";
import { RuntimeCard } from "@/widgets/runtime-card";
import { SchemaCatalog } from "@/widgets/schema-catalog";
import { StatusCard } from "@/widgets/status-card";

interface FieldConfig {
  key: string;
  label: string;
  type?: "text" | "number" | "textarea" | "checkbox";
}

interface ResourceConfig {
  key: string;
  label: string;
  idKey: string;
  fields: FieldConfig[];
  columns: string[];
}

const inelicomResources: ResourceConfig[] = [
  {
    key: "contacts",
    label: "Contacts",
    idKey: "contact_id",
    fields: [
      { key: "name", label: "Name" },
      { key: "email", label: "Email" },
      { key: "phone", label: "Phone" },
      { key: "website", label: "Website" },
    ],
    columns: ["contact_id", "name", "email", "phone", "website"],
  },
  {
    key: "faculties",
    label: "Faculties",
    idKey: "faculty_id",
    fields: [{ key: "name", label: "Name" }],
    columns: ["faculty_id", "name"],
  },
  {
    key: "buildings",
    label: "Buildings",
    idKey: "building_id",
    fields: [
      { key: "name", label: "Name" },
      { key: "gpin", label: "GPIN" },
    ],
    columns: ["building_id", "name", "gpin"],
  },
  {
    key: "departments",
    label: "Departments",
    idKey: "department_id",
    fields: [
      { key: "name", label: "Name" },
      { key: "faculty_id", label: "Faculty ID", type: "number" },
      { key: "building_id", label: "Building ID", type: "number" },
    ],
    columns: ["department_id", "name", "faculty_id", "building_id"],
  },
  {
    key: "rooms",
    label: "Rooms",
    idKey: "room_id",
    fields: [
      { key: "code", label: "Code" },
      { key: "name", label: "Name" },
      { key: "building_id", label: "Building ID", type: "number" },
      { key: "department_id", label: "Department ID", type: "number" },
    ],
    columns: ["room_id", "code", "name", "building_id", "department_id"],
  },
  {
    key: "projects",
    label: "Projects",
    idKey: "project_id",
    fields: [
      { key: "name", label: "Name" },
      { key: "description", label: "Description", type: "textarea" },
    ],
    columns: ["project_id", "name", "description"],
  },
  {
    key: "organizations",
    label: "Organizations",
    idKey: "organization_id",
    fields: [
      { key: "name", label: "Name" },
      { key: "description", label: "Description", type: "textarea" },
    ],
    columns: ["organization_id", "name", "description"],
  },
];

const statusIcons = [Server, Database, Bot, Database];

interface DashboardAdminProps {
  initialData: DashboardDataEntity;
}

export function DashboardAdmin({ initialData }: DashboardAdminProps) {
  const [dashboard, setDashboard] = useState(initialData);
  const [guilds, setGuilds] = useState<GuildSummary[]>([]);
  const [selectedGuildId, setSelectedGuildId] = useState<number | null>(null);
  const [profile, setProfile] = useState<GuildProfile | null>(null);
  const [botUsers, setBotUsers] = useState<BotUser[]>([]);
  const [members, setMembers] = useState<BotMember[]>([]);
  const [roles, setRoles] = useState<BotRole[]>([]);
  const [channels, setChannels] = useState<BotChannel[]>([]);
  const [resourceKey, setResourceKey] = useState(inelicomResources[0].key);
  const [records, setRecords] = useState<AdminRecord[]>([]);
  const [message, setMessage] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const model = getDashboardModel(dashboard);
  const resource = useMemo(
    () => inelicomResources.find((item) => item.key === resourceKey) ?? inelicomResources[0],
    [resourceKey]
  );

  useEffect(() => {
    void refreshShell();
  }, []);

  useEffect(() => {
    void loadInelicom(resource.key);
  }, [resource.key]);

  useEffect(() => {
    if (selectedGuildId) {
      void loadGuild(selectedGuildId);
    }
  }, [selectedGuildId]);

  async function refreshShell() {
    setLoading(true);
    try {
      const [nextDashboard, nextGuilds, nextUsers] = await Promise.all([
        getDashboardData(),
        getGuilds(),
        getBotUsers(),
      ]);
      setDashboard(nextDashboard);
      setGuilds(nextGuilds);
      setBotUsers(nextUsers);
      setSelectedGuildId((current) => current ?? nextGuilds[0]?.guild_id ?? null);
      setMessage("Data refreshed.");
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "Could not refresh data.");
    } finally {
      setLoading(false);
    }
  }

  async function loadInelicom(key: string) {
    try {
      setRecords(await listInelicom(key));
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "Could not load records.");
    }
  }

  async function loadGuild(guildId: number) {
    try {
      const [nextProfile, nextMembers, nextRoles, nextChannels] = await Promise.all([
        getGuildProfile(guildId),
        getGuildMembers(guildId),
        getGuildRoles(guildId),
        getGuildChannels(guildId),
      ]);
      setProfile(nextProfile);
      setMembers(nextMembers);
      setRoles(nextRoles);
      setChannels(nextChannels);
    } catch (error) {
      setMessage(error instanceof Error ? error.message : "Could not load guild data.");
    }
  }

  async function saveInelicom(values: AdminRecord, current?: AdminRecord) {
    const payload = normalizeRecord(values, resource.fields);
    if (current) {
      await updateInelicom(resource.key, Number(current[resource.idKey]), payload);
    } else {
      await createInelicom(resource.key, payload);
    }
    await loadInelicom(resource.key);
    setMessage(`${resource.label} saved.`);
  }

  async function removeInelicom(row: AdminRecord) {
    await deleteInelicom(resource.key, Number(row[resource.idKey]));
    await loadInelicom(resource.key);
    setMessage(`${resource.label} deleted.`);
  }

  async function saveBotUser(values: AdminRecord, current?: BotUser) {
    const payload = {
      email: String(values.email ?? ""),
      fullname: String(values.fullname ?? ""),
      username: String(values.username ?? ""),
    };

    if (current) {
      await updateBotUser(current.user_id, payload);
    } else {
      await createBotUser(payload);
    }

    setBotUsers(await getBotUsers());
    setMessage("User saved.");
  }

  async function saveRole(values: AdminRecord, current?: BotRole) {
    if (!selectedGuildId) {
      return;
    }

    const payload = normalizeRecord(values, roleFields);
    if (current) {
      await updateGuildRole(selectedGuildId, current.role_id, payload);
    } else {
      await createGuildRole(selectedGuildId, payload);
    }
    setRoles(await getGuildRoles(selectedGuildId));
    setMessage("Role saved.");
  }

  async function saveChannel(values: AdminRecord, current?: BotChannel) {
    if (!selectedGuildId) {
      return;
    }

    const payload = normalizeRecord(values, channelFields);
    if (current) {
      await updateGuildChannel(selectedGuildId, current.channel_id, payload);
    } else {
      await createGuildChannel(selectedGuildId, payload);
    }
    setChannels(await getGuildChannels(selectedGuildId));
    setMessage("Channel saved.");
  }

  async function registerMember(values: RegisterMemberRequest) {
    if (!selectedGuildId) {
      return;
    }

    await registerGuildMember(selectedGuildId, values);
    setMembers(await getGuildMembers(selectedGuildId));
    setMessage("Member registered.");
  }

  async function uploadMembers(file: File) {
    if (!selectedGuildId) {
      return;
    }

    const text = await file.text();
    const membersToRegister = parseMembersCsv(text);
    await registerGuildMembers(selectedGuildId, membersToRegister);
    setMembers(await getGuildMembers(selectedGuildId));
    setMessage(`${membersToRegister.length} members registered.`);
  }

  return (
    <main className="min-h-screen bg-background">
      <header className="border-b border-border bg-background/80">
        <div className="mx-auto flex max-w-7xl items-center justify-between px-6 py-4">
          <div className="flex items-center gap-3">
            <div className="flex h-9 w-9 items-center justify-center rounded-md bg-discord text-white">
              <Bot className="h-5 w-5" />
            </div>
            <div>
              <h1 className="text-lg font-semibold tracking-normal">Friday</h1>
              <p className="text-sm text-muted-foreground">Incoming student operations</p>
            </div>
          </div>
          <div className="flex items-center gap-2">
            {message && <Badge variant="outline">{message}</Badge>}
            <Button variant="outline" size="sm" onClick={refreshShell} disabled={loading}>
              <RefreshCcw className={cn("h-4 w-4", loading && "animate-spin")} />
              Refresh
            </Button>
          </div>
        </div>
      </header>

      <section className="mx-auto max-w-7xl px-6 py-8">
        <Tabs defaultValue="overview" className="gap-6">
          <TabsList>
            <TabsTrigger value="overview">Overview</TabsTrigger>
            <TabsTrigger value="inelicom">Inelicom</TabsTrigger>
            <TabsTrigger value="bot">Bot</TabsTrigger>
          </TabsList>

          <TabsContent value="overview" className="space-y-6">
            <div className="flex flex-wrap items-end justify-between gap-4">
              <div>
                <div className="mb-2 flex items-center gap-2 text-sm font-medium text-discord">
                  <Server className="h-4 w-4" />
                  Control center
                </div>
                <h2 className="text-3xl font-semibold tracking-normal">Services and database</h2>
              </div>
              <HealthBadge tone={model.healthTone}>{model.healthLabel}</HealthBadge>
            </div>

            <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
              {model.statuses.map((status, index) => (
                <StatusCard
                  key={status.label}
                  icon={statusIcons[index]}
                  label={status.label}
                  value={status.value}
                  ok={status.ok}
                />
              ))}
            </div>

            <div className="grid gap-6 lg:grid-cols-[1fr_360px]">
              <SchemaCatalog schemas={model.schemas} />
              <RuntimeCard
                metrics={model.runtimeMetrics}
                backendOk={model.backendOk}
                databaseOk={model.databaseOk}
              />
            </div>
          </TabsContent>

          <TabsContent value="inelicom" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle>Inelicom data</CardTitle>
                <CardDescription>{records.length} records loaded</CardDescription>
                <CardAction>
                  <div className="flex gap-2">
                    <Select value={resource.key} onValueChange={setResourceKey}>
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
                    <RecordDialog title={`New ${resource.label}`} fields={resource.fields} onSave={saveInelicom} />
                  </div>
                </CardAction>
              </CardHeader>
              <CardContent>
                <RecordTable
                  rows={records}
                  columns={resource.columns}
                  idKey={resource.idKey}
                  fields={resource.fields}
                  onSave={saveInelicom}
                  onDelete={removeInelicom}
                />
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="bot" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle>Bot servers</CardTitle>
                <CardDescription>{guilds.length} servers available</CardDescription>
                <CardAction>
                  <Select
                    value={selectedGuildId?.toString() ?? ""}
                    onValueChange={(value) => setSelectedGuildId(Number(value))}
                  >
                    <SelectTrigger className="w-64">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {guilds.map((guild) => (
                        <SelectItem key={guild.guild_id} value={guild.guild_id.toString()}>
                          {guild.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </CardAction>
              </CardHeader>
            </Card>

            <div className="grid gap-4 xl:grid-cols-2">
              {profile && selectedGuildId && (
                <GuildSettingsCard
                  profile={profile}
                  roles={roles}
                  channels={channels}
                  onSave={async (nextProfile) => {
                    const saved = await updateGuildProfile(selectedGuildId, nextProfile);
                    setProfile(saved);
                    setMessage("Server settings saved.");
                  }}
                />
              )}
              <MemberRegistrationCard roles={roles} onRegister={registerMember} onUpload={uploadMembers} />
            </div>

            <div className="grid gap-4 xl:grid-cols-2">
              <BotUsersCard users={botUsers} onSave={saveBotUser} onDelete={async (user) => {
                await deleteBotUser(user.user_id);
                setBotUsers(await getBotUsers());
                setMessage("User deleted.");
              }} />
              <MembersCard members={members} roles={roles} />
            </div>

            <div className="grid gap-4 xl:grid-cols-2">
              <BotRolesCard roles={roles} onSave={saveRole} onDelete={async (role) => {
                if (!selectedGuildId) return;
                await deleteGuildRole(selectedGuildId, role.role_id);
                setRoles(await getGuildRoles(selectedGuildId));
                setMessage("Role deleted.");
              }} />
              <BotChannelsCard channels={channels} onSave={saveChannel} onDelete={async (channel) => {
                if (!selectedGuildId) return;
                await deleteGuildChannel(selectedGuildId, channel.channel_id);
                setChannels(await getGuildChannels(selectedGuildId));
                setMessage("Channel deleted.");
              }} />
            </div>
          </TabsContent>
        </Tabs>
      </section>
    </main>
  );
}

function HealthBadge({ tone, children }: { tone: HealthTone; children: React.ReactNode }) {
  return (
    <Badge
      variant="outline"
      className={cn(
        "h-6 rounded-md px-2",
        tone === "ok"
          ? "border-success/40 bg-success/10 text-success"
          : "border-warning/40 bg-warning/10 text-warning"
      )}
    >
      {children}
    </Badge>
  );
}

function RecordTable({
  rows,
  columns,
  idKey,
  fields,
  onSave,
  onDelete,
}: {
  rows: AdminRecord[];
  columns: string[];
  idKey: string;
  fields: FieldConfig[];
  onSave: (values: AdminRecord, current?: AdminRecord) => Promise<void>;
  onDelete: (row: AdminRecord) => Promise<void>;
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

function RecordDialog({
  title,
  fields,
  current,
  onSave,
}: {
  title: string;
  fields: FieldConfig[];
  current?: AdminRecord;
  onSave: (values: AdminRecord, current?: AdminRecord) => Promise<void>;
}) {
  const [open, setOpen] = useState(false);
  const [values, setValues] = useState<AdminRecord>(() => seedValues(fields, current));

  useEffect(() => {
    if (open) {
      setValues(seedValues(fields, current));
    }
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
        <FieldGroup>
          {fields.map((field) => (
            <EditorField
              key={field.key}
              field={field}
              value={values[field.key]}
              onChange={(value) => setValues((previous) => ({ ...previous, [field.key]: value }))}
            />
          ))}
        </FieldGroup>
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

function EditorField({
  field,
  value,
  onChange,
}: {
  field: FieldConfig;
  value: RecordValue;
  onChange: (value: RecordValue) => void;
}) {
  if (field.type === "checkbox") {
    return (
      <Field orientation="horizontal">
        <Checkbox checked={Boolean(value)} onCheckedChange={(checked) => onChange(checked === true)} />
        <FieldLabel>{field.label}</FieldLabel>
      </Field>
    );
  }

  return (
    <Field>
      <FieldLabel>{field.label}</FieldLabel>
      {field.type === "textarea" ? (
        <Textarea value={String(value ?? "")} onChange={(event) => onChange(event.target.value)} />
      ) : (
        <Input
          type={field.type === "number" ? "number" : "text"}
          value={String(value ?? "")}
          onChange={(event) => onChange(field.type === "number" ? numberOrNull(event.target.value) : event.target.value)}
        />
      )}
    </Field>
  );
}

const userFields: FieldConfig[] = [
  { key: "email", label: "Email" },
  { key: "fullname", label: "Full name" },
  { key: "username", label: "Username" },
];

const roleFields: FieldConfig[] = [
  { key: "discord_role_id", label: "Discord role ID" },
  { key: "name", label: "Name" },
  { key: "color", label: "Color", type: "number" },
  { key: "position", label: "Position", type: "number" },
  { key: "managed", label: "Managed", type: "checkbox" },
  { key: "mentionable", label: "Mentionable", type: "checkbox" },
  { key: "hoisted", label: "Hoisted", type: "checkbox" },
];

const channelFields: FieldConfig[] = [
  { key: "discord_channel_id", label: "Discord channel ID" },
  { key: "parent_channel_id", label: "Parent channel ID" },
  { key: "name", label: "Name" },
  { key: "type", label: "Type" },
  { key: "position", label: "Position", type: "number" },
  { key: "topic", label: "Topic", type: "textarea" },
  { key: "nsfw", label: "NSFW", type: "checkbox" },
];

function BotUsersCard({
  users,
  onSave,
  onDelete,
}: {
  users: BotUser[];
  onSave: (values: AdminRecord, current?: BotUser) => Promise<void>;
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
                    <RecordDialog title="Edit user" fields={userFields} current={user as unknown as AdminRecord} onSave={(values) => onSave(values, user)} />
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

function MembersCard({ members, roles }: { members: BotMember[]; roles: BotRole[] }) {
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
                <TableCell>{member.verified ? <Check className="h-4 w-4 text-success" /> : <X className="h-4 w-4 text-muted-foreground" />}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </CardContent>
    </Card>
  );
}

function MemberRegistrationCard({
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
              <Input
                type="file"
                accept=".csv,text/csv"
                className="sr-only"
                onChange={(event) => {
                  const file = event.target.files?.[0];
                  if (file) void onUpload(file);
                }}
              />
            </FieldLabel>
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}

function BotRolesCard({
  roles,
  onSave,
  onDelete,
}: {
  roles: BotRole[];
  onSave: (values: AdminRecord, current?: BotRole) => Promise<void>;
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
                <TableCell>
                  <div className="flex gap-1">
                    {role.managed && <Badge variant="outline">Managed</Badge>}
                    {role.mentionable && <Badge variant="outline">Mentionable</Badge>}
                    {role.hoisted && <Badge variant="outline">Hoisted</Badge>}
                  </div>
                </TableCell>
                <TableCell>
                  <div className="flex gap-1">
                    <RecordDialog title="Edit role" fields={roleFields} current={role as unknown as AdminRecord} onSave={(values) => onSave(values, role)} />
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

function BotChannelsCard({
  channels,
  onSave,
  onDelete,
}: {
  channels: BotChannel[];
  onSave: (values: AdminRecord, current?: BotChannel) => Promise<void>;
  onDelete: (channel: BotChannel) => Promise<void>;
}) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>Channels</CardTitle>
        <CardDescription>{channels.length} channels synced</CardDescription>
        <CardAction>
          <RecordDialog title="New channel" fields={channelFields} onSave={(values) => onSave(values)} />
        </CardAction>
      </CardHeader>
      <CardContent>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Name</TableHead>
              <TableHead>Discord ID</TableHead>
              <TableHead>Type</TableHead>
              <TableHead>Position</TableHead>
              <TableHead className="w-24">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {channels.map((channel) => (
              <TableRow key={channel.channel_id}>
                <TableCell>{channel.name}</TableCell>
                <TableCell>{channel.discord_channel_id}</TableCell>
                <TableCell>{channel.type}</TableCell>
                <TableCell>{channel.position}</TableCell>
                <TableCell>
                  <div className="flex gap-1">
                    <RecordDialog title="Edit channel" fields={channelFields} current={channel as unknown as AdminRecord} onSave={(values) => onSave(values, channel)} />
                    <Button variant="destructive" size="icon-sm" onClick={() => void onDelete(channel)}>
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

function GuildSettingsCard({
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
        <Field>
          <FieldLabel>Verification description</FieldLabel>
          <Textarea value={values.verification.description} onChange={(event) => setValues((previous) => ({ ...previous, verification: { ...previous.verification, description: event.target.value } }))} />
        </Field>
        <FieldGroup className="grid gap-4 md:grid-cols-2">
          <SelectField
            label="Verification role"
            value={values.verification.verified_role_id ?? ""}
            items={roles.filter((role) => role.discord_role_id).map((role) => ({ value: role.discord_role_id ?? "", label: role.name }))}
            onChange={(verified_role_id) => setValues((previous) => ({ ...previous, verification: { ...previous.verification, verified_role_id } }))}
          />
          <SelectField
            label="Verification channel"
            value={values.verification.channel_id ?? ""}
            items={channels.map((channel) => ({ value: channel.discord_channel_id, label: channel.name }))}
            onChange={(channel_id) => setValues((previous) => ({ ...previous, verification: { ...previous.verification, channel_id } }))}
          />
          <TextInput label="Welcome title" value={values.welcome.title} onChange={(title) => setValues((previous) => ({ ...previous, welcome: { ...previous.welcome, title } }))} />
          <SelectField
            label="Welcome channel"
            value={values.welcome.channel_id ?? ""}
            items={channels.map((channel) => ({ value: channel.discord_channel_id, label: channel.name }))}
            onChange={(channel_id) => setValues((previous) => ({ ...previous, welcome: { ...previous.welcome, channel_id } }))}
          />
        </FieldGroup>
        <Field>
          <FieldLabel>Welcome description</FieldLabel>
          <Textarea value={values.welcome.description} onChange={(event) => setValues((previous) => ({ ...previous, welcome: { ...previous.welcome, description: event.target.value } }))} />
        </Field>
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

function SelectField({
  label,
  value,
  items,
  onChange,
}: {
  label: string;
  value: string;
  items: { value: string; label: string }[];
  onChange: (value: string) => void;
}) {
  return (
    <Field>
      <FieldLabel>{label}</FieldLabel>
      <Select value={value} onValueChange={onChange}>
        <SelectTrigger className="w-full">
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          {items.map((item) => (
            <SelectItem key={item.value} value={item.value}>
              {item.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </Field>
  );
}

function seedValues(fields: FieldConfig[], current?: AdminRecord): AdminRecord {
  return Object.fromEntries(fields.map((field) => [field.key, current?.[field.key] ?? defaultForField(field)]));
}

function defaultForField(field: FieldConfig): RecordValue {
  if (field.type === "checkbox") return false;
  if (field.type === "number") return null;
  return "";
}

function normalizeRecord(values: AdminRecord, fields: FieldConfig[]): AdminRecord {
  return Object.fromEntries(
    fields.map((field) => {
      const value = values[field.key];
      if (field.type === "number") {
        return [field.key, numberOrNull(value)];
      }
      if (field.type === "checkbox") {
        return [field.key, Boolean(value)];
      }
      return [field.key, value === "" ? null : value];
    })
  );
}

function numberOrNull(value: RecordValue): number | null {
  if (value === null || value === undefined || value === "") return null;
  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : null;
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

function parseMembersCsv(text: string): RegisterMemberRequest[] {
  const rows = text
    .split(/\r?\n/)
    .map((row) => row.trim())
    .filter(Boolean)
    .map(parseCsvLine);

  const [headers = [], ...values] = rows;
  return values.map((row) => {
    const record = Object.fromEntries(headers.map((header, index) => [header, row[index] ?? ""]));
    return {
      email: record.email ?? "",
      fullname: record.fullname ?? record.full_name ?? "",
      username: record.username ?? "",
      discord_user_id: record.discord_user_id ?? record.discordUserId ?? "-",
      fun_fact: record.fun_fact ?? record.funfact ?? "",
      discord_role_ids: (record.discord_role_ids ?? record.roles ?? "")
        .split(/[|;]/)
        .map((role) => role.trim())
        .filter(Boolean),
    };
  });
}

function parseCsvLine(line: string): string[] {
  const values: string[] = [];
  let current = "";
  let quoted = false;

  for (let index = 0; index < line.length; index += 1) {
    const char = line[index];
    const next = line[index + 1];

    if (char === '"' && next === '"') {
      current += '"';
      index += 1;
    } else if (char === '"') {
      quoted = !quoted;
    } else if (char === "," && !quoted) {
      values.push(current.trim());
      current = "";
    } else {
      current += char;
    }
  }

  values.push(current.trim());
  return values;
}
