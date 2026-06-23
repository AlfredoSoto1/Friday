"use client";

import { useEffect, useMemo, useState } from "react";
import { Bot, RefreshCcw } from "lucide-react";

import type { EditableRecord } from "@/components/custom/record-fields";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  createBotUser,
  createGuildRole,
  createInelicom,
  deleteBotUser,
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
  updateGuildProfile,
  updateGuildRole,
  updateInelicom,
} from "@/features/dashboard/admin-api";
import type { AdminRecord, BotChannel, BotMember, BotRole, BotUser, DashboardDataEntity, GuildProfile, GuildSummary, RegisterMemberRequest } from "@/features/dashboard/admin-types";
import { BotChannelsCard } from "@/features/dashboard/bot-channels-card";
import { BotRolesCard } from "@/features/dashboard/bot-roles-card";
import { BotUsersCard } from "@/features/dashboard/bot-users-card";
import { inelicomResources, roleFields } from "@/features/dashboard/dashboard-config";
import { normalizeAdminRecord, parseMembersCsv } from "@/features/dashboard/dashboard-utils";
import { GuildSettingsCard } from "@/features/dashboard/guild-settings-card";
import { InelicomCard } from "@/features/dashboard/inelicom-card";
import { MemberRegistrationCard } from "@/features/dashboard/member-registration-card";
import { MembersCard } from "@/features/dashboard/members-card";
import { OverviewPanel } from "@/features/dashboard/overview-panel";
import { cn } from "@/lib/utils";

export function DashboardAdmin({ initialData }: { initialData: DashboardDataEntity }) {
  const [dashboard, setDashboard] = useState(initialData);
  const [guilds, setGuilds] = useState<GuildSummary[]>([]);
  const [guildId, setGuildId] = useState<number | null>(null);
  const [profile, setProfile] = useState<GuildProfile | null>(null);
  const [users, setUsers] = useState<BotUser[]>([]);
  const [members, setMembers] = useState<BotMember[]>([]);
  const [roles, setRoles] = useState<BotRole[]>([]);
  const [channels, setChannels] = useState<BotChannel[]>([]);
  const [resourceKey, setResourceKey] = useState(inelicomResources[0].key);
  const [records, setRecords] = useState<AdminRecord[]>([]);
  const [message, setMessage] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const resource = useMemo(() => inelicomResources.find((item) => item.key === resourceKey) ?? inelicomResources[0], [resourceKey]);

  useEffect(() => { void refreshShell(); }, []);
  useEffect(() => { void loadInelicom(resource.key); }, [resource.key]);
  useEffect(() => { if (guildId) void loadGuild(guildId); }, [guildId]);

  async function refreshShell() {
    setLoading(true);
    try {
      const [nextDashboard, nextGuilds, nextUsers] = await Promise.all([getDashboardData(), getGuilds(), getBotUsers()]);
      setDashboard(nextDashboard); setGuilds(nextGuilds); setUsers(nextUsers);
      setGuildId((current) => current ?? nextGuilds[0]?.guild_id ?? null); setMessage("Data refreshed.");
    } catch (error) { setMessage(error instanceof Error ? error.message : "Could not refresh data."); }
    finally { setLoading(false); }
  }

  async function loadInelicom(key: string) { try { setRecords(await listInelicom(key)); } catch (error) { setMessage(error instanceof Error ? error.message : "Could not load records."); } }
  async function loadGuild(id: number) {
    try {
      const [nextProfile, nextMembers, nextRoles, nextChannels] = await Promise.all([getGuildProfile(id), getGuildMembers(id), getGuildRoles(id), getGuildChannels(id)]);
      setProfile(nextProfile); setMembers(nextMembers); setRoles(nextRoles); setChannels(nextChannels);
    } catch (error) { setMessage(error instanceof Error ? error.message : "Could not load guild data."); }
  }
  async function saveInelicom(values: EditableRecord, current?: EditableRecord) {
    const payload = normalizeAdminRecord(values, resource.fields);
    current ? await updateInelicom(resource.key, Number(current[resource.idKey]), payload) : await createInelicom(resource.key, payload);
    await loadInelicom(resource.key); setMessage(`${resource.label} saved.`);
  }
  async function saveUser(values: EditableRecord, current?: BotUser) {
    const payload = { email: String(values.email ?? ""), fullname: String(values.fullname ?? ""), username: String(values.username ?? "") };
    current ? await updateBotUser(current.user_id, payload) : await createBotUser(payload);
    setUsers(await getBotUsers()); setMessage("User saved.");
  }
  async function saveRole(values: EditableRecord, current?: BotRole) {
    if (!guildId) return;
    const payload = normalizeAdminRecord(values, roleFields);
    current ? await updateGuildRole(guildId, current.role_id, payload) : await createGuildRole(guildId, payload);
    setRoles(await getGuildRoles(guildId)); setMessage("Role saved.");
  }
  async function uploadMembers(file: File) { if (guildId) { const parsed = parseMembersCsv(await file.text()); await registerGuildMembers(guildId, parsed); setMembers(await getGuildMembers(guildId)); setMessage(`${parsed.length} members registered.`); } }

  return (
    <main className="min-h-screen bg-background">
      <header className="border-b border-border bg-background/80"><div className="mx-auto flex max-w-7xl items-center justify-between px-6 py-4"><div className="flex items-center gap-3"><div className="flex h-9 w-9 items-center justify-center rounded-md bg-discord text-white"><Bot className="h-5 w-5" /></div><div><h1 className="text-lg font-semibold tracking-normal">Friday</h1><p className="text-sm text-muted-foreground">Incoming student operations</p></div></div><div className="flex items-center gap-2">{message && <Badge variant="outline">{message}</Badge>}<Button variant="outline" size="sm" onClick={refreshShell} disabled={loading}><RefreshCcw className={cn("h-4 w-4", loading && "animate-spin")} />Refresh</Button></div></div></header>
      <section className="mx-auto max-w-7xl px-6 py-8"><Tabs defaultValue="overview" className="gap-6"><TabsList><TabsTrigger value="overview">Overview</TabsTrigger><TabsTrigger value="inelicom">Inelicom</TabsTrigger><TabsTrigger value="bot">Bot</TabsTrigger></TabsList><TabsContent value="overview"><OverviewPanel data={dashboard} /></TabsContent><TabsContent value="inelicom"><InelicomCard resource={resource} records={records} onResourceChange={setResourceKey} onSave={saveInelicom} onDelete={async (row) => { await deleteInelicom(resource.key, Number(row[resource.idKey])); await loadInelicom(resource.key); setMessage(`${resource.label} deleted.`); }} /></TabsContent><TabsContent value="bot" className="space-y-4"><GuildSelector guilds={guilds} guildId={guildId} onChange={setGuildId} /><div className="grid gap-4 xl:grid-cols-2">{profile && guildId && <GuildSettingsCard profile={profile} roles={roles} channels={channels} onSave={async (next) => { const saved = await updateGuildProfile(guildId, next); setProfile(saved); setMessage("Server settings saved."); }} />}<MemberRegistrationCard roles={roles} onRegister={async (values: RegisterMemberRequest) => { if (guildId) { await registerGuildMember(guildId, values); setMembers(await getGuildMembers(guildId)); setMessage("Member registered."); } }} onUpload={uploadMembers} /></div><div className="grid gap-4 xl:grid-cols-2"><BotUsersCard users={users} onSave={saveUser} onDelete={async (user) => { await deleteBotUser(user.user_id); setUsers(await getBotUsers()); setMessage("User deleted."); }} /><MembersCard members={members} roles={roles} /></div><div className="grid gap-4 xl:grid-cols-2"><BotRolesCard roles={roles} onSave={saveRole} onDelete={async (role) => { if (guildId) { await deleteGuildRole(guildId, role.role_id); setRoles(await getGuildRoles(guildId)); setMessage("Role deleted."); } }} /><BotChannelsCard channels={channels} /></div></TabsContent></Tabs></section>
    </main>
  );
}

function GuildSelector({ guilds, guildId, onChange }: { guilds: GuildSummary[]; guildId: number | null; onChange: (id: number) => void }) {
  return <Select value={guildId?.toString() ?? ""} onValueChange={(value) => onChange(Number(value))}><SelectTrigger className="w-64"><SelectValue /></SelectTrigger><SelectContent>{guilds.map((guild) => <SelectItem key={guild.guild_id} value={guild.guild_id.toString()}>{guild.name}</SelectItem>)}</SelectContent></Select>;
}
