namespace SIR.Client

/// Runtime-neutral helpers shared by browser composition and client qualifications.
[<RequireQualifiedAccess>]
module ClientModuleBoundaries =
    /// Produces the canonical registry representation for a keyboard gesture.
    let canonicalGesture (gesture: string) =
        if System.String.IsNullOrWhiteSpace gesture then
            ""
        else
            gesture.Trim().ToUpperInvariant()
