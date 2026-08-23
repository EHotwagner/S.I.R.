module SIR.Client.Web.TacticalSharedControls
open System
open Browser.Dom
open Browser.Types
open Elmish
open Elmish.React
open Fable.Core
open Fable.Core.JsInterop
open Feliz
open SIR.Client
open SIR.Domain
open SIR.Protocol.Http
open SIR.Protocol.Realtime
open SIR.Client.Web.BrowserInfrastructure
open SIR.Client.Web.AppTypes
open SIR.Client.Web.AppShell
open SIR.Client.Web.ClientFeatureRuntime
open SIR.Client.Web.CommandRegistry
open SIR.Client.Web.ModeAdapters
open SIR.Client.Web.TacticalOverlayView
open SIR.Client.Web.SceneAdapters
open SIR.Client.Web.PanelViews

/// Command availability, status/button primitives, the unit glyph, and the shared
/// inspector, extracted from App so the tactical scene owner and the shell can both
/// consume one definition instead of App re-growing past its ownership ceiling.

let tacticalCommandAvailable model (command: TacticalCommandDefinition) =
    let availability =
        match command.Availability with
        | AlwaysAvailable -> true
        | TimelineEditable ->
            UnifiedTacticalWorkspace.canEditAt model.Tactical.Cursor model.Tactical
        | TimelineSelectionRequired ->
            model.Planning |> Option.bind _.SelectedCommand |> Option.isSome
        | PredictionRequired ->
            model.Planning |> Option.bind _.Predicted |> Option.isSome
        | CommittedHistoryRequired -> model.Tactical.CommittedThrough >= 0L
        | HelpOpenRequired -> model.InputHelpExpanded
        | PlanningAcceptedRequired ->
            model.Planning
            |> Option.exists (fun planning ->
                planning.AcceptedRevision = Some planning.Revision
                && planning.Predicted
                   |> Option.exists (fun preview -> preview.Revision = planning.Revision))
        | PlanningIssuesRequired ->
            model.Planning |> Option.exists (fun planning -> planning.Issues.Length > 0)
        | ReplayLoadedRequired -> model.Shell.Playback.FinalTick > 0
        | ReplayEventsRequired ->
            model.Shell.Inspection
            |> Option.exists (fun inspection -> not (List.isEmpty inspection.Events))
        | ReplayOperationRequired -> model.Shell.ActiveOperation.IsSome

    let currentTick, finalTick, transportReady =
        match model.Workspace, model.SampleReplayFrames, model.Simulator with
        | ReplayWorkspace, _, _ when model.Shell.Playback.FinalTick > 0 ->
            int64 model.Shell.Playback.CurrentTick,
            int64 model.Shell.Playback.FinalTick,
            model.Shell.Playback.FinalTick > 0
        | _, _, Some simulator ->
            int64 simulator.Tick, model.Tactical.Horizon, true
        | ReplayWorkspace, _, None ->
            int64 model.Shell.Playback.CurrentTick,
            int64 model.Shell.Playback.FinalTick,
            model.Shell.Playback.FinalTick > 0
        | _ -> model.Tactical.Cursor, model.Tactical.Horizon, true

    let transportAvailability =
        match command.Id with
        | "timeline.play-toggle" ->
            model.Simulator.IsSome
            || (model.Workspace = ReplayWorkspace && transportReady)
        | "timeline.step-back"
        | "timeline.home" -> currentTick > 0L
        | "timeline.step-forward"
        | "timeline.end" -> transportReady && currentTick < finalTick
        | _ -> true

    let planningAvailability =
        match command.Id, model.Planning with
        | "planning.undo", Some planning -> PlanningWorkspace.canUndo planning
        | "planning.redo", Some planning -> PlanningWorkspace.canRedo planning
        | "planning.validate", Some planning ->
            planning.Predicted
            |> Option.exists (fun preview -> preview.Revision = planning.Revision)
        | ("timeline.move-command" | "timeline.remove-command"), Some planning ->
            planning.SelectedCommand
            |> Option.bind (fun id ->
                planning.Commands |> List.tryFind (fun current -> current.Id = id))
            |> Option.exists (fun selected ->
                planning.CommittedTick
                |> Option.forall (fun boundary ->
                    selected.EarliestTick > boundary
                    && (command.Id <> "timeline.move-command"
                        || model.Tactical.Cursor > int64 boundary)))
        | id, Some _ when
            id.StartsWith("planning.inspector.", StringComparison.Ordinal)
            ->
            UnifiedTacticalWorkspace.canEditAt
                model.Tactical.Cursor
                model.Tactical
        | id, Some planning when
            id.StartsWith("planning.battlefield.cell.", StringComparison.Ordinal)
            ->
            planning.Tool = RouteTool
            && planning.SelectedUnit.IsSome
            && UnifiedTacticalWorkspace.canEditAt
                model.Tactical.Cursor
                model.Tactical
        | _ -> true

    let simulatorAvailability =
        if
            command.Id.StartsWith("simulator.pointer.", StringComparison.Ordinal)
        then
            model.Simulator.IsSome
            && model.SimulatorSelectedUnit.IsSome
            && (model.Simulator |> Option.forall (fun simulator -> not simulator.IsRunning))
        else true

    availability
    && transportAvailability
    && planningAvailability
    && simulatorAvailability

let status model =
    match model.Verification with
    | NotLoaded -> "Ready — choose a scenario or load a replay", "status-neutral"
    | Loading -> "Loading replay", "status-loading"
    | BrowserKernelVerified ->
        "Verified browser-kernel replay", "status-verified"
    | PerspectiveReady ->
        "Perspective playback — hidden state unavailable", "status-perspective"
    | SandboxDerived identity ->
        "Sandbox fork — not authoritative (" + identity + ")", "status-sandbox"
    | Unsupported reason -> "Unsupported replay — " + reason, "status-unsupported"
    | Diverged(tick, phase, detail) ->
        "Diverged at tick "
        + string tick
        + " during "
        + phase
        + " — "
        + detail,
        "status-diverged"
    | Failed reason -> "Replay failed — " + reason, "status-failed"

/// The render-time policy for command controls.  A control is either supplied
/// with registry-derived `aria-keyshortcuts` by its family adapter, or is
/// explicitly disclosed as unassigned.  This keeps unbound/dynamic controls
/// out of a silent third state without mutating the DOM after render.
let commandButton properties =
    let hasRegistryBinding =
        properties
        |> List.exists (fun property ->
            let name, _ = unbox<string * obj> property
            name = "aria-keyshortcuts")
    Html.button (
        if hasRegistryBinding then properties
        else
            properties
            @ [ prop.custom ("data-binding-state", "unassigned")
                prop.custom ("aria-description", "Shortcut: Unassigned") ])

let button
    (text: string)
    (label: string)
    (disabled: bool)
    (onClick: MouseEvent -> unit)
    =
    commandButton [
        prop.type'.button
        prop.text text
        prop.ariaLabel label
        prop.disabled disabled
        prop.onClick onClick
    ]


let glyphView
    (palette: PaletteTokens)
    (centerX: float)
    (centerY: float)
    (scale: float)
    (classId: UnitClassId)
    =
    let glyph = UnitGlyphCatalog.resolve classId
    let transform =
        "translate("
        + string centerX
        + " "
        + string centerY
        + ") scale("
        + string scale
        + ") translate(-12 -12)"

    Svg.g [
        svg.custom ("transform", transform)
        svg.custom ("data-class-id", UnitClassId.value classId)
        svg.children [
            for primitive in glyph.Primitives do
                match primitive with
                | FilledPath path ->
                    Svg.path [ svg.d path; svg.fill palette.Text ]
                | StrokedPath path ->
                    Svg.path [
                        svg.d path
                        svg.fill "none"
                        svg.stroke palette.Text
                        svg.strokeWidth 1.8
                        svg.strokeLineCap "round"
                        svg.strokeLineJoin "round"
                    ]
                | Circle(x, y, radius) ->
                    Svg.circle [
                        svg.cx x
                        svg.cy y
                        svg.r radius
                        svg.fill palette.Text
                    ]
        ]
    ]

let inspector (model: SIR.Client.Model) dispatch =
    let inspection =
        model.Inspection
        |> Option.defaultValue
            { Tick = 0
              BoardMinimumColumn = 0
              BoardMinimumRow = 0
              BoardMaximumColumn = 0
              BoardMaximumRow = 0
              Units = []
              Edges = []
              Events = []
              Checkpoints = []
              PerspectiveHash = None }

    let selectedUnit =
        model.Selection.Unit
        |> Option.bind (fun selected ->
            inspection.Units
            |> List.tryFind (fun unit -> unit.Id = selected))

    let selectedEvent =
        model.Selection.Event
        |> Option.bind (fun selected ->
            inspection.Events
            |> List.tryFind (fun event -> event.Id = selected))

    Html.section [
        prop.className "panel inspector-panel"
        prop.ariaLabel "Replay inspector"
        prop.children [
            Html.h2 "Inspector"
            Html.p (
                "Compact projection at tick "
                + string inspection.Tick
                + "; complete world state remains in the worker."
            )
            Html.h3 "Board"
            Html.div [
                prop.className "board"
                prop.role.img
                prop.ariaLabel (
                    "Board from column "
                    + string inspection.BoardMinimumColumn
                    + " row "
                    + string inspection.BoardMinimumRow
                    + " to column "
                    + string inspection.BoardMaximumColumn
                    + " row "
                    + string inspection.BoardMaximumRow
                )
                prop.children (
                    inspection.Units
                    |> List.map (fun unit ->
                        commandButton [
                            prop.type'.button
                            prop.className ("unit-token unit-" + unit.Side.ToLowerInvariant())
                            prop.ariaLabel (
                                "Inspect "
                                + unit.Side
                                + " unit "
                                + string unit.Id
                            )
                            prop.text (unit.Side.Substring(0, 1) + string unit.Id)
                            prop.onClick (fun _ ->
                                dispatch (ShellMsg(UnitSelected(Some unit.Id))))
                        ])
                )
            ]
            Html.h3 "Timeline and events"
            Html.ol [
                prop.className "event-list"
                prop.children (
                    inspection.Events
                    |> List.map (fun event ->
                        Html.li [
                            commandButton [
                                prop.type'.button
                                prop.ariaLabel ("Inspect event " + string event.Id)
                                prop.text (
                                    "T"
                                    + string event.Tick
                                    + " · "
                                    + event.Source
                                    + " · "
                                    + event.Summary
                                )
                                prop.onClick (fun _ ->
                                    dispatch (ShellMsg(EventSelected(Some event.Id))))
                            ]
                        ])
                )
            ]
            Html.h3 "Formula"
            button
                "Attack formula"
                "Inspect attack formula"
                false
                (fun _ ->
                    dispatch (
                        ShellMsg(FormulaSelected(Some "damage = max(0, attack power)"))
                    ))
            Html.h3 "Checkpoints"
            Html.table [
                prop.children [
                    Html.thead [
                        Html.tr [
                            Html.th "Tick"
                            Html.th "State hash"
                            Html.th "Event hash"
                        ]
                    ]
                    Html.tbody [
                        for checkpoint in inspection.Checkpoints do
                            Html.tr [
                                Html.td (string checkpoint.Tick)
                                Html.td checkpoint.StateHash
                                Html.td checkpoint.EventHash
                            ]
                    ]
                ]
            ]
            Html.dl [
                Html.dt "Unit"
                Html.dd (
                    selectedUnit
                    |> Option.map (fun unit ->
                        unit.Side
                        + " "
                        + string unit.Id
                        + " at "
                        + string unit.Column
                        + ","
                        + string unit.Row
                        + "; health "
                        + string unit.Health)
                    |> Option.defaultValue "None"
                )
                Html.dt "Event"
                Html.dd (
                    selectedEvent
                    |> Option.map (fun event -> event.Summary)
                    |> Option.defaultValue "None"
                )
                Html.dt "Formula"
                Html.dd (model.Selection.Formula |> Option.defaultValue "None")
                Html.dt "Perspective hash"
                Html.dd (inspection.PerspectiveHash |> Option.defaultValue "Not applicable")
            ]
        ]
    ]


[<Emit("$0.setPointerCapture($1)")>]
let capturePointer (target: EventTarget) (pointerId: int) : unit = jsNative

[<Emit("$0.releasePointerCapture($1)")>]
let releasePointer (target: EventTarget) (pointerId: int) : unit = jsNative

let mutable editorSceneProjectionConstructionCount = 0
