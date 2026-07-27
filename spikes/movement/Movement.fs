module Movement

open World
open Clearance

/// Units as struct-of-arrays. Positions are anchors of an N x N footprint.
type Force =
    { Count: int
      X: int array
      Y: int array
      Level: int array
      Team: int array
      /// fixed-point movement credit, 256 = one cell
      Credit: int array
      /// cells per 256 ticks of credit accrual
      Speed: int array
      Path: int array
      PathLen: int array
      PathIdx: int array
      /// consecutive ticks this unit has wanted to move and could not
      Blocked: int array
      Alive: bool array }

[<Literal>]
let CreditPerCell = 256

[<Literal>]
let MaxPath = 512

let createForce (n: int) =
    { Count = n
      X = Array.zeroCreate n
      Y = Array.zeroCreate n
      Level = Array.zeroCreate n
      Team = Array.zeroCreate n
      Credit = Array.zeroCreate n
      Speed = Array.create n 40
      Path = Array.zeroCreate (n * MaxPath)
      PathLen = Array.zeroCreate n
      PathIdx = Array.zeroCreate n
      Blocked = Array.zeroCreate n
      Alive = Array.create n true }

/// Space-time reservation over the committed tick only, plus the next tick as a
/// one-step lookahead. A cell is claimed by at most one unit, stamped by tick so
/// the table never needs clearing.
type Reservations =
    { Owner: int array
      Stamp: int array
      mutable Current: int }

let createReservations (g: Grid) =
    let n = g.Width * g.Height * g.Levels
    { Owner = Array.create n -1; Stamp = Array.zeroCreate n; Current = 0 }

let inline private claim (r: Reservations) (i: int) (unit: int) =
    if r.Stamp.[i] = r.Current then r.Owner.[i] = unit
    else
        r.Stamp.[i] <- r.Current
        r.Owner.[i] <- unit
        true

let inline private ownerOf (r: Reservations) (i: int) =
    if r.Stamp.[i] = r.Current then r.Owner.[i] else -1

type Counters =
    { mutable Attempted: int
      mutable Committed: int
      mutable BlockedTerrain: int
      mutable BlockedFriendly: int
      mutable BlockedHostile: int
      mutable ChainAdvances: int
      mutable Yields: int
      mutable Deadlocks: int
      mutable Passes: int }

let newCounters () =
    { Attempted = 0; Committed = 0; BlockedTerrain = 0; BlockedFriendly = 0
      BlockedHostile = 0; ChainAdvances = 0; Yields = 0; Deadlocks = 0; Passes = 0 }

/// One tick of movement.
///
///   accrue credit  ->  collect intents  ->  resolve dependencies  ->  commit
///
/// Friendly units are moving reservations: a follower may enter space a leader
/// is guaranteed to vacate on the same tick, which is what lets a column
/// advance coherently instead of shuffling one unit per tick. That is resolved
/// by repeated passes over the pending set until nothing more can commit, which
/// is the deterministic equivalent of a dependency-ordered walk.
///
/// Hostile conflicts are symmetric: neither side gets identifier priority and
/// both stay put.
let step
    (g: Grid)
    (p: Profile)
    (f: Force)
    (r: Reservations)
    (intentUnit: int array)
    (intentDx: int array)
    (intentDy: int array)
    (committed: bool array)
    (c: Counters)
    =
    r.Current <- r.Current + 1
    let s = p.Size

    // ---- accrue credit and collect this tick's intended transitions
    let mutable pending = 0
    for i in 0 .. f.Count - 1 do
        if f.Alive.[i] && f.PathIdx.[i] < f.PathLen.[i] then
            // a blocked unit keeps its credit but does not bank it without
            // limit, so blocking delays a step rather than storing up steps
            f.Credit.[i] <- min (f.Credit.[i] + f.Speed.[i]) (CreditPerCell * 2 - 1)
            if f.Credit.[i] >= CreditPerCell then
                let next = f.Path.[i * MaxPath + f.PathIdx.[i]]
                let nx = next % g.Width
                let ny = next / g.Width
                let dx = sign (nx - f.X.[i])
                let dy = sign (ny - f.Y.[i])
                if dx <> 0 || dy <> 0 then
                    intentUnit.[pending] <- i
                    intentDx.[pending] <- dx
                    intentDy.[pending] <- dy
                    committed.[pending] <- false
                    pending <- pending + 1
                else
                    // already on the waypoint; consume it without moving
                    f.PathIdx.[i] <- f.PathIdx.[i] + 1

    c.Attempted <- c.Attempted + pending

    // ---- claim the footprint every unit currently occupies
    for i in 0 .. f.Count - 1 do
        if f.Alive.[i] then
            let lv = f.Level.[i]
            for dy in 0 .. s - 1 do
                for dx in 0 .. s - 1 do
                    claim r (idx g lv (f.X.[i] + dx) (f.Y.[i] + dy)) i |> ignore

    // ---- resolve: repeated passes until no further transition can commit
    let mutable progress = true
    let mutable passes = 0
    while progress && passes < 8 do
        progress <- false
        passes <- passes + 1
        for k in 0 .. pending - 1 do
            if not committed.[k] then
                let i = intentUnit.[k]
                let dx = intentDx.[k]
                let dy = intentDy.[k]
                let lv = f.Level.[i]
                let x = f.X.[i]
                let y = f.Y.[i]

                if not (transitionAllowed p g lv x y dx dy) then
                    c.BlockedTerrain <- c.BlockedTerrain + 1
                    committed.[k] <- true // permanently blocked this tick
                    f.Blocked.[i] <- f.Blocked.[i] + 1
                else
                    // the destination footprint, minus cells we already hold
                    let nx = x + dx
                    let ny = y + dy
                    let mutable free = true
                    let mutable hostile = false
                    for ddy in 0 .. s - 1 do
                        for ddx in 0 .. s - 1 do
                            let cx = nx + ddx
                            let cy = ny + ddy
                            let inSelf = cx >= x && cx < x + s && cy >= y && cy < y + s
                            if not inSelf then
                                let o = ownerOf r (idx g lv cx cy)
                                if o >= 0 && o <> i then
                                    free <- false
                                    if f.Team.[o] <> f.Team.[i] then hostile <- true

                    if free then
                        // release vacated cells, claim entered cells
                        for ddy in 0 .. s - 1 do
                            for ddx in 0 .. s - 1 do
                                let cx = x + ddx
                                let cy = y + ddy
                                let stillIn = cx >= nx && cx < nx + s && cy >= ny && cy < ny + s
                                if not stillIn then
                                    let ii = idx g lv cx cy
                                    r.Stamp.[ii] <- r.Current
                                    r.Owner.[ii] <- -1
                        for ddy in 0 .. s - 1 do
                            for ddx in 0 .. s - 1 do
                                claim r (idx g lv (nx + ddx) (ny + ddy)) i |> ignore
                        f.X.[i] <- nx
                        f.Y.[i] <- ny
                        f.Credit.[i] <- f.Credit.[i] - CreditPerCell
                        f.Blocked.[i] <- 0
                        if nx = (f.Path.[i * MaxPath + f.PathIdx.[i]] % g.Width)
                           && ny = (f.Path.[i * MaxPath + f.PathIdx.[i]] / g.Width) then
                            f.PathIdx.[i] <- f.PathIdx.[i] + 1
                        committed.[k] <- true
                        c.Committed <- c.Committed + 1
                        if passes > 1 then c.ChainAdvances <- c.ChainAdvances + 1
                        progress <- true
                    elif hostile then
                        c.BlockedHostile <- c.BlockedHostile + 1
                        committed.[k] <- true
                        f.Blocked.[i] <- f.Blocked.[i] + 1

    c.Passes <- c.Passes + passes

    // anything still pending was blocked by a friendly that never moved
    for k in 0 .. pending - 1 do
        if not committed.[k] then
            let i = intentUnit.[k]
            c.BlockedFriendly <- c.BlockedFriendly + 1
            f.Blocked.[i] <- f.Blocked.[i] + 1
            // persistent wait: the server detects it and picks a unit to replan
            if f.Blocked.[i] > 20 then
                c.Deadlocks <- c.Deadlocks + 1
                c.Yields <- c.Yields + 1
                f.Blocked.[i] <- 0
                f.PathIdx.[i] <- f.PathLen.[i] // abandon; caller will replan
