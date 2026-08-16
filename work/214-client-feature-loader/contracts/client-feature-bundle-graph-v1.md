# Client feature bundle graph receipt v1

The post-build validator emits canonical UTF-8 JSON plus one LF under
`docs/evidence/client-feature-bundle-graph-v1/<sha256>.json`, where the
filename digest is SHA-256 of those exact bytes.

The payload contains schema, registry version/digest, build-input digest, sorted
features, phase/route/logical-chunk identity, sorted emitted files/import edges,
raw/gzip/Brotli bytes, budget ceilings, and SHA-256 content digests. Object keys
and arrays use declared lexical ordering.

Timestamps, elapsed durations, host names, absolute paths, working-tree state,
and feedback/report metadata are excluded. Re-running over the same frozen
artifact and inputs must produce the same bytes and path. A missing file,
unregistered/eager edge, stale logical identity, digest mismatch, malformed
manifest, or budget excess fails without writing a passing receipt.
