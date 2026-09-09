#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
task_root=$(mktemp -d)
trap 'rm -rf -- "$task_root"' EXIT
model=readiness/184-scenario-catalog/work-model.json

snapshot() {
  local root=$1
  while IFS= read -r -d '' relative; do
    printf '%s  %s\n' "$(sha256sum "$root/$relative" | cut -d' ' -f1)" "$relative"
  done < <(git -C "$repo_root" ls-files -z readiness/184-scenario-catalog | sort -z)
}

historical_before=$(snapshot "$repo_root")
historical_model=$(sha256sum "$repo_root/$model" | cut -d' ' -f1)
# Historical accepted output belongs to its producing SDD version. Exercise the current
# installed generator in a disposable workspace, including the original source/evidence
# paths, rather than requiring a tool upgrade to reproduce an older generator's bytes.
mkdir -p "$task_root/workspace"
tar -C "$repo_root" --exclude=.git --exclude=node_modules --exclude=artifacts \
  --exclude=bin --exclude=obj -cf - . | tar -C "$task_root/workspace" -xf -
cd "$task_root/workspace"
git init --quiet
git add --all
git -c user.name='SDD compatibility fixture' -c user.email='fixture@example.invalid' \
  -c commit.gpgsign=false commit --quiet -m 'Snapshot receiver fixture'

run_stage() {
  local stage=$1
  if ! dotnet fsgg-sdd "$stage" --work 184-scenario-catalog --root . --json > "$task_root/$stage.json"; then
    jq '{outcome, diagnostics}' "$task_root/$stage.json" >&2
    return 1
  fi
  jq -e --arg version "$(jq -r '.tools["fs.gg.sdd.cli"].version' .config/dotnet-tools.json)" \
    '.toolVersion == $version and (.outcome == "succeeded" or .outcome == "succeededWithWarnings" or .outcome == "noChange")' \
    "$task_root/$stage.json" >/dev/null
}

# Establish a baseline produced by the currently pinned tool. The first pass reports
# stale input while regenerating it; the second must settle without that diagnostic.
# FS.GG.SDD#857's pre-evidence analyze projection remains bounded: current verify/ship
# must restore their own stable final projection, not an older release's bytes.
run_stage analyze
run_stage analyze
run_stage verify
run_stage verify
run_stage ship
run_stage ship
current_model=$(sha256sum "$model" | cut -d' ' -f1)
current_views=$(snapshot "$task_root/workspace")
run_stage verify
run_stage ship
[[ "$(snapshot "$task_root/workspace")" == "$current_views" ]] || {
  echo "current SDD verify -> ship did not preserve its own generated baseline" >&2
  diff -u <(printf '%s\n' "$current_views") <(snapshot "$task_root/workspace") >&2 || true
  exit 1
}
run_stage analyze
run_stage analyze
run_stage verify
run_stage verify
run_stage ship
run_stage ship
[[ "$(sha256sum "$model" | cut -d' ' -f1)" == "$current_model" ]] || {
  echo "current SDD analyze -> verify -> ship did not restore its own work-model baseline" >&2
  exit 1
}
[[ "$(snapshot "$task_root/workspace")" == "$current_views" ]] || {
  echo "current SDD analyze -> verify -> ship changed generated readiness views" >&2
  exit 1
}
[[ "$(snapshot "$repo_root")" == "$historical_before" ]] || {
  echo "SDD compatibility verification modified historical receiver evidence" >&2
  exit 1
}
printf 'SDD current-version stability passed; historical model %s preserved, current model %s verified.\n' \
  "$historical_model" "$current_model"
