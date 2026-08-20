#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
dev_public="$repo_root/src/SIR.Client.Web/.dev-public"

mkdir -p "$dev_public/content/sir-client/v1"
cd "$repo_root"
node scripts/generate-in-app-docs.mjs "$dev_public"

export SIR_CLIENT_DEV_PUBLIC_DIR="$dev_public"
exec vite --config src/SIR.Client.Web/vite.config.js "$@"
