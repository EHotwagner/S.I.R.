namespace SIR.Match

open SIR.Simulation

/// One authoritative, replay-safe physical-combat resolution returned to match hosts.
type PhysicalCombatServiceResponse =
    { Result: CombatResult
      CanonicalBytes: byte array
      ExplanationBytes: byte array }

[<RequireQualifiedAccess>]
module PhysicalCombatServices =
    /// Resolves a bounded request through Simulation-owned spatial and combat authority.
    let resolve world request =
        Combat.resolve world request
        |> Result.map (fun result ->
            { Result = result
              CanonicalBytes = Combat.canonicalResultBytes result
              ExplanationBytes = Combat.canonicalFactsBytes result.Facts })
