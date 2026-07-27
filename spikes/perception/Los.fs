module Los

open World

/// Edge-aware line of sight.
///
/// A ray is blocked by an opaque cell it enters, and by an opaque edge it
/// crosses. A diagonal step passes a vertex and is blocked when both flanking
/// edges are closed, matching the corner rule the spatial model already applies
/// to movement.
///
/// Integer only, no allocation.
let hasLos (g: Grid) (level: int) (x0: int) (y0: int) (x1: int) (y1: int) =
    if x0 = x1 && y0 = y1 then true
    else
        let dx = abs (x1 - x0)
        let dy = abs (y1 - y0)
        let sx = if x0 < x1 then 1 else -1
        let sy = if y0 < y1 then 1 else -1
        let mutable x = x0
        let mutable y = y0
        let mutable err = dx - dy
        let mutable blocked = false
        let mutable running = true

        while running && not blocked do
            if x = x1 && y = y1 then running <- false
            else
                let e2 = err <<< 1
                let stepX = e2 > -dy
                let stepY = e2 < dx

                if stepX && stepY then
                    // diagonal: blocked only when both flanking edges are closed
                    let vi = if sx > 0 then idx g level x y else idx g level (x - 1) y
                    let hi = if sy > 0 then idx g level x y else idx g level x (y - 1)
                    let vBlocked =
                        (sx > 0 && x < g.Width - 1) || (sx < 0 && x > 0)
                        |> function true -> g.VEdge.[vi] <> Clear | false -> true
                    let hBlocked =
                        (sy > 0 && y < g.Height - 1) || (sy < 0 && y > 0)
                        |> function true -> g.HEdge.[hi] <> Clear | false -> true
                    if vBlocked && hBlocked then blocked <- true
                    else
                        err <- err - dy + dx
                        x <- x + sx
                        y <- y + sy
                elif stepX then
                    let vi = if sx > 0 then idx g level x y else idx g level (x - 1) y
                    let ok = (sx > 0 && x < g.Width - 1) || (sx < 0 && x > 0)
                    if not ok || g.VEdge.[vi] <> Clear then blocked <- true
                    else
                        err <- err - dy
                        x <- x + sx
                else
                    let hi = if sy > 0 then idx g level x y else idx g level x (y - 1)
                    let ok = (sy > 0 && y < g.Height - 1) || (sy < 0 && y > 0)
                    if not ok || g.HEdge.[hi] <> Clear then blocked <- true
                    else
                        err <- err + dx
                        y <- y + sy

                if not blocked && running then
                    if g.Cells.[idx g level x y] <> Clear then blocked <- true

        not blocked

/// Cross-level sight: only where the intervening floors are open, and only
/// along the shared column. Deliberately conservative — enough to measure what
/// a third dimension costs, not a finished vertical visibility rule.
let hasLosAcrossLevels (g: Grid) (l0: int) (x0: int) (y0: int) (l1: int) (x1: int) (y1: int) =
    if l0 = l1 then hasLos g l0 x0 y0 x1 y1
    else
        let lo = min l0 l1
        let hi = max l0 l1
        let mutable openPath = true
        for l in lo .. hi - 1 do
            if g.Floor.[idx g l x1 y1] <> Clear then openPath <- false
        openPath && hasLos g l1 x1 y1 x1 y1 && hasLos g l0 x0 y0 x0 y0

/// Cheap rejection before any ray is walked.
let inline chebyshev x0 y0 x1 y1 = max (abs (x1 - x0)) (abs (y1 - y0))

/// Eight-direction attention sector test. A target is in sector when the
/// dominant axis of the offset matches the attended direction, or one of its
/// neighbours, giving a hard-edged ~135 degree forward arc.
let inline inSector (facing: int) (dx: int) (dy: int) =
    if dx = 0 && dy = 0 then true
    else
        // octant of the offset, 0 = east, counter-clockwise
        let oct =
            if dx > 0 && dy = 0 then 0
            elif dx > 0 && dy > 0 then 1
            elif dx = 0 && dy > 0 then 2
            elif dx < 0 && dy > 0 then 3
            elif dx < 0 && dy = 0 then 4
            elif dx < 0 && dy < 0 then 5
            elif dx = 0 && dy < 0 then 6
            else 7
        let d = abs (oct - facing)
        let d = if d > 4 then 8 - d else d
        d <= 1
