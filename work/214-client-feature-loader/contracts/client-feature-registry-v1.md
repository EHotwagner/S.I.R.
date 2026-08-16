# Client feature registry v1

The source registry is `src/SIR.Client.Web/feature-registry.v1.json`. It is the
authoritative machine contract for client feature delivery; F# feature constants
and Vite output are validated projections.

Each feature entry contains, in stable ID order:

- `id`: lowercase kebab-case stable identity;
- `phase`: `bootstrap`, `eager`, or `deferred`;
- `control`: stable production control identity;
- `route`: stable initial/user route identity;
- `module`: repository-relative source/import identity;
- `logicalChunk`: stable chunk identity independent of the content hash;
- `budget`: non-negative raw, gzip, and Brotli byte ceilings.

Registry schema/version changes are explicit. Duplicate IDs/chunks, unknown
fields/phases, missing budgets, computed/external deferred imports, and F# or
build projections that disagree with the registry fail closed. Increasing a
budget requires a registry version/rebaseline and a red budget mutation receipt.

Version 1 entries are `shell`/bootstrap, `tactical-environment`/eager,
`delivery-support`/deferred, `rules-explorer`/deferred, and `docs`/deferred.
Delivery support is included because its production control owns a real Vite
dynamic entry; every emitted dynamic identity must have exactly one registry
owner.
