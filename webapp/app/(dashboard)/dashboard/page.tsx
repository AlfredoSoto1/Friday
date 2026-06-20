import { DashboardView } from "@/features/dashboard/dashboard-view";
import { getDashboardData } from "@/server/webservices/dashboard-webservice";

export default async function DashboardPage() {
  const data = await getDashboardData();

  return <DashboardView data={data} />;
}
