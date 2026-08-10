module SIR.Client.TestsClientPerformanceQualification

open System
open SIR.Client
open SIR.Domain

/// Owns dense-map and player-scene performance qualification; Program composes shared fixtures.
let run require address editor denseMaximumMap denseMaximumText denseMaximumState qualificationElapsed performanceScene =
    let p95 runs operation =
        for _ in 1 .. 5 do operation ()
        let timings =
            Array.init runs (fun _ ->
                let started = Diagnostics.Stopwatch.GetTimestamp()
                operation ()
                let elapsed = Diagnostics.Stopwatch.GetTimestamp() - started
                float elapsed * 1000.0 / float Diagnostics.Stopwatch.Frequency)
            |> Array.sort
        timings[int (float timings.Length * 0.95)]

    let pointerPreviewP95 =
        p95 80 (fun () ->
            denseMaximumState
            |> MapEditor.update (ChooseTerrain Objective)
            |> MapEditor.update (ChooseTool(Terrain LineTool))
            |> MapEditor.update (BeginTerrainGesture(address 0 0))
            |> MapEditor.update (ExtendTerrainGesture(address 39 39))
            |> MapEditor.terrainPreview
            |> ignore)
    let panZoomP95 =
        let view = MapEditorWorkspace.initial false
        p95 120 (fun () ->
            view
            |> MapEditorWorkspace.update
                denseMaximumMap
                (MapEditor.selected denseMaximumState)
                (PanEditorBy(2.0, -1.0))
            |> MapEditorWorkspace.update
                denseMaximumMap
                (MapEditor.selected denseMaximumState)
                (ZoomEditorAt(480.0, 320.0, 1.01))
            |> ignore)
    let commandValidationP95 =
        p95 40 (fun () ->
            MapEditor.validateCommand
                denseMaximumMap
                (PaintCells(Objective, [| address 0 0 |]))
            |> ignore)
    let fullValidationP95 =
        p95 30 (fun () ->
            MapEditor.validationIssues denseMaximumMap |> ignore)
    let changedDenseState =
        denseMaximumState
        |> MapEditor.update (ChooseTerrain Objective)
        |> MapEditor.update (ChooseTool(Terrain PencilTool))
        |> MapEditor.update (ActivateCell(0, 0))
    let undoRedoP95 =
        p95 50 (fun () ->
            changedDenseState
            |> MapEditor.update UndoEditorCommand
            |> MapEditor.update RedoEditorCommand
            |> ignore)
    let importP95 =
        p95 20 (fun () ->
            MapEditor.tryImport denseMaximumText
            |> Result.defaultWith failwith
            |> ignore)
    let exportP95 =
        p95 20 (fun () ->
            MapEditor.export { editor with Map = denseMaximumMap } |> ignore)
    let denseScene =
        Battlefield.scene
            (MapEditor.frame denseMaximumState)
            { Battlefield.initial with
                Camera =
                    { PanX = 0.0
                      PanY = 0.0
                      Zoom = MapEditorWorkspace.MinimumZoom } }
    require
        (denseMaximumState.Map = denseMaximumMap
         && pointerPreviewP95 < 8.0
         && panZoomP95 < (1000.0 / 60.0)
         && commandValidationP95 < 16.0
         && fullValidationP95 < 100.0
         && undoRedoP95 < 50.0
         && importP95 < 250.0
         && exportP95 < 250.0
         && denseScene.InteractiveNodeEstimate < 8_000)
        (sprintf
            "Maximum-document editor budgets failed: preview %.3f ms, pan/zoom %.3f ms, command %.3f ms, document %.3f ms, undo/redo %.3f ms, import %.3f ms, export %.3f ms, interactive nodes %d."
            pointerPreviewP95
            panZoomP95
            commandValidationP95
            fullValidationP95
            undoRedoP95
            importP95
            exportP95
            denseScene.InteractiveNodeEstimate)
    printfn
        "Map-editor qualification: automated task trace %.1f ms; dense 40x40 document (%d terrain, %d edges, %d units, %d regions) p95 preview %.3f ms, pan/zoom %.3f ms, command %.3f ms, document %.3f ms, undo/redo %.3f ms, import %.3f ms, export %.3f ms; %d estimated interactive nodes."
        qualificationElapsed
        denseMaximumMap.Terrain.Count
        denseMaximumMap.Edges.Count
        denseMaximumMap.Units.Count
        denseMaximumMap.Regions.Count
        pointerPreviewP95
        panZoomP95
        commandValidationP95
        fullValidationP95
        undoRedoP95
        importP95
        exportP95
        denseScene.InteractiveNodeEstimate

    let performanceFrame = Battlefield.performanceFrame 200
    for _ in 1 .. 20 do
        Battlefield.scene performanceFrame Battlefield.initial |> ignore

    let sceneTimings =
        Array.init 240 (fun _ ->
            let started = Diagnostics.Stopwatch.GetTimestamp()
            Battlefield.scene performanceFrame Battlefield.initial |> ignore
            let elapsed = Diagnostics.Stopwatch.GetTimestamp() - started
            float elapsed * 1000.0 / float Diagnostics.Stopwatch.Frequency)
        |> Array.sort

    let p95SceneMilliseconds = sceneTimings[int (float sceneTimings.Length * 0.95)]
    require
        (p95SceneMilliseconds < 8.0)
        "The 200-unit pure scene projection exceeded the 8 ms p95 frame-work budget."

    let stressFrame = Battlefield.performanceFrame 400
    let stressScene = Battlefield.scene stressFrame Battlefield.initial
    let stressTimings =
        Array.init 120 (fun _ ->
            let started = Diagnostics.Stopwatch.GetTimestamp()
            Battlefield.scene stressFrame Battlefield.initial |> ignore
            let elapsed = Diagnostics.Stopwatch.GetTimestamp() - started
            float elapsed * 1000.0 / float Diagnostics.Stopwatch.Frequency)
        |> Array.sort
    let p95StressMilliseconds =
        stressTimings[int (float stressTimings.Length * 0.95)]

    let performanceProvenance frame =
        { SourceIdentity = "performance-fixture"
          ReplayIdentity = "none"
          ProjectionIdentity = EvidenceExport.projectionIdentity frame
          EngineIdentity = "test-engine"
          RulesetIdentity = None
          Tick = frame.Tick
          Mode = DerivedSimulationEvidence
          PaletteIdentity = ReplayPalettes.accessibleDefault.Id
          RendererVersion = EvidenceExport.RendererVersion }
    let exportTimings frame runs =
        Array.init runs (fun _ ->
            let started = Diagnostics.Stopwatch.GetTimestamp()
            EvidenceExport.svg (performanceProvenance frame) None frame |> ignore
            let elapsed = Diagnostics.Stopwatch.GetTimestamp() - started
            float elapsed * 1000.0 / float Diagnostics.Stopwatch.Frequency)
        |> Array.sort
    let normalExportTimings = exportTimings performanceFrame 80
    let stressExportTimings = exportTimings stressFrame 40
    let p95NormalExport =
        normalExportTimings[int (float normalExportTimings.Length * 0.95)]
    let p95StressExport =
        stressExportTimings[int (float stressExportTimings.Length * 0.95)]
    require
        (stressScene.Units.Length = 400
         && p95NormalExport < 100.0
         && p95StressExport < 250.0)
        "Stress projection or deterministic evidence export exceeded its review guardrail."

    printfn
        "Static battlefield performance: 200 units, %d estimated interactive nodes, pure scene projection p95 %.3f ms over %d runs; 400-unit stress projection p95 %.3f ms over %d runs; safe SVG export p95 %.3f ms normal / %.3f ms stress."
        performanceScene.InteractiveNodeEstimate
        p95SceneMilliseconds
        sceneTimings.Length
        p95StressMilliseconds
        stressTimings.Length
        p95NormalExport
        p95StressExport
