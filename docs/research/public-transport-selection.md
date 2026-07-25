---
title: S.I.R. Public Transport Selection
status: accepted
decision-status: canonical
document-type: research-and-options
version: "0.2"
last-updated: 2026-07-25
related:
  - docs/technology-stack.md
  - docs/codebase-architecture.md
  - docs/wasm-control-architecture.md
  - docs/skirmish-development-plan.md
---

# S.I.R. Public Transport Selection

## Current direction

Native gRPC over HTTP/2 is the canonical first public transport.

The primary reason is not latency. S.I.R.'s server-resident WASM control makes
network latency less decisive for unit micro-control, while gRPC offers strong
schema, tooling, authentication, streaming, and generated-client support for
custom clients.

The decision must distinguish:

- the **S.I.R. protocol**, whose messages, sequencing, knowledge rules, and
  compatibility are canonical; and
- **gRPC transport**, which carries that protocol but does not define gameplay
  semantics.

## Advantages of gRPC

### Custom-client development

A canonical `.proto` contract can generate clients for many languages. This
gives a custom-client developer:

- typed messages and service methods;
- stable numeric field identities;
- standard unary and streaming models;
- standard status, cancellation, deadlines, metadata, TLS, and interceptors;
- mature debugging and reflection tools; and
- less custom framing and connection code.

This advantage depends on contract-first publication. F# code-first records are
convenient for the canonical implementation but are not, by themselves, a
language-neutral public specification.

### Suitable live-session model

A native gRPC bidirectional stream fits the live match:

```protobuf
rpc OpenSession(stream ClientEnvelope) returns (stream ServerEnvelope);
```

`ClientEnvelope` can carry:

- handshake and capability negotiation;
- client sequence and acknowledgement state;
- opaque player-to-HQ payloads;
- module, force, deployment, and ready decisions when permitted;
- resynchronization requests; and
- connection health messages.

`ServerEnvelope` can carry:

- accepted protocol and ruleset versions;
- server sequence and acknowledgement state;
- knowledge-filtered snapshots and deltas;
- HQ-to-client opaque payloads;
- mission and objective events;
- recoverable errors and resynchronization instructions; and
- completion and result records.

One writer queue per stream preserves send legality and application ordering.
The match kernel remains independent of stream scheduling.

### Unified service surface

The same public API can use conventional gRPC methods for:

- capability and version discovery;
- catalog and ruleset retrieval;
- account and company queries;
- module upload, validation, and selection;
- force and match setup;
- replay and result retrieval; and
- the bidirectional live-match session.

Large artifact and replay transfers can stream independently instead of sharing
the latency-sensitive live-session queue.

### Performance is sufficient

At 20 authoritative ticks per second, gRPC framing is not a meaningful concern
if the server batches each client's applicable projection changes into bounded
tick or event envelopes. S.I.R. should avoid one remote call per entity,
observation, or gameplay effect.

The client does not wait for a network round trip before every field-unit
action. The server assigns accepted client input to an eligible tick and the
HQ/unit WASM hierarchy continues operating when the client is delayed or
disconnected.

## Disadvantages and risks

### Browser clients

Browsers cannot directly use full native HTTP/2 gRPC. gRPC-Web does not support
client streaming or bidirectional streaming in browser clients. A browser
client would therefore require:

- a WebSocket gateway;
- split unary/client-message and server-streaming gRPC-Web calls;
- Connect or another browser-oriented protocol layer; or
- a separately maintained browser transport profile.

If first-class browser clients become a near-term requirement, WebSocket is the
more universal live-session baseline. If native desktop, CLI, bot, and server
clients are the initial target, this limitation is acceptable and a gateway
can be added later.

### Streaming still needs application semantics

gRPC does not solve:

- reconnection and resume;
- duplicate input detection;
- server-assigned target ticks;
- knowledge-filtered projections;
- bounded outbound queues;
- slow-consumer policy;
- snapshot and delta recovery;
- replay identity;
- deterministic message application; or
- hiding participant metadata.

These remain explicit S.I.R. protocol rules. HTTP/2 flow control is not a
substitute for bounded game-level backpressure.

### Contract evolution

Public Protobuf schemas require discipline:

- never reuse a field number;
- reserve removed fields and enum values;
- distinguish absent values from default scalar values;
- bound all repeated fields and byte strings at validation;
- avoid exposing internal F# unions or storage layouts directly;
- version behavioral contracts that Protobuf compatibility cannot express; and
- test compatibility combinations promised by policy.

`google.protobuf.Any` should not become an escape hatch for ordinary protocol
evolution. Stable `oneof` envelope cases and explicit opaque payload fields are
more auditable.

### Operational complexity

Native gRPC requires HTTP/2-aware hosting, TLS, proxies, observability, and
keepalive configuration. Long-lived streams must be drained during deployment
and bounded against abusive or stalled clients.

Generated code adds a build step, and every officially documented client
language needs reproducible generation instructions or published artifacts.

### Latency still matters at command level

Server-side WASM removes the need for continuous low-latency unit micro-control,
but latency still affects:

- what the human sees and when;
- commander-to-HQ messages;
- reaction to new strategic information;
- deployment and ready transitions;
- negotiation with encountered players; and
- reconnect and resynchronization time.

The architecture should tolerate latency, not ignore it. Projections remain
incremental and compact, and player inputs receive prompt transport-level and
authoritative acceptance feedback.

## Comparison with raw Protobuf over WebSocket

| Concern | Native gRPC | Protobuf over WebSocket |
|---|---|---|
| Native custom clients | Excellent generated tooling | Requires S.I.R. channel SDK or custom implementation |
| Browser live client | Poor without a gateway | Excellent |
| Bidirectional native streaming | Built in | Built in at transport level |
| Schema | Canonical `.proto` and generated services | Canonical `.proto`, but service semantics are custom |
| Framing and statuses | Standard | S.I.R. must define them |
| Reconnect/resume | S.I.R.-defined | S.I.R.-defined |
| Backpressure policy | S.I.R.-defined over HTTP/2 flow control | S.I.R.-defined over socket flow control |
| Infrastructure | HTTP/2-aware | Broad proxy support but long-lived socket handling still required |
| FS.GG.Net role | Thin channel lifecycle; ASP.NET Core owns server dispatch | Symmetric `ITransport`, fragment reassembly, and Kestrel acceptor |

Both use TCP, so both inherit transport-level head-of-line blocking after packet
loss. Neither is a substitute for future datagram or QUIC work if measurements
ever demonstrate that need.

## Canonical profile

The initial public transport uses native gRPC, while browser support is deferred
to a later gateway or transport profile:

1. Publish contract-first `.proto` files under `schemas/protocol/`.
2. Use one bidirectional `OpenSession` stream for a connected participant's
   live match.
3. Use unary or dedicated streaming methods for discovery, artifacts,
   catalogs, match setup, and replay transfer.
4. Define one bounded client envelope and one bounded server envelope with
   explicit `oneof` payload cases.
5. Give every session message protocol version, session identity, sequence, and
   acknowledgement semantics where applicable.
6. Keep the server's authoritative input journal and projection queue behind a
   transport-independent session port.
7. Provide a headless reference client and generated-client quickstarts for at
   least F#, another .NET language, and one non-.NET language.
8. Defer a WebSocket/browser gateway until there is a real browser-client
   requirement.

FS.GG.Net.Grpc supplies the canonical client's channel lifecycle bridge.
ASP.NET Core and the selected Protobuf/gRPC packages own service hosting and
method dispatch. S.I.R. owns the schemas and all application-level stream
semantics.

No FS.GG.Net change is currently required.

## Acceptance criteria

Before releasing the first public protocol, a transport spike should prove:

- bidirectional session streaming through Kestrel;
- F# server and client generation from the canonical `.proto`;
- one non-.NET generated headless client;
- authentication metadata and reconnect with a new stream;
- sequence, duplicate, acknowledgement, and resume behavior;
- bounded slow-client queues and forced resynchronization;
- snapshot plus tick/event delta delivery;
- opaque client-to-HQ and HQ-to-client payload transport;
- independent large module upload and replay download;
- knowledge filtering before serialization;
- no participant-count leakage through discovery or match metadata; and
- measured bandwidth and CPU at 100 units per side and 20 simulation ticks per
  second.

## Primary sources

- [FS.GG.Net repository and transport boundaries](https://github.com/FS-GG/FS.GG.Net)
- [Using gRPC from browser applications](https://learn.microsoft.com/en-us/aspnet/core/grpc/browser?view=aspnetcore-10.0)
- [gRPC-Web streaming limitations](https://learn.microsoft.com/en-us/aspnet/core/grpc/grpcweb?view=aspnetcore-10.0)
- [ASP.NET Core WebSocket support and security](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/websockets?view=aspnetcore-10.0)
- [ASP.NET Core gRPC performance guidance](https://learn.microsoft.com/en-us/aspnet/core/grpc/performance?view=aspnetcore-10.0)
