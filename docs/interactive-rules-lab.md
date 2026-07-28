---
title: Interactive replay and rules laboratory
category: Interactive
categoryindex: 3
index: 1
description: Run the Fable replay inspector and exploratory rules laboratory inside the documentation site.
---

# Interactive replay and rules laboratory

<div class="sir-status sir-status-browser">
  <strong>Runtime: Fable/JavaScript in your browser.</strong>
  Replay verification re-executes accepted kernel inputs; it does not re-run
  player WASM and cannot establish authoritative match verification. That
  stronger claim is produced only by the .NET verifier after exact-artifact
  Wasmtime re-execution reproduces the accepted-output journal.
</div>

The application below executes replay and laboratory work in a Web Worker.
Changing a parameter creates a derived sandbox identity and permanently ends
any verification claim for that run. Laboratory charts are projections of
canonical integer results and are labeled as exploratory evidence.

No replay file is required for the laboratory. Choose one of the scenario cards
at the top of the application, run its fixed baseline, edit attack power or
attack count, then sweep either parameter or export the reproducible result.
The scenarios include short and sustained exchanges, one heavy strike, rapid
chip damage, and both sides of the exact lethality threshold.

<noscript>
  <div class="sir-status sir-status-warning">
    <strong>JavaScript is disabled.</strong>
    The interactive replay and rules laboratory cannot run, but the explanatory
    corpus, deterministic .NET example, architecture pages, and API reference
    remain available.
  </div>
</noscript>

<div id="sir-replay-app" aria-label="S.I.R. interactive replay and rules laboratory">
  <p>Loading the Fable browser application…</p>
</div>

## Trust boundary

The browser receives only replay or scenario data deliberately supplied to it.
It is not a live match client, does not receive hidden authoritative state, and
does not silently upgrade old replay engines. Read the
[Fable client and documentation architecture](fable-client-and-documentation.md)
for the complete verification and disclosure contract.
