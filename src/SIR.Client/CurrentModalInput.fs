namespace SIR.Client

/// The browser targets excluded by the keyboard subscription before the
/// current Editor and Simulator key branches run.
type CurrentInputTarget =
    | InputElement
    | TextAreaElement
    | SelectElement
    | ContentEditableElement
    | ApplicationElement

type CurrentInputWorkspace =
    | CurrentEditor
    | CurrentSimulator
    | CurrentOther

type CurrentEditorPanel =
    | CurrentTerrainPanel
    | CurrentUnitPanel
    | CurrentEdgePanel
    | CurrentZonePanel
    | CurrentDocumentPanel

/// A reviewable description of the behavior implemented by the legacy
/// KeyPressed branches. This is deliberately a characterization surface, not
/// the proposed modal command catalog.
type CurrentKeyCommand =
    | CurrentEditorAction of MapEditorAction
    | CurrentEditorWorkspaceAction of EditorWorkspaceAction
    | CurrentChooseEditorPanel of CurrentEditorPanel
    | CurrentChooseSelectAndShowTerrainPanel
    | CurrentToggleEditorPanel
    | CurrentToggleSimulatorPanel
    | CurrentEscapeEditor
    | CurrentSetEditorSpaceHeld of bool
    | CurrentSimulatorAction of SimulatorAction

[<RequireQualifiedAccess>]
module CurrentModalInput =
    /// Mirrors the browser text-entry boundary after M2 closed the
    /// characterized content-editable gap.
    let acceptsKeyDown target =
        match target with
        | InputElement
        | TextAreaElement
        | SelectElement
        | ContentEditableElement -> false
        | ApplicationElement -> true

    /// Returns the current key-down command. Destructive deletion is
    /// deliberately suppressed for repeated key-down events.
    let resolveKeyDown workspace key controlOrMeta shift repeat =
        match workspace with
        | CurrentEditor ->
            match key, controlOrMeta, shift with
            | ("z" | "Z"), true, true ->
                Some(CurrentEditorAction RedoEditorCommand)
            | ("z" | "Z"), true, false ->
                Some(CurrentEditorAction UndoEditorCommand)
            | ("y" | "Y"), true, _ ->
                Some(CurrentEditorAction RedoEditorCommand)
            | ("c" | "C"), true, _ ->
                Some(CurrentEditorAction CopyEditorSelection)
            | ("v" | "V"), true, _ ->
                Some(CurrentEditorAction PasteEditorClipboard)
            | ("d" | "D"), true, _ ->
                Some(CurrentEditorAction DuplicateEditorSelection)
            | ("a" | "A"), true, _ ->
                Some(CurrentEditorAction SelectAllInActiveDomain)
            | ("Delete" | "Backspace"), false, _ when not repeat ->
                Some(CurrentEditorAction DeleteEditorSelection)
            | "[", false, _ ->
                Some(CurrentEditorAction SelectPreviousIssue)
            | "]", false, _ ->
                Some(CurrentEditorAction SelectNextIssue)
            | " ", false, _ ->
                Some(CurrentSetEditorSpaceHeld true)
            | "1", false, true ->
                Some(CurrentEditorAction(ChooseTerrain Open))
            | "2", false, true ->
                Some(CurrentEditorAction(ChooseTerrain Rough))
            | "3", false, true ->
                Some(CurrentEditorAction(ChooseTerrain Blocked))
            | "4", false, true ->
                Some(CurrentEditorAction(ChooseTerrain Objective))
            | "!", false, _ ->
                Some(CurrentEditorAction(ChooseTerrain Open))
            | "@", false, _ ->
                Some(CurrentEditorAction(ChooseTerrain Rough))
            | "#", false, _ ->
                Some(CurrentEditorAction(ChooseTerrain Blocked))
            | "$", false, _ ->
                Some(CurrentEditorAction(ChooseTerrain Objective))
            | "0", false, _ ->
                Some(CurrentEditorWorkspaceAction FitEditorBoard)
            | "1", false, _ ->
                Some(CurrentEditorWorkspaceAction ResetEditorCamera)
            | ("f" | "F"), false, _ ->
                Some(CurrentEditorWorkspaceAction FrameEditorSelection)
            | ("v" | "V"), false, _ ->
                Some CurrentChooseSelectAndShowTerrainPanel
            | ("t" | "T"), false, _ ->
                Some(CurrentChooseEditorPanel CurrentTerrainPanel)
            | ("p" | "P"), false, _ ->
                Some(CurrentEditorAction(ChooseTool(Terrain PencilTool)))
            | ("r" | "R"), false, _ ->
                Some(CurrentEditorAction(ChooseTool(Terrain RectangleTool)))
            | ("l" | "L"), false, _ ->
                Some(CurrentEditorAction(ChooseTool(Terrain LineTool)))
            | ("g" | "G"), false, _ ->
                Some(CurrentEditorAction(ChooseTool(Terrain FloodFillTool)))
            | ("i" | "I"), false, _ ->
                Some(CurrentEditorAction(ChooseTool(Terrain EyedropperTool)))
            | ("x" | "X"), false, _ ->
                Some(CurrentEditorAction(ChooseTool(Terrain EraseTool)))
            | ("u" | "U"), false, _ ->
                Some(CurrentChooseEditorPanel CurrentUnitPanel)
            | ("e" | "E"), false, _ ->
                Some(CurrentChooseEditorPanel CurrentEdgePanel)
            | ("z" | "Z"), false, _ ->
                Some(CurrentChooseEditorPanel CurrentZonePanel)
            | ("m" | "M"), false, _ ->
                Some(CurrentChooseEditorPanel CurrentDocumentPanel)
            | "F2", false, _ ->
                Some CurrentToggleEditorPanel
            | "F3", false, _ ->
                Some(CurrentEditorWorkspaceAction ToggleEditorInspector)
            | "Escape", false, _ ->
                Some CurrentEscapeEditor
            | _ -> None
        | CurrentSimulator ->
            match key with
            | "F2" -> Some CurrentToggleSimulatorPanel
            | "ArrowLeft" ->
                Some(CurrentSimulatorAction(MoveSimulatorPreview(-1, 0)))
            | "ArrowRight" ->
                Some(CurrentSimulatorAction(MoveSimulatorPreview(1, 0)))
            | "ArrowUp" ->
                Some(CurrentSimulatorAction(MoveSimulatorPreview(0, -1)))
            | "ArrowDown" ->
                Some(CurrentSimulatorAction(MoveSimulatorPreview(0, 1)))
            | "Enter" ->
                Some(CurrentSimulatorAction CommitSimulatorPreview)
            | "Escape" ->
                Some(CurrentSimulatorAction ResetSimulatorPreview)
            | " "
            | "k"
            | "K" ->
                Some(CurrentSimulatorAction ToggleSimulatorRun)
            | _ -> None
        | CurrentOther -> None

    /// Key-up currently bypasses the text-entry and workspace checks.
    let resolveKeyUp key =
        if key = " " then
            Some(CurrentSetEditorSpaceHeld false)
        else
            None

    /// Resolves the transient destructive-confirmation layer before ordinary
    /// Editor commands, independent of which non-text application element
    /// currently owns focus.
    let resolvePendingDestructiveKey key controlOrMeta shift alt repeat =
        if controlOrMeta || shift || alt || repeat then
            None
        else
            match key with
            | "Enter" -> Some ConfirmDestructiveChange
            | "Escape" -> Some CancelDestructiveChange
            | _ -> None

    /// Escape first cancels pointer state, then cancels an active gesture or
    /// clears unit selection when no gesture is active.
    let editorEscapeActions gesture =
        CancelEditorPointers,
        if gesture <> IdleGesture then
            CancelEditorGesture
        else
            SelectEditorUnit None

    /// Workspace changes clear the current held-Space flag.
    let spaceHeldAfterWorkspaceChange _ = false

    /// There is currently no window-blur handler, so focus loss preserves the
    /// held-Space flag until key-up or a workspace change.
    let spaceHeldAfterFocusLoss held = held
