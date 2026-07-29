namespace SIR.Client

open System

type ComparisonView =
    | Split
    | Swipe
    | DifferenceOverlay

type ComparisonBookmark =
    { Tick: int32
      Label: string }

type DivergentField =
    { Tick: int32
      UnitId: int32 option
      Field: string
      Baseline: string
      Fork: string }

type ComparisonInspection =
    { FirstDivergentEvent: int32 option
      FirstDifferingField: DivergentField option
      MetricDeltas: Map<string, int32> }

type LinkedComparison =
    { SourceIdentity: string
      BaselineIdentity: string
      ForkIdentity: string
      BaselineLabel: string
      ForkLabel: string
      Tick: int32
      SelectedUnit: int32 option
      View: ComparisonView
      Bookmarks: ComparisonBookmark list
      Inspection: ComparisonInspection }

[<RequireQualifiedAccess>]
module Comparison =
    [<Literal>]
    let BaselineLabel = "Immutable baseline — exploratory simulation"

    [<Literal>]
    let ForkLabel = "Derived fork — exploratory simulation, not verified replay"

    let private unitFields (unit: UnitProjection) =
        [ "side", unit.Side
          "column", string unit.Column
          "row", string unit.Row
          "health", string unit.Health
          "health-maximum", string unit.HealthMaximum ]

    let inspect
        (baseline: InspectionProjection)
        (fork: InspectionProjection)
        (metricDeltas: Map<string, int32>)
        =
        let baselineEvents =
            baseline.Events
            |> List.map (fun event -> event.Id, event)
            |> Map.ofList

        let forkEvents =
            fork.Events
            |> List.map (fun event -> event.Id, event)
            |> Map.ofList

        let firstDivergentEvent =
            Set.union
                (baselineEvents |> Map.keys |> Set.ofSeq)
                (forkEvents |> Map.keys |> Set.ofSeq)
            |> Set.toList
            |> List.choose (fun id ->
                match Map.tryFind id baselineEvents, Map.tryFind id forkEvents with
                | Some left, Some right when left = right -> None
                | Some left, Some right -> Some(min left.Tick right.Tick, id)
                | Some left, None -> Some(left.Tick, id)
                | None, Some right -> Some(right.Tick, id)
                | None, None -> None)
            |> List.sort
            |> List.tryHead
            |> Option.map snd

        let baselineUnits =
            baseline.Units |> List.map (fun unit -> unit.Id, unit) |> Map.ofList
        let forkUnits =
            fork.Units |> List.map (fun unit -> unit.Id, unit) |> Map.ofList

        let firstDifferingField =
            Set.union
                (baselineUnits |> Map.keys |> Set.ofSeq)
                (forkUnits |> Map.keys |> Set.ofSeq)
            |> Set.toList
            |> List.collect (fun id ->
                match Map.tryFind id baselineUnits, Map.tryFind id forkUnits with
                | Some left, Some right ->
                    List.zip (unitFields left) (unitFields right)
                    |> List.choose (fun ((field, baselineValue), (_, forkValue)) ->
                        if baselineValue = forkValue then None
                        else
                            Some
                                { Tick = max baseline.Tick fork.Tick
                                  UnitId = Some id
                                  Field = field
                                  Baseline = baselineValue
                                  Fork = forkValue })
                | Some _, None ->
                    [ { Tick = max baseline.Tick fork.Tick
                        UnitId = Some id
                        Field = "presence"
                        Baseline = "present"
                        Fork = "absent" } ]
                | None, Some _ ->
                    [ { Tick = max baseline.Tick fork.Tick
                        UnitId = Some id
                        Field = "presence"
                        Baseline = "absent"
                        Fork = "present" } ]
                | None, None -> [])
            |> List.sortBy (fun difference ->
                difference.Tick,
                difference.UnitId,
                difference.Field)
            |> List.tryHead

        { FirstDivergentEvent = firstDivergentEvent
          FirstDifferingField = firstDifferingField
          MetricDeltas = metricDeltas }

    let create sourceIdentity baselineIdentity forkIdentity tick selected inspection =
        { SourceIdentity = sourceIdentity
          BaselineIdentity = baselineIdentity
          ForkIdentity = forkIdentity
          BaselineLabel = BaselineLabel
          ForkLabel = ForkLabel
          Tick = tick
          SelectedUnit = selected
          View = Split
          Bookmarks = []
          Inspection = inspection }

    let addBookmark tick label comparison =
        let safeLabel =
            if String.IsNullOrWhiteSpace label then "Bookmark at tick " + string tick
            else label.Trim().Substring(0, min 80 (label.Trim().Length))

        let bookmark = { Tick = max 0 tick; Label = safeLabel }

        { comparison with
            Bookmarks =
                bookmark
                :: comparison.Bookmarks
                |> List.distinctBy (fun item -> item.Tick, item.Label)
                |> List.sortBy (fun item -> item.Tick, item.Label) }

    let setLinkedTick tick (comparison: LinkedComparison) =
        { comparison with Tick = max 0 tick }

    let setLinkedSelection selected (comparison: LinkedComparison) =
        { comparison with SelectedUnit = selected }

    let setView view (comparison: LinkedComparison) =
        { comparison with View = view }
