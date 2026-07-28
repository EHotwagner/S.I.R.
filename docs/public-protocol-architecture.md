---
title: S.I.R. Public gRPC Protocol Architecture
status: accepted
decision-status: canonical
document-type: living-architecture
version: "0.3"
last-updated: 2026-07-28
related:
  - docs/game-vision.md
  - docs/technology-stack.md
  - docs/codebase-architecture.md
  - docs/research/public-transport-selection.md
  - docs/wasm-control-architecture.md
  - docs/mission-lifecycle.md
  - docs/cross-runtime-replay.md
---

# S.I.R. Public gRPC Protocol Architecture

## Decision status

This document defines the canonical protocol architecture for the native gRPC
transport, including the service split, live-session envelope, sequencing,
resume, projection, backpressure, and compatibility model.

## Goals

The public protocol must:

- give the canonical client no gameplay privilege over custom clients;
- generate usable clients in multiple languages;
- carry a continuous knowledge-filtered match projection;
- transport opaque player-to-HQ and HQ-to-player payloads;
- tolerate delay, disconnect, duplicate delivery, and reconnect;
- distinguish transport receipt from authoritative gameplay acceptance;
- prevent API metadata from revealing hidden participants or state;
- support single-player and multiplayer through the same server path;
- remain bounded under slow, malicious, or faulty clients; and
- evolve without silently changing match rules.

## Contract ownership

Canonical `.proto` files under `schemas/protocol/` are the language-neutral
source of truth:

```text
schemas/protocol/sir/api/v1/
├── common.proto
├── discovery.proto
├── account.proto
├── catalog.proto
├── artifact.proto
├── match.proto
├── session.proto
└── replay.proto
```

All initial packages use:

```protobuf
syntax = "proto3";
package sir.api.v1;
```

The repository commits schemas and generated F# sources. Generation is an
explicit reproducible step, not an implicit dependency on whatever generator
happens to be installed. Buf lint and breaking-change checks gate schema
changes.

The generated representation is not the authoritative domain model.
`SIR.Protocol` converts untrusted generated messages into validated
`SIR.Domain` values and converts knowledge-filtered domain projections back
into wire messages.

## Canonical project boundary

The project split is:

```text
SIR.Protocol.Generated
  generated immutable F# protobuf and gRPC types
           │
           ▼
SIR.Protocol
  validation, limits, compatibility, domain mapping, session semantics
```

FsGrpc is the preferred F# generator because it produces immutable records and
discriminated unions for `oneof`. Generated sources are committed so ordinary
builds and downstream source inspection do not require the generator.

If FsGrpc cannot satisfy the accepted streaming or tooling requirements, a
small C# `Grpc.Tools` generated project is the fallback. The `.proto` contract
does not change with the canonical implementation's generator.

## Service surface

### `DiscoveryService`

Unauthenticated or minimally authenticated discovery:

- `GetServerInfo`;
- supported API major versions;
- canonical server build identity;
- public ruleset and content versions;
- authentication authority information;
- service limits safe to disclose; and
- maintenance or compatibility status.

Discovery never reveals active match count, hidden allocations, participants,
or other campaign-sensitive operational data.

### `AccountService`

Authenticated account-level operations:

- current account and company identity;
- campaign memberships visible to the account;
- owned artifact summaries;
- public account preferences; and
- later persistent roster and company operations.

Authentication itself should use a standard bearer-token or mutually
authenticated deployment mechanism. Credentials are carried through gRPC
metadata, not embedded repeatedly in gameplay envelopes.

### `CatalogService`

Immutable or versioned public data:

- ruleset manifests;
- content catalogs;
- mode definitions;
- point catalogs;
- capability schemas;
- execution profiles;
- map or scenario metadata allowed before a match; and
- content hashes and download locations.

Large immutable content can be served through cacheable object or HTTP delivery
when appropriate. gRPC remains the discovery and integrity-contract surface.

### `ArtifactService`

Control-module lifecycle between matches:

- upload an immutable WASM artifact;
- inspect hash and validation result;
- list compatible host classes and execution profiles;
- retrieve diagnostics visible to the owner;
- assign or unassign an artifact before lock-in; and
- delete or retire an artifact when persistence rules permit.

Uploads have declared and enforced size limits. The server computes the
canonical content hash and never trusts a client-supplied hash as proof.

### `MatchService`

Pre-live and post-live match operations:

- create or join an allowed skirmish;
- retrieve the player's disclosed match configuration;
- submit force and deployment choices;
- lock force and module assignments;
- transition ready state;
- obtain a one-use session admission token;
- retrieve the player's result after completion; and
- abandon or surrender when the mode permits.

The API returns only information the requesting account is entitled to know.
For hidden co-allocation, it does not expose participant counts, rosters,
identities, slots, connection state, or allocation metadata.

### `SessionService`

Exactly one initial RPC:

```protobuf
service SessionService {
  rpc OpenSession(stream ClientEnvelope)
      returns (stream ServerEnvelope);
}
```

One logical participant session may reconnect across several physical gRPC
streams. Only one stream is the active writer for that logical session at a
time. Takeover, stale-stream closure, and duplicate connection rules are
explicit.

### `ReplayService`

Replay and diagnostic operations allowed by disclosure policy:

- retrieve replay metadata;
- stream replay segments;
- retrieve public result records;
- verify content and execution-profile dependencies; and
- request state-hash or divergence information when permitted.

Replay access is authorization- and knowledge-scoped. A completed match does not
automatically expose opponent code or observations that remain campaign-secret.

An authorized full replay package can contain complete snapshots, ordered
kernel inputs, accepted WASM outputs, checkpoints, and hashes for deterministic
browser re-simulation. A player-perspective package contains only the
knowledge-filtered projections and messages the player may receive and is
projection playback rather than complete re-simulation. The protocol labels
the disclosure scope and verification level explicitly. See
[Cross-Runtime Determinism and Browser Replay](cross-runtime-replay.md).

## Live-session envelopes

The precise field numbers remain to be assigned in `.proto`, but the logical
shape is:

```protobuf
message ClientEnvelope {
  uint64 client_sequence = 1;
  uint64 acknowledged_server_sequence = 2;

  oneof payload {
    OpenSessionRequest open = 10;
    Heartbeat heartbeat = 11;
    HqPayload hq_payload = 12;
    MatchDecision decision = 13;
    ResyncRequest resync = 14;
  }
}

message ServerEnvelope {
  uint64 server_sequence = 1;
  uint64 acknowledged_client_sequence = 2;
  uint64 simulation_tick = 3;

  oneof payload {
    SessionAccepted accepted = 10;
    Heartbeat heartbeat = 11;
    ClientInputResult input_result = 12;
    ProjectionSnapshot snapshot = 13;
    ProjectionDelta delta = 14;
    HqPayload hq_payload = 15;
    ResyncRequired resync_required = 16;
    MatchCompleted completed = 17;
    SessionNotice notice = 18;
  }
}
```

The first client payload on every physical stream is `OpenSessionRequest`.
Later envelopes cannot repeat it.

Every envelope and nested message has a declared decoded-size and element-count
limit. Unknown fields do not bypass these bounds.

## Identity representation

Different identity scopes use different wire representations:

- globally persistent opaque identifiers such as account, campaign, match,
  artifact, and session identifiers use fixed-length `bytes` with exact length
  validation;
- match-local high-frequency entity identifiers use an unsigned fixed-width
  numeric value scoped by match;
- content, ruleset, ABI, execution-profile, and capability identifiers use
  stable names plus explicit versions; and
- opaque resume and admission tokens are bounded bytes whose internal form is
  not a public contract.

Generated primitive values are wrapped or validated before entering the domain.
Zero or empty values mean “unspecified” only where the message explicitly
permits that state.

## Open and negotiation

`OpenSessionRequest` carries:

- match identifier;
- one-use admission or reconnect token;
- protocol major and minor capability information;
- client build description for diagnostics;
- last applied server sequence;
- last applied projection revision; and
- optional compression and feature capabilities.

`SessionAccepted` returns:

- selected protocol and feature set;
- logical session identifier;
- active ruleset, content, ABI, and execution-profile versions;
- current server sequence and simulation tick;
- heartbeat and public limit policy;
- whether delta resume succeeded; and
- the next required client action.

Protocol negotiation cannot select different gameplay rules or execution
budgets for different clients in the same match. It selects representation and
optional presentation-safe features only.

## Client sequencing and authoritative acceptance

Every mutating client envelope has a monotonically increasing
`client_sequence`. The server remembers a bounded deduplication window per
logical session.

Sequence acknowledgement means:

> The server has received, authenticated, structurally validated, and recorded
> or rejected this client envelope for the logical session.

It does not mean that a requested gameplay action was legal or accepted.

Every gameplay-relevant request also carries a stable client input identifier.
The server emits `ClientInputResult` containing:

- input identifier;
- accepted, rejected, expired, or duplicate status;
- stable reason code;
- assigned authoritative target tick when accepted; and
- permitted diagnostic detail.

The match coordinator, not the client, assigns authoritative tick and ordering.
Retrying an input identifier is idempotent and cannot create another gameplay
action.

## Server sequence and projection revision

`server_sequence` orders envelopes within one logical participant session.
It is not the simulation tick.

Knowledge projections have their own monotonically increasing revision:

- a snapshot establishes revision `R`;
- a delta declares base revision `R` and result revision `R+1` or another
  explicit next revision;
- a client applies a delta only to its declared base;
- mismatch triggers resynchronization rather than best-effort application; and
- reconnect can resume by server sequence only if all necessary projection
  revisions remain available.

The server can emit more than one envelope per simulation tick or batch several
ticks when information does not require immediate delivery. Presentation
interpolation is client-local.

## Knowledge-filtered snapshot and delta

The projection builder runs before serialization and receives only the actor's
permitted knowledge. Neither the Protobuf mapper nor stream writer receives the
unfiltered authoritative world as a convenience.

A snapshot contains:

- the recipient's current known entities and facts;
- current HQ-visible force, objective, communication, and logistics state;
- the projection revision and applicable simulation tick;
- stable provenance or certainty data where the rules provide it; and
- no tombstone for an entity the recipient was never entitled to know.

A delta can:

- add or update a disclosed fact;
- expire or retract a fact according to the knowledge model;
- deliver a fixed report or HQ payload;
- advance disclosed objective or mission state; and
- remove a projection object whose public lifecycle ended.

The protocol transmits information facts. The client chooses how to visualize
certainty, age, provenance, color, overlays, and alerts.

## Reconnect and resume

Reconnect creates a new physical stream for an existing logical session:

1. authenticate;
2. present the bounded reconnect token;
3. report the last applied server sequence and projection revision;
4. invalidate or supersede any older physical stream;
5. replay retained envelopes when the complete gap is available; or
6. emit `ResyncRequired` followed by a fresh knowledge-filtered snapshot.

Resume buffers are bounded by bytes, envelopes, and age. The server does not
retain an unbounded history for a disconnected or stalled client.

Client disconnection never pauses the match. HQ and field-unit modules continue
under their current code, orders, and communication state.

## Backpressure and slow clients

Each physical stream has one bounded outbound writer queue because concurrent
writes to one gRPC stream are not permitted.

When a client falls behind:

1. coalesce replaceable projection work before serialization where semantics
   permit;
2. preserve required session-control and input-result messages;
3. stop accumulating a long chain of obsolete deltas;
4. mark the client as requiring a fresh snapshot;
5. close and require reconnect if it cannot consume the bounded recovery; and
6. never slow or pause the authoritative simulation.

The policy is based on deterministic queue categories and public limits, not
client identity or account tier.

Network delivery of an HQ payload is distinct from the simulated delivery of
that payload to the HQ unit. The gameplay communication system owns the latter;
the stream merely carries information already available at the player/HQ
boundary.

## Heartbeats and time

Transport keepalive, session heartbeat, and simulation ticks are separate:

- HTTP/2 keepalive maintains infrastructure connections;
- a session heartbeat detects a logically stalled peer and carries sequence
  progress; and
- simulation tick is authoritative game time.

Clients never supply authoritative timestamps. Operational wall-clock
timestamps may use `google.protobuf.Timestamp` in account, audit, or service
metadata, but gameplay timing uses integer ticks and durations.

## Error model

gRPC status codes are reserved for RPC-level failures:

- unauthenticated or unauthorized call;
- incompatible endpoint or protocol major;
- malformed stream lifecycle;
- exceeded transport/message limit;
- unavailable service;
- cancelled or deadline-exceeded operation; and
- internal infrastructure fault.

Expected game outcomes remain typed protocol messages:

- illegal order;
- module assignment already locked;
- unknown entity in the client's knowledge;
- action unavailable;
- input too late;
- match already completed; or
- deployment rejected.

Stable enum-like S.I.R. reason codes drive client behavior. Human-readable text
is diagnostic and may be localized or changed without becoming a compatibility
contract.

## Compatibility policy

### Schema compatibility

Within `sir.api.v1`:

- changes are additive and wire-compatible;
- published field numbers are never changed or reused;
- removed fields and enum values are reserved;
- every enum begins with a type-prefixed `UNSPECIFIED = 0`;
- scalar presence uses `optional`, a wrapper, or a containing message where
  absence matters;
- unknown optional fields and envelope cases are handled safely;
- maps are avoided where stable ordering matters;
- repeated fields and bytes are bounded after decoding; and
- default values cannot accidentally authorize an action.

### Behavioral compatibility

Protobuf wire compatibility does not guarantee behavioral compatibility.
Server discovery and session acceptance therefore identify:

- API package major;
- protocol behavior revision;
- ruleset and content versions;
- WASM ABI and execution profile;
- required and optional capability identifiers; and
- minimum supported client capability set.

A breaking API change creates `sir.api.v2` and can coexist with v1 during a
declared migration window. Supporting an old wire version does not permit it to
access gameplay state it cannot represent safely.

## Security and privacy

- TLS is mandatory outside explicit local development.
- Authentication metadata is validated before session admission.
- Origin is not an authentication mechanism for any later browser gateway.
- Every decoded message is size-, count-, range-, and state-validated.
- Compression has decompressed-size and ratio safeguards.
- Reflection may expose the public schema because it is already public, but
  production policy can disable it to reduce operational attack surface.
- Error details, metrics, trailers, timing, and service discovery are reviewed
  for hidden-participant leakage.
- Rate and concurrency limits apply independently to unary calls, streams,
  artifacts, and decoded gameplay inputs.
- A custom client cannot request an unfiltered projection or administrative
  observer role through an ordinary player session.

## Generation and distribution

The repository should provide:

- canonical `.proto` files;
- Buf lint and breaking-change configuration;
- reproducible generation scripts with pinned tools;
- committed generated F# code for the canonical implementation;
- published schema archives or packages by API version;
- generated-client instructions for F#, C#, Rust, and one scripting language;
- a minimal headless client in at least one non-.NET language; and
- a protocol conformance suite usable without the canonical UI.

Custom clients are responsible for their UI and policy. They should not need to
implement gRPC framing, reverse-engineer F# records, or copy canonical-client
source to connect correctly.

## Validation scenarios

The first protocol spike and subsequent conformance suite cover:

- fresh connection and negotiation;
- invalid, expired, and reused admission tokens;
- duplicate and out-of-window client sequences;
- accepted and rejected inputs with stable results;
- reconnect with retained deltas;
- reconnect requiring a full snapshot;
- projection revision mismatch;
- slow-client queue overflow;
- concurrent physical-stream takeover;
- client disconnect while the match continues;
- opaque HQ message round trip;
- unknown additive fields and envelope cases;
- oversized and compression-amplified messages;
- a native non-.NET generated client;
- hidden-participant metadata inspection; and
- replay of a session against recorded server envelopes.

## Open implementation parameters

- Exact message and field definitions.
- Exact identifier byte widths and local entity-id width.
- FsGrpc versus the C# `Grpc.Tools` fallback after a generation spike.
- Authentication authority and token format.
- Resume-window byte, message, and time limits.
- Snapshot batching and ordinary projection-delivery cadence.
- Compression algorithms and thresholds.
- Exact API support and migration window.
