#!/usr/bin/env bash
set -euo pipefail

if [[ "${1:-}" != "build" || "${2:-}" != "-t" || -z "${3:-}" ]]; then
  echo "Usage: ./fake.sh build -t Dev|Test|Verify" >&2
  exit 2
fi

case "$3" in
  Dev)
    dotnet build SIR.slnx --no-restore
    ;;
  Test)
    npm test
    ;;
  Verify)
    npm run build:docs
    node scripts/smoke-client.mjs
    ;;
  *)
    echo "Unknown target: $3" >&2
    exit 2
    ;;
esac
