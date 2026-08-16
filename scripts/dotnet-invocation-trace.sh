#!/usr/bin/env bash
set -euo pipefail

: "${SIR_REAL_DOTNET:?dotnet-invocation-trace: SIR_REAL_DOTNET is required}"
: "${SIR_DOTNET_INVOCATION_LOG:?dotnet-invocation-trace: SIR_DOTNET_INVOCATION_LOG is required}"

verb=${1:-}
kind=""
project=""
identity="-"
isolated=false
normalize_project() {
  local value=$1
  if [[ -n "${SIR_DOTNET_TRACE_ROOT:-}" && "$value" == "$SIR_DOTNET_TRACE_ROOT/"* ]]; then
    value=${value#"$SIR_DOTNET_TRACE_ROOT/"}
  fi
  printf '%s' "$value"
}
if [[ "$verb" == "fable" ]]; then
  for argument in "$@"; do
    if [[ "$argument" == *.fsproj ]]; then
      project=$argument
      break
    fi
  done
  [[ -n "$project" ]] || { echo "dotnet-invocation-trace: fable invocation has no project" >&2; exit 2; }
  kind=fable
elif [[ "$verb" == "build" || "$verb" == "publish" ]]; then
  project=${2:-missing-project}
  kind=$verb
elif [[ "$verb" == "run" ]]; then
  no_build=false
  previous=""
  for argument in "$@"; do
    if [[ "$previous" == "--project" ]]; then project=$argument; fi
    if [[ "$argument" == "--no-build" ]]; then no_build=true; fi
    if [[ "$argument" == "--artifacts-path" ]]; then isolated=true; fi
    previous=$argument
  done
  if [[ "$no_build" == false ]]; then
    [[ -n "$project" ]] || { echo "dotnet-invocation-trace: building run invocation has no project" >&2; exit 2; }
    kind=run-build
  fi
fi

if [[ -z "$kind" ]]; then exec "$SIR_REAL_DOTNET" "$@"; fi

project=$(normalize_project "$project")
if [[ -n "${SIR_BUILD_EXCEPTION:-}" ]]; then
  identity="exception:${SIR_BUILD_EXCEPTION}"
  if [[ "$isolated" == true ]]; then identity="$identity:artifacts-path:isolated"; fi
fi
started=$(date +%s%3N)
set +e
"$SIR_REAL_DOTNET" "$@"
status=$?
set -e
completed=$(date +%s%3N)
printf '%s\t%s\t%s\t%s\t%s\n' "$kind" "$project" "$identity" "$started" "$completed" >> "$SIR_DOTNET_INVOCATION_LOG"
exit "$status"
