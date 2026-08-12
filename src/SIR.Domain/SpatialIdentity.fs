namespace SIR.Domain

/// <summary>Binds a spatial query to immutable map/ruleset and requester-knowledge revisions.</summary>
type SpatialAuthorityIdentity =
    { MapIdentity: string
      RulesetIdentity: string
      SpatialRevision: int64
      KnowledgeIdentity: string
      KnowledgeRevision: int64 }

/// <summary>Validates and canonically encodes spatial authority identities.</summary>
[<RequireQualifiedAccess>]
module SpatialAuthorityIdentity =
    let private utf8 (value: string) =
        let bytes = System.Text.Encoding.UTF8.GetBytes value
        CanonicalEncoding.concatenate [ CanonicalEncoding.int32LittleEndian bytes.Length; bytes ]

    let private int64LittleEndian (value: int64) =
        [| for shift in 0 .. 8 .. 56 -> byte (value >>> shift) |]

    /// <summary>Creates a non-empty identity with non-negative revisions.</summary>
    let create mapIdentity rulesetIdentity spatialRevision knowledgeIdentity knowledgeRevision =
        if System.String.IsNullOrWhiteSpace mapIdentity then Error "Map identity is required."
        elif System.String.IsNullOrWhiteSpace rulesetIdentity then Error "Ruleset identity is required."
        elif System.String.IsNullOrWhiteSpace knowledgeIdentity then Error "Knowledge identity is required."
        elif spatialRevision < 0L || knowledgeRevision < 0L then Error "Spatial and knowledge revisions must be non-negative."
        else
            Ok
                { MapIdentity = mapIdentity
                  RulesetIdentity = rulesetIdentity
                  SpatialRevision = spatialRevision
                  KnowledgeIdentity = knowledgeIdentity
                  KnowledgeRevision = knowledgeRevision }

    /// <summary>Encodes identity fields deterministically for cache and fixture binding.</summary>
    let canonicalBytes identity =
        CanonicalEncoding.concatenate
            [ utf8 identity.MapIdentity
              utf8 identity.RulesetIdentity
              int64LittleEndian identity.SpatialRevision
              utf8 identity.KnowledgeIdentity
              int64LittleEndian identity.KnowledgeRevision ]
