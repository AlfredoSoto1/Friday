import Link from "next/link";
import { ArrowLeft, Database } from "lucide-react";

import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { ItemContent, ItemMedia, ItemTitle } from "@/components/ui/item";
import { CsvImportManager } from "@/features/imports/csv-import-manager";

export default function ImportsPage(): React.ReactElement {
  return (
    <Card className="min-h-screen w-full min-w-0 overflow-x-hidden rounded-none bg-transparent py-0 ring-0">
      <CardHeader className="border-b border-border bg-background/80 px-0 py-4">
        <div className="mx-auto flex w-full max-w-7xl items-center justify-between gap-4 px-4 sm:px-6">
          <div className="flex min-w-0 items-center gap-4">
            <ItemMedia
              variant="icon"
              className="size-9 shrink-0 rounded-md bg-discord text-white"
            >
              <Database className="size-5" />
            </ItemMedia>
            <ItemContent className="min-w-0">
              <ItemTitle className="text-lg">Friday</ItemTitle>
              <CardDescription>Dataset imports</CardDescription>
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
            <CardTitle className="text-3xl">Import dataset</CardTitle>
            <CardDescription>
              Upload content CSVs for Google pins/rooms, projects, organizations, and faculty.
            </CardDescription>
          </CardHeader>
        </Card>
        <CsvImportManager />
      </CardContent>
    </Card>
  );
}
