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
      "Previous issue", "[", false, false, EditorCommand SelectPreviousIssue
      "Reset camera", "1", false, false, EditorWorkspaceCommand ResetEditorCamera
      "Select tool", "v", false, false, EditorCommand(ChooseTool Select)
      "Terrain domain", "t", false, false, ChooseEditorDomain TerrainDomain
      "Pencil tool", "p", false, false, EditorCommand(ChooseTool(Terrain PencilTool))
      "Rectangle tool", "r", false, false, EditorCommand(ChooseTool(Terrain RectangleTool))
      "Line tool", "l", false, false, EditorCommand(ChooseTool(Terrain LineTool))
      "Flood-fill tool", "g", false, false, EditorCommand(ChooseTool(Terrain FloodFillTool))
      "Eyedropper tool", "i", false, false, EditorCommand(ChooseTool(Terrain EyedropperTool))
      "Eraser tool", "x", false, false, EditorCommand(ChooseTool(Terrain EraseTool))
      "Unit domain", "u", false, false, ChooseEditorDomain UnitDomain
      "Edge domain", "e", false, false, ChooseEditorDomain EdgeDomain
      "Zone domain", "z", false, false, ChooseEditorDomain RegionDomain
      "Document domain", "m", false, false, ChooseEditorDomain DocumentDomain
      "Toggle editor panel", "F2", false, false, ToggleEditorCommandPanel
      "Toggle inspector", "F3", false, false, EditorWorkspaceCommand ToggleEditorInspector ]
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
        "Authoritative modal catalog passed: %d accepted Editor inputs, retained Simulator corpus, native boundaries, held-input recovery, conflicts, and possible-input enumeration."
        editorCases.Length
