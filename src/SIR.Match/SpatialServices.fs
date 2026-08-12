namespace SIR.Match

open SIR.Simulation

/// One bounded control-facing spatial service response. Cache internals are not exposed.
type SpatialServiceResponse =
    { Result: SpatialQueryResult
      CanonicalBytes: byte array }

[<RequireQualifiedAccess>]
module SpatialServices =
    /// Evaluates a knowledge-projected request under the public schema bounds.
    let query world request =
        let result, _ = SpatialQuery.evaluate world request
        { Result = result
          CanonicalBytes = SpatialQuery.canonicalResultBytes result }

    /// Evaluates through an explicit caller-owned cache while preserving public bytes.
    let queryCached cache world request =
        let result, next, source = SpatialQuery.evaluateCached cache world request
        { Result = result
          CanonicalBytes = SpatialQuery.canonicalResultBytes result }, next, source
