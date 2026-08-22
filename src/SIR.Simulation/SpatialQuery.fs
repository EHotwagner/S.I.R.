namespace SIR.Simulation

open System
open System.Text
open FS.GG.Game.Core
open SIR.Domain

/// <summary>Selects the permeability policy applied by a spatial query.</summary>
[<RequireQualifiedAccess>]
type SpatialModality = GroundMovement | Vision | ProjectileTrace
/// <summary>Represents requester-visible cell terrain in schema v1.</summary>
[<RequireQualifiedAccess>]
type SpatialTerrain = Open | Rough | Blocked | Unknown

/// <summary>Declares modality-specific permeability for one semantic boundary.</summary>
type SpatialPermeability =
    { Ground: bool
      Vision: bool
      Projectile: bool }

/// <summary>Associates a canonical edge with permeability and a disclosed revision token.</summary>
type SpatialBoundary =
    { Edge: Edge
      Permeability: SpatialPermeability
      RevisionToken: string }

/// <summary>Caps work, results, crossings, and footprint samples for one request.</summary>
type QueryBounds =
    { MaximumExpansions: int32
      MaximumResultCells: int32
      MaximumCrossedItems: int32
      MaximumFootprintSamples: int32 }

/// <summary>Names the movement or sensor semantics and provisional stance/height/facing inputs.</summary>
type SpatialProfile =
    { ProfileId: string
      Modality: SpatialModality
      Stance: string
      HeightBand: int32
      Facing: Direction8 }

/// <summary>Contains only battlefield facts disclosed to the requester before evaluation begins.</summary>
type ProjectedSpatialWorld =
    { Identity: SpatialAuthorityIdentity
      Minimum: Cell
      Maximum: Cell
      Terrain: Map<Cell, SpatialTerrain>
      Boundaries: SpatialBoundary list
      /// Maps each disclosed occupied cell to its opaque disclosed revision token.
      Occupancy: Map<Cell, string>
      DisclosedRevisionTokens: Set<string> }

/// <summary>Identifies each bounded spatial service in schema v1.</summary>
[<RequireQualifiedAccess>]
type SpatialQueryKind = LineTrace | ExactLineOfSight | BoundedPath | Reachability | MovementCost | Cover | Exposure

/// <summary>Supplies normalized endpoints, footprint, profile, and explicit bounds.</summary>
type SpatialQueryRequest =
    { QueryId: string
      QueryKind: SpatialQueryKind
      Origin: Cell
      Target: Cell
      Footprint: Cell list
      Profile: SpatialProfile
      Bounds: QueryBounds }

/// <summary>Reports a stable public result without leaking hidden-state causes.</summary>
[<RequireQualifiedAccess>]
type SpatialOutcome = Found | Unreachable | Exhausted | InvalidInput
/// <summary>Records private evaluation provenance excluded from canonical public bytes.</summary>
[<RequireQualifiedAccess>]
type SpatialEvaluationSource = Uncached | Cached

/// <summary>Provides renderer-neutral evidence for the authoritative decision.</summary>
type SpatialExplanation =
    { SchemaVersion: int32
      QueryId: string
      QueryKind: SpatialQueryKind
      Outcome: SpatialOutcome
      Origin: Cell
      Target: Cell
      FootprintSamples: Cell list
      CrossedCells: Cell list
      CrossedEdges: Edge list
      CoverContributors: Edge list
      ExposureDirections: Direction8 list
      Expansions: int32
      Truncated: bool
      SpatialRevision: int64
      KnowledgeIdentity: string
      KnowledgeRevision: int64
      ProfileId: string
      PackageIdentity: string
      SourceSymbol: string }

/// <summary>Returns the bounded semantic outcome and its authoritative explanation.</summary>
type SpatialQueryResult =
    { Outcome: SpatialOutcome
      Path: Cell list
      MovementCost: int32
      Visible: bool
      Explanation: SpatialExplanation }

/// <summary>Opaque cache identity; its representation is intentionally not public.</summary>
type SpatialCacheKey = private SpatialCacheKey of string
/// <summary>Names disclosed dynamic revision tokens actually depended on by evaluation.</summary>
type SpatialDependencyReceipt = { RevisionTokens: Set<string> }
/// <summary>Stores one result with its opaque key and invalidation dependencies.</summary>
type SpatialCacheEntry = { Key: SpatialCacheKey; Result: SpatialQueryResult; Dependencies: SpatialDependencyReceipt }
/// <summary>Separates immutable static geometry entries from locally invalidated dynamic entries.</summary>
type SpatialCache = { StaticEntries: SpatialCacheEntry list; DynamicEntries: SpatialCacheEntry list }

/// <summary>Evaluates, caches, invalidates, and canonically encodes spatial queries.</summary>
/// <category>Authoritative spatial queries</category>
[<RequireQualifiedAccess>]
module SpatialQuery =
    /// <summary>Current public spatial schema version.</summary>
    let schemaVersion = 1
    /// <summary>Published Game.Core compatibility profile consumed by the adapters.</summary>
    let compatibilityProfile = "fs-gg-game-core-fable-lockstep-v1"
    /// <summary>Exact package identity consumed package-only by the adapters.</summary>
    let packageIdentity = "FS.GG.Game.Core@0.13.0"

    /// <summary>Maximum accepted schema-v1 bounds.</summary>
    let defaultBounds =
        { MaximumExpansions = 4096
          MaximumResultCells = 64
          MaximumCrossedItems = 4096
          MaximumFootprintSamples = 256 }

    /// <summary>Creates an empty immutable cache.</summary>
    let emptyCache = { StaticEntries = []; DynamicEntries = [] }

    let private cellKey (cell: Cell) = cell.Row, cell.Col
    let private distinctCells cells = cells |> List.distinct |> List.sortBy cellKey
    let private addCell left right: Cell = { Col = left.Col + right.Col; Row = left.Row + right.Row }
    let private cell col row: Cell = { Col = col; Row = row }

    /// <summary>Sorts and deduplicates a non-empty footprint within its sample bound.</summary>
    let normalizeFootprint bounds footprint =
        let normalized = distinctCells footprint
        if List.isEmpty normalized then Error "Spatial footprint must contain at least one square."
        elif normalized.Length > int bounds.MaximumFootprintSamples then Error "Spatial footprint exceeds MaximumFootprintSamples."
        else Ok normalized

    let private modalityCode = function SpatialModality.GroundMovement -> 0 | SpatialModality.Vision -> 1 | SpatialModality.ProjectileTrace -> 2
    let private queryKindCode = function SpatialQueryKind.LineTrace -> 0 | SpatialQueryKind.ExactLineOfSight -> 1 | SpatialQueryKind.BoundedPath -> 2 | SpatialQueryKind.Reachability -> 3 | SpatialQueryKind.MovementCost -> 4 | SpatialQueryKind.Cover -> 5 | SpatialQueryKind.Exposure -> 6
    let private outcomeCode = function SpatialOutcome.Found -> 0 | SpatialOutcome.Unreachable -> 1 | SpatialOutcome.Exhausted -> 2 | SpatialOutcome.InvalidInput -> 3
    let private directionCode direction = Direction8.toCode direction |> int

    let private inBounds world position =
        position.Col >= world.Minimum.Col && position.Col <= world.Maximum.Col
        && position.Row >= world.Minimum.Row && position.Row <= world.Maximum.Row

    let private terrainAt world position = Map.tryFind position world.Terrain |> Option.defaultValue SpatialTerrain.Open

    let private terrainPassable modality terrain =
        match modality, terrain with
        | SpatialModality.GroundMovement, (SpatialTerrain.Blocked | SpatialTerrain.Unknown) -> false
        | (SpatialModality.Vision | SpatialModality.ProjectileTrace), SpatialTerrain.Blocked -> false
        | _ -> true

    let private permeability modality value =
        match modality with
        | SpatialModality.GroundMovement -> value.Ground
        | SpatialModality.Vision -> value.Vision
        | SpatialModality.ProjectileTrace -> value.Projectile

    /// <summary>Indexes disclosed boundaries by exact canonical edge for keyed resolution.</summary>
    ///
    /// FIRST-WINS, DELIBERATELY. The linear scan this replaces is `List.tryFind`, which answers
    /// with the FIRST matching boundary. `Map.ofList` keeps the LAST, so a world that disclosed
    /// the same edge twice would resolve differently through a map built that way. This fold
    /// reproduces `tryFind`'s answer exactly.
    ///
    /// CROSS-RUNTIME RULE FOR EVERY INDEX IN THIS MODULE: use it for `tryFind` ONLY, and never
    /// iterate it to produce output. This file is compiled for both .NET and Fable, and
    /// `scripts/verify-spatial-query.sh` requires the canonical `--print-spatial-query` bytes to
    /// be byte-identical under both runtimes. Reading a Map by key cannot move those bytes;
    /// enumerating one to build a result could. Every ordering must keep coming from the
    /// already-ordered source lists.
    let private indexBoundaries (boundaries: SpatialBoundary list) =
        (Map.empty, boundaries)
        ||> List.fold (fun index boundary ->
            if Map.containsKey boundary.Edge index then index else Map.add boundary.Edge boundary index)

    let private boundaryAt (boundaries: Map<Edge, SpatialBoundary>) left right =
        Edges.edgeBetween left right
        |> Option.bind (fun edge -> Map.tryFind edge boundaries)

    let private edgePassable observeBoundary boundaries modality left right =
        boundaryAt boundaries left right
        |> Option.forall (fun boundary ->
            observeBoundary boundary
            permeability modality boundary.Permeability)

    let private absoluteFootprint anchor footprint = footprint |> List.map (addCell anchor)

    let private anchorPassable observeCell world request footprint anchor =
        absoluteFootprint anchor footprint
        |> List.forall (fun position ->
            observeCell position
            inBounds world position
            && terrainAt world position |> terrainPassable request.Profile.Modality
            && not (Map.containsKey position world.Occupancy))

    let private orthogonalEnvelope observeBoundary boundaries modality footprint origin destination =
        footprint
        |> List.forall (fun offset -> edgePassable observeBoundary boundaries modality (addCell origin offset) (addCell destination offset))

    let private transitionPassable observeCell observeBoundary boundaries world request footprint origin destination =
        let dc = destination.Col - origin.Col
        let dr = destination.Row - origin.Row
        if (dc = 0 && dr = 0) || abs dc > 1 || abs dr > 1 then false
        elif not (anchorPassable observeCell world request footprint destination) then false
        elif dc = 0 || dr = 0 then orthogonalEnvelope observeBoundary boundaries request.Profile.Modality footprint origin destination
        else
            let horizontal = cell destination.Col origin.Row
            let vertical = cell origin.Col destination.Row
            anchorPassable observeCell world request footprint horizontal
            && anchorPassable observeCell world request footprint vertical
            && orthogonalEnvelope observeBoundary boundaries request.Profile.Modality footprint origin horizontal
            && orthogonalEnvelope observeBoundary boundaries request.Profile.Modality footprint horizontal destination
            && orthogonalEnvelope observeBoundary boundaries request.Profile.Modality footprint origin vertical
            && orthogonalEnvelope observeBoundary boundaries request.Profile.Modality footprint vertical destination

    let private neighbours observeCell observeBoundary boundaries world request footprint origin =
        [ for dr in -1 .. 1 do
              for dc in -1 .. 1 do
                  if dc <> 0 || dr <> 0 then
                      let destination = cell (origin.Col + dc) (origin.Row + dr)
                      if transitionPassable observeCell observeBoundary boundaries world request footprint origin destination then yield destination ]
        |> List.sortBy cellKey

    let private movementCost world path =
        path
        |> List.skip 1
        |> List.sumBy (fun position -> if terrainAt world position = SpatialTerrain.Rough then 2 else 1)

    /// Lexicographic comparison of two forward cell paths under `cellKey` order.
    ///
    /// This must order by (Row, Col) because the frontier tiebreaker it replaces was
    /// `path |> List.map cellKey`. F#'s structural comparison on `Cell` orders by DECLARATION
    /// order - (Col, Row) - so `compare left right` on the raw lists is NOT the same ordering
    /// and would silently change which equal-cost path wins.
    let rec private comparePathCells (left: Cell list) (right: Cell list) =
        match left, right with
        | [], [] -> 0
        | [], _ -> -1
        | _, [] -> 1
        | leftHead :: leftTail, rightHead :: rightTail ->
            let byRow = compare leftHead.Row rightHead.Row
            if byRow <> 0 then byRow
            else
                let byCol = compare leftHead.Col rightHead.Col
                if byCol <> 0 then byCol else comparePathCells leftTail rightTail

    /// Selects the frontier entry `List.sortBy` would have put at the head, in one pass.
    ///
    /// EQUIVALENCE ARGUMENT. `List.sortBy` is stable, so `List.head (List.sortBy key frontier)`
    /// is the FIRST entry attaining the minimum key - which is exactly what this fold keeps,
    /// because a later entry replaces the incumbent only on a STRICT improvement. The old
    /// `List.tail ordered` handed the next iteration a fully sorted remainder while this hands
    /// it an unsorted one, and that is safe: the remainder is re-selected from scratch every
    /// iteration, so its order can only be observed through a tie among entries that compare
    /// EQUAL on the whole key (cost, Row, Col, path) - and such entries are identical tuples,
    /// hence interchangeable.
    ///
    /// The path leg is kept rather than dropped. `best` prunes equal-cost duplicates for the
    /// same cell, which makes a full (cost, Row, Col) tie look unreachable today, but that is
    /// a property of the pruning rule rather than of this function, and it would age badly.
    /// The leg costs one `List.rev` pair only when three comparisons have already tied.
    let private selectFrontier frontier =
        match frontier with
        | [] -> None
        | head :: tail ->
            let mutable chosen = head
            let mutable others = []
            for entry in tail do
                let entryCost, entryCell, entryPath, _ = entry
                let chosenCost, chosenCell, chosenPath, _ = chosen
                let byCost = compare entryCost chosenCost
                let improves =
                    if byCost <> 0 then byCost < 0
                    else
                        let byRow = compare entryCell.Row chosenCell.Row
                        if byRow <> 0 then byRow < 0
                        else
                            let byCol = compare entryCell.Col chosenCell.Col
                            if byCol <> 0 then byCol < 0
                            else comparePathCells (List.rev entryPath) (List.rev chosenPath) < 0
                if improves then
                    others <- chosen :: others
                    chosen <- entry
                else others <- entry :: others
            Some(chosen, others)

    let private boundedPath observeCell observeBoundary boundaries world request footprint =
        let packageCandidate =
            Pathfinding.astar
                Neighbourhood.EightWay
                request.Bounds.MaximumExpansions
                (anchorPassable observeCell world request footprint)
                request.Origin
                request.Target
        let validPackageCandidate =
            packageCandidate
            |> Option.filter (fun path ->
                path.Length <= int request.Bounds.MaximumResultCells
                && (path |> List.pairwise |> List.forall (fun (origin, destination) -> transitionPassable observeCell observeBoundary boundaries world request footprint origin destination)))
        // Frontier entries carry the path REVERSED plus its length, so extending a candidate is
        // one cons instead of the `path @ [ candidate ]` copy this replaces, and the length test
        // needs no traversal. The forward path is materialized once, on the success return.
        let rec loop frontier best expansions =
            match selectFrontier frontier with
            | None -> SpatialOutcome.Unreachable, [], 0, expansions
            | Some _ when expansions >= request.Bounds.MaximumExpansions -> SpatialOutcome.Exhausted, [], 0, expansions
            | Some((cost, current, reversedPath, pathLength), rest) ->
                if current = request.Target then
                    if pathLength > int request.Bounds.MaximumResultCells then SpatialOutcome.Exhausted, [], 0, expansions
                    else SpatialOutcome.Found, List.rev reversedPath, cost, expansions
                elif Map.tryFind current best |> Option.exists (fun known -> known < cost) then
                    loop rest best expansions
                else
                    // Evaluated ONCE per expansion. The two folds below previously each rebuilt
                    // this list, duplicating every boundary and terrain probe inside it.
                    let expanded = neighbours observeCell observeBoundary boundaries world request footprint current
                    let next =
                        expanded
                        |> List.fold (fun state candidate ->
                            let candidateCost = cost + (if terrainAt world candidate = SpatialTerrain.Rough then 2 else 1)
                            match Map.tryFind candidate best with
                            | Some known when known <= candidateCost -> state
                            | _ -> (candidateCost, candidate, candidate :: reversedPath, pathLength + 1) :: state) rest
                    let nextBest =
                        expanded
                        |> List.fold (fun state candidate ->
                            let candidateCost = cost + (if terrainAt world candidate = SpatialTerrain.Rough then 2 else 1)
                            match Map.tryFind candidate state with
                            | Some known when known <= candidateCost -> state
                            | _ -> Map.add candidate candidateCost state) best
                    loop next nextBest (expansions + 1)
        if not (anchorPassable observeCell world request footprint request.Origin) || not (anchorPassable observeCell world request footprint request.Target) then
            SpatialOutcome.Unreachable, [], 0, 0
        else
            match validPackageCandidate with
            | Some path -> SpatialOutcome.Found, path, movementCost world path, int32 path.Length
            | None when packageCandidate |> Option.exists (fun path -> path.Length > int request.Bounds.MaximumResultCells) -> SpatialOutcome.Exhausted, [], 0, request.Bounds.MaximumExpansions
            | None -> loop [ 0, request.Origin, [ request.Origin ], 1 ] (Map.ofList [ request.Origin, 0 ]) 0

    let private lineStepCount origin target =
        max
            (abs (int64 target.Col - int64 origin.Col))
            (abs (int64 target.Row - int64 origin.Row))

    let private lineCells origin target =
        // observedTrace proves this delta is within MaximumCrossedItems before
        // any line cells are materialized, so bounded int32 arithmetic is safe.
        let dc = target.Col - origin.Col
        let dr = target.Row - origin.Row
        let steps = max (abs dc) (abs dr)
        if steps = 0 then [ origin ]
        else
            [ 0 .. steps ]
            |> List.map (fun index -> cell (origin.Col + dc * index / steps) (origin.Row + dr * index / steps))
            |> distinctCells

    // `cells` is the already-materialized `lineCells origin target` for this pair. It used to be
    // recomputed here, which was one of the four generations of the same line per pair.
    let private lineVisible observeBoundary boundaries world modality cells origin target =
        let transparent position = inBounds world position && terrainAt world position |> terrainPassable modality
        Los.lineOfSightBy Supercover transparent origin target
        && (cells |> List.pairwise |> List.forall (fun (left, right) -> edgePassable observeBoundary boundaries modality left right))

    let private directionFrom origin target =
        Direction8.tryFromDelta (target.Col - origin.Col) (target.Row - origin.Row)

    let private observedTrace observeCell observeBoundary boundaries (world: ProjectedSpatialWorld) (request: SpatialQueryRequest) footprint =
        let origins = absoluteFootprint request.Origin footprint
        let targets = absoluteFootprint request.Target footprint
        let maximumWork = int64 request.Bounds.MaximumCrossedItems
        if origins.Length * targets.Length > int request.Bounds.MaximumCrossedItems then false, [], [], [], [], true
        else
            let pairs = [ for origin in origins do for target in targets do yield origin, target ]
            if (pairs |> List.sumBy (fun (origin, target) -> lineStepCount origin target + 1L)) > maximumWork then
                false, [], [], [], [], true
            else
                // Each traced line is generated ONCE here and threaded to every consumer. It was
                // previously regenerated four times per pair - by the visibility filter, inside
                // `lineVisible`, for `crossedCells`, and again for `crossedEdges` - and each
                // generation ran `distinctCells`, itself a `List.distinct` plus a `List.sortBy`.
                // The work bound above is deliberately still measured with `lineStepCount`, which
                // materializes nothing, so it keeps refusing oversized work before this runs.
                let traced = pairs |> List.map (fun (origin, target) -> origin, target, lineCells origin target)
                let visiblePairs =
                    traced
                    |> List.filter (fun (origin, target, cells) ->
                        cells |> List.iter observeCell
                        lineVisible observeBoundary boundaries world request.Profile.Modality cells origin target)
                let crossedCells = traced |> List.collect (fun (_, _, cells) -> cells) |> distinctCells
                let crossedEdges =
                    traced
                    |> List.collect (fun (_, _, cells) -> cells |> List.pairwise |> List.choose (fun (left, right) -> Edges.edgeBetween left right))
                    |> List.distinct
                let cover =
                    crossedEdges
                    |> List.filter (fun edge ->
                        Map.tryFind edge boundaries
                        |> Option.exists (fun boundary -> not (permeability request.Profile.Modality boundary.Permeability)))
                let exposure =
                    visiblePairs
                    |> List.choose (fun (origin, target, _) -> directionFrom target origin)
                    |> List.distinct
                    |> List.sortBy directionCode
                not (List.isEmpty visiblePairs), crossedCells, crossedEdges, cover, exposure, false

    let private emptyExplanation (world: ProjectedSpatialWorld) (request: SpatialQueryRequest) outcome footprint : SpatialExplanation =
        { SchemaVersion = schemaVersion
          QueryId = request.QueryId
          QueryKind = request.QueryKind
          Outcome = outcome
          Origin = request.Origin
          Target = request.Target
          FootprintSamples = footprint
          CrossedCells = []
          CrossedEdges = []
          CoverContributors = []
          ExposureDirections = []
          Expansions = 0
          Truncated = false
          SpatialRevision = world.Identity.SpatialRevision
          KnowledgeIdentity = world.Identity.KnowledgeIdentity
          KnowledgeRevision = world.Identity.KnowledgeRevision
          ProfileId = request.Profile.ProfileId
          PackageIdentity = packageIdentity
          SourceSymbol = "SIR.Simulation.SpatialQuery.evaluate" }

    /// <summary>Evaluates one bounded request over an already knowledge-projected world.</summary>
    let evaluate (world: ProjectedSpatialWorld) (request: SpatialQueryRequest) =
        let validBounds =
            request.Bounds.MaximumExpansions > 0
            && request.Bounds.MaximumResultCells > 0
            && request.Bounds.MaximumCrossedItems > 0
            && request.Bounds.MaximumFootprintSamples > 0
            && request.Bounds.MaximumExpansions <= 4096
            && request.Bounds.MaximumResultCells <= 64
            && request.Bounds.MaximumCrossedItems <= 4096
            && request.Bounds.MaximumFootprintSamples <= 256
        match normalizeFootprint request.Bounds request.Footprint with
        | Error _ ->
            let explanation = emptyExplanation world request SpatialOutcome.InvalidInput []
            { Outcome = SpatialOutcome.InvalidInput; Path = []; MovementCost = 0; Visible = false; Explanation = explanation }, { RevisionTokens = Set.empty }
        | Ok footprint when not validBounds || String.IsNullOrWhiteSpace request.QueryId || String.IsNullOrWhiteSpace request.Profile.ProfileId ->
            let explanation = emptyExplanation world request SpatialOutcome.InvalidInput footprint
            { Outcome = SpatialOutcome.InvalidInput; Path = []; MovementCost = 0; Visible = false; Explanation = explanation }, { RevisionTokens = Set.empty }
        | Ok footprint ->
            let mutable dependedTokens = Set.empty
            let observeToken token =
                if Set.contains token world.DisclosedRevisionTokens then
                    dependedTokens <- Set.add token dependedTokens
            let observeCell position =
                observeToken $"occupancy:{position.Col}:{position.Row}"
                world.Occupancy |> Map.tryFind position |> Option.iter observeToken
            let observeBoundary (boundary: SpatialBoundary) = observeToken boundary.RevisionToken
            // Built ONCE per evaluation and threaded through every boundary probe below. The scan
            // it replaces ran per consecutive cell pair on every traced line and per footprint
            // offset on every path transition, so its cost was multiplied by the whole query.
            let boundaries = indexBoundaries world.Boundaries
            match request.QueryKind with
            | SpatialQueryKind.BoundedPath | SpatialQueryKind.Reachability | SpatialQueryKind.MovementCost ->
                let outcome, path, cost, expansions = boundedPath observeCell observeBoundary boundaries world request footprint
                let crossedEdges = path |> List.pairwise |> List.choose (fun (left, right) -> Edges.edgeBetween left right)
                let crossedCells = path |> List.truncate (int request.Bounds.MaximumCrossedItems)
                let explanation =
                    { emptyExplanation world request outcome footprint with
                        CrossedCells = crossedCells
                        CrossedEdges = crossedEdges |> List.truncate (int request.Bounds.MaximumCrossedItems)
                        Expansions = expansions
                        Truncated = outcome = SpatialOutcome.Exhausted }
                let result = { Outcome = outcome; Path = path; MovementCost = cost; Visible = false; Explanation = explanation }
                result, { RevisionTokens = dependedTokens }
            | SpatialQueryKind.LineTrace | SpatialQueryKind.ExactLineOfSight | SpatialQueryKind.Cover | SpatialQueryKind.Exposure ->
                let visible, cells, edges, cover, exposure, truncated = observedTrace observeCell observeBoundary boundaries world request footprint
                let outcome = if truncated then SpatialOutcome.Exhausted elif visible then SpatialOutcome.Found else SpatialOutcome.Unreachable
                let explanation =
                    { emptyExplanation world request outcome footprint with
                        CrossedCells = cells
                        CrossedEdges = edges
                        CoverContributors = cover
                        ExposureDirections = exposure
                        Expansions = int32 cells.Length
                        Truncated = truncated }
                // Every truncated branch above returns visible=false.
                let result = { Outcome = outcome; Path = []; MovementCost = 0; Visible = visible; Explanation = explanation }
                result, { RevisionTokens = dependedTokens }

    let private escapeJson (value: string) = value.Replace("\\", "\\\\").Replace("\"", "\\\"")
    let private cellJson position = $"{{\"col\":{position.Col},\"row\":{position.Row}}}"
    let private edgeJson edge = $"{{\"lo\":{cellJson edge.Lo},\"hi\":{cellJson edge.Hi}}}"
    let private arrayJson render values = values |> List.map render |> String.concat "," |> fun body -> "[" + body + "]"

    /// <summary>Encodes the complete public result as deterministic schema-v1 JSON.</summary>
    let canonicalPublicJson result =
        let explanation = result.Explanation
        let visible = if result.Visible then "true" else "false"
        let truncated = if explanation.Truncated then "true" else "false"
        let footprintJson = arrayJson cellJson explanation.FootprintSamples
        let pathJson = arrayJson cellJson result.Path
        let crossedCellsJson = arrayJson cellJson explanation.CrossedCells
        let crossedEdgesJson = arrayJson edgeJson explanation.CrossedEdges
        let coverJson = arrayJson edgeJson explanation.CoverContributors
        let exposureJson = arrayJson (directionCode >> string) explanation.ExposureDirections
        $"{{\"schemaVersion\":{explanation.SchemaVersion},\"queryId\":\"{escapeJson explanation.QueryId}\",\"queryKind\":{queryKindCode explanation.QueryKind},\"outcome\":{outcomeCode result.Outcome},\"origin\":{cellJson explanation.Origin},\"target\":{cellJson explanation.Target},\"footprint\":{footprintJson},\"path\":{pathJson},\"movementCost\":{result.MovementCost},\"visible\":{visible},\"crossedCells\":{crossedCellsJson},\"crossedEdges\":{crossedEdgesJson},\"coverContributors\":{coverJson},\"exposureDirections\":{exposureJson},\"expansions\":{explanation.Expansions},\"truncated\":{truncated},\"spatialRevision\":{explanation.SpatialRevision},\"knowledgeIdentity\":\"{escapeJson explanation.KnowledgeIdentity}\",\"knowledgeRevision\":{explanation.KnowledgeRevision},\"profileId\":\"{escapeJson explanation.ProfileId}\",\"compatibilityProfile\":\"{compatibilityProfile}\",\"packageIdentity\":\"{packageIdentity}\",\"sourceSymbol\":\"{explanation.SourceSymbol}\"}}"

    /// <summary>Returns UTF-8 bytes of the deterministic public JSON payload.</summary>
    let canonicalResultBytes result = canonicalPublicJson result |> Encoding.UTF8.GetBytes

    let private cacheKey world request =
        let footprint = request.Footprint |> distinctCells |> List.map (fun value -> $"{value.Col}:{value.Row}") |> String.concat ";"
        SpatialCacheKey($"{world.Identity.MapIdentity}|{world.Identity.RulesetIdentity}|{world.Identity.SpatialRevision}|{world.Identity.KnowledgeIdentity}|{world.Identity.KnowledgeRevision}|{request.QueryId}|{queryKindCode request.QueryKind}|{modalityCode request.Profile.Modality}|{request.Profile.ProfileId}|{request.Profile.Stance}|{request.Profile.HeightBand}|{directionCode request.Profile.Facing}|{request.Origin.Col}:{request.Origin.Row}|{request.Target.Col}:{request.Target.Row}|{footprint}|{request.Bounds.MaximumExpansions}:{request.Bounds.MaximumResultCells}:{request.Bounds.MaximumCrossedItems}:{request.Bounds.MaximumFootprintSamples}")

    /// <summary>Maximum retained dynamic cache entries; older entries are evicted past it.</summary>
    ///
    /// WHY THIS VALUE. The largest dynamic working set any declared fixture or budget in this
    /// repository exercises is the 256-receipt local-invalidation fixture behind
    /// `docs/performance-budget.md`'s 10 ms local-invalidation target. 1024 sits four times above
    /// that, so no exercised workload reaches it and no existing outcome moves - while still being
    /// a ceiling a long-lived session genuinely hits, because dynamic entries are keyed by spatial
    /// and knowledge revision and therefore turn over every time the world changes.
    ///
    /// It is deliberately NOT larger. `SpatialCache` exposes both tiers as lists, so a lookup is a
    /// linear scan; a ceiling far above the working set would leave the cache costing more per
    /// lookup than the evaluation it exists to avoid, which is the F3 defect rather than a fix for
    /// it. 1024 is the point where the linear probe is still cheaper than re-evaluating.
    let private dynamicCacheCapacity = 1024

    /// True when `entries` is longer than `capacity`, walking at most `capacity` links and
    /// allocating nothing. `List.length` would traverse the whole tier on every insertion.
    let rec private exceedsCapacity capacity entries =
        match entries with
        | [] -> false
        | _ :: rest -> if capacity <= 0 then true else exceedsCapacity (capacity - 1) rest

    /// <summary>Returns a byte-equivalent cached result or stores a newly evaluated entry.</summary>
    let evaluateCached cache world request =
        let key = cacheKey world request
        // Probe the dynamic tier, then the static tier. This replaces
        // `cache.DynamicEntries @ cache.StaticEntries`, which COPIED the entire dynamic tier on
        // every lookup - including on a hit - so lookup allocation grew with cache size. Probing
        // dynamic first preserves the precedence the concatenated list already had.
        let hit =
            match cache.DynamicEntries |> List.tryFind (fun entry -> entry.Key = key) with
            | Some entry -> Some entry
            | None -> cache.StaticEntries |> List.tryFind (fun entry -> entry.Key = key)
        match hit with
        | Some entry -> entry.Result, cache, SpatialEvaluationSource.Cached
        | None ->
            let result, dependencies = evaluate world request
            let entry = { Key = key; Result = result; Dependencies = dependencies }
            let next =
                if Set.isEmpty dependencies.RevisionTokens
                then { cache with StaticEntries = entry :: cache.StaticEntries }
                else
                    // Newest-first, so truncation drops the OLDEST entries. Only the dynamic tier
                    // is bounded: static entries carry no dependency tokens, are never invalidated,
                    // and are the geometry results the cache exists to keep.
                    let grown = entry :: cache.DynamicEntries
                    let bounded =
                        if exceedsCapacity dynamicCacheCapacity grown
                        then List.truncate dynamicCacheCapacity grown
                        else grown
                    { cache with DynamicEntries = bounded }
            result, next, SpatialEvaluationSource.Uncached

    /// <summary>Removes only dynamic entries whose disclosed dependency tokens changed.</summary>
    let invalidate changedTokens cache =
        let keep entry = Set.intersect changedTokens entry.Dependencies.RevisionTokens |> Set.isEmpty
        { cache with DynamicEntries = cache.DynamicEntries |> List.filter keep }

    /// <summary>Exposes the profiled package A* primitive through a bounded point-footprint adapter.</summary>
    let packagePointPath maximumExpansions walkable origin target =
        Pathfinding.astar Neighbourhood.FourWay maximumExpansions walkable origin target
