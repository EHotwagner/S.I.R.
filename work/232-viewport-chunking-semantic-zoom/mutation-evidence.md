# Focused mutation evidence

- Subject: `ViewportOverscanCells` in `TacticalSceneProjection.fs`.
- Mutation: changed the declared `2.0` cell overscan to `20.0`.
- Command: `dotnet run -c Release --project tests/SIR.Client.Tests/SIR.Client.Tests.fsproj --no-restore`.
- Observed result: exit 134; the focused equal-small-viewport structural-budget assertion failed at `TacticalSceneProjectionQualification.fs`.
- Restoration: restored `ViewportOverscanCells = 2.0`; the production value is not mutated in the delivered head.

An exploratory chunk-size mutation (`8.0` to `16.0`) remained green and is not claimed as satisfying mutation evidence; it showed that the acceptance gate correctly targets bounded visible work rather than a particular implementation constant.
