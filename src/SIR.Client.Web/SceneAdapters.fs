module SIR.Client.Web.SceneAdapters

open Feliz
open SIR.Client
open SIR.Client.Web.AppTypes

let mergeRuntimeTruth (contextual: SharedSceneProjection) (runtime: SharedSceneProjection) =
    let runtimeUnits = runtime.Units |> Array.map (fun unit -> unit.Visual.Id, unit) |> Map.ofArray
    let units =
        Array.append
            (contextual.Units
             |> Array.choose (fun authored ->
                 Map.tryFind authored.Visual.Id runtimeUnits
                 |> Option.map (fun live ->
                     { authored with
                         PresentationColumn = live.PresentationColumn
                         PresentationRow = live.PresentationRow
                         Visual = live.Visual })))
            (runtime.Units
             |> Array.filter (fun live ->
                 contextual.Units
                 |> Array.exists (fun authored -> authored.Visual.Id = live.Visual.Id)
                 |> not))
    let routes = Array.append contextual.Routes runtime.Routes
    let annotations = Array.append contextual.Annotations runtime.Annotations
    let maximumEffects =
        (TacticalSceneProjection.visualSystem "accessible-default" false units.Length).MaximumActiveEffects
    let effects =
        Array.append contextual.Effects runtime.Effects
        |> Array.distinctBy _.PrimitiveId
        |> Array.sortByDescending (fun effect -> effect.Tick, effect.EventId)
        |> Array.truncate maximumEffects
        |> Array.sortBy (fun effect -> effect.Tick, effect.Order, ScenePrimitiveId.value effect.PrimitiveId)
    let addedUnits = max 0 (units.Length - contextual.Units.Length)
    let addedRoutes = max 0 (routes.Length - contextual.Routes.Length)
    let addedAnnotations = max 0 (annotations.Length - contextual.Annotations.Length)
    let addedEffects = max 0 (effects.Length - contextual.Effects.Length)
    { contextual with
        RevisionIdentity = runtime.RevisionIdentity
        Tick = runtime.Tick
        Board = runtime.Board
        Terrain = runtime.Terrain
        Edges = runtime.Edges
        Units = units
        Routes = routes
        Annotations = annotations
        Effects = effects
        VisualCost =
            { UnitCount = units.Length
              EffectInstances = effects.Length
              EstimatedSvgNodes =
                contextual.VisualCost.EstimatedSvgNodes
                + addedUnits * 12 + addedRoutes + addedAnnotations + addedEffects * 3 } }

let tacticalDensityToken = function
    | OrdinaryDensity -> "ordinary"
    | DenseDensity -> "dense"
    | StressDensity -> "stress"

let tacticalEffectKindToken = function
    | MovementEffect -> "movement"
    | AttackEffect -> "attack"
    | ImpactEffect -> "impact"
    | SuppressionEffect -> "suppression"
    | RecoveryEffect -> "recovery"
    | SignalEffect -> "signal"
    | ObjectiveEffect -> "objective"
    | GenericEffect -> "event"

let tacticalEffectLifecycleToken = function
    | PreviewEffect -> "preview"
    | PredictedEffect -> "predicted"
    | AcceptedEffect -> "accepted"
    | CommittedEffect -> "committed"
    | RejectedEffect -> "rejected"
    | HistoricalEffect -> "historical"

let tacticalEffectColor (system: TacticalVisualSystem) = function
    | ImpactEffect -> system.Impact
    | SuppressionEffect -> system.Suppression
    | RecoveryEffect -> system.Recovery
    | AttackEffect
    | MovementEffect
    | SignalEffect -> system.Intent
    | ObjectiveEffect -> system.Palette.NeutralFaction
    | GenericEffect -> system.Palette.Text

let tacticalEffectLayer cellSize (system: TacticalVisualSystem) (projection: SharedSceneProjection option) =
    Svg.g [
        svg.id "persistent-layer-effects"
        svg.custom ("data-scene-layer", "effects")
        svg.custom ("data-effect-motion", if system.ReducedMotion then "emphasis-120ms" else "causal")
        svg.custom ("pointer-events", "none")
        svg.custom ("aria-hidden", "true")
        svg.children [
            match projection with
            | Some scene ->
                for effect in scene.Effects do
                    let kind = tacticalEffectKindToken effect.Kind
                    let lifecycle = tacticalEffectLifecycleToken effect.Lifecycle
                    let color = tacticalEffectColor system effect.Kind
                    Svg.g [
                        svg.key (ScenePrimitiveId.value effect.PrimitiveId)
                        svg.className ("tactical-effect tactical-effect-" + kind + " tactical-effect-" + lifecycle)
                        svg.custom ("data-primitive-id", ScenePrimitiveId.value effect.PrimitiveId)
                        svg.custom ("data-effect-kind", kind)
                        svg.custom ("data-effect-lifecycle", lifecycle)
                        svg.custom ("data-effect-event", string effect.EventId)
                        svg.custom ("data-effect-tick", string effect.Tick)
                        svg.custom ("data-effect-order", string effect.Order)
                        svg.children [
                            match effect.SourcePoint, effect.TargetPoint with
                            | Some(sourceX, sourceY), Some(targetX, targetY) ->
                                Svg.line [
                                    svg.className "tactical-effect-trace"
                                    svg.x1 (sourceX * cellSize); svg.y1 (sourceY * cellSize)
                                    svg.x2 (targetX * cellSize); svg.y2 (targetY * cellSize)
                                    svg.stroke color; svg.strokeWidth (if system.ReducedMotion then 6 else 4)
                                    svg.custom ("stroke-dasharray", if effect.Kind = AttackEffect then "12 7" else "4 5")
                                ]
                                Svg.circle [
                                    svg.className "tactical-effect-impact"
                                    svg.cx (targetX * cellSize); svg.cy (targetY * cellSize)
                                    svg.r (if system.ReducedMotion then 11 else 8)
                                    svg.fill "none"; svg.stroke color; svg.strokeWidth 4
                                ]
                            | _, Some(targetX, targetY)
                            | Some(targetX, targetY), _ ->
                                Svg.circle [
                                    svg.className "tactical-effect-impact"
                                    svg.cx (targetX * cellSize); svg.cy (targetY * cellSize)
                                    svg.r (if system.ReducedMotion then 11 else 8)
                                    svg.fill "none"; svg.stroke color; svg.strokeWidth 4
                                ]
                            | None, None -> ()
                        ]
                    ]
            | None -> ()
        ]
    ]

let sharedSceneUnitCommand model unitId =
    match model.Workspace with
    | EditorWorkspace -> Some("editor.scene.select.unit." + string unitId)
    | PlanningWorkspace -> Some("planning.roster.select." + string unitId)
    | SimulatorWorkspace -> Some("simulator.scene.select.unit." + string unitId)
    | ReplayWorkspace -> Some("review.scene.select.unit." + string unitId)

let sharedSceneCellCommand model column row =
    match model.Workspace with
    | EditorWorkspace -> Some("editor.scene.cell." + string column + "." + string row)
    | PlanningWorkspace -> Some("planning.battlefield.cell." + string column + "." + string row)
    | _ -> None
