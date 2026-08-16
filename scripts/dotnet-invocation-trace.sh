#!/usr/bin/env bash
set -euo pipefail

: "${SIR_REAL_DOTNET:?dotnet-invocation-trace: SIR_REAL_DOTNET is required}"
: "${SIR_DOTNET_INVOCATION_LOG:?dotnet-invocation-trace: SIR_DOTNET_INVOCATION_LOG is required}"

verb=${1:-}
if [[ "$verb" == "fable" ]]; then
  project=""
  for argument in "$@"; do
    if [[ "$argument" == *.fsproj ]]; then
      project=$argument
      break
    fi
  done
  [[ -n "$project" ]] || { echo "dotnet-invocation-trace: fable invocation has no project" >&2; exit 2; }
  printf 'fable\t%s\n' "$project" >> "$SIR_DOTNET_INVOCATION_LOG"
elif [[ "$verb" == "build" || "$verb" == "publish" ]]; then
  project=${2:-missing-project}
  printf '%s\t%s\n' "$verb" "$project" >> "$SIR_DOTNET_INVOCATION_LOG"
elif [[ "$verb" == "run" ]]; then
  project=""
  no_build=false
  previous=""
  for argument in "$@"; do
    if [[ "$previous" == "--project" ]]; then project=$argument; fi
    if [[ "$argument" == "--no-build" ]]; then no_build=true; fi
    previous=$argument
  done
  if [[ "$no_build" == false ]]; then
    [[ -n "$project" ]] || { echo "dotnet-invocation-trace: building run invocation has no project" >&2; exit 2; }
    printf 'run-build\t%s\n' "$project" >> "$SIR_DOTNET_INVOCATION_LOG"
  fi
fi

exec "$SIR_REAL_DOTNET" "$@"
