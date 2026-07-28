module Scenarios

open System
open Domain
open Combat
open Catalog

let private rule () = String.replicate 78 "-"

let private header title =
    printfn ""
    printfn "%s" (rule ())
    printfn "%s" title
    printfn "%s" (rule ())

let private finite value =
    if Double.IsPositiveInfinity value then "   immune"
    else sprintf "%7.2fs" value

let engagementCurves () =
    header "1. Engagement-time curves on a fully exposed target"
    printfn "  %-20s %8s %8s %8s %8s %8s" "weapon" "8 m" "20 m" "35 m" "50 m" "65 m"
    for weapon in [ carbine; rifle; marksmanRifle ] do
        let values =
            [ 8.0; 20.0; 35.0; 50.0; 65.0 ]
            |> List.map (fun range ->
                state "curve" goblin Bearing.Front range 1.0 0.0
                |> engagementSeconds parameters weapon)
        printfn "  %-20s %8.2f %8.2f %8.2f %8.2f %8.2f"
            weapon.Name values.[0] values.[1] values.[2] values.[3] values.[4]

let directionalArmour () =
    header "2. Fixed board: rifle against a shielded orc at 25 m"
    printfn "  %-10s %12s %14s %12s %12s" "bearing" "armour" "outcome" "damage/shot" "time to 0 HP"
    for bearing in [ Bearing.Front; Bearing.Flank; Bearing.Rear ] do
        let board = state "orc facing" orc bearing 25.0 1.0 0.0
        let protection = armourAt bearing orc.Armour
        printfn "  %-10A %12.1f %14A %12.2f %12s"
            bearing
            protection
            (armourOutcome rifle.Penetration protection)
            (expectedDamagePerShot rifle board)
            (finite (expectedTimeToIncapacitation parameters rifle board))

let exposureAndPeeking () =
    header "3. Fixed board: exposure, hard cover, and short peeks"
    let openTarget = state "open" orc Bearing.Front 45.0 1.0 0.0
    let partialTarget = state "partial" orc Bearing.Front 45.0 0.30 45.0
    printfn "  %-22s %12s %12s %12s" "state" "engage" "damage/shot" "shots in 0.75s"
    for label, board in [ "fully exposed", openTarget; "30% + hard cover", partialTarget ] do
        printfn "  %-22s %11.2fs %12.2f %12d"
            label
            (engagementSeconds parameters marksmanRifle board)
            (expectedDamagePerShot marksmanRifle board)
            (shotsResolved parameters marksmanRifle board 0.75)
    printfn ""
    printfn "  A 0.75 s peek defeats the current marksman preparation at this range;"
    printfn "  a committed 2.5 s exposure permits %d shot(s)."
        (shotsResolved parameters marksmanRifle openTarget 2.5)

let areaSuppression samples =
    header "4. Fixed board: three goblins crossing a held area for 3 s"
    let crossing = state "crossing" goblin Bearing.Front 35.0 1.0 0.0
    let result = sample parameters supportWeapon crossing 3.0 samples 0x51A7UL
    printfn "  samples                  %d" samples
    printfn "  damage per goblin        mean %6.1f  p10 %6.1f  p50 %6.1f  p90 %6.1f"
        result.MeanDamage result.P10Damage result.P50Damage result.P90Damage
    printfn "  incapacitation           %6.1f%%" result.IncapacitationPercent
    printfn "  suppression per goblin   %6.1f / %.1f threshold"
        result.MeanSuppression parameters.SuppressionThreshold
    printfn "  total expected damage across three occupants: %6.1f"
        (3.0 * result.MeanDamage)
    printfn "  point-rifle damage rate against one goblin:    %6.1f HP/s"
        (expectedDamagePerSecond rifle crossing)

let trollComparison samples =
    header "5. Fixed board: armoured troll advancing frontally at 30 m for 8 s"
    let board = state "troll advance" troll Bearing.Front 30.0 1.0 0.0
    printfn "  %-22s %12s %12s %12s %12s" "weapon" "mean damage" "p10" "p90" "incap."
    for weapon in [ rifle; supportWeapon; marksmanRifle; antiArmourLauncher ] do
        let result = sample parameters weapon board 8.0 samples (uint64 weapon.Name.Length * 7717UL)
        printfn "  %-22s %12.1f %12.1f %12.1f %11.1f%%"
            weapon.Name result.MeanDamage result.P10Damage result.P90Damage result.IncapacitationPercent

let armourSweep () =
    header "6. Parameter sweep: orc frontal armour against a rifle at 25 m"
    printfn "  %-10s %16s %14s %14s" "armour" "nominal outcome" "retained effect" "time to 0 HP"
    for protection in 20 .. 2 .. 50 do
        let body = withFrontArmour (float protection) orc
        let board = state "sweep" body Bearing.Front 25.0 1.0 0.0
        printfn "  %-10d %16A %13.1f%% %14s"
            protection
            (armourOutcome rifle.Penetration (float protection))
            (100.0 * retainedEffect rifle.Penetration (float protection))
            (finite (expectedTimeToIncapacitation parameters rifle board))

let regenerationSweep () =
    header "7. Parameter sweep: troll regeneration and frontal fire at 30 m"
    printfn "  %-8s %14s %14s %14s" "regen" "rifle" "marksman" "anti-armour"
    for regeneration in 0 .. 2 .. 12 do
        let body = { troll with RegenerationPerSecond = float regeneration }
        let board = state "regen sweep" body Bearing.Front 30.0 1.0 0.0
        printfn "  %-8d %14s %14s %14s"
            regeneration
            (finite (expectedTimeToIncapacitation parameters rifle board))
            (finite (expectedTimeToIncapacitation parameters marksmanRifle board))
            (finite (expectedTimeToIncapacitation parameters antiArmourLauncher board))

type Check =
    { Name: string
      Passed: bool
      Detail: string }

let checks () =
    header "8. Qualitative invariant checks"
    let orcFront = state "front" orc Bearing.Front 25.0 1.0 0.0
    let orcFlank = { orcFront with Bearing = Bearing.Flank }
    let orcRear = { orcFront with Bearing = Bearing.Rear }
    let goblinCrossing = state "crossing" goblin Bearing.Front 35.0 1.0 0.0
    let trollFront = state "troll" troll Bearing.Front 30.0 1.0 0.0
    let openMarksman = state "peek" orc Bearing.Front 45.0 1.0 0.0
    let coveredMarksman = state "covered" orc Bearing.Front 45.0 0.30 45.0
    let close = state "close" goblin Bearing.Front 8.0 1.0 0.0
    let far = state "far" goblin Bearing.Front 65.0 1.0 0.0
    let activeSupport =
        3.0 - engagementSeconds parameters supportWeapon goblinCrossing

    let results =
        [ { Name = "flanking beats frontal armour"
            Passed = (expectedDamagePerShot rifle orcFlank > expectedDamagePerShot rifle orcFront)
            Detail = "rifle damage per shot" }
          { Name = "rear is no safer than flank"
            Passed = (expectedDamagePerShot rifle orcRear >= expectedDamagePerShot rifle orcFlank)
            Detail = "directional armour ordering" }
          { Name = "short peek defeats marksman preparation"
            Passed = shotsResolved parameters marksmanRifle openMarksman 0.75 = 0
            Detail = "0.75 s exposure at 45 m" }
          { Name = "committed exposure permits precision fire"
            Passed = shotsResolved parameters marksmanRifle openMarksman 2.5 > 0
            Detail = "2.5 s exposure at 45 m" }
          { Name = "cover buys preparation time"
            Passed = (engagementSeconds parameters marksmanRifle coveredMarksman > engagementSeconds parameters marksmanRifle openMarksman)
            Detail = "30% exposure versus open" }
          { Name = "cover also reduces resolved damage"
            Passed = (expectedDamagePerShot marksmanRifle coveredMarksman < expectedDamagePerShot marksmanRifle openMarksman)
            Detail = "separate trace-layer effect" }
          { Name = "carbine prepares faster at close range"
            Passed = (engagementSeconds parameters carbine close < engagementSeconds parameters rifle close)
            Detail = "8 m comparison" }
          { Name = "rifle prepares faster at long range"
            Passed = (engagementSeconds parameters rifle far < engagementSeconds parameters carbine far)
            Detail = "65 m comparison" }
          { Name = "held area suppresses a crossing goblin"
            Passed = (expectedSuppression parameters supportWeapon goblinCrossing activeSupport >= parameters.SuppressionThreshold)
            Detail = "3 s area crossing" }
          { Name = "support fire is less lethal per individual"
            Passed = (expectedDamagePerSecond supportWeapon goblinCrossing < expectedDamagePerSecond rifle goblinCrossing)
            Detail = "area versus point damage rate" }
          { Name = "troll regeneration resists frontal rifle fire"
            Passed = (expectedDamagePerSecond rifle trollFront <= troll.RegenerationPerSecond)
            Detail = "one rifle versus armoured front" }
          { Name = "anti-armour overcomes troll regeneration"
            Passed = (expectedDamagePerSecond antiArmourLauncher trollFront > troll.RegenerationPerSecond)
            Detail = "dedicated counter" } ]

    for result in results do
        printfn "  [%s] %-48s %s"
            (if result.Passed then "PASS" else "FAIL")
            result.Name
            result.Detail

    results

let run quick =
    let samples = if quick then 2_000 else 25_000
    engagementCurves ()
    directionalArmour ()
    exposureAndPeeking ()
    areaSuppression samples
    trollComparison samples
    armourSweep ()
    regenerationSweep ()
    checks ()
