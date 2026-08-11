namespace SIR.Client

/// Declares the live-client assembly boundary. The replay/editor host is owned by
/// SIR.Replay.Web and is deliberately not referenced here.
module LiveClientBoundary =
    [<Literal>]
    let AssemblyRole = "live-client"
