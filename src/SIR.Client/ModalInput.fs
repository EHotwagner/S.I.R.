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
        |> List.sortBy (fun input -> input.Group, input.Label, input.Id)

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
