namespace SIR.Simulation

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

/// <summary>Names the movement or sensor semantics and provisional stance, height, and facing inputs.</summary>
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

/// <summary>Identifies an immutable normalized request within one authority and knowledge revision.</summary>
type SpatialCacheKey
/// <summary>Lists disclosed revision tokens consulted while evaluating a result.</summary>
type SpatialDependencyReceipt = { RevisionTokens: Set<string> }
/// <summary>Stores a result and the exact disclosed dependencies that may invalidate it.</summary>
type SpatialCacheEntry = { Key: SpatialCacheKey; Result: SpatialQueryResult; Dependencies: SpatialDependencyReceipt }
/// <summary>Separates reusable static entries from revision-sensitive dynamic entries.</summary>
type SpatialCache = { StaticEntries: SpatialCacheEntry list; DynamicEntries: SpatialCacheEntry list }

/// <summary>Evaluates bounded, footprint-aware spatial questions against requester-visible facts.</summary>
[<RequireQualifiedAccess>]
module SpatialQuery =
    /// <summary>Gets the public spatial-result schema version.</summary>
    val schemaVersion: int32
    /// <summary>Gets the compatibility profile governing canonical spatial semantics.</summary>
    val compatibilityProfile: string
    /// <summary>Gets the exact Game.Core package identity used by the adapter.</summary>
    val packageIdentity: string
    /// <summary>Gets conservative default work and result limits.</summary>
    val defaultBounds: QueryBounds
    /// <summary>Gets a cache with no entries.</summary>
    val emptyCache: SpatialCache
    /// <summary>Normalizes an origin-relative footprint or reports why it is invalid.</summary>
    val normalizeFootprint: QueryBounds -> Cell list -> Result<Cell list, string>
    /// <summary>Evaluates one request and returns its disclosed dependency receipt.</summary>
    val evaluate: ProjectedSpatialWorld -> SpatialQueryRequest -> SpatialQueryResult * SpatialDependencyReceipt
    /// <summary>Evaluates through an immutable cache while preserving canonical public bytes.</summary>
    val evaluateCached: SpatialCache -> ProjectedSpatialWorld -> SpatialQueryRequest -> SpatialQueryResult * SpatialCache * SpatialEvaluationSource
    /// <summary>Invalidates only entries that depend on one of the supplied revision tokens.</summary>
    val invalidate: Set<string> -> SpatialCache -> SpatialCache
    /// <summary>Encodes the public result deterministically for native/Fable comparison.</summary>
    val canonicalResultBytes: SpatialQueryResult -> byte array
    /// <summary>Projects a deterministic public JSON explanation without private cache provenance.</summary>
    val canonicalPublicJson: SpatialQueryResult -> string
    /// <summary>Runs the pinned Game.Core point-path adapter for package conformance fixtures.</summary>
    val packagePointPath: maximumExpansions: int32 -> (Cell -> bool) -> Cell -> Cell -> Cell list option
