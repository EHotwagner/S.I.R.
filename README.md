# S.I.R.

S.I.R. is a fast-paced, grid-based, real-time tactical skirmish game for large
forces in a near-future world undergoing an incursion by monsters and magic.
Players command rather than puppeteer: units execute player-supplied WebAssembly
control logic on the authoritative server while humans direct squads, doctrine,
intelligence, communications, and logistics.

## Documentation

The [published S.I.R. documentation](https://ehotwagner.github.io/S.I.R./) is
the authoritative source for the game design, gameplay rules, architecture,
implementation status, research, API reference, and development roadmap. The
README is intentionally limited to repository orientation and build commands;
project information belongs in the published documentation.

> **URL note:** the repository name ends in a period, so the Pages project path
> is `/S.I.R./`. The final period in
> `https://ehotwagner.github.io/S.I.R./` is required; `/S.I.R/` is a different
> path and returns 404.

Start with:

- [S.I.R. Combat in Quint handbook](https://ehotwagner.github.io/S.I.R./sir-combat-quint-handbook.html)
  — learn Quint through the bounded physical-combat model, or audit its rule and evidence traceability.
- [Game vision](https://ehotwagner.github.io/S.I.R./game-vision.html) — the
  authoritative living description of the intended game.
- [Gameplay reference](https://ehotwagner.github.io/S.I.R./gameplay-reference.html)
  — the indexed rules and content corpus.
- [Codebase architecture](https://ehotwagner.github.io/S.I.R./codebase-architecture.html)
  — solution structure, boundaries, and dependencies.
- [Fable client and documentation architecture](https://ehotwagner.github.io/S.I.R./fable-client-and-documentation.html)
  — shared .NET/Fable simulation, browser tooling, replay, and roadmap.
- [Interactive replay and rules laboratory](https://ehotwagner.github.io/S.I.R./interactive-rules-lab.html)
  — the browser-hosted replay inspector and balance laboratory.
- [API reference](https://ehotwagner.github.io/S.I.R./reference/) — generated
  documentation for the implemented F# libraries.

Repository Markdown under `docs/` is the source used to generate that site.
Use the published pages when reading or linking documentation so navigation,
cross-references, evaluated examples, API pages, and interactive content remain
available together.

## Project board

The canonical planning and delivery board for this repository is the
[EHotwagner / S.I.R. project](https://github.com/users/EHotwagner/projects/6).
All issues owned by `EHotwagner/S.I.R.` belong there, including work that has
cross-repository dependencies. The FS-GG organization Coordination project is
not the S.I.R. product backlog. Producer-owned dependencies stay with their
owning repository and are linked from S.I.R. issues with `Blocked by:`.

## Build and test

The deterministic gameplay foundation is built from the root solution:

```bash
npm ci
./scripts/test-conformance.sh
```

## Browser tests

The browser suite uses the Chromium revision pinned by `@playwright/test` unless
`PLAYWRIGHT_EXECUTABLE_PATH` selects a supported system Chromium. After a clean
clone, provision the pinned browser once, then run the suite:

```bash
npm ci
npm run setup:browser
dotnet publish src/SIR.Server/SIR.Server.fsproj -c Release -o artifacts/publish
npm run test:browser
```

`npm run setup:browser` uses Playwright's revisioned browser cache, so its
downloads remain cacheable without being mistaken for a browser from a different
Playwright release. The test output reports the selected policy, executable, and
browser version. In restricted environments, set
`PLAYWRIGHT_EXECUTABLE_PATH=/path/to/chromium` before running the test; if no
browser is available, the failure names the same provisioning command.

Build the complete documentation site locally with:

```bash
npm ci
./scripts/build-docs.sh
```

Generated documentation is written beneath `artifacts/site/`. The conformance
gate verifies the shared .NET/Fable kernel, authoritative replay qualification,
browser application, worker protocol, and compatibility baseline.

## Repository layout

- `src/` — shared domain, simulation, match, client, and browser projects.
- `tests/` — .NET, Fable, match, and client conformance tests.
- `docs/` — source content for the authoritative published documentation.
- `scripts/` — locked build, conformance, publication, and verification gates.
- `spikes/` — bounded research programs retained as supporting evidence.

## Contributing

Changes to behavior, architecture, or status must update the relevant
documentation source and pass both the conformance and documentation builds.
The published documentation should remain complete and internally consistent;
the README should link to it instead of duplicating canonical project details.

## License

Licensed under the [GNU Affero General Public License v3.0](LICENSE).
