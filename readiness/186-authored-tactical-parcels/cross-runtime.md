# Tactical environment cross-runtime receipt

The production Fable build (`npm run build:client`) transpiled the same
Domain/Simulation sources consumed by .NET and Vite published the production
client. A Node ES-module probe invoked the generated
`TacticalEnvironment_exteriorParcelSet`, `TacticalEnvironment_assemble`, and
`TacticalEnvironment_canonicalBytes` exports for seed `0x186` and compared the
complete byte stream, not only a summary hash, with the native Match gate.

The selection adapter is product-owned SHA-256 addressed randomness over the
seed and stable slot id. This is intentional: the exact restored package
profile classifies Game.Core sequential `Rng` as `DotNetOnly`; the product does
not claim that unavailable surface as Fable authority. The shared assembly
still consumes only package `LockstepExact` cell/edge surfaces at the spatial
boundary.

The exact comparison passed over all 982 canonical bytes. Both runtimes emitted
content identity
`44db01bc52ed2250b728786e258a133cfcb550a56e1bc5e43c6d6c09674fa466`.
The candidate commit is recorded after the last candidate build so this receipt
cannot accidentally bless stale generated JavaScript.
