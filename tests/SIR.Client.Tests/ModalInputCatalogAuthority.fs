module SIR.Client.TestsModalInputCatalogAuthority

open SIR.Client

let private require condition message =
    if not condition then failwith message

type private EditorCase =
    { Name: string
      Key: string
      ControlOrMeta: bool
      Shift: bool
      Expected: ModalCommand }

let private editorCases =
    [ "Ctrl+Shift+Z redo", "z", true, true, EditorCommand RedoEditorCommand
      "Ctrl+Z undo", "z", true, false, EditorCommand UndoEditorCommand
      "Ctrl+Y redo", "y", true, false, EditorCommand RedoEditorCommand
      "Ctrl+C copy", "c", true, false, EditorCommand CopyEditorSelection
      "Ctrl+V paste", "v", true, false, EditorCommand PasteEditorClipboard
      "Ctrl+D duplicate", "d", true, false, EditorCommand DuplicateEditorSelection
      "Ctrl+A select all", "a", true, false, EditorCommand SelectAllInActiveDomain
      "Delete selection", "Delete", false, false, EditorCommand DeleteEditorSelection
      "Shift+Backspace compatibility", "Backspace", false, true, EditorCommand DeleteEditorSelection
      "Previous issue", "[", false, false, EditorCommand SelectPreviousIssue
      "Shift+] compatibility", "]", false, true, EditorCommand SelectNextIssue
      "Shift+Space compatibility", " ", false, true, SetEditorPanHeld true
      "Shift+1 open terrain", "1", false, true, EditorCommand(ChooseTerrain Open)
      "Shift+2 rough terrain", "2", false, true, EditorCommand(ChooseTerrain Rough)
      "Shift+3 blocked terrain", "3", false, true, EditorCommand(ChooseTerrain Blocked)
      "Shift+4 objective terrain", "4", false, true, EditorCommand(ChooseTerrain Objective)
      "Exclamation open terrain", "!", false, false, EditorCommand(ChooseTerrain Open)
      "At rough terrain", "@", false, false, EditorCommand(ChooseTerrain Rough)
      "Hash blocked terrain", "#", false, false, EditorCommand(ChooseTerrain Blocked)
      "Dollar objective terrain", "$", false, false, EditorCommand(ChooseTerrain Objective)
      "Shift+0 fit compatibility", "0", false, true, EditorWorkspaceCommand FitEditorBoard
      "Reset camera", "1", false, false, EditorWorkspaceCommand ResetEditorCamera
      "Shift+F frame compatibility", "f", false, true, EditorWorkspaceCommand FrameEditorSelection
      "Select tool", "v", false, false, EditorCommand(ChooseTool Select)
      "Shift+T terrain compatibility", "t", false, true, ChooseEditorDomain TerrainDomain
      "Pencil tool", "p", false, false, EditorCommand(ChooseTool(Terrain PencilTool))
      "Shift+R rectangle compatibility", "r", false, true, EditorCommand(ChooseTool(Terrain RectangleTool))
      "Line tool", "l", false, false, EditorCommand(ChooseTool(Terrain LineTool))
      "Shift+G flood compatibility", "g", false, true, EditorCommand(ChooseTool(Terrain FloodFillTool))
      "Eyedropper tool", "i", false, false, EditorCommand(ChooseTool(Terrain EyedropperTool))
      "Shift+X erase compatibility", "x", false, true, EditorCommand(ChooseTool(Terrain EraseTool))
      "Unit domain", "u", false, false, ChooseEditorDomain UnitDomain
      "Shift+E edge compatibility", "e", false, true, ChooseEditorDomain EdgeDomain
      "Zone domain", "z", false, false, ChooseEditorDomain RegionDomain
      "Shift+M document compatibility", "m", false, true, ChooseEditorDomain DocumentDomain
      "Toggle editor panel", "F2", false, false, ToggleEditorCommandPanel
      "Shift+F3 inspector compatibility", "F3", false, true, EditorWorkspaceCommand ToggleEditorInspector
      "Shift+Escape compatibility", "Escape", false, true, EditorCommand(SelectEditorUnit None) ]
    |> List.map (fun (name, key, controlOrMeta, shift, expected) ->
        { Name = name
          Key = key
          ControlOrMeta = controlOrMeta
          Shift = shift
          Expected = expected })

let private commandFromResolution = function
    | Resolved input -> Some input.Command
    | NoAvailableMatch (input :: _) -> Some input.Command
    | NoAvailableMatch []
    | NoMatch -> None

let private resolveEditor facts key controlOrMeta shift repeat =
    let gesture =
        { Key = NormalizedKey.create key None
          Modifiers =
            { ControlOrMeta = controlOrMeta
              Shift = shift
              Alt = false }
          Phase = KeyDown }
    ModalInput.resolve
        (ModalInput.deriveEditorContexts facts)
        gesture
        repeat
        (ModalInput.editorCatalog facts)

let run () =
    let facts =
        { Editor = MapEditor.initial
          ActiveDomain = TerrainDomain
          PanHeld = false
          InputHelpExpanded = false }
    let editorCatalog = ModalInput.editorCatalog facts

    for case in editorCases do
        let caseFacts =
            match case.Expected with
            | EditorCommand(ChooseTool(Terrain _)) ->
                { facts with
                    Editor =
                        { facts.Editor with
                            Tool = Terrain PencilTool } }
            | ChooseEditorDomain DocumentDomain ->
                { facts with
                    Editor =
                        { facts.Editor with
                            Tool = Terrain PencilTool } }
            | _ -> facts
        let actual =
            resolveEditor
                caseFacts
                case.Key
                case.ControlOrMeta
                case.Shift
                false
            |> commandFromResolution
        require
            (actual = Some case.Expected)
            $"Catalog diverged from characterized Editor input {case.Name}: {actual}."

    require
        (resolveEditor facts "t" true false false = NoMatch
         && resolveEditor facts "Escape" true false false = NoMatch)
        "Browser-reserved modified Editor keys leaked into the modal catalog."

    require
        (resolveEditor facts "Delete" false false true = NoMatch
         && resolveEditor facts "ArrowRight" false false true
            |> commandFromResolution
            |> Option.isSome)
        "Catalog repeat ownership changed for destructive or movement input."

    let held =
        HeldInputSession.empty
        |> HeldInputSession.apply (SetEditorPanHeld true)
    require
        (HeldInputSession.contains EditorPan held
         && not (
             held
             |> HeldInputSession.apply (SetEditorPanHeld false)
             |> HeldInputSession.contains EditorPan
         )
         && not (
             held
             |> HeldInputSession.recover
             |> HeldInputSession.contains EditorPan
         ))
        "Held-input session did not recover on release, focus loss, or workspace change."

    require
        (not (ModalInput.acceptsTarget ModalInputTarget.InputElement)
         && not (ModalInput.acceptsTarget ModalInputTarget.TextAreaElement)
         && not (ModalInput.acceptsTarget ModalInputTarget.SelectElement)
         && not (ModalInput.acceptsTarget ModalInputTarget.ContentEditableElement)
         && ModalInput.acceptsTarget ModalInputTarget.ApplicationElement)
        "Native text-entry boundaries changed."

    let handoff =
        MapEditorSimulator.tryHandoff MapEditor.initial
        |> Result.toOption
    let simulatorFacts =
        { SimulatorHandoffPresent = handoff.IsSome
          SimulatorIsRunning = false
          SimulatorHasRoutePreview = false
          SimulatorControllerSelection = None
          SimulatorRevisionIsStale = false
          InputHelpExpanded = false }
    let simulatorCatalog =
        ModalInput.simulatorCatalog None handoff None
    let resolveSimulator key modifiers repeat =
        ModalInput.resolve
            (ModalInput.deriveSimulatorContexts simulatorFacts)
            { Key = NormalizedKey.create key None
              Modifiers = modifiers
              Phase = KeyDown }
            repeat
            simulatorCatalog
        |> commandFromResolution

    require
        (resolveSimulator "F2" KeyModifiers.none false =
            Some ToggleSimulatorCommandPanel
         && resolveSimulator "ArrowLeft" KeyModifiers.none false =
            Some(SimulatorCommand(MoveSimulatorPreview(-1, 0)))
         && resolveSimulator "ArrowRight" KeyModifiers.none false =
            Some(SimulatorCommand(MoveSimulatorPreview(1, 0)))
         && resolveSimulator "ArrowUp" KeyModifiers.none false =
            Some(SimulatorCommand(MoveSimulatorPreview(0, -1)))
         && resolveSimulator "ArrowDown" KeyModifiers.none false =
            Some(SimulatorCommand(MoveSimulatorPreview(0, 1)))
         && resolveSimulator "Enter" KeyModifiers.none false =
            Some ModalCommand.BeginSimulatorControllerSelection
         && resolveSimulator "Space" KeyModifiers.none false =
            Some(SimulatorCommand ToggleSimulatorRun)
         && resolveSimulator "k" KeyModifiers.none false =
            Some(SimulatorCommand ToggleSimulatorRun)
         && resolveSimulator "Escape" KeyModifiers.none false = None)
        "Catalog diverged from the retained Simulator corpus or its M6 preview qualifier."

    require
        (resolveSimulator
             "ArrowRight"
             { KeyModifiers.none with Shift = true }
             false =
            Some(SimulatorCommand(MoveSimulatorPreview(5, 0)))
         && resolveSimulator
                "ArrowRight"
                { KeyModifiers.none with ControlOrMeta = true }
                false = None)
        "Simulator fast movement or browser-reserved boundary changed."

    let allCatalogs = [ editorCatalog; simulatorCatalog ]
    require
        (allCatalogs |> List.collect ModalInput.validateCatalog |> List.isEmpty)
        "An authoritative production catalog contains a conflict."

    for catalog, contexts in
        [ editorCatalog, ModalInput.deriveEditorContexts facts
          simulatorCatalog, ModalInput.deriveSimulatorContexts simulatorFacts ] do
        for input in ModalInput.possibleInputs contexts catalog do
            match ModalInput.resolve contexts input.InputGesture false catalog with
            | Resolved resolved ->
                require
                    (resolved.Id = input.Id)
                    $"Possible input {input.Id} did not resolve to itself."
            | outcome ->
                failwith $"Possible input {input.Id} was not executable: {outcome}."

    printfn
        "Authoritative modal catalog passed: %d characterized Editor inputs, retained Simulator corpus, native boundaries, compatibility aliases through M8, held-input recovery, conflicts, and possible-input enumeration."
        editorCases.Length
