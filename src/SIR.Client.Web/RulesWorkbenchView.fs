module SIR.Client.Web.RulesWorkbenchView

open Feliz
open SIR.Client
open SIR.Domain
open SIR.Client.Web.AppTypes

let private button (text: string) (label: string) disabled onClick =
    Html.button [
        prop.type'.button
        prop.className "command-button"
        prop.text text
        prop.ariaLabel label
        prop.disabled disabled
        prop.custom ("data-binding-state", "unassigned")
        prop.custom ("aria-description", "Keyboard binding unassigned.")
        prop.onClick onClick
    ]

let private sandbox (model: SIR.Client.Model) dispatch =
    Html.section [
        prop.className "panel sandbox-panel"
        prop.ariaLabel "Sandbox parameters"
        prop.children [
            Html.h2 "Typed parameters"
            Html.p "Edits create a derived sandbox; the baseline stays immutable."
            match model.Lab.Scenario with
            | None -> Html.p "Load a design scenario or verified replay to edit parameters."
            | Some selected ->
                for parameter in selected.Parameters do
                    let current = model.Patch |> Map.tryFind parameter.Key |> Option.defaultValue parameter.DefaultValue
                    Html.label [ prop.htmlFor parameter.Key; prop.text (parameter.Label + ": " + string current) ]
                    Html.input [
                        prop.id parameter.Key
                        prop.type'.number
                        prop.min parameter.Minimum
                        prop.max parameter.Maximum
                        prop.step parameter.Step
                        prop.value current
                        prop.onChange (fun (value: int) -> dispatch (ShellMsg(ParameterEdited(parameter.Key, int32 value))))
                    ]
            model.Lab.ValidationError
            |> Option.map (fun error -> Html.p [ prop.className "validation-error"; prop.role.alert; prop.text error ])
            |> Option.defaultValue Html.none
        ]
    ]

let private scenarioCatalog (model: SIR.Client.Model) dispatch =
    Html.section [
        prop.className "panel catalog-panel quick-start-panel"
        prop.ariaLabel "Design scenario catalog"
        prop.children [
            Html.p [ prop.className "eyebrow"; prop.text "No replay file required" ]
            Html.h2 "Try an interactive scenario"
            Html.p "Run an example, edit typed values, compare, sweep, and export."
            Html.div [
                prop.className "scenario-grid"
                prop.children [
                    for scenario in Lab.catalog do
                        let selected = model.Lab.Scenario |> Option.exists (fun current -> current.Identity = scenario.Identity)
                        let defaults =
                            scenario.Parameters
                            |> List.map (fun parameter -> parameter.Label + " " + string parameter.DefaultValue)
                            |> String.concat " · "
                        Html.article [
                            prop.className (if selected then "scenario-card scenario-card-selected" else "scenario-card")
                            prop.children [
                                Html.h3 scenario.Title
                                Html.p scenario.Description
                                Html.p [ prop.className "scenario-defaults"; prop.text defaults ]
                                Html.p [ prop.className "identity"; prop.text (scenario.Identity + " r" + string scenario.Revision) ]
                                button
                                    (if selected then "Simulate again" else "Simulate now")
                                    ("Simulate design scenario " + scenario.Title)
                                    false
                                    (fun _ -> dispatch (ShellMsg(ScenarioSelected scenario.Identity)))
                            ]
                        ]
                ]
            ]
        ]
    ]

let private resultTable (title: string) (result: ExperimentResult) =
    Html.div [
        Html.h3 title
        Html.p [
            prop.className "identity"
            prop.text (
                "Result " + result.ResultIdentity
                + " · engine " + result.Input.EngineIdentity.Substring(0, 12)
                + " · rules " + result.Input.RulesetIdentity.Substring(0, 12))
        ]
        Html.table [
            Html.thead [ Html.tr [ Html.th "Metric"; Html.th "Canonical integer result" ] ]
            Html.tbody [
                for KeyValue(key, value) in result.Metrics do
                    Html.tr [ Html.td key; Html.td (string value) ]
            ]
        ]
    ]

let private comparisonPanel (model: Model) (evidence: SvgEvidence) dispatch =
    let shell = model.Shell
    let viewName =
        match model.ComparisonView with
        | Split -> "Linked split"
        | Swipe -> "Linked swipe"
        | DifferenceOverlay -> "Difference overlay"

    let resultPreview label className (result: ExperimentResult) =
        let remaining = result.Metrics |> Map.tryFind "remaining-health" |> Option.defaultValue 0
        let scene = Battlefield.scene (Lab.renderFrame result) model.Battlefield
        let transform =
            "translate(" + string scene.Camera.PanX + " " + string scene.Camera.PanY
            + ") scale(" + string scene.Camera.Zoom + ")"
        Html.figure [
            prop.className ("comparison-result " + className)
            prop.ariaLabel (label + ", target remaining health " + string remaining)
            prop.children [
                Html.figcaption [ Html.strong label; Html.span (" · " + result.ResultIdentity) ]
                Svg.svg [
                    svg.viewBox (0, 0, 210, 150)
                    svg.custom ("role", "img")
                    svg.custom ("aria-label", label + " battlefield at linked tick " + string shell.Playback.CurrentTick + "; target has " + string remaining + " of 100 health")
                    svg.custom ("data-comparison-camera", transform)
                    svg.children [
                        Svg.g [
                            svg.custom ("transform", transform)
                            svg.children [
                                Svg.rect [ svg.x 0; svg.y 0; svg.width scene.Width; svg.height scene.Height; svg.fill scene.Palette.Terrain; svg.stroke scene.Palette.Grid ]
                                for column in 0 .. 3 do
                                    Svg.line [ svg.x1 (float column * scene.CellSize); svg.y1 0; svg.x2 (float column * scene.CellSize); svg.y2 scene.Height; svg.stroke scene.Palette.Grid ]
                                for row in 0 .. 2 do
                                    Svg.line [ svg.x1 0; svg.y1 (float row * scene.CellSize); svg.x2 scene.Width; svg.y2 (float row * scene.CellSize); svg.stroke scene.Palette.Grid ]
                                Svg.line [ svg.x1 scene.CellSize; svg.y1 0; svg.x2 (scene.CellSize * 2.0); svg.y2 0; svg.stroke scene.Palette.Text; svg.strokeWidth 4 ]
                                for unit in scene.Units do
                                    let selected = model.Battlefield.SelectedUnit = Some unit.Unit.Id
                                    let faction =
                                        match unit.Unit.Faction with
                                        | Human -> scene.Palette.HumanFaction
                                        | Arcane -> scene.Palette.ArcaneFaction
                                        | Neutral
                                        | OtherFaction _ -> scene.Palette.NeutralFaction
                                    Svg.g [
                                        svg.custom ("data-comparison-unit", string unit.Unit.Id)
                                        svg.children [
                                            Svg.rect [
                                                svg.x (unit.SymbolCenterX - 14.0); svg.y (unit.SymbolCenterY - 14.0)
                                                svg.width 28; svg.height 28; svg.fill scene.Palette.Canvas
                                                svg.stroke (if selected then scene.Palette.Focus else faction)
                                                svg.strokeWidth (if selected then 4 else 2)
                                            ]
                                            Svg.text [ svg.x (unit.SymbolCenterX - 8.0); svg.y (unit.SymbolCenterY + 4.0); svg.fill scene.Palette.Text; svg.fontSize 10; svg.text (string unit.Unit.Id) ]
                                        ]
                                    ]
                            ]
                        ]
                        Svg.text [ svg.x 8; svg.y 142; svg.fill "#f5f1e8"; svg.fontSize 11; svg.text ("Target " + string remaining + " HP") ]
                    ]
                ]
            ]
        ]

    Html.section [
        prop.className "panel comparison-panel"
        prop.ariaLabel "Linked baseline and fork comparison"
        prop.children [
            Html.p [ prop.className "eyebrow"; prop.text "Exploratory simulation comparison" ]
            Html.h2 "Immutable baseline and derived fork"
            Html.p [ prop.className "comparison-warning"; prop.role.note; prop.text "Neither side is verified replay evidence; edits create an identified fork." ]
            match shell.Lab.Report with
            | None -> Html.p "Run a scenario, then edit a parameter to compare a linked fork."
            | Some report ->
                let baseline = report.Comparison.Baseline
                let fork = report.Comparison.Fork
                let baselineAttacks = baseline.Metrics |> Map.tryFind "attack-events" |> Option.defaultValue 0
                let forkAttacks = fork.Metrics |> Map.tryFind "attack-events" |> Option.defaultValue 0
                let firstEvent = if baselineAttacks = forkAttacks then "No differing disclosed event" else "Attack event " + string (min baselineAttacks forkAttacks + 1)
                let firstField =
                    report.Comparison.Delta
                    |> Map.toList
                    |> List.tryFind (fun (_, delta) -> delta <> 0)
                    |> Option.map (fun (field, delta) -> field + " (" + (if delta > 0 then "+" else "") + string delta + ")")
                    |> Option.defaultValue "No differing disclosed field"
                Html.div [
                    prop.className ("comparison-viewport comparison-" + (match model.ComparisonView with | Split -> "split" | Swipe -> "swipe" | DifferenceOverlay -> "difference"))
                    prop.custom ("data-linked-camera", "true")
                    prop.custom ("data-linked-selection", "true")
                    prop.custom ("data-linked-tick", string shell.Playback.CurrentTick)
                    prop.custom ("data-linked-overlays", "true")
                    prop.children [ resultPreview Comparison.BaselineLabel "comparison-baseline" baseline; resultPreview Comparison.ForkLabel "comparison-fork" fork ]
                ]
                Html.p [
                    prop.className "linked-state"
                    prop.text (viewName + " · linked camera, selection, tick " + string shell.Playback.CurrentTick + ", and overlays · selected unit " + (model.Battlefield.SelectedUnit |> Option.map string |> Option.defaultValue "none"))
                ]
                Html.dl [
                    Html.dt "First divergent event"; Html.dd firstEvent
                    Html.dt "First differing disclosed field"; Html.dd firstField
                    Html.dt "Source link"
                    Html.dd (match shell.Source with | Loaded metadata -> metadata.SourceIdentity | _ -> baseline.Input.ScenarioIdentity)
                ]
                Html.table [
                    Html.caption "Metric deltas (fork − immutable baseline)"
                    Html.thead [ Html.tr [ Html.th "Metric"; Html.th "Delta" ] ]
                    Html.tbody [ for KeyValue(metric, delta) in report.Comparison.Delta do Html.tr [ Html.td metric; Html.td (string delta) ] ]
                ]
                Html.div [
                    prop.className "control-row"
                    prop.children [
                        button "Split" "Use linked split comparison" false (fun _ -> dispatch (ComparisonViewChanged Split))
                        button "Swipe" "Use linked swipe comparison" false (fun _ -> dispatch (ComparisonViewChanged Swipe))
                        button "Difference" "Use difference overlay comparison" false (fun _ -> dispatch (ComparisonViewChanged DifferenceOverlay))
                        button "Bookmark" "Bookmark linked comparison tick" false (fun _ -> dispatch AddComparisonBookmark)
                    ]
                ]
                Html.ul [ prop.ariaLabel "Comparison bookmarks"; prop.children [ for bookmark in model.ComparisonBookmarks do Html.li (bookmark.Label + " · tick " + string bookmark.Tick) ] ]
            Html.div [
                prop.className "control-row evidence-export-controls"
                prop.children [
                    button "Export safe SVG" "Export sanitized SVG evidence with provenance" false (fun _ -> dispatch ExportEvidenceSvg)
                    button "Export safe PNG" "Export PNG evidence rasterized from sanitized SVG" false (fun _ -> dispatch ExportEvidencePng)
                ]
            ]
            Html.p [
                prop.className "evidence-provenance"
                prop.text (
                    "Evidence provenance: source " + evidence.Provenance.SourceIdentity
                    + " · replay " + evidence.Provenance.ReplayIdentity
                    + " · projection " + evidence.Provenance.ProjectionIdentity.Substring(0, 12)
                    + " · palette " + evidence.Provenance.PaletteIdentity
                    + " · renderer " + evidence.Provenance.RendererVersion
                    + " · SHA-256 " + evidence.Sha256.Substring(0, 12))
            ]
        ]
    ]

let private laboratoryResults (model: SIR.Client.Model) dispatch =
    Html.section [
        prop.className "panel lab-results"
        prop.ariaLabel "Laboratory results"
        prop.children [
            Html.h2 "Simulation result"
            match model.Lab.Report with
            | None -> Html.p "Run a scenario to see its deterministic result."
            | Some report ->
                Html.p [ prop.className "evidence-label"; prop.text report.EvidenceLabel ]
                let remaining = Map.find "remaining-health" report.Comparison.Fork.Metrics
                let damage = Map.find "total-damage" report.Comparison.Fork.Metrics
                let attacks = Map.find "attack-events" report.Comparison.Fork.Metrics
                Html.p [ prop.className "simulation-summary"; prop.role.status; prop.text (string attacks + " attacks resolved · " + string damage + " damage · target finishes on " + string remaining + " HP") ]
                Html.h3 "Attack sequence"
                Html.div [
                    prop.className "simulation-frames"
                    prop.role.img
                    prop.ariaLabel "Target health after each simulated attack"
                    prop.children [
                        for attack, health in Lab.attackFrames report do
                            Html.div [
                                prop.className "simulation-frame"
                                prop.children [
                                    Html.strong (if attack = 0 then "Start" else "Attack " + string attack)
                                    Html.meter [ prop.min 0; prop.max 100; prop.value health; prop.ariaLabel ("Target health after " + string attack + " attacks: " + string health) ]
                                    Html.span (string health + " HP")
                                ]
                            ]
                    ]
                ]
                Html.h3 "Baseline and editable fork"
                resultTable "Immutable baseline" report.Comparison.Baseline
                resultTable "Derived fork" report.Comparison.Fork
                Html.h3 "Delta"
                Html.table [
                    Html.thead [ Html.tr [ Html.th "Metric"; Html.th "Fork − baseline" ] ]
                    Html.tbody [ for KeyValue(key, value) in report.Comparison.Delta do Html.tr [ Html.td key; Html.td (string value) ] ]
                ]
                match model.Lab.Scenario with
                | Some scenario ->
                    Html.div [
                        prop.className "control-row"
                        prop.children [
                            for parameter in scenario.Parameters do
                                button ("Sweep " + parameter.Label) ("Run deterministic sweep for " + parameter.Label) (Option.isSome model.Lab.ValidationError) (fun _ -> dispatch (ShellMsg(SweepRequested parameter.Key)))
                        ]
                    ]
                | None -> Html.none
                match report.Sweep with
                | Some sweep ->
                    Html.h3 ("Sweep chart · " + sweep.Parameter)
                    Html.div [
                        prop.className "integer-chart"
                        prop.ariaLabel ("Integer results for " + sweep.Parameter)
                        prop.children [
                            for result in sweep.Results do
                                let parameterValue = Map.find sweep.Parameter result.Input.Parameters
                                let remaining = Map.find "remaining-health" result.Metrics
                                Html.div [
                                    prop.className "chart-row"
                                    prop.children [
                                        Html.span (string parameterValue)
                                        Html.meter [ prop.min 0; prop.max 200; prop.value remaining; prop.ariaLabel (string parameterValue + ": remaining health " + string remaining) ]
                                        Html.span (string remaining)
                                    ]
                                ]
                        ]
                    ]
                | None -> Html.none
                button "Export reproducible experiment" "Export reproducible laboratory experiment" false (fun _ -> dispatch ExportExperiment)
        ]
    ]

[<ReactComponent>]
let RulesWorkbenchPanel (model: Model) (evidence: SvgEvidence) dispatch =
    Html.div [
        prop.ariaLabel "Rules supporting panel"
        prop.children [
            scenarioCatalog model.Shell dispatch
            laboratoryResults model.Shell dispatch
            comparisonPanel model evidence dispatch
            sandbox model.Shell dispatch
        ]
    ]
