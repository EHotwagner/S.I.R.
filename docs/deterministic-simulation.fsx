(**
---
title: Deterministic simulation evidence
category: Foundations
categoryindex: 2
index: 7
description: Evaluate fixed authoritative integer behavior on .NET and identify the equivalent Fable browser boundary.
---
*)

(**
# Deterministic simulation evidence

This page is evaluated by FSharp.Formatting on **.NET** during the strict
documentation build. The interactive application uses the same domain source
compiled by Fable, but browser output is explicitly identified on the
[interactive rules laboratory](interactive-rules-lab.md) page.

The authoritative numeric layer uses bounded integers and four-place
fixed-point values. Neither example depends on a process clock, random ambient
state, or floating-point presentation.
*)

(*** condition: prepare ***)
#r "../src/SIR.Domain/bin/Release/net10.0/SIR.Domain.dll"

open SIR.Domain

(**
## Saturating bounded arithmetic

Construction validates both the inclusive range and the value. Addition uses a
wide intermediate and saturates at the declared maximum.
*)

let required result =
    match result with
    | Ok value -> value
    | Error error -> failwithf "Unexpected invalid fixture: %A" error

let current = BoundedInt32.create 0 100 90 |> required
let increase = BoundedInt32.create 0 100 25 |> required

let saturated =
    BoundedInt32.addSaturating current increase
    |> required
    |> BoundedInt32.value

printfn "Runtime: .NET build-time evaluation"
printfn "90 + 25 in [0, 100] = %d" saturated
(*** include-output ***)

(**
## Explicit fixed-point rounding

`FixedPoint` stores four base-ten places as a signed integer. Division rounds
to nearest with midpoint ties away from zero, so the canonical raw value is
portable across the .NET and Fable builds.
*)

let oneSixth =
    FixedPoint.fromRatio 1 6
    |> required
    |> FixedPoint.raw

printfn "1 / 6 at scale 10,000 has canonical raw value %d" oneSixth
(*** include-output ***)

(**
## Why this is fixed evidence

These values are documentation evidence, not live balance inputs. Editing a
browser parameter creates a derived sandbox run with its own identity. See the
[interactive replay and rules laboratory](interactive-rules-lab.md), the
[numeric architecture](fable-client-and-documentation.md#authoritative-numeric-contract),
and the generated [API reference](reference/index.html).
*)
