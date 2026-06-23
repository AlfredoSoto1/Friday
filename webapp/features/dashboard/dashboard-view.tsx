import { DashboardAdmin } from "@/features/dashboard/dashboard-admin";
import type { DashboardDataEntity } from "@/server/entities/dashboard";

interface DashboardViewProps {
  data: DashboardDataEntity;
}

export function DashboardView({ data }: DashboardViewProps) {
  return <DashboardAdmin initialData={data} />;
}
