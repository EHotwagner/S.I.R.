---
schemaVersion: 1
workId: 377-handbook-m6v
title: Handbook M6V visual explanations
stage: clarify
changeTier: tier1
status: clarified
sourceSpec: work/377-handbook-m6v/spec.md
publicOrToolFacingImpact: true
---

# Handbook M6V visual explanations Clarifications

## Source Specification
- work/377-handbook-m6v/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001] blocking open: Resolve source ambiguity AMB-001 before checklist.
- CQ-002 [AMB:AMB-002] blocking open: Resolve source ambiguity AMB-002 before checklist.
- CQ-003 [AMB:AMB-003] blocking open: Resolve source ambiguity AMB-003 before checklist.
- CQ-004 [AMB:AMB-004] blocking open: Resolve source ambiguity AMB-004 before checklist.

## Answers
- CQ-001: Commit reviewable authored SVGs and a schema-versioned binding manifest; mechanically compare every authority-derived token, exact glyph primitive, rule/declaration reference, and declared visual fingerprint against current sources. The SVGs explain; authorities decide.
- CQ-002: Use pure SVG filter primitives (`feGaussianBlur`, `feColorMatrix`, and compositing) as the shader-like progressive layer. Never require WebGL; base SVG stays complete and the unsupported-effects route removes filters without semantic loss.
- CQ-003: Import the producer posture and capability discipline, not unrelated tactical numeric limits. Declare a typed documentation workload with structural node/byte/animation limits before code, inspect the production FsDocs/browser route, and report no live compositor/FPS claim.
- CQ-004: Bind regression to canonicalized SVG semantics and DOM fingerprints, then perform actual browser screenshots in normal, reduced-motion, print, and effects-disabled modes. Raster bytes are inspection artifacts, not cross-host equality claims.

## Decisions
- DEC-001 [CQ-001] [AMB:AMB-001]: Authored SVG plus a checked binding manifest is the reviewable publication shape; exact source-token, digest, rule, declaration, and glyph comparisons prevent it from becoming a second authority.
- DEC-002 [CQ-002] [AMB:AMB-002]: SVG filter primitives are the only shader-like enhancement. Every effect is removable through the same `data-effects="off"` route used by the audit and preserves static geometry/text.
- DEC-003 [CQ-003] [AMB:AMB-003]: The M6V performance contract covers six diagrams on the real docs route with predeclared structural budgets and capability facts; tactical workload budgets remain producer-owned and unchanged.
- DEC-004 [CQ-004] [AMB:AMB-004]: Semantic DOM fingerprints are deterministic regression gates; Chromium screenshots and computed-layout inspection provide actual-render evidence with honest host limits.

## Accepted Deferrals
No accepted deferrals recorded.

## Remaining Ambiguity
No blocking ambiguity remains.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 377-handbook-m6v`.
