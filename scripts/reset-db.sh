#!/usr/bin/env bash
set -euo pipefail

workspace="${WORKSPACE_FOLDER:-/workspaces/friday}"
DB_HOST="${POSTGRES_HOST:-db}"
DB_PORT="${POSTGRES_PORT:-5432}"
DB_NAME="${POSTGRES_DB:-friday}"
DB_USER="${POSTGRES_USER:-friday}"
SCHEMA_FILE="${SCHEMA_FILE:-$workspace/database/schema.sql}"

if [ -z "${POSTGRES_PASSWORD:-}" ]; then
  echo "POSTGRES_PASSWORD must be set." >&2
  exit 1
fi

if [ ! -f "$SCHEMA_FILE" ]; then
  echo "Schema file not found: $SCHEMA_FILE" >&2
  exit 1
fi

export PGPASSWORD="$POSTGRES_PASSWORD"

echo "Waiting for PostgreSQL at ${DB_HOST}:${DB_PORT}/${DB_NAME}..."
until pg_isready -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" >/dev/null 2>&1; do
  sleep 1
done

echo "Resetting database ${DB_NAME} on ${DB_HOST}:${DB_PORT}..."
psql \
  -h "$DB_HOST" \
  -p "$DB_PORT" \
  -U "$DB_USER" \
  -d "$DB_NAME" \
  -v ON_ERROR_STOP=1 \
  -f "$SCHEMA_FILE"

echo "Done."
