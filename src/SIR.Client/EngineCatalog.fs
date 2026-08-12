namespace SIR.Client

/// One immutable browser engine retained for replay compatibility.
type RetainedEngine =
    { Version: string
      Identity: string
      EngineHash: byte array
      ReplayFormatVersions: int32 list
      WorkerPath: string }

/// Compile-time catalog used to select an exact engine before worker execution.
[<RequireQualifiedAccess>]
module EngineCatalog =
    [<Literal>]
    let CurrentIdentity =
        "0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20"

    [<Literal>]
    let CurrentWorkerPath =
        "engines/0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20/worker.js"

    let private identity (bytes: byte array) =
        bytes
        |> Array.map (fun value -> value.ToString("x2"))
        |> String.concat ""

    let Current =
        { Version = "v1"
          Identity = CurrentIdentity
          EngineHash = [| for value in 1 .. 32 -> byte value |]
          ReplayFormatVersions = [ 1; 2; 3 ]
          WorkerPath = CurrentWorkerPath }

    let Retained = [ Current ]

    /// Selects only a retained engine whose replay-format contract includes the fixed header.
    let tryFind engineHash formatVersion =
        Retained
        |> List.tryFind (fun engine ->
            engine.Identity = identity engineHash
            && List.contains formatVersion engine.ReplayFormatVersions)
