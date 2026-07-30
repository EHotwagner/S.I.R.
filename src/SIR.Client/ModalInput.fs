namespace SIR.Client

open System

/// A layout-sensitive key value normalized from KeyboardEvent.key. The
/// optional physical code is retained for diagnostics, not binding identity.
[<StructuralEquality; StructuralComparison>]
type NormalizedKey =
    private
        { Value: string
          PhysicalCode: string option }

[<RequireQualifiedAccess>]
module NormalizedKey =
    let create (key: string) (physicalCode: string option) =
        let value =
            if String.IsNullOrEmpty key then
                ""
            else
                match key with
                | " " -> "Space"
                | "Esc" -> "Escape"
                | value when value.Length = 1 -> value.ToLowerInvariant()
                | value -> value

        { Value = value
          PhysicalCode =
            physicalCode
            |> Option.bind (fun value ->
                if String.IsNullOrWhiteSpace value then None else Some value) }

    let value key = key.Value
    let physicalCode key = key.PhysicalCode
    let sameProducedKey left right = left.Value = right.Value

[<StructuralEquality; StructuralComparison>]
type KeyModifiers =
    { ControlOrMeta: bool
      Shift: bool
      Alt: bool }

[<RequireQualifiedAccess>]
module KeyModifiers =
    let none =
        { ControlOrMeta = false
          Shift = false
          Alt = false }

type InputPhase =
    | KeyDown
    | KeyUp

type RepeatPolicy =
    | IgnoreRepeat
    | AllowRepeat

type InputGesture =
    { Key: NormalizedKey
      Modifiers: KeyModifiers
      Phase: InputPhase }

type EditorGestureKind =
    | BoxSelection
    | CommandPreview
    | UnitMovePreview
    | TerrainPreview of TerrainAuthoringTool
    | EdgePolyline

type ModalContext =
    | EditorBase
    | EditorDomain of EditorDomain
    | EditorTool of MapEditorTool
    | EditorGesture of EditorGestureKind
    | EditorPanHeld
    | EditorDestructiveConfirmation
    | SimulatorBase
    | SimulatorPaused
    | SimulatorRunning
    | SimulatorRoutePreview
    | SimulatorRevisionStale
    | SimulatorNoHandoff
    | InputHelpPopup

/// Closed selectors make binding overlap validation deterministic and keep
/// catalog structure inspectable in both .NET and Fable.
type ModalContextSelector =
    | AnyEditorContext
    | AnySimulatorContext
    | ExactContext of ModalContext

type ModalPrecedence =
    | WorkspaceCommands
    | ActiveTool
    | ActiveGestureOrPreview
    | HeldLayer
    | TransientPopup
    | InputPopup

type BindingAvailability =
    | Available
    | Unavailable of reason: string

type SimulatorPanel =
    | ControllerPanel
    | EventPanel
    | SimulatorSamplePanel

/// Commands describe application intent only. The web edge lowers them to its
/// Elmish message union; this module performs no browser or simulation I/O.
type ModalCommand =
    | EditorCommand of MapEditorAction
    | EditorWorkspaceCommand of EditorWorkspaceAction
    | ChooseEditorDomain of EditorDomain
    | ToggleEditorCommandPanel
    | ChooseSimulatorPanel of SimulatorPanel
    | ToggleSimulatorCommandPanel
    | SimulatorCommand of SimulatorAction
    | SetEditorPanHeld of bool
    | ToggleInputHelp

type ModalBinding<'command> =
    { Id: string
      Context: ModalContextSelector
      Precedence: ModalPrecedence
      BindingGesture: InputGesture
      Label: string
      Group: string
      Repeat: RepeatPolicy
      Availability: ModalContext list -> BindingAvailability
      Command: 'command }

type PossibleInput<'command> =
    { Id: string
      InputGesture: InputGesture
      Label: string
      Group: string
      Availability: BindingAvailability
      Command: 'command }

type ModalProjection<'command> =
    { Contexts: ModalContext list
      Breadcrumb: string list
      Headline: string
      Detail: string
      PossibleInputs: PossibleInput<'command> list }

type InputResolution<'command> =
    | Resolved of PossibleInput<'command>
    | NoMatch
    | NoAvailableMatch of PossibleInput<'command> list

type CatalogDiagnostic =
    | DuplicateBindingId of id: string
    | EqualPrecedenceGestureConflict of
        firstId: string *
        secondId: string *
        precedence: ModalPrecedence *
        gesture: InputGesture

type EditorModalFacts =
    { Editor: MapEditorState
      ActiveDomain: EditorDomain
      PanHeld: bool
      InputHelpExpanded: bool }

type SimulatorModalFacts =
    { SimulatorHandoffPresent: bool
      SimulatorIsRunning: bool
      SimulatorHasRoutePreview: bool
      SimulatorRevisionIsStale: bool
      InputHelpExpanded: bool }

[<RequireQualifiedAccess>]
module ModalInput =
    let private precedenceRank = function
        | WorkspaceCommands -> 0
        | ActiveTool -> 1
        | ActiveGestureOrPreview -> 2
        | HeldLayer -> 3
        | TransientPopup -> 4
        | InputPopup -> 5

    let private isEditorContext = function
        | EditorBase
        | EditorDomain _
        | EditorTool _
        | EditorGesture _
        | EditorPanHeld
        | EditorDestructiveConfirmation -> true
        | _ -> false

    let private isSimulatorContext = function
        | SimulatorBase
        | SimulatorPaused
        | SimulatorRunning
        | SimulatorRoutePreview
        | SimulatorRevisionStale
        | SimulatorNoHandoff -> true
        | _ -> false

    let private isSharedPopupContext = function
        | InputHelpPopup -> true
        | _ -> false

    let private editorContextCategory = function
        | EditorBase -> 0
        | EditorDomain _ -> 1
        | EditorTool _ -> 2
        | EditorGesture _ -> 3
        | EditorPanHeld -> 4
        | EditorDestructiveConfirmation -> 5
        | _ -> -1

    let private simulatorContextsCanCoexist left right =
        match left, right with
        | SimulatorPaused, SimulatorRunning
        | SimulatorRunning, SimulatorPaused -> false
        | SimulatorNoHandoff, (SimulatorPaused | SimulatorRunning | SimulatorRoutePreview | SimulatorRevisionStale)
        | (SimulatorPaused | SimulatorRunning | SimulatorRoutePreview | SimulatorRevisionStale), SimulatorNoHandoff ->
            false
        | _ -> true

    let private exactContextsOverlap left right =
        if left = right || isSharedPopupContext left || isSharedPopupContext right then
            true
        elif isEditorContext left && isEditorContext right then
            let leftCategory = editorContextCategory left
            let rightCategory = editorContextCategory right
            leftCategory <> rightCategory
        elif isSimulatorContext left && isSimulatorContext right then
            simulatorContextsCanCoexist left right
        else
            false

    let private selectorMatches contexts = function
        | AnyEditorContext -> contexts |> List.exists isEditorContext
        | AnySimulatorContext -> contexts |> List.exists isSimulatorContext
        | ExactContext expected -> contexts |> List.contains expected

    let private selectorsOverlap left right =
        match left, right with
        | AnyEditorContext, AnyEditorContext
        | AnySimulatorContext, AnySimulatorContext -> true
        | AnyEditorContext, ExactContext context
        | ExactContext context, AnyEditorContext ->
            isEditorContext context || isSharedPopupContext context
        | AnySimulatorContext, ExactContext context
        | ExactContext context, AnySimulatorContext ->
            isSimulatorContext context || isSharedPopupContext context
        | ExactContext leftContext, ExactContext rightContext ->
            exactContextsOverlap leftContext rightContext
        | AnyEditorContext, AnySimulatorContext
        | AnySimulatorContext, AnyEditorContext -> false

    let private sameGesture left right =
        NormalizedKey.sameProducedKey left.Key right.Key
        && left.Modifiers = right.Modifiers
        && left.Phase = right.Phase

    let private gestureMatches actual expected =
        sameGesture actual expected

    let private toPossible
        (contexts: ModalContext list)
        (binding: ModalBinding<'command>)
        : PossibleInput<'command> =
        { Id = binding.Id
          InputGesture = binding.BindingGesture
          Label = binding.Label
          Group = binding.Group
          Availability = binding.Availability contexts
          Command = binding.Command }

    let deriveEditorContexts facts =
        let gestureContexts =
            match facts.Editor.Gesture with
            | IdleGesture -> []
            | BoxSelectionGesture _ -> [ EditorGesture BoxSelection ]
            | CommandPreviewGesture _ -> [ EditorGesture CommandPreview ]
            | UnitMoveGesture _ -> [ EditorGesture UnitMovePreview ]
            | TerrainGesture(tool, _, _, _) ->
                [ EditorGesture(TerrainPreview tool) ]
            | EdgePolylineGesture _ -> [ EditorGesture EdgePolyline ]

        [ yield EditorBase
          yield EditorDomain facts.ActiveDomain
          yield EditorTool facts.Editor.Tool
          yield! gestureContexts

          if facts.PanHeld then
              yield EditorPanHeld

          if facts.Editor.PendingDestructiveChange.IsSome then
              yield EditorDestructiveConfirmation

          if facts.InputHelpExpanded then
              yield InputHelpPopup ]

    let deriveSimulatorContexts facts =
        if not facts.SimulatorHandoffPresent then
            [ SimulatorBase
              SimulatorNoHandoff

              if facts.InputHelpExpanded then
                  InputHelpPopup ]
        else
            [ yield SimulatorBase
              yield if facts.SimulatorIsRunning then SimulatorRunning else SimulatorPaused

              if facts.SimulatorHasRoutePreview then
                  yield SimulatorRoutePreview

              if facts.SimulatorRevisionIsStale then
                  yield SimulatorRevisionStale

              if facts.InputHelpExpanded then
                  yield InputHelpPopup ]

    /// Selects the available binding at the highest precedence. Catalog order
    /// never decides a tie: stable IDs provide deterministic behavior even
    /// while an invalid catalog is being diagnosed.
    let resolve
        (contexts: ModalContext list)
        (gesture: InputGesture)
        isRepeat
        (catalog: ModalBinding<'command> list)
        =
        let matching =
            catalog
            |> List.filter (fun binding ->
                selectorMatches contexts binding.Context
                && gestureMatches gesture binding.BindingGesture)
            |> List.sortBy (fun binding ->
                -precedenceRank binding.Precedence, binding.Id)

        match matching with
        | [] -> NoMatch
        | highest :: _ ->
            let highestRank = precedenceRank highest.Precedence

            let owningBindings =
                matching
                |> List.takeWhile (fun binding ->
                    precedenceRank binding.Precedence = highestRank)

            if
                isRepeat
                && owningBindings
                   |> List.forall (fun binding -> binding.Repeat = IgnoreRepeat)
            then
                NoMatch
            else
                let possible =
                    owningBindings
                    |> List.filter (fun binding ->
                        not isRepeat || binding.Repeat = AllowRepeat)
                    |> List.map (toPossible contexts)

                match
                    possible
                    |> List.tryFind (fun input ->
                        input.Availability = Available)
                with
                | Some input -> Resolved input
                | None -> NoAvailableMatch possible

    let possibleInputs
        (contexts: ModalContext list)
        (catalog: ModalBinding<'command> list)
        =
        catalog
        |> List.filter (fun binding -> selectorMatches contexts binding.Context)
        |> List.map (toPossible contexts)
        |> List.filter (fun input -> input.Availability = Available)
        |> List.filter (fun input ->
            match resolve contexts input.InputGesture false catalog with
            | Resolved resolved -> resolved.Id = input.Id
            | NoMatch
            | NoAvailableMatch _ -> false)
        |> List.sortBy (fun input -> input.Group, input.Label, input.Id)

    let private key value modifiers phase =
        { Key = NormalizedKey.create value None
          Modifiers = modifiers
          Phase = phase }

    let private plain = KeyModifiers.none
    let private control shift =
        { KeyModifiers.none with
            ControlOrMeta = true
            Shift = shift }

    let private available _ = Available

    let private binding id context precedence gesture label group repeat availability command =
        { Id = id
          Context = context
          Precedence = precedence
          BindingGesture = gesture
          Label = label
          Group = group
          Repeat = repeat
          Availability = availability
          Command = command }

    let editorCatalog facts =
        let selectionAvailable _ =
            if facts.Editor.SelectedUnits.IsEmpty && facts.Editor.SelectedRegion.IsNone then
                Unavailable "Nothing is selected."
            else
                Available

        let unitSelectionAvailable _ =
            if facts.Editor.SelectedUnits.IsEmpty then
                Unavailable "No units are selected."
            else
                Available

        let historyAvailable history reason _ =
            if List.isEmpty history then Unavailable reason else Available

        let clipboardAvailable _ =
            if facts.Editor.Clipboard.IsSome then Available
            else Unavailable "The editor clipboard is empty."

        let selectableDomainAvailable _ =
            if facts.Editor.Map.Units.IsEmpty then
                Unavailable "The active domain contains no selectable objects."
            else
                Available

        let validationAvailable _ =
            if Array.isEmpty facts.Editor.Issues then
                Unavailable "The map has no validation issues."
            else
                Available

        let editorKey id value modifiers label group repeat availability command =
            binding id AnyEditorContext WorkspaceCommands
                (key value modifiers KeyDown) label group repeat availability command

        [ yield editorKey "editor.help.toggle" "?" { plain with Shift = true }
              "Show or hide possible inputs" "Help" IgnoreRepeat available ToggleInputHelp
          yield editorKey "editor.panel.toggle" "F2" plain
              "Show or hide the active command panel" "Panels" IgnoreRepeat available ToggleEditorCommandPanel
          yield editorKey "editor.inspector.toggle" "F3" plain
              "Show or hide the selected-object inspector" "Panels" IgnoreRepeat available
              (EditorWorkspaceCommand ToggleEditorInspector)
          yield editorKey "editor.history.undo" "z" (control false)
              "Undo" "Edit" IgnoreRepeat
              (historyAvailable facts.Editor.UndoHistory "There is nothing to undo.")
              (EditorCommand UndoEditorCommand)
          yield editorKey "editor.history.redo-shift-z" "z" (control true)
              "Redo" "Edit" IgnoreRepeat
              (historyAvailable facts.Editor.RedoHistory "There is nothing to redo.")
              (EditorCommand RedoEditorCommand)
          yield editorKey "editor.history.redo-y" "y" (control false)
              "Redo" "Edit" IgnoreRepeat
              (historyAvailable facts.Editor.RedoHistory "There is nothing to redo.")
              (EditorCommand RedoEditorCommand)
          yield editorKey "editor.selection.copy" "c" (control false)
              "Copy selected units" "Edit" IgnoreRepeat unitSelectionAvailable
              (EditorCommand CopyEditorSelection)
          yield editorKey "editor.selection.paste" "v" (control false)
              "Paste the editor clipboard" "Edit" IgnoreRepeat clipboardAvailable
              (EditorCommand PasteEditorClipboard)
          yield editorKey "editor.selection.duplicate" "d" (control false)
              "Duplicate selected units" "Edit" IgnoreRepeat unitSelectionAvailable
              (EditorCommand DuplicateEditorSelection)
          yield editorKey "editor.selection.all" "a" (control false)
              "Select all in the active domain" "Selection" IgnoreRepeat selectableDomainAvailable
              (EditorCommand SelectAllInActiveDomain)
          yield editorKey "editor.selection.delete" "Delete" plain
              "Delete the selection" "Selection" IgnoreRepeat selectionAvailable
              (EditorCommand DeleteEditorSelection)
          yield editorKey "editor.selection.delete-backspace" "Backspace" plain
              "Delete the selection" "Selection" IgnoreRepeat selectionAvailable
              (EditorCommand DeleteEditorSelection)
          yield editorKey "editor.camera.fit" "0" plain
              "Fit the complete map" "View" IgnoreRepeat available
              (EditorWorkspaceCommand FitEditorBoard)
          yield editorKey "editor.camera.reset" "1" plain
              "Reset the camera to 100%" "View" IgnoreRepeat available
              (EditorWorkspaceCommand ResetEditorCamera)
          yield editorKey "editor.camera.frame-selection" "f" plain
              "Frame the selection" "View" IgnoreRepeat selectionAvailable
              (EditorWorkspaceCommand FrameEditorSelection)
          yield editorKey "editor.mode.select" "v" plain
              "Enter Select" "Modes" IgnoreRepeat available
              (EditorCommand(ChooseTool Select))
          yield editorKey "editor.domain.terrain" "t" plain
              "Open Terrain commands" "Modes" IgnoreRepeat available
              (ChooseEditorDomain TerrainDomain)
          yield editorKey "editor.domain.units" "u" plain
              "Open Unit commands" "Modes" IgnoreRepeat available
              (ChooseEditorDomain UnitDomain)
          yield editorKey "editor.domain.edges" "e" plain
              "Open Edge commands" "Modes" IgnoreRepeat available
              (ChooseEditorDomain EdgeDomain)
          yield editorKey "editor.domain.zones" "z" plain
              "Open Zone commands" "Modes" IgnoreRepeat available
              (ChooseEditorDomain RegionDomain)
          yield editorKey "editor.domain.document" "m" plain
              "Open Document commands" "Modes" IgnoreRepeat available
              (ChooseEditorDomain DocumentDomain)
          yield editorKey "editor.tool.terrain.pencil" "p" plain
              "Choose Pencil" "Terrain tools" IgnoreRepeat available
              (EditorCommand(ChooseTool(Terrain PencilTool)))
          yield editorKey "editor.tool.terrain.rectangle" "r" plain
              "Choose Rectangle" "Terrain tools" IgnoreRepeat available
              (EditorCommand(ChooseTool(Terrain RectangleTool)))
          yield editorKey "editor.tool.terrain.line" "l" plain
              "Choose Line" "Terrain tools" IgnoreRepeat available
              (EditorCommand(ChooseTool(Terrain LineTool)))
          yield editorKey "editor.tool.terrain.flood-fill" "g" plain
              "Choose Flood fill" "Terrain tools" IgnoreRepeat available
              (EditorCommand(ChooseTool(Terrain FloodFillTool)))
          yield editorKey "editor.tool.terrain.eyedropper" "i" plain
              "Choose Eyedropper" "Terrain tools" IgnoreRepeat available
              (EditorCommand(ChooseTool(Terrain EyedropperTool)))
          yield editorKey "editor.tool.terrain.erase" "x" plain
              "Choose Eraser" "Terrain tools" IgnoreRepeat available
              (EditorCommand(ChooseTool(Terrain EraseTool)))
          yield editorKey "editor.validation.previous" "[" plain
              "Select the previous validation issue" "Validation" AllowRepeat validationAvailable
              (EditorCommand SelectPreviousIssue)
          yield editorKey "editor.validation.next" "]" plain
              "Select the next validation issue" "Validation" AllowRepeat validationAvailable
              (EditorCommand SelectNextIssue)
          yield binding "editor.camera.pan-held" AnyEditorContext HeldLayer
              (key "Space" plain KeyDown) "Hold to pan the map" "View" IgnoreRepeat available
              (SetEditorPanHeld true)
          yield binding "editor.camera.pan-release" (ExactContext EditorPanHeld) HeldLayer
              (key "Space" plain KeyUp) "Release held pan" "View" IgnoreRepeat available
              (SetEditorPanHeld false)

          if facts.InputHelpExpanded then
              yield binding "editor.help.close" (ExactContext InputHelpPopup) InputPopup
                  (key "Escape" plain KeyDown) "Close possible inputs" "Help" IgnoreRepeat available
                  ToggleInputHelp
          elif facts.Editor.PendingDestructiveChange.IsSome then
              yield binding "editor.confirmation.cancel" (ExactContext EditorDestructiveConfirmation) TransientPopup
                  (key "Escape" plain KeyDown) "Cancel the pending destructive change" "Current operation"
                  IgnoreRepeat available (EditorCommand CancelDestructiveChange)
          elif facts.Editor.Gesture <> IdleGesture then
              yield binding "editor.gesture.cancel" AnyEditorContext ActiveGestureOrPreview
                  (key "Escape" plain KeyDown) "Cancel the current operation" "Current operation"
                  IgnoreRepeat available (EditorCommand CancelEditorGesture)
          else
              yield binding "editor.selection.clear" AnyEditorContext WorkspaceCommands
                  (key "Escape" plain KeyDown) "Clear the selection" "Selection"
                  IgnoreRepeat selectionAvailable (EditorCommand(SelectEditorUnit None))

          match facts.Editor.Gesture with
          | IdleGesture -> ()
          | _ ->
              yield binding "editor.gesture.commit" AnyEditorContext ActiveGestureOrPreview
                        (key "Enter" plain KeyDown) "Commit the current operation" "Current operation"
                        IgnoreRepeat available (EditorCommand CommitEditorGesture)

          match facts.Editor.Tool with
          | Terrain _ ->
              let movement id value dx dy =
                  binding id (ExactContext(EditorTool facts.Editor.Tool)) ActiveTool
                      (key value plain KeyDown) "Move the terrain cursor" "Terrain"
                      AllowRepeat available (EditorCommand(MoveTerrainCursor(dx, dy, false)))
              yield movement "editor.terrain.cursor.west" "ArrowLeft" -1 0
              yield movement "editor.terrain.cursor.east" "ArrowRight" 1 0
              yield movement "editor.terrain.cursor.north" "ArrowUp" 0 -1
              yield movement "editor.terrain.cursor.south" "ArrowDown" 0 1
              yield binding "editor.terrain.activate" (ExactContext(EditorTool facts.Editor.Tool)) ActiveTool
                  (key "Enter" plain KeyDown) "Activate at the terrain cursor" "Terrain"
                  IgnoreRepeat available (EditorCommand ActivateTerrainCursor)
          | _ -> () ]

    let simulatorCatalog
        (selectedUnitId: int32 option)
        (handoff: SimulatorHandoff option)
        =
        let simulatorKey id value label group repeat availability command =
            binding id AnySimulatorContext WorkspaceCommands
                (key value plain KeyDown) label group repeat availability command

        let hasHandoff _ =
            if handoff.IsSome then Available
            else Unavailable "Create a simulator handoff from the Editor first."

        let selectedPaused _ =
            match selectedUnitId, handoff with
            | Some _, Some simulator when not simulator.IsRunning -> Available
            | None, Some _ -> Unavailable "Select a unit first."
            | _, Some _ -> Unavailable "Route preview is unavailable while running."
            | _ -> Unavailable "Create a simulator handoff from the Editor first."

        let previewAvailable _ =
            match handoff with
            | Some simulator when simulator.PreviewDestination.IsSome && not simulator.IsRunning -> Available
            | Some simulator when simulator.IsRunning -> Unavailable "Route preview is unavailable while running."
            | Some _ -> Unavailable "No route preview is active."
            | None -> Unavailable "Create a simulator handoff from the Editor first."

        [ yield binding "simulator.help.toggle" AnySimulatorContext WorkspaceCommands
              (key "?" { plain with Shift = true } KeyDown)
              "Show or hide possible inputs" "Help" IgnoreRepeat available ToggleInputHelp
          yield simulatorKey "simulator.panel.toggle" "F2" "Show or hide the active simulator panel" "Panels"
              IgnoreRepeat hasHandoff ToggleSimulatorCommandPanel
          yield simulatorKey "simulator.run.toggle-space" "Space" "Start or pause the simulator" "Simulation"
              IgnoreRepeat hasHandoff (SimulatorCommand ToggleSimulatorRun)
          yield simulatorKey "simulator.run.toggle-k" "k" "Start or pause the simulator" "Simulation"
              IgnoreRepeat hasHandoff (SimulatorCommand ToggleSimulatorRun)
          let movement id value dx dy =
              simulatorKey id value "Move the route-preview destination" "Route preview"
                  AllowRepeat selectedPaused (SimulatorCommand(MoveSimulatorPreview(dx, dy)))
          yield movement "simulator.preview.west" "ArrowLeft" -1 0
          yield movement "simulator.preview.east" "ArrowRight" 1 0
          yield movement "simulator.preview.north" "ArrowUp" 0 -1
          yield movement "simulator.preview.south" "ArrowDown" 0 1
          yield simulatorKey "simulator.preview.commit" "Enter" "Commit the route preview" "Route preview"
              IgnoreRepeat previewAvailable (SimulatorCommand CommitSimulatorPreview)

          match handoff with
          | Some simulator when simulator.PreviewDestination.IsSome ->
              yield simulatorKey "simulator.preview.cancel" "Escape" "Discard the route preview" "Route preview"
                  IgnoreRepeat previewAvailable (SimulatorCommand ResetSimulatorPreview)
          | _ -> () ]

    let private columnName column =
        let rec loop value suffix =
            let remainder = value % 26
            let letter = string (char (int 'A' + remainder))
            let next = value / 26 - 1
            if next < 0 then letter + suffix else loop next (letter + suffix)
        loop (max 0 (int column)) ""

    let private addressText address =
        columnName address.CellColumn + string (address.CellRow + 1)

    let private terrainName = function
        | Open -> "Open"
        | Rough -> "Rough"
        | Blocked -> "Blocked"
        | Objective -> "Objective"

    let private terrainToolName = function
        | PencilTool -> "Pencil"
        | RectangleTool -> "Rectangle"
        | LineTool -> "Line"
        | FloodFillTool -> "Flood fill"
        | EyedropperTool -> "Eyedropper"
        | EraseTool -> "Erase"

    let projectEditor facts catalog =
        let contexts = deriveEditorContexts facts
        let underlyingBreadcrumb, underlyingDetail =
            match facts.Editor.Gesture, facts.Editor.Tool with
            | BoxSelectionGesture(anchor, current), _ ->
                [ "Editor"; "Select"; "Box selection" ],
                "Anchor " + addressText anchor + " — current " + addressText current
            | UnitMoveGesture(_, current, original, _), _ ->
                [ "Editor"; "Units"; "Move preview" ],
                string original.Length + " units — preview at " + addressText current
            | TerrainGesture(tool, anchor, current, visited), _ ->
                [ "Editor"; "Terrain"; terrainToolName tool ],
                terrainName facts.Editor.TerrainSelection
                + " terrain — anchor " + addressText anchor
                + ", endpoint " + addressText current
                + " — " + string visited.Length + " cells"
            | EdgePolylineGesture(_, segments), _ ->
                [ "Editor"; "Edges"; "Polyline" ],
                string segments.Length + " segments staged"
            | CommandPreviewGesture _, _ ->
                [ "Editor"; "Command preview" ], "A validated editor command is ready to commit"
            | IdleGesture, Select ->
                [ "Editor"; "Select" ],
                "Cursor " + addressText facts.Editor.TerrainCursor
                + " — " + string facts.Editor.SelectedUnits.Count + " units selected"
            | IdleGesture, Terrain tool ->
                [ "Editor"; "Terrain"; terrainToolName tool ],
                terrainName facts.Editor.TerrainSelection
                + " terrain — " + string facts.Editor.BrushSize + "×" + string facts.Editor.BrushSize
                + " brush — cursor " + addressText facts.Editor.TerrainCursor
            | IdleGesture, Place(side, classId, size) ->
                [ "Editor"; "Units"; "Place" ],
                string side + " / " + classId + " — " + string size + "×" + string size
            | IdleGesture, Edge(direction, kind) ->
                [ "Editor"; "Edges" ],
                string kind + " / " + string direction
            | IdleGesture, Paint terrain ->
                [ "Editor"; "Terrain"; "Pencil" ], terrainName terrain + " terrain"

        let breadcrumb, detail =
            if facts.Editor.PendingDestructiveChange.IsSome then
                [ "Editor"; "Destructive confirmation" ],
                (match facts.Editor.PendingDestructiveChange with
                 | Some ClearPending -> "Confirm clearing the current map"
                 | Some(ResizePending preview) ->
                     "Confirm resize to " + string preview.TargetWidth + "×" + string preview.TargetHeight
                 | Some(NewMapPending(width, height, name)) ->
                     "Confirm new map " + name + " at " + string width + "×" + string height
                 | None -> underlyingDetail)
            elif facts.PanHeld then
                [ "Editor"; "Pan held" ],
                "Underlying mode: " + (underlyingBreadcrumb |> List.tail |> String.concat " / ")
            else
                underlyingBreadcrumb, underlyingDetail

        { Contexts = contexts
          Breadcrumb = breadcrumb
          Headline = breadcrumb |> List.map _.ToUpperInvariant() |> String.concat " / "
          Detail = detail
          PossibleInputs = possibleInputs contexts catalog }

    let projectSimulator facts selectedUnitId handoff catalog =
        let contexts = deriveSimulatorContexts facts
        let breadcrumb, detail =
            match handoff with
            | None ->
                [ "Simulator"; "No handoff" ],
                "Create an immutable simulator handoff from the Editor"
            | Some simulator when simulator.PreviewDestination.IsSome && not simulator.IsRunning ->
                let destination = simulator.PreviewDestination.Value
                let route = MapEditorSimulator.preview selectedUnitId destination simulator
                [ "Simulator"; "Route preview" ],
                (match route with
                 | Some preview ->
                     "Unit " + string preview.UnitId + " → " + addressText destination
                     + " — "
                     + (if preview.Collision = RouteClear then "route clear" else string preview.Collision)
                     + " — " + string preview.DistanceMillimeters + " mm"
                 | None -> "Route preview at " + addressText destination + " — select a unit")
            | Some simulator ->
                [ "Simulator"; if simulator.IsRunning then "Running" else "Paused" ],
                "Revision " + string simulator.Revision.Number
                + " — tick " + string simulator.Tick
                + (selectedUnitId
                   |> Option.map (fun id -> " — unit " + string id + " selected")
                   |> Option.defaultValue "")
                + (if facts.SimulatorRevisionIsStale then " — revision stale" else "")

        { Contexts = contexts
          Breadcrumb = breadcrumb
          Headline = breadcrumb |> List.map _.ToUpperInvariant() |> String.concat " / "
          Detail = detail
          PossibleInputs = possibleInputs contexts catalog }

    let validateCatalog (catalog: ModalBinding<'command> list) =
        let duplicates =
            catalog
            |> List.groupBy (fun binding -> binding.Id)
            |> List.choose (fun (id, bindings) ->
                if List.length bindings > 1 then
                    Some(DuplicateBindingId id)
                else
                    None)

        let conflicts =
            catalog
            |> List.indexed
            |> List.collect (fun (index, (first: ModalBinding<'command>)) ->
                catalog
                |> List.skip (index + 1)
                |> List.choose (fun (second: ModalBinding<'command>) ->
                    if
                        first.Precedence = second.Precedence
                        && sameGesture first.BindingGesture second.BindingGesture
                        && selectorsOverlap first.Context second.Context
                    then
                        let firstId, secondId =
                            if first.Id <= second.Id then
                                first.Id, second.Id
                            else
                                second.Id, first.Id

                        Some(
                            EqualPrecedenceGestureConflict(
                                firstId,
                                secondId,
                                first.Precedence,
                                first.BindingGesture
                            )
                        )
                    else
                        None))
            |> List.distinct
            |> List.sortBy (function
                | EqualPrecedenceGestureConflict(firstId, secondId, _, _) ->
                    firstId, secondId
                | DuplicateBindingId id -> id, "")

        duplicates @ conflicts
        |> List.sortBy (function
            | DuplicateBindingId id -> 0, id, ""
            | EqualPrecedenceGestureConflict(firstId, secondId, _, _) ->
                1, firstId, secondId)
