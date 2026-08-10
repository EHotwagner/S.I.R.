namespace SIR.Server

open System
open System.Collections.Concurrent
open SIR.Match
open SIR.Protocol.Http

type private SessionState =
    { ActorId: string
      Admission: LiveAdmission
      mutable LastInputSequence: int
      mutable FrameIndex: int }

/// Concurrency-safe host shell around S.I.R.'s existing deterministic live slice.
/// The replay and admission rules remain consumer-owned in SIR.Match; this module
/// only adapts them to the scaffold's transport boundary.
[<RequireQualifiedAccess>]
module LiveAuthority =

    let private qualification = lazy (LiveIntegration.qualify ())
    let private sessions = ConcurrentDictionary<string, SessionState>()
    let private gate = obj ()

    let private hex (bytes: byte array) = Convert.ToHexString(bytes).ToLowerInvariant()

    let private snapshotAt index : BootstrapV1.Snapshot =
        let frame = qualification.Value.Replay.Frames[index]

        { Version = 1
          Tick = frame.Tick
          ServerSequence = int frame.ServerSequence
          ProjectionRevision = int frame.ProjectionRevision
          VisibleUnits =
            frame.VisibleUnits
            |> Array.map (fun visibleUnit ->
                ({ UnitId = visibleUnit.UnitId
                   Column = visibleUnit.DisplayColumn
                   Row = visibleUnit.DisplayRow
                   Health = visibleUnit.Health }: BootstrapV1.VisibleUnit))
            |> Array.toList
          StateIdentity = hex frame.StateIdentity }

    let bootstrap actorName =
        if String.IsNullOrWhiteSpace actorName then
            Error "SIR.LIVE.BOOTSTRAP.ACTOR_REQUIRED"
        else
            let sessionId = Guid.NewGuid().ToString "N"
            let actorId = Guid.NewGuid().ToString "N"
            let slice = qualification.Value

            match LiveIntegration.admit sessionId actorId slice.Artifact slice.Artifact with
            | Error error -> Error error
            | Ok admission ->
                let state =
                    { ActorId = actorId
                      Admission = admission
                      LastInputSequence = 0
                      FrameIndex = 0 }

                sessions[sessionId] <- state

                let response: BootstrapV1.Response =
                    { Version = 1
                      SessionId = sessionId
                      ActorId = actorId
                      MatchLock = hex admission.MatchLock
                      Snapshot = snapshotAt 0 }

                Ok response

    let trySnapshot sessionId actorId =
        lock gate (fun () ->
            match sessions.TryGetValue sessionId with
            | true, state when state.ActorId = actorId -> Some(snapshotAt state.FrameIndex)
            | _ -> None)

    let advance sessionId actorId sequence =
        lock gate (fun () ->
            match sessions.TryGetValue sessionId with
            | true, state when state.ActorId = actorId && sequence > state.LastInputSequence ->
                state.LastInputSequence <- sequence
                state.FrameIndex <- min (state.FrameIndex + 1) (qualification.Value.Replay.Frames.Length - 1)
                Some(snapshotAt state.FrameIndex)
            | _ -> None)

    let reconnect sessionId actorId lastServerSequence lastProjectionRevision =
        lock gate (fun () ->
            match sessions.TryGetValue sessionId with
            | true, state when state.ActorId = actorId ->
                match
                    LiveIntegration.reconnect
                        state.Admission
                        qualification.Value.Replay
                        (int64 lastServerSequence)
                        (int64 lastProjectionRevision)
                with
                | Error error -> Error error
                | Ok _ -> Ok(snapshotAt state.FrameIndex)
            | _ -> Error "SIR.LIVE.RECONNECT.SESSION_UNKNOWN")
