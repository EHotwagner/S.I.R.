namespace SIR.Server

open System.Threading.Tasks
open Microsoft.AspNetCore.SignalR
open SIR.Protocol.Realtime

type GameHub() =
    inherit Hub()

    let identity (hub: GameHub) =
        match hub.Context.GetHttpContext() with
        | null -> ""
        | http ->
            let authorization = string http.Request.Headers.Authorization
            let token =
                if authorization.StartsWith("Bearer ", System.StringComparison.OrdinalIgnoreCase) then authorization.Substring("Bearer ".Length)
                else ""
            token

    let binding (hub: GameHub) =
        match hub.Context.Items.TryGetValue "sessionId", hub.Context.Items.TryGetValue "actorId" with
        | (true, sessionId), (true, actorId) -> string sessionId, string actorId
        | _ -> "", ""

    override this.OnConnectedAsync() : Task =
        task {
            let accessToken = identity this

            match LiveAuthority.authorize accessToken this.Context.ConnectionId with
            | None ->
                this.Context.Abort()
                return raise (HubException "unknown S.I.R. live session")
            | Some(sessionId, actorId, snapshot) ->
                this.Context.Items["sessionId"] <- sessionId
                this.Context.Items["actorId"] <- actorId
                do! this.Groups.AddToGroupAsync(this.Context.ConnectionId, sessionId)
                do!
                    this.Clients.Caller.SendAsync(
                        "Message",
                        RealtimeV1.encodeMessage (RealtimeV1.ResyncSnapshotMessage snapshot)
                    )
        }

    member this.SendMessage(json: string) : Task =
        task {
            let sessionId, actorId = binding this
            let accessToken = identity this

            match RealtimeV1.messageFromJson json with
            | Error error -> raise (HubException($"rejected realtime message: {error}"))
            | Ok(RealtimeV1.AdvanceInputMessage input) when input.Version <> 1 ->
                raise (HubException "unsupported realtime version")
            | Ok(RealtimeV1.AdvanceInputMessage input) ->
                match LiveAuthority.advance sessionId actorId accessToken this.Context.ConnectionId input.Sequence with
                | None -> raise (HubException "stale input or unknown S.I.R. live session")
                | Some snapshot ->
                    do!
                        this.Clients.Group(sessionId).SendAsync(
                            "Message",
                            RealtimeV1.encodeMessage (RealtimeV1.SnapshotMessage snapshot)
                        )
            | Ok(RealtimeV1.ResyncRequestMessage request) when request.Version <> 1 ->
                raise (HubException "unsupported realtime version")
            | Ok(RealtimeV1.ResyncRequestMessage request) ->
                match
                    LiveAuthority.reconnect sessionId actorId accessToken this.Context.ConnectionId
                        request.LastServerSequence
                        request.LastProjectionRevision
                with
                | Error error -> raise (HubException error)
                | Ok snapshot ->
                    do!
                        this.Clients.Caller.SendAsync(
                            "Message",
                            RealtimeV1.encodeMessage (RealtimeV1.ResyncSnapshotMessage snapshot)
                        )
            | Ok(RealtimeV1.SnapshotMessage _)
            | Ok(RealtimeV1.ResyncSnapshotMessage _) ->
                raise (HubException "this message kind is server-authoritative")
        }
