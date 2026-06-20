export interface BackendStatusEntity {
  status?: string;
  database?: string;
}

export interface TableSummaryEntity {
  schema: string;
  table: string;
  rows: number;
}

export interface SchemaSummaryEntity {
  name: string;
  tables: TableSummaryEntity[];
}

export interface CatalogSummaryEntity {
  schemas: SchemaSummaryEntity[];
}

export interface DashboardDataEntity {
  status: BackendStatusEntity | null;
  catalog: CatalogSummaryEntity | null;
}
