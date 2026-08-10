namespace SIR.Server

open System
open System.Collections.Concurrent
open System.Security.Cryptography
open SIR.Match
open SIR.Protocol.Http

type private SessionState =
    { ActorId: string
      PrincipalId: string
      AccessToken: string
      ExpiresAt: DateTimeOffset
      Admission: LiveAdmission
      mutable LastInputSequence: int
      mutable FrameIndex: int
      mutable Revoked: bool
      mutable ConnectionId: string option }

/// Concurrency-safe host shell around S.I.R.'s existing deterministic live slice.
/// The replay and admission rules remain consumer-owned in SIR.Match; this module
/// only adapts them to the scaffold's transport boundary.
[<RequireQualifiedAccess>]
module LiveAuthority =

    let private qualification = lazy (LiveIntegration.qualify ())
    let private sessions = ConcurrentDictionary<string, SessionState>()
    let private gate = obj ()

    let private tokenLifetime = TimeSpan.FromMinutes 15.0

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

    let private createToken () =
        RandomNumberGenerator.GetBytes 32
        |> Convert.ToBase64String
        |> fun value -> value.TrimEnd('=').Replace('+', '-').Replace('/', '_')

    let private validToken (stored: string) (provided: string) =
        CryptographicOperations.FixedTimeEquals(System.Text.Encoding.UTF8.GetBytes(stored), System.Text.Encoding.UTF8.GetBytes(provided))

    let bootstrap principalId actorName =
        if String.IsNullOrWhiteSpace principalId || String.IsNullOrWhiteSpace actorName then
            Error "SIR.LIVE.BOOTSTRAP.ACTOR_REQUIRED"
        elif principalId <> actorName then
            Error "SIR.LIVE.BOOTSTRAP.ACTOR_FORBIDDEN"
        else
            let sessionId = Guid.NewGuid().ToString "N"
            let actorId = Guid.NewGuid().ToString "N"
            let accessToken = createToken ()
            let slice = qualification.Value

            match LiveIntegration.admit sessionId actorId slice.Artifact slice.Artifact with
            | Error error -> Error error
            | Ok admission ->
                lock gate (fun () ->
                    sessions.Values
                    |> Seq.filter (fun existing -> existing.PrincipalId = principalId && not existing.Revoked)
                    |> Seq.iter (fun existing -> existing.Revoked <- true))

                let state =
                    { ActorId = actorId
                      PrincipalId = principalId
                      AccessToken = accessToken
                      ExpiresAt = DateTimeOffset.UtcNow.Add tokenLifetime
                      Admission = admission
                      LastInputSequence = 0
                      FrameIndex = 0
                      Revoked = false
                      ConnectionId = None }

                sessions[sessionId] <- state

                let response: BootstrapV1.Response =
                    { Version = 1
                      SessionId = sessionId
                      ActorId = actorId
                      AccessToken = accessToken
                      MatchLock = hex admission.MatchLock
                      Snapshot = snapshotAt 0 }

                Ok response

    let authorize accessToken connectionId =
        lock gate (fun () ->
            sessions
            |> Seq.tryPick (fun pair ->
                let state = pair.Value

                if not state.Revoked
                   && state.ExpiresAt > DateTimeOffset.UtcNow
                     && validToken state.AccessToken accessToken then
                    state.ConnectionId <- Some connectionId
                    Some(pair.Key, state.ActorId, snapshotAt state.FrameIndex)
                else
                    None))

    let advance sessionId actorId accessToken connectionId sequence =
        lock gate (fun () ->
            match sessions.TryGetValue sessionId with
            | true, state
                when state.ActorId = actorId
                     && state.ConnectionId = Some connectionId
                     && not state.Revoked
                     && state.ExpiresAt > DateTimeOffset.UtcNow
                     && validToken state.AccessToken accessToken
                     && sequence > state.LastInputSequence ->
                state.LastInputSequence <- sequence
                state.FrameIndex <- min (state.FrameIndex + 1) (qualification.Value.Replay.Frames.Length - 1)
                Some(snapshotAt state.FrameIndex)
            | _ -> None)

    let reconnect sessionId actorId accessToken connectionId lastServerSequence lastProjectionRevision =
        lock gate (fun () ->
            match sessions.TryGetValue sessionId with
            | true, state
                when state.ActorId = actorId
                     && state.ConnectionId = Some connectionId
                     && not state.Revoked
                     && state.ExpiresAt > DateTimeOffset.UtcNow
                     && validToken state.AccessToken accessToken ->
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
