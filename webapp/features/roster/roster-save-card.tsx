import { Check, Save } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Spinner } from "@/components/ui/spinner";

interface RosterSaveCardProps {
  assignedCount: number;
  studentCount: number;
  teamCount: number;
  saving: boolean;
  saved: boolean;
  disabled: boolean;
  onSave: () => void;
}

export function RosterSaveCard({
  assignedCount,
  studentCount,
  teamCount,
  saving,
  saved,
  disabled,
  onSave,
}: RosterSaveCardProps): React.ReactElement {
  return (
    <Card className="min-w-0 rounded-md border-border bg-card shadow-panel">
      <CardContent className="flex flex-wrap items-center justify-between gap-4">
        <p className="text-sm text-muted-foreground">
          {teamCount
            ? `${assignedCount}/${studentCount} students assigned across ${teamCount} teams.`
            : "Upload a list and generate teams before saving."}
        </p>
        <Button onClick={onSave} disabled={disabled || saving}>
          {saving ? <Spinner /> : saved ? <Check /> : <Save />}
          {saved ? "Saved" : "Save team distribution"}
        </Button>
      </CardContent>
    </Card>
  );
}
