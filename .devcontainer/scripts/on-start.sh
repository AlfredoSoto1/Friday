#!/usr/bin/env bash
set -euo pipefail

workspace="/workspaces/friday"

sudo mkdir -p \
  "$workspace/backend/bin" \
  "$workspace/webapp/node_modules" \
  "$workspace/webapp/.next"

sudo chown -R vscode:vscode \
  "$workspace/backend/bin" \
  "$workspace/webapp/node_modules" \
  "$workspace/webapp/.next"

sudo chmod -R u+rwX,g+rwX \
  "$workspace/backend/bin" \
  "$workspace/webapp/node_modules" \
  "$workspace/webapp/.next"
