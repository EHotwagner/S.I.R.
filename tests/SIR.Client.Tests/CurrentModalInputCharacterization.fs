module SIR.Client.TestsCurrentModalInputCharacterization

open SIR.Client

let private require condition message =
    if not condition then failwith message

type private KeyCase =
    { Name: string
      Key: string
      ControlOrMeta: bool
      Shift: bool
      Expected: CurrentKeyCommand option }

let private editorCases =
    [ { Name = "Ctrl+Shift+Z redo"
        Key = "z"
        ControlOrMeta = true
        Shift = true
        Expected = Some(CurrentEditorAction RedoEditorCommand) }
      { Name = "Ctrl+Z undo"
        Key = "Z"
        ControlOrMeta = true
        Shift = false
        Expected = Some(CurrentEditorAction UndoEditorCommand) }
      { Name = "Ctrl+Y redo"
        Key = "y"
        ControlOrMeta = true
        Shift = true
        Expected = Some(CurrentEditorAction RedoEditorCommand) }
      { Name = "Ctrl+C copy"
        Key = "C"
        ControlOrMeta = true
        Shift = false
        Expected = Some(CurrentEditorAction CopyEditorSelection) }
      { Name = "Ctrl+V paste"
        Key = "v"
        ControlOrMeta = true
        Shift = false
        Expected = Some(CurrentEditorAction PasteEditorClipboard) }
      { Name = "Ctrl+D duplicate"
        Key = "D"
        ControlOrMeta = true
        Shift = false
        Expected = Some(CurrentEditorAction DuplicateEditorSelection) }
      { Name = "Ctrl+A select all"
        Key = "a"
        ControlOrMeta = true
        Shift = false
        Expected = Some(CurrentEditorAction SelectAllInActiveDomain) }
      { Name = "Delete selection"
        Key = "Delete"
        ControlOrMeta = false
        Shift = false
        Expected = Some(CurrentEditorAction DeleteEditorSelection) }
      { Name = "Backspace selection"
        Key = "Backspace"
        ControlOrMeta = false
        Shift = true
        Expected = Some(CurrentEditorAction DeleteEditorSelection) }
      { Name = "Previous issue"
        Key = "["
        ControlOrMeta = false
        Shift = false
        Expected = Some(CurrentEditorAction SelectPreviousIssue) }
      { Name = "Next issue"
        Key = "]"
        ControlOrMeta = false
        Shift = true
        Expected = Some(CurrentEditorAction SelectNextIssue) }
      { Name = "Space held"
        Key = " "
        ControlOrMeta = false
        Shift = true
        Expected = Some(CurrentSetEditorSpaceHeld true) }
      { Name = "Shift+1 open terrain"
        Key = "1"
        ControlOrMeta = false
        Shift = true
        Expected = Some(CurrentEditorAction(ChooseTerrain Open)) }
      { Name = "Shift+2 rough terrain"
        Key = "2"
        ControlOrMeta = false
        Shift = true
        Expected = Some(CurrentEditorAction(ChooseTerrain Rough)) }
      { Name = "Shift+3 blocked terrain"
        Key = "3"
        ControlOrMeta = false
        Shift = true
        Expected = Some(CurrentEditorAction(ChooseTerrain Blocked)) }
      { Name = "Shift+4 objective terrain"
        Key = "4"
        ControlOrMeta = false
        Shift = true
        Expected = Some(CurrentEditorAction(ChooseTerrain Objective)) }
      { Name = "Exclamation open terrain"
        Key = "!"
        ControlOrMeta = false
        Shift = false
        Expected = Some(CurrentEditorAction(ChooseTerrain Open)) }
      { Name = "At rough terrain"
        Key = "@"
        ControlOrMeta = false
        Shift = false
        Expected = Some(CurrentEditorAction(ChooseTerrain Rough)) }
      { Name = "Hash blocked terrain"
        Key = "#"
        ControlOrMeta = false
        Shift = false
        Expected = Some(CurrentEditorAction(ChooseTerrain Blocked)) }
      { Name = "Dollar objective terrain"
        Key = "$"
        ControlOrMeta = false
        Shift = false
        Expected = Some(CurrentEditorAction(ChooseTerrain Objective)) }
      { Name = "Fit board"
        Key = "0"
        ControlOrMeta = false
        Shift = true
        Expected = Some(CurrentEditorWorkspaceAction FitEditorBoard) }
      { Name = "Reset camera"
        Key = "1"
        ControlOrMeta = false
        Shift = false
        Expected = Some(CurrentEditorWorkspaceAction ResetEditorCamera) }
      { Name = "Frame selection"
        Key = "F"
        ControlOrMeta = false
        Shift = true
        Expected = Some(CurrentEditorWorkspaceAction FrameEditorSelection) }
      { Name = "Select tool"
        Key = "v"
        ControlOrMeta = false
        Shift = false
        Expected = Some CurrentChooseSelectAndShowTerrainPanel }
      { Name = "Terrain panel"
        Key = "T"
        ControlOrMeta = false
        Shift = true
        Expected = Some(CurrentChooseEditorPanel CurrentTerrainPanel) }
      { Name = "Pencil tool"
        Key = "p"
        ControlOrMeta = false
        Shift = false
        Expected = Some(CurrentEditorAction(ChooseTool(Terrain PencilTool))) }
      { Name = "Rectangle tool"
        Key = "R"
        ControlOrMeta = false
        Shift = true
        Expected = Some(CurrentEditorAction(ChooseTool(Terrain RectangleTool))) }
      { Name = "Line tool"
        Key = "l"
        ControlOrMeta = false
        Shift = false
        Expected = Some(CurrentEditorAction(ChooseTool(Terrain LineTool))) }
      { Name = "Flood-fill tool"
        Key = "G"
        ControlOrMeta = false
        Shift = true
        Expected = Some(CurrentEditorAction(ChooseTool(Terrain FloodFillTool))) }
      { Name = "Eyedropper tool"
        Key = "i"
        ControlOrMeta = false
        Shift = false
        Expected = Some(CurrentEditorAction(ChooseTool(Terrain EyedropperTool))) }
      { Name = "Erase tool"
        Key = "X"
        ControlOrMeta = false
        Shift = true
        Expected = Some(CurrentEditorAction(ChooseTool(Terrain EraseTool))) }
      { Name = "Unit panel"
        Key = "u"
        ControlOrMeta = false
        Shift = false
        Expected = Some(CurrentChooseEditorPanel CurrentUnitPanel) }
      { Name = "Edge panel"
        Key = "E"
        ControlOrMeta = false
        Shift = true
        Expected = Some(CurrentChooseEditorPanel CurrentEdgePanel) }
      { Name = "Zone panel"
        Key = "z"
        ControlOrMeta = false
        Shift = false
        Expected = Some(CurrentChooseEditorPanel CurrentZonePanel) }
      { Name = "Document panel"
        Key = "M"
        ControlOrMeta = false
        Shift = true
        Expected = Some(CurrentChooseEditorPanel CurrentDocumentPanel) }
      { Name = "Toggle editor panel"
        Key = "F2"
        ControlOrMeta = false
        Shift = false
        Expected = Some CurrentToggleEditorPanel }
      { Name = "Toggle editor inspector"
        Key = "F3"
        ControlOrMeta = false
        Shift = true
        Expected = Some(CurrentEditorWorkspaceAction ToggleEditorInspector) }
      { Name = "Editor escape"
        Key = "Escape"
        ControlOrMeta = false
        Shift = true
        Expected = Some CurrentEscapeEditor } ]

let private simulatorCases =
    [ "F2", Some CurrentToggleSimulatorPanel
      "ArrowLeft", Some(CurrentSimulatorAction(MoveSimulatorPreview(-1, 0)))
      "ArrowRight", Some(CurrentSimulatorAction(MoveSimulatorPreview(1, 0)))
      "ArrowUp", Some(CurrentSimulatorAction(MoveSimulatorPreview(0, -1)))
      "ArrowDown", Some(CurrentSimulatorAction(MoveSimulatorPreview(0, 1)))
      "Enter", Some(CurrentSimulatorAction CommitSimulatorPreview)
      "Escape", Some(CurrentSimulatorAction ResetSimulatorPreview)
      " ", Some(CurrentSimulatorAction ToggleSimulatorRun)
      "k", Some(CurrentSimulatorAction ToggleSimulatorRun)
      "K", Some(CurrentSimulatorAction ToggleSimulatorRun) ]

let run () =
    for case in editorCases do
        let keyForms =
            if case.Key.Length = 1 && System.Char.IsLetter case.Key[0] then
                [ case.Key.ToLowerInvariant(); case.Key.ToUpperInvariant() ]
                |> List.distinct
            else
                [ case.Key ]

        for key in keyForms do
            let actual =
                CurrentModalInput.resolveKeyDown
                    CurrentEditor
                    key
                    case.ControlOrMeta
                    case.Shift
                    false

            require
                (actual = case.Expected)
                $"Current Editor key branch changed for {case.Name} ({key}): {actual}."

            let repeated =
                CurrentModalInput.resolveKeyDown
                    CurrentEditor
                    key
                    case.ControlOrMeta
                    case.Shift
                    true

            require
                (repeated = case.Expected)
                $"Current Editor repeat behavior changed for {case.Name} ({key}): {repeated}."

    for key, expected in simulatorCases do
        for controlOrMeta, shift in [ false, false; true, false; false, true; true, true ] do
            let initial =
                CurrentModalInput.resolveKeyDown
                    CurrentSimulator
                    key
                    controlOrMeta
                    shift
                    false
            let repeated =
                CurrentModalInput.resolveKeyDown
                    CurrentSimulator
                    key
                    controlOrMeta
                    shift
                    true

            require
                (initial = expected && repeated = expected)
                $"Current Simulator key branch or modifier/repeat behavior changed for {key}."

    let modifiedV =
        CurrentModalInput.resolveKeyDown CurrentEditor "v" true false false
    let modifiedZ =
        CurrentModalInput.resolveKeyDown CurrentEditor "z" true true false
    let shiftedOne =
        CurrentModalInput.resolveKeyDown CurrentEditor "1" false true false
    let modifiedT =
        CurrentModalInput.resolveKeyDown CurrentEditor "t" true false false
    let modifiedEscape =
        CurrentModalInput.resolveKeyDown CurrentEditor "Escape" true false false

    require
        (modifiedV = Some(CurrentEditorAction PasteEditorClipboard)
         && modifiedZ = Some(CurrentEditorAction RedoEditorCommand)
         && shiftedOne = Some(CurrentEditorAction(ChooseTerrain Open))
         && modifiedT = None
         && modifiedEscape = None)
        "Current Editor modifier precedence changed."

    let idleEscape =
        CurrentModalInput.editorEscapeActions IdleGesture
    let gestureEscape =
        CurrentModalInput.editorEscapeActions (
            BoxSelectionGesture(
                { CellColumn = 1; CellRow = 2 },
                { CellColumn = 3; CellRow = 4 }
            )
        )

    require
        (idleEscape = (CancelEditorPointers, SelectEditorUnit None)
         && gestureEscape = (CancelEditorPointers, CancelEditorGesture))
        "Current Editor Escape precedence changed."

    require
        (not (CurrentModalInput.acceptsKeyDown InputElement)
         && not (CurrentModalInput.acceptsKeyDown TextAreaElement)
         && not (CurrentModalInput.acceptsKeyDown SelectElement)
         && CurrentModalInput.acceptsKeyDown ContentEditableElement
         && CurrentModalInput.acceptsKeyDown ApplicationElement)
        "The current browser text-entry exclusion boundary changed."

    let spaceUp = CurrentModalInput.resolveKeyUp " "
    let namedSpaceUp = CurrentModalInput.resolveKeyUp "Space"

    require
        (spaceUp = Some(CurrentSetEditorSpaceHeld false)
         && namedSpaceUp = None
         && CurrentModalInput.spaceHeldAfterWorkspaceChange true = false
         && CurrentModalInput.spaceHeldAfterFocusLoss true)
        "Current Space key-up, focus-loss, or workspace-change behavior changed."

    let otherEscape =
        CurrentModalInput.resolveKeyDown CurrentOther "Escape" false false false
    let otherSpace =
        CurrentModalInput.resolveKeyDown CurrentOther " " false false false
    let unknownEditor =
        CurrentModalInput.resolveKeyDown CurrentEditor "Q" false false false
    let unknownSimulator =
        CurrentModalInput.resolveKeyDown CurrentSimulator "Q" false false false

    require
        (otherEscape = None
         && otherSpace = None
         && unknownEditor = None
         && unknownSimulator = None)
        "A no-match input resolved, or Editor/Simulator input leaked into another workspace."

    // These are the durable commands currently shared by keyboard and visible
    // toolbar/inspector routes. Pointer, touch, and object-list-only commands
    // are recorded separately in the reviewed baseline document.
    let visibleEquivalentActions =
        [ UndoEditorCommand
          RedoEditorCommand
          CopyEditorSelection
          PasteEditorClipboard
          DuplicateEditorSelection
          DeleteEditorSelection
          SelectAllInActiveDomain
          SelectPreviousIssue
          SelectNextIssue
          ChooseTerrain Open
          ChooseTerrain Rough
          ChooseTerrain Blocked
          ChooseTerrain Objective
          ChooseTool Select
          ChooseTool(Terrain PencilTool)
          ChooseTool(Terrain RectangleTool)
          ChooseTool(Terrain LineTool)
          ChooseTool(Terrain FloodFillTool)
          ChooseTool(Terrain EyedropperTool)
          ChooseTool(Terrain EraseTool) ]

    require
        (visibleEquivalentActions.Length = 20
         && visibleEquivalentActions |> List.distinct |> List.length = 20)
        "The locked keyboard/visible-command equivalence set changed."

    printfn
        "Current modal-input characterization passed: %d Editor branches, %d Simulator key forms, text-entry boundary, modifier precedence, repeat, Escape, Space down/up, focus loss, workspace change, and visible equivalents."
        editorCases.Length
        simulatorCases.Length
