#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
cd "$repo_root"

if [[ ${SIR_PROTECTED_SCHEDULED:-false} == true ]]; then
  ./scripts/test-ci-scheduled-full-mutation.sh
fi
./scripts/verify-rules-corpus.sh
./scripts/verify-spatial-query.sh --static-only
./scripts/test-worker-cancellation-subject-mutation.sh
