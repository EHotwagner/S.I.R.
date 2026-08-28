#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
handbook="$repo_root/docs/sir-combat-quint-handbook.md"
diagram_ids=(attack-pipeline invariant-boundary q4-arithmetic rule-dependency state-action trace-counterexample)
tooltip_hotspots=0

fail() {
  echo "sir-combat-quint-tooltips: $*" >&2
  exit 1
}

for id in "${diagram_ids[@]}"; do
  canonical="$repo_root/docs/assets/sir-combat-quint/$id.svg"
  interactive="$repo_root/docs/assets/sir-combat-quint-interactive/$id.svg"
  test -f "$canonical" || fail "missing reviewed diagram: $id"
  test -f "$interactive" || fail "missing interactive edition: $id"
  tooltip_count="$(awk '{ count += gsub(/data-tooltip=/, "") } END { print count + 0 }' "$interactive")"
  title_count="$(awk '{ count += gsub(/<title([ >])/, "") } END { print count + 0 }' "$interactive")"
  (( tooltip_count >= 8 )) || fail "tooltip density regressed: $id ($tooltip_count; expected at least 8)"
  (( title_count >= tooltip_count + 1 )) || fail "tooltip titles are incomplete: $id"
  tooltip_hotspots=$((tooltip_hotspots + tooltip_count))
  grep -F "data-diagram-embed=\"$id\"" "$handbook" >/dev/null || fail "handbook embed missing: $id"
  grep -F "<object type=\"image/svg+xml\" data=\"assets/sir-combat-quint-interactive/$id.svg\"" "$handbook" >/dev/null \
    || fail "interactive object missing: $id"
  grep -F "<img src=\"assets/sir-combat-quint-interactive/$id.svg\"" "$handbook" >/dev/null \
    || fail "repository-rendered image fallback missing: $id"
  grep -F "id=\"diagram-transcript-$id\"" "$handbook" >/dev/null || fail "transcript missing: $id"
done

(( tooltip_hotspots >= 48 )) || fail "aggregate tooltip density regressed: $tooltip_hotspots"

node --input-type=module - "$repo_root" "${diagram_ids[@]}" <<'NODE'
import fs from "node:fs";
import path from "node:path";

const [root, ...ids] = process.argv.slice(2);
const normalize = value => value
  .replace(/\sdata-tooltip="[^"]+"/g, "")
  .replace(/<title>(?:.|\n)*?<\/title>/g, "")
  .replaceAll("></path>", "/>")
  .replaceAll("></polyline>", "/>")
  .replaceAll("></circle>", "/>");

for (const id of ids) {
  const canonical = fs.readFileSync(path.join(root, "docs/assets/sir-combat-quint", `${id}.svg`), "utf8");
  const interactive = fs.readFileSync(path.join(root, "docs/assets/sir-combat-quint-interactive", `${id}.svg`), "utf8");
  if (normalize(interactive) !== canonical) {
    console.error(`sir-combat-quint-tooltips: interactive edition changes reviewed visual bytes after metadata normalization: ${id}`);
    process.exit(1);
  }
}
NODE

echo "sir-combat-quint-tooltips: PASS (6 provenance-preserving interactive SVG editions; $tooltip_hotspots detailed tooltip hotspots)"
