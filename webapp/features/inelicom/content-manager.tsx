"use client";

import { useEffect, useState } from "react";
import { TriangleAlert } from "lucide-react";

import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Skeleton } from "@/components/ui/skeleton";
import { ContentTableCard } from "@/features/inelicom/content-table-card";
import type {
  ContentData,
  ContentFormValues,
  ContentKind,
  ContentRecord,
} from "@/features/inelicom/content-types";
import { InelicomApi } from "@/server/webservices/inelicom-webservice";

const emptyData: ContentData = {
  faculties: [],
  buildings: [],
};

export function ContentManager(): React.ReactElement {
  const [data, setData] = useState<ContentData>(emptyData);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect((): void => {
    void loadContent();
  }, []);

  async function loadContent(): Promise<void> {
    setLoading(true);
    const results = await Promise.all([
      InelicomApi.getFaculties(),
      InelicomApi.getBuildings(),
    ]);
    const failed = results.find((result) => result.isFailure);

    if (failed?.isFailure) {
      setError(failed.error.message);
      setLoading(false);
      return;
    }

    setData({
      faculties: results[0].value.items,
      buildings: results[1].value.items,
    });
    setError("");
    setLoading(false);
  }

  async function saveRecord(
    kind: ContentKind,
    values: ContentFormValues,
    current?: ContentRecord
  ): Promise<boolean> {
    const name = values.name.trim();
    let failedMessage = "";

    if (kind === "faculty") {
      const result = current && "facultyId" in current
        ? await InelicomApi.updateFaculty(current.facultyId, { name })
        : await InelicomApi.createFaculty({ name });
      failedMessage = result.isFailure ? result.error.message : "";
    } else if (kind === "building") {
      const request = { name, gpin: values.gpin.trim() };
      const result = current && "buildingId" in current
        ? await InelicomApi.updateBuilding(current.buildingId, request)
        : await InelicomApi.createBuilding(request);
      failedMessage = result.isFailure ? result.error.message : "";
    }

    if (failedMessage) {
      setError(failedMessage);
      return false;
    }

    await loadContent();
    return true;
  }

  async function deleteRecord(
    kind: ContentKind,
    current: ContentRecord
  ): Promise<void> {
    let failedMessage = "";

    if (kind === "faculty" && "facultyId" in current) {
      const result = await InelicomApi.deleteFaculty(current.facultyId);
      failedMessage = result.isFailure ? result.error.message : "";
    } else if (kind === "building" && "buildingId" in current) {
      const result = await InelicomApi.deleteBuilding(current.buildingId);
      failedMessage = result.isFailure ? result.error.message : "";
    }

    if (failedMessage) {
      setError(failedMessage);
      return;
    }

  }

  if (loading) {
    return (
      <div className="grid gap-6 md:grid-cols-2">
        <Skeleton className="h-64" />
        <Skeleton className="h-64" />
        <Skeleton className="h-64" />
      </div>
    );
  }

  return (
    <div className="grid min-w-0 gap-6">
      {error ? (
        <Alert variant="destructive">
          <TriangleAlert />
          <AlertTitle>Catalog action failed</AlertTitle>
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      ) : null}
      <div className="grid min-w-0 gap-6 xl:grid-cols-2">
        <ContentTableCard
          kind="faculty"
          title="Faculties"
          description={`${data.faculties.length} faculty records`}
          records={data.faculties}
          data={data}
          onSave={saveRecord}
          onDelete={deleteRecord}
        />
        <ContentTableCard
          kind="building"
          title="Buildings"
          description={`${data.buildings.length} building records`}
          records={data.buildings}
          data={data}
          onSave={saveRecord}
          onDelete={deleteRecord}
        />
      </div>
    </div>
  );
}
