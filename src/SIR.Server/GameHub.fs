namespace SIR.Server

open System.Threading.Tasks
open Microsoft.AspNetCore.SignalR
open SIR.Protocol.Realtime

type GameHub() =
    inherit Hub()

    let identity (hub: GameHub) =
        match hub.Context.GetHttpContext() with
        | null -> "", ""
        | http -> string http.Request.Query["sessionId"], string http.Request.Query["actorId"]

    override this.OnConnectedAsync() : Task =
        task {
            let sessionId, actorId = identity this

            match LiveAuthority.trySnapshot sessionId actorId with
            | None ->
                this.Context.Abort()
                return raise (HubException "unknown S.I.R. live session")
            | Some snapshot ->
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
            let sessionId, actorId = identity this

            match RealtimeV1.messageFromJson json with
            | Error error -> raise (HubException($"rejected realtime message: {error}"))
            | Ok(RealtimeV1.AdvanceInputMessage input) when input.Version <> 1 ->
                raise (HubException "unsupported realtime version")
            | Ok(RealtimeV1.AdvanceInputMessage input) ->
                match LiveAuthority.advance sessionId actorId input.Sequence with
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
                    LiveAuthority.reconnect
                        sessionId
                        actorId
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
