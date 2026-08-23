#!/usr/bin/env bash
# Rebind the rules-corpus source correspondence baseline.
#
# This is the mutable half of the rules source pin (S.I.R.#264). When a pull request makes a
# reviewed change to one of the declared implementation sources, run this to record the source's
# new normalized digest in tests/fixtures/rules-corpus/v2/source-correspondence.json, in the SAME
# commit as the source change.
#
# It deliberately CANNOT:
#   * rebind a path whose normalized text has not actually changed -- an accidental sweep produces
#     an empty diff rather than nineteen fresh blessings;
#   * add or remove a path -- the frozen set is owned by implementation-sources.json .sources, and
#     scripts/verify-rules-corpus.sh independently refuses any divergence;
#   * touch sourceCommit, the sealed implementation digest, or any generated corpus fixture --
#     those belong to the immutable half of the pin and are not this tool's business.
#
# Usage:
#   scripts/rebind-rules-corpus-sources.sh              # report what would be rebound, write nothing
#   scripts/rebind-rules-corpus-sources.sh --write      # record the drifted digests
#   scripts/rebind-rules-corpus-sources.sh --write src/SIR.Client.Web/App.fs [...]
#                                                       # restrict the rebind to named paths
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
source_manifest="$repo_root/tests/fixtures/rules-corpus/v2/implementation-sources.json"
correspondence_manifest="$repo_root/tests/fixtures/rules-corpus/v2/source-correspondence.json"

mode=report
requested=()
for argument in "$@"; do
  case "$argument" in
    --write) mode=write ;;
    --report) mode=report ;;
    -h|--help) sed -n '2,26p' "${BASH_SOURCE[0]}" | sed 's/^# \{0,1\}//'; exit 0 ;;
    -*) echo "unknown option: $argument" >&2; exit 2 ;;
    *) requested+=("$argument") ;;
  esac
done

test -f "$source_manifest" || { echo "missing implementation source manifest: $source_manifest" >&2; exit 1; }
test -f "$correspondence_manifest" || { echo "missing source correspondence: $correspondence_manifest" >&2; exit 1; }

# Same normalization as scripts/verify-rules-corpus.sh. Kept identical on purpose: a divergence
# here would record digests the gate cannot reproduce.
normalize_implementation_source() {
  local artifact_path=$1
  local input_path=$2
  if test "$artifact_path" = "src/SIR.Simulation/CombatRules.fs"; then
    sed -E \
      -e 's/(Commit = ")[0-9a-f]{40}(" })/\1<SOURCE_COMMIT>\2/' \
      -e 's/(GetBytes ")[0-9a-f]{64}(" \])/\1<IMPLEMENTATION_DIGEST>\2/' \
      -e 's/(FS.GG.Game.Core@0\.13\.0" ")[0-9a-f]{40}(" implementationArtifacts)/\1<SOURCE_COMMIT>\2/' \
      "$input_path"
  else
    command cat "$input_path"
  fi
}

normalized_source_digest() {
  normalize_implementation_source "$1" "$2" | sha256sum | cut -d' ' -f1
}

declared_sources=$(jq -r '.sources[]' "$source_manifest")

# A requested path that is not a declared implementation source is a mistake, not a request to
# widen the frozen set. Refuse rather than silently ignoring it.
for requested_path in "${requested[@]-}"; do
  test -n "$requested_path" || continue
  printf '%s\n' "$declared_sources" | grep -Fxq -- "$requested_path" || {
    echo "not a declared implementation source: $requested_path" >&2
    echo "  the frozen set is owned by tests/fixtures/rules-corpus/v2/implementation-sources.json .sources" >&2
    exit 1
  }
done

drifted=()
missing=()
while IFS= read -r artifact_path; do
  if test ${#requested[@]} -gt 0; then
    printf '%s\n' "${requested[@]}" | grep -Fxq -- "$artifact_path" || continue
  fi
  if ! test -f "$repo_root/$artifact_path"; then
    missing+=("$artifact_path")
    continue
  fi
  recorded=$(jq -r --arg path "$artifact_path" '.paths[$path] // empty' "$correspondence_manifest")
  current=$(normalized_source_digest "$artifact_path" "$repo_root/$artifact_path")
  if test "$recorded" != "$current"; then
    drifted+=("$artifact_path")
    printf 'rebind %s\n  recorded %s\n  current  %s\n' "$artifact_path" "${recorded:-<none>}" "$current" >&2
  fi
done <<< "$declared_sources"

if test ${#missing[@]} -gt 0; then
  echo "declared implementation sources are missing from the tree:" >&2
  printf '  %s\n' "${missing[@]}" >&2
  echo "  refusing to rebind a partial tree; restore the sources or amend .sources first" >&2
  exit 1
fi

if test ${#drifted[@]} -eq 0; then
  echo "source correspondence is already current: nothing to rebind"
  exit 0
fi

if test "$mode" != write; then
  printf '%d implementation source(s) would be rebound; re-run with --write to record them\n' "${#drifted[@]}"
  exit 0
fi

# Refuse to record a digest for text whose behaviour the corpus rejects.
#
# A correspondence entry asserts that the sealed corpus identity still describes this source.
# Building is not enough to support that claim: a change to an algorithm body compiles cleanly,
# moves no declared rule value, and would be blessed here while the rules it implements no longer
# behave as the corpus says (S.I.R.#264 round 1). So execute the corpus before recording anything.
# This is defence in depth, not the guard: scripts/verify-rules-corpus.sh runs the same conformance
# route, so a hand-edited correspondence file that never went through this tool is still refused.
echo "building and executing the rules corpus before recording new identity digests..." >&2
dotnet build "$repo_root/tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj" -c Release >/dev/null 2>&1 || {
  echo "refusing to rebind: the corpus test project does not build" >&2
  echo "  a correspondence digest recorded for text nobody compiled is an untestable assertion" >&2
  exit 1
}
conformance_log=$(mktemp /tmp/sir-rules-rebind-conformance.XXXXXX)
if ! dotnet run --project "$repo_root/tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj" -c Release --no-build >"$conformance_log" 2>&1; then
  echo "refusing to rebind: registered executable behaviour does not satisfy the rules corpus fixtures" >&2
  echo "  rebinding here would record a digest asserting the corpus still describes this source, which it does not" >&2
  grep -iE 'exception|failwith|did not' "$conformance_log" | head -5 >&2
  rm -f "$conformance_log"
  exit 1
fi
rm -f "$conformance_log"

updated=$(mktemp /tmp/sir-rules-rebind.XXXXXX)
cp "$correspondence_manifest" "$updated"
for artifact_path in "${drifted[@]}"; do
  digest=$(normalized_source_digest "$artifact_path" "$repo_root/$artifact_path")
  jq --arg path "$artifact_path" --arg digest "$digest" '.paths[$path] = $digest' "$updated" > "$updated.next"
  mv "$updated.next" "$updated"
done
mv "$updated" "$correspondence_manifest"

printf 'rebound %d implementation source correspondence entr%s\n' \
  "${#drifted[@]}" "$(test ${#drifted[@]} -eq 1 && printf 'y' || printf 'ies')"
printf '  %s\n' "${drifted[@]}"
echo "review the source-correspondence.json diff: it names exactly the identity subjects that moved"
