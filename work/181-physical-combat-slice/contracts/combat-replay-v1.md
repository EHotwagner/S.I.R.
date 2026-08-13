# Combat replay binding v1

Canonical combat state/event/fact payloads start with an explicit schema tag and encode maps/lists in stable cell/entity/fact order. Each attack binds combat schema/profile identity, the complete `SpatialAuthorityIdentity` plus spatial schema/profile/package identity, and the complete `RulePackageIdentity` (engine, compatibility profile, package version, source commit, implementation digest, semantic digest, manifest digest).

Timeline seeking replays player inputs through the shared kernel; it never applies serialized outcomes as authority. A retained package must match every identity component exactly. Missing or mismatched retained combat/rules identity returns a typed historical-package-unavailable/malformed result and never reinterprets old bytes through current rules.

Existing replay v1/v2/v3 decode behavior remains. The new combat payload is additive and bounded by the replay limits before allocation.
