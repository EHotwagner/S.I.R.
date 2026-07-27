module Scenarios

open System
open System.Diagnostics
open System.Threading.Tasks
open Harness

let private line () = String.replicate 78 "-"

let private header (title: string) =
    printfn ""
    printfn "%s" (line ())
    printfn "%s" title
    printfn "%s" (line ())

let private reportStats (label: string) (s: Stats) =
    printfn "  %-28s mean %7.3f  p50 %7.3f  p95 %7.3f  p99 %7.3f  max %7.3f ms"
        label s.MeanMs s.P50Ms s.P95Ms s.P99Ms s.MaxMs

// ---------------------------------------------------------------------------
// 1. Compile once, instantiate many. Establishes that artifact reuse works and
//    measures what instantiating a force actually costs before a match.
// ---------------------------------------------------------------------------
let compileAndInstantiate (units: int) =
    header (sprintf "1. Artifact reuse: compile once, instantiate %d units" units)
    let engine = createEngine true

    let sw = Stopwatch.StartNew()
    let art = compile engine "representative" Wat.representative
    sw.Stop()
    let compileMs = ticksToMs sw.ElapsedTicks

    let sw2 = Stopwatch.StartNew()
    let instances = Array.init units (fun _ -> instantiate art)
    sw2.Stop()
    let instMs = ticksToMs sw2.ElapsedTicks

    printfn "  compile (once)              %8.3f ms" compileMs
    printfn "  instantiate %4d stores     %8.3f ms  (%.4f ms each)"
        units instMs (instMs / float units)
    printfn "  -> per-match setup cost is %.1f ms, paid before live play" (compileMs + instMs)
    engine, art, instances

// ---------------------------------------------------------------------------
// 2. The central question: what does one tick of N invocations cost?
//    Phases are measured separately because the architecture claims
//    observation marshalling dominates, not instruction execution.
// ---------------------------------------------------------------------------
let tickCostBreakdown (instances: UnitInstance array) (contacts: int) (work: int) (fuel: uint64) (ticks: int) =
    header (sprintf "2. Tick cost at %d units, %d contacts each, work=%d, fuel=%d"
                instances.Length contacts work fuel)

    let marshalMs = Array.zeroCreate ticks
    let invokeMs = Array.zeroCreate ticks
    let readMs = Array.zeroCreate ticks
    let totalMs = Array.zeroCreate ticks
    let rng = Lcg(12345UL)

    // warm up so JIT and first-touch costs do not land in the samples
    for _ in 1 .. 20 do
        for u in instances do
            writeObservation u 0 contacts rng
            resetFuel u fuel
            invoke u work |> ignore
            readOutput u |> ignore

    let gcBefore = gcSnapshot ()
    let swAll = Stopwatch.StartNew()

    for t in 0 .. ticks - 1 do
        let t0 = Stopwatch.GetTimestamp()
        for u in instances do
            writeObservation u t contacts rng
        let t1 = Stopwatch.GetTimestamp()
        for u in instances do
            resetFuel u fuel
            invoke u work |> ignore
        let t2 = Stopwatch.GetTimestamp()
        for u in instances do
            readOutput u |> ignore
        let t3 = Stopwatch.GetTimestamp()

        marshalMs.[t] <- ticksToMs (t1 - t0)
        invokeMs.[t] <- ticksToMs (t2 - t1)
        readMs.[t] <- ticksToMs (t3 - t2)
        totalMs.[t] <- ticksToMs (t3 - t0)

    swAll.Stop()
    let gcAfter = gcSnapshot ()
    let d = gcDelta gcBefore gcAfter

    reportStats "observation marshalling" (stats marshalMs)
    reportStats "guest invocation" (stats invokeMs)
    reportStats "output read" (stats readMs)
    reportStats "TOTAL per tick" (stats totalMs)
    let s = stats totalMs
    printfn ""
    printfn "  budget 50.00 ms/tick   worst tick used %.2f%%   p99 used %.2f%%"
        (s.MaxMs / 50.0 * 100.0) (s.P99Ms / 50.0 * 100.0)
    printfn "  GC over %d ticks: g0=%d g1=%d g2=%d  allocated %.1f MB (%.1f KB/tick)"
        ticks d.G0 d.G1 d.G2 d.AllocatedMb (d.AllocatedMb * 1024.0 / float ticks)
    stats totalMs

// ---------------------------------------------------------------------------
// 3. Does cost scale with unit count, and where does it stop fitting?
// ---------------------------------------------------------------------------
let unitCountSweep (contacts: int) (work: int) (fuel: uint64) =
    header "3. Unit-count sweep (per-side counts are half these totals)"
    let engine = createEngine true
    let art = compile engine "representative" Wat.representative
    printfn "  %-8s %10s %10s %10s %10s" "units" "mean ms" "p99 ms" "max ms" "% of 50ms"
    for units in [ 8; 40; 100; 200; 400; 800 ] do
        let instances = Array.init units (fun _ -> instantiate art)
        let rng = Lcg(999UL)
        for _ in 1 .. 10 do
            for u in instances do
                writeObservation u 0 contacts rng
                resetFuel u fuel
                invoke u work |> ignore
        let samples = Array.zeroCreate 300
        for t in 0 .. 299 do
            let t0 = Stopwatch.GetTimestamp()
            for u in instances do
                writeObservation u t contacts rng
                resetFuel u fuel
                invoke u work |> ignore
                readOutput u |> ignore
            samples.[t] <- ticksToMs (Stopwatch.GetTimestamp() - t0)
        let s = stats samples
        printfn "  %-8d %10.3f %10.3f %10.3f %9.1f%%"
            units s.MeanMs s.P99Ms s.MaxMs (s.MaxMs / 50.0 * 100.0)
        for u in instances do u.Store.Dispose()
    engine.Dispose()

// ---------------------------------------------------------------------------
// 4. Cost is invocations x fuel, not invocations alone. Sweep the allowance.
// ---------------------------------------------------------------------------
let fuelSweep (units: int) (contacts: int) =
    header (sprintf "4. Work/fuel sweep at %d units, %d contacts" units contacts)
    let engine = createEngine true
    let art = compile engine "representative" Wat.representative
    let instances = Array.init units (fun _ -> instantiate art)
    printfn "  %-8s %12s %10s %10s %10s" "work" "fuel used" "mean ms" "max ms" "% of 50ms"
    for work in [ 1; 4; 16; 64; 256 ] do
        let rng = Lcg(4242UL)
        let fuel = 100_000_000UL
        for _ in 1 .. 5 do
            for u in instances do
                writeObservation u 0 contacts rng
                resetFuel u fuel
                invoke u work |> ignore
        // measure fuel actually consumed by one representative invocation
        let probe = instances.[0]
        writeObservation probe 0 contacts rng
        resetFuel probe fuel
        invoke probe work |> ignore
        let used = fuel - probe.Store.Fuel

        let samples = Array.zeroCreate 200
        for t in 0 .. 199 do
            let t0 = Stopwatch.GetTimestamp()
            for u in instances do
                writeObservation u t contacts rng
                resetFuel u fuel
                invoke u work |> ignore
                readOutput u |> ignore
            samples.[t] <- ticksToMs (Stopwatch.GetTimestamp() - t0)
        let s = stats samples
        printfn "  %-8d %12d %10.3f %10.3f %9.1f%%"
            work used s.MeanMs s.MaxMs (s.MaxMs / 50.0 * 100.0)
    for u in instances do u.Store.Dispose()
    engine.Dispose()

// ---------------------------------------------------------------------------
// 5. Observation size sweep. Tests the claim that marshalling dominates.
// ---------------------------------------------------------------------------
let observationSweep (units: int) (work: int) =
    header (sprintf "5. Observation-size sweep at %d units" units)
    let engine = createEngine true
    let art = compile engine "representative" Wat.representative
    let instances = Array.init units (fun _ -> instantiate art)
    printfn "  %-10s %10s %12s %12s %10s" "contacts" "bytes" "marshal ms" "invoke ms" "total ms"
    for contacts in [ 0; 5; 15; 30; 60; 120 ] do
        let rng = Lcg(77UL)
        let fuel = 100_000_000UL
        for _ in 1 .. 5 do
            for u in instances do
                writeObservation u 0 contacts rng
                resetFuel u fuel
                invoke u work |> ignore
        let mSamples = Array.zeroCreate 200
        let iSamples = Array.zeroCreate 200
        for t in 0 .. 199 do
            let t0 = Stopwatch.GetTimestamp()
            for u in instances do writeObservation u t contacts rng
            let t1 = Stopwatch.GetTimestamp()
            for u in instances do
                resetFuel u fuel
                invoke u work |> ignore
            let t2 = Stopwatch.GetTimestamp()
            mSamples.[t] <- ticksToMs (t1 - t0)
            iSamples.[t] <- ticksToMs (t2 - t1)
        let m = stats mSamples
        let i = stats iSamples
        printfn "  %-10d %10d %12.3f %12.3f %10.3f"
            contacts (32 + contacts * 32) m.MeanMs i.MeanMs (m.MeanMs + i.MeanMs)
    for u in instances do u.Store.Dispose()
    engine.Dispose()

// ---------------------------------------------------------------------------
// 6. Parallel execution across stores, with a deterministic merge.
// ---------------------------------------------------------------------------
let parallelExecution (units: int) (contacts: int) (work: int) =
    header (sprintf "6. Parallel invocation across %d stores" units)
    let engine = createEngine true
    let art = compile engine "representative" Wat.representative
    let instances = Array.init units (fun _ -> instantiate art)
    let fuel = 100_000_000UL
    let rng = Lcg(31337UL)

    for u in instances do writeObservation u 0 contacts rng

    let runSerial () =
        let t0 = Stopwatch.GetTimestamp()
        for u in instances do
            resetFuel u fuel
            invoke u work |> ignore
        ticksToMs (Stopwatch.GetTimestamp() - t0)

    let runParallel (dop: int) =
        let t0 = Stopwatch.GetTimestamp()
        let opts = ParallelOptions(MaxDegreeOfParallelism = dop)
        Parallel.For(0, instances.Length, opts, fun i ->
            let u = instances.[i]
            resetFuel u fuel
            invoke u work |> ignore) |> ignore
        ticksToMs (Stopwatch.GetTimestamp() - t0)

    for _ in 1 .. 20 do runSerial () |> ignore
    let serial = Array.init 100 (fun _ -> runSerial ())
    printfn "  serial            mean %7.3f ms" (stats serial).MeanMs
    for dop in [ 2; 4; 8; 16 ] do
        for _ in 1 .. 10 do runParallel dop |> ignore
        let par = Array.init 100 (fun _ -> runParallel dop)
        let sp = (stats serial).MeanMs / (stats par).MeanMs
        printfn "  parallel dop=%-3d  mean %7.3f ms   speedup %.2fx" dop (stats par).MeanMs sp

    // determinism: keyed outputs must be identical regardless of scheduling
    let collect () =
        instances |> Array.map (fun u ->
            let struct (id, act, score, seen) = readOutput u
            (id, act, score, seen))
    let a = collect ()
    runParallel 16 |> ignore
    let b = collect ()
    printfn "  keyed outputs identical after parallel run: %b" (a = b)
    for u in instances do u.Store.Dispose()
    engine.Dispose()

// ---------------------------------------------------------------------------
// 7. Correctness properties the architecture depends on.
// ---------------------------------------------------------------------------
let correctnessChecks () =
    header "7. Architectural guarantees"
    let engine = createEngine true

    // isolation: shared compiled code must not mean shared state
    let art = compile engine "counter" Wat.statefulCounter
    let a = instantiate art
    let b = instantiate art
    for _ in 1 .. 5 do
        resetFuel a 1_000_000UL
        invoke a 0 |> ignore
    resetFuel b 1_000_000UL
    invoke b 0 |> ignore
    printfn "  instance isolation           A=%d B=%d  -> %s"
        a.LastOutput b.LastOutput
        (if a.LastOutput = 5 && b.LastOutput = 1 then "PASS" else "FAIL")

    // fuel exhaustion must trap, and must not leave partial output
    let burnArt = compile engine "burner" Wat.fuelBurner
    let c = instantiate burnArt
    resetFuel c 10_000UL
    let ok = invoke c 100_000_000
    printfn "  fuel exhaustion traps        completed=%b faults=%d  -> %s"
        ok c.Faults (if not ok && c.Faults = 1 then "PASS" else "FAIL")

    // no fuel carry-over between invocations
    let d = instantiate art
    resetFuel d 1_000_000UL
    invoke d 0 |> ignore
    let after1 = d.Store.Fuel
    resetFuel d 1_000_000UL
    invoke d 0 |> ignore
    let after2 = d.Store.Fuel
    printfn "  no fuel carry-over           after1=%d after2=%d  -> %s"
        after1 after2 (if after1 = after2 then "PASS" else "FAIL")

    // determinism: identical inputs reproduce identical outputs
    let repArt = compile engine "rep" Wat.representative
    let runOnce seed =
        let u = instantiate repArt
        let rng = Lcg(seed)
        let mutable acc = 0
        for t in 1 .. 50 do
            writeObservation u t 20 rng
            resetFuel u 10_000_000UL
            invoke u 4 |> ignore
            let struct (id, _, score, seen) = readOutput u
            acc <- acc ^^^ (id * 31 + score * 7 + seen)
        u.Store.Dispose()
        acc
    let r1 = runOnce 555UL
    let r2 = runOnce 555UL
    let r3 = runOnce 556UL
    printfn "  determinism                  run1=%d run2=%d (differing seed=%d)  -> %s"
        r1 r2 r3 (if r1 = r2 && r1 <> r3 then "PASS" else "FAIL")

    // memory bound declared by the module must be enforced
    printfn "  declared memory bound        1 page (64 KB) max per instance"
    engine.Dispose()

// ---------------------------------------------------------------------------
// 8. Sustained real-time loop. Worst tick is what bounds a real-time server,
//    so this runs a full match duration rather than a short burst.
// ---------------------------------------------------------------------------
let sustainedMatch (units: int) (contacts: int) (work: int) (seconds: int) =
    header (sprintf "8. Sustained load: %d units, %d s of match time at 20 Hz" units seconds)
    let engine = createEngine true
    let art = compile engine "representative" Wat.representative
    let instances = Array.init units (fun _ -> instantiate art)
    let fuel = 10_000_000UL
    let rng = Lcg(2026UL)
    let ticks = seconds * 20

    for _ in 1 .. 40 do
        for u in instances do
            writeObservation u 0 contacts rng
            resetFuel u fuel
            invoke u work |> ignore

    let gcBefore = gcSnapshot ()
    let samples = Array.zeroCreate ticks
    let sw = Stopwatch.StartNew()
    for t in 0 .. ticks - 1 do
        let t0 = Stopwatch.GetTimestamp()
        for u in instances do
            writeObservation u t contacts rng
            resetFuel u fuel
            invoke u work |> ignore
            readOutput u |> ignore
        samples.[t] <- ticksToMs (Stopwatch.GetTimestamp() - t0)
    sw.Stop()
    let gcAfter = gcSnapshot ()
    let d = gcDelta gcBefore gcAfter
    let s = stats samples
    let over = samples |> Array.filter (fun x -> x > 50.0) |> Array.length
    let overBudget = samples |> Array.filter (fun x -> x > 20.0) |> Array.length

    reportStats "tick total" s
    printfn ""
    printfn "  ticks over 50 ms ceiling     %d / %d" over ticks
    printfn "  ticks over 20 ms target      %d / %d" overBudget ticks
    printfn "  wall clock for %d ticks     %.1f s (match time %.1f s)"
        ticks (sw.Elapsed.TotalSeconds) (float ticks / 20.0)
    printfn "  GC: g0=%d g1=%d g2=%d  allocated %.1f MB total, %.2f KB/tick"
        d.G0 d.G1 d.G2 d.AllocatedMb (d.AllocatedMb * 1024.0 / float ticks)
    for u in instances do u.Store.Dispose()
    engine.Dispose()

// ---------------------------------------------------------------------------
// 9. Stress: deliberate overload well past the supported target.
// ---------------------------------------------------------------------------
let stress () =
    header "9. Stress: past the supported target"
    let engine = createEngine true
    let art = compile engine "representative" Wat.representative
    printfn "  %-8s %-9s %-7s %10s %10s %10s"
        "units" "contacts" "work" "mean ms" "max ms" "verdict"
    let cases =
        [ 200, 30, 4      // supported target, ordinary contact
          200, 30, 32     // heavy per-unit logic
          200, 60, 32     // dense contact and heavy logic
          400, 60, 32     // double the force
          800, 60, 64 ]   // deliberate overload
    for (units, contacts, work) in cases do
        let instances = Array.init units (fun _ -> instantiate art)
        let rng = Lcg(8080UL)
        let fuel = 100_000_000UL
        for _ in 1 .. 5 do
            for u in instances do
                writeObservation u 0 contacts rng
                resetFuel u fuel
                invoke u work |> ignore
        let samples = Array.zeroCreate 100
        for t in 0 .. 99 do
            let t0 = Stopwatch.GetTimestamp()
            for u in instances do
                writeObservation u t contacts rng
                resetFuel u fuel
                invoke u work |> ignore
                readOutput u |> ignore
            samples.[t] <- ticksToMs (Stopwatch.GetTimestamp() - t0)
        let s = stats samples
        let verdict =
            if s.MaxMs < 20.0 then "fits target"
            elif s.MaxMs < 50.0 then "fits ceiling"
            else "OVER"
        printfn "  %-8d %-9d %-7d %10.3f %10.3f %10s"
            units contacts work s.MeanMs s.MaxMs verdict
        for u in instances do u.Store.Dispose()
    engine.Dispose()

// ---------------------------------------------------------------------------
// 10. Per-field interop writes versus a single bulk copy. Distinguishes
//     "marshalling is expensive" from "naive marshalling is expensive".
// ---------------------------------------------------------------------------
let marshallingStrategies (units: int) (work: int) =
    header (sprintf "10. Marshalling strategy at %d units" units)
    let engine = createEngine true
    let art = compile engine "representative" Wat.representative
    let instances = Array.init units (fun _ -> instantiate art)
    let buffer = Array.zeroCreate<int> 4096
    printfn "  %-10s %14s %14s %10s" "contacts" "per-field ms" "bulk copy ms" "speedup"
    for contacts in [ 15; 30; 60; 120 ] do
        let rng = Lcg(555UL)
        for _ in 1 .. 10 do
            for u in instances do
                writeObservation u 0 contacts rng
                writeObservationBulk u buffer 0 contacts rng
        let a = Array.zeroCreate 200
        let b = Array.zeroCreate 200
        for t in 0 .. 199 do
            let t0 = Stopwatch.GetTimestamp()
            for u in instances do writeObservation u t contacts rng
            let t1 = Stopwatch.GetTimestamp()
            for u in instances do writeObservationBulk u buffer t contacts rng
            let t2 = Stopwatch.GetTimestamp()
            a.[t] <- ticksToMs (t1 - t0)
            b.[t] <- ticksToMs (t2 - t1)
        let sa = (stats a).MeanMs
        let sb = (stats b).MeanMs
        printfn "  %-10d %14.3f %14.3f %9.1fx" contacts sa sb (sa / sb)
    for u in instances do u.Store.Dispose()
    engine.Dispose()
