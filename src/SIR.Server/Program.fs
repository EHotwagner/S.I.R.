namespace SIR.Server

open System
open System.IO
open System.Threading.Tasks
open System.Security.Claims
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

        app.Use(
            Func<HttpContext, Func<Task>, Task>(fun context next ->
                let allowDevelopmentAnonymous =
                    String.Equals(app.Environment.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase)
                    && String.Equals(builder.Configuration["SIR_ALLOW_ANONYMOUS_LIVE_SESSIONS"], "true", StringComparison.OrdinalIgnoreCase)

                let authenticated =
                    match context.User.Identity with
                    | null -> false
                    | identity -> identity.IsAuthenticated

                if allowDevelopmentAnonymous && not authenticated then
                    let actor = string context.Request.Headers["X-SIR-Development-Actor"]

                    if not (String.IsNullOrWhiteSpace actor) then
                        let identity = ClaimsIdentity([ Claim(ClaimTypes.Name, actor) ], "sir-development")
                        context.User <- ClaimsPrincipal identity

                next.Invoke()))
        |> ignore

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
                        let principal =
                            match request.HttpContext.User.Identity with
                            | null -> ""
                            | identity when identity.IsAuthenticated -> identity.Name |> Option.ofObj |> Option.defaultValue ""
                            | _ -> ""

                        match LiveAuthority.bootstrap principal parsed.ActorName with
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
