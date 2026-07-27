module Pathfinding

open World
open Clearance

/// Eight-way A* with **equal orthogonal and diagonal step cost**, which is the
/// canonical Chebyshev rule and the reason the generic weighted eight-way A*
/// in the framework is not usable as-is.
///
/// Edge-aware: a step is only expanded when the footprint may legally cross the
/// edges the transition touches.
///
/// Preallocated per searcher, so a search allocates nothing.
type Searcher =
    { Width: int
      Height: int
      G: int array
      Came: int array
      Stamp: int array
      mutable Current: int
      /// binary heap of (f, node) packed into one int64: f in the high bits
      Heap: int64 array
      mutable HeapCount: int
      mutable Expanded: int
      mutable Pushed: int }

let create (g: Grid) =
    let n = g.Width * g.Height
    { Width = g.Width
      Height = g.Height
      G = Array.zeroCreate n
      Came = Array.create n -1
      Stamp = Array.zeroCreate n
      Current = 0
      Heap = Array.zeroCreate (n / 2)
      HeapCount = 0
      Expanded = 0
      Pushed = 0 }

let inline private push (s: Searcher) (f: int) (node: int) =
    if s.HeapCount < s.Heap.Length then
        let mutable i = s.HeapCount
        s.Heap.[i] <- ((int64 f) <<< 32) ||| int64 node
        s.HeapCount <- s.HeapCount + 1
        let mutable go = true
        while go && i > 0 do
            let parent = (i - 1) >>> 1
            if s.Heap.[parent] > s.Heap.[i] then
                let t = s.Heap.[parent]
                s.Heap.[parent] <- s.Heap.[i]
                s.Heap.[i] <- t
                i <- parent
            else go <- false

let inline private pop (s: Searcher) =
    let top = s.Heap.[0]
    s.HeapCount <- s.HeapCount - 1
    s.Heap.[0] <- s.Heap.[s.HeapCount]
    let mutable i = 0
    let mutable go = true
    while go do
        let l = 2 * i + 1
        let r = l + 1
        let mutable m = i
        if l < s.HeapCount && s.Heap.[l] < s.Heap.[m] then m <- l
        if r < s.HeapCount && s.Heap.[r] < s.Heap.[m] then m <- r
        if m <> i then
            let t = s.Heap.[m]
            s.Heap.[m] <- s.Heap.[i]
            s.Heap.[i] <- t
            i <- m
        else go <- false
    struct (int (top >>> 32), int (top &&& 0xFFFFFFFFL))

let inline private chebyshev x0 y0 x1 y1 = max (abs (x1 - x0)) (abs (y1 - y0))

/// Returns path length in steps, or -1 when unreachable within `maxExpand`.
/// The path is written into `outPath` as packed y*W+x, goal first.
let search
    (s: Searcher)
    (g: Grid)
    (p: Profile)
    (level: int)
    (sx: int) (sy: int)
    (gx: int) (gy: int)
    (maxExpand: int)
    (outPath: int array)
    =
    s.Current <- s.Current + 1
    s.HeapCount <- 0
    s.Expanded <- 0
    s.Pushed <- 0

    let W = s.Width
    let start = sy * W + sx
    let goal = gy * W + gx

    if not (canStand p g level sx sy) || not (canStand p g level gx gy) then -1
    else
        s.G.[start] <- 0
        s.Came.[start] <- -1
        s.Stamp.[start] <- s.Current
        push s (chebyshev sx sy gx gy) start
        s.Pushed <- s.Pushed + 1

        let mutable found = false
        let mutable exhausted = false

        while not found && not exhausted && s.HeapCount > 0 do
            let struct (_, node) = pop s
            if node = goal then found <- true
            else
                s.Expanded <- s.Expanded + 1
                if s.Expanded > maxExpand then exhausted <- true
                else
                    let cx = node % W
                    let cy = node / W
                    let gc = s.G.[node]
                    let mutable dy = -1
                    while dy <= 1 do
                        let mutable dx = -1
                        while dx <= 1 do
                            if dx <> 0 || dy <> 0 then
                                let nx = cx + dx
                                let ny = cy + dy
                                if nx >= 0 && ny >= 0 && nx < W && ny < s.Height then
                                    let nn = ny * W + nx
                                    let ng = gc + 1
                                    if (s.Stamp.[nn] <> s.Current || ng < s.G.[nn])
                                       && transitionAllowed p g level cx cy dx dy then
                                        s.Stamp.[nn] <- s.Current
                                        s.G.[nn] <- ng
                                        s.Came.[nn] <- node
                                        push s (ng + chebyshev nx ny gx gy) nn
                                        s.Pushed <- s.Pushed + 1
                            dx <- dx + 1
                        dy <- dy + 1

        if not found then -1
        else
            let mutable n = goal
            let mutable len = 0
            while n <> -1 && len < outPath.Length do
                outPath.[len] <- n
                len <- len + 1
                n <- s.Came.[n]
            len
