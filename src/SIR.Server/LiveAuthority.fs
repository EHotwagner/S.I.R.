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
      mutable ConnectionId: string option
      mutable DisconnectedAt: DateTimeOffset option
      Gate: obj }

type LiveSessionMetrics =
    { ActiveSessions: int
      RejectedAdmissions: int
      Evictions: int
      Expiries: int
      Contention: int }

/// Concurrency-safe host shell around S.I.R.'s existing deterministic live slice.
/// The replay and admission rules remain consumer-owned in SIR.Match; this module
/// only adapts them to the scaffold's transport boundary.
[<RequireQualifiedAccess>]
module LiveAuthority =

    let private qualification = lazy (LiveIntegration.qualify ())
    let private sessions = ConcurrentDictionary<string, SessionState>()
    let private countersGate = obj ()
    let private admissionGate = obj ()
    let private maximumSessions = 64
    let private maximumActorNameLength = 128
    let private maximumAdmissionsPerMinute = 8
    let private admissions = ConcurrentDictionary<string, DateTimeOffset list>()
    let private disconnectGrace = TimeSpan.FromMinutes 2.0

    let mutable private timeProvider = TimeProvider.System
    let mutable private tokenLifetime = TimeSpan.FromMinutes 15.0
    let mutable private tokenValidationCount = 0
    let mutable private sessionLookupCount = 0
    let mutable private rejectedAdmissions = 0
    let mutable private evictions = 0
    let mutable private expiries = 0
    let mutable private contention = 0

    /// Configures the host-owned lifetime policy; production supplies DI's TimeProvider.
    let configure (clock: TimeProvider) lifetime =
        timeProvider <- clock
        tokenLifetime <- lifetime
        sessions.Clear()
        admissions.Clear()
        lock countersGate (fun () -> rejectedAdmissions <- 0; evictions <- 0; expiries <- 0; contention <- 0)

    /// Resets deterministic admission work counters used by the focused transport baseline.
    let resetStructuralCounters () =
        lock countersGate (fun () ->
            tokenValidationCount <- 0
            sessionLookupCount <- 0)

    /// Returns (token validations, session lookups) for the current live-process baseline.
    let structuralCounters () =
        lock countersGate (fun () -> tokenValidationCount, sessionLookupCount)

    let activeSessionCount () = sessions.Count

    let metrics () = lock countersGate (fun () -> { ActiveSessions = sessions.Count; RejectedAdmissions = rejectedAdmissions; Evictions = evictions; Expiries = expiries; Contention = contention })

    let private removeExpired () =
        let now = timeProvider.GetUtcNow()
        sessions
        |> Seq.filter (fun pair -> pair.Value.ExpiresAt <= now || pair.Value.DisconnectedAt |> Option.exists (fun disconnected -> disconnected.Add disconnectGrace <= now))
        |> Seq.iter (fun pair -> if sessions.TryRemove pair.Key |> fst then lock countersGate (fun () -> expiries <- expiries + 1))

    let private countLookup () = lock countersGate (fun () -> sessionLookupCount <- sessionLookupCount + 1)
    let private countValidation () = lock countersGate (fun () -> tokenValidationCount <- tokenValidationCount + 1)
    let private withContention gate action =
        if not (System.Threading.Monitor.TryEnter gate) then
            lock countersGate (fun () -> contention <- contention + 1)
            System.Threading.Monitor.Enter gate
        try action () finally System.Threading.Monitor.Exit gate

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
      withContention admissionGate (fun () ->
        removeExpired ()
        let now = timeProvider.GetUtcNow()
        let recent = admissions.GetOrAdd(principalId, []) |> List.filter (fun admitted -> admitted.AddMinutes 1.0 > now)
        admissions[principalId] <- now :: recent
        if String.IsNullOrWhiteSpace principalId || String.IsNullOrWhiteSpace actorName || actorName.Length > maximumActorNameLength then
            lock countersGate (fun () -> rejectedAdmissions <- rejectedAdmissions + 1)
            Error "SIR.LIVE.BOOTSTRAP.ACTOR_REQUIRED"
        elif principalId <> actorName then
            lock countersGate (fun () -> rejectedAdmissions <- rejectedAdmissions + 1)
            Error "SIR.LIVE.BOOTSTRAP.ACTOR_FORBIDDEN"
        elif recent.Length >= maximumAdmissionsPerMinute then
            lock countersGate (fun () -> rejectedAdmissions <- rejectedAdmissions + 1)
            Error "SIR.LIVE.BOOTSTRAP.RATE_REJECTED"
        elif sessions.Count >= maximumSessions then
            lock countersGate (fun () -> rejectedAdmissions <- rejectedAdmissions + 1)
            Error "SIR.LIVE.BOOTSTRAP.CAPACITY_REJECTED"
        else
            let sessionId = Guid.NewGuid().ToString "N"
            let actorId = Guid.NewGuid().ToString "N"
            let accessToken = createToken ()
            let slice = qualification.Value

            match LiveIntegration.admit sessionId actorId slice.Artifact slice.Artifact with
            | Error error -> Error error
            | Ok admission ->
                sessions
                |> Seq.filter (fun pair -> pair.Value.PrincipalId = principalId && not pair.Value.Revoked)
                |> Seq.iter (fun pair -> if sessions.TryRemove pair.Key |> fst then lock countersGate (fun () -> evictions <- evictions + 1))

                let state =
                    { ActorId = actorId
                      PrincipalId = principalId
                      AccessToken = accessToken
                      ExpiresAt = timeProvider.GetUtcNow().Add tokenLifetime
                      Admission = admission
                      LastInputSequence = 0
                      FrameIndex = 0
                      Revoked = false
                      ConnectionId = None
                      DisconnectedAt = None
                      Gate = obj () }

                sessions[sessionId] <- state

                let response: BootstrapV1.Response =
                    { Version = 1
                      SessionId = sessionId
                      ActorId = actorId
                      AccessToken = accessToken
                      MatchLock = hex admission.MatchLock
                      Snapshot = snapshotAt 0 }

                Ok response)

    let authorize accessToken connectionId =
        removeExpired (); countValidation (); countLookup ()
        sessions
        |> Seq.tryPick (fun pair ->
            let state = pair.Value
            lock state.Gate (fun () ->
                if not state.Revoked && state.ExpiresAt > timeProvider.GetUtcNow() && validToken state.AccessToken accessToken then
                    state.ConnectionId <- Some connectionId; state.DisconnectedAt <- None; Some(pair.Key, state.ActorId, snapshotAt state.FrameIndex)
                else None))

    let advance sessionId actorId accessToken connectionId sequence =
        removeExpired (); countValidation (); countLookup ()
        match sessions.TryGetValue sessionId with
        | true, state -> withContention state.Gate (fun () ->
            match true, state with
            | true, state
                when state.ActorId = actorId
                     && state.ConnectionId = Some connectionId
                     && not state.Revoked
                     && state.ExpiresAt > timeProvider.GetUtcNow()
                     && validToken state.AccessToken accessToken
                     && sequence > state.LastInputSequence ->
                state.LastInputSequence <- sequence
                state.FrameIndex <- min (state.FrameIndex + 1) (qualification.Value.Replay.Frames.Length - 1)
                Some(snapshotAt state.FrameIndex)
            | _ -> None)
        | _ -> None

    let reconnect sessionId actorId accessToken connectionId lastServerSequence lastProjectionRevision =
        removeExpired (); countValidation (); countLookup ()
        match sessions.TryGetValue sessionId with
        | true, state -> withContention state.Gate (fun () ->
            match true, state with
            | true, state
                when state.ActorId = actorId
                     && state.ConnectionId = Some connectionId
                     && not state.Revoked
                     && state.ExpiresAt > timeProvider.GetUtcNow()
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
        | _ -> Error "SIR.LIVE.RECONNECT.SESSION_UNKNOWN"

    let disconnected sessionId connectionId =
        match sessions.TryGetValue sessionId with
        | true, state -> withContention state.Gate (fun () -> if state.ConnectionId = Some connectionId then state.DisconnectedAt <- Some(timeProvider.GetUtcNow()); state.ConnectionId <- None)
        | _ -> ()
