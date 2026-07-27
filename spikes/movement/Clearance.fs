module Clearance

open World

/// Footprint-aware traversability, precomputed once per map and movement
/// profile. A unit with an N x N base can stand anchored at (x,y) only when
/// every cell of that base is clear.
type Profile =
    { Size: int
      /// CanStand[level][y*W+x]
      CanStand: bool array }

let build (g: Grid) (size: int) =
    let n = g.Width * g.Height * g.Levels
    let stand = Array.zeroCreate<bool> n
    for level in 0 .. g.Levels - 1 do
        for y in 0 .. g.Height - size do
            for x in 0 .. g.Width - size do
                let mutable ok = true
                for dy in 0 .. size - 1 do
                    for dx in 0 .. size - 1 do
                        if g.Cells.[idx g level (x + dx) (y + dy)] <> Clear then ok <- false
                stand.[idx g level x y] <- ok
    { Size = size; CanStand = stand }

let inline canStand (p: Profile) (g: Grid) level x y =
    x >= 0 && y >= 0 && x <= g.Width - p.Size && y <= g.Height - p.Size
    && p.CanStand.[idx g level x y]

/// Can an N x N base anchored at (x,y) step by (dx,dy)?
///
/// An orthogonal step crosses N edges along the leading face. A diagonal step
/// also passes N-1 vertices, and is refused when both flanking edges at a
/// vertex are closed, matching the corner rule the spatial model applies.
let transitionAllowed (p: Profile) (g: Grid) level x y dx dy =
    let s = p.Size
    let nx = x + dx
    let ny = y + dy
    if not (canStand p g level nx ny) then false
    else
        let mutable ok = true

        if dx <> 0 then
            // vertical edges crossed by the leading column
            let ex = if dx > 0 then x + s - 1 else x - 1
            if ex < 0 || ex >= g.Width - 1 then ok <- false
            else
                for i in 0 .. s - 1 do
                    if g.VEdge.[idx g level ex (y + i)] <> Clear then ok <- false

        if ok && dy <> 0 then
            let ey = if dy > 0 then y + s - 1 else y - 1
            if ey < 0 || ey >= g.Height - 1 then ok <- false
            else
                for i in 0 .. s - 1 do
                    if g.HEdge.[idx g level (x + i) ey] <> Clear then ok <- false

        ok
