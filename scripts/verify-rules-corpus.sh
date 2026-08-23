#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
project="$repo_root/tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj"

search_quiet() {
  local pattern=$1
  local path=$2
  if test "${SIR_RULES_FORCE_GREP:-0}" != 1 && command -v rg >/dev/null 2>&1; then
    rg -q -- "$pattern" "$path"
  else
    grep -Eq -- "$pattern" "$path"
  fi
}

source_manifest="$repo_root/tests/fixtures/rules-corpus/v2/implementation-sources.json"
correspondence_manifest="$repo_root/tests/fixtures/rules-corpus/v2/source-correspondence.json"
source_commit=$(jq -r '.sourceCommit' "$source_manifest")

# The rules source pin has two duties that a single commit cannot discharge together:
#
#   P1 source-link durability. `sourceCommit` is the commit published rule source links resolve
#      against, so a fresh network clone must be able to reach it. That REQUIRES it to be an
#      ancestor of the canonical default branch -- see require_durable_source_commit below.
#   P2 identity correspondence. The implementation sources must still hold the text the corpus
#      identity was sealed over. Rebinding that baseline REQUIRES naming text that is not yet on
#      the default branch, because a pull request's own content never is.
#
# P1 demands ancestry; P2 forbids it. Binding both to `sourceCommit` made P2 unsatisfiable: no
# pull request changing a pinned source could pass, because advancing the pin needs a commit that
# only exists after the merge the pin gates (S.I.R.#264). The duties are therefore split across two
# artifacts. `implementation-sources.json` keeps P1 and the sealed identity digest and does not
# change on rebind; `source-correspondence.json` carries P2 and is rebound in the same pull request
# that changes a source, via scripts/rebind-rules-corpus-sources.sh.
#
# Enforcement is NOT narrowed by this split: correspondence is still required for every one of the
# declared implementation sources, byte-exactly, after the same normalization as before.
declared_source_schema=$(jq -r '.schema' "$source_manifest")
test "$declared_source_schema" = "sir-rules-implementation-sources-v1" || {
  echo "unsupported implementation source manifest schema: $declared_source_schema" >&2
  exit 1
}
declared_correspondence_schema=$(jq -r '.schema' "$correspondence_manifest")
test "$declared_correspondence_schema" = "sir-rules-source-correspondence-v1" || {
  echo "unsupported source correspondence schema: $declared_correspondence_schema" >&2
  exit 1
}

require_durable_source_commit() {
  local git_repo=$1
  local commit=$2
  local canonical_ref=$3

  if [[ ! "$commit" =~ ^[0-9a-f]{40}$ ]]; then
    echo "declared rules source commit is not a 40-character lowercase Git object id: $commit" >&2
    return 1
  fi
  if ! git -C "$git_repo" cat-file -e "$commit^{commit}" 2>/dev/null; then
    echo "declared rules source commit is unavailable: $commit (fetch canonical history or rebind the corpus to a durable commit)" >&2
    return 1
  fi
  if ! git -C "$git_repo" show-ref --verify --quiet "$canonical_ref"; then
    echo "canonical remote default branch is unavailable: $canonical_ref (fetch the canonical remote before verifying the rules corpus)" >&2
    return 1
  fi
  if ! git -C "$git_repo" merge-base --is-ancestor "$commit" "$canonical_ref"; then
    echo "declared rules source commit is not durably reachable from $canonical_ref: $commit (local-only and deleted-branch objects are not reproducible in a fresh network clone)" >&2
    return 1
  fi
}

canonical_source_ref=refs/remotes/origin/main
require_durable_source_commit "$repo_root" "$source_commit" "$canonical_source_ref"

"$repo_root/scripts/generate-rules-corpus.sh" --check

for fixture in manifest.json coverage.json representative-application.hex; do
  fixture_mutant=$(mktemp -d /tmp/sir-rules-fixture-mutant.XXXXXX)
  cp "$repo_root/tests/fixtures/rules-corpus/v2/"* "$fixture_mutant/"
  printf '\n ' >> "$fixture_mutant/$fixture"
  if SIR_RULES_FIXTURE_DIR="$fixture_mutant" "$repo_root/scripts/generate-rules-corpus.sh" --check >/dev/null 2>&1; then
    echo "rules-corpus fixture mutation unexpectedly passed: $fixture" >&2
    rm -rf "$fixture_mutant"
    exit 1
  fi
  rm -rf "$fixture_mutant"
done

coverage_mutant=$(mktemp /tmp/sir-rules-coverage-mutant.XXXXXX)
jq '.edges[0].to = "missing:node"' "$repo_root/tests/fixtures/rules-corpus/v2/coverage.json" > "$coverage_mutant"
if "$repo_root/scripts/validate-rules-coverage.sh" "$coverage_mutant" >/dev/null 2>&1; then
  echo "rules coverage dangling-endpoint mutation unexpectedly passed" >&2
  rm -f "$coverage_mutant"
  exit 1
fi
rm -f "$coverage_mutant"

test "$(sha256sum "$repo_root/tests/fixtures/rules-corpus/v1/manifest.json" | cut -d' ' -f1)" = "e5bfe82d40e72ff8b41898e408c50dd0d8fb7e05b72c6acc24baab0e3b451ddc" || { echo "retained v1 manifest changed" >&2; exit 1; }
test "$(sha256sum "$repo_root/tests/fixtures/rules-corpus/v1/coverage.json" | cut -d' ' -f1)" = "39eecda1018c504eab7b03c60228bf155c99aa42433724655da42d9ee470d554" || { echo "retained v1 coverage changed" >&2; exit 1; }
test "$(sha256sum "$repo_root/tests/fixtures/rules-corpus/v1/representative-application.hex" | cut -d' ' -f1)" = "f42835c3fc4691b59ff71c0b31de0e74caa21455bf9d5e7658b483e0b2da2606" || { echo "retained v1 application changed" >&2; exit 1; }

while IFS=$'\t' read -r source_path source_symbol; do
  test -f "$repo_root/$source_path" || { echo "missing rule source: $source_path" >&2; exit 1; }
  symbol_name=${source_symbol##*.}
  search_quiet "let (private )?${symbol_name}( |$)" "$repo_root/$source_path" || {
    echo "unresolved rule source symbol: $source_symbol in $source_path" >&2
    exit 1
  }
done <<< "$(jq -r '.rules[].source | select(. != null) | [.path, .symbol] | @tsv' "$repo_root/tests/fixtures/rules-corpus/v2/manifest.json")"

manifest_source_commit=$(jq -r '.sourceCommit' "$repo_root/tests/fixtures/rules-corpus/v2/manifest.json")
test "$manifest_source_commit" = "$source_commit" || { echo "implementation source manifest does not bind the package source commit" >&2; exit 1; }

reachability_mutant=$(mktemp -d /tmp/sir-rules-reachability-mutant.XXXXXX)
reachability_log=$(mktemp /tmp/sir-rules-reachability.XXXXXX)
git -C "$reachability_mutant" init -q
empty_tree=$(git -C "$reachability_mutant" hash-object -t tree /dev/null)
durable_mutant_commit=$(printf 'durable rules source\n' | env GIT_AUTHOR_NAME=Rules GIT_AUTHOR_EMAIL=rules@example.invalid GIT_COMMITTER_NAME=Rules GIT_COMMITTER_EMAIL=rules@example.invalid git -C "$reachability_mutant" commit-tree "$empty_tree")
local_only_mutant_commit=$(printf 'local-only rules source\n' | env GIT_AUTHOR_NAME=Rules GIT_AUTHOR_EMAIL=rules@example.invalid GIT_COMMITTER_NAME=Rules GIT_COMMITTER_EMAIL=rules@example.invalid git -C "$reachability_mutant" commit-tree "$empty_tree")
git -C "$reachability_mutant" update-ref refs/remotes/origin/main "$durable_mutant_commit"
if require_durable_source_commit "$reachability_mutant" "$durable_mutant_commit" refs/remotes/origin/missing >"$reachability_log" 2>&1; then
  echo "missing canonical source ref mutation unexpectedly passed" >&2
  rm -rf "$reachability_mutant"
  rm -f "$reachability_log"
  exit 1
fi
search_quiet 'canonical remote default branch is unavailable: refs/remotes/origin/missing.*fetch the canonical remote' "$reachability_log" || {
  echo "missing canonical source ref mutation failed without the actionable fetch diagnostic" >&2
  rm -rf "$reachability_mutant"
  rm -f "$reachability_log"
  exit 1
}
if require_durable_source_commit "$reachability_mutant" "$local_only_mutant_commit" refs/remotes/origin/main >"$reachability_log" 2>&1; then
  echo "local-only rules source commit mutation unexpectedly passed" >&2
  rm -rf "$reachability_mutant"
  rm -f "$reachability_log"
  exit 1
fi
search_quiet 'not durably reachable from refs/remotes/origin/main.*local-only and deleted-branch objects' "$reachability_log" || {
  echo "local-only rules source commit mutation failed without the actionable durability diagnostic" >&2
  rm -rf "$reachability_mutant"
  rm -f "$reachability_log"
  exit 1
}
if require_durable_source_commit "$reachability_mutant" 0000000000000000000000000000000000000000 refs/remotes/origin/main >"$reachability_log" 2>&1; then
  echo "missing rules source commit mutation unexpectedly passed" >&2
  rm -rf "$reachability_mutant"
  rm -f "$reachability_log"
  exit 1
fi
search_quiet 'declared rules source commit is unavailable.*fetch canonical history or rebind' "$reachability_log" || {
  echo "missing rules source commit mutation failed without the actionable availability diagnostic" >&2
  rm -rf "$reachability_mutant"
  rm -f "$reachability_log"
  exit 1
}
if require_durable_source_commit "$reachability_mutant" not-a-commit refs/remotes/origin/main >"$reachability_log" 2>&1; then
  echo "malformed rules source commit mutation unexpectedly passed" >&2
  rm -rf "$reachability_mutant"
  rm -f "$reachability_log"
  exit 1
fi
search_quiet 'not a 40-character lowercase Git object id' "$reachability_log" || {
  echo "malformed rules source commit mutation failed without the actionable format diagnostic" >&2
  rm -rf "$reachability_mutant"
  rm -f "$reachability_log"
  exit 1
}
rm -rf "$reachability_mutant"
rm -f "$reachability_log"

source_digest_input=$(mktemp /tmp/sir-rules-source-digest.XXXXXX)
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
  local artifact_path=$1
  local input_path=$2
  normalize_implementation_source "$artifact_path" "$input_path" | sha256sum | cut -d' ' -f1
}

source_matches_correspondence() {
  local artifact_path=$1
  local current_path=$2
  local correspondence_json=${3:-$correspondence_manifest}
  local expected
  local actual
  expected=$(jq -r --arg path "$artifact_path" '.paths[$path] // empty' "$correspondence_json")
  test -n "$expected" || return 1
  actual=$(normalized_source_digest "$artifact_path" "$current_path")
  test "$actual" = "$expected"
}

# The recorded baseline must name EXACTLY the declared implementation identity set. Without this,
# the cheapest way to unfreeze a source would be to delete its row -- the gate would then check
# eighteen files and report success, which is the vacuity failure this mechanism must not have.
check_correspondence_coverage() {
  local sources_json=$1
  local correspondence_json=$2
  local identity
  local recorded
  local malformed
  identity=$(jq -r '.sources[]' "$sources_json" | sort -u)
  recorded=$(jq -r '.paths | keys[]' "$correspondence_json" | sort -u)
  if test -z "$recorded"; then
    echo "recorded source correspondence is empty: every implementation source would go unchecked" >&2
    return 1
  fi
  if test "$identity" != "$recorded"; then
    echo "recorded source correspondence does not cover the implementation identity set exactly" >&2
    comm -23 <(printf '%s\n' "$identity") <(printf '%s\n' "$recorded") | sed 's/^/  declared implementation source with no recorded correspondence: /' >&2
    comm -13 <(printf '%s\n' "$identity") <(printf '%s\n' "$recorded") | sed 's/^/  recorded correspondence for a path that is not a declared implementation source: /' >&2
    return 1
  fi
  # Digest well-formedness is the ONLY arm here whose empty result means "pass"; for the two arms
  # above an empty result still reaches a `return 1`, so they already fail closed. That asymmetry is
  # why this arm -- and only this arm -- has to prove it actually evaluated its input.
  #
  # Two distinct failures are guarded, because fixing either alone leaves the other live:
  #
  #   1. `test/1` RAISES on any non-string (number, null, boolean, array, object) rather than
  #      returning false. Typing the predicate keeps `test/1` unreachable for a non-string, so a
  #      non-string is CLASSIFIED as malformed instead of aborting the filter.
  #   2. This function is only ever called on the LEFT of `||`, which suspends `set -e` for its whole
  #      body. So ANY jq failure -- the raise above, unreadable JSON, a jq crash -- would otherwise
  #      leave `malformed` empty and fall through to a confident `return 0` on input that was never
  #      evaluated. Checking jq's exit status makes an unevaluated input a refusal, which is what
  #      keeps this closed against a failure mode not enumerated above.
  local malformed_status=0
  malformed=$(jq -r '
    .paths
    | to_entries[]
    | select(if (.value | type) == "string"
             then (.value | test("^[0-9a-f]{64}$") | not)
             else true
             end)
    | "\(.key)\t\(.value | type)"' "$correspondence_json") || malformed_status=$?
  if test "$malformed_status" -ne 0; then
    echo "recorded source correspondence could not be evaluated for digest well-formedness" >&2
    echo "  jq exited $malformed_status over: $correspondence_json" >&2
    echo "  refusing rather than reporting a pass on input this check did not evaluate" >&2
    return 1
  fi
  if test -n "$malformed"; then
    echo "recorded source correspondence carries malformed digests:" >&2
    printf '%s\n' "$malformed" | sed 's/^/  /' >&2
    return 1
  fi
}

enforce_source_correspondence() {
  local sources_json=$1
  local correspondence_json=$2
  local tree_root=$3
  local artifact_path
  while IFS= read -r artifact_path; do
    test -f "$tree_root/$artifact_path" || {
      echo "declared implementation source is missing from the tree: $artifact_path" >&2
      return 1
    }
    source_matches_correspondence "$artifact_path" "$tree_root/$artifact_path" "$correspondence_json" || {
      echo "current implementation source differs from package pin: $artifact_path" >&2
      return 1
    }
  done <<< "$(jq -r '.sources[]' "$sources_json")"
}

# The sealed identity digest is derived ONLY from blobs at $source_commit. The working tree
# contributes nothing to it, which is why rebinding correspondence leaves the seal, the manifest
# identity, and the generated corpus fixtures byte-identical.
while IFS= read -r artifact_path; do
  actual_artifact_sha=$(git -C "$repo_root" show "$source_commit:$artifact_path" | sha256sum | cut -d' ' -f1)
  printf '%s\t%s\n' "$artifact_path" "$actual_artifact_sha" >> "$source_digest_input"
done <<< "$(jq -r '.sources[]' "$source_manifest")"

check_correspondence_coverage "$source_manifest" "$correspondence_manifest" || { rm -f "$source_digest_input"; exit 1; }
enforce_source_correspondence "$source_manifest" "$correspondence_manifest" "$repo_root" || { rm -f "$source_digest_input"; exit 1; }

# ---------------------------------------------------------------------------------------------
# Source-correspondence inversions (S.I.R.#264).
#
# These drive the production enforcement path -- enforce_source_correspondence and
# check_correspondence_coverage, the same functions the gate calls above -- against synthetic trees
# and synthetic correspondence documents, so a change that makes the real gate vacuous fails here
# rather than passing quietly.
#
# Five prove refusals fire. The sixth proves a LEGAL input still exists, which is the class the
# durability hardening lacked: work item #239 shipped four inversions for its new refusals across
# d76b477 and d1f6ea7, all four still pass, and every one proves a bad input is refused. None
# demonstrates that the operation the new precondition constrains -- rebinding the pin so a changed
# source can pass -- has any legal execution at all. That gap is what made this gate unsatisfiable
# from d76b477 (2026-08-20) until it was first exercised.
# ---------------------------------------------------------------------------------------------
pin_probe_dir=$(mktemp -d /tmp/sir-rules-pin-probe.XXXXXX)
pin_probe_log=$(mktemp /tmp/sir-rules-pin-probe-log.XXXXXX)

pin_probe_fail() {
  echo "$1" >&2
  rm -rf "$pin_probe_dir"
  rm -f "$pin_probe_log" "$source_digest_input"
  exit 1
}

pin_probe_tree() {
  local tree_root=$1
  local artifact_path
  while IFS= read -r artifact_path; do
    mkdir -p "$tree_root/$(dirname "$artifact_path")"
    cp "$repo_root/$artifact_path" "$tree_root/$artifact_path"
  done <<< "$(jq -r '.sources[]' "$source_manifest")"
}

# 1. A changed non-rule-hosting identity subject is refused. This is the property an independent
#    critic required on item #194 after proving that hashing pinned objects alone let a changed
#    App.fs pass; narrowing enforcement would reopen it.
app_probe_tree="$pin_probe_dir/app-mutant"
mkdir -p "$app_probe_tree"
pin_probe_tree "$app_probe_tree"
printf '\n// implementation identity subject mutation\n' >> "$app_probe_tree/src/SIR.Client.Web/App.fs"
if enforce_source_correspondence "$source_manifest" "$correspondence_manifest" "$app_probe_tree" >"$pin_probe_log" 2>&1; then
  pin_probe_fail "App.fs implementation source mutation unexpectedly passed"
fi
search_quiet 'current implementation source differs from package pin: src/SIR.Client.Web/App.fs' "$pin_probe_log" || {
  pin_probe_fail "App.fs implementation source mutation failed without the actionable pin diagnostic"
}

# 2. A non-metadata change to the rule-hosting source is refused.
combat_probe_tree="$pin_probe_dir/combat-mutant"
mkdir -p "$combat_probe_tree"
pin_probe_tree "$combat_probe_tree"
sed '0,/module CombatRules =/s//module CombatRules = \/\/ implementation identity subject mutation/' \
  "$repo_root/src/SIR.Simulation/CombatRules.fs" > "$combat_probe_tree/src/SIR.Simulation/CombatRules.fs"
if enforce_source_correspondence "$source_manifest" "$correspondence_manifest" "$combat_probe_tree" >"$pin_probe_log" 2>&1; then
  pin_probe_fail "CombatRules.fs non-metadata source mutation unexpectedly passed"
fi
search_quiet 'current implementation source differs from package pin: src/SIR.Simulation/CombatRules.fs' "$pin_probe_log" || {
  pin_probe_fail "CombatRules.fs non-metadata source mutation failed without the actionable pin diagnostic"
}

# 3. A metadata-only identity rebind is still normalized away, so re-sealing the corpus does not
#    read as a source change.
metadata_probe_tree="$pin_probe_dir/combat-metadata"
mkdir -p "$metadata_probe_tree"
pin_probe_tree "$metadata_probe_tree"
sed -E 's/(Commit = ")[0-9a-f]{40}(" })/\10000000000000000000000000000000000000000\2/' \
  "$repo_root/src/SIR.Simulation/CombatRules.fs" > "$metadata_probe_tree/src/SIR.Simulation/CombatRules.fs"
enforce_source_correspondence "$source_manifest" "$correspondence_manifest" "$metadata_probe_tree" >"$pin_probe_log" 2>&1 || {
  pin_probe_fail "CombatRules.fs metadata-only source rebind was not normalized"
}

# 4. Coverage guard: a source cannot be unfrozen by deleting its recorded row, and a path that is
#    not a declared implementation source cannot be smuggled in.
dropped_correspondence="$pin_probe_dir/dropped-correspondence.json"
jq 'del(.paths["src/SIR.Client.Web/App.fs"])' "$correspondence_manifest" > "$dropped_correspondence"
if check_correspondence_coverage "$source_manifest" "$dropped_correspondence" >"$pin_probe_log" 2>&1; then
  pin_probe_fail "dropping a recorded source correspondence unexpectedly passed"
fi
search_quiet 'declared implementation source with no recorded correspondence: src/SIR.Client.Web/App.fs' "$pin_probe_log" || {
  pin_probe_fail "dropped source correspondence failed without the actionable coverage diagnostic"
}

extra_correspondence="$pin_probe_dir/extra-correspondence.json"
jq '.paths["src/SIR.Domain/RuleTypes.fs"] = "0000000000000000000000000000000000000000000000000000000000000000"' \
  "$correspondence_manifest" > "$extra_correspondence"
if check_correspondence_coverage "$source_manifest" "$extra_correspondence" >"$pin_probe_log" 2>&1; then
  pin_probe_fail "undeclared source correspondence entry unexpectedly passed"
fi
search_quiet 'recorded correspondence for a path that is not a declared implementation source: src/SIR.Domain/RuleTypes.fs' "$pin_probe_log" || {
  pin_probe_fail "undeclared source correspondence entry failed without the actionable coverage diagnostic"
}

# 5. An emptied baseline and a malformed digest are both refused.
emptied_correspondence="$pin_probe_dir/emptied-correspondence.json"
jq '.paths = {}' "$correspondence_manifest" > "$emptied_correspondence"
if check_correspondence_coverage "$source_manifest" "$emptied_correspondence" >"$pin_probe_log" 2>&1; then
  pin_probe_fail "emptied source correspondence unexpectedly passed"
fi
search_quiet 'recorded source correspondence is empty' "$pin_probe_log" || {
  pin_probe_fail "emptied source correspondence failed without the actionable vacuity diagnostic"
}

# A malformed digest is refused for EVERY JSON type a digest can be, not merely for a string that
# does not look like a digest. `test/1` raises on any non-string, and this function is called on the
# left of `||`, so before S.I.R.#264's repair phase a non-string digest aborted the filter and the
# arm returned 0 -- a confident pass on input it had not evaluated. Only the string case below was
# ever exercised, which is why that survived four rounds of review.
#
# The six cases enumerated here are the COMPLETE set of JSON value types, so this inversion cannot
# be defeated by "a further value" the way an enumeration of observed literals could be.
malformed_correspondence="$pin_probe_dir/malformed-correspondence.json"
while IFS='|' read -r probe_label probe_value probe_type; do
  test -n "$probe_label" || continue
  jq --argjson injected "$probe_value" \
     '.paths["src/SIR.Domain/Rules.fs"] = $injected' \
     "$correspondence_manifest" > "$malformed_correspondence"

  # Guard the probe itself: assert the fixture really carries the type under test, so a probe that
  # silently stopped injecting could not pass by testing nothing.
  actual_type=$(jq -r '.paths["src/SIR.Domain/Rules.fs"] | type' "$malformed_correspondence")
  test "$actual_type" = "$probe_type" || {
    pin_probe_fail "malformed-digest probe '$probe_label' injected $actual_type, expected $probe_type"
  }

  if check_correspondence_coverage "$source_manifest" "$malformed_correspondence" >"$pin_probe_log" 2>&1; then
    pin_probe_fail "malformed source correspondence digest ($probe_label) unexpectedly passed"
  fi
  search_quiet 'recorded source correspondence carries malformed digests' "$pin_probe_log" || {
    pin_probe_fail "malformed source correspondence digest ($probe_label) failed without the actionable format diagnostic"
  }
  search_quiet "src/SIR.Domain/Rules.fs.*$probe_type" "$pin_probe_log" || {
    pin_probe_fail "malformed source correspondence digest ($probe_label) did not name the offending path and its type"
  }
done <<'MALFORMED_DIGEST_DOMAIN'
non-digest string|"not-a-sha256"|string
number|12345|number
null|null|null
boolean|true|boolean
array|["deadbeef"]|array
object|{"a":1}|object
MALFORMED_DIGEST_DOMAIN

# Unparseable correspondence is refused -- and this probe records WHICH check refuses it, because
# "the property is provided, but by a different check than the one named for it" is precisely the
# defect S.I.R.#264's repair phase exists to remove. Naming the wrong arm here would reproduce it.
#
# The refusal comes from the EMPTINESS arm above, not from the evaluability guard: `.paths | keys[]`
# fails first on unparseable input, leaving `recorded` empty. Every file-level jq failure is caught
# there before the digest arm is ever reached. The evaluability guard is therefore defence in depth
# against a FUTURE edit reintroducing a raising filter -- unreachable through this function's own
# interface today, and deliberately not claimed as the arm under test here.
unreadable_correspondence="$pin_probe_dir/unreadable-correspondence.json"
printf '{"paths": {"src/SIR.Domain/Rules.fs": ' > "$unreadable_correspondence"
if check_correspondence_coverage "$source_manifest" "$unreadable_correspondence" >"$pin_probe_log" 2>&1; then
  pin_probe_fail "unreadable source correspondence unexpectedly passed"
fi
search_quiet 'recorded source correspondence is empty' "$pin_probe_log" || {
  pin_probe_fail "unreadable source correspondence failed, but not through the emptiness arm this probe names"
}

# 6. A LEGITIMATE rebind succeeds. A genuinely changed implementation source, with its
#    correspondence rebound in the same commit, must pass -- otherwise no pull request touching a
#    pinned file could ever satisfy this gate, which is the defect S.I.R.#264 was filed for.
rebind_probe_tree="$pin_probe_dir/legitimate-rebind"
mkdir -p "$rebind_probe_tree"
pin_probe_tree "$rebind_probe_tree"
printf '\n// reviewed change to an implementation identity subject\n' >> "$rebind_probe_tree/src/SIR.Simulation/Simulation.fs"
if source_matches_correspondence "src/SIR.Simulation/Simulation.fs" "$rebind_probe_tree/src/SIR.Simulation/Simulation.fs" "$correspondence_manifest"; then
  pin_probe_fail "legitimate rebind probe did not actually change the implementation source"
fi
rebound_correspondence="$pin_probe_dir/rebound-correspondence.json"
jq --arg path "src/SIR.Simulation/Simulation.fs" \
   --arg digest "$(normalized_source_digest "src/SIR.Simulation/Simulation.fs" "$rebind_probe_tree/src/SIR.Simulation/Simulation.fs")" \
   '.paths[$path] = $digest' "$correspondence_manifest" > "$rebound_correspondence"
check_correspondence_coverage "$source_manifest" "$rebound_correspondence" >"$pin_probe_log" 2>&1 || {
  pin_probe_fail "a rebound source correspondence was refused by the coverage guard"
}
enforce_source_correspondence "$source_manifest" "$rebound_correspondence" "$rebind_probe_tree" >"$pin_probe_log" 2>&1 || {
  pin_probe_fail "a legitimate rebound implementation source change was refused: no pull request could satisfy this gate"
}

# 7. The rebind writer cannot widen the frozen set. The declared identity set is owned by
#    implementation-sources.json, and the writer must refuse a path outside it rather than adding
#    one -- otherwise the tool that maintains the baseline could also redefine what it covers.
if "$repo_root/scripts/rebind-rules-corpus-sources.sh" --write src/SIR.Domain/RuleTypes.fs >"$pin_probe_log" 2>&1; then
  pin_probe_fail "rebind writer accepted a path outside the declared implementation identity set"
fi
search_quiet 'not a declared implementation source: src/SIR.Domain/RuleTypes.fs' "$pin_probe_log" || {
  pin_probe_fail "rebind writer refused an undeclared path without the actionable ownership diagnostic"
}

# 8. The writer's normalization agrees with this verifier's. The writer necessarily carries its own
#    copy of normalize_implementation_source; if the two ever diverge it would record digests this
#    gate cannot reproduce, and every rebind would land broken. On a tree this gate considers
#    current, the writer must therefore find nothing to rebind.
"$repo_root/scripts/rebind-rules-corpus-sources.sh" >"$pin_probe_log" 2>&1 || {
  pin_probe_fail "rebind writer failed on a tree this gate considers current"
}
search_quiet 'already current: nothing to rebind' "$pin_probe_log" || {
  pin_probe_fail "rebind writer reports drift on a tree this gate considers current: the writer and verifier normalizations have diverged"
}

rm -rf "$pin_probe_dir"
rm -f "$pin_probe_log"
printf 'package\t%s\nalgorithm\t%s\n' "$(jq -r '.packageSha256' "$source_manifest")" "$(jq -r '.algorithmFingerprint' "$source_manifest")" >> "$source_digest_input"
actual_sources_digest=$(sha256sum "$source_digest_input" | cut -d' ' -f1)
identity_mutant=$(mktemp /tmp/sir-rules-source-digest-mutant.XXXXXX)
sed 's#^src/SIR.Domain/Rules.fs\t[0-9a-f]\{64\}$#src/SIR.Domain/Rules.fs\t0000000000000000000000000000000000000000000000000000000000000000#' \
  "$source_digest_input" > "$identity_mutant"
mutated_sources_digest=$(sha256sum "$identity_mutant" | cut -d' ' -f1)
rm -f "$identity_mutant"
rm -f "$source_digest_input"
declared_sources_digest=$(sed -n 's/.*"implementation", System.Text.Encoding.UTF8.GetBytes "\([0-9a-f]\{64\}\)".*/\1/p' "$repo_root/src/SIR.Simulation/CombatRules.fs")
test "$declared_sources_digest" = "$actual_sources_digest" || { echo "implementation source manifest digest does not match pinned sources" >&2; exit 1; }
test "$declared_sources_digest" != "$mutated_sources_digest" || { echo "implementation identity source mutation unexpectedly passed" >&2; exit 1; }
declared_package_sha=$(jq -r '.packageSha256' "$source_manifest")
captured_package_sha=$(jq -r '.sha256' "$repo_root/docs/dependency-surface/FS.GG.Game.Core/0.13.0.json")
test "$declared_package_sha" = "$captured_package_sha" || { echo "Game.Core implementation fingerprint does not match dependency receipt" >&2; exit 1; }
test "$(jq -r '.algorithmFingerprint' "$source_manifest")" = "FS.GG.Game.Core@0.13.0:Los.lineOfSightBy:Supercover" || { echo "Game.Core algorithm fingerprint changed" >&2; exit 1; }

copied_semantics_pattern='(baseDamage|expectedDamage).*(trace|retention)|(trace|retention).*(baseDamage|expectedDamage)'
if test "${SIR_RULES_FORCE_GREP:-0}" != 1 && command -v rg >/dev/null 2>&1; then
  copied_semantics=$(rg -n --glob '*.js' --glob '*.ts' --glob '!**/.fable*/**' "$copied_semantics_pattern" "$repo_root/src" || true)
else
  copied_semantics=$(find "$repo_root/src" -type f \( -name '*.js' -o -name '*.ts' \) ! -path '*/.fable*/*' -exec grep -EnH -- "$copied_semantics_pattern" {} + || true)
fi
if test -n "$copied_semantics"; then
  printf '%s\n' "$copied_semantics"
  echo "copied JavaScript/TypeScript combat semantics detected" >&2
  exit 1
fi

# ---------------------------------------------------------------------------------------------
# Execute the corpus (S.I.R.#264 round 1).
#
# A rebindable correspondence baseline CANNOT be the only thing standing between a changed
# implementation source and a green gate. Everything above this line is a comparison of
# DECLARATIONS: regenerated manifest/coverage/representative-application, sealed digests, recorded
# per-path text digests. A change to an algorithm BODY moves none of them -- `implementationDigest`
# is a sealed literal, `semanticDigest` derives from it plus the DECLARATIVE rule payload, and the
# representative application does not exercise every registered symbol. Byte identity against the
# pin used to be the only detector of that class, and making it rebindable retires it.
#
# So execute the rules rather than only re-describing them. An independent critic demonstrated the
# gap on this pull request: mutating CombatRules.resolveCoverImpact -- the symbol manifest.json
# binds to COMBAT-COVER-003 and COMBAT-COVER-DESTRUCTION-001 -- and rebinding its correspondence in
# the same tree left every declared artifact byte-identical and this gate green, while the corpus
# fixtures refused. This step is what makes the rebind path safe, and it is deliberately NOT
# restricted to rule-hosting paths: FixedPoint.fs, CanonicalEncoding.fs, Rules.fs and CombatModel.fs
# are all pinned, none is rule-hosting, and a damage-arithmetic change in any of them moves rule
# behaviour while remaining rebindable.
conformance_log=$(mktemp /tmp/sir-rules-conformance.XXXXXX)
if ! dotnet run --project "$project" -c Release --no-build >"$conformance_log" 2>&1; then
  echo "registered executable behaviour does not satisfy the rules corpus fixtures" >&2
  echo "  a rebound source correspondence cannot make this pass: the corpus is executed, not described" >&2
  grep -iE 'exception|failwith|did not' "$conformance_log" | head -5 >&2
  rm -f "$conformance_log"
  exit 1
fi
rm -f "$conformance_log"

# And prove that execution is not vacuous: a divergence injected into the combat route -- the same
# class as the critic's mutation, without needing a rebuild -- must be refused.
#
# The exit code alone CANNOT establish that. `--inject-combat-divergence` computes its offset with
# Array.findIndex, which THROWS when the two evaluations agree, and then failwiths regardless -- so
# the process aborts whether or not a divergence was found, and an exit-code-only check passes even
# when its subject is broken. That is the vacuity this whole gate exists to refuse, and the first
# version of this guard had it (S.I.R.#264 review round 1). Assert the diagnostic, exactly as the
# adjacent rules-corpus mutation below does: if CombatFixtures.evaluate stops diverging, findIndex
# throws before printing and this line is absent.
combat_divergence_log=$(mktemp /tmp/sir-rules-combat-divergence.XXXXXX)
if dotnet run --project "$project" -c Release --no-build -- --inject-combat-divergence >"$combat_divergence_log" 2>&1; then
  echo "combat divergence mutation unexpectedly passed the corpus conformance route" >&2
  rm -f "$combat_divergence_log"
  exit 1
fi
search_quiet 'first divergence: fixture=physical-combat' "$combat_divergence_log" || {
  echo "combat divergence mutation failed without the actionable divergence diagnostic" >&2
  echo "  the injected mutation did not actually diverge, so this guard proved nothing" >&2
  rm -f "$combat_divergence_log"
  exit 1
}
rm -f "$combat_divergence_log"

mutation_log=$(mktemp /tmp/sir-rules-mutation.XXXXXX)
trap 'rm -f "$mutation_log"' EXIT
if dotnet run --project "$project" -c Release --no-build -- --inject-rules-corpus-divergence >"$mutation_log" 2>&1; then
  echo "rules-corpus protected-subject mutation unexpectedly passed" >&2
  exit 1
fi
search_quiet 'first divergence: fixture=rules-corpus' "$mutation_log" || {
  echo "rules-corpus mutation failed without the actionable divergence diagnostic" >&2
  exit 1
}

echo "rules corpus generation, source resolution, copied-semantics, and mutation gates passed"
