#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
subject="$repo_root/tests/fixtures/rules-corpus/v2/coverage.json"
temporary=$(mktemp -d /tmp/sir-ci-full-mutation.XXXXXX)
original="$temporary/coverage.json"
route="$temporary/route.json"
cp -p "$subject" "$original"

restore() {
  cp -p "$original" "$subject"
}
cleanup() {
  restore
  rm -rf -- "$temporary"
}
trap cleanup EXIT

cd "$repo_root"
node scripts/ci-route.mjs route --path docs/index.md --commit "$(git rev-parse HEAD)" --tree "$(git rev-parse 'HEAD^{tree}')" --output "$route" >/dev/null
jq -e '.classification == "documentation" and (.selectedGates | index("rules") | not)' "$route" >/dev/null || {
  echo "ci-full-mutation: focused documentation route unexpectedly selected rules" >&2
  exit 1
}
jq '.edges[0].to = "missing:scheduled-full-subject"' "$subject" > "$temporary/mutant.json"
cp "$temporary/mutant.json" "$subject"
if ./scripts/generate-rules-corpus.sh --check >"$temporary/full.log" 2>&1; then
  echo "ci-full-mutation: full rules surface accepted the hidden cross-surface defect" >&2
  exit 1
fi
restore
./scripts/generate-rules-corpus.sh --check >/dev/null
cmp -s "$original" "$subject" || { echo "ci-full-mutation: subject restoration failed" >&2; exit 1; }
echo "Scheduled full-route mutation proved a docs-focused skip and a full-surface rules rejection; fixture restored."
