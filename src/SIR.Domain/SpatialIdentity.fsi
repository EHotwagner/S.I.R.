namespace SIR.Domain

/// <summary>Binds a spatial query to immutable map, ruleset, spatial, and requester-knowledge revisions.</summary>
type SpatialAuthorityIdentity =
    { MapIdentity: string
      RulesetIdentity: string
      SpatialRevision: int64
      KnowledgeIdentity: string
      KnowledgeRevision: int64 }

[<RequireQualifiedAccess>]
module SpatialAuthorityIdentity =
    /// <summary>Creates a non-empty identity with non-negative revisions.</summary>
    val create: mapIdentity: string -> rulesetIdentity: string -> spatialRevision: int64 -> knowledgeIdentity: string -> knowledgeRevision: int64 -> Result<SpatialAuthorityIdentity, string>
    /// <summary>Encodes identity fields deterministically for cache and fixture binding.</summary>
    val canonicalBytes: SpatialAuthorityIdentity -> byte array
