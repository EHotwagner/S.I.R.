# Pre-implementation performance smoke

- Candidate base: `3dc50b5` (`origin/main` at lane creation)
- Host capability: Linux x64, .NET SDK 10.0.302, Release builds; headless only, no compositor evidence.
- Contract: `work/186-authored-tactical-parcels/contracts/environment-performance-v1.md`
- Match route: `dotnet run --project tests/SIR.Match.Tests -c Release` passed in 10.145 s total. Existing production-adjacent live integration measured 40 ticks in 27.944 ms, preview 0.213 ms, serialization 0.161 ms, worker transfer 0.028 ms, projection 0.086 ms, and exact replay digest `84c086053d423768c51f2dc7be23d495904a70fef4de6957f2c8b36ab31d4137`.
- Client route: `dotnet run --project tests/SIR.Client.Tests -c Release` passed in 14.649 s total. Existing dense 40x40 editor content (1,600 terrain, 3,120 edges, 200 units, 200 regions) measured p95 preview 2.628 ms, command 2.471 ms, document 3.239 ms, undo/redo 10.969 ms, import 9.369 ms, export 1.348 ms, and 7,136 estimated interactive nodes.
- Baseline setup note: an initial `--no-restore` attempt failed because the fresh isolated worktree had no `obj/project.assets.json`; the subsequent ordinary Release commands restored and passed. This setup failure is not a product-performance observation.

The new parcel/environment workload does not exist on the base commit. Release acceptance will compare its structural counters and host-qualified observations to the authored contract; this smoke only anchors the inherited production routes.
