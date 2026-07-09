import Link from "next/link";
import { ArrowLeft, ClipboardList } from "lucide-react";

import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { ItemContent, ItemMedia, ItemTitle } from "@/components/ui/item";
import { AttendanceManager } from "@/features/attendance/attendance-manager";

export default function AttendancePage(): React.ReactElement {
  return (
    <Card className="min-h-screen w-full min-w-0 overflow-x-hidden rounded-none bg-transparent py-0 ring-0">
      <CardHeader className="border-b border-border bg-background/80 px-0 py-4">
        <div className="mx-auto flex w-full max-w-7xl items-center justify-between gap-4 px-4 sm:px-6">
          <div className="flex min-w-0 items-center gap-4">
            <ItemMedia
              variant="icon"
              className="size-9 shrink-0 rounded-md bg-discord text-white"
            >
              <ClipboardList className="size-5" />
            </ItemMedia>
            <ItemContent className="min-w-0">
              <ItemTitle className="text-lg">Friday</ItemTitle>
              <CardDescription>Attendance</CardDescription>
            </ItemContent>
          </div>
          <Button asChild variant="outline" size="sm">
            <Link href="/dashboard">
              <ArrowLeft />
              Dashboard
            </Link>
          </Button>
        </div>
      </CardHeader>
      <CardContent className="mx-auto grid w-full min-w-0 max-w-7xl gap-6 px-4 py-8 sm:px-6">
        <Card className="rounded-md border-border bg-card shadow-panel">
          <CardHeader>
            <CardTitle className="text-3xl">Attendance</CardTitle>
            <CardDescription>
              Compare a master student CSV with an attendance subset and export missing or present students.
            </CardDescription>
          </CardHeader>
        </Card>
        <AttendanceManager />
      </CardContent>
    </Card>
  );
}
