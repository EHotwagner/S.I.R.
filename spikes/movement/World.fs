module World

/// A battlefield in the canonical spatial model: cell terrain for volumes,
/// semantic edges for thin structures, and a small number of discrete levels.
///
/// Layout is flat and struct-of-arrays throughout. Nothing in the hot path
/// allocates.
[<Literal>]
let Opaque = 1uy

[<Literal>]
let Clear = 0uy

type Grid =
    { Width: int
      Height: int
      Levels: int
      /// Cell blockers, indexed level * W * H + y * W + x.
      Cells: byte array
      /// Vertical edges: VEdge[i] separates cell (x,y) from (x+1,y).
      VEdge: byte array
      /// Horizontal edges: HEdge[i] separates cell (x,y) from (x,y+1).
      HEdge: byte array
      /// Floor between level L and L+1 at (x,y). Opaque blocks sight between
      /// levels; a hole or grating does not.
      Floor: byte array }

let inline idx (g: Grid) level x y = (level * g.Height + y) * g.Width + x

let create width height levels =
    let n = width * height * levels
    { Width = width
      Height = height
      Levels = levels
      Cells = Array.zeroCreate n
      VEdge = Array.zeroCreate n
      HEdge = Array.zeroCreate n
      Floor = Array.create n Opaque }

let inline inBounds (g: Grid) x y = x >= 0 && y >= 0 && x < g.Width && y < g.Height

let inline cellBlocked (g: Grid) level x y = g.Cells.[idx g level x y] <> Clear

/// Deterministic generator, so a run is reproducible.
type Rng(seed: uint64) =
    let mutable s = seed
    member _.Next() =
        s <- s * 6364136223846793005UL + 1442695040888963407UL
        int ((s >>> 33) &&& 0x7FFFFFFFUL)
    member this.Range(lo, hi) = if hi <= lo then lo else lo + (this.Next() % (hi - lo))
    member this.Chance(pct: int) = this.Next() % 100 < pct

/// Builds an urban block layout: buildings whose exterior and interior walls
/// are edge features with door and window openings, plus scattered
/// cell-occupying cover. This is the shape that makes edge-aware line of sight
/// expensive, which is the point of measuring it.
let generateUrban (g: Grid) (seed: uint64) =
    let rng = Rng(seed)
    let W, H = g.Width, g.Height

    let inline setV level x y v =
        if x >= 0 && x < W - 1 && y >= 0 && y < H then g.VEdge.[idx g level x y] <- v
    let inline setH level x y v =
        if x >= 0 && x < W && y >= 0 && y < H - 1 then g.HEdge.[idx g level x y] <- v

    // Buildings on a coarse street grid.
    let block = 48
    let street = 14
    for level in 0 .. g.Levels - 1 do
        let mutable by = 4
        while by + block < H do
            let mutable bx = 4
            while bx + block < W do
                let w = block - street
                let h = block - street
                // exterior walls as edges, with openings
                for x in bx .. bx + w - 1 do
                    if not (rng.Chance 6) then setH level x (by - 1) Opaque
                    if not (rng.Chance 6) then setH level x (by + h - 1) Opaque
                for y in by .. by + h - 1 do
                    if not (rng.Chance 6) then setV level (bx - 1) y Opaque
                    if not (rng.Chance 6) then setV level (bx + w - 1) y Opaque
                // interior partitions
                let rooms = rng.Range(2, 4)
                for r in 1 .. rooms do
                    let cut = bx + (w * r) / (rooms + 1)
                    for y in by .. by + h - 1 do
                        if not (rng.Chance 12) then setV level cut y Opaque
                    let cutY = by + (h * r) / (rooms + 1)
                    for x in bx .. bx + w - 1 do
                        if not (rng.Chance 12) then setH level x cutY Opaque
                // a few solid cells: machinery, rubble, pillars
                for _ in 1 .. 10 do
                    let px = rng.Range(bx, bx + w)
                    let py = rng.Range(by, by + h)
                    if inBounds g px py then g.Cells.[idx g level px py] <- Opaque
                // stair openings and floor holes between levels
                if level < g.Levels - 1 then
                    for _ in 1 .. 3 do
                        let px = rng.Range(bx, bx + w)
                        let py = rng.Range(by, by + h)
                        for dx in 0 .. 3 do
                            for dy in 0 .. 3 do
                                if inBounds g (px + dx) (py + dy) then
                                    g.Floor.[idx g level (px + dx) (py + dy)] <- Clear
                bx <- bx + block
            by <- by + block

    // street furniture outside buildings
    for level in 0 .. g.Levels - 1 do
        for _ in 1 .. (W * H / 900) do
            let px = rng.Range(0, W)
            let py = rng.Range(0, H)
            if inBounds g px py then g.Cells.[idx g level px py] <- Opaque

/// Occluder density, reported so results can be compared against a map's
/// actual complexity rather than an assumed one.
let describe (g: Grid) =
    let n = g.Width * g.Height * g.Levels
    let cells = g.Cells |> Array.sumBy (fun b -> if b <> Clear then 1 else 0)
    let v = g.VEdge |> Array.sumBy (fun b -> if b <> Clear then 1 else 0)
    let h = g.HEdge |> Array.sumBy (fun b -> if b <> Clear then 1 else 0)
    struct (float cells / float n * 100.0, float (v + h) / float (n * 2) * 100.0)
