module SIR.ModalInput.Fixtures

open SIR.Client

let private require condition message =
    if not condition then
        failwith message

let private gesture key code modifiers phase =
    { Key = NormalizedKey.create key code
      Modifiers = modifiers
      Phase = phase }

let private binding
    id
    context
    precedence
    input
    repeat
    availability
    command
    : ModalBinding<ModalCommand> =
    { Id = id
      Context = context
      Precedence = precedence
      BindingGesture = input
      Label = id
      Group = "Fixture"
      Repeat = repeat
      Availability = availability
      Command = command }

let private alwaysAvailable _ = Available

let private outcomeName = function
    | Resolved input -> "resolved:" + input.Id
    | NoMatch -> "no-match"
    | NoAvailableMatch inputs ->
        "unavailable:" + (inputs |> List.map _.Id |> String.concat ",")

let private diagnosticName = function
    | DuplicateBindingId id -> "duplicate:" + id
    | EqualPrecedenceGestureConflict(firstId, secondId, precedence, input) ->
        $"conflict:{firstId}:{secondId}:{precedence}:{NormalizedKey.value input.Key}"

let evaluate () =
    let plain = KeyModifiers.none
    let enterFromDom = gesture "Enter" (Some "Enter") plain KeyDown
    let enterWithoutCode = gesture "Enter" None plain KeyDown
    let escape = gesture "Esc" (Some "Escape") plain KeyDown
    let question = gesture "?" (Some "Slash") plain KeyDown
    let arrow = gesture "ArrowRight" (Some "ArrowRight") plain KeyDown
    let spaceUp = gesture " " (Some "Space") plain KeyUp

    require
        (NormalizedKey.value (NormalizedKey.create "A" (Some "KeyA")) = "a"
         && NormalizedKey.value escape.Key = "Escape"
         && NormalizedKey.physicalCode question.Key = Some "Slash")
        "Key values or physical diagnostic codes were not normalized."

    let catalog =
        [ binding
              "editor.workspace.enter"
              AnyEditorContext
              WorkspaceCommands
              enterWithoutCode
              AllowRepeat
              alwaysAvailable
              ToggleInputHelp
          binding
              "editor.gesture.commit"
              (ExactContext(EditorGesture CommandPreview))
              ActiveGestureOrPreview
              enterWithoutCode
              IgnoreRepeat
              alwaysAvailable
              (EditorCommand CommitEditorGesture)
          binding
              "editor.camera.move-east"
              AnyEditorContext
              WorkspaceCommands
              arrow
              AllowRepeat
              alwaysAvailable
              (EditorWorkspaceCommand(PanEditorBy(1.0, 0.0)))
          binding
              "editor.selection.delete"
              (ExactContext(EditorTool Select))
              ActiveTool
              (gesture "Delete" (Some "Delete") plain KeyDown)
              IgnoreRepeat
              (fun _ -> Unavailable "Nothing is selected.")
              (EditorCommand DeleteEditorSelection)
          binding
              "editor.camera.pan-release"
              AnyEditorContext
              HeldLayer
              spaceUp
              IgnoreRepeat
              alwaysAvailable
              (SetEditorPanHeld false)
          binding
              "editor.confirmation.cancel"
              (ExactContext EditorDestructiveConfirmation)
              TransientPopup
              escape
              IgnoreRepeat
              alwaysAvailable
              (EditorCommand CancelDestructiveChange)
          binding
              "editor.help.close"
              (ExactContext InputHelpPopup)
              InputPopup
              escape
              IgnoreRepeat
              alwaysAvailable
              ToggleInputHelp ]

    require
        (ModalInput.validateCatalog catalog = [])
        "A valid catalog produced diagnostics."

    let gestureContexts =
        [ EditorBase
          EditorTool Select
          EditorGesture CommandPreview ]

    let commit =
        ModalInput.resolve gestureContexts enterFromDom false catalog
    let repeatedCommit =
        ModalInput.resolve gestureContexts enterFromDom true catalog
    let moving =
        ModalInput.resolve gestureContexts arrow true catalog
    let unknown =
        ModalInput.resolve gestureContexts question false catalog
    let panRelease =
        ModalInput.resolve gestureContexts spaceUp false catalog
    let unavailable =
        ModalInput.resolve
            [ EditorBase; EditorTool Select ]
            (gesture "Delete" None plain KeyDown)
            false
            catalog

    require
        (outcomeName commit = "resolved:editor.gesture.commit")
        "The highest-precedence binding did not resolve."

    require
        (outcomeName repeatedCommit = "no-match"
         && outcomeName moving = "resolved:editor.camera.move-east")
        "Repeat policy was not applied."

    require
        (outcomeName unknown = "no-match"
         && outcomeName unavailable = "unavailable:editor.selection.delete")
        "No-match and unavailable outcomes were not distinguished."

    require
        (outcomeName panRelease = "resolved:editor.camera.pan-release"
         && outcomeName (
             ModalInput.resolve
                 gestureContexts
                 { spaceUp with Phase = KeyDown }
                 false
                 catalog
         ) = "no-match")
        "Key-up and key-down phases were not distinguished."

    let popupContexts =
        [ EditorBase
          EditorDestructiveConfirmation
          InputHelpPopup ]

    require
        (outcomeName (ModalInput.resolve popupContexts escape false catalog) =
            "resolved:editor.help.close"
         && outcomeName (
             ModalInput.resolve
                 [ EditorBase; EditorDestructiveConfirmation ]
                 escape
                 false
                 catalog
         ) = "resolved:editor.confirmation.cancel")
        "Popup precedence did not unwind input help before confirmation."

    let visible =
        ModalInput.possibleInputs [ EditorBase; EditorTool Select ] catalog
        |> List.map _.Id

    require
        (visible =
            [ "editor.camera.move-east"
              "editor.camera.pan-release"
              "editor.workspace.enter" ])
        "Possible inputs included an unavailable binding or were unstable."

    let conflictingCatalog =
        [ binding
              "editor.gesture.alpha"
              AnyEditorContext
              ActiveGestureOrPreview
              enterWithoutCode
              IgnoreRepeat
              alwaysAvailable
              ToggleInputHelp
          binding
              "editor.gesture.beta"
              (ExactContext(EditorGesture CommandPreview))
              ActiveGestureOrPreview
              enterFromDom
              IgnoreRepeat
              alwaysAvailable
              ToggleInputHelp
          binding
              "simulator.enter"
              AnySimulatorContext
              ActiveGestureOrPreview
              enterWithoutCode
              IgnoreRepeat
              alwaysAvailable
              ToggleInputHelp
          binding
              "duplicate.id"
              AnyEditorContext
              WorkspaceCommands
              question
              IgnoreRepeat
              alwaysAvailable
              ToggleInputHelp
          binding
              "duplicate.id"
              AnySimulatorContext
              WorkspaceCommands
              question
              IgnoreRepeat
              alwaysAvailable
              ToggleInputHelp ]

    let diagnostics =
        ModalInput.validateCatalog conflictingCatalog
        |> List.map diagnosticName

    require
        (diagnostics =
            [ "duplicate:duplicate.id"
              "conflict:editor.gesture.alpha:editor.gesture.beta:ActiveGestureOrPreview:Enter" ])
        "Duplicate IDs or overlapping equal-precedence conflicts were not diagnosed deterministically."

    let deterministicTie =
        ModalInput.resolve
            [ EditorBase; EditorGesture CommandPreview ]
            enterFromDom
            false
            (conflictingCatalog |> List.rev)

    require
        (outcomeName deterministicTie = "resolved:editor.gesture.alpha")
        "Invalid catalog order changed deterministic resolution."

    let coexistingContextConflict =
        [ binding
              "editor.tool.enter"
              (ExactContext(EditorTool Select))
              ActiveTool
              enterWithoutCode
              IgnoreRepeat
              alwaysAvailable
              ToggleInputHelp
          binding
              "editor.gesture.enter"
              (ExactContext(EditorGesture CommandPreview))
              ActiveTool
              enterWithoutCode
              IgnoreRepeat
              alwaysAvailable
              ToggleInputHelp ]
        |> ModalInput.validateCatalog
        |> List.map diagnosticName

    require
        (coexistingContextConflict =
            [ "conflict:editor.gesture.enter:editor.tool.enter:ActiveTool:Enter" ])
        "Coexisting tool and gesture contexts were not recognized as overlapping."

    let editor =
        { MapEditor.initial with
            Tool = Terrain RectangleTool
            Gesture =
                TerrainGesture(
                    RectangleTool,
                    { CellColumn = 1; CellRow = 2 },
                    { CellColumn = 3; CellRow = 4 },
                    [||]
                )
            PendingDestructiveChange = Some ClearPending }

    let editorContexts =
        ModalInput.deriveEditorContexts
            { Editor = editor
              ActiveDomain = TerrainDomain
              PanHeld = true
              InputHelpExpanded = true }

    require
        (editorContexts =
            [ EditorBase
              EditorDomain TerrainDomain
              EditorTool(Terrain RectangleTool)
              EditorGesture(TerrainPreview RectangleTool)
              EditorPanHeld
              EditorDestructiveConfirmation
              InputHelpPopup ])
        "Editor contexts were not projected from authoritative editor facts."

    let simulatorContexts =
        ModalInput.deriveSimulatorContexts
            { SimulatorHandoffPresent = true
              SimulatorIsRunning = false
              SimulatorHasRoutePreview = true
              SimulatorRevisionIsStale = true
              InputHelpExpanded = false }

    let noHandoffContexts =
        ModalInput.deriveSimulatorContexts
            { SimulatorHandoffPresent = false
              SimulatorIsRunning = true
              SimulatorHasRoutePreview = true
              SimulatorRevisionIsStale = true
              InputHelpExpanded = true }

    require
        (simulatorContexts =
            [ SimulatorBase
              SimulatorPaused
              SimulatorRoutePreview
              SimulatorRevisionStale ]
         && noHandoffContexts =
            [ SimulatorBase; SimulatorNoHandoff; InputHelpPopup ])
        "Simulator lifecycle qualifiers were not projected deterministically."

    let productionEditorFacts =
        { Editor = editor
          ActiveDomain = TerrainDomain
          PanHeld = false
          InputHelpExpanded = false }
    let productionEditorCatalog =
        ModalInput.editorCatalog productionEditorFacts
    let editorProjection =
        ModalInput.projectEditor
            productionEditorFacts
            productionEditorCatalog

    require
        (ModalInput.validateCatalog productionEditorCatalog = []
         && editorProjection.Headline = "EDITOR / DESTRUCTIVE CONFIRMATION"
         && editorProjection.Detail.Contains("clearing")
         && editorProjection.PossibleInputs
            |> List.exists (fun input -> input.Id = "editor.panel.toggle")
         && editorProjection.PossibleInputs
            |> List.exists (fun input -> input.Id = "editor.inspector.toggle"))
        "The production Editor projection did not expose live state, F2, and F3."

    let assertProjectionIsExact
        (projection: ModalProjection<ModalCommand>)
        (productionCatalog: ModalBinding<ModalCommand> list)
        label
        =
        let displayedIds = projection.PossibleInputs |> List.map _.Id |> Set.ofList

        let resolvedIds =
            productionCatalog
            |> List.map _.BindingGesture
            |> List.distinct
            |> List.choose (fun input ->
                match ModalInput.resolve projection.Contexts input false productionCatalog with
                | Resolved resolved -> Some resolved.Id
                | NoMatch
                | NoAvailableMatch _ -> None)
            |> Set.ofList

        require
            (displayedIds = resolvedIds
             && projection.PossibleInputs
                |> List.forall (fun input ->
                    match
                        ModalInput.resolve
                            projection.Contexts
                            input.InputGesture
                            false
                            productionCatalog
                    with
                    | Resolved resolved -> resolved.Id = input.Id
                    | NoMatch
                    | NoAvailableMatch _ -> false))
            (label + " possible inputs diverged from live resolution.")

    assertProjectionIsExact
        editorProjection
        productionEditorCatalog
        "Editor"

    let repeatedF2 =
        productionEditorCatalog
        |> ModalInput.resolve
            editorProjection.Contexts
            (gesture "F2" None plain KeyDown)
            true

    require
        (repeatedF2 = NoMatch)
        "A repeated production F2 toggle was not ignored."

    let m3Editor =
        { MapEditor.initial with
            SelectedUnit = None
            SelectedUnits = Set.empty
            Tool = Select
            Gesture = IdleGesture }
    let m3Facts panHeld editor =
        { Editor = editor
          ActiveDomain =
              match editor.Tool with
              | Terrain _ -> TerrainDomain
              | _ -> UnitDomain
          PanHeld = panHeld
          InputHelpExpanded = false }
    let m3SelectFacts = m3Facts false m3Editor
    let m3SelectCatalog = ModalInput.editorCatalog m3SelectFacts
    let m3SelectContexts = ModalInput.deriveEditorContexts m3SelectFacts
    let resolveM3 contexts catalog keyValue modifiers =
        ModalInput.resolve
            contexts
            (gesture keyValue None modifiers KeyDown)
            false
            catalog
        |> outcomeName
    let m3Terrain =
        { m3Editor with Tool = Terrain RectangleTool }
    let m3TerrainFacts = m3Facts false m3Terrain
    let m3TerrainCatalog = ModalInput.editorCatalog m3TerrainFacts
    let m3TerrainContexts = ModalInput.deriveEditorContexts m3TerrainFacts
    let m3PanFacts = m3Facts true m3Terrain
    let m3PanCatalog = ModalInput.editorCatalog m3PanFacts
    let m3PanContexts = ModalInput.deriveEditorContexts m3PanFacts

    require
        (ModalInput.validateCatalog m3SelectCatalog = []
         && ModalInput.validateCatalog m3TerrainCatalog = []
         && ModalInput.validateCatalog m3PanCatalog = []
         && resolveM3 m3SelectContexts m3SelectCatalog "ArrowRight" plain =
            "resolved:editor.cursor.east"
         && resolveM3 m3SelectContexts m3SelectCatalog "Enter" plain =
            "resolved:editor.selection.single"
         && resolveM3 m3SelectContexts m3SelectCatalog "b" plain =
            "resolved:editor.selection.box.begin"
         && resolveM3 m3TerrainContexts m3TerrainCatalog "1" plain =
            "resolved:editor.terrain.value.open"
         && resolveM3 m3TerrainContexts m3TerrainCatalog "]" plain =
            "resolved:editor.terrain.brush.increase"
         && resolveM3 m3TerrainContexts m3TerrainCatalog "ArrowRight" { plain with Shift = true } =
            "resolved:editor.terrain.cursor.paint-east"
         && resolveM3 m3PanContexts m3PanCatalog "ArrowRight" plain =
            "resolved:editor.camera.pan-east"
         && resolveM3 m3PanContexts m3PanCatalog "p" plain = "no-match")
        "M3 Select, Terrain, or held-pan catalog resolution diverged from the vocabulary."

    let boxCommandAt current keyValue =
        let boxEditor =
            { m3Editor with
                Gesture =
                    BoxSelectionGesture(
                        { CellColumn = 0; CellRow = 0 },
                        current
                    ) }
        let facts = m3Facts false boxEditor
        match
            ModalInput.resolve
                (ModalInput.deriveEditorContexts facts)
                (gesture keyValue None plain KeyDown)
                false
                (ModalInput.editorCatalog facts)
        with
        | Resolved input -> input.Command
        | NoMatch
        | NoAvailableMatch _ -> failwith "Expected a box-selection boundary command."

    require
        (boxCommandAt { CellColumn = 0; CellRow = 0 } "ArrowLeft" =
            EditorCommand(
                ExtendEditorBoxSelection
                    { CellColumn = 0
                      CellRow = 0 }
            )
         && boxCommandAt { CellColumn = 0; CellRow = 0 } "ArrowUp" =
            EditorCommand(
                ExtendEditorBoxSelection
                    { CellColumn = 0
                      CellRow = 0 }
            )
         && boxCommandAt
                { CellColumn = m3Editor.Map.Width - 1
                  CellRow = m3Editor.Map.Height - 1 }
                "ArrowRight" =
            EditorCommand(
                ExtendEditorBoxSelection
                    { CellColumn = m3Editor.Map.Width - 1
                      CellRow = m3Editor.Map.Height - 1 }
            )
         && boxCommandAt
                { CellColumn = m3Editor.Map.Width - 1
                  CellRow = m3Editor.Map.Height - 1 }
                "ArrowDown" =
            EditorCommand(
                ExtendEditorBoxSelection
                    { CellColumn = m3Editor.Map.Width - 1
                      CellRow = m3Editor.Map.Height - 1 }
            ))
        "Box-selection catalog movement escaped the map boundary."

    let selectedRegionEditor =
        { m3Editor with
            SelectedRegion = Some 7
            Gesture = SelectedObjectActionsGesture }
    let selectedRegionFacts = m3Facts false selectedRegionEditor
    let selectedRegionProjection =
        ModalInput.projectEditor
            selectedRegionFacts
            (ModalInput.editorCatalog selectedRegionFacts)

    require
        (selectedRegionProjection.Headline = "EDITOR / SELECT / ACTIONS"
         && selectedRegionProjection.Detail = "Region 7 selected")
        "Selected-object Actions projected a region as an empty unit selection."

    let simulator =
        MapEditorSimulator.tryHandoff MapEditor.initial
        |> Result.defaultWith failwith
    let pausedSimulatorFacts =
        { SimulatorHandoffPresent = true
          SimulatorIsRunning = false
          SimulatorHasRoutePreview = false
          SimulatorRevisionIsStale = true
          InputHelpExpanded = false }
    let productionSimulatorCatalog =
        ModalInput.simulatorCatalog MapEditor.initial.SelectedUnit (Some simulator)
    let simulatorProjection =
        ModalInput.projectSimulator
            pausedSimulatorFacts
            MapEditor.initial.SelectedUnit
            (Some simulator)
            productionSimulatorCatalog

    require
        (ModalInput.validateCatalog productionSimulatorCatalog = []
         && simulatorProjection.Headline = "SIMULATOR / PAUSED"
         && simulatorProjection.Detail.Contains("revision stale")
         && simulatorProjection.PossibleInputs
            |> List.exists (fun input -> input.Id = "simulator.panel.toggle")
         && simulatorProjection.PossibleInputs
            |> List.exists (fun input -> input.Id = "simulator.run.toggle-space"))
        "The production Simulator projection did not expose live lifecycle and panels."

    assertProjectionIsExact
        simulatorProjection
        productionSimulatorCatalog
        "Simulator"

    let noHandoffCatalog =
        ModalInput.simulatorCatalog None None
    let noHandoffProjection =
        ModalInput.projectSimulator
            { SimulatorHandoffPresent = false
              SimulatorIsRunning = false
              SimulatorHasRoutePreview = false
              SimulatorRevisionIsStale = false
              InputHelpExpanded = false }
            None
            None
            noHandoffCatalog

    require
        (noHandoffProjection.Headline = "SIMULATOR / NO HANDOFF"
         && (noHandoffProjection.PossibleInputs |> List.map _.Id) =
            [ "simulator.help.toggle" ])
        "The no-handoff state disclosed simulator commands that cannot execute."

    assertProjectionIsExact
        noHandoffProjection
        noHandoffCatalog
        "No-handoff Simulator"

    [ outcomeName commit
      outcomeName repeatedCommit
      outcomeName moving
      outcomeName unknown
      outcomeName panRelease
      outcomeName unavailable
      String.concat "|" visible
      String.concat "|" diagnostics
      editorContexts |> List.map string |> String.concat "|"
      simulatorContexts |> List.map string |> String.concat "|"
      noHandoffContexts |> List.map string |> String.concat "|"
      editorProjection.Headline
      simulatorProjection.Headline
      noHandoffProjection.Headline ]
    |> String.concat "\n"
