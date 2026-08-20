---
schemaVersion: 1
workId: 239-durable-rules-identity
title: Durable rules corpus source identity
stage: charter
changeTier: tier1
status: chartered
policyPointers:
  - .fsgg/sdd.yml
  - .fsgg/agents.yml
  - .fsgg/policy.yml
  - .fsgg/capabilities.yml
  - .fsgg/tooling.yml
---

# Durable Rules Corpus Source Identity Charter

## Identity
- Make the retained rules corpus reproducible from a fresh GitHub checkout by binding its source identity to a commit reachable from canonical `origin/main`.
- Work id: `239-durable-rules-identity`; issue: `EHotwagner/S.I.R.#239`.

## Principles
- Treat the manifest and its retained evidence as one dependency cone: source identity, semantic digest, generated fixtures, and governance receipts move together.
- Refuse unverifiable or checkout-local identities before generation, with actionable diagnostics for malformed, missing, non-canonical, and unreachable commits.
- Prove portability with a real full network clone that has no object alternates.

## Scope Boundaries
- In: the rules corpus source pin, its implementation digest and retained fixtures, fail-closed verifier gates and mutations, a fresh-clone proof, this SDD package, and the affected Governance binding/protected-boundary receipts.
- Out: gameplay-rule semantic changes, new rules, broad qualification redesign, and relaxing any protected rule or evidence gate.

## Policy Pointers
- Constitution II requires manifest and generated evidence identities to remain a coherent structured contract.
- Constitution VI requires executable portability and refusal evidence; VIII requires early actionable failure for invalid source identities.
- `.fsgg/sdd.yml`, `.fsgg/agents.yml`, and the current delivery-route receipt define the required SDD lifecycle; Governance remains the protected-boundary owner.

## Lifecycle Notes
- Tier 1: this changes a retained artifact identity, verifier contract, and protected-boundary evidence bindings.
- The delivery route was revised from lightweight to SDD-required after hosted Governance correctly rejected the changed rules identity without a current ship artifact.
- Next lifecycle action: `fsgg-sdd specify --work 239-durable-rules-identity`.
