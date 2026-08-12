# Spatial query schema v1
This contract is the authoritative semantic boundary for issue #180. The F# signature is normative; canonical fixtures are its executable projection.

## Normalization and ordering

- A footprint is a non-empty, sorted, duplicate-free set of square-cell offsets relative to a top-left anchor, capped at 256 samples.
- Canonical cell order is row then column. Canonical paths compare total cost, cell row, cell column, then predecessor canonical bytes.
- A move transition validates every destination cell and every swept orthogonal boundary for every footprint sample. A diagonal validates both orthogonal decompositions and fails when either envelope is blocked.
- `GroundMovement`, `Vision`, and `ProjectileTrace` apply distinct terrain and semantic-edge permeability declared in the projected world.

## Requests and bounded outcomes

Schema v1 supports line trace, exact LOS, bounded path, reachability, movement cost, cover contributors, and exposure directions. Requests carry immutable map/ruleset identity, spatial revision, profiles, normalized footprint/endpoints, requester knowledge, and explicit bounds.

Path outcomes are exactly `Found`, `Unreachable`, `Exhausted`, or `InvalidInput`. A request may inspect at most 4,096 expansions, return 64 route cells, report 4,096 crossed cells/edges, sample 256 footprint cells, and encode 65,536 explanation bytes.

## Canonical encoding

Canonical bytes are schema-versioned and length-prefixed. All sets/maps are sorted by the order above. Results include request identity, normalized inputs, public outcome, ordered route/crossings/contributors, cost, expansion/result counts, and renderer-neutral explanation nodes. Private cache keys, buckets, dependency sets, timings, and hit counters are excluded.
