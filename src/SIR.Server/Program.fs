namespace SIR.Server

open System
open System.IO
open System.Threading.Tasks
open System.Security.Claims
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.ResponseCompression
open Microsoft.AspNetCore.StaticFiles
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.IdentityModel.Tokens
open System.Text
open SIR.Protocol.Http

type Program() = class end

module Program =

    let private maximumBootstrapBodyBytes = 16 * 1024

    let private cacheControlForStaticAsset (path: PathString) =
        // Engine paths embed the retained engine identity. Everything else can
        // be replaced by a normal deployment and must therefore revalidate.
        if path.StartsWithSegments(PathString("/engines")) then
            "public,max-age=31536000,immutable"
        else
            "public,max-age=0,must-revalidate"

    let private configureStaticAssetResponse (context: StaticFileResponseContext) =
        context.Context.Response.Headers.CacheControl <- cacheControlForStaticAsset context.Context.Request.Path

    let private readBoundedBody maximumBytes (stream: Stream) =
        task {
            use output = new MemoryStream()
            let buffer = Array.zeroCreate<byte> 4096
            let mutable total = 0
            let mutable complete = false
            let mutable tooLarge = false
            while not complete && not tooLarge do
                let! count = stream.ReadAsync(buffer.AsMemory())
                if count = 0 then complete <- true
                elif total + count > maximumBytes then tooLarge <- true
                else
                    output.Write(buffer, 0, count)
                    total <- total + count
            return if tooLarge then Error "SIR.LIVE.BOOTSTRAP.BODY_TOO_LARGE" else Ok(Encoding.UTF8.GetString(output.ToArray()))
        }

    let private readBootstrapBody stream = readBoundedBody maximumBootstrapBodyBytes stream

    let private mapRoutes (app: WebApplication) =
        app.MapPost(
            "/api/bootstrap",
            Func<HttpRequest, Task<IResult>>(fun request ->
                task {
                    if request.ContentLength.HasValue && request.ContentLength.Value > int64 maximumBootstrapBodyBytes then
                        return Results.BadRequest {| error = "SIR.LIVE.BOOTSTRAP.BODY_TOO_LARGE" |}
                    else
                        let! boundedBody = readBootstrapBody request.Body
                        match boundedBody with
                        | Error error -> return Results.BadRequest {| error = error |}
                        | Ok body ->
                            match BootstrapV1.requestFromJson body with
                            | Error error -> return Results.BadRequest {| error = error |}
                            | Ok parsed when parsed.Version <> 1 -> return Results.BadRequest {| error = "unsupported bootstrap version" |}
                            | Ok parsed ->
                                let principal =
                                    match request.HttpContext.User.Identity with
                                    | null -> ""
                                    | identity when identity.IsAuthenticated -> identity.Name |> Option.ofObj |> Option.defaultValue ""
                                    | _ -> ""
                                match LiveAuthority.bootstrap principal parsed.ActorName with
                                | Error error -> return Results.BadRequest {| error = error |}
                                | Ok response -> return Results.Text(BootstrapV1.encodeResponse response, "application/json")
                })
        )
            .RequireAuthorization()
        |> ignore

        app.MapPost(
            "/api/spatial/diagnostics",
            Func<HttpRequest, Task<IResult>>(fun request ->
                task {
                    let maximumBytes = 256 * 1024
                    let authorization = string request.Headers.Authorization
                    let accessToken =
                        if authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) then authorization.Substring("Bearer ".Length)
                        else ""
                    if not (LiveAuthority.authorizeHttp accessToken) then
                        return Results.Unauthorized()
                    elif request.ContentLength.HasValue && request.ContentLength.Value > int64 maximumBytes then
                        return Results.BadRequest {| error = "spatial diagnostic body too large" |}
                    else
                        let! boundedBody = readBoundedBody maximumBytes request.Body
                        match boundedBody with
                        | Error _ -> return Results.BadRequest {| error = "spatial diagnostic body too large" |}
                        | Ok body ->
                            match SpatialDiagnostics.evaluate body with
                            | Error error -> return Results.BadRequest {| error = error |}
                            | Ok response -> return Results.Text(response, "application/json")
                })
        ) |> ignore

        app.MapPost(
            "/api/combat/physical-drill",
            Func<HttpRequest, Task<IResult>>(fun request ->
                task {
                    let maximumBytes = 4 * 1024
                    let authorization = string request.Headers.Authorization
                    let accessToken =
                        if authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) then authorization.Substring("Bearer ".Length)
                        else ""
                    if not (LiveAuthority.authorizeHttp accessToken) then
                        return Results.Unauthorized()
                    elif request.ContentLength.HasValue && request.ContentLength.Value > int64 maximumBytes then
                        return Results.BadRequest {| error = "physical combat body too large" |}
                    else
                        let! boundedBody = readBoundedBody maximumBytes request.Body
                        match boundedBody with
                        | Error _ -> return Results.BadRequest {| error = "physical combat body too large" |}
                        | Ok body ->
                            match PhysicalCombatDiagnostics.evaluate body with
                            | Error error -> return Results.BadRequest {| error = error |}
                            | Ok response -> return Results.Text(response, "application/json")
                })
        ) |> ignore

        app.MapPost(
            "/api/awareness/local-projection",
            Func<HttpRequest, Task<IResult>>(fun request -> task {
                let authorization = string request.Headers.Authorization
                let accessToken = if authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) then authorization.Substring("Bearer ".Length) else ""
                if not (LiveAuthority.authorizeHttp accessToken) then return Results.Unauthorized()
                else
                    let! body = readBoundedBody 1024 request.Body
                    match body with
                    | Error _ -> return Results.BadRequest {| error = "awareness action body too large" |}
                    | Ok value -> return Results.Text(AwarenessReactionDiagnostics.evaluate value, "application/json") })
        ) |> ignore

        app.MapHub<GameHub>("/hub/game") |> ignore
        app.UseDefaultFiles() |> ignore
        app.UseStaticFiles(StaticFileOptions(OnPrepareResponse = Action<StaticFileResponseContext>(configureStaticAssetResponse))) |> ignore
        app.MapFallbackToFile("index.html") |> ignore

    let createApp (args: string array) =
        let builder = WebApplication.CreateBuilder args
        // The hosting diagnostics logger formats the complete request URL before
        // application middleware can run. SignalR permits its bearer token in the
        // query string during a WebSocket upgrade, so do not emit those framework
        // request-start/request-finished messages at Information level.
        builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning) |> ignore
        builder.Services.AddSignalR() |> ignore
        builder.Services
            .AddResponseCompression(fun options ->
                options.EnableForHttps <- true
                options.MimeTypes <-
                    ResponseCompressionDefaults.MimeTypes
                    |> Seq.append [ "application/javascript"; "application/wasm"; "application/json" ])
        |> ignore
        builder.Services.AddSingleton<TimeProvider>(TimeProvider.System) |> ignore
        let jwtIssuer = builder.Configuration["LiveAuthentication:Jwt:Issuer"] |> Option.ofObj |> Option.defaultValue ""
        let jwtAudience = builder.Configuration["LiveAuthentication:Jwt:Audience"] |> Option.ofObj |> Option.defaultValue ""
        let jwtSigningKey = builder.Configuration["LiveAuthentication:Jwt:SigningKey"] |> Option.ofObj |> Option.defaultValue ""
        let unavailableKey = "unconfigured-live-authentication-signing-key"
        builder.Services
            .AddAuthentication("sir-live")
            .AddPolicyScheme(
                "sir-live",
                "Development-only live identity or configured JWT bearer identity",
                (fun options ->
                    options.ForwardDefaultSelector <-
                        Func<HttpContext, string>(fun context ->
                            let developmentAllowed =
                                String.Equals(context.RequestServices.GetRequiredService<Microsoft.Extensions.Hosting.IHostEnvironment>().EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase)
                                && String.Equals(context.RequestServices.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>()["SIR_ALLOW_ANONYMOUS_LIVE_SESSIONS"], "true", StringComparison.OrdinalIgnoreCase)
                            if developmentAllowed then "sir-development" else JwtBearerDefaults.AuthenticationScheme)))
            .AddScheme<AuthenticationSchemeOptions, LiveAuthenticationHandler>("sir-development", null)
            .AddJwtBearer(
                JwtBearerDefaults.AuthenticationScheme,
                (fun options ->
                    options.MapInboundClaims <- false
                    options.TokenValidationParameters <-
                        TokenValidationParameters(
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = SymmetricSecurityKey(Encoding.UTF8.GetBytes(if String.IsNullOrWhiteSpace jwtSigningKey then unavailableKey else jwtSigningKey)),
                            ValidateIssuer = true,
                            ValidIssuer = jwtIssuer,
                            ValidateAudience = true,
                            ValidAudience = jwtAudience,
                            NameClaimType = "sub",
                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.Zero)))
            |> ignore
        builder.Services.AddAuthorization() |> ignore
        let app = builder.Build()
        LiveAuthority.configure (app.Services.GetRequiredService<TimeProvider>()) (TimeSpan.FromMinutes 15.0)
        app.UseAuthentication() |> ignore
        app.UseAuthorization() |> ignore
        app.UseResponseCompression() |> ignore
        mapRoutes app
        app

    [<EntryPoint>]
    let main args =
        let app = createApp args
        app.Run()
        0
