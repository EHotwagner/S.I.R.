module Harness

open System
open System.Diagnostics
open Wasmtime

/// One unit's control instance: its own store, instance, memory and entry point.
type UnitInstance =
    { Store: Store
      Memory: Memory
      Tick: Function
      mutable LastOutput: int
      mutable Faults: int }

/// A compiled artifact shared by many instances, as the architecture requires.
type Artifact =
    { Engine: Engine
      Module: Module
      Linker: Linker }

/// The restricted execution profile recommended in
/// docs/research/wasm-runtime-selection.md: core modules only, deterministic
/// fuel, NaN canonicalisation, and every optional proposal disabled until a
/// guest toolchain demonstrably requires it.
let createEngine (fuelEnabled: bool) =
    let cfg = new Config()
    cfg.WithFuelConsumption(fuelEnabled) |> ignore
    cfg.WithOptimizationLevel(OptimizationLevel.Speed) |> ignore
    // NaN canonicalisation is omitted here because these modules are
    // integer-only. It takes a two-argument form in this binding and only
    // matters once floating point is permitted in guest code.
    cfg.WithReferenceTypes(false) |> ignore
    cfg.WithBulkMemory(false) |> ignore
    cfg.WithSIMD(false) |> ignore
    cfg.WithRelaxedSIMD(false, false) |> ignore
    cfg.WithMultiValue(false) |> ignore
    cfg.WithMultiMemory(false) |> ignore
    cfg.WithWasmThreads(false) |> ignore
    cfg.WithTailCalls(false) |> ignore
    cfg.WithComponentModel(false) |> ignore
    new Engine(cfg)

let compile (engine: Engine) (name: string) (wat: string) =
    let m = Module.FromText(engine, name, wat)
    let linker = new Linker(engine)
    { Engine = engine; Module = m; Linker = linker }

let instantiate (a: Artifact) : UnitInstance =
    let store = new Store(a.Engine)
    let inst = a.Linker.Instantiate(store, a.Module)
    let mem = inst.GetMemory("memory")
    let tick = inst.GetFunction("tick")
    if isNull (box mem) then failwith "module did not export memory"
    if isNull (box tick) then failwith "module did not export tick"
    { Store = store; Memory = mem; Tick = tick; LastOutput = 0; Faults = 0 }

/// Deterministic pseudo-random source. Not for gameplay — only to build
/// varied but reproducible observation payloads.
type Lcg(seed: uint64) =
    let mutable s = seed
    member _.Next() =
        s <- s * 6364136223846793005UL + 1442695040888963407UL
        int ((s >>> 33) &&& 0x7FFFFFFFUL)
    member this.Range(lo, hi) = lo + (this.Next() % (hi - lo))

/// Writes a knowledge-filtered observation into an instance's linear memory.
/// This models the cost the architecture identifies as dominant: constructing
/// and marshalling what the unit is entitled to know, once per invocation.
let writeObservation (u: UnitInstance) (tick: int) (contactCount: int) (rng: Lcg) =
    let m = u.Memory
    m.WriteInt32(0L, tick)
    m.WriteInt32(4L, rng.Range(1, 100))
    m.WriteInt32(8L, rng.Range(0, 210))
    let ox = rng.Range(0, 512)
    let oy = rng.Range(0, 512)
    m.WriteInt32(12L, ox)
    m.WriteInt32(16L, oy)
    m.WriteInt32(20L, rng.Range(0, 8))
    m.WriteInt32(24L, rng.Range(0, 5))
    m.WriteInt32(int64 Wat.ContactCountOffset, contactCount)
    for i in 0 .. contactCount - 1 do
        let b = int64 (Wat.ContactBase + i * Wat.ContactStride)
        m.WriteInt32(b, 1000 + i)
        m.WriteInt32(b + 4L, rng.Range(0, 512))
        m.WriteInt32(b + 8L, rng.Range(0, 512))
        m.WriteInt32(b + 12L, rng.Range(0, 6))
        m.WriteInt32(b + 16L, rng.Range(0, 10))
        m.WriteInt32(b + 20L, rng.Range(0, 60))
        m.WriteInt32(b + 24L, 0)
        m.WriteInt32(b + 28L, 0)


/// Bulk alternative: build the observation once in a host-side buffer and copy
/// it into guest memory as a single span write, rather than one interop call
/// per field. This is the realistic floor for marshalling cost.
let writeObservationBulk (u: UnitInstance) (buffer: int array) (tick: int) (contactCount: int) (rng: Lcg) =
    buffer.[0] <- tick
    buffer.[1] <- rng.Range(1, 100)
    buffer.[2] <- rng.Range(0, 210)
    buffer.[3] <- rng.Range(0, 512)
    buffer.[4] <- rng.Range(0, 512)
    buffer.[5] <- rng.Range(0, 8)
    buffer.[6] <- rng.Range(0, 5)
    buffer.[7] <- contactCount
    for i in 0 .. contactCount - 1 do
        let w = 8 + i * 8
        buffer.[w] <- 1000 + i
        buffer.[w + 1] <- rng.Range(0, 512)
        buffer.[w + 2] <- rng.Range(0, 512)
        buffer.[w + 3] <- rng.Range(0, 6)
        buffer.[w + 4] <- rng.Range(0, 10)
        buffer.[w + 5] <- rng.Range(0, 60)
        buffer.[w + 6] <- 0
        buffer.[w + 7] <- 0
    let words = 8 + contactCount * 8
    let src = System.Runtime.InteropServices.MemoryMarshal.AsBytes(
                System.ReadOnlySpan<int>(buffer, 0, words))
    let dst = u.Memory.GetSpan(0L, words * 4)
    src.CopyTo(dst)

let readOutput (u: UnitInstance) =
    let b = int64 Wat.OutputBase
    struct (u.Memory.ReadInt32(b),
            u.Memory.ReadInt32(b + 4L),
            u.Memory.ReadInt32(b + 8L),
            u.Memory.ReadInt32(b + 12L))

/// Per the canonical fuel lifecycle: no carry-over, a fresh allowance each
/// invocation, and unused fuel discarded rather than accumulating.
let resetFuel (u: UnitInstance) (allowance: uint64) = u.Store.Fuel <- allowance

let invoke (u: UnitInstance) (work: int) =
    try
        let r = u.Tick.Invoke(work)
        u.LastOutput <- (match r with :? int as i -> i | _ -> 0)
        true
    with _ ->
        u.Faults <- u.Faults + 1
        false

// ---------------------------------------------------------------- statistics

type Stats =
    { Count: int
      MeanMs: float
      P50Ms: float
      P95Ms: float
      P99Ms: float
      MaxMs: float }

let stats (samplesMs: float array) =
    let s = Array.sort samplesMs
    let pick (p: float) =
        if s.Length = 0 then 0.0
        else s.[min (s.Length - 1) (int (p * float s.Length))]
    { Count = s.Length
      MeanMs = if s.Length = 0 then 0.0 else Array.average s
      P50Ms = pick 0.50
      P95Ms = pick 0.95
      P99Ms = pick 0.99
      MaxMs = if s.Length = 0 then 0.0 else s.[s.Length - 1] }

let ticksToMs (t: int64) = float t * 1000.0 / float Stopwatch.Frequency

type GcSnapshot =
    { G0: int; G1: int; G2: int; AllocatedMb: float }

let gcSnapshot () =
    { G0 = GC.CollectionCount(0)
      G1 = GC.CollectionCount(1)
      G2 = GC.CollectionCount(2)
      AllocatedMb = float (GC.GetTotalAllocatedBytes(false)) / 1048576.0 }

let gcDelta (a: GcSnapshot) (b: GcSnapshot) =
    { G0 = b.G0 - a.G0
      G1 = b.G1 - a.G1
      G2 = b.G2 - a.G2
      AllocatedMb = b.AllocatedMb - a.AllocatedMb }
