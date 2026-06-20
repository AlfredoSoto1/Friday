#!/usr/bin/env bash
set -euo pipefail

workspace="/workspaces/friday"

sudo mkdir -p \
  "$workspace/backend/bin" \
  "$workspace/webapp/node_modules" \
  "$workspace/webapp/.next" \
  "$workspace/scripts"

sudo chown -R vscode:vscode \
  "$workspace/backend/bin" \
  "$workspace/webapp/node_modules" \
  "$workspace/webapp/.next" \
  "$workspace/scripts"

sudo chmod -R u+rwX,g+rwX \
  "$workspace/backend/bin" \
  "$workspace/webapp/node_modules" \
  "$workspace/webapp/.next" \
  "$workspace/scripts"

if [ -f "$workspace/scripts/reset-db.sh" ]; then
  sudo chown vscode:vscode "$workspace/scripts/reset-db.sh"
  sudo chmod ug+x "$workspace/scripts/reset-db.sh"
fi
