namespace SIR.Server

open System
open System.IO
open System.Threading.Tasks
open System.Security.Claims
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Authentication
open Microsoft.Extensions.DependencyInjection
open SIR.Protocol.Http

type Program() = class end

module Program =

    [<EntryPoint>]
    let main args =
        let builder = WebApplication.CreateBuilder args
        builder.Services.AddSignalR() |> ignore
        builder.Services.AddSingleton<TimeProvider>(TimeProvider.System) |> ignore
        builder.Services
            .AddAuthentication("sir-live")
            .AddScheme<AuthenticationSchemeOptions, LiveAuthenticationHandler>("sir-live", null)
            |> ignore
        builder.Services.AddAuthorization() |> ignore
        let app = builder.Build()
        LiveAuthority.configure (app.Services.GetRequiredService<TimeProvider>()) (TimeSpan.FromMinutes 15.0)
        app.UseAuthentication() |> ignore
        app.UseAuthorization() |> ignore

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
            .RequireAuthorization()
        |> ignore

        app.MapHub<GameHub>("/hub/game") |> ignore
        app.UseDefaultFiles() |> ignore
        app.UseStaticFiles() |> ignore
        app.MapFallbackToFile("index.html") |> ignore
        app.Run()
        0
