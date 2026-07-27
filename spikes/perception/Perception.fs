module Perception

open World
open Los

/// Units as struct-of-arrays. Footprints are 2x2 anchored at (X,Y).
type Force =
    { Count: int
      X: int array
      Y: int array
      Level: int array
      Facing: int array
      Team: int array
      Alive: bool array }

let createForce (n: int) =
    { Count = n
      X = Array.zeroCreate n
      Y = Array.zeroCreate n
      Level = Array.zeroCreate n
      Facing = Array.zeroCreate n
      Team = Array.zeroCreate n
      Alive = Array.create n true }

/// Two forces in contact. This is the expensive case: perception, engagement
/// and reactions all peak together, and it is where the budget must hold.
let placeInContact (g: Grid) (f: Force) (seed: uint64) (separation: int) =
    let rng = Rng(seed)
    let cx = g.Width / 2
    let cy = g.Height / 2
    let half = f.Count / 2
    for i in 0 .. f.Count - 1 do
        let team = if i < half then 0 else 1
        let ox = if team = 0 then cx - separation else cx + separation
        let mutable x = 0
        let mutable y = 0
        let mutable placed = false
        let mutable attempts = 0
        while not placed && attempts < 200 do
            x <- max 1 (min (g.Width - 3) (ox + rng.Range(-40, 40)))
            y <- max 1 (min (g.Height - 3) (cy + rng.Range(-60, 60)))
            if not (cellBlocked g 0 x y) then placed <- true
            attempts <- attempts + 1
        f.X.[i] <- x
        f.Y.[i] <- y
        f.Level.[i] <- if g.Levels > 1 then rng.Range(0, g.Levels) else 0
        f.Facing.[i] <- if team = 0 then 0 else 4
        f.Team.[i] <- team

// --------------------------------------------------------------- broad phase

/// Uniform bucket index over the grid. Rebuilt per tick; the rebuild is
/// measured separately so its cost is visible rather than hidden.
type SpatialIndex =
    { CellSize: int
      Cols: int
      Rows: int
      Heads: int array
      Next: int array }

let createIndex (g: Grid) (cellSize: int) (units: int) =
    let cols = (g.Width + cellSize - 1) / cellSize
    let rows = (g.Height + cellSize - 1) / cellSize
    { CellSize = cellSize
      Cols = cols
      Rows = rows
      Heads = Array.create (cols * rows) -1
      Next = Array.create units -1 }

let rebuildIndex (ix: SpatialIndex) (f: Force) =
    System.Array.Fill(ix.Heads, -1)
    for i in 0 .. f.Count - 1 do
        if f.Alive.[i] then
            let bx = f.X.[i] / ix.CellSize
            let by = f.Y.[i] / ix.CellSize
            let b = by * ix.Cols + bx
            ix.Next.[i] <- ix.Heads.[b]
            ix.Heads.[b] <- i

// -------------------------------------------------------------- acquisition

/// Per-observer contact episodes, in fixed slots so nothing allocates and the
/// worst case is bounded rather than open-ended.
[<Literal>]
let MaxEpisodes = 48

type Acquisition =
    { /// Target index per slot, -1 when free.
      Target: int array
      /// Accumulated progress, 0..Threshold.
      Progress: int array
      /// Slots currently used per observer.
      Used: int array }

let createAcquisition (units: int) =
    { Target = Array.create (units * MaxEpisodes) -1
      Progress = Array.zeroCreate (units * MaxEpisodes)
      Used = Array.zeroCreate units }

[<Literal>]
let Threshold = 100

type Counters =
    { mutable Candidates: int
      mutable SectorPassed: int
      mutable Rays: int
      mutable RaySteps: int
      mutable Acquired: int
      mutable Decayed: int }

let newCounters () =
    { Candidates = 0; SectorPassed = 0; Rays = 0; RaySteps = 0; Acquired = 0; Decayed = 0 }

/// One tick of perception for every observer.
///
///   broad phase cull  ->  sector test  ->  line of sight  ->  acquisition
///
/// `samplePoints` models a multi-cell footprint's exposure points: 1 treats a
/// unit as a point, 4 traces to each cell of a 2x2 base.
let step
    (g: Grid)
    (f: Force)
    (ix: SpatialIndex)
    (acq: Acquisition)
    (sightRange: int)
    (samplePoints: int)
    (useSector: bool)
    (memo: Cache.Memo option)
    (symmetric: bool)
    (c: Counters)
    =
    let bucketRadius = (sightRange + ix.CellSize - 1) / ix.CellSize

    for o in 0 .. f.Count - 1 do
        if f.Alive.[o] then
            let ox = f.X.[o]
            let oy = f.Y.[o]
            let ol = f.Level.[o]
            let oteam = f.Team.[o]
            let ofacing = f.Facing.[o]
            let bx = ox / ix.CellSize
            let by = oy / ix.CellSize
            let baseSlot = o * MaxEpisodes

            // mark all existing episodes as unseen this tick
            for s in 0 .. MaxEpisodes - 1 do
                if acq.Target.[baseSlot + s] >= 0 then
                    acq.Progress.[baseSlot + s] <- acq.Progress.[baseSlot + s] ||| 0x40000000

            let y0 = max 0 (by - bucketRadius)
            let y1 = min (ix.Rows - 1) (by + bucketRadius)
            let x0 = max 0 (bx - bucketRadius)
            let x1 = min (ix.Cols - 1) (bx + bucketRadius)

            for byy in y0 .. y1 do
                for bxx in x0 .. x1 do
                    let mutable t = ix.Heads.[byy * ix.Cols + bxx]
                    while t >= 0 do
                        if t <> o && f.Team.[t] <> oteam && f.Alive.[t] then
                            c.Candidates <- c.Candidates + 1
                            let tx = f.X.[t]
                            let ty = f.Y.[t]
                            let dist = chebyshev ox oy tx ty
                            if dist <= sightRange then
                                let inArc =
                                    not useSector || inSector ofacing (tx - ox) (ty - oy)
                                if inArc then
                                    c.SectorPassed <- c.SectorPassed + 1
                                    // trace to the target's exposure points,
                                    // consulting the memo when one is supplied
                                    let tl = f.Level.[t]
                                    let cached =
                                        match memo with
                                        | Some m -> Cache.tryGet m symmetric ox oy ol tx ty tl
                                        | None -> -1
                                    let mutable visible = false
                                    if cached >= 0 then visible <- cached = 1
                                    else
                                        let mutable sp = 0
                                        while not visible && sp < samplePoints do
                                            let ax = tx + (sp &&& 1)
                                            let ay = ty + (sp >>> 1)
                                            c.Rays <- c.Rays + 1
                                            c.RaySteps <- c.RaySteps + dist
                                            if tl = ol then
                                                if hasLos g ol ox oy ax ay then visible <- true
                                            else
                                                if hasLosAcrossLevels g ol ox oy tl ax ay then
                                                    visible <- true
                                            sp <- sp + 1
                                        match memo with
                                        | Some m -> Cache.store m symmetric ox oy ol tx ty tl visible
                                        | None -> ()

                                    if visible then
                                        // find or open an episode slot
                                        let mutable slot = -1
                                        let mutable s = 0
                                        while slot < 0 && s < MaxEpisodes do
                                            if acq.Target.[baseSlot + s] = t then slot <- s
                                            s <- s + 1
                                        if slot < 0 then
                                            let mutable s2 = 0
                                            while slot < 0 && s2 < MaxEpisodes do
                                                if acq.Target.[baseSlot + s2] < 0 then slot <- s2
                                                s2 <- s2 + 1
                                            if slot >= 0 then
                                                acq.Target.[baseSlot + slot] <- t
                                                acq.Progress.[baseSlot + slot] <- 0
                                        if slot >= 0 then
                                            let p = (acq.Progress.[baseSlot + slot] &&& 0x3FFFFFFF)
                                            // rate falls off with range; a nearer,
                                            // attended contact resolves faster
                                            let rate = 6 + (sightRange - dist) * 20 / sightRange
                                            let np = min Threshold (p + rate)
                                            if p < Threshold && np >= Threshold then
                                                c.Acquired <- c.Acquired + 1
                                            acq.Progress.[baseSlot + slot] <- np
                        t <- ix.Next.[t]

            // decay episodes not refreshed this tick
            for s in 0 .. MaxEpisodes - 1 do
                let v = acq.Progress.[baseSlot + s]
                if acq.Target.[baseSlot + s] >= 0 && (v &&& 0x40000000) <> 0 then
                    let p = (v &&& 0x3FFFFFFF) - 8
                    if p <= 0 then
                        acq.Target.[baseSlot + s] <- -1
                        acq.Progress.[baseSlot + s] <- 0
                        c.Decayed <- c.Decayed + 1
                    else
                        acq.Progress.[baseSlot + s] <- p
