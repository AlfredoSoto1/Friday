import { Database } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import type { SchemaSummaryEntity } from "@/server/entities/dashboard";

interface SchemaCatalogProps {
  schemas: SchemaSummaryEntity[];
}

export function SchemaCatalog({ schemas }: SchemaCatalogProps) {
  return (
    <Card className="rounded-md border-border bg-card shadow-panel">
      <CardHeader className="border-b border-border">
        <div className="flex items-center gap-2">
          <Database className="h-4 w-4 text-muted-foreground" />
          <CardTitle className="text-sm">Database schemas</CardTitle>
        </div>
      </CardHeader>
      <CardContent className="p-0">
        {schemas.length === 0 ? (
          <div className="px-4 py-8 text-sm text-muted-foreground">
            No schema data available.
          </div>
        ) : (
          <div>
            {schemas.map((schema, index) => (
              <div key={schema.name}>
                {index > 0 ? <Separator /> : null}
                <div className="px-4 py-4">
                  <div className="mb-3 flex items-center justify-between gap-3">
                    <h3 className="font-medium">{schema.name}</h3>
                    <Badge
                      variant="secondary"
                      className="rounded-md bg-muted text-muted-foreground"
                    >
                      {schema.tables.length} tables
                    </Badge>
                  </div>
                  <div className="grid gap-2 sm:grid-cols-2 xl:grid-cols-3">
                    {schema.tables.map((table) => (
                      <div
                        key={`${schema.name}.${table.table}`}
                        className="rounded-md border border-border bg-background px-3 py-2"
                      >
                        <div className="text-sm font-medium">{table.table}</div>
                        <div className="text-xs text-muted-foreground">
                          {table.rows.toLocaleString("en-US")} rows
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
