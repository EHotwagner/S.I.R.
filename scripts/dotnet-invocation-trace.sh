#!/usr/bin/env bash
set -euo pipefail

: "${SIR_REAL_DOTNET:?dotnet-invocation-trace: SIR_REAL_DOTNET is required}"
: "${SIR_DOTNET_INVOCATION_LOG:?dotnet-invocation-trace: SIR_DOTNET_INVOCATION_LOG is required}"

if [[ "${1:-}" == "fable" ]]; then
  project=""
  for argument in "$@"; do
    if [[ "$argument" == *.fsproj ]]; then
      project=$argument
      break
    fi
  done
  [[ -n "$project" ]] || { echo "dotnet-invocation-trace: fable invocation has no project" >&2; exit 2; }
  printf 'fable\t%s\n' "$project" >> "$SIR_DOTNET_INVOCATION_LOG"
fi

exec "$SIR_REAL_DOTNET" "$@"
