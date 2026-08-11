# Issue 138: fs-gg-fable-game integration report

## Adopted identities

- Template: `FS.GG.Workspace.Template` 0.8.0, `fs-gg-fable-game`, provider contract 1.1.0, tag `fs-gg-templates/v0.8.0`, commit `fef78f5b96280a235770712439571367517956ed`.
- Lifecycle: `FS.GG.SDD.Cli` 1.0.0, one work item at `work/138-sir-fable-game-scaffold/`.
- Game skill: `FS.GG.Game.Skills` 0.7.0, `fs-gg-game-fable`, digest `443a82d24a0b4bbd21f4499b06f6e3d12b95a36a858f3880b414b74cae1a5c50`, matching the production materializer receipt in FS.GG.SDD#817.
- Game runtime: `FS.GG.Game.Core` 0.13.0 and profile `fs-gg-game-core-fable-lockstep-v1`.

Machine-verifiable identities live in `config/scaffold-provenance.json`; `npm run verify:scaffold` validates the materialized skill bytes, restored compatibility metadata and oracle, exact pins, and absence of sibling project/file dependencies.

## Migration mapping

| Published lane | S.I.R. lane | Migration |
| --- | --- | --- |
| `Domain` | `src/SIR.Domain`, `src/SIR.Simulation`, `src/SIR.Match` | Preserved; existing deterministic simulation, live qualification, canonical encoding, replay, and controller boundaries remain consumer-owned. |
| `Protocol` | `src/SIR.Protocol` | Added bounded bootstrap and realtime DTOs with named .NET/Fable codecs. No privileged domain type crosses the wire. |
| `Server` | `src/SIR.Server` | Added ASP.NET Core host and SignalR hub around `SIR.Match.LiveIntegration`; the host does not own gameplay rules. |
| `Client` | `src/SIR.Client`, `src/SIR.Replay.Web` | The live client is protocol-only; the replay/editor Fable host owns the existing Elmish/Feliz tactical workspace and narrow HTTP/SignalR live-session adapter. |
| `Domain.Tests` / `Protocol.Tests` / `Server.Tests` | Existing shared .NET/Fable suites plus browser integration | Existing canonical and match qualification evidence remains; the browser test exercises both named codec and authoritative transport runtimes. |
| `Browser.Tests` | `tests/SIR.Browser.Tests` | Added production-publish test for advance, forced disconnect/reconnect, and bounded full resync. |
| root lifecycle | `build.sh`, `scripts/test-conformance.sh`, `.github/workflows/ci.yml` | One orchestration path owns restore, build, tests, production bundle/publish, browser smoke, provenance, SDD evidence import, and doctor. |

No existing S.I.R. feature lane was deleted or replaced by the template arena sample.

## Project-graph reconciliation

Issue #153 restores the originally documented separated graph. `SIR.Wasm` now
owns the concrete Wasmtime adapter, `SIR.Protocol.Generated` owns transport
records/codecs, `SIR.Replay.Web` owns the Fable replay/editor host, and
`SIR.Tools` is the command host. This is a boundary restoration, not a transport
rewrite: the HTTP/Thoth plus SignalR vertical slice remains the released path.

## Deliberate deviations

Issue #138 asks for Fable.Remoting request/response traffic. The published 0.8.0 workspace contract explicitly superseded Fable.Remoting in ADR-0073: typed request/response uses plain ASP.NET Core HTTP plus explicit named Thoth codecs, while SignalR remains the session transport. This integration follows the published contract and records the mismatch rather than reintroducing the superseded proxy boundary.

The scaffold's toy room and client are not retained. The same generic transport split, project boundaries, package locks, production publish shape, and browser lifecycle wrap S.I.R.'s real `LiveIntegration` replay/admission/reconnect logic instead.

## Authoritative slice and resync

`POST /api/bootstrap` admits an actor against the real S.I.R. match lock and returns the first knowledge-scoped `LiveProjectionFrame`. `GameHub` accepts only monotonic advance intents, selects the next authoritative replay frame, and broadcasts an explicit snapshot. A reconnect retains the server session and always sends a bounded full current snapshot; the client never applies a local prediction as authority. The Playwright scenario asserts advance and reconnect/resync against the published server output.

## Package-only and exactness boundary

All Game.Core consumption is through the public NuGet package. The adopted skill restricts cross-runtime authority to the four profile-v1 `LockstepExact` surfaces: `Cell` order, `Edges.edgeBetween`, `Los.lineOfSightBy`, and `Pathfinding.astar`. Existing S.I.R. canonical tests compare complete .NET and Fable byte streams and retain first-divergence controls; product protocol, hashes, and replay identities remain explicitly S.I.R.-owned.

## Producer follow-ups

- The issue's Fable.Remoting wording should be updated to the released ADR-0073 HTTP-codec contract so future consumers do not face contradictory acceptance text.
- A production SDD scaffold into an already initialized target refuses to replace authored `.fsgg/scaffold-provenance.json`; in-place receiver adoption therefore needs a documented merge/adopt mode. This consumer records and verifies the exact upstream receipt and package-owned skill bytes without claiming the refusal was a successful in-place materialization.
