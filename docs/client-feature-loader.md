---
title: Client Feature Loader
category: Engineering
index: 7
description: Versioned client bootstrap, eager and deferred feature delivery.
---

# Client feature loader

The client feature registry is the versioned source of truth for when a browser
feature is present and which logical chunk supplies it. Version 2 classifies the
application shell as bootstrap and every optional workspace or supporting panel,
including the in-application Docs workspace, as deferred. A feature identity is the tuple
of registry version, feature id, and logical chunk; late completions with a
different tuple are ignored deterministically. The post-build gate compares the
complete Vite dynamic-entry inventory with those registry owners, so an
unregistered future chunk fails closed.

Deferred imports use literal module paths so Fable and Vite can construct a
stable bundle graph without runtime code generation. The production build
explicitly disables property-name mangling. This keeps the public JavaScript
identity fields (`registryVersion`, `featureId`, and `logicalChunk`) stable and
remains compatible with a restrictive script-src CSP.

## Runtime states

Each feature is in `Idle`, `Loading`, `Loaded`, or `Failed`. Failures retain a
stable category: `missing-chunk`, `offline`, `import-rejected`, or
`stale-identity`. Retrying uses the same declared identity. The visible tactical
Docs modality, View → Rules data menu item, and Editor → Environment action are
production deferred entry points.

The bootstrap shell owns documentation navigation state and manifest I/O through
`DocumentationFeatureContract`; the registered `DocsView` chunk owns rendering.
New documentation behavior extends that deferred module and its scoped budget,
so it does not grow the bootstrap shell or force one global size ceiling to cover
future features.

Version 2 preserves the version-1 per-feature ownership model while replacing
the placeholder Docs module with the real deferred `DocsView`. Its source-frozen
bundle observation also rebaselines only the Rules Explorer Brotli ceiling from
16,000 to 16,384 bytes; the other route ceilings are unchanged. Version 1 remains
tracked as the immutable prior contract instead of being rewritten in place.

## Build evidence

Every declared logical chunk has raw, gzip, and Brotli ceilings in the registry.
The post-build gate reads the Vite manifest and emitted bytes, rejects missing or
extra identities and budget overruns, and writes canonical content-addressed
receipts under `docs/evidence/client-feature-bundle-graph-v1/`. Receipt input
identity covers client source and the client build script; feedback, reports,
timestamps, host paths, and elapsed durations are deliberately excluded, so
feedback-only metadata cannot trigger a different build identity.
