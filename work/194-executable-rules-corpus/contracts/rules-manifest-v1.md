# Rules manifest v1 contract

`rules-manifest-v1` is a generated, deterministic envelope over one S.I.R. rules registry.

- Canonical strings are UTF-8 preceded by an unsigned little-endian 32-bit byte count; lists are preceded by a count and sorted by their declared stable identity where source order is not semantic.
- Canonical numbers are signed little-endian integers. Authoritative fractional values are Q4 raw `int32`; units and value kinds are stable strings. JSON is a readable projection of these values, not its own hash input.
- `implementationDigest` is SHA-256 over the profile/toolchain identity and sorted `(artifact name, artifact SHA-256)` pairs for the declared .NET and Fable runtime artifacts.
- `semanticDigest` is SHA-256 over executable facts, expression/transition canonical forms, algorithm contracts/fingerprints, and `implementationDigest`.
- `manifestDigest` is SHA-256 over the complete manifest payload plus `implementationDigest` and `semanticDigest`; its own field and volatile time/absolute-path data are omitted.
- The envelope carries schema version, engine/profile/package/source identities, the three digests, rules, value kinds/units, explanation vocabulary, coverage, and presentation metadata.
- Duplicate/dangling IDs, unresolved executable registrations, invalid units/statuses, non-finite or unsupported values, unordered emission, uncovered algorithm artifacts, and any digest cycle are errors.

Changing documentation metadata alone may change only `manifestDigest`; changing inspectable semantics changes `semanticDigest` and `manifestDigest`; changing a registered runtime artifact changes all three identities.
