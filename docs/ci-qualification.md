# CI qualification routes

S.I.R. has four explicit qualification routes. They share evidence contracts but not authority.

- Pull requests run the focused lane in `.github/workflows/ci.yml`: changed paths are classified, the always-on integrity floor runs, selected prerequisites are built once into a content-addressed artifact chain, independent named gates run concurrently, and `pr-verdict` joins their typed results. Every omitted gate has a route-policy reason; unknown or mixed product paths select the conservative cross-cutting route.
- Protected `main` pushes run `./scripts/qualify-production.sh --protected`. This is the complete clean-room ship-boundary route; it does not trust a PR cache as evidence.
- The nightly schedule runs the same protected command and first proves that a cross-surface rules defect legitimately omitted by a documentation-focused route is rejected by the full surface.
- Local development uses `./scripts/qualify-pr.sh route <changed-path-file>`, then `integrity`, `prepare`, and `gate <id>` for the selected gates. `npm run test:ci-route` is the fast route/join/workflow contract suite. `npm run qualify:production` remains the comparable local full qualification command.

The PR budget is 300 seconds from its route runner start to the actionable join verdict on `ubuntu-latest`. It is a runner-feedback budget, not a gameplay-performance assertion. Gate receipts record setup/restore/build/test/total durations, cache and receipt reuse, build invocations, retries, failure stage, critical path, and runner-minutes. Missing, stale, malformed, duplicate, cancelled, failed, or unexpected required results fail closed.

Prepared outputs are acceleration only. `sir.ci-artifact-manifest/v1` chains the exact route to `sir.production-build-receipt/v1`; every consumer verifies candidate commit/tree, tracked inputs and locks, tool versions, owning command, expected paths, and output identities before and after use. A consumer never rebuilds silently when reuse fails. Clean-room mutation subjects remain named exceptions because their purpose is to rebuild a deliberately changed source and prove rejection/restoration.
