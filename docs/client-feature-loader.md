---
title: Client Feature Loader
category: Engineering
index: 7
description: Versioned client bootstrap, eager and deferred feature delivery.
---

# Client feature loader

The client feature registry is the versioned source of truth for when a browser
feature is present and which logical chunk supplies it. Version 1 classifies the
application shell as bootstrap, Tactical Environment as eager, and Rules
Explorer and Docs as deferred. A feature identity is the tuple of registry
version, feature id, and logical chunk; late completions with a different tuple
are ignored deterministically.

Deferred imports use literal module paths so Fable and Vite can construct a
stable bundle graph without runtime code generation. The production build
explicitly disables property-name mangling. This keeps the public JavaScript
identity fields (`registryVersion`, `featureId`, and `logicalChunk`) stable and
remains compatible with a restrictive script-src CSP.

## Runtime states

Each feature is in `Idle`, `Loading`, `Loaded`, or `Failed`. Failures retain a
stable category: `missing-chunk`, `offline`, `import-rejected`, or
`stale-identity`. Retrying uses the same declared identity. The visible Docs
toolbar control and View → Rules data menu item are the production deferred
entry points; Editor → Environment proves the eager feature remains available
without a chunk request.

## Build evidence

Every declared logical chunk has raw, gzip, and Brotli ceilings in the registry.
The post-build gate reads the Vite manifest and emitted bytes, rejects missing or
extra identities and budget overruns, and writes canonical content-addressed
receipts under `docs/evidence/client-feature-bundle-graph-v1/`. Receipt input
identity covers client source and the client build script; feedback, reports,
timestamps, host paths, and elapsed durations are deliberately excluded, so
feedback-only metadata cannot trigger a different build identity.
