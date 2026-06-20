#!/usr/bin/env bash
set -euo pipefail

sudo apt-get update
sudo apt-get install -y --no-install-recommends postgresql-client
sudo rm -rf /var/lib/apt/lists/*

bash .devcontainer/scripts/on-start.sh

dotnet restore backend
pip install -r bot/requirements.txt -r slm/requirements.txt
npm install --prefix webapp
