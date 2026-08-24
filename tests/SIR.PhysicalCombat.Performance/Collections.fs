/// Collection-strategy regression benchmarks for the authoritative simulation hot paths.
///
/// These exist because the shapes they measure are the ones that silently reintroduce
/// super-linear per-tick cost as a map grows (S.I.R.#249). They are RATIO gates, not absolute
/// budgets: absolute nanoseconds vary by host, but the ordering between strategies does not.
///
/// WHAT THIS GATE DOES AND DOES NOT GUARD, stated because a `Subject:` line naming a production
/// function invites the stronger reading and the stronger reading is false (S.I.R.#263).
///
///   IT GUARDS the ORDERING CLAIM #249 rests on: that at the sizes the simulation actually
///   reaches, an indexed strategy still beats the shape in use by the margin that was measured.
///   Break a strategy here -- flatten the binary search to a linear scan, say -- and the gate
///   reds and names the assertion that failed. That is the subject, and it is inverted in review.
///
///   IT DOES NOT GUARD the production call sites. Every strategy below is a local
///   reimplementation; nothing in this file calls SpatialQuery.boundaryAt, distinctCells, or
///   TacticalSceneProjection. A regression inside those functions will NOT red this gate. The
///   `Subject:` comments name where each shape is USED, so that a reader knows which call site
///   the ordering claim is about -- they are not a claim that the call site is under test.
///
///   AND THE ROUTING FOLLOWS FROM THAT, deliberately. This project's only ProjectReference is
///   src/SIR.Simulation (see the .fsproj), so that IS a compile input of this gate -- but a
///   `domain`-classified route changes src/SIR.Simulation WITHOUT selecting `collection-strategies`,
///   and that is intended rather than an accident of the classifier. Nothing here calls into it:
///   EVERY occurrence of SpatialQuery, Simulation or TacticalSceneProjection in this file is a
///   comment or a string literal (the `case` labels), and the only opens are System,
///   System.Diagnostics, System.Collections.Generic and FS.GG.Game.Core -- so every type and
///   function the measured loops touch, Edge and Cell included, comes from the package. So a change
///   in src/SIR.Simulation cannot move a ratio; it can only break the BUILD, which prepare-native
///   already covers wherever SIR.slnx is compiled.
///   (Stated this way after review: an earlier revision said "the only external call in any measured
///   loop is Edges.edgeBetween", which is false in both halves -- both edgeBetween sites are fixture
///   SETUP, not measured loops, and the measured loops do call List.exists, Map.containsKey,
///   Set.ofArray and package Edge/Cell equality and hashing. The conclusion holds independently and
///   more strongly. No line numbers are cited on purpose: the commit that added this note moved the
///   very lines an earlier revision of it pinned.) Selecting on it would re-measure, on every domain PR, a number that could
///   not have changed. Recorded here because a row whose whole subject is "a regression gate nothing
///   runs is indistinguishable from a gate that passes" cannot leave its own non-selection unstated.
///
/// The second failure direction is the load-bearing one: a benchmark that quietly stops measuring
/// anything collapses its own ratios, so `list-is-not-viable-at-MaxEdges` and its siblings red on
/// a dead harness exactly as they do on a reintroduced slow shape.
///
/// Run: dotnet run --project tests/SIR.PhysicalCombat.Performance -c Release -- --collections
/// CI:  scripts/verify-collection-strategies.sh (which also guards the runtime the numbers are
///      measured on -- see that script's environment confound guard).
module SIR.PhysicalCombat.Performance.Collections

open System
open System.Diagnostics
open System.Collections.Generic
open FS.GG.Game.Core

// ---------------------------------------------------------------- receipt shape

[<CLIMutable>]
type StrategySample =
    { Strategy: string
      NanosecondsPerOperation: float
      RatioToBest: float }

[<CLIMutable>]
type CaseSample =
    { Case: string
      Subject: string
      Size: int
      Secondary: int
      Best: string
      Strategies: StrategySample array }

[<CLIMutable>]
type CollectionsReceipt =
    { SchemaVersion: int
      RuntimeVersion: string
      TieredCompilation: string
      Trials: int
      Cases: CaseSample array
      Assertions: string array
      Failures: string array }

let schemaVersion = 1

// ---------------------------------------------------------------- timing

/// 7, RESTORED. An earlier revision of this row cut it to 3 and justified that by the feedback
/// critical path, on the reasoning that this gate ran inside the wave-1 `integrity` job. THAT
/// JUSTIFICATION IS STALE: the gate is a wave-2 subject with its own job, measured at 56s against a
/// wave-2 maximum of 2m14s, so the trials it needs are very nearly free.
///
/// AND THE REDUNDANCY WAS LOAD-BEARING, which the cut discovered the expensive way. The minimum of N
/// is the statistic least polluted by a slow first pass, so trials are exactly what absorbs a
/// tiering-warmed outlier. Measured against the direct-dispatch route with tiering left at its
/// default: at 7 trials, 0 of 8 runs red; at 3 trials, 8 of 8 red. The cut did not cause the tiering
/// confound -- `scripts/verify-collection-strategies.sh` guards that with a hard abort now -- but it
/// removed the margin that had been masking it, and a gate whose correctness depends on nobody
/// removing an unstated margin is a gate waiting to red on a clean tree.
///
/// Override with SIR_COLLECTIONS_TRIALS when investigating locally.
let private trials =
    match Environment.GetEnvironmentVariable "SIR_COLLECTIONS_TRIALS" with
    | null | "" -> 7
    | value -> match Int32.TryParse value with | true, v when v > 0 -> v | _ -> 7

/// Minimum-of-N wall time in nanoseconds per operation. Minimum, not mean: it is the statistic
/// least polluted by scheduler preemption, and these are ratio comparisons.
let private timeMin (opsPerTrial: int64) (run: unit -> int) =
    let mutable sink = 0
    for _ in 1 .. 3 do
        sink <- sink + run ()
    let mutable best = Double.MaxValue
    for _ in 1 .. trials do
        let sw = Stopwatch.StartNew()
        sink <- sink + run ()
        sw.Stop()
        let ns = sw.Elapsed.TotalMilliseconds * 1e6 / float opsPerTrial
        if ns < best then best <- ns
    if sink = Int32.MinValue then failwith "unreachable; defeats dead-code elimination"
    best

let private case name subject size secondary (measured: (string * float) list) =
    let best = measured |> List.minBy snd
    { Case = name
      Subject = subject
      Size = size
      Secondary = secondary
      Best = fst best
      Strategies =
        measured
        |> List.map (fun (s, ns) ->
            { Strategy = s; NanosecondsPerOperation = ns; RatioToBest = ns / snd best })
        |> Array.ofList }

// ---------------------------------------------------------------- keys

/// Bit-pack an Edge into one int64. Valid while every coordinate fits 16 bits, which the board
/// bounds guarantee. Layout only — see mixKey before using this as a hash.
let inline packEdge (e: Edge) =
    (int64 (uint16 e.Lo.Col) <<< 48)
    ||| (int64 (uint16 e.Lo.Row) <<< 32)
    ||| (int64 (uint16 e.Hi.Col) <<< 16)
    ||| int64 (uint16 e.Hi.Row)

/// splitmix64 finaliser. REQUIRED before using a packed edge as a Dictionary key: Int64.GetHashCode
/// is (hi ^^^ lo), and for an axis-aligned edge the natural layout collapses that to a handful of
/// distinct values, degrading the hash table to a linear scan.
let inline mixKey (x: int64) =
    let mutable z = uint64 x
    z <- (z ^^^ (z >>> 30)) * 0xBF58476D1CE4E5B9UL
    z <- (z ^^^ (z >>> 27)) * 0x94D049BB133111EBUL
    int64 (z ^^^ (z >>> 31))

let inline private cellOf col row : Cell = { Col = col; Row = row }

let private makeEdges n =
    let rng = Random 20260822
    let side = max 4 (int (ceil (sqrt (float n * 2.0))))
    let seen = HashSet<Edge>()
    let acc = ResizeArray<Edge>()
    let mutable guard = 0
    while acc.Count < n && guard < n * 200 do
        guard <- guard + 1
        let c = rng.Next side
        let r = rng.Next side
        let a = cellOf c r
        let b = if rng.Next 2 = 0 then cellOf (c + 1) r else cellOf c (r + 1)
        match Edges.edgeBetween a b with
        | Some e when seen.Add e -> acc.Add e
        | _ -> ()
    acc.ToArray(), side

// ================================================================ case 1: blocker lookup
//
// Subject: SpatialQuery.boundaryAt / Simulation.blockingEdge — resolve one Edge against the
// declared boundary set. Miss-dominated: most traced edges carry no wall.

let private blockerLookup n =
    let edges, side = makeEdges n
    let rng = Random 7717
    // 10% hits, 90% misses: the real call site, where most traced edges carry no wall and the
    // miss path is what a linear scan pays in full.
    let missEdge () =
        let c = side + 8 + rng.Next 64
        let r = rng.Next (side + 64)
        Edges.edgeBetween (cellOf c r) (cellOf (c + 1) r)
        |> Option.defaultWith (fun () -> failwith "adjacent cells must form an edge")
    let queries =
        Array.init 512 (fun _ ->
            if rng.NextDouble() < 0.10 && edges.Length > 0 then edges[rng.Next edges.Length]
            else missEdge ())

    let asList = List.ofArray edges
    let asMap = edges |> Array.map (fun e -> e, true) |> Map.ofArray
    let asDict =
        let d = Dictionary<Edge, bool>(edges.Length)
        for e in edges do d[e] <- true
        d
    let asDictPacked =
        let d = Dictionary<int64, bool>(edges.Length)
        for e in edges do d[mixKey (packEdge e)] <- true
        d
    let sortedKeys = edges |> Array.map packEdge |> Array.sort

    let reps = max 1 (min 2000 (1_500_000 / (512 * (1 + n / 32))))
    let listReps = max 1 (min 200 (400_000 / (512 * (1 + n))))
    let ops = int64 reps * 512L
    let listOps = int64 listReps * 512L

    let runList () =
        let mutable acc = 0
        for _ in 1 .. listReps do
            for q in queries do
                if asList |> List.exists (fun e -> e = q) then acc <- acc + 1
        acc
    let runMap () =
        let mutable acc = 0
        for _ in 1 .. reps do
            for q in queries do
                if Map.containsKey q asMap then acc <- acc + 1
        acc
    let runDict () =
        let mutable acc = 0
        for _ in 1 .. reps do
            for q in queries do
                if asDict.ContainsKey q then acc <- acc + 1
        acc
    let runDictPacked () =
        let mutable acc = 0
        for _ in 1 .. reps do
            for q in queries do
                if asDictPacked.ContainsKey(mixKey (packEdge q)) then acc <- acc + 1
        acc
    let runBinary () =
        let mutable acc = 0
        for _ in 1 .. reps do
            for q in queries do
                let k = packEdge q
                let mutable lo = 0
                let mutable len = sortedKeys.Length
                while len > 0 do
                    let half = len >>> 1
                    let mid = lo + half
                    if sortedKeys[mid] < k then
                        lo <- mid + 1
                        len <- len - half - 1
                    else len <- half
                if lo < sortedKeys.Length && sortedKeys[lo] = k then acc <- acc + 1
        acc

    case "blocker-lookup" "SpatialQuery.boundaryAt" n 0
        [ "List.exists", timeMin listOps runList
          "Map", timeMin ops runMap
          "Dictionary", timeMin ops runDict
          "Dictionary/packed", timeMin ops runDictPacked
          "Array/binary", timeMin ops runBinary ]

// ================================================================ case 2: id-keyed state update
//
// Subject: Simulation per-tick unit/engagement updates — `Map.add id value` folded over updates.
// The merge-scan strategy is the two-pointer sorted co-iteration: state sorted by ordinal id,
// updates sorted by the same ordinal, one linear pass producing the next state.

[<Struct>]
type private Entity = { Id: int; Health: int; Suppression: int }

let private stateUpdate n updateCount =
    let rng = Random 4242
    let entities = Array.init n (fun i -> { Id = i; Health = 100; Suppression = 0 })
    let ids = Array.init n (fun i -> sprintf "unit-%05d" i)

    // which entities receive an update this tick, ascending by ordinal
    let touched =
        let s = HashSet<int>()
        while s.Count < updateCount do s.Add(rng.Next n) |> ignore
        let a = Array.ofSeq s
        Array.sortInPlace a
        a

    let updatesByOrdinal = touched |> Array.map (fun i -> i, -7)
    let updatesByString = touched |> Array.map (fun i -> ids[i], -7)

    let baseMap = Array.zip ids entities |> Map.ofArray
    let reps = max 1 (min 3000 (600_000 / (1 + n)))
    let ops = int64 reps

    // A. current shape: fold Map.add over the updates
    let runMapFold () =
        let mutable c = 0
        for _ in 1 .. reps do
            let next =
                updatesByString
                |> Array.fold
                    (fun acc (id, delta) ->
                        match Map.tryFind id acc with
                        | Some e -> Map.add id { e with Health = e.Health + delta } acc
                        | None -> acc)
                    baseMap
            c <- c + next.Count
        c

    // B. copy into a Dictionary, then apply
    let runDictCopy () =
        let mutable c = 0
        for _ in 1 .. reps do
            let d = Dictionary<string, Entity>(baseMap.Count)
            for KeyValue (k, v) in baseMap do d[k] <- v
            for (id, delta) in updatesByString do
                match d.TryGetValue id with
                | true, e -> d[id] <- { e with Health = e.Health + delta }
                | _ -> ()
            c <- c + d.Count
        c

    // C. two-pointer merge over arrays ordered by ordinal id — one pass, one allocation
    let runMergeScan () =
        let mutable c = 0
        for _ in 1 .. reps do
            let next = Array.zeroCreate<Entity> entities.Length
            let mutable i = 0
            let mutable u = 0
            while i < entities.Length do
                let e = entities[i]
                if u < updatesByOrdinal.Length && fst updatesByOrdinal[u] = e.Id then
                    next[i] <- { e with Health = e.Health + snd updatesByOrdinal[u] }
                    u <- u + 1
                else next[i] <- e
                i <- i + 1
            c <- c + next.Length
        c

    // D. dense scatter: copy the state array, index updates directly by ordinal
    let runDenseScatter () =
        let mutable c = 0
        for _ in 1 .. reps do
            let next = Array.copy entities
            for (ordinal, delta) in updatesByOrdinal do
                let e = next[ordinal]
                next[ordinal] <- { e with Health = e.Health + delta }
            c <- c + next.Length
        c

    case "state-update" "Simulation per-tick Map.add fold" n updateCount
        [ "Map.add fold", timeMin ops runMapFold
          "Dictionary copy", timeMin ops runDictCopy
          "Array/merge-scan", timeMin ops runMergeScan
          "Array/dense-scatter", timeMin ops runDenseScatter ]

// ================================================================ case 3: line dedupe
//
// Subject: SpatialQuery.distinctCells — List.distinct |> List.sortBy on every traced line.

let private lineDedupe n =
    let rng = Random 5
    let cells = List.init n (fun _ -> cellOf (rng.Next n) (rng.Next n))
    let arr = Array.ofList cells
    let reps = max 1 (min 20000 (600_000 / (1 + n)))
    let ops = int64 reps

    let runListWay () =
        let mutable c = 0
        for _ in 1 .. reps do
            c <- c + (cells |> List.distinct |> List.sortBy (fun x -> x.Row, x.Col)).Length
        c
    let runArrayStructs () =
        let mutable c = 0
        for _ in 1 .. reps do
            let a = Array.copy arr
            Array.sortInPlaceBy (fun (x: Cell) -> struct (x.Row, x.Col)) a
            let mutable w = 0
            for i in 0 .. a.Length - 1 do
                if i = 0 || a[i] <> a[i - 1] then
                    a[w] <- a[i]
                    w <- w + 1
            c <- c + w
        c
    let runPacked () =
        let mutable c = 0
        for _ in 1 .. reps do
            let k = Array.zeroCreate<int64> arr.Length
            for i in 0 .. arr.Length - 1 do
                k[i] <- (int64 arr[i].Row <<< 32) ||| int64 (uint32 arr[i].Col)
            Array.sortInPlace k
            let mutable w = 0
            for i in 0 .. k.Length - 1 do
                if i = 0 || k[i] <> k[i - 1] then
                    k[w] <- k[i]
                    w <- w + 1
            c <- c + w
        c

    case "line-dedupe" "SpatialQuery.distinctCells" n 0
        [ "List.distinct|>sortBy", timeMin ops runListWay
          "Array/struct-sort", timeMin ops runArrayStructs
          "Array/packed-sort", timeMin ops runPacked ]

// ================================================================ case 4: membership probe
//
// Subject: TacticalSceneProjection per-frame Set.ofArray rebuilds (#235) — build an N-element
// Set<string> to answer one membership test.

let private membershipProbe n =
    let all = Array.init n (fun i -> sprintf "unit-%05d" i)
    let probe = all[n / 2]
    let reps = max 1 (min 20000 (400_000 / (1 + n)))
    let ops = int64 reps

    let runSet () =
        let mutable c = 0
        for _ in 1 .. reps do
            if Set.contains probe (Set.ofArray all) then c <- c + 1
        c
    let runHashSet () =
        let mutable c = 0
        for _ in 1 .. reps do
            if HashSet<string>(all).Contains probe then c <- c + 1
        c
    let runArray () =
        let mutable c = 0
        for _ in 1 .. reps do
            if Array.contains probe all then c <- c + 1
        c

    case "membership-probe" "TacticalSceneProjection Set.ofArray" n 1
        [ "Set.ofArray", timeMin ops runSet
          "HashSet", timeMin ops runHashSet
          "Array.contains", timeMin ops runArray ]

// ================================================================ assertions

/// Ratio gates. Each names the shape it forbids and the margin it allows. These are deliberately
/// loose — they exist to catch a reintroduced super-linear shape, not to police a few percent.
let private assess (cases: CaseSample array) =
    let failures = ResizeArray<string>()
    let assertions = ResizeArray<string>()

    let ratio caseName size strategy =
        cases
        |> Array.tryFind (fun c -> c.Case = caseName && c.Size = size)
        |> Option.bind (fun c -> c.Strategies |> Array.tryFind (fun s -> s.Strategy = strategy))
        |> Option.map (fun s -> s.RatioToBest)

    let require name condition detail =
        assertions.Add name
        if not condition then failures.Add(name + ": " + detail)

    // At the declared MaxEdges ceiling the linear scan must be catastrophically behind an index.
    match ratio "blocker-lookup" 16384 "List.exists", ratio "blocker-lookup" 16384 "Array/binary" with
    | Some listRatio, Some binRatio ->
        require
            "blocker-lookup/list-is-not-viable-at-MaxEdges"
            (listRatio > 100.0)
            (sprintf "List.exists ratio %.1f is suspiciously close to the best strategy; the benchmark may be measuring nothing" listRatio)
        require
            "blocker-lookup/index-is-flat-at-MaxEdges"
            (binRatio < 4.0)
            (sprintf "Array/binary ratio %.1f exceeds the flatness margin" binRatio)
    | _ -> failures.Add "blocker-lookup: expected a 16384 sample"

    // F# Map must not be chosen for edge lookup; record the measured penalty so the choice stays informed.
    match ratio "blocker-lookup" 16384 "Map" with
    | Some mapRatio ->
        require
            "blocker-lookup/map-penalty-recorded"
            (mapRatio > 5.0)
            (sprintf "Map ratio %.1f no longer shows the structural-comparison penalty this gate documents" mapRatio)
    | None -> failures.Add "blocker-lookup: expected a Map sample at 16384"

    // The merge scan must beat the persistent-map fold once the entity count is non-trivial.
    match ratio "state-update" 1024 "Map.add fold", ratio "state-update" 1024 "Array/merge-scan" with
    | Some foldRatio, Some mergeRatio ->
        require
            "state-update/merge-scan-beats-map-fold"
            (foldRatio > mergeRatio * 3.0)
            (sprintf "Map.add fold ratio %.1f is within 3x of merge-scan ratio %.1f" foldRatio mergeRatio)
    | _ -> failures.Add "state-update: expected a 1024 sample"

    // Packing before sorting must remain a large win over struct comparison.
    match ratio "line-dedupe" 64 "List.distinct|>sortBy", ratio "line-dedupe" 64 "Array/packed-sort" with
    | Some listRatio, Some packedRatio ->
        require
            "line-dedupe/packing-beats-structural-comparison"
            (listRatio > packedRatio * 10.0)
            (sprintf "List path ratio %.1f is within 10x of the packed path %.1f" listRatio packedRatio)
    | _ -> failures.Add "line-dedupe: expected a 64 sample"

    // Building a Set to answer one question must stay obviously wrong.
    match ratio "membership-probe" 1024 "Set.ofArray" with
    | Some setRatio ->
        require
            "membership-probe/set-rebuild-is-not-free"
            (setRatio > 20.0)
            (sprintf "Set.ofArray ratio %.1f no longer shows the per-call rebuild cost" setRatio)
    | None -> failures.Add "membership-probe: expected a 1024 sample"

    assertions.ToArray(), failures.ToArray()

// ================================================================ entry

let run () =
    // THE FULL MATRIX, RESTORED. An earlier revision of this row cut 10 rows to 6 and 65
    // strategy-measurements to 24, justified -- like the trials cut above -- by the PR feedback
    // critical path on the reasoning that this gate ran inside the wave-1 `integrity` job. THAT
    // JUSTIFICATION IS STALE: it is a wave-2 subject with its own job now, contributing nothing to
    // either wave maximum, so the rows cost nothing they were cut to save.
    //
    // Restoring them is not tidiness. The intermediate sizes are what make each row a SCALING claim
    // rather than a single point, and "this shape is super-linear at the sizes the simulation
    // reaches" is the entire content of the ordering claim these assertions defend. A single point
    // cannot distinguish a super-linear shape from a slow constant.
    let cases =
        [| for n in [ 8; 128; 2048; 16384 ] -> blockerLookup n
           for n in [ 64; 256; 1024 ] do
               for fraction in [ 8; 2 ] -> stateUpdate n (max 1 (n / fraction))
           for n in [ 8; 24; 64; 256 ] -> lineDedupe n
           for n in [ 64; 256; 1024 ] -> membershipProbe n |]

    let assertions, failures = assess cases
    { SchemaVersion = schemaVersion
      RuntimeVersion = Environment.Version.ToString()
      TieredCompilation =
        match Environment.GetEnvironmentVariable "DOTNET_TieredCompilation" with
        | null | "" -> "default"
        | v -> v
      Trials = trials
      Cases = cases
      Assertions = assertions
      Failures = failures }

let render (receipt: CollectionsReceipt) =
    let sb = Text.StringBuilder()
    let line (s: string) = sb.AppendLine s |> ignore
    line ""
    line (sprintf "collection strategy benchmarks — .NET %s, TieredCompilation=%s, min of %d trials"
                  receipt.RuntimeVersion receipt.TieredCompilation receipt.Trials)
    line "nanoseconds per operation; (xN) is the ratio to the best strategy for that row"
    let mutable current = ""
    for c in receipt.Cases do
        if c.Case <> current then
            current <- c.Case
            line ""
            line (sprintf "  %s — %s" c.Case c.Subject)
        let sizeLabel =
            if c.Secondary > 0 then sprintf "n=%d/%d" c.Size c.Secondary else sprintf "n=%d" c.Size
        let cells =
            c.Strategies
            |> Array.map (fun s ->
                let mark = if s.Strategy = c.Best then "*" else " "
                sprintf "%s%s %.1f (x%.1f)" mark s.Strategy s.NanosecondsPerOperation s.RatioToBest)
            |> String.concat "   "
        line (sprintf "    %-12s %s" sizeLabel cells)
    line ""
    for a in receipt.Assertions do
        let failed = receipt.Failures |> Array.exists (fun f -> f.StartsWith a)
        line (sprintf "  [%s] %s" (if failed then "FAIL" else " ok " ) a)
    if receipt.Failures.Length > 0 then
        line ""
        for f in receipt.Failures do line ("  " + f)
    sb.ToString()
