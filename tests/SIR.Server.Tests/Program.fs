module SIR.Server.Tests

open System
open System.IO
open System.Net
open System.Net.Http
open System.Text
open System.Text.Json
open System.Security.Cryptography
open System.Collections.Concurrent
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Mvc.Testing
open Microsoft.Extensions.Logging
open SIR.Protocol.Http
open SIR.Server
open SIR.Simulation
open Xunit

let require condition message = if not condition then failwith message

type MutableTimeProvider(initial: DateTimeOffset) =
    inherit TimeProvider()
    let mutable current = initial
    member _.Advance(by: TimeSpan) = current <- current.Add by
    override _.GetUtcNow() = current

type CapturingLoggerProvider() =
    let messages = ConcurrentQueue<string>()
    member _.Messages = messages.ToArray()
    interface ILoggerProvider with
        member _.CreateLogger(_categoryName) =
            { new ILogger with
                member _.BeginScope<'TState when 'TState : not null>(_state: 'TState) : IDisposable =
                    System.Threading.CancellationToken.None.Register(fun () -> ()) :> IDisposable
                member _.IsEnabled(_logLevel) = true
                member _.Log<'TState>(_logLevel, _eventId, state: 'TState, exn, formatter) =
                    messages.Enqueue(formatter.Invoke(state, exn)) }
    interface IDisposable with
        member _.Dispose() = ()

type ServerFactory(loggerProvider: ILoggerProvider option) =
    inherit WebApplicationFactory<SIR.Server.Program>()
    override _.ConfigureWebHost(builder) =
        builder.UseContentRoot(Path.GetFullPath("../../src/SIR.Server", __SOURCE_DIRECTORY__)) |> ignore
        loggerProvider
        |> Option.iter (fun provider ->
            builder.ConfigureLogging(fun logging ->
                logging.ClearProviders() |> ignore
                logging.SetMinimumLevel(LogLevel.Trace) |> ignore
                logging.AddProvider(provider) |> ignore
                ())
            |> ignore)

let base64Url (bytes: byte array) = Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')

let productionToken actor =
    let encode (text: string) = Encoding.UTF8.GetBytes(text) |> base64Url
    let header = encode "{\"alg\":\"HS256\",\"typ\":\"JWT\"}"
    let payload = encode $"{{\"iss\":\"sir-tests\",\"aud\":\"sir-live-tests\",\"sub\":\"{actor}\",\"exp\":{DateTimeOffset.UtcNow.AddMinutes(10.0).ToUnixTimeSeconds()}}}"
    let unsignedToken = $"{header}.{payload}"
    use hmac = new HMACSHA256(Encoding.UTF8.GetBytes("sir-tests-signing-key-must-be-at-least-32-bytes"))
    $"{unsignedToken}.{hmac.ComputeHash(Encoding.UTF8.GetBytes(unsignedToken)) |> base64Url}"

let post (client: HttpClient) actor (bearer: string option) (spoofedHeader: string option) (developmentHeader: string option) =
    let request = new HttpRequestMessage(HttpMethod.Post, "/api/bootstrap?access_token=query-only-token")
    request.Content <- new StringContent(BootstrapV1.encodeRequest { Version = 1; ActorName = actor }, Encoding.UTF8, "application/json")
    bearer |> Option.iter (fun value -> request.Headers.Authorization <- Headers.AuthenticationHeaderValue("Bearer", value))
    spoofedHeader |> Option.iter (fun value -> request.Headers.Add("X-SIR-Authenticated-Actor", value))
    developmentHeader |> Option.iter (fun value -> request.Headers.Add("X-SIR-Development-Actor", value))
    client.SendAsync(request).GetAwaiter().GetResult()

let withProductionClient assertion =
    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production")
    Environment.SetEnvironmentVariable("LiveAuthentication__Jwt__Issuer", "sir-tests")
    Environment.SetEnvironmentVariable("LiveAuthentication__Jwt__Audience", "sir-live-tests")
    Environment.SetEnvironmentVariable("LiveAuthentication__Jwt__SigningKey", "sir-tests-signing-key-must-be-at-least-32-bytes")
    use factory = new ServerFactory(None)
    use client = factory.CreateClient()
    assertion client

let spatialRequest width height =
    $"""{{"MapIdentity":"server-test-map","SpatialRevision":7,"Width":{width},"Height":{height},"OriginColumn":1,"OriginRow":1,"UnitSize":1,"Facing":2,"Terrain":[{{"Column":2,"Row":1,"Kind":1}}]}}"""

let postSpatial (client: HttpClient) bearer body =
    let request = new HttpRequestMessage(HttpMethod.Post, "/api/spatial/diagnostics")
    request.Content <- new StringContent(body, Encoding.UTF8, "application/json")
    bearer |> Option.iter (fun value -> request.Headers.Authorization <- Headers.AuthenticationHeaderValue("Bearer", value))
    client.SendAsync(request).GetAwaiter().GetResult()

let postPhysicalCombat (client: HttpClient) bearer body =
    let request = new HttpRequestMessage(HttpMethod.Post, "/api/combat/physical-drill")
    request.Content <- new StringContent(body, Encoding.UTF8, "application/json")
    bearer |> Option.iter (fun value -> request.Headers.Authorization <- Headers.AuthenticationHeaderValue("Bearer", value))
    client.SendAsync(request).GetAwaiter().GetResult()

let admittedSession (client: HttpClient) actor =
    use response = post client actor (Some(productionToken actor)) None None
    require (response.StatusCode = HttpStatusCode.OK) "the production identity must obtain a live-session admission"
    response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
    |> BootstrapV1.responseFromJson
    |> Result.defaultWith failwith

type LiveSessionAuthenticationTests() =
    [<Fact>]
    member _.``physical combat drill requires admission and returns four-profile replay projection``() =
        withProductionClient (fun client ->
            use unauthorized = postPhysicalCombat client None "{\"AttackId\":\"unauthorized\",\"Scenario\":\"four-profile-cover-replay-v1\"}"
            require (unauthorized.StatusCode = HttpStatusCode.Unauthorized) "physical combat must reject an absent bearer admission"
            let admission = admittedSession client "combat-player"
            use response = postPhysicalCombat client (Some admission.AccessToken) "{\"AttackId\":\"server-combat-test\",\"Scenario\":\"four-profile-cover-replay-v1\"}"
            let body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
            require (response.StatusCode = HttpStatusCode.OK) "authorized physical combat must resolve"
            let projection =
                JsonSerializer.Deserialize<PhysicalCombatResponseDto>(body)
                |> box
                |> function null -> failwith "physical combat must return its typed projection" | value -> unbox<PhysicalCombatResponseDto> value
            require (projection.Scenario = "four-profile-cover-replay-v1") "the bounded scenario identity changed"
            require (projection.Profiles |> Array.map _.Profile = [| "Rifle"; "SupportWeapon"; "AntiArmor"; "LobbedArea" |]) "the real entry path must exercise all four profiles in canonical order"
            require (projection.Profiles |> Array.forall (fun profile -> profile.CanonicalByteCount > 0)) "every profile must retain canonical authority evidence"
            require (projection.Profiles |> Array.filter (fun profile -> profile.Profile <> "LobbedArea") |> Array.forall (fun profile -> profile.Trace.Length >= 2)) "every direct-fire profile must disclose its physical trace"
            require (projection.InitialCoverIntegrity = 50 && projection.FinalCoverIntegrity = 0 && projection.CoverDestroyed) "the scenario must expose cover degradation and destruction"
            let integrity = projection.Profiles |> Array.map (fun profile -> profile.CoverIntegrityBefore, profile.CoverIntegrityAfter)
            let integrityText = integrity |> Array.map (fun (before, after) -> $"{before}->{after}") |> String.concat ","
            require (integrity = [| (50, 38); (38, 38); (38, 13); (13, 0) |]) $"per-profile cover integrity changed: {integrityText}"
            let antiArmor = projection.Profiles |> Array.find (fun profile -> profile.Profile = "AntiArmor")
            require (antiArmor.CoverSource = "roadblock-2" && antiArmor.CoverRetainedPercent = 50) "anti-armor cover projection changed"
            require (antiArmor.ArmorArc = "Front" && antiArmor.ArmorRating = 50 && antiArmor.ArmorRetainedPercent = 100 && antiArmor.RemainingHealth = 67) "anti-armor directional armor and HP projection changed"
            let steps = antiArmor.Facts |> Array.map _.Step
            let index step = steps |> Array.findIndex ((=) step)
            let stepsText = String.concat "," steps
            require (index "Physical trace" < index "Cover" && index "Cover" < index "Armor" && index "Armor" < index "HP" && index "HP" < index "Suppression") $"authority facts lost canonical consequence ordering: {stepsText}"
            require (projection.Replay.FormatVersion = Replay.CurrentFormatVersion && projection.Replay.Verified) "the scenario must run replay verification"
            require (projection.Replay.SeekPointsVerified = 4 && projection.Replay.FinalTick = 4 && projection.Replay.FinalStateHash.Length = 64) "replay evidence must cover the initial snapshot and three retained seek points")

    [<Fact>]
    member _.``physical combat drill rejects unknown scenarios before evaluation``() =
        withProductionClient (fun client ->
            let admission = admittedSession client "combat-bounds"
            use response = postPhysicalCombat client (Some admission.AccessToken) "{\"AttackId\":\"invalid-scenario\",\"Scenario\":\"unbounded-skirmish\"}"
            require (response.StatusCode = HttpStatusCode.BadRequest) "unknown scenarios must fail closed")

    [<Fact>]
    member _.``spatial diagnostics require identity and return bounded authoritative projection``() =
        withProductionClient (fun client ->
            let unauthorized = postSpatial client None (spatialRequest 8 8)
            require (unauthorized.StatusCode = HttpStatusCode.Unauthorized) "spatial diagnostics must reject an absent bearer identity"
            let admission = admittedSession client "spatial-player"
            let response = postSpatial client (Some admission.AccessToken) (spatialRequest 8 8)
            let body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
            require (response.StatusCode = HttpStatusCode.OK) $"authorized spatial diagnostics must succeed (got {response.StatusCode})"
            let projection =
                JsonSerializer.Deserialize<SpatialDiagnosticResponseDto>(body)
                |> box
                |> function
                    | null -> failwith "the endpoint must return a typed spatial diagnostic projection"
                    | value -> unbox<SpatialDiagnosticResponseDto> value
            let route = projection.Queries |> Array.find (fun query -> query.QueryKind = "BoundedPath")
            require (route.Outcome = "Found" && route.Path.Length >= 2) "the endpoint must return the complete non-empty authoritative path for a found bounded route"
            require (route.Path[0].Column = route.Origin.Column && route.Path[0].Row = route.Origin.Row) "the authoritative path must begin at the normalized origin"
            let destination = route.Path[route.Path.Length - 1]
            require (destination.Column = route.Target.Column && destination.Row = route.Target.Row) "the authoritative path must end at the normalized target"
            require (body.Contains("ExactLineOfSight") && body.Contains("BoundedPath") && body.Contains("Cover")) "the endpoint must return LOS, route, and cover projections"
            require (body.Contains("\"Origin\"") && body.Contains("\"Target\"") && body.Contains("\"FootprintSamples\"") && body.Contains("\"Path\"")) "the endpoint must return exact normalized input and path fields"
            require (body.Contains("\"CrossedCells\"") && body.Contains("\"CrossedEdges\"") && body.Contains("\"CoverContributors\"") && body.Contains("\"Decisions\"")) "the endpoint must return exact authoritative explanation fields"
            require (body.Contains("\"Expansions\"") && body.Contains("\"Truncated\"") && body.Contains("SIR.Simulation.SpatialQuery.evaluate") && body.Contains("player-disclosed")) "the endpoint must return bounded authority and knowledge identity fields")

    [<Fact>]
    member _.``spatial diagnostics reject invalid dimensions before evaluation``() =
        withProductionClient (fun client ->
            let admission = admittedSession client "spatial-bounds"
            let response = postSpatial client (Some admission.AccessToken) (spatialRequest 81 8)
            let body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
            require (response.StatusCode = HttpStatusCode.BadRequest && body.Contains("invalid spatial diagnostic dimensions")) "the endpoint must enforce the declared map bound")

    [<Fact>]
    member _.``production rejects absent bearer identity``() =
        withProductionClient (fun client ->
            let response = post client "proxy-player" None None None
            require (response.StatusCode = HttpStatusCode.Unauthorized) "production bootstrap must reject an absent bearer identity")

    [<Fact>]
    member _.``production rejects development identity header``() =
        withProductionClient (fun client ->
            let response = post client "proxy-player" None None (Some "proxy-player")
            require (response.StatusCode = HttpStatusCode.Unauthorized) "production bootstrap must reject the development identity header")

    [<Fact>]
    member _.``production rejects spoofed header and admits signed identity``() =
        withProductionClient (fun client ->
            let spoofed = post client "proxy-player" None (Some "proxy-player") None
            require (spoofed.StatusCode = HttpStatusCode.Unauthorized) "direct callers must not forge an actor with the retired proxy header"
            let response = post client "proxy-player" (Some(productionToken "proxy-player")) None None
            require (response.StatusCode = HttpStatusCode.OK) $"a configured signed identity must admit the matching actor (got {response.StatusCode})")

    [<Fact>]
    member _.``framework diagnostics suppress rejected query credentials``() =
        let querySecret = "rejected-access-token-must-never-be-logged"
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production")
        Environment.SetEnvironmentVariable("LiveAuthentication__Jwt__Issuer", "sir-tests")
        Environment.SetEnvironmentVariable("LiveAuthentication__Jwt__Audience", "sir-live-tests")
        Environment.SetEnvironmentVariable("LiveAuthentication__Jwt__SigningKey", "sir-tests-signing-key-must-be-at-least-32-bytes")
        use logs = new CapturingLoggerProvider()
        use factory = new ServerFactory(Some logs)
        use client = factory.CreateClient()
        let request = new HttpRequestMessage(HttpMethod.Post, $"/api/bootstrap?access_token={querySecret}")
        request.Content <- new StringContent(BootstrapV1.encodeRequest { Version = 1; ActorName = "query-player" }, Encoding.UTF8, "application/json")
        let response = client.SendAsync(request).GetAwaiter().GetResult()
        require (response.StatusCode = HttpStatusCode.Unauthorized) "query-only access tokens must not authenticate bootstrap"
        let captured = String.concat "\n" logs.Messages
        require (captured.IndexOf(querySecret, StringComparison.Ordinal) < 0) "framework logs must never contain a rejected query access token"
        require (captured.IndexOf("Request starting", StringComparison.Ordinal) < 0 && captured.IndexOf("Request finished", StringComparison.Ordinal) < 0) "framework request diagnostics must be suppressed before they can format query credentials"

    [<Fact>]
    member _.``bootstrap rejects cross actor identity``() =
        withProductionClient (fun client ->
            let response = post client "other-player" (Some(productionToken "proxy-player")) None None
            require (response.StatusCode = HttpStatusCode.BadRequest) "authenticated principals must not bootstrap another actor")

    [<Fact>]
    member _.``chunked bootstrap body is rejected before parsing``() =
        withProductionClient (fun client ->
            let request = new HttpRequestMessage(HttpMethod.Post, "/api/bootstrap")
            request.Headers.Authorization <- Headers.AuthenticationHeaderValue("Bearer", productionToken "chunked")
            let content = new StringContent(String.replicate 17000 "x", Encoding.UTF8, "application/json")
            content.Headers.ContentLength <- Nullable()
            request.Content <- content
            let response = client.SendAsync(request).GetAwaiter().GetResult()
            let body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
            require (response.StatusCode = HttpStatusCode.BadRequest && body.Contains("SIR.LIVE.BOOTSTRAP.BODY_TOO_LARGE")) "chunked oversized bootstrap bodies must be refused by the streaming bound before parsing")

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
        require (LiveAuthority.activeSessionCount() = 0) "expired sessions must release their process record"

    [<Fact>]
    member _.``bootstrap bounds actor names and global session admission``() =
        let clock = MutableTimeProvider(DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero))
        LiveAuthority.configure clock (TimeSpan.FromMinutes 1.0)
        require (LiveAuthority.bootstrap "alpha" (String.replicate 129 "a") = Error "SIR.LIVE.BOOTSTRAP.ACTOR_REQUIRED") "oversized actor names must be rejected before session allocation"
        for index in 1 .. 64 do
            let actor = $"player-{index}"
            LiveAuthority.bootstrap actor actor |> Result.defaultWith failwith |> ignore
        require (LiveAuthority.bootstrap "overflow" "overflow" = Error "SIR.LIVE.BOOTSTRAP.CAPACITY_REJECTED") "global admission must reject capacity overflow"

    [<Fact>]
    member _.``bootstrap rate limiting and lifecycle metrics are observable``() =
        let clock = MutableTimeProvider(DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero))
        LiveAuthority.configure clock (TimeSpan.FromMinutes 1.0)
        for _ in 1 .. 8 do LiveAuthority.bootstrap "rate-player" "rate-player" |> Result.defaultWith failwith |> ignore
        require (LiveAuthority.bootstrap "rate-player" "rate-player" = Error "SIR.LIVE.BOOTSTRAP.RATE_REJECTED") "per-principal admission floods must be rate rejected"
        let snapshot = LiveAuthority.metrics()
        require (snapshot.RejectedAdmissions = 1 && snapshot.Evictions = 7) "metrics must expose rejected admissions and superseded-session evictions"

    [<Fact>]
    member _.``concurrent admissions never exceed capacity``() =
        let clock = MutableTimeProvider(DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero))
        LiveAuthority.configure clock (TimeSpan.FromHours 1.0)
        let admitted =
            [| 1 .. 256 |]
            |> Array.Parallel.map (fun index -> let actor = $"parallel-{index}" in LiveAuthority.bootstrap actor actor |> Result.isOk)
            |> Array.filter id
            |> Array.length
        require (admitted = 64 && LiveAuthority.activeSessionCount() = 64) "global capacity must remain atomic under concurrent admission"

    [<Fact>]
    member _.``disconnect grace expires and releases a session``() =
        let clock = MutableTimeProvider(DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero))
        LiveAuthority.configure clock (TimeSpan.FromHours 1.0)
        let session = LiveAuthority.bootstrap "grace" "grace" |> Result.defaultWith failwith
        LiveAuthority.authorize session.AccessToken "connection" |> ignore
        LiveAuthority.disconnected session.SessionId "connection"
        clock.Advance(TimeSpan.FromMinutes 3.0)
        require (LiveAuthority.advance session.SessionId session.ActorId session.AccessToken "connection" 1 |> Option.isNone) "expired disconnect grace must return the stable unknown-session result"
        require (LiveAuthority.activeSessionCount() = 0 && (LiveAuthority.metrics()).Expiries = 1) "disconnect grace expiry must release the session record and increment expiry metrics"

    [<Fact>]
    member _.``superseded admission releases capacity with stable denial``() =
        let clock = MutableTimeProvider(DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero))
        LiveAuthority.configure clock (TimeSpan.FromHours 1.0)
        let first = LiveAuthority.bootstrap "evict" "evict" |> Result.defaultWith failwith
        let second = LiveAuthority.bootstrap "evict" "evict" |> Result.defaultWith failwith
        require (LiveAuthority.activeSessionCount() = 1 && (LiveAuthority.metrics()).Evictions = 1) "superseded admissions must remove their prior record"
        require (LiveAuthority.authorize first.AccessToken "old" |> Option.isNone) "evicted credentials must receive the stable authorization denial"
        require (LiveAuthority.authorize second.AccessToken "new" |> Option.isSome) "replacement admission must remain usable"

    [<Fact>]
    member _.``independent sessions mutate concurrently``() =
        let clock = MutableTimeProvider(DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero))
        LiveAuthority.configure clock (TimeSpan.FromHours 1.0)
        let left = LiveAuthority.bootstrap "left" "left" |> Result.defaultWith failwith
        let right = LiveAuthority.bootstrap "right" "right" |> Result.defaultWith failwith
        LiveAuthority.authorize left.AccessToken "left-c" |> ignore
        LiveAuthority.authorize right.AccessToken "right-c" |> ignore
        require (LiveAuthority.independentSessionGates left.SessionId right.SessionId) "independent sessions must retain distinct synchronization gates"
        let results = [| left, "left-c"; right, "right-c" |] |> Array.Parallel.map (fun (session, connection) -> LiveAuthority.advance session.SessionId session.ActorId session.AccessToken connection 1 |> Option.isSome)
        require (results = [| true; true |]) "independent sessions must mutate concurrently without a shared authority lock"

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
