module SIR.Server.Tests

open System
open SIR.Server

let require condition message = if not condition then failwith message

let first = LiveAuthority.bootstrap "alpha" "alpha" |> Result.defaultWith failwith
let second = LiveAuthority.bootstrap "alpha" "alpha" |> Result.defaultWith failwith

require (LiveAuthority.authorize first.AccessToken "old" |> Option.isNone) "rebootstrap must revoke the prior admission"
require (LiveAuthority.authorize second.AccessToken "new" |> Option.isSome) "current admission must authorize"
require (LiveAuthority.authorize "query-only-token" "bad" |> Option.isNone) "unknown/query token must be rejected"

printfn "Live authority revocation and token rejection passed."
