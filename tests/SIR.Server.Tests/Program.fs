module SIR.Server.Tests

open System
open System.IO
open System.Net
open System.Net.Http
open System.Text
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Mvc.Testing
open SIR.Protocol.Http
open SIR.Server
open Xunit

let require condition message = if not condition then failwith message

type MutableTimeProvider(initial: DateTimeOffset) =
    inherit TimeProvider()
    let mutable current = initial
    member _.Advance(by: TimeSpan) = current <- current.Add by
    override _.GetUtcNow() = current

type ServerFactory() =
    inherit WebApplicationFactory<SIR.Server.Program>()
    override _.ConfigureWebHost(builder) =
        builder.UseContentRoot(Path.GetFullPath("../../src/SIR.Server", __SOURCE_DIRECTORY__)) |> ignore

let post (client: HttpClient) actor (header: string option) (developmentHeader: string option) =
    let request = new HttpRequestMessage(HttpMethod.Post, "/api/bootstrap?access_token=query-only-token")
    request.Content <- new StringContent(BootstrapV1.encodeRequest { Version = 1; ActorName = actor }, Encoding.UTF8, "application/json")
    header |> Option.iter (fun value -> request.Headers.Add("X-SIR-Authenticated-Actor", value))
    developmentHeader |> Option.iter (fun value -> request.Headers.Add("X-SIR-Development-Actor", value))
    client.SendAsync(request).GetAwaiter().GetResult()

let withProductionClient assertion =
    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production")
    Environment.SetEnvironmentVariable("LiveAuthentication__TrustedProxyActorHeader", "true")
    use factory = new ServerFactory()
    use client = factory.CreateClient()
    assertion client

type LiveSessionAuthenticationTests() =
    [<Fact>]
    member _.``production rejects absent trusted proxy identity``() =
        withProductionClient (fun client ->
            let response = post client "proxy-player" None None
            require (response.StatusCode = HttpStatusCode.Unauthorized) "production bootstrap must reject an absent trusted-proxy identity")

    [<Fact>]
    member _.``production rejects development identity header``() =
        withProductionClient (fun client ->
            let response = post client "proxy-player" None (Some "proxy-player")
            require (response.StatusCode = HttpStatusCode.Unauthorized) "production bootstrap must reject the development identity header")

    [<Fact>]
    member _.``production admits configured matching proxy identity``() =
        withProductionClient (fun client ->
            let response = post client "proxy-player" (Some "proxy-player") None
            require (response.StatusCode = HttpStatusCode.OK) "configured trusted-proxy identity must admit the matching actor")

    [<Fact>]
    member _.``bootstrap rejects cross actor identity``() =
        withProductionClient (fun client ->
            let response = post client "other-player" (Some "proxy-player") None
            require (response.StatusCode = HttpStatusCode.BadRequest) "authenticated principals must not bootstrap another actor")

    [<Fact>]
    member _.``query only credentials and revoked sessions are denied``() =
        let clock = MutableTimeProvider(DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero))
        LiveAuthority.configure clock (TimeSpan.FromMinutes 1.0)
        let first = LiveAuthority.bootstrap "alpha" "alpha" |> Result.defaultWith failwith
        let second = LiveAuthority.bootstrap "alpha" "alpha" |> Result.defaultWith failwith
        require (LiveAuthority.authorize first.AccessToken "old" |> Option.isNone) "rebootstrap must revoke the prior admission"
        require (LiveAuthority.authorize "query-only-token" "bad" |> Option.isNone) "query-string credentials are never accepted as bearer metadata"
        require (LiveAuthority.authorize second.AccessToken "current" |> Option.isSome) "current admission must authorize"

    [<Fact>]
    member _.``injected clock expires admission``() =
        let clock = MutableTimeProvider(DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero))
        LiveAuthority.configure clock (TimeSpan.FromMinutes 1.0)
        let session = LiveAuthority.bootstrap "expiry-player" "expiry-player" |> Result.defaultWith failwith
        clock.Advance(TimeSpan.FromMinutes 2.0)
        require (LiveAuthority.authorize session.AccessToken "expired" |> Option.isNone) "the injected TimeProvider must expire admissions"

    [<Fact>]
    member _.``takeover rejects superseded connection and keeps deterministic work``() =
        let clock = MutableTimeProvider(DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero))
        LiveAuthority.configure clock (TimeSpan.FromMinutes 1.0)
        let replay = LiveAuthority.bootstrap "bravo" "bravo" |> Result.defaultWith failwith
        LiveAuthority.resetStructuralCounters()
        require (LiveAuthority.authorize replay.AccessToken "first-connection" |> Option.isSome) "initial connection must authorize"
        require (LiveAuthority.authorize replay.AccessToken "current-connection" |> Option.isSome) "current connection must take over"
        require (LiveAuthority.advance replay.SessionId replay.ActorId replay.AccessToken "first-connection" 1 |> Option.isNone) "superseded connections must not advance live state"
        require (LiveAuthority.reconnect replay.SessionId replay.ActorId replay.AccessToken "current-connection" 0 0 |> Result.isOk) "current connection must reconnect and resync"
        let tokenValidations, sessionLookups = LiveAuthority.structuralCounters()
        require (tokenValidations = 4 && sessionLookups = 4) "LongPolling baseline must preserve one token validation and session lookup per connection or intent"

[<EntryPoint>]
let main _ = 0
