namespace SIR.Client.Web

open Fable.Core
open Fable.Core.JsInterop
open SIR.Protocol.Http

[<RequireQualifiedAccess>]
module LiveApi =

    [<Global>]
    let private fetch (url: string, options: obj) : JS.Promise<obj> = jsNative

    let bootstrap (request: BootstrapV1.Request) =
        async {
            let options =
                createObj
                    [ "method" ==> "POST"
                      "headers" ==>
                        createObj
                            [ "Content-Type" ==> "application/json"
                              "X-SIR-Development-Actor" ==> request.ActorName ]
                      "body" ==> BootstrapV1.encodeRequest request ]

            let! response = fetch ("/api/bootstrap", options) |> Async.AwaitPromise
            let! body = response?text() |> Async.AwaitPromise

            if not (unbox<bool> response?ok) then
                failwith $"bootstrap request failed: {body}"

            return
                BootstrapV1.responseFromJson (string body)
                |> Result.defaultWith (fun error -> failwith $"bootstrap response did not decode: {error}")
        }
