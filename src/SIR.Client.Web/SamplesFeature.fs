module SIR.Client.Web.SamplesFeature

open Browser.Dom
open Browser.Types
open SIR.Client

let private element tag className text =
    let node = document.createElement tag
    if className <> "" then node.className <- className
    if text <> "" then node.textContent <- text
    node

let private append (parent: HTMLElement) (child: HTMLElement) =
    parent.appendChild child |> ignore

let private actionButton label action =
    let node = element "button" "command-button" label
    node.setAttribute("type", "button")
    node.setAttribute("aria-label", label)
    node.addEventListener("click", fun _ -> action ())
    node

let render
    (root: HTMLElement)
    (dispatch: string -> MapEditorState -> SimulatorHandoff option -> string -> InspectionProjection array -> unit)
    =
    root.innerHTML <- ""
    let content = element "section" "samples-panel-content" ""
    content.setAttribute("aria-label", "Curated maps simulations and replays")
    let heading = element "div" "samples-heading" ""
    append heading (element "p" "eyebrow" "Explore mechanics")
    append heading (element "h2" "" "Curated samples")
    append heading (element "p" "" "Open a map, run its sandbox, or inspect a replay walkthrough.")
    append content heading
    append content (element "h3" "" "Maps and simulations")
    let maps = element "div" "sample-list" ""
    for sample in ExperienceSamples.maps do
        let card = element "details" "panel sample-list-item sample-card" ""
        let summary = element "summary" "" ""
        append summary (element "span" "sample-kind" "Map · Simulation")
        let title = element "span" "sample-title" ""
        append title (element "strong" "" sample.Title)
        if sample.Id = "troll-assault" then
            append title (element "span" "sample-legacy-title" "Troll assault")
        append summary title
        append summary (element "span" "sample-summary" sample.Summary)
        append card summary
        let body = element "div" "sample-list-body" ""
        let highlights = element "ul" "" ""
        for highlight in sample.Highlights do append highlights (element "li" "" highlight)
        append body highlights
        append body (element "p" "sample-lesson" ("Lesson: " + sample.Lesson))
        append body (element "p" "sample-notes" ("Design notes: " + String.concat " " sample.DesignNotes))
        let controls = element "div" "control-row" ""
        let prepareSample action target =
            let editor = ExperienceSamples.editorState target
            dispatch action editor (ExperienceSamples.simulator target) "" [||]
        let prepare action = prepareSample action sample
        append controls (actionButton ("Open " + sample.Title + " in Editor") (fun () -> prepare "map"))
        append controls (actionButton ("Run " + sample.Title + " in Simulator") (fun () -> prepare "simulation"))
        if sample.Id = "troll-assault" then
            append controls (actionButton "Run Troll assault in Simulator" (fun () ->
                prepareSample "simulation" ExperienceSamples.legacyTrollAssault))
        append body controls
        append card body
        append maps card
    append content maps
    append content (element "h3" "" "Replay walkthroughs")
    let replays = element "div" "sample-list" ""
    for sample in ExperienceSamples.replays do
        let card = element "details" "panel sample-list-item sample-card" ""
        let summary = element "summary" "" ""
        append summary (element "span" "sample-kind" "Replay")
        append summary (element "strong" "" sample.Title)
        append summary (element "span" "sample-summary" sample.Summary)
        append card summary
        let body = element "div" "sample-list-body" ""
        append body (element "p" "" (string sample.Ticks + " deterministic sample ticks · locally navigable · sandbox evidence"))
        append body (actionButton ("Open replay walkthrough " + sample.Title) (fun () ->
            dispatch "replay" MapEditor.initial None (sample.Id + "\n" + sample.Title) (ExperienceSamples.replayFrames sample)))
        append card body
        append replays card
    append content replays
    append content (element "p" "sample-disclosure" "Walkthroughs are sandbox evidence, not verified match replays.")
    append root content
