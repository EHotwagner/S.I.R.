namespace SIR.Client.Web

open SIR.Client
open SIR.Domain
open SIR.Simulation

[<RequireQualifiedAccess>]
module Runner =
    let supportedEngine =
        [| for value in 1 .. 32 -> byte value |]

    let private shortIdentity (bytes: byte array) =
        bytes
        |> Array.take 6
        |> Array.map (fun value -> value.ToString("x2"))
        |> String.concat ""

    let private replayError error =
        match error with
        | UnsupportedFormat(actual, supported) ->
            RunnerUnsupported(
                "format "
                + string actual
                + " is not supported; expected "
                + string supported
            )
        | EngineMismatch _ ->
            RunnerUnsupported "the required retained engine bundle is unavailable"
        | ReplayDivergence(tick, field) ->
            RunnerDiverged(tick, "kernel", field)
        | PackageTooLarge(actual, maximum) ->
            RunnerFailed(
                "package size "
                + string actual
                + " exceeds "
                + string maximum
            )
        | MalformedPackage detail -> RunnerFailed detail
        | UnauthorizedFullReplay ->
            RunnerFailed "the full replay is not authorized"
        | InvalidHashLength(field, _) ->
            RunnerFailed("invalid hash length for " + field)
        | ResourceLimitExceeded(field, _, _) ->
            RunnerFailed("resource limit exceeded for " + field)
        | InvalidOrdering field ->
            RunnerFailed("invalid canonical ordering for " + field)
        | InvalidCheckpoint(tick, detail) ->
            RunnerFailed(
                "invalid checkpoint at tick "
                + string tick
                + ": "
                + detail
            )
        | PerspectiveHasNoKernel ->
            RunnerFailed "perspective playback has no reconstructable kernel"
        | WasmExecutionNotVerified ->
            RunnerFailed "browser verification does not include WASM execution"

    let private metadata sourceName package =
        let kind, finalTick =
            match package.Content with
            | AuthorizedFullReplay full -> FullReplay, full.FinalResult.Tick
            | PerspectivePlayback frames ->
                PerspectiveReplay,
                (frames
                 |> List.tryLast
                 |> Option.map (fun frame -> frame.Tick)
                 |> Option.defaultValue 0)

        { SourceName = sourceName
          SourceIdentity =
            package
            |> Replay.encode
            |> CanonicalHash.sha256
            |> shortIdentity
          EngineIdentity = shortIdentity package.EngineHash
          FinalTick = finalTick
          Kind = kind }

    let execute request =
        async {
            match request with
            | LoadPackage(sourceName, bytes) ->
                match Replay.decode Replay.defaultLimits bytes with
                | Error error -> return replayError error
                | Ok package ->
                    match
                        Replay.runKernelReplay
                            Replay.defaultLimits
                            supportedEngine
                            package
                    with
                    | Ok(BrowserKernelVerified _) ->
                        return
                            LoadedPackage(
                                metadata sourceName package,
                                KernelVerified
                            )
                    | Ok(PerspectiveReady _) ->
                        return
                            LoadedPackage(
                                metadata sourceName package,
                                ProjectionOnly
                            )
                    | Ok(AuthoritativeVerified _) ->
                        return
                            RunnerFailed(
                                "browser runner made an authoritative verification claim"
                            )
                    | Error error -> return replayError error
            | Advance(currentTick, tickCount, finalTick) ->
                return Progressed(min finalTick (currentTick + tickCount))
            | Seek(targetTick, finalTick) ->
                return Progressed(max 0 (min finalTick targetTick))
            | Fork(identity, _) -> return Forked identity
            | Cancel -> return RunnerFailed "cancelled"
        }
