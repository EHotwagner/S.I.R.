module Cache

/// Open-addressed line-of-sight memo. Keys pack both endpoints and their
/// levels into 44 bits; values are visible/not plus a validity stamp.
///
/// Two strategies are measurable here, and they are different things:
///
///   symmetry  - geometric line of sight is symmetric, so an unordered pair
///               need only be traced once per tick even though acquisition is
///               evaluated separately in each direction.
///   temporal  - a pair's result stays valid while neither endpoint moves and
///               the spatial revision is unchanged.
type Memo =
    { Keys: int64 array
      Vals: byte array
      Stamp: int array
      Mask: int
      mutable Current: int
      mutable Hits: int
      mutable Misses: int
      mutable Evictions: int }

let create (capacityPow2: int) =
    let n = 1 <<< capacityPow2
    { Keys = Array.create n -1L
      Vals = Array.zeroCreate n
      Stamp = Array.zeroCreate n
      Mask = n - 1
      Current = 1
      Hits = 0
      Misses = 0
      Evictions = 0 }

let inline private pack (ox: int) (oy: int) (ol: int) (tx: int) (ty: int) (tl: int) =
    (int64 ox)
    ||| ((int64 oy) <<< 10)
    ||| ((int64 ol) <<< 20)
    ||| ((int64 tx) <<< 23)
    ||| ((int64 ty) <<< 33)
    ||| ((int64 tl) <<< 43)

/// Canonical ordering so A->B and B->A hit the same slot.
let inline private packSymmetric ox oy ol tx ty tl =
    let a = (int64 ox) ||| ((int64 oy) <<< 10) ||| ((int64 ol) <<< 20)
    let b = (int64 tx) ||| ((int64 ty) <<< 10) ||| ((int64 tl) <<< 20)
    if a <= b then pack ox oy ol tx ty tl else pack tx ty tl ox oy ol

let inline private hash (k: int64) =
    let mutable h = uint64 k
    h <- h ^^^ (h >>> 33)
    h <- h * 0xff51afd7ed558ccdUL
    h <- h ^^^ (h >>> 33)
    int h

/// Advance to a new validity generation. Called when the spatial revision
/// changes, which invalidates every stored result at once.
let invalidateAll (m: Memo) = m.Current <- m.Current + 1

/// Returns -1 on miss, 0 or 1 on hit.
let inline tryGet (m: Memo) (symmetric: bool) ox oy ol tx ty tl =
    let k = if symmetric then packSymmetric ox oy ol tx ty tl else pack ox oy ol tx ty tl
    let i = (hash k) &&& m.Mask
    if m.Keys.[i] = k && m.Stamp.[i] = m.Current then
        m.Hits <- m.Hits + 1
        int m.Vals.[i]
    else
        m.Misses <- m.Misses + 1
        -1

let inline store (m: Memo) (symmetric: bool) ox oy ol tx ty tl (visible: bool) =
    let k = if symmetric then packSymmetric ox oy ol tx ty tl else pack ox oy ol tx ty tl
    let i = (hash k) &&& m.Mask
    if m.Keys.[i] <> -1L && m.Keys.[i] <> k && m.Stamp.[i] = m.Current then
        m.Evictions <- m.Evictions + 1
    m.Keys.[i] <- k
    m.Vals.[i] <- (if visible then 1uy else 0uy)
    m.Stamp.[i] <- m.Current

let resetCounters (m: Memo) =
    m.Hits <- 0
    m.Misses <- 0
    m.Evictions <- 0
