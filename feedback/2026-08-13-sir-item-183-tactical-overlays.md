---
feedbackSchema: 2
date: 2026-08-13
workspace: S.I.R
cycle: item-183-tactical-overlays
lane: sdd
toolVersion: 1.0.1
commit: development-head
---

## §1 Provenance and confidence

- **activation:** active
- **phases:** lifecycle-authoring, implementation-test-evidence, verify-ship-pr
- **material events:** 4
- **checkpoint:** `feedback/checkpoints/item-183-tactical-overlays.jsonl` (4 records)
- **confidence limits:** Native qualification, Fable/Vite production delivery, published Chromium, accessibility zoom, disclosure inversions, and SDD readiness are observed. Independent exact-head review and hosted CI remain pending.

## §2 What worked

A single public registry owns overlay identity, label, category, command ID/default gesture, modes, ordering, availability, disclosure policy, and payload type. Command availability/default gestures and projected payload metadata/policy are consumed from those descriptors. Keeping projection renderer-neutral allowed native tests to prove exact LOS retention and disclosure-first behavior while the browser only rendered disclosed payloads.

## §3 What did not

The first aggregate run found two fixed-capacity test assumptions: the smoke oracle expected seven SVG layers, and the browser test host allowed only 32 bootstraps after the suite grew to 33 journeys. Both test-only capacities were updated to describe the new bounded inventory.

## §4 Findings

#### §4.1 Registry-derived presentation prevents client truth invention

- **Kind:** positive-pattern
- **Impact:** View controls and rendered overlays reuse descriptor identity, label, order, availability, default gesture, modes, payload kind, and disclosure policy instead of re-authoring them.
- **Expected:** One canonical registry and authoritative disclosed payload input.
- **Observed:** `CommandRegistry.fs` derives commands from the Client registry and filters its availability; `projectOverlays` enforces descriptor disclosure policy and stamps payload kind; the SVG renders that projection. The focused journey passes, while the retained detached-registry inversion receipt is red.
- **Evidence:** source:src/SIR.Client.Web/CommandRegistry.fs; source:src/SIR.Client/TacticalSceneProjection.fs; readiness/183-tactical-overlays/tactical-overlays-browser.junit.xml; readiness/183-tactical-overlays/tactical-overlays-browser-inversion.junit.xml
- **Version:** development head
- **Owner:** EHotwagner/S.I.R. tactical overlays
- **Recurrence:** new
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.2 Disclosure-first projection is cheaper to audit

- **Kind:** positive-pattern
- **Impact:** A malformed authority envelope cannot leak shape count, labels, or per-item filtering work.
- **Expected:** Reject before payload mapping.
- **Observed:** When `PreservesFieldDisclosures=false`, projection returns zero payloads, labels, and candidates before payload mapping; its declarative cost record reports one disclosure-pass and one registry-traversal budget unit. On a disclosed injected LOS route, exact points and route-kind tokens are copied unchanged.
- **Evidence:** command:dotnet run --project tests/SIR.Client.Tests/SIR.Client.Tests.fsproj -c Release --no-build
- **Version:** development head
- **Owner:** EHotwagner/S.I.R. tactical projection
- **Recurrence:** new
- **Avoidable cost:** none
- **Disposition:** accepted

The structural counters are declarative budget markers rather than runtime instrumentation, and this gate exercises the disclosure-preservation flag rather than every possible malformed authority input.

#### §4.3 Growing browser inventories need explicit test-host headroom

- **Kind:** friction
- **Impact:** The aggregate suite reached the admission cap only in its final live-session journeys.
- **Expected:** The test-only server cap remains above the serial bounded browser inventory.
- **Observed:** The test-host cap moved from 32 to 40; the server fallback remains 8 in `LiveAuthority.fs`, and the final JUnit records 33 tests, zero failures, and one intentional skip.
- **Evidence:** source:tests/SIR.Browser.Tests/playwright.config.js; source:src/SIR.Server/LiveAuthority.fs; readiness/183-tactical-overlays/tactical-overlays-all-browser.junit.xml
- **Version:** development head
- **Owner:** EHotwagner/S.I.R. browser qualification
- **Recurrence:** new
- **Avoidable cost:** historical rerun count is not retained as a durable receipt
- **Disposition:** accepted test-only capacity update

## §5 Did not exercise

No server protocol, simulation authority, save format, or upstream package release changed.

## §6 Doc-versus-behavior contradictions

None known.

## §7 Workarounds still in the tree

Overlay payloads adapt the existing disclosed scene vocabulary; richer future tactical facts should extend authority projections rather than add browser evaluators.

## §8 Friction and avoidable cost

One aggregate rerun followed repair of the stale layer-count oracle and bounded browser-host capacity.

## §9 Skill value and gaps

The SDD, project, grids, line-drawing, visibility, playtest, and parallel-work contracts kept authority, exactness, browser evidence, and delivery boundaries explicit.

## §10 Outcome markers

Native Release qualification is green with overlay p95 below 2 ms. Production Fable/Vite, smoke, focused DOM, and full Chromium are green. Verify records 54 ready test dispositions plus 54 ready evidence dispositions with zero findings; ship has zero findings and diagnostics.

## §11 Falsifiable improvements

- Retain approximate-LOS, unreadable-preference, and detached-registry inversions.
- Keep the browser test-host admission cap above the counted serial journey inventory.
- Extend the authority scene before adding any new tactical overlay payload kind.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| sdd-authoring | exercised | Charter through ship is current and ship-ready. |
| implementation-apis | exercised | Public registry, preferences, diagnostics, and projection added. |
| dependencies-build | exercised | Native and production Fable builds pass. |
| testing | exercised | Native, smoke, focused and complete Chromium pass. |
| evidence | exercised | 54 observed obligations; zero synthetic satisfaction. |
| runtime-playtest | exercised | Production DOM View/shortcut/reload/400% journey passes. |
| performance | exercised | 100/200-unit bounded projection and node caps pass. |
| documentation | exercised | Tactical overlay authority and operating contract documented. |
| worker-git-pr | partial | Commit, PR, exact-head review, and landing pending. |
