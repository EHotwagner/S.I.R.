namespace SIR.Client.Web

open Fable.Core
open Fable.Core.JsInterop

/// Narrow Fable binding over the official npm SignalR client, retained from the
/// published scaffold rather than depending on an unmaintained wrapper package.
[<RequireQualifiedAccess>]
module SignalR =

    type HubConnection =
        abstract on: methodName: string * handler: (string -> unit) -> unit
        abstract onreconnected: handler: (string -> unit) -> unit
        abstract onclose: handler: (obj -> unit) -> unit
        abstract start: unit -> JS.Promise<unit>
        abstract stop: unit -> JS.Promise<unit>
        abstract invoke: methodName: string * argument: string -> JS.Promise<obj>

    [<Import("HubConnectionBuilder", "@microsoft/signalr")>]
    type HubConnectionBuilder() =
        member _.withUrl(url: string) : HubConnectionBuilder = jsNative
        member _.withAutomaticReconnect() : HubConnectionBuilder = jsNative
        member _.build() : HubConnection = jsNative

    let build url = HubConnectionBuilder().withUrl(url).withAutomaticReconnect().build()
