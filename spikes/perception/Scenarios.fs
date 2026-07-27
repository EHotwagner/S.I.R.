module Scenarios

open System
open System.Diagnostics
open World
open Perception

let private rule () = String.replicate 78 "-"

let header (t: string) =
    printfn ""
    printfn "%s" (rule ())
    printfn "%s" t
    printfn "%s" (rule ())

type Stats =
    { Mean: float; P50: float; P95: float; P99: float; Max: float }

let stats (a: float array) =
    let s = Array.sort a
    let pick p = if s.Length = 0 then 0.0 else s.[min (s.Length - 1) (int (p * float s.Length))]
    { Mean = (if s.Length = 0 then 0.0 else Array.average s)
      P50 = pick 0.50; P95 = pick 0.95; P99 = pick 0.99
      Max = (if s.Length = 0 then 0.0 else s.[s.Length - 1]) }

let ms (t: int64) = float t * 1000.0 / float Stopwatch.Frequency

let private jitter (f: Force) (rng: Rng) (g: Grid) =
    // units drift and turn, so the candidate set and ray outcomes change tick
    // to tick rather than measuring one frozen configuration
    for i in 0 .. f.Count - 1 do
        let nx = f.X.[i] + rng.Range(-1, 2)
        let ny = f.Y.[i] + rng.Range(-1, 2)
        if nx > 1 && nx < g.Width - 3 && not (cellBlocked g f.Level.[i] nx ny) then f.X.[i] <- nx
        if ny > 1 && ny < g.Height - 3 && not (cellBlocked g f.Level.[i] f.X.[i] ny) then f.Y.[i] <- ny
        if rng.Chance 5 then f.Facing.[i] <- rng.Range(0, 8)

let private run (g: Grid) (units: int) (levels: int) (sight: int) (samples: int)
                (sector: bool) (bucket: int) (ticks: int) (separation: int) =
    let f = createForce units
    placeInContact g f 4242UL separation
    let ix = createIndex g bucket units
    let acq = createAcquisition units
    let rng = Rng(99UL)
    let c = newCounters ()

    // warm up until tiered JIT has promoted the hot paths; the first scenario
    // otherwise measures tier-0 code and reads high
    for _ in 1 .. 120 do
        rebuildIndex ix f
        step g f ix acq sight samples sector c

    let idxMs = Array.zeroCreate ticks
    let perMs = Array.zeroCreate ticks
    let c = newCounters ()
    let gc0 = GC.CollectionCount(0)
    let alloc0 = GC.GetTotalAllocatedBytes(false)

    for t in 0 .. ticks - 1 do
        jitter f rng g
        let t0 = Stopwatch.GetTimestamp()
        rebuildIndex ix f
        let t1 = Stopwatch.GetTimestamp()
        step g f ix acq sight samples sector c
        let t2 = Stopwatch.GetTimestamp()
        idxMs.[t] <- ms (t1 - t0)
        perMs.[t] <- ms (t2 - t1)

    let allocKb = float (GC.GetTotalAllocatedBytes(false) - alloc0) / 1024.0
    let total = Array.map2 (+) idxMs perMs
    struct (stats idxMs, stats perMs, stats total, c, ticks,
            GC.CollectionCount(0) - gc0, allocKb)

// ---------------------------------------------------------------------------

let mapProfile (g: Grid) =
    header "0. Map profile"
    let struct (cellPct, edgePct) = describe g
    printfn "  grid                     %d x %d, %d level(s)" g.Width g.Height g.Levels
    printfn "  blocking cells           %.2f%%" cellPct
    printfn "  opaque edges             %.2f%%" edgePct
    printfn "  cells total              %d" (g.Width * g.Height * g.Levels)

let baseline (g: Grid) (units: int) (sight: int) (ticks: int) =
    header (sprintf "1. Baseline: %d units in contact, sight %d cells, 4 sample points" units sight)
    let struct (i, p, tot, c, n, gc, kb) = run g units g.Levels sight 4 true 16 ticks 30
    printfn "  spatial index rebuild    mean %6.3f  max %6.3f ms" i.Mean i.Max
    printfn "  perception               mean %6.3f  max %6.3f ms" p.Mean p.Max
    printfn "  TOTAL                    mean %6.3f  p95 %6.3f  p99 %6.3f  max %6.3f ms"
        tot.Mean tot.P95 tot.P99 tot.Max
    printfn ""
    printfn "  budget 50 ms  ->  mean %.1f%%   worst %.1f%%" (tot.Mean / 50.0 * 100.0) (tot.Max / 50.0 * 100.0)
    printfn "  target 20 ms  ->  mean %.1f%%   worst %.1f%%" (tot.Mean / 20.0 * 100.0) (tot.Max / 20.0 * 100.0)
    printfn ""
    printfn "  per tick: candidates %d, in sector %d, rays %d, ray steps %d"
        (c.Candidates / n) (c.SectorPassed / n) (c.Rays / n) (c.RaySteps / n)
    printfn "  acquisitions completed %d, episodes decayed %d (over %d ticks)"
        c.Acquired c.Decayed n
    printfn "  GC gen0 %d, allocated %.1f KB total" gc kb
    tot

let cullingValue (g: Grid) (units: int) (sight: int) =
    header "2. What culling is worth"
    printfn "  %-34s %10s %10s" "configuration" "mean ms" "rays/tick"
    let variants =
        [ "broad phase + sector + 4 samples", 16, true, 4
          "broad phase + sector + 1 sample", 16, true, 1
          "broad phase, no sector, 4 samples", 16, false, 4
          "coarse buckets (64), sector, 4", 64, true, 4
          "no broad phase (bucket = map)", 512, true, 4 ]
    for (label, bucket, sector, samples) in variants do
        let struct (_, p, _, c, n, _, _) = run g units g.Levels sight samples sector bucket 200 30
        printfn "  %-34s %10.3f %10d" label p.Mean (c.Rays / n)

let unitSweep (g: Grid) (sight: int) =
    header "3. Unit-count sweep"
    printfn "  %-8s %10s %10s %10s %12s" "units" "mean ms" "max ms" "% of 50ms" "rays/tick"
    for units in [ 50; 100; 200; 400; 800 ] do
        let struct (_, _, tot, c, n, _, _) = run g units g.Levels sight 4 true 16 200 30
        printfn "  %-8d %10.3f %10.3f %9.1f%% %12d"
            units tot.Mean tot.Max (tot.Max / 50.0 * 100.0) (c.Rays / n)

let sightSweep (g: Grid) (units: int) =
    header "4. Sight-range sweep (0.5 m cells: 60 cells = 30 m)"
    printfn "  %-10s %10s %10s %12s %12s" "range" "mean ms" "max ms" "rays/tick" "steps/tick"
    for sight in [ 20; 40; 60; 100; 160 ] do
        let struct (_, _, tot, c, n, _, _) = run g units g.Levels sight 4 true 16 200 30
        printfn "  %-10d %10.3f %10.3f %12d %12d"
            sight tot.Mean tot.Max (c.Rays / n) (c.RaySteps / n)

let separationSweep (g: Grid) (units: int) (sight: int) =
    header "5. Force separation: how much worse does close contact get?"
    printfn "  %-14s %10s %10s %12s" "separation" "mean ms" "max ms" "rays/tick"
    for sep in [ 150; 90; 50; 30; 15 ] do
        let struct (_, _, tot, c, n, _, _) = run g units g.Levels sight 4 true 16 200 sep
        printfn "  %-14d %10.3f %10.3f %12d" sep tot.Mean tot.Max (c.Rays / n)

let levelSweep (units: int) (sight: int) =
    header "6. Verticality: what a third dimension costs"
    printfn "  %-10s %10s %10s %12s" "levels" "mean ms" "max ms" "rays/tick"
    for levels in [ 1; 2; 3; 4 ] do
        let g = create 512 512 levels
        generateUrban g 7UL
        let struct (_, _, tot, c, n, _, _) = run g units levels sight 4 true 16 200 30
        printfn "  %-10d %10.3f %10.3f %12d" levels tot.Mean tot.Max (c.Rays / n)

let openTerrain (units: int) (sight: int) =
    header "7. Open terrain: no occluders, the worst case for culling"
    let g = create 512 512 1
    let struct (_, _, tot, c, n, _, _) = run g units 1 sight 4 true 16 200 30
    printfn "  mean %.3f ms   max %.3f ms   rays/tick %d   steps/tick %d"
        tot.Mean tot.Max (c.Rays / n) (c.RaySteps / n)
    printfn "  every ray runs to full length because nothing blocks it early."

let sustained (g: Grid) (units: int) (sight: int) (seconds: int) =
    header (sprintf "8. Sustained: %d s of match time at 20 Hz" seconds)
    let ticks = seconds * 20
    let struct (_, _, tot, c, n, gc, kb) = run g units g.Levels sight 4 true 16 ticks 30
    printfn "  tick total               mean %6.3f  p95 %6.3f  p99 %6.3f  max %6.3f ms"
        tot.Mean tot.P95 tot.P99 tot.Max
    printfn "  ticks over 20 ms target  measured across %d ticks" n
    printfn "  GC gen0 %d, allocated %.1f KB total (%.2f KB/tick)" gc kb (kb / float n)
    printfn "  rays/tick %d" (c.Rays / n)
