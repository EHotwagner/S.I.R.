namespace SIR.Server

open System
open System.Security.Claims
open System.Threading.Tasks
open Microsoft.AspNetCore.Authentication
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Options
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open System.Text.Encodings.Web

/// Authentication boundary for bootstrap admission. Development identities are
/// deliberately opt-in; production requires the hosting authentication system
/// to provide an authenticated principal before the endpoint is reached.
type LiveAuthenticationHandler
    (options: IOptionsMonitor<AuthenticationSchemeOptions>, logger: ILoggerFactory, encoder: UrlEncoder) =
    inherit AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)

    override this.HandleAuthenticateAsync() : Task<AuthenticateResult> =
        let context = base.Context
        let request = base.Request
        let scheme = base.Scheme

        task {
            let environment = context.RequestServices.GetRequiredService<IHostEnvironment>()
            let configuration = context.RequestServices.GetRequiredService<IConfiguration>()
            let allowed =
                String.Equals(environment.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase)
                && String.Equals(configuration["SIR_ALLOW_ANONYMOUS_LIVE_SESSIONS"], "true", StringComparison.OrdinalIgnoreCase)
            let actor = string request.Headers["X-SIR-Development-Actor"]

            if allowed && not (String.IsNullOrWhiteSpace actor) then
                let identity = ClaimsIdentity([ Claim(ClaimTypes.Name, actor) ], scheme.Name)
                let ticket = AuthenticationTicket(ClaimsPrincipal identity, scheme.Name)
                return AuthenticateResult.Success ticket
            else
                return AuthenticateResult.NoResult()
        }
