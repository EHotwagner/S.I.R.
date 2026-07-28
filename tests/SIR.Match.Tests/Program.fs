module SIR.Match.Tests

open SIR.Domain
open SIR.Match
open SIR.Simulation

let private require condition message =
    if not condition then failwith message

let private containsSubsequence (needle: byte array) (haystack: byte array) =
    if needle.Length = 0 then
        true
    else
        [ 0 .. haystack.Length - needle.Length ]
        |> List.exists (fun offset ->
            haystack[offset .. offset + needle.Length - 1] = needle)

[<EntryPoint>]
let main _ =
    let qualification = MatchReplay.qualify ()
    let fullBytes = Replay.encode qualification.FullPackage
    let perspectiveBytes = Replay.encode qualification.PerspectivePackage
    let expectedEngine = qualification.FullPackage.EngineHash

    match
        Replay.runKernelReplay
            Replay.defaultLimits
            expectedEngine
            qualification.FullPackage
    with
    | Ok(BrowserKernelVerified browserResult) ->
        match
            Replay.verifyAuthoritative
                Replay.defaultLimits
                expectedEngine
                (Some qualification.ReexecutedOutputs)
                qualification.FullPackage
        with
        | Ok(AuthoritativeVerified authoritativeResult) ->
            require
                (browserResult.StateHash = authoritativeResult.StateHash
                 && browserResult.EventHash = authoritativeResult.EventHash)
                "Browser-kernel and authoritative hashes differ."
        | result ->
            failwithf "Exact-artifact authoritative verification failed: %A" result
    | result -> failwithf "Full browser-kernel replay failed: %A" result

    let changedOutputs =
        qualification.ReexecutedOutputs
        |> List.map (fun output ->
            if output.Tick = 2 then
                { output with
                    Input =
                        Move(
                            Simulation.unitId 10,
                            { Col = 0; Row = 1 }
                        ) }
            else
                output)

    match
        Replay.verifyAuthoritative
            Replay.defaultLimits
            expectedEngine
            (Some changedOutputs)
            qualification.FullPackage
    with
    | Error(WasmOutputDivergence(2, 2)) -> ()
    | result ->
        failwithf "Changed WASM output did not identify its first divergence: %A" result

    let corrupted =
        match qualification.FullPackage.Content with
        | PerspectivePlayback _ -> failwith "Expected an authorized full replay."
        | AuthorizedFullReplay full ->
            let checkpoints =
                full.Checkpoints
                |> List.map (fun checkpoint ->
                    if checkpoint.Tick = 2 then
                        let changed = Array.copy checkpoint.EventHash
                        changed[0] <- changed[0] ^^^ 1uy
                        { checkpoint with EventHash = changed }
                    else
                        checkpoint)

            { qualification.FullPackage with
                Content =
                    AuthorizedFullReplay
                        { full with Checkpoints = checkpoints } }

    match Replay.runKernelReplay Replay.defaultLimits expectedEngine corrupted with
    | Error(ReplayDivergence(2, "checkpoint event hash")) -> ()
    | result ->
        failwithf "Corrupt replay lost first-tick divergence diagnostics: %A" result

    match
        Replay.runKernelReplay
            Replay.defaultLimits
            expectedEngine
            qualification.PerspectivePackage
    with
    | Ok(PerspectiveReady frames) ->
        require (frames.Length = 5) "Perspective playback omitted a committed frame."
    | result -> failwithf "Perspective playback qualification failed: %A" result

    match Replay.requireKernel qualification.PerspectivePackage with
    | Error PerspectiveHasNoKernel -> ()
    | result -> failwithf "Perspective playback exposed kernel material: %A" result

    let hiddenFinalHash =
        match qualification.FullPackage.Content with
        | AuthorizedFullReplay full -> full.FinalResult.StateHash
        | PerspectivePlayback _ -> failwith "Expected an authorized full replay."

    require
        (not (containsSubsequence hiddenFinalHash perspectiveBytes))
        "Perspective bytes contain the hidden final-state hash."
    require
        (not (
            containsSubsequence
                qualification.Artifact.ArtifactBytes
                perspectiveBytes
        ))
        "Perspective bytes contain the opponent control artifact."
    require
        (perspectiveBytes.Length < fullBytes.Length)
        "Perspective package is not a reduced disclosure."

    printfn
        "Full match replay qualified: %d full bytes, %d perspective bytes, 4 exact WASM outputs."
        fullBytes.Length
        perspectiveBytes.Length

    0
