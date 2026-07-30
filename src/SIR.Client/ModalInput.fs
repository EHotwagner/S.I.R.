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

type ModalInputTarget =
    | InputElement
    | TextAreaElement
    | SelectElement
    | ContentEditableElement
    | ApplicationElement

type RepeatPolicy =
    | IgnoreRepeat
    | AllowRepeat

type InputGesture =
    { Key: NormalizedKey
      Modifiers: KeyModifiers
      Phase: InputPhase }

type EditorGestureKind =
    | SelectedObjectActions
    | BoxSelection
    | CommandPreview
    | UnitMovePreview
    | TerrainPreview of TerrainAuthoringTool
    | EdgePolyline
    | RegionPurpose
    | RegionShape
    | RegionRectangleMode
    | RegionPolygonMode
    | RegionMove
    | RegionResize
    | RegionVertex

type EditorDocumentControl =
    | MapImportControl
    | LayerStateControls
    | LocalBackgroundControls
    | MapDimensionControls
    | SavedViewControls

type EditorDocumentCommand =
    | ExportMapDocument
    | OpenMapImport
    | ExportRepositoryDesignBundle
    | FocusDocumentControl of EditorDocumentControl

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
    | SimulatorControllerSelection
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
    | TraverseSimulatorUnit of delta: int
    | BeginSimulatorControllerSelection
    | ChooseSimulatorController of MapController
    | CommitSimulatorController
    | CancelSimulatorController
    | RequestSimulatorSandboxReset
    | SetEditorPanHeld of bool
    | FocusUnitPresetSearch
    | EditorDocumentCommand of EditorDocumentCommand
    | ToggleInputHelp

type HeldInput =
    | EditorPan

type HeldInputSession =
    private
    | HeldInputSession of Set<HeldInput>

[<RequireQualifiedAccess>]
module HeldInputSession =
    let empty = HeldInputSession Set.empty

    let contains input (HeldInputSession inputs) =
        Set.contains input inputs

    let apply command (HeldInputSession inputs) =
        match command with
        | SetEditorPanHeld true ->
            HeldInputSession(Set.add EditorPan inputs)
        | SetEditorPanHeld false ->
            HeldInputSession(Set.remove EditorPan inputs)
        | _ ->
            HeldInputSession inputs

    let recover (_: HeldInputSession) = empty

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
      SimulatorControllerSelection: MapController option
      SimulatorRevisionIsStale: bool
      InputHelpExpanded: bool }

[<RequireQualifiedAccess>]
module ModalInput =
    let acceptsTarget target =
        match target with
        | InputElement
        | TextAreaElement
        | SelectElement
        | ContentEditableElement -> false
        | ApplicationElement -> true

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
        | SimulatorControllerSelection
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
        | SimulatorRoutePreview, SimulatorControllerSelection
        | SimulatorControllerSelection, SimulatorRoutePreview -> false
        | SimulatorNoHandoff, (SimulatorPaused | SimulatorRunning | SimulatorRoutePreview | SimulatorControllerSelection | SimulatorRevisionStale)
        | (SimulatorPaused | SimulatorRunning | SimulatorRoutePreview | SimulatorControllerSelection | SimulatorRevisionStale), SimulatorNoHandoff ->
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
            | SelectedObjectActionsGesture -> [ EditorGesture SelectedObjectActions ]
            | BoxSelectionGesture _ -> [ EditorGesture BoxSelection ]
            | CommandPreviewGesture _ -> [ EditorGesture CommandPreview ]
            | UnitMoveGesture _ -> [ EditorGesture UnitMovePreview ]
            | TerrainGesture(tool, _, _, _) ->
                [ EditorGesture(TerrainPreview tool) ]
            | EdgePolylineGesture _ -> [ EditorGesture EdgePolyline ]

        let regionContexts =
            match facts.Editor.RegionKeyboardMode with
            | RegionIdle -> []
            | RegionPurposeSelection _ -> [ EditorGesture RegionPurpose ]
            | RegionShapeSelection _ -> [ EditorGesture RegionShape ]
            | RegionRectangleConstruction _ -> [ EditorGesture RegionRectangleMode ]
            | RegionPolygonConstruction _ -> [ EditorGesture RegionPolygonMode ]
            | RegionMovePreview _ -> [ EditorGesture RegionMove ]
            | RegionResizePreview _ -> [ EditorGesture RegionResize ]
            | RegionVertexPreview _ -> [ EditorGesture RegionVertex ]

        [ yield EditorBase
          yield EditorDomain facts.ActiveDomain
          yield EditorTool facts.Editor.Tool
          yield! gestureContexts
          yield! regionContexts

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

              if facts.SimulatorControllerSelection.IsSome then
                  yield SimulatorControllerSelection

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
                && gestureMatches gesture binding.BindingGesture
                && (not (List.contains EditorPanHeld contexts)
                    || precedenceRank binding.Precedence >= precedenceRank HeldLayer))
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

        let selectedRegionAvailable _ =
            if facts.Editor.SelectedRegion.IsSome then Available
            else Unavailable "Select a region first."

        let selectedRectangleAvailable _ =
            match
                facts.Editor.SelectedRegion
                |> Option.bind (fun id -> Map.tryFind id facts.Editor.Map.Regions)
            with
            | Some { Geometry = RegionRectangle _ } -> Available
            | Some _ -> Unavailable "The selected region is not a rectangle."
            | None -> Unavailable "Select a region first."

        let selectedPolygonAvailable _ =
            match
                facts.Editor.SelectedRegion
                |> Option.bind (fun id -> Map.tryFind id facts.Editor.Map.Regions)
            with
            | Some { Geometry = RegionPolygon _ } -> Available
            | Some _ -> Unavailable "The selected region is not a polygon."
            | None -> Unavailable "Select a region first."

        let editorKey id value modifiers label group repeat availability command =
            binding id AnyEditorContext WorkspaceCommands
                (key value modifiers KeyDown) label group repeat availability command

        let catalog =
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

              if facts.PanHeld then
                  let pan id value modifiers x y =
                      binding id (ExactContext EditorPanHeld) HeldLayer
                          (key value modifiers KeyDown) "Pan the map" "View"
                          AllowRepeat available (EditorWorkspaceCommand(PanEditorBy(x, y)))
                  yield pan "editor.camera.pan-west" "ArrowLeft" plain 40.0 0.0
                  yield pan "editor.camera.pan-east" "ArrowRight" plain -40.0 0.0
                  yield pan "editor.camera.pan-north" "ArrowUp" plain 0.0 40.0
                  yield pan "editor.camera.pan-south" "ArrowDown" plain 0.0 -40.0
                  yield pan "editor.camera.pan-west-large" "ArrowLeft" { plain with Shift = true } 120.0 0.0
                  yield pan "editor.camera.pan-east-large" "ArrowRight" { plain with Shift = true } -120.0 0.0
                  yield pan "editor.camera.pan-north-large" "ArrowUp" { plain with Shift = true } 0.0 120.0
                  yield pan "editor.camera.pan-south-large" "ArrowDown" { plain with Shift = true } 0.0 -120.0
                  yield binding "editor.camera.pan-cancel" (ExactContext EditorPanHeld) HeldLayer
                      (key "Escape" plain KeyDown) "Release held pan" "View"
                      IgnoreRepeat available (SetEditorPanHeld false)

              if facts.InputHelpExpanded then
                  yield binding "editor.help.close" (ExactContext InputHelpPopup) InputPopup
                      (key "Escape" plain KeyDown) "Close possible inputs" "Help" IgnoreRepeat available
                      ToggleInputHelp
              elif facts.Editor.PendingDestructiveChange.IsSome then
                  yield binding "editor.confirmation.confirm" (ExactContext EditorDestructiveConfirmation) TransientPopup
                      (key "Enter" plain KeyDown) "Confirm the pending destructive change" "Current operation"
                      IgnoreRepeat available (EditorCommand ConfirmDestructiveChange)
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
              | IdleGesture
              | SelectedObjectActionsGesture -> ()
              | _ ->
                  yield binding "editor.gesture.commit" AnyEditorContext ActiveGestureOrPreview
                            (key "Enter" plain KeyDown) "Commit the current operation" "Current operation"
                            IgnoreRepeat available (EditorCommand CommitEditorGesture)

              match facts.Editor.Gesture with
              | SelectedObjectActionsGesture ->
                  yield binding "editor.selection.actions.copy"
                      (ExactContext(EditorGesture SelectedObjectActions)) ActiveGestureOrPreview
                      (key "c" plain KeyDown) "Copy selected units" "Selected-object actions"
                      IgnoreRepeat unitSelectionAvailable (EditorCommand CopyEditorSelection)
                  yield binding "editor.selection.actions.duplicate"
                      (ExactContext(EditorGesture SelectedObjectActions)) ActiveGestureOrPreview
                      (key "d" plain KeyDown) "Duplicate selected units" "Selected-object actions"
                      IgnoreRepeat unitSelectionAvailable (EditorCommand DuplicateEditorSelection)
                  yield binding "editor.selection.actions.delete"
                      (ExactContext(EditorGesture SelectedObjectActions)) ActiveGestureOrPreview
                      (key "Delete" plain KeyDown) "Delete the selection" "Selected-object actions"
                      IgnoreRepeat selectionAvailable (EditorCommand DeleteEditorSelection)
                  yield binding "editor.selection.actions.inspector"
                      (ExactContext(EditorGesture SelectedObjectActions)) ActiveGestureOrPreview
                      (key "i" plain KeyDown) "Open selected-object inspector" "Selected-object actions"
                      IgnoreRepeat available (EditorWorkspaceCommand ToggleEditorInspector)
                  yield binding "editor.selection.actions.move"
                      (ExactContext(EditorGesture SelectedObjectActions)) ActiveGestureOrPreview
                      (key "m" plain KeyDown) "Move selected units" "Selected-object actions"
                      IgnoreRepeat unitSelectionAvailable
                      (EditorCommand(BeginUnitMove facts.Editor.KeyboardCursor.Cell))
              | BoxSelectionGesture(anchor, current) ->
                  let clamp minimum maximum value =
                      max minimum (min maximum value)
                  let move id value modifiers dx dy =
                      binding id (ExactContext(EditorGesture BoxSelection)) ActiveGestureOrPreview
                          (key value modifiers KeyDown) "Move the box corner" "Selection"
                          AllowRepeat available
                          (EditorCommand(
                              ExtendEditorBoxSelection
                                  { CellColumn =
                                      clamp 0 (facts.Editor.Map.Width - 1) (current.CellColumn + dx)
                                    CellRow =
                                      clamp 0 (facts.Editor.Map.Height - 1) (current.CellRow + dy) }
                          ))
                  for modifiers, suffix in [ plain, ""; { plain with Shift = true }, "-extended" ] do
                      yield move ("editor.selection.box.west" + suffix) "ArrowLeft" modifiers -1 0
                      yield move ("editor.selection.box.east" + suffix) "ArrowRight" modifiers 1 0
                      yield move ("editor.selection.box.north" + suffix) "ArrowUp" modifiers 0 -1
                      yield move ("editor.selection.box.south" + suffix) "ArrowDown" modifiers 0 1
                  yield binding "editor.selection.box.add" (ExactContext(EditorGesture BoxSelection))
                      ActiveGestureOrPreview (key "Enter" { plain with Shift = true } KeyDown)
                      "Add enclosed units to the selection" "Selection" IgnoreRepeat available
                      (EditorCommand(
                          AddEditorUnitsInBox
                              { FirstColumn = anchor.CellColumn
                                FirstRow = anchor.CellRow
                                LastColumn = current.CellColumn
                                LastRow = current.CellRow }
                      ))
              | TerrainGesture(_, _, _, _) ->
                  let extend id value modifiers dx dy =
                      binding id (ExactContext(EditorGesture(
                          match facts.Editor.Gesture with
                          | TerrainGesture(tool, _, _, _) -> TerrainPreview tool
                          | _ -> TerrainPreview PencilTool
                      ))) ActiveGestureOrPreview
                          (key value modifiers KeyDown) "Move the terrain endpoint" "Terrain"
                          AllowRepeat available (EditorCommand(MoveTerrainCursor(dx, dy, true)))
                  for modifiers, suffix in [ plain, ""; { plain with Shift = true }, "-extended" ] do
                      yield extend ("editor.terrain.gesture.west" + suffix) "ArrowLeft" modifiers -1 0
                      yield extend ("editor.terrain.gesture.east" + suffix) "ArrowRight" modifiers 1 0
                      yield extend ("editor.terrain.gesture.north" + suffix) "ArrowUp" modifiers 0 -1
                      yield extend ("editor.terrain.gesture.south" + suffix) "ArrowDown" modifiers 0 1
                  yield binding "editor.terrain.gesture.reset"
                      (ExactContext(
                          match facts.Editor.Gesture with
                          | TerrainGesture(tool, _, _, _) -> EditorGesture(TerrainPreview tool)
                          | _ -> EditorGesture(TerrainPreview PencilTool)
                      )) ActiveGestureOrPreview
                      (key "Backspace" plain KeyDown) "Reset endpoint to anchor" "Terrain"
                      IgnoreRepeat available (EditorCommand ResetTerrainPreview)
              | UnitMoveGesture(_, current, _, _) ->
                  let extend id value modifiers distance dx dy =
                      binding id (ExactContext(EditorGesture UnitMovePreview)) ActiveGestureOrPreview
                          (key value modifiers KeyDown) "Move the formation preview" "Units"
                          AllowRepeat available
                          (EditorCommand(
                              ExtendUnitMove
                                  { CellColumn =
                                      max 0 (min (facts.Editor.Map.Width - 1) (current.CellColumn + dx * distance))
                                    CellRow =
                                      max 0 (min (facts.Editor.Map.Height - 1) (current.CellRow + dy * distance)) }
                          ))
                  for modifiers, distance, suffix in
                      [ plain, 1, ""; { plain with Shift = true }, 5, "-large" ] do
                      yield extend ("editor.unit.move.west" + suffix) "ArrowLeft" modifiers distance -1 0
                      yield extend ("editor.unit.move.east" + suffix) "ArrowRight" modifiers distance 1 0
                      yield extend ("editor.unit.move.north" + suffix) "ArrowUp" modifiers distance 0 -1
                      yield extend ("editor.unit.move.south" + suffix) "ArrowDown" modifiers distance 0 1
                  yield binding "editor.unit.move.reset"
                      (ExactContext(EditorGesture UnitMovePreview)) ActiveGestureOrPreview
                      (key "Backspace" plain KeyDown) "Reset movement preview" "Units"
                      IgnoreRepeat available (EditorCommand ResetUnitMovePreview)
              | _ -> ()

              if facts.ActiveDomain = EdgeDomain then
                  let edgeColumn, edgeRow, edgeDirection = facts.Editor.EdgeCursor
                  let edgeTool =
                      match facts.Editor.Tool with
                      | Edge(direction, kind) -> direction, kind
                      | _ -> edgeDirection, Wall
                  let direction, kind = edgeTool
                  let edgeBinding id value modifiers label repeat command =
                      binding id (ExactContext(EditorDomain EdgeDomain)) ActiveTool
                          (key value modifiers KeyDown) label "Edges" repeat available command
                  yield edgeBinding "editor.edge.kind.wall" "w" plain "Choose wall" IgnoreRepeat
                      (EditorCommand(ConvertEdge(edgeColumn, edgeRow, edgeDirection, Wall)))
                  yield edgeBinding "editor.edge.kind.door" "d" plain "Choose closed door" IgnoreRepeat
                      (EditorCommand(ConvertEdge(edgeColumn, edgeRow, edgeDirection, Door)))
                  yield edgeBinding "editor.edge.kind.window" "n" plain "Choose window" IgnoreRepeat
                      (EditorCommand(ConvertEdge(edgeColumn, edgeRow, edgeDirection, Window)))
                  yield edgeBinding "editor.edge.orientation.rotate" "r" plain "Rotate edge orientation" IgnoreRepeat
                      (EditorCommand(
                          ChooseTool(
                              Edge(
                                  (if direction = EastEdge then SouthEdge else EastEdge),
                                  kind
                              )
                          )
                      ))
                  let edgeMove id value modifiers dx dy extend =
                      edgeBinding id value modifiers
                          (if extend then "Extend the wall polyline" else "Move the snapped edge cursor")
                          AllowRepeat
                          (EditorCommand(MoveEdgeCursor(dx, dy, extend)))
                  yield edgeMove "editor.edge.cursor.west" "ArrowLeft" plain -1 0 false
                  yield edgeMove "editor.edge.cursor.east" "ArrowRight" plain 1 0 false
                  yield edgeMove "editor.edge.cursor.north" "ArrowUp" plain 0 -1 false
                  yield edgeMove "editor.edge.cursor.south" "ArrowDown" plain 0 1 false
                  yield edgeMove "editor.edge.polyline.west" "ArrowLeft" { plain with Shift = true } -1 0 true
                  yield edgeMove "editor.edge.polyline.east" "ArrowRight" { plain with Shift = true } 1 0 true
                  yield edgeMove "editor.edge.polyline.north" "ArrowUp" { plain with Shift = true } 0 -1 true
                  yield edgeMove "editor.edge.polyline.south" "ArrowDown" { plain with Shift = true } 0 1 true
                  if facts.Editor.Gesture = IdleGesture then
                      yield edgeBinding "editor.edge.activate" "Enter" plain
                          "Apply the selected edge or begin a wall polyline" IgnoreRepeat
                          (EditorCommand ActivateEdgeCursor)
                  else
                      match facts.Editor.Gesture with
                      | EdgePolylineGesture _ ->
                          yield binding "editor.edge.polyline.backtrack"
                              (ExactContext(EditorGesture EdgePolyline)) ActiveGestureOrPreview
                              (key "Backspace" plain KeyDown) "Remove the last polyline segment"
                              "Edges" IgnoreRepeat available (EditorCommand BacktrackEdgePolyline)
                      | _ -> ()
                  yield edgeBinding "editor.edge.door.toggle" "o" plain "Toggle door open or closed" IgnoreRepeat
                      (EditorCommand(ToggleDoorState(edgeColumn, edgeRow, edgeDirection)))
                  yield edgeBinding "editor.edge.erase" "x" plain "Erase the cursor edge" IgnoreRepeat
                      (EditorCommand(EraseEdge(edgeColumn, edgeRow, edgeDirection)))
                  yield edgeBinding "editor.edge.split" "s" plain "Split the edge run" IgnoreRepeat
                      (EditorCommand(SplitEdge(edgeColumn, edgeRow, edgeDirection)))
                  yield edgeBinding "editor.edge.join" "j" plain "Join a compatible edge run" IgnoreRepeat
                      (EditorCommand(JoinEdge(edgeColumn, edgeRow, edgeDirection)))

              if facts.ActiveDomain = RegionDomain then
                  let regionBinding id context value modifiers label repeat availability command =
                      binding id context ActiveGestureOrPreview
                          (key value modifiers KeyDown) label "Zones" repeat availability command
                  let cursorMove id context value modifiers dx dy =
                      regionBinding id context value modifiers "Move the region cursor"
                          AllowRepeat available (EditorCommand(MoveRegionCursor(dx, dy)))
                  let previewMove id context value modifiers dx dy opposite =
                      let distance = if modifiers.Shift && not opposite then 5 else 1
                      regionBinding id context value modifiers "Update the region preview"
                          AllowRepeat available
                          (EditorCommand(MoveRegionEditPreview(dx * distance, dy * distance, opposite)))

                  match facts.Editor.RegionKeyboardMode with
                  | RegionIdle ->
                      let context = ExactContext(EditorDomain RegionDomain)
                      let idle id value label repeat availability command =
                          binding id context ActiveGestureOrPreview (key value plain KeyDown)
                              label "Zones" repeat availability command
                      for id, value, dx, dy in
                          [ "west", "ArrowLeft", -1, 0
                            "east", "ArrowRight", 1, 0
                            "north", "ArrowUp", 0, -1
                            "south", "ArrowDown", 0, 1 ] do
                          yield binding ("editor.region.cursor." + id) context ActiveGestureOrPreview
                              (key value plain KeyDown) "Move the region cursor" "Zones"
                              AllowRepeat available (EditorCommand(MoveRegionCursor(dx, dy)))
                      yield idle "editor.region.select" "Enter" "Select the region under the cursor"
                          IgnoreRepeat available (EditorCommand ActivateRegionCursor)
                      yield idle "editor.region.create.begin" "n" "Begin a new region"
                          IgnoreRepeat available (EditorCommand BeginNewRegion)
                      yield idle "editor.region.edit.move" "m" "Move the selected region"
                          IgnoreRepeat selectedRegionAvailable (EditorCommand BeginSelectedRegionMove)
                      yield idle "editor.region.edit.resize" "r" "Resize the selected rectangle"
                          IgnoreRepeat selectedRectangleAvailable (EditorCommand BeginSelectedRegionResize)
                      yield idle "editor.region.edit.vertices" "v" "Edit selected polygon vertices"
                          IgnoreRepeat selectedPolygonAvailable (EditorCommand BeginSelectedRegionVertexEdit)
                      yield idle "editor.region.edit.purpose" "p" "Change selected region purpose"
                          IgnoreRepeat selectedRegionAvailable (EditorCommand BeginSelectedRegionPurposeEdit)
                      yield idle "editor.region.delete" "Delete" "Delete the selected region"
                          IgnoreRepeat selectedRegionAvailable (EditorCommand RemoveSelectedRegion)
                      yield idle "editor.region.exit" "Escape"
                          (if facts.Editor.SelectedRegion.IsSome then "Clear the region selection" else "Return to Select")
                          IgnoreRepeat available (EditorCommand CancelRegionKeyboardMode)
                  | RegionPurposeSelection(editingExisting, _) ->
                      let context = ExactContext(EditorGesture RegionPurpose)
                      for id, value, purpose, name in
                          [ "objective", "o", ObjectiveRegion, "Objective"
                            "blue", "b", DeploymentZone Blue, "Blue deployment"
                            "red", "r", DeploymentZone Red, "Red deployment" ] do
                          yield regionBinding ("editor.region.purpose." + id) context value plain
                              ("Choose " + name) IgnoreRepeat available
                              (EditorCommand(ChooseRegionPurpose purpose))
                      if editingExisting then
                          yield regionBinding "editor.region.purpose.commit" context "Enter" plain
                              "Apply the highlighted purpose" IgnoreRepeat available
                              (EditorCommand CommitRegionEditPreview)
                      yield regionBinding "editor.region.purpose.cancel" context "Escape" plain
                          "Cancel purpose selection" IgnoreRepeat available
                          (EditorCommand CancelRegionKeyboardMode)
                  | RegionShapeSelection _ ->
                      let context = ExactContext(EditorGesture RegionShape)
                      yield regionBinding "editor.region.shape.rectangle" context "r" plain
                          "Choose rectangle geometry" IgnoreRepeat available
                          (EditorCommand(ChooseRegionShape RectangleRegionShape))
                      yield regionBinding "editor.region.shape.polygon" context "p" plain
                          "Choose polygon geometry" IgnoreRepeat available
                          (EditorCommand(ChooseRegionShape PolygonRegionShape))
                      yield regionBinding "editor.region.shape.back" context "Escape" plain
                          "Return to purpose selection" IgnoreRepeat available
                          (EditorCommand CancelRegionKeyboardMode)
                  | RegionRectangleConstruction(_, anchor) ->
                      let context = ExactContext(EditorGesture RegionRectangleMode)
                      for id, value, dx, dy in
                          [ "west", "ArrowLeft", -1, 0
                            "east", "ArrowRight", 1, 0
                            "north", "ArrowUp", 0, -1
                            "south", "ArrowDown", 0, 1 ] do
                          yield cursorMove ("editor.region.rectangle." + id) context value plain dx dy
                      yield regionBinding "editor.region.rectangle.activate" context "Enter" plain
                          (if anchor.IsSome then "Commit the rectangle" else "Set the first rectangle corner")
                          IgnoreRepeat available (EditorCommand ActivateRegionCursor)
                      yield regionBinding "editor.region.rectangle.reset" context "Backspace" plain
                          "Clear the first rectangle corner" IgnoreRepeat
                          (if anchor.IsSome then available else fun _ -> Unavailable "No rectangle corner is set.")
                          (EditorCommand BacktrackRegionConstruction)
                      yield regionBinding "editor.region.rectangle.cancel" context "Escape" plain
                          "Cancel rectangle geometry" IgnoreRepeat available
                          (EditorCommand CancelRegionKeyboardMode)
                  | RegionPolygonConstruction(_, vertices) ->
                      let context = ExactContext(EditorGesture RegionPolygonMode)
                      for id, value, dx, dy in
                          [ "west", "ArrowLeft", -1, 0
                            "east", "ArrowRight", 1, 0
                            "north", "ArrowUp", 0, -1
                            "south", "ArrowDown", 0, 1 ] do
                          yield cursorMove ("editor.region.polygon." + id) context value plain dx dy
                      yield regionBinding "editor.region.polygon.vertex" context "Enter" plain
                          "Add a polygon vertex" IgnoreRepeat available (EditorCommand ActivateRegionCursor)
                      yield regionBinding "editor.region.polygon.commit" context "Enter" { plain with Shift = true }
                          "Close and commit the polygon" IgnoreRepeat
                          (if vertices.Length >= 3 then available else fun _ -> Unavailable "Add at least three vertices.")
                          (EditorCommand CommitRegionPolygon)
                      yield regionBinding "editor.region.polygon.backtrack" context "Backspace" plain
                          "Remove the last polygon vertex" IgnoreRepeat
                          (if Array.isEmpty vertices then fun _ -> Unavailable "No polygon vertex is staged." else available)
                          (EditorCommand BacktrackRegionConstruction)
                      yield regionBinding "editor.region.polygon.cancel" context "Escape" plain
                          "Cancel polygon geometry" IgnoreRepeat available
                          (EditorCommand CancelRegionKeyboardMode)
                  | RegionMovePreview _ ->
                      let context = ExactContext(EditorGesture RegionMove)
                      for modifiers, distance, suffix in
                          [ plain, 1, ""; { plain with Shift = true }, 5, "-large" ] do
                          for id, value, dx, dy in
                              [ "west", "ArrowLeft", -1, 0
                                "east", "ArrowRight", 1, 0
                                "north", "ArrowUp", 0, -1
                                "south", "ArrowDown", 0, 1 ] do
                              yield regionBinding ("editor.region.move." + id + suffix) context value modifiers
                                  "Move the region preview" AllowRepeat available
                                  (EditorCommand(MoveRegionEditPreview(dx * distance, dy * distance, false)))
                      yield regionBinding "editor.region.move.commit" context "Enter" plain "Commit the region move"
                          IgnoreRepeat available (EditorCommand CommitRegionEditPreview)
                      yield regionBinding "editor.region.move.reset" context "Backspace" plain "Reset the region move"
                          IgnoreRepeat available (EditorCommand ResetRegionEditPreview)
                      yield regionBinding "editor.region.move.cancel" context "Escape" plain "Cancel the region move"
                          IgnoreRepeat available (EditorCommand CancelRegionKeyboardMode)
                  | RegionResizePreview _ ->
                      let context = ExactContext(EditorGesture RegionResize)
                      yield previewMove "editor.region.resize.width.decrease" context "ArrowLeft" plain -1 0 false
                      yield previewMove "editor.region.resize.width.increase" context "ArrowRight" plain 1 0 false
                      yield previewMove "editor.region.resize.height.decrease" context "ArrowUp" plain 0 -1 false
                      yield previewMove "editor.region.resize.height.increase" context "ArrowDown" plain 0 1 false
                      yield previewMove "editor.region.resize.origin.east" context "ArrowLeft" { plain with Shift = true } 1 0 true
                      yield previewMove "editor.region.resize.origin.west" context "ArrowRight" { plain with Shift = true } -1 0 true
                      yield previewMove "editor.region.resize.origin.south" context "ArrowUp" { plain with Shift = true } 0 1 true
                      yield previewMove "editor.region.resize.origin.north" context "ArrowDown" { plain with Shift = true } 0 -1 true
                      yield regionBinding "editor.region.resize.commit" context "Enter" plain "Commit the rectangle resize"
                          IgnoreRepeat available (EditorCommand CommitRegionEditPreview)
                      yield regionBinding "editor.region.resize.reset" context "Backspace" plain "Reset the rectangle resize"
                          IgnoreRepeat available (EditorCommand ResetRegionEditPreview)
                      yield regionBinding "editor.region.resize.cancel" context "Escape" plain "Cancel the rectangle resize"
                          IgnoreRepeat available (EditorCommand CancelRegionKeyboardMode)
                  | RegionVertexPreview _ ->
                      let context = ExactContext(EditorGesture RegionVertex)
                      yield regionBinding "editor.region.vertex.previous" context "[" plain "Previous polygon vertex"
                          AllowRepeat available (EditorCommand(CycleRegionVertex -1))
                      yield regionBinding "editor.region.vertex.next" context "]" plain "Next polygon vertex"
                          AllowRepeat available (EditorCommand(CycleRegionVertex 1))
                      for modifiers, distance, suffix in
                          [ plain, 1, ""; { plain with Shift = true }, 5, "-large" ] do
                          for id, value, dx, dy in
                              [ "west", "ArrowLeft", -1, 0
                                "east", "ArrowRight", 1, 0
                                "north", "ArrowUp", 0, -1
                                "south", "ArrowDown", 0, 1 ] do
                              yield regionBinding ("editor.region.vertex." + id + suffix) context value modifiers
                                  "Move the active polygon vertex" AllowRepeat available
                                  (EditorCommand(MoveRegionEditPreview(dx * distance, dy * distance, false)))
                      yield regionBinding "editor.region.vertex.commit" context "Enter" plain "Commit polygon vertex edits"
                          IgnoreRepeat available (EditorCommand CommitRegionEditPreview)
                      yield regionBinding "editor.region.vertex.reset" context "Backspace" plain "Reset the active polygon vertex"
                          IgnoreRepeat available (EditorCommand ResetRegionEditPreview)
                      yield regionBinding "editor.region.vertex.cancel" context "Escape" plain "Cancel polygon vertex edits"
                          IgnoreRepeat available (EditorCommand CancelRegionKeyboardMode)

              if facts.ActiveDomain = DocumentDomain then
                  let document id value label command =
                      binding id (ExactContext(EditorDomain DocumentDomain)) ActiveGestureOrPreview
                          (key value plain KeyDown) label "Document" IgnoreRepeat available command
                  yield document "editor.document.new" "n" "Request a new map"
                      (EditorCommand RequestNewMap)
                  yield document "editor.document.clear" "c" "Request clearing the map"
                      (EditorCommand RequestClearMap)
                  yield document "editor.document.export" "s" "Save or export the canonical map"
                      (EditorDocumentCommand ExportMapDocument)
                  yield document "editor.document.import" "i" "Open the native map import picker"
                      (EditorDocumentCommand OpenMapImport)
                  yield document "editor.document.bundle" "b" "Export the repository design bundle"
                      (EditorDocumentCommand ExportRepositoryDesignBundle)
                  yield document "editor.document.layers" "l" "Focus layer-state controls"
                      (EditorDocumentCommand(FocusDocumentControl LayerStateControls))
                  yield document "editor.document.background" "g" "Focus local background controls"
                      (EditorDocumentCommand(FocusDocumentControl LocalBackgroundControls))
                  yield document "editor.document.resize" "r" "Focus map dimensions"
                      (EditorDocumentCommand(FocusDocumentControl MapDimensionControls))
                  yield document "editor.document.views" "v" "Focus saved views"
                      (EditorDocumentCommand(FocusDocumentControl SavedViewControls))
                  yield document "editor.document.exit" "Escape" "Return to Select"
                      (EditorCommand(ChooseTool Select))

              match facts.Editor.Tool with
              | Select when facts.Editor.Gesture = IdleGesture ->
                  let movement id value dx dy =
                      binding id (ExactContext(EditorTool Select)) ActiveTool
                          (key value plain KeyDown) "Move the map cursor" "Selection"
                          AllowRepeat available (EditorCommand(MoveEditorKeyboardCursor(dx, dy)))
                  yield movement "editor.cursor.west" "ArrowLeft" -1 0
                  yield movement "editor.cursor.east" "ArrowRight" 1 0
                  yield movement "editor.cursor.north" "ArrowUp" 0 -1
                  yield movement "editor.cursor.south" "ArrowDown" 0 1
                  yield binding "editor.selection.single" (ExactContext(EditorTool Select)) ActiveTool
                      (key "Enter" plain KeyDown) "Select the current object" "Selection"
                      IgnoreRepeat available (EditorCommand(ActivateEditorKeyboardCursor false))
                  yield binding "editor.selection.toggle" (ExactContext(EditorTool Select)) ActiveTool
                      (key "Enter" { plain with Shift = true } KeyDown)
                      "Toggle the current object in the selection" "Selection"
                      IgnoreRepeat available (EditorCommand(ActivateEditorKeyboardCursor true))
                  yield binding "editor.cursor.next-object" (ExactContext(EditorTool Select)) ActiveTool
                      (key "n" plain KeyDown) "Select the next object at the cursor" "Selection"
                      IgnoreRepeat available (EditorCommand(CycleEditorKeyboardObject 1))
                  yield binding "editor.cursor.previous-object" (ExactContext(EditorTool Select)) ActiveTool
                      (key "p" plain KeyDown) "Select the previous object at the cursor" "Selection"
                      IgnoreRepeat available (EditorCommand(CycleEditorKeyboardObject -1))
                  yield binding "editor.selection.box.begin" (ExactContext(EditorTool Select)) ActiveTool
                      (key "b" plain KeyDown) "Begin box selection" "Selection"
                      IgnoreRepeat available (EditorCommand BeginKeyboardBoxSelection)
                  yield binding "editor.selection.all-domain" (ExactContext(EditorTool Select)) ActiveTool
                      (key "a" plain KeyDown) "Select all units" "Selection"
                      IgnoreRepeat selectableDomainAvailable (EditorCommand SelectAllInActiveDomain)
                  yield binding "editor.unit.move.begin" (ExactContext(EditorTool Select)) ActiveTool
                      (key "m" plain KeyDown) "Begin moving selected units" "Units"
                      IgnoreRepeat unitSelectionAvailable
                      (EditorCommand(BeginUnitMove facts.Editor.KeyboardCursor.Cell))
              | UnitBrowse ->
                  let browse id value repeat delta =
                      binding id (ExactContext(EditorTool UnitBrowse)) ActiveTool
                          (key value plain KeyDown) "Browse unit presets" "Units"
                          repeat available (EditorCommand(MoveUnitPaletteCursor delta))
                  yield browse "editor.unit.preset.previous-arrow" "ArrowUp" AllowRepeat -1
                  yield browse "editor.unit.preset.next-arrow" "ArrowDown" AllowRepeat 1
                  yield browse "editor.unit.preset.previous-bracket" "[" AllowRepeat -1
                  yield browse "editor.unit.preset.next-bracket" "]" AllowRepeat 1
                  yield binding "editor.unit.preset.previous-faction"
                      (ExactContext(EditorTool UnitBrowse)) ActiveTool
                      (key "PageUp" plain KeyDown) "Previous faction group" "Units"
                      AllowRepeat available (EditorCommand(PageUnitPaletteFaction -1))
                  yield binding "editor.unit.preset.next-faction"
                      (ExactContext(EditorTool UnitBrowse)) ActiveTool
                      (key "PageDown" plain KeyDown) "Next faction group" "Units"
                      AllowRepeat available (EditorCommand(PageUnitPaletteFaction 1))
                  yield binding "editor.unit.preset.first"
                      (ExactContext(EditorTool UnitBrowse)) ActiveTool
                      (key "Home" plain KeyDown) "First visible preset" "Units"
                      IgnoreRepeat available (EditorCommand(SelectUnitPaletteBoundary false))
                  yield binding "editor.unit.preset.last"
                      (ExactContext(EditorTool UnitBrowse)) ActiveTool
                      (key "End" plain KeyDown) "Last visible preset" "Units"
                      IgnoreRepeat available (EditorCommand(SelectUnitPaletteBoundary true))
                  yield binding "editor.unit.preset.arm"
                      (ExactContext(EditorTool UnitBrowse)) ActiveTool
                      (key "Enter" plain KeyDown) "Arm highlighted preset" "Units"
                      IgnoreRepeat available (EditorCommand ArmUnitPalettePreset)
                  yield binding "editor.unit.preset.search"
                      (ExactContext(EditorTool UnitBrowse)) ActiveTool
                      (key "/" plain KeyDown) "Focus preset search" "Units"
                      IgnoreRepeat available FocusUnitPresetSearch
                  yield binding "editor.unit.preset.exit"
                      (ExactContext(EditorTool UnitBrowse)) ActiveTool
                      (key "Escape" plain KeyDown) "Return to Select" "Units"
                      IgnoreRepeat available (EditorCommand(ChooseTool Select))
              | Place _ when facts.Editor.Gesture = IdleGesture ->
                  let move id value dx dy =
                      binding id (ExactContext(EditorTool facts.Editor.Tool)) ActiveTool
                          (key value plain KeyDown) "Move the placement cursor" "Units"
                          AllowRepeat available (EditorCommand(MoveUnitPlacementCursor(dx, dy)))
                  yield move "editor.unit.place.west" "ArrowLeft" -1 0
                  yield move "editor.unit.place.east" "ArrowRight" 1 0
                  yield move "editor.unit.place.north" "ArrowUp" 0 -1
                  yield move "editor.unit.place.south" "ArrowDown" 0 1
                  yield binding "editor.unit.place.previous-preset"
                      (ExactContext(EditorTool facts.Editor.Tool)) ActiveTool
                      (key "[" plain KeyDown) "Arm previous visible preset" "Units"
                      AllowRepeat available (EditorCommand(CycleArmedUnitPreset -1))
                  yield binding "editor.unit.place.next-preset"
                      (ExactContext(EditorTool facts.Editor.Tool)) ActiveTool
                      (key "]" plain KeyDown) "Arm next visible preset" "Units"
                      AllowRepeat available (EditorCommand(CycleArmedUnitPreset 1))
                  yield binding "editor.unit.place.commit"
                      (ExactContext(EditorTool facts.Editor.Tool)) ActiveTool
                      (key "Enter" plain KeyDown) "Place and remain armed" "Units"
                      IgnoreRepeat available (EditorCommand(CommitUnitPlacement false))
                  yield binding "editor.unit.place.commit-return"
                      (ExactContext(EditorTool facts.Editor.Tool)) ActiveTool
                      (key "Enter" { plain with Shift = true } KeyDown)
                      "Place and return to preset browse" "Units"
                      IgnoreRepeat available (EditorCommand(CommitUnitPlacement true))
                  for value, suffix in [ "b", "browse"; "Escape", "cancel" ] do
                      yield binding ("editor.unit.place." + suffix)
                          (ExactContext(EditorTool facts.Editor.Tool)) ActiveTool
                          (key value plain KeyDown) "Return to unit preset browse" "Units"
                          IgnoreRepeat available (EditorCommand ReturnToUnitBrowse)
              | Terrain _ ->
                  let movement id value dx dy =
                      binding id (ExactContext(EditorTool facts.Editor.Tool)) ActiveTool
                          (key value plain KeyDown) "Move the terrain cursor" "Terrain"
                          AllowRepeat available (EditorCommand(MoveTerrainCursor(dx, dy, false)))
                  yield movement "editor.terrain.cursor.west" "ArrowLeft" -1 0
                  yield movement "editor.terrain.cursor.east" "ArrowRight" 1 0
                  yield movement "editor.terrain.cursor.north" "ArrowUp" 0 -1
                  yield movement "editor.terrain.cursor.south" "ArrowDown" 0 1
                  let shiftedMovement id value dx dy =
                      binding id (ExactContext(EditorTool facts.Editor.Tool)) ActiveTool
                          (key value { plain with Shift = true } KeyDown)
                          "Paint or extend through the moved cell" "Terrain"
                          AllowRepeat available (EditorCommand(MoveTerrainCursor(dx, dy, true)))
                  yield shiftedMovement "editor.terrain.cursor.paint-west" "ArrowLeft" -1 0
                  yield shiftedMovement "editor.terrain.cursor.paint-east" "ArrowRight" 1 0
                  yield shiftedMovement "editor.terrain.cursor.paint-north" "ArrowUp" 0 -1
                  yield shiftedMovement "editor.terrain.cursor.paint-south" "ArrowDown" 0 1
                  yield binding "editor.terrain.activate" (ExactContext(EditorTool facts.Editor.Tool)) ActiveTool
                      (key "Enter" plain KeyDown) "Activate at the terrain cursor" "Terrain"
                      IgnoreRepeat available (EditorCommand ActivateTerrainCursor)
                  for (value, terrain, name) in
                      [ "1", Open, "Open"; "2", Rough, "Rough"; "3", Blocked, "Blocked"; "4", Objective, "Objective" ] do
                      yield binding ("editor.terrain.value." + name.ToLowerInvariant())
                          (ExactContext(EditorTool facts.Editor.Tool)) ActiveTool
                          (key value plain KeyDown) ("Choose " + name + " terrain") "Terrain values"
                          IgnoreRepeat available (EditorCommand(ChooseTerrain terrain))
                  yield binding "editor.terrain.brush.decrease"
                      (ExactContext(EditorTool facts.Editor.Tool)) ActiveTool
                      (key "[" plain KeyDown) "Decrease brush size" "Terrain"
                      AllowRepeat available (EditorCommand(SetTerrainBrushSize(facts.Editor.BrushSize - 1)))
                  yield binding "editor.terrain.brush.increase"
                      (ExactContext(EditorTool facts.Editor.Tool)) ActiveTool
                      (key "]" plain KeyDown) "Increase brush size" "Terrain"
                      AllowRepeat available (EditorCommand(SetTerrainBrushSize(facts.Editor.BrushSize + 1)))
                  if facts.Editor.Gesture = IdleGesture then
                      yield binding "editor.terrain.exit" (ExactContext(EditorTool facts.Editor.Tool)) ActiveTool
                          (key "Escape" plain KeyDown) "Return to Select" "Terrain"
                          IgnoreRepeat available
                          (EditorCommand(
                              match facts.Editor.Tool with
                              | Terrain EyedropperTool -> ChooseTool(Terrain facts.Editor.LastTerrainPaintTool)
                              | _ -> ChooseTool Select
                          ))
              | _ -> () ]
        catalog

    let simulatorCatalog
        (selectedUnitId: int32 option)
        (handoff: SimulatorHandoff option)
        (controllerSelection: MapController option)
        =
        let simulatorKey id value label group repeat availability command =
            binding id AnySimulatorContext WorkspaceCommands
                (key value plain KeyDown) label group repeat availability command

        let popupInactive contexts =
            if List.contains SimulatorControllerSelection contexts then
                Unavailable "Finish or cancel controller selection first."
            elif handoff.IsSome then Available
            else Unavailable "Create a simulator handoff from the Editor first."

        let paused contexts =
            if List.contains SimulatorControllerSelection contexts then
                Unavailable "Finish or cancel controller selection first."
            else
                match handoff with
                | Some simulator when not simulator.IsRunning -> Available
                | Some _ -> Unavailable "This command is unavailable while running."
                | None -> Unavailable "Create a simulator handoff from the Editor first."

        let selectedPaused contexts =
            match paused contexts, selectedUnitId, handoff with
            | Available, Some _, Some _ -> Available
            | Available, None, Some _ -> Unavailable "Select a unit first."
            | Unavailable reason, _, _ -> Unavailable reason
            | _ -> Unavailable "Create a simulator handoff from the Editor first."

        let hasUnits contexts =
            match popupInactive contexts, handoff with
            | Available, Some simulator when not (Map.isEmpty simulator.RuntimeMap.Units) -> Available
            | Available, Some _ -> Unavailable "The simulator has no units."
            | Unavailable reason, _ -> Unavailable reason
            | _ -> Unavailable "Create a simulator handoff from the Editor first."

        let previewAvailable contexts =
            match paused contexts, handoff with
            | Available, Some simulator when simulator.PreviewDestination.IsSome -> Available
            | Available, Some _ -> Unavailable "No route preview is active."
            | Unavailable reason, _ -> Unavailable reason
            | _ -> Unavailable "Create a simulator handoff from the Editor first."

        let controllerActive contexts =
            match controllerSelection, selectedUnitId, handoff with
            | Some _, Some _, Some simulator when not simulator.IsRunning -> Available
            | None, _, _ -> Unavailable "Controller selection is not active."
            | _, None, Some _ -> Unavailable "Select a unit first."
            | _, _, Some _ -> Unavailable "Controller mutation is unavailable while running."
            | _, _, None -> Unavailable "Create a simulator handoff from the Editor first."

        [ yield binding "simulator.help.toggle" AnySimulatorContext WorkspaceCommands
              (key "?" { plain with Shift = true } KeyDown)
              "Show or hide possible inputs" "Help" IgnoreRepeat available ToggleInputHelp
          yield simulatorKey "simulator.panel.toggle" "F2" "Show or hide the active simulator panel" "Panels"
              IgnoreRepeat popupInactive ToggleSimulatorCommandPanel
          yield simulatorKey "simulator.panel.controls" "c" "Show the Controls panel" "Panels"
              IgnoreRepeat popupInactive (ChooseSimulatorPanel ControllerPanel)
          yield simulatorKey "simulator.panel.events" "e" "Show the Events panel" "Panels"
              IgnoreRepeat popupInactive (ChooseSimulatorPanel EventPanel)
          yield simulatorKey "simulator.panel.samples" "a" "Show the Samples panel" "Panels"
              IgnoreRepeat popupInactive (ChooseSimulatorPanel SimulatorSamplePanel)
          yield simulatorKey "simulator.unit.previous" "[" "Inspect the previous unit" "Units"
              IgnoreRepeat hasUnits (TraverseSimulatorUnit -1)
          yield simulatorKey "simulator.unit.next" "]" "Inspect the next unit" "Units"
              IgnoreRepeat hasUnits (TraverseSimulatorUnit 1)
          yield simulatorKey "simulator.run.toggle-space" "Space" "Start or pause the simulator" "Simulation"
              IgnoreRepeat popupInactive (SimulatorCommand ToggleSimulatorRun)
          yield simulatorKey "simulator.run.toggle-k" "k" "Start or pause the simulator" "Simulation"
              IgnoreRepeat popupInactive (SimulatorCommand ToggleSimulatorRun)
          yield simulatorKey "simulator.step" "." "Advance exactly one deterministic tick" "Simulation"
              IgnoreRepeat paused (SimulatorCommand StepSimulator)
          yield simulatorKey "simulator.reset.request" "r" "Reset the simulator sandbox" "Simulation"
              IgnoreRepeat paused RequestSimulatorSandboxReset
          yield simulatorKey "simulator.controller.begin" "Enter" "Choose the selected unit controller" "Controllers"
              IgnoreRepeat selectedPaused BeginSimulatorControllerSelection
          let movement id value dx dy =
              simulatorKey id value "Move the route-preview destination" "Route preview"
                  AllowRepeat selectedPaused (SimulatorCommand(MoveSimulatorPreview(dx, dy)))
          yield movement "simulator.preview.west" "ArrowLeft" -1 0
          yield movement "simulator.preview.east" "ArrowRight" 1 0
          yield movement "simulator.preview.north" "ArrowUp" 0 -1
          yield movement "simulator.preview.south" "ArrowDown" 0 1
          let fastMovement id value dx dy =
              binding id AnySimulatorContext WorkspaceCommands
                  (key value { plain with Shift = true } KeyDown)
                  "Move the route-preview destination five cells" "Route preview"
                  AllowRepeat selectedPaused
                  (SimulatorCommand(MoveSimulatorPreview(dx * 5, dy * 5)))
          yield fastMovement "simulator.preview.fast-west" "ArrowLeft" -1 0
          yield fastMovement "simulator.preview.fast-east" "ArrowRight" 1 0
          yield fastMovement "simulator.preview.fast-north" "ArrowUp" 0 -1
          yield fastMovement "simulator.preview.fast-south" "ArrowDown" 0 1
          yield binding "simulator.preview.commit" (ExactContext SimulatorRoutePreview)
              ActiveGestureOrPreview (key "Enter" plain KeyDown)
              "Commit the route preview" "Route preview"
              IgnoreRepeat previewAvailable (SimulatorCommand CommitSimulatorPreview)
          yield binding "simulator.preview.reset" (ExactContext SimulatorRoutePreview)
              ActiveGestureOrPreview (key "Backspace" plain KeyDown)
              "Return the route preview to the unit origin" "Route preview"
              IgnoreRepeat previewAvailable (SimulatorCommand ResetSimulatorPreviewToOrigin)

          match handoff with
          | Some simulator when simulator.PreviewDestination.IsSome ->
              yield binding "simulator.preview.cancel" (ExactContext SimulatorRoutePreview)
                  ActiveGestureOrPreview (key "Escape" plain KeyDown)
                  "Discard the route preview" "Route preview"
                  IgnoreRepeat previewAvailable (SimulatorCommand ResetSimulatorPreview)
          | _ -> () ]

        @
        [ yield binding "simulator.controller.manual" (ExactContext SimulatorControllerSelection)
                    ActiveGestureOrPreview (key "m" plain KeyDown)
                    "Choose Manual controller" "Controller selection"
                    IgnoreRepeat controllerActive (ChooseSimulatorController Manual)
          yield binding "simulator.controller.scripted" (ExactContext SimulatorControllerSelection)
                    ActiveGestureOrPreview (key "s" plain KeyDown)
                    "Choose Scripted controller" "Controller selection"
                    IgnoreRepeat controllerActive (ChooseSimulatorController Scripted)
          yield binding "simulator.controller.general" (ExactContext SimulatorControllerSelection)
                    ActiveGestureOrPreview (key "g" plain KeyDown)
                    "Choose General AI controller" "Controller selection"
                    IgnoreRepeat controllerActive (ChooseSimulatorController General)
          yield binding "simulator.controller.commit" (ExactContext SimulatorControllerSelection)
                    ActiveGestureOrPreview (key "Enter" plain KeyDown)
                    "Commit controller choice" "Controller selection"
                    IgnoreRepeat controllerActive CommitSimulatorController
          yield binding "simulator.controller.cancel" (ExactContext SimulatorControllerSelection)
                    ActiveGestureOrPreview (key "Escape" plain KeyDown)
                    "Cancel controller choice" "Controller selection"
                    IgnoreRepeat controllerActive CancelSimulatorController ]

    let traverseSimulatorUnit delta selectedUnitId (handoff: SimulatorHandoff) =
        let identifiers =
            handoff.RuntimeMap.Units
            |> Map.toArray
            |> Array.map fst
            |> Array.sort

        if Array.isEmpty identifiers then
            None
        else
            let currentIndex =
                selectedUnitId
                |> Option.bind (fun selected ->
                    identifiers |> Array.tryFindIndex ((=) selected))

            let nextIndex =
                match currentIndex with
                | Some index ->
                    (index + delta % identifiers.Length + identifiers.Length) % identifiers.Length
                | None when delta < 0 -> identifiers.Length - 1
                | None -> 0

            Some identifiers[nextIndex]

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

    let private keyboardObjectName = function
        | KeyboardUnit id -> "unit " + string id
        | KeyboardRegion id -> "region " + string id
        | KeyboardEdge(_, _, direction) -> string direction + " edge"
        | KeyboardTerrain _ -> "terrain cell"

    let projectEditor facts catalog =
        let contexts = deriveEditorContexts facts
        let baseBreadcrumb, baseDetail =
            match facts.Editor.Gesture, facts.Editor.Tool with
            | SelectedObjectActionsGesture, _ ->
                [ "Editor"; "Select"; "Actions" ],
                match facts.Editor.SelectedRegion with
                | Some id -> "Region " + string id + " selected"
                | None ->
                    string facts.Editor.SelectedUnits.Count
                    + (if facts.Editor.SelectedUnits.Count = 1 then " unit selected" else " units selected")
            | BoxSelectionGesture(anchor, current), _ ->
                [ "Editor"; "Select"; "Box selection" ],
                "Anchor " + addressText anchor + " — current " + addressText current
            | UnitMoveGesture(_, current, original, _), _ ->
                [ "Editor"; "Units"; "Move preview" ],
                string original.Length + " units — preview at " + addressText current
            | TerrainGesture(tool, anchor, current, visited), _ ->
                let previewCount =
                    facts.Editor
                    |> MapEditor.terrainPreview
                    |> Option.map (fun (_, addresses, _) -> addresses.Length)
                    |> Option.defaultValue visited.Length
                [ "Editor"; "Terrain"; terrainToolName tool ],
                terrainName facts.Editor.TerrainSelection
                + " terrain — anchor " + addressText anchor
                + ", endpoint " + addressText current
                + " — " + string previewCount + " cells"
            | EdgePolylineGesture(_, segments), _ ->
                [ "Editor"; "Edges"; "Polyline" ],
                string segments.Length + " segments staged"
            | CommandPreviewGesture(AddUnits units), Place _ ->
                [ "Editor"; "Units"; "Place preview" ],
                string units.Length
                + (if units.Length = 1 then " unit footprint" else " unit footprints")
                + " staged"
            | CommandPreviewGesture(AddUnits units), _ ->
                [ "Editor"; "Units"; "Paste preview" ],
                string units.Length
                + (if units.Length = 1 then " unit" else " units")
                + " staged — Enter commits one undoable command"
            | CommandPreviewGesture _, _ ->
                [ "Editor"; "Command preview" ], "A validated editor command is ready to commit"
            | IdleGesture, Select ->
                let objects = MapEditor.keyboardObjectsAtCursor facts.Editor
                let current =
                    facts.Editor.KeyboardObject
                    |> Option.orElseWith (fun () -> List.tryHead objects)
                [ "Editor"; "Select" ],
                "Cursor " + addressText facts.Editor.KeyboardCursor.Cell
                + " — " + string facts.Editor.SelectedUnits.Count + " units selected"
                + (current
                   |> Option.map (fun value ->
                       " — " + keyboardObjectName value
                       + " (" + string (facts.Editor.KeyboardCursor.ObjectCycleIndex + 1)
                       + " of " + string objects.Length + ")")
                   |> Option.defaultValue "")
            | IdleGesture, Terrain tool ->
                [ "Editor"; "Terrain"; terrainToolName tool ],
                terrainName facts.Editor.TerrainSelection
                + " terrain — " + string facts.Editor.BrushSize + "×" + string facts.Editor.BrushSize
                + " brush — cursor " + addressText facts.Editor.TerrainCursor
            | IdleGesture, UnitBrowse ->
                let visible =
                    MapEditor.searchCanonicalUnitPresets facts.Editor.UnitPaletteSearch
                match MapEditor.selectedUnitPalettePreset facts.Editor with
                | Some preset ->
                    [ "Editor"; "Units"; "Browse" ],
                    preset.Faction
                    + " / "
                    + preset.Name
                    + " — "
                    + string preset.FootprintSize
                    + "×"
                    + string preset.FootprintSize
                    + " — "
                    + string preset.HealthMaximum
                    + " HP — preset "
                    + string (facts.Editor.UnitPaletteCursor.ResultIndex + 1)
                    + " of "
                    + string visible.Length
                | None ->
                    [ "Editor"; "Units"; "Browse" ],
                    "No presets match “" + facts.Editor.UnitPaletteSearch + "”"
            | IdleGesture, Place(side, classId, size) ->
                [ "Editor"; "Units"; "Place" ],
                string side
                + " / "
                + classId
                + " — "
                + string size
                + "×"
                + string size
                + " — preview at "
                + addressText facts.Editor.UnitPlacementCursor
                + " — "
                + (MapEditor.unitPlacementIssue facts.Editor
                   |> Option.map (fun reason -> "invalid: " + reason)
                   |> Option.defaultValue "valid")
            | IdleGesture, Edge(direction, kind) ->
                [ "Editor"; "Edges" ],
                string kind + " / " + string direction
            | IdleGesture, Paint terrain ->
                [ "Editor"; "Terrain"; "Pencil" ], terrainName terrain + " terrain"

        let underlyingBreadcrumb, underlyingDetail =
            match facts.Editor.RegionKeyboardMode with
            | RegionPurposeSelection(editing, highlighted) ->
                [ "Editor"; "Zones"; if editing then "Purpose" else "New"; "Purpose" ],
                (if editing then "Change selected region purpose — " else "Choose region purpose — ")
                + string highlighted
            | RegionShapeSelection purpose ->
                [ "Editor"; "Zones"; "New"; "Shape" ],
                string purpose + " — choose rectangle or polygon"
            | RegionRectangleConstruction(_, anchor) ->
                [ "Editor"; "Zones"; "New"; "Rectangle" ],
                "Cursor " + addressText facts.Editor.KeyboardCursor.Cell
                + (anchor
                   |> Option.map (fun value -> " — anchor " + addressText value)
                   |> Option.defaultValue " — choose first corner")
            | RegionPolygonConstruction(_, vertices) ->
                [ "Editor"; "Zones"; "New"; "Polygon" ],
                string vertices.Length + " vertices — cursor " + addressText facts.Editor.KeyboardCursor.Cell
            | RegionMovePreview _ ->
                [ "Editor"; "Zones"; "Move" ], facts.Editor.RegionAnnouncement
            | RegionResizePreview(_, RegionRectangle(_, _, width, height)) ->
                [ "Editor"; "Zones"; "Resize" ],
                string width + "×" + string height + " preview"
            | RegionResizePreview _ ->
                [ "Editor"; "Zones"; "Resize" ], facts.Editor.RegionAnnouncement
            | RegionVertexPreview(_, RegionPolygon vertices, index) ->
                [ "Editor"; "Zones"; "Vertices" ],
                "Vertex " + string (index + 1) + " of " + string vertices.Length
            | RegionVertexPreview _ ->
                [ "Editor"; "Zones"; "Vertices" ], facts.Editor.RegionAnnouncement
            | RegionIdle when facts.ActiveDomain = RegionDomain ->
                [ "Editor"; "Zones" ],
                "Cursor " + addressText facts.Editor.KeyboardCursor.Cell
                + (facts.Editor.SelectedRegion
                   |> Option.map (fun id -> " — region " + string id + " selected")
                   |> Option.defaultValue " — no region selected")
            | RegionIdle when facts.ActiveDomain = DocumentDomain ->
                [ "Editor"; "Document" ],
                "Map “" + facts.Editor.Authoring.Name + "” — revision "
                + string facts.Editor.Revision.Number
                + (if facts.Editor.RevisionState = DirtyRevision then " — dirty" else " — saved")
            | RegionIdle when facts.ActiveDomain = EdgeDomain ->
                let column, row, direction = facts.Editor.EdgeCursor
                let existing =
                    facts.Editor.Map.Edges
                    |> Map.tryFind (column, row, direction)
                    |> Option.map (fun (kind, isOpen) ->
                        string kind + if kind = Door then (if isOpen then " open" else " closed") else "")
                    |> Option.defaultValue "no edge"
                [ "Editor"; "Edges" ],
                "Cursor edge " + addressText { CellColumn = column; CellRow = row }
                + " " + string direction + " — " + existing
            | RegionIdle -> baseBreadcrumb, baseDetail

        let breadcrumb, detail =
            if facts.Editor.PendingDestructiveChange.IsSome then
                [ "Editor"; "Destructive confirmation" ],
                (match facts.Editor.PendingDestructiveChange with
                 | Some ClearPending -> "Confirm clearing the current map"
                 | Some(ResizePending preview) ->
                     "Confirm resize to " + string preview.TargetWidth + "×" + string preview.TargetHeight
                 | Some(NewMapPending(width, height, name)) ->
                     "Confirm new map " + name + " at " + string width + "×" + string height
                 | Some(UnitDeletionPending identifiers) ->
                     "Confirm deleting "
                     + string identifiers.Length
                     + (if identifiers.Length = 1 then " unit" else " units")
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
            | Some simulator when facts.SimulatorControllerSelection.IsSome ->
                let choice = facts.SimulatorControllerSelection.Value
                [ "Simulator"; "Controller" ],
                "Unit "
                + (selectedUnitId |> Option.map string |> Option.defaultValue "not selected")
                + " — " + MapEditor.controllerLabel choice
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
