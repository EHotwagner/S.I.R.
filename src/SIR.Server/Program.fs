namespace SIR.Server

open System
open System.IO
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open SIR.Protocol.Http

type Program() = class end

module Program =

    [<EntryPoint>]
    let main args =
        let builder = WebApplication.CreateBuilder args
        builder.Services.AddSignalR() |> ignore
        let app = builder.Build()

        app.MapPost(
            "/api/bootstrap",
            Func<HttpRequest, Task<IResult>>(fun request ->
                task {
                    use reader = new StreamReader(request.Body)
                    let! body = reader.ReadToEndAsync()

                    match BootstrapV1.requestFromJson body with
                    | Error error -> return Results.BadRequest {| error = error |}
                    | Ok parsed when parsed.Version <> 1 ->
                        return Results.BadRequest {| error = "unsupported bootstrap version" |}
                    | Ok parsed ->
                        match LiveAuthority.bootstrap parsed.ActorName with
                        | Error error -> return Results.BadRequest {| error = error |}
                        | Ok response ->
                            return Results.Text(BootstrapV1.encodeResponse response, "application/json")
                })
        )
        |> ignore

        app.MapHub<GameHub>("/hub/game") |> ignore
        app.UseDefaultFiles() |> ignore
        app.UseStaticFiles() |> ignore
        app.MapFallbackToFile("index.html") |> ignore
        app.Run()
        0
