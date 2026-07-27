module Scenarios

open System
open System.Diagnostics
open World
open Clearance
open Movement

let private rule () = String.replicate 78 "-"

let header (t: string) =
    printfn ""
    printfn "%s" (rule ())
    printfn "%s" t
    printfn "%s" (rule ())

type Stats = { Mean: float; P50: float; P95: float; P99: float; Max: float }

let stats (a: float array) =
    let s = Array.sort a
    let pick p = if s.Length = 0 then 0.0 else s.[min (s.Length - 1) (int (p * float s.Length))]
    { Mean = (if s.Length = 0 then 0.0 else Array.average s)
      P50 = pick 0.50; P95 = pick 0.95; P99 = pick 0.99
      Max = (if s.Length = 0 then 0.0 else s.[s.Length - 1]) }

let ms (t: int64) = float t * 1000.0 / float Stopwatch.Frequency

// ---------------------------------------------------------------- world setup

type Sim =
    { G: Grid
      P: Profile
      F: Force
      R: Reservations
      S: Pathfinding.Searcher
      Buf: int array
      IU: int array
      IDx: int array
      IDy: int array
      Cm: bool array
      Rng: Rng }

let makeSim (g: Grid) (units: int) (footprint: int) (seed: uint64) =
    let p = Clearance.build g footprint
    let f = createForce units
    { G = g; P = p; F = f
      R = createReservations g
      S = Pathfinding.create g
      Buf = Array.zeroCreate MaxPath
      IU = Array.zeroCreate units
      IDx = Array.zeroCreate units
      IDy = Array.zeroCreate units
      Cm = Array.zeroCreate units
      Rng = Rng(seed) }

let private findOpen (sim: Sim) level cx cy spread =
    let mutable x, y = 0, 0
    let mutable ok = false
    let mutable tries = 0
    while not ok && tries < 400 do
        x <- max 1 (min (sim.G.Width - sim.P.Size - 1) (cx + sim.Rng.Range(-spread, spread)))
        y <- max 1 (min (sim.G.Height - sim.P.Size - 1) (cy + sim.Rng.Range(-spread, spread)))
        if canStand sim.P sim.G level x y then ok <- true
        tries <- tries + 1
    struct (x, y, ok)

/// Node-expansion cap. A search that exceeds it fails rather than stalling the
/// tick, which is the behaviour a bounded server needs.
let mutable maxExpand = 60000

/// Assign a fresh destination and path. Returns nodes expanded.
let repath (sim: Sim) (i: int) (gx: int) (gy: int) =
    let f = sim.F
    let len =
        Pathfinding.search sim.S sim.G sim.P f.Level.[i] f.X.[i] f.Y.[i] gx gy maxExpand sim.Buf
    if len > 0 then
        // stored goal-first; reverse into the unit's path, skipping its own cell
        let n = min (len - 1) MaxPath
        for k in 0 .. n - 1 do
            f.Path.[i * MaxPath + k] <- sim.Buf.[len - 2 - k]
        f.PathLen.[i] <- n
        f.PathIdx.[i] <- 0
    else
        f.PathLen.[i] <- 0
        f.PathIdx.[i] <- 0
    sim.S.Expanded

// ---------------------------------------------------------------------------

let mapProfile (g: Grid) (p: Profile) =
    header "0. Map and profile"
    let struct (cellPct, edgePct) = describe g
    let n = g.Width * g.Height * g.Levels
    let standable = p.CanStand |> Array.sumBy (fun b -> if b then 1 else 0)
    printfn "  grid                  %d x %d, %d level(s)" g.Width g.Height g.Levels
    printfn "  blocking cells        %.2f%%   opaque edges %.2f%%" cellPct edgePct
    printfn "  standable anchors     %.1f%% for a %dx%d footprint"
        (float standable / float n * 100.0) p.Size p.Size

/// Two forces advancing into one another's start areas. This deliberately
/// produces heavy mutual blocking: the interesting question is whether resolve
/// cost rises when almost nothing can commit.
let openMovement (g: Grid) (units: int) (footprint: int) (ticks: int) =
    header (sprintf "1. %d units converging head-on, %dx%d footprints" units footprint footprint)
    let sim = makeSim g units footprint 11UL
    let f = sim.F
    let half = units / 2
    for i in 0 .. units - 1 do
        f.Team.[i] <- if i < half then 0 else 1
        f.Speed.[i] <- 30 + sim.Rng.Range(0, 40)
        let cx = if i < half then 90 else g.Width - 90
        let struct (x, y, _) = findOpen sim 0 cx (g.Height / 2) 70
        f.X.[i] <- x
        f.Y.[i] <- y
    // send each force at the other's start area
    for i in 0 .. units - 1 do
        let tx = if i < half then g.Width - 90 else 90
        let struct (gx, gy, _) = findOpen sim 0 tx (g.Height / 2) 70
        repath sim i gx gy |> ignore

    let c = newCounters ()
    for _ in 1 .. 60 do step sim.G sim.P f sim.R sim.IU sim.IDx sim.IDy sim.Cm c

    let samples = Array.zeroCreate ticks
    let c = newCounters ()
    let gc0 = GC.CollectionCount(0)
    let alloc0 = GC.GetTotalAllocatedBytes(false)
    let mutable repaths = 0
    for t in 0 .. ticks - 1 do
        let t0 = Stopwatch.GetTimestamp()
        step sim.G sim.P f sim.R sim.IU sim.IDx sim.IDy sim.Cm c
        samples.[t] <- ms (Stopwatch.GetTimestamp() - t0)
        // a unit that arrived or gave up receives a new distant objective.
        // This happens outside the timed region so that path search is
        // measured on its own rather than smeared into resolve cost.
        for i in 0 .. units - 1 do
            if f.PathIdx.[i] >= f.PathLen.[i] then
                let tx = if f.Team.[i] = 0 then g.Width - 90 else 90
                let struct (gx, gy, ok) = findOpen sim 0 tx (g.Height / 2) 70
                if ok then
                    repath sim i gx gy |> ignore
                    repaths <- repaths + 1

    let s = stats samples
    let kb = float (GC.GetTotalAllocatedBytes(false) - alloc0) / 1024.0
    printfn "  resolve per tick      mean %6.3f  p95 %6.3f  p99 %6.3f  max %6.3f ms"
        s.Mean s.P95 s.P99 s.Max
    printfn "  budget 50 ms -> mean %.2f%%  worst %.2f%%" (s.Mean / 50.0 * 100.0) (s.Max / 50.0 * 100.0)
    printfn ""
    printfn "  transitions attempted %d/tick, committed %d/tick (%.1f%%)"
        (c.Attempted / ticks) (c.Committed / ticks)
        (float c.Committed / float (max 1 c.Attempted) * 100.0)
    printfn "  blocked: terrain %d  friendly %d  hostile %d (totals over %d ticks)"
        c.BlockedTerrain c.BlockedFriendly c.BlockedHostile ticks
    printfn "  chain advances %d (a follower entering space a leader vacated)" c.ChainAdvances
    printfn "  resolve passes %.2f per tick, deadlock replans %d"
        (float c.Passes / float ticks) c.Deadlocks
    printfn "  repaths outside the timed region: %d (%.0f/tick)"
        repaths (float repaths / float ticks)
    printfn "  -> commit rate is low because the forces gridlock; resolve cost"
    printfn "     stays flat regardless, which is the point of the measurement"
    printfn "  GC gen0 %d, allocated %.1f KB" (GC.CollectionCount(0) - gc0) kb
    s

/// A column forcing itself through one doorway: the worst case for reservations.
let congestion (g: Grid) (units: int) (footprint: int) (ticks: int) =
    header (sprintf "2. Congestion: %d units funnelled through one 5-cell gap" units)
    let sim = makeSim g units footprint 22UL
    let f = sim.F
    // A clean map with one barrier, so the bottleneck is the only obstacle and
    // the measurement is of contention rather than of map structure.
    let wallX = g.Width / 2
    let gapY = g.Height / 2
    System.Array.Fill(g.Cells, Clear)
    System.Array.Fill(g.VEdge, Clear)
    System.Array.Fill(g.HEdge, Clear)
    for y in 0 .. g.Height - 1 do
        g.VEdge.[idx g 0 wallX y] <- Opaque
    for y in gapY - 2 .. gapY + 2 do
        g.VEdge.[idx g 0 wallX y] <- Clear
    let p = Clearance.build g footprint
    let sim = { sim with P = p }

    for i in 0 .. units - 1 do
        f.Team.[i] <- 0
        f.Speed.[i] <- 40
        let struct (x, y, _) = findOpen sim 0 (wallX - 40) gapY 30
        f.X.[i] <- x
        f.Y.[i] <- y
    let mutable pathed = 0
    let mutable expandTotal = 0
    let mutable searchMs = 0.0
    for i in 0 .. units - 1 do
        let t0 = Stopwatch.GetTimestamp()
        let e = repath sim i (wallX + 30) (gapY + sim.Rng.Range(-20, 20))
        searchMs <- searchMs + ms (Stopwatch.GetTimestamp() - t0)
        expandTotal <- expandTotal + e
        if f.PathLen.[i] > 0 then pathed <- pathed + 1
    printfn "  units with a route through the gap: %d / %d" pathed units
    printfn "  route search: %.3f ms each, %d nodes expanded each"
        (searchMs / float units) (expandTotal / units)

    let c = newCounters ()
    for _ in 1 .. 30 do step sim.G sim.P f sim.R sim.IU sim.IDx sim.IDy sim.Cm c

    let samples = Array.zeroCreate ticks
    let c = newCounters ()
    for t in 0 .. ticks - 1 do
        let t0 = Stopwatch.GetTimestamp()
        step sim.G sim.P f sim.R sim.IU sim.IDx sim.IDy sim.Cm c
        samples.[t] <- ms (Stopwatch.GetTimestamp() - t0)
    let s = stats samples
    printfn "  resolve per tick      mean %6.3f  max %6.3f ms" s.Mean s.Max
    printfn "  attempted %d/tick, committed %d/tick (%.1f%%)"
        (c.Attempted / ticks) (c.Committed / ticks)
        (float c.Committed / float (max 1 c.Attempted) * 100.0)
    printfn "  blocked by a friendly %d, chain advances %d"
        c.BlockedFriendly c.ChainAdvances
    printfn "  resolve passes %.2f per tick, deadlock replans %d"
        (float c.Passes / float ticks) c.Deadlocks
    printfn "  -> contention raises blocked transitions, not resolve cost"

let unitSweep (g: Grid) (footprint: int) =
    header "3. Unit-count sweep (resolve only)"
    printfn "  %-8s %10s %10s %11s %12s" "units" "mean ms" "max ms" "% of 50ms" "committed/tk"
    for units in [ 50; 100; 200; 400; 800 ] do
        let sim = makeSim g units footprint 33UL
        let f = sim.F
        let half = units / 2
        for i in 0 .. units - 1 do
            f.Team.[i] <- if i < half then 0 else 1
            f.Speed.[i] <- 40
            let cx = if i < half then 90 else g.Width - 90
            let struct (x, y, _) = findOpen sim 0 cx (g.Height / 2) 70
            f.X.[i] <- x
            f.Y.[i] <- y
        for i in 0 .. units - 1 do
            let tx = if i < half then g.Width - 90 else 90
            let struct (gx, gy, _) = findOpen sim 0 tx (g.Height / 2) 70
            repath sim i gx gy |> ignore
        let c = newCounters ()
        for _ in 1 .. 40 do step sim.G sim.P f sim.R sim.IU sim.IDx sim.IDy sim.Cm c
        let samples = Array.zeroCreate 300
        let c = newCounters ()
        for t in 0 .. 299 do
            let t0 = Stopwatch.GetTimestamp()
            step sim.G sim.P f sim.R sim.IU sim.IDx sim.IDy sim.Cm c
            samples.[t] <- ms (Stopwatch.GetTimestamp() - t0)
        let s = stats samples
        printfn "  %-8d %10.3f %10.3f %10.2f%% %12d"
            units s.Mean s.Max (s.Max / 50.0 * 100.0) (c.Committed / 300)

/// Path search is the bursty cost. How many replans fit in a tick?
let pathfindingCost (g: Grid) (footprint: int) =
    header "4. Path search cost and how many replans fit in a tick"
    let sim = makeSim g 1 footprint 44UL
    let f = sim.F
    f.Level.[0] <- 0
    printfn "  %-22s %10s %10s %12s %12s" "distance" "mean ms" "max ms" "expanded" "path len"
    let cases =
        [ "short (30 cells)", 30
          "medium (120 cells)", 120
          "long (300 cells)", 300
          "cross-map (450 cells)", 450 ]
    for (label, dist) in cases do
        let mutable samples = ResizeArray<float>()
        let mutable expanded = 0
        let mutable plen = 0
        let mutable n = 0
        for _ in 1 .. 60 do
            let struct (sx, sy, ok1) = findOpen sim 0 (g.Width / 2 - dist / 2) (g.Height / 2) 60
            let struct (gx, gy, ok2) = findOpen sim 0 (g.Width / 2 + dist / 2) (g.Height / 2) 60
            if ok1 && ok2 then
                f.X.[0] <- sx
                f.Y.[0] <- sy
                let t0 = Stopwatch.GetTimestamp()
                let e = repath sim 0 gx gy
                let el = ms (Stopwatch.GetTimestamp() - t0)
                if f.PathLen.[0] > 0 then
                    samples.Add el
                    expanded <- expanded + e
                    plen <- plen + f.PathLen.[0]
                    n <- n + 1
        if n > 0 then
            let s = stats (samples.ToArray())
            printfn "  %-22s %10.3f %10.3f %12d %12d"
                label s.Mean s.Max (expanded / n) (plen / n)
            printfn "     -> %.0f such searches fit in a 50 ms tick, %.0f in a 20 ms target"
                (50.0 / s.Mean) (20.0 / s.Mean)

let footprintSweep (g: Grid) =
    header "5. Footprint size"
    printfn "  %-12s %10s %10s %12s" "footprint" "mean ms" "max ms" "standable %"
    for size in [ 1; 2; 3; 4 ] do
        let sim = makeSim g 200 size 55UL
        let f = sim.F
        for i in 0 .. 199 do
            f.Team.[i] <- i % 2
            f.Speed.[i] <- 40
            let struct (x, y, _) = findOpen sim 0 (g.Width / 2) (g.Height / 2) 120
            f.X.[i] <- x
            f.Y.[i] <- y
        for i in 0 .. 199 do
            let struct (gx, gy, _) = findOpen sim 0 (g.Width / 2) (g.Height / 2) 150
            repath sim i gx gy |> ignore
        let c = newCounters ()
        for _ in 1 .. 40 do step sim.G sim.P f sim.R sim.IU sim.IDx sim.IDy sim.Cm c
        let samples = Array.zeroCreate 300
        let c = newCounters ()
        for t in 0 .. 299 do
            let t0 = Stopwatch.GetTimestamp()
            step sim.G sim.P f sim.R sim.IU sim.IDx sim.IDy sim.Cm c
            samples.[t] <- ms (Stopwatch.GetTimestamp() - t0)
        let s = stats samples
        let n = g.Width * g.Height * g.Levels
        let standable = sim.P.CanStand |> Array.sumBy (fun b -> if b then 1 else 0)
        printfn "  %-12s %10.3f %10.3f %11.1f%%"
            (sprintf "%dx%d" size size) s.Mean s.Max (float standable / float n * 100.0)

/// The realistic model: routes are staggered, not recomputed every tick.
/// How much of the tick does a given replan cadence consume?
let replanBudget (g: Grid) (units: int) (footprint: int) =
    header "6. Staggered replanning: what cadence fits the budget"
    let sim = makeSim g units footprint 66UL
    let f = sim.F
    for i in 0 .. units - 1 do
        f.Team.[i] <- i % 2
        f.Speed.[i] <- 40
        let struct (x, y, _) = findOpen sim 0 (g.Width / 2) (g.Height / 2) 150
        f.X.[i] <- x
        f.Y.[i] <- y
    for i in 0 .. units - 1 do
        let struct (gx, gy, _) = findOpen sim 0 (g.Width / 2) (g.Height / 2) 180
        repath sim i gx gy |> ignore

    printfn "  %-30s %10s %10s %11s" "cadence" "mean ms" "max ms" "% of 50ms"
    let cases =
        [ "every unit, every tick", 1
          "every unit every 4 ticks", 4
          "every unit every 20 ticks (1 s)", 20
          "every unit every 100 ticks (5 s)", 100 ]
    for (label, period) in cases do
        let perTick = max 1 (units / period)
        let samples = Array.zeroCreate 200
        let c = newCounters ()
        let mutable cursor = 0
        // warm
        for _ in 1 .. 20 do
            step sim.G sim.P f sim.R sim.IU sim.IDx sim.IDy sim.Cm c
        for t in 0 .. 199 do
            let t0 = Stopwatch.GetTimestamp()
            step sim.G sim.P f sim.R sim.IU sim.IDx sim.IDy sim.Cm c
            for _ in 1 .. perTick do
                let i = cursor % units
                cursor <- cursor + 1
                let struct (gx, gy, ok) = findOpen sim 0 (g.Width / 2) (g.Height / 2) 180
                if ok then repath sim i gx gy |> ignore
            samples.[t] <- ms (Stopwatch.GetTimestamp() - t0)
        let s = stats samples
        printfn "  %-30s %10.3f %10.3f %10.2f%%   (%d searches/tick)"
            label s.Mean s.Max (s.Max / 50.0 * 100.0) perTick
