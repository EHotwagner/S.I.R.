namespace SIR.Conformance

open FS.GG.Game.Core
open SIR.Domain
open SIR.Simulation

[<RequireQualifiedAccess>]
module SpatialQueryFixtures =
    let private require condition message = if not condition then failwith message
    let private cell col row: Cell = { Col = col; Row = row }
    let private identity revision knowledgeRevision =
        SpatialAuthorityIdentity.create "fixture-map" "fixture-rules" revision "red-known" knowledgeRevision
        |> Result.defaultWith failwith
    let private edge left right = Edges.edgeBetween left right |> Option.defaultWith (fun () -> failwith "Fixture edge must be orthogonal.")
    let private boundary left right ground vision projectile token =
        { Edge = edge left right
          Permeability = { Ground = ground; Vision = vision; Projectile = projectile }
          RevisionToken = token }
    let private world revision knowledgeRevision boundaries occupancy disclosed =
        { Identity = identity revision knowledgeRevision
          Minimum = cell 0 0
          Maximum = cell 7 7
          Terrain = Map.ofList [ cell 3 3, SpatialTerrain.Rough ]
          Boundaries = boundaries
          Occupancy = occupancy
          DisclosedRevisionTokens = disclosed }
    let private profile modality =
        { ProfileId = "infantry-v1"
          Modality = modality
          Stance = "standing"
          HeightBand = 1
          Facing = East }
    let private request id kind modality origin target footprint bounds =
        { QueryId = id
          QueryKind = kind
          Origin = origin
          Target = target
          Footprint = footprint
          Profile = profile modality
          Bounds = bounds }

    let evaluate injectDivergence =
        let bounds = SpatialQuery.defaultBounds
        let footprint = [ cell 0 0; cell 0 1 ]
        let blocker = boundary (cell 1 1) (cell 2 1) false false false "door:1:1:east"
        let blockedWorld = world 7L 3L [ blocker ] Map.empty (Set.ofList [ blocker.RevisionToken ])
        let diagonal = request "multi-cell-diagonal" SpatialQueryKind.BoundedPath SpatialModality.GroundMovement (cell 0 0) (cell 2 2) footprint bounds
        let blocked, dependencies = SpatialQuery.evaluate blockedWorld diagonal
        require (blocked.Outcome = SpatialOutcome.Found) "Bounded path should route around a blocked footprint edge."
        require (blocked.Path <> [ cell 0 0; cell 1 1; cell 2 2 ]) "A multi-cell diagonal cut through the blocked transition envelope."
        require (blocked.Explanation.Expansions <= bounds.MaximumExpansions) "Path exceeded its expansion bound."

        let openWorld = world 7L 3L [] Map.empty Set.empty
        let equalCost = request "equal-cost" SpatialQueryKind.BoundedPath SpatialModality.GroundMovement (cell 0 0) (cell 2 1) [ cell 0 0 ] bounds
        let pathResult, _ = SpatialQuery.evaluate openWorld equalCost
        require (pathResult.Outcome = SpatialOutcome.Found) "Equal-cost path was not found."
        require (pathResult.Path.Head = cell 0 0 && List.last pathResult.Path = cell 2 1) "Path endpoints changed."

        let losRequest = request "footprint-los" SpatialQueryKind.ExactLineOfSight SpatialModality.Vision (cell 0 0) (cell 4 0) footprint bounds
        let losResult, _ = SpatialQuery.evaluate openWorld losRequest
        require losResult.Visible "Open footprint LOS was not visible."
        require (losResult.Explanation.FootprintSamples = footprint) "LOS explanation omitted a footprint sample."
        require (losResult.Explanation.ExposureDirections.Length > 0) "LOS explanation omitted exposure direction."

        let uncached, cache1, source1 = SpatialQuery.evaluateCached SpatialQuery.emptyCache openWorld equalCost
        let cached, cache2, source2 = SpatialQuery.evaluateCached cache1 openWorld equalCost
        require (source1 = SpatialEvaluationSource.Uncached && source2 = SpatialEvaluationSource.Cached) "Cache source classification did not transition."
        require (SpatialQuery.canonicalResultBytes uncached = SpatialQuery.canonicalResultBytes cached) "Cache hit changed public canonical bytes."
        require (cache1 = cache2) "Cache hit mutated cache state."

        let disclosedOccupancy = Map.ofList [ cell 1 0, "known-unit" ]
        let dynamicWorld = world 7L 3L [] disclosedOccupancy (Set.ofList [ "occupancy:1:0" ])
        let dynamicResult, dynamicCache, _ = SpatialQuery.evaluateCached SpatialQuery.emptyCache dynamicWorld equalCost
        let invalidated = SpatialQuery.invalidate (Set.ofList [ "occupancy:1:0" ]) dynamicCache
        require (invalidated.DynamicEntries.Length < dynamicCache.DynamicEntries.Length || dynamicCache.DynamicEntries.IsEmpty) "Dependent dynamic cache entry survived invalidation."
        require (SpatialQuery.invalidate (Set.ofList [ "unrelated" ]) dynamicCache = dynamicCache) "Unrelated revision invalidated a cache entry."

        let hiddenA = world 9L 4L [] Map.empty Set.empty
        let hiddenB = world 9L 4L [] Map.empty Set.empty
        let publicA, _ = SpatialQuery.evaluate hiddenA losRequest
        let publicB, _ = SpatialQuery.evaluate hiddenB losRequest
        require (SpatialQuery.canonicalResultBytes publicA = SpatialQuery.canonicalResultBytes publicB) "Knowledge-indistinguishable worlds emitted different public bytes."

        let exhaustedRequest = request "bounded-exhaustion" SpatialQueryKind.BoundedPath SpatialModality.GroundMovement (cell 0 0) (cell 7 7) [ cell 0 0 ] { bounds with MaximumExpansions = 1 }
        let exhausted, _ = SpatialQuery.evaluate openWorld exhaustedRequest
        require (exhausted.Outcome = SpatialOutcome.Exhausted) "Expansion exhaustion was not typed."

        let packagePath = SpatialQuery.packagePointPath 16 (fun position -> position.Row = 0 && position.Col >= 0 && position.Col <= 2) (cell 0 0) (cell 2 0)
        require (packagePath = Some [ cell 0 0; cell 1 0; cell 2 0 ]) "Package Pathfinding.astar adapter changed."

        let canonical =
            [ SpatialQuery.canonicalResultBytes blocked
              SpatialQuery.canonicalResultBytes pathResult
              SpatialQuery.canonicalResultBytes losResult
              SpatialQuery.canonicalResultBytes dynamicResult
              SpatialQuery.canonicalResultBytes exhausted ]
            |> CanonicalEncoding.concatenate
        if injectDivergence then
            let changed = Array.copy canonical
            changed[0] <- changed[0] ^^^ 1uy
            changed
        else canonical

#if !FABLE_COMPILER
    let performanceWorkload () =
        let bounds = SpatialQuery.defaultBounds
        let identity = identity 101L 17L
        let maximumWorld =
            { Identity = identity
              Minimum = cell 0 0
              Maximum = cell 79 79
              Terrain = Map.empty
              Boundaries = []
              Occupancy = Map.empty
              DisclosedRevisionTokens = Set.empty }
        let requestFor id kind target =
            request id kind SpatialModality.GroundMovement (cell 0 0) target [ cell 0 0 ] bounds
        let measure action =
            let stopwatch = System.Diagnostics.Stopwatch.StartNew()
            let result = action ()
            stopwatch.Stop()
            result, stopwatch.ElapsedMilliseconds
        // Warm JIT/runtime paths before measuring the exact candidate workload.
        SpatialQuery.evaluate maximumWorld (request "warm-los" SpatialQueryKind.ExactLineOfSight SpatialModality.Vision (cell 0 0) (cell 79 79) [ cell 0 0 ] bounds) |> ignore
        SpatialQuery.evaluate maximumWorld (requestFor "warm-route" SpatialQueryKind.BoundedPath (cell 40 40)) |> ignore
        let los, losMs = measure (fun () -> SpatialQuery.evaluate maximumWorld (request "selected-los" SpatialQueryKind.ExactLineOfSight SpatialModality.Vision (cell 0 0) (cell 79 79) [ cell 0 0 ] bounds) |> fst)
        let route, routeMs = measure (fun () -> SpatialQuery.evaluate maximumWorld (requestFor "route-preview" SpatialQueryKind.BoundedPath (cell 40 40)) |> fst)
        let _, cache, _ = SpatialQuery.evaluateCached SpatialQuery.emptyCache maximumWorld (requestFor "invalidation" SpatialQueryKind.BoundedPath (cell 20 20))
        let _, invalidationMs = measure (fun () -> SpatialQuery.invalidate (Set.ofList [ "unrelated-token" ]) cache)
        let demand count =
            measure (fun () ->
                [ 1 .. count ]
                |> List.sumBy (fun index ->
                    let target = cell (index % 32) ((index * 7) % 32)
                    let result, _ = SpatialQuery.evaluate maximumWorld (requestFor ("demand-" + string index) SpatialQueryKind.Reachability target)
                    result.Explanation.Expansions))
        let demand100, demand100Ms = demand 100
        let demand200, demand200Ms = demand 200
        los, losMs, route, routeMs, invalidationMs, demand100, demand100Ms, demand200, demand200Ms
#endif
