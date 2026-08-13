# Awareness/reaction replay contract v1

Canonical replay state/events bind schema v1, sensor-profile identity, reaction-order identity, spatial-query profile/package identity, Game.Core compatibility/package identity, ordered inputs, facts, effects, and exact retained implementation identity. Integer fields and canonically sorted identifiers/cells/edges use the existing little-endian encoding conventions.

Current packages encode explicit awareness/reaction payloads under the new identity. Legacy packages retain their historical schema and meaning; decoders do not fabricate awareness from LOS or upgrade old engagement shortcuts. Missing, mismatched, malformed, or unavailable retained identities return typed errors before state mutation.

.NET and package-derived Fable/Node/browser consumers must emit byte-identical fixtures for current state, every transition/reason, simultaneous ordering, and seeks. Divergence diagnostics identify fixture and first differing byte; an injected identity/order/state divergence must make the owning gate red.
