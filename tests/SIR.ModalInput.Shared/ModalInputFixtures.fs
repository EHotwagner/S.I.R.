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
              SimulatorControllerSelection = None
              SimulatorRevisionIsStale = true
              InputHelpExpanded = false }

    let noHandoffContexts =
        ModalInput.deriveSimulatorContexts
            { SimulatorHandoffPresent = false
              SimulatorIsRunning = true
              SimulatorHasRoutePreview = true
              SimulatorControllerSelection = None
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

    let m4Browse =
        { m3Editor with
            Tool = UnitBrowse
            UnitPaletteSearch = ""
            UnitPaletteCursor =
                { PresetId = Some "goblin"
                  FactionIndex = 0
                  ResultIndex = 2 } }
    let m4BrowseFacts = m3Facts false m4Browse
    let m4BrowseCatalog = ModalInput.editorCatalog m4BrowseFacts
    let m4BrowseContexts = ModalInput.deriveEditorContexts m4BrowseFacts
    let m4Place =
        m4Browse
        |> MapEditor.update ArmUnitPalettePreset
    let m4PlaceFacts = m3Facts false m4Place
    let m4PlaceCatalog = ModalInput.editorCatalog m4PlaceFacts
    let m4PlaceContexts = ModalInput.deriveEditorContexts m4PlaceFacts
    let m4Move =
        { m3Editor with
            Tool = Select
            SelectedUnit = Some 1
            SelectedUnits = Set.singleton 1 }
        |> MapEditor.update (BeginUnitMove { CellColumn = 0; CellRow = 0 })
    let m4MoveFacts = m3Facts false m4Move
    let m4MoveCatalog = ModalInput.editorCatalog m4MoveFacts
    let m4MoveContexts = ModalInput.deriveEditorContexts m4MoveFacts

    require
        (ModalInput.validateCatalog m4BrowseCatalog = []
         && ModalInput.validateCatalog m4PlaceCatalog = []
         && ModalInput.validateCatalog m4MoveCatalog = []
         && resolveM3 m4BrowseContexts m4BrowseCatalog "ArrowDown" plain =
            "resolved:editor.unit.preset.next-arrow"
         && resolveM3 m4BrowseContexts m4BrowseCatalog "PageDown" plain =
            "resolved:editor.unit.preset.next-faction"
         && resolveM3 m4BrowseContexts m4BrowseCatalog "Enter" plain =
            "resolved:editor.unit.preset.arm"
         && resolveM3 m4BrowseContexts m4BrowseCatalog "/" plain =
            "resolved:editor.unit.preset.search"
         && resolveM3 m4PlaceContexts m4PlaceCatalog "Enter" plain =
            "resolved:editor.unit.place.commit"
         && resolveM3 m4PlaceContexts m4PlaceCatalog "Enter" { plain with Shift = true } =
            "resolved:editor.unit.place.commit-return"
         && resolveM3 m4MoveContexts m4MoveCatalog "ArrowRight" { plain with Shift = true } =
            "resolved:editor.unit.move.east-large"
         && resolveM3 m4MoveContexts m4MoveCatalog "Backspace" plain =
            "resolved:editor.unit.move.reset")
        "M4 Browse, Place, or atomic movement catalog resolution diverged from the vocabulary."

    let repeatedDelete =
        ModalInput.resolve
            (ModalInput.deriveEditorContexts (m3Facts false { m3Editor with SelectedUnits = Set.singleton 1 }))
            (gesture "Delete" None plain KeyDown)
            true
            (ModalInput.editorCatalog (m3Facts false { m3Editor with SelectedUnits = Set.singleton 1 }))
    let confirmationEditor =
        { m3Editor with PendingDestructiveChange = Some(UnitDeletionPending [| 1 |]) }
    let confirmationFacts = m3Facts false confirmationEditor
    let confirmationCatalog = ModalInput.editorCatalog confirmationFacts
    require
        (repeatedDelete = NoMatch
         && resolveM3
                (ModalInput.deriveEditorContexts confirmationFacts)
                confirmationCatalog
                "Enter"
                plain = "resolved:editor.confirmation.confirm")
        "M4 destructive key repeat or explicit Enter confirmation policy regressed."

    let m5Facts domain editor =
        { Editor = editor
          ActiveDomain = domain
          PanHeld = false
          InputHelpExpanded = false }
    let resolveM5 facts keyValue modifiers repeated =
        ModalInput.resolve
            (ModalInput.deriveEditorContexts facts)
            (gesture keyValue None modifiers KeyDown)
            repeated
            (ModalInput.editorCatalog facts)
        |> outcomeName

    let edgeEditor =
        { m3Editor with
            Tool = Edge(EastEdge, Wall)
            EdgeCursor = 1, 1, EastEdge }
    let edgeFacts = m5Facts EdgeDomain edgeEditor
    let edgePolylineFacts =
        m5Facts
            EdgeDomain
            { edgeEditor with
                Gesture = EdgePolylineGesture(Wall, [| 1, 1, EastEdge |]) }
    require
        (ModalInput.validateCatalog (ModalInput.editorCatalog edgeFacts) = []
         && ModalInput.validateCatalog (ModalInput.editorCatalog edgePolylineFacts) = []
         && resolveM5 edgeFacts "w" plain false = "resolved:editor.edge.kind.wall"
         && resolveM5 edgeFacts "d" plain false = "resolved:editor.edge.kind.door"
         && resolveM5 edgeFacts "n" plain false = "resolved:editor.edge.kind.window"
         && resolveM5 edgeFacts "r" plain false = "resolved:editor.edge.orientation.rotate"
         && resolveM5 edgeFacts "ArrowRight" plain true = "resolved:editor.edge.cursor.east"
         && resolveM5 edgeFacts "ArrowDown" { plain with Shift = true } true =
            "resolved:editor.edge.polyline.south"
         && resolveM5 edgeFacts "Enter" plain false = "resolved:editor.edge.activate"
         && resolveM5 edgePolylineFacts "Enter" plain false = "resolved:editor.gesture.commit"
         && resolveM5 edgePolylineFacts "Backspace" plain false =
            "resolved:editor.edge.polyline.backtrack"
         && resolveM5 edgeFacts "o" plain false = "resolved:editor.edge.door.toggle"
         && resolveM5 edgeFacts "x" plain false = "resolved:editor.edge.erase"
         && resolveM5 edgeFacts "s" plain false = "resolved:editor.edge.split"
         && resolveM5 edgeFacts "j" plain false = "resolved:editor.edge.join")
        "M5 semantic-edge catalog routes or polyline precedence diverged."

    let regionBase =
        { m3Editor with
            Map = { m3Editor.Map with Regions = Map.empty; NextRegionId = 1 }
            SelectedRegion = None }
        |> MapEditor.update (
            CreateRectangleRegion(
                ObjectiveRegion,
                { CellColumn = 1; CellRow = 1 },
                { CellColumn = 2; CellRow = 2 }
            )
        )
    let regionIdleFacts = m5Facts RegionDomain regionBase
    let regionPurpose =
        regionBase |> MapEditor.update BeginNewRegion
    let regionPurposeFacts = m5Facts RegionDomain regionPurpose
    let regionShape =
        regionPurpose |> MapEditor.update (ChooseRegionPurpose(DeploymentZone Blue))
    let regionShapeFacts = m5Facts RegionDomain regionShape
    let regionRectangle =
        regionShape |> MapEditor.update (ChooseRegionShape RectangleRegionShape)
    let regionRectangleFacts = m5Facts RegionDomain regionRectangle
    let regionPolygon =
        regionShape |> MapEditor.update (ChooseRegionShape PolygonRegionShape)
    let regionPolygonFacts = m5Facts RegionDomain regionPolygon
    let regionMove =
        regionBase |> MapEditor.update BeginSelectedRegionMove
    let regionMoveFacts = m5Facts RegionDomain regionMove
    let regionResize =
        regionBase |> MapEditor.update BeginSelectedRegionResize
    let regionResizeFacts = m5Facts RegionDomain regionResize
    let polygonBase =
        { regionBase with
            Map =
                { regionBase.Map with
                    Regions =
                        Map.ofList [
                            1,
                            { Id = 1
                              Purpose = ObjectiveRegion
                              Geometry =
                                RegionPolygon(
                                    [| { CellColumn = 1; CellRow = 1 }
                                       { CellColumn = 4; CellRow = 1 }
                                       { CellColumn = 2; CellRow = 4 } |]
                                )
                              Behavior = NoRegionBehavior }
                        ] }
            SelectedRegion = Some 1 }
    let regionVertex =
        polygonBase |> MapEditor.update BeginSelectedRegionVertexEdit
    let regionVertexFacts = m5Facts RegionDomain regionVertex
    let regionPurposeEdit =
        regionBase |> MapEditor.update BeginSelectedRegionPurposeEdit
    let regionPurposeEditFacts = m5Facts RegionDomain regionPurposeEdit

    let m5RegionOutcomes =
        [ resolveM5 regionIdleFacts "n" plain false
          resolveM5 regionIdleFacts "m" plain false
          resolveM5 regionIdleFacts "r" plain false
          resolveM5 regionPurposeFacts "b" plain false
          resolveM5 regionShapeFacts "r" plain false
          resolveM5 regionRectangleFacts "Enter" plain false
          resolveM5 regionPolygonFacts "Enter" { plain with Shift = true } false
          resolveM5 regionMoveFacts "ArrowRight" { plain with Shift = true } true
          resolveM5 regionMoveFacts "Backspace" plain false
          resolveM5 regionResizeFacts "ArrowLeft" { plain with Shift = true } true
          resolveM5 regionVertexFacts "]" plain true
          resolveM5 regionPurposeEditFacts "r" plain false
          resolveM5 regionPurposeEditFacts "Enter" plain false ]
    require
        ([ regionIdleFacts
           regionPurposeFacts
           regionShapeFacts
           regionRectangleFacts
           regionPolygonFacts
           regionMoveFacts
           regionResizeFacts
           regionVertexFacts
           regionPurposeEditFacts ]
         |> List.forall (ModalInput.editorCatalog >> ModalInput.validateCatalog >> List.isEmpty)
         && resolveM5 regionIdleFacts "n" plain false = "resolved:editor.region.create.begin"
         && resolveM5 regionIdleFacts "m" plain false = "resolved:editor.region.edit.move"
         && resolveM5 regionIdleFacts "r" plain false = "resolved:editor.region.edit.resize"
         && resolveM5 regionPurposeFacts "b" plain false = "resolved:editor.region.purpose.blue"
         && resolveM5 regionShapeFacts "r" plain false = "resolved:editor.region.shape.rectangle"
         && resolveM5 regionRectangleFacts "Enter" plain false =
            "resolved:editor.region.rectangle.activate"
         && resolveM5 regionPolygonFacts "Enter" { plain with Shift = true } false =
            "unavailable:editor.region.polygon.commit"
         && resolveM5 regionMoveFacts "ArrowRight" { plain with Shift = true } true =
            "resolved:editor.region.move.east-large"
         && resolveM5 regionMoveFacts "Backspace" plain false =
            "resolved:editor.region.move.reset"
         && resolveM5 regionResizeFacts "ArrowLeft" { plain with Shift = true } true =
            "resolved:editor.region.resize.origin.east"
         && resolveM5 regionVertexFacts "]" plain true =
            "resolved:editor.region.vertex.next"
         && resolveM5 regionPurposeEditFacts "r" plain false =
            "resolved:editor.region.purpose.red"
         && resolveM5 regionPurposeEditFacts "Enter" plain false =
            "resolved:editor.region.purpose.commit")
        ("M5 nested region construction or resettable edit routes diverged: "
         + String.concat "|" m5RegionOutcomes)

    let documentFacts = m5Facts DocumentDomain m3Editor
    let documentCatalog = ModalInput.editorCatalog documentFacts
    let documentProjection =
        ModalInput.projectEditor documentFacts documentCatalog
    require
        (ModalInput.validateCatalog documentCatalog = []
         && documentProjection.Headline = "EDITOR / DOCUMENT"
         && resolveM5 documentFacts "n" plain false = "resolved:editor.document.new"
         && resolveM5 documentFacts "c" plain false = "resolved:editor.document.clear"
         && resolveM5 documentFacts "s" plain false = "resolved:editor.document.export"
         && resolveM5 documentFacts "i" plain false = "resolved:editor.document.import"
         && resolveM5 documentFacts "b" plain false = "resolved:editor.document.bundle"
         && resolveM5 documentFacts "l" plain false = "resolved:editor.document.layers"
         && resolveM5 documentFacts "g" plain false = "resolved:editor.document.background"
         && resolveM5 documentFacts "r" plain false = "resolved:editor.document.resize"
         && resolveM5 documentFacts "v" plain false = "resolved:editor.document.views"
         && resolveM5 documentFacts "c" plain true = "no-match")
        "M5 document routes, projection, or destructive repeat suppression diverged."

    assertProjectionIsExact
        documentProjection
        documentCatalog
        "M5 Document"

    let simulator =
        MapEditorSimulator.tryHandoff MapEditor.initial
        |> Result.defaultWith failwith
    let pausedSimulatorFacts =
        { SimulatorHandoffPresent = true
          SimulatorIsRunning = false
          SimulatorHasRoutePreview = false
          SimulatorControllerSelection = None
          SimulatorRevisionIsStale = true
          InputHelpExpanded = false }
    let productionSimulatorCatalog =
        ModalInput.simulatorCatalog MapEditor.initial.SelectedUnit (Some simulator) None
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

    let resolveSimulator facts catalog key modifiers repeat =
        ModalInput.resolve
            (ModalInput.deriveSimulatorContexts facts)
            (gesture key None modifiers KeyDown)
            repeat
            catalog
        |> outcomeName

    let pausedInputIds =
        simulatorProjection.PossibleInputs |> List.map _.Id |> Set.ofList

    require
        ([ "simulator.panel.controls"
           "simulator.panel.events"
           "simulator.panel.samples"
           "simulator.unit.previous"
           "simulator.unit.next"
           "simulator.step"
           "simulator.reset.request"
           "simulator.controller.begin"
           "simulator.preview.west" ]
         |> List.forall (fun id -> Set.contains id pausedInputIds))
        "M6 paused Simulator inputs omitted traversal, lifecycle, panels, controller selection, or route begin."

    let runningSimulator = { simulator with IsRunning = true }
    let runningFacts =
        { pausedSimulatorFacts with
            SimulatorIsRunning = true
            SimulatorRevisionIsStale = false }
    let runningCatalog =
        ModalInput.simulatorCatalog
            MapEditor.initial.SelectedUnit
            (Some runningSimulator)
            None
    let runningProjection =
        ModalInput.projectSimulator
            runningFacts
            MapEditor.initial.SelectedUnit
            (Some runningSimulator)
            runningCatalog
    let runningInputIds =
        runningProjection.PossibleInputs |> List.map _.Id |> Set.ofList

    require
        (runningProjection.Headline = "SIMULATOR / RUNNING"
         && [ "simulator.run.toggle-space"
              "simulator.panel.controls"
              "simulator.unit.previous"
              "simulator.unit.next" ]
            |> List.forall (fun id -> Set.contains id runningInputIds)
         && [ "simulator.step"
              "simulator.reset.request"
              "simulator.controller.begin"
              "simulator.preview.west"
              "simulator.preview.commit" ]
            |> List.forall (fun id -> not (Set.contains id runningInputIds))
         && resolveSimulator runningFacts runningCatalog "." plain false = "unavailable:simulator.step"
         && resolveSimulator runningFacts runningCatalog "ArrowRight" plain false =
            "unavailable:simulator.preview.east")
        "M6 running projection disclosed or resolved an unavailable mutation."

    assertProjectionIsExact
        runningProjection
        runningCatalog
        "Running Simulator"

    let unitIds =
        simulator.RuntimeMap.Units |> Map.toArray |> Array.map fst |> Array.sort
    let firstUnit = Array.head unitIds
    let lastUnit = Array.last unitIds
    require
        (ModalInput.traverseSimulatorUnit 1 None simulator = Some firstUnit
         && ModalInput.traverseSimulatorUnit -1 None simulator = Some lastUnit
         && ModalInput.traverseSimulatorUnit 1 (Some lastUnit) simulator = Some firstUnit
         && ModalInput.traverseSimulatorUnit -1 (Some firstUnit) simulator = Some lastUnit)
        "M6 unit traversal was not deterministic and wrapping."

    let selectedId = MapEditor.initial.SelectedUnit |> Option.defaultWith (fun () -> failwith "Expected selected fixture unit.")
    let selectedUnit = Map.find selectedId simulator.RuntimeMap.Units
    let previewing =
        MapEditorSimulator.update
            (MoveSimulatorPreview(1, 0))
            (Some selectedId)
            simulator
    let resetPreview =
        MapEditorSimulator.update
            ResetSimulatorPreviewToOrigin
            (Some selectedId)
            previewing
    let cancelledPreview =
        MapEditorSimulator.update
            ResetSimulatorPreview
            (Some selectedId)
            previewing
    let startedFromPreview =
        MapEditorSimulator.update
            ToggleSimulatorRun
            (Some selectedId)
            previewing
    let runningMutation =
        MapEditorSimulator.update
            (MoveSimulatorPreview(1, 0))
            (Some selectedId)
            runningSimulator
    let runningSingleStep =
        MapEditorSimulator.update
            StepSimulator
            (Some selectedId)
            runningSimulator
    let runningPulse =
        MapEditorSimulator.update
            AdvanceRunningSimulatorTick
            (Some selectedId)
            runningSimulator

    require
        (previewing.PreviewDestination.IsSome
         && resetPreview.PreviewDestination =
            Some
                { CellColumn = selectedUnit.Column
                  CellRow = selectedUnit.Row }
         && cancelledPreview.PreviewDestination.IsNone
         && startedFromPreview.IsRunning
         && startedFromPreview.PreviewDestination.IsNone
         && runningMutation = runningSimulator
         && runningSingleStep = runningSimulator
         && runningPulse.Tick = runningSimulator.Tick + 1)
        "M6 route-preview reset, cancel, run transition, or running guard diverged."

    let controllerFacts =
        { pausedSimulatorFacts with
            SimulatorControllerSelection = Some Scripted
            SimulatorRevisionIsStale = false }
    let controllerCatalog =
        ModalInput.simulatorCatalog
            (Some selectedId)
            (Some simulator)
            (Some Scripted)
    let controllerProjection =
        ModalInput.projectSimulator
            controllerFacts
            (Some selectedId)
            (Some simulator)
            controllerCatalog
    let controllerInputIds =
        controllerProjection.PossibleInputs |> List.map _.Id |> Set.ofList

    require
        (controllerProjection.Headline = "SIMULATOR / CONTROLLER"
         && controllerProjection.Detail.Contains("Scripted AI")
         && [ "simulator.controller.manual"
              "simulator.controller.scripted"
              "simulator.controller.general"
              "simulator.controller.commit"
              "simulator.controller.cancel" ]
            |> List.forall (fun id -> Set.contains id controllerInputIds)
         && not (Set.contains "simulator.run.toggle-space" controllerInputIds)
         && resolveSimulator controllerFacts controllerCatalog "g" plain false =
            "resolved:simulator.controller.general"
         && resolveSimulator controllerFacts controllerCatalog "Enter" plain false =
            "resolved:simulator.controller.commit")
        "M6 controller selection did not own modal input while preserving a separate native script field."

    assertProjectionIsExact
        controllerProjection
        controllerCatalog
        "Simulator controller selection"

    let authoredBefore = MapEditor.export MapEditor.initial
    ModalInput.projectSimulator
        controllerFacts
        (Some selectedId)
        (Some simulator)
        controllerCatalog
    |> ignore
    ModalInput.traverseSimulatorUnit 1 (Some selectedId) simulator |> ignore
    let authoredAfter = MapEditor.export MapEditor.initial

    require
        (authoredBefore = authoredAfter
         && simulator = simulator)
        "M6 input presentation projection entered authored map serialization or simulator runtime state."

    let noHandoffCatalog =
        ModalInput.simulatorCatalog None None None
    let noHandoffProjection =
        ModalInput.projectSimulator
            { SimulatorHandoffPresent = false
              SimulatorIsRunning = false
              SimulatorHasRoutePreview = false
              SimulatorControllerSelection = None
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

    let authoritativeEditorFacts =
        { Editor = MapEditor.initial
          ActiveDomain = TerrainDomain
          PanHeld = false
          InputHelpExpanded = false }
    let authoritativeEditorCatalog =
        ModalInput.editorCatalog authoritativeEditorFacts
    let authoritativeEditorContexts =
        ModalInput.deriveEditorContexts authoritativeEditorFacts
    let compatibilityAliases =
        authoritativeEditorCatalog
        |> List.filter (fun input -> input.Id.Contains(".compat-"))

    require
        (not (List.isEmpty compatibilityAliases)
         && compatibilityAliases
            |> List.forall (fun input ->
                input.Id.EndsWith("-m8")
                && input.Group = "Compatibility aliases"
                && input.Label.Contains("compatibility alias"))
         && ModalInput.validateCatalog authoritativeEditorCatalog = []
         && (authoritativeEditorCatalog
             |> List.map _.Id
             |> List.distinct
             |> List.length) = authoritativeEditorCatalog.Length)
        "M7 production catalog aliases were not labelled, tested, conflict-free, and assigned to M8."

    let authoritativePossible =
        ModalInput.possibleInputs
            authoritativeEditorContexts
            authoritativeEditorCatalog

    require
        (authoritativePossible
         |> List.forall (fun input ->
             match
                 ModalInput.resolve
                     authoritativeEditorContexts
                     input.InputGesture
                     false
                     authoritativeEditorCatalog
             with
             | Resolved resolved -> resolved.Id = input.Id
             | NoMatch
             | NoAvailableMatch _ -> false))
        "M7 possible-input enumeration diverged from production dispatch."

    let panDown =
        ModalInput.resolve
            authoritativeEditorContexts
            (gesture " " None plain KeyDown)
            false
            authoritativeEditorCatalog
    let held =
        HeldInputSession.empty
        |> HeldInputSession.apply (SetEditorPanHeld true)
    let heldFacts =
        { authoritativeEditorFacts with PanHeld = true }
    let panUp =
        ModalInput.resolve
            (ModalInput.deriveEditorContexts heldFacts)
            (gesture " " None plain KeyUp)
            false
            (ModalInput.editorCatalog heldFacts)

    require
        (outcomeName panDown = "resolved:editor.camera.pan-held"
         && outcomeName panUp = "resolved:editor.camera.pan-release"
         && HeldInputSession.contains EditorPan held
         && not (
             held
             |> HeldInputSession.recover
             |> HeldInputSession.contains EditorPan
         )
         && not (ModalInput.acceptsTarget ModalInputTarget.InputElement)
         && not (ModalInput.acceptsTarget ModalInputTarget.TextAreaElement)
         && not (ModalInput.acceptsTarget ModalInputTarget.SelectElement)
         && not (ModalInput.acceptsTarget ModalInputTarget.ContentEditableElement)
         && ModalInput.acceptsTarget ModalInputTarget.ApplicationElement)
        "M7 held-input resolution, recovery, or native-control boundary diverged."

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
      documentProjection.Headline
      simulatorProjection.Headline
      noHandoffProjection.Headline
      outcomeName panDown
      outcomeName panUp
      string compatibilityAliases.Length
      string authoritativePossible.Length ]
    |> String.concat "\n"
