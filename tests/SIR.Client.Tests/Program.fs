module SIR.Client.Tests

open SIR.Client

let private require condition message =
    if not condition then failwith message

let private operationFrom effects =
    effects
    |> List.choose (function
        | Run(operation, Cancel) -> None
        | Run(operation, _) -> Some operation)
    |> List.exactlyOne

let private metadata kind =
    { SourceName = "fixture.sirr"
      SourceIdentity = "fixture"
      EngineIdentity = "engine"
      FinalTick = 20
      Kind = kind }

[<EntryPoint>]
let main _ =
    let initial = Shell.init ()
    let loading, effects =
        Shell.update (ReplayBytesSelected("fixture.sirr", [| 1uy |])) initial

    require (loading.Verification = Loading) "Replay selection must enter Loading."
    let firstOperation = operationFrom effects

    let superseded, replacementEffects =
        Shell.update
            (ReplayBytesSelected("replacement.sirr", [| 2uy |]))
            loading

    let replacementOperation = operationFrom replacementEffects
    require
        (replacementOperation <> firstOperation)
        "Replacing a load reused its operation identity."

    let stale, staleEffects =
        Shell.update
            (RunnerResponded(
                firstOperation,
                RunnerFailed "stale"
            ))
            superseded

    require
        (stale = superseded && List.isEmpty staleEffects)
        "Stale response changed the model."

    let verified =
        Shell.update
            (RunnerResponded(
                replacementOperation,
                LoadedPackage(metadata FullReplay, KernelVerified)
            ))
            superseded
        |> fst

    require
        (verified.Mode = VerifiedReplay
         && verified.Verification = BrowserKernelVerified)
        "Full replay did not become browser-kernel verified."

    let perspective =
        let pending, pendingEffects =
            Shell.update (ReplayBytesSelected("perspective.sirr", [| 2uy |])) initial

        let operation = operationFrom pendingEffects
        Shell.update
            (RunnerResponded(
                operation,
                LoadedPackage(metadata PerspectiveReplay, ProjectionOnly)
            ))
            pending
        |> fst

    require
        (perspective.Mode = PerspectivePlayback
         && perspective.Verification = PerspectiveReady)
        "Perspective package was not kept projection-only."

    let sandbox, forkEffects =
        Shell.update (ParameterEdited("attack-power", 30)) verified

    require
        (match sandbox.Mode, sandbox.Verification with
         | SandboxFork identity, SandboxDerived verificationIdentity ->
             identity = verificationIdentity
         | _ -> false)
        "Parameter edit did not irreversibly create a sandbox identity."
    require (not (List.isEmpty forkEffects)) "Sandbox edit did not request a runner fork."

    let cancelled, cancelEffects = Shell.update CancelRequested sandbox
    require (Option.isNone cancelled.ActiveOperation) "Cancel retained an active operation."
    require
        (cancelEffects
         |> List.exists (function
             | Run(_, Cancel) -> true
             | _ -> false))
        "Cancel did not request runner cancellation."

    let unsupported =
        let pending, pendingEffects =
            Shell.update (ReplayBytesSelected("old.sirr", [| 3uy |])) initial

        let operation = operationFrom pendingEffects
        Shell.update
            (RunnerResponded(operation, RunnerUnsupported "engine unavailable"))
            pending
        |> fst

    require
        (unsupported.Verification = Unsupported "engine unavailable")
        "Unsupported replay state was not preserved."
    require
        (unsupported.Source = Rejected("old.sirr", "engine unavailable"))
        "Unsupported replay did not retain its rejected source."

    let divergent =
        let pending, pendingEffects =
            Shell.update (ReplayBytesSelected("bad.sirr", [| 4uy |])) initial

        let operation = operationFrom pendingEffects
        Shell.update
            (RunnerResponded(
                operation,
                RunnerDiverged(7, "attack", "state hash")
            ))
            pending
        |> fst

    require
        (divergent.Verification = Diverged(7, "attack", "state hash"))
        "Divergence detail was not preserved."
    require
        (match divergent.Source with
         | Rejected("bad.sirr", reason) ->
             reason.Contains("tick 7") && reason.Contains("attack")
         | _ -> false)
        "Divergent replay did not retain its rejected source."

    let deterministicLeft = Shell.update (SpeedChanged Double) verified
    let deterministicRight = Shell.update (SpeedChanged Double) verified
    require
        (deterministicLeft = deterministicRight)
        "Equal messages and models produced different states or effects."

    let requestLeft = Shell.update StepForward verified
    let requestRight = Shell.update StepForward verified
    require
        (requestLeft = requestRight)
        "Equal runner requests produced different operation identities or effects."

    printfn "Elmish shell tests passed: deterministic update, modes, sandbox, stale responses, and cancellation."
    0
