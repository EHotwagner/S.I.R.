module SIR.Client.TestsUnifiedTacticalWorkspaceQualification

open SIR.Client

let private require condition message =
    if not condition then failwith message

let run () =
    let authored =
        { Id = "unit-1-route-1"
          UnitId = Some 1
          StartTick = 4L
          EndTick = 8L
          Channel = Authored
          Label = "Route east"
          Issue = None }
    let initial = UnifiedTacticalWorkspace.initial 20L
    let planned =
        initial
        |> UnifiedTacticalWorkspace.switchModality Plan
        |> UnifiedTacticalWorkspace.addSegment authored
        |> Result.defaultWith (fun error -> failwithf "Could not author fixture: %A" error)
    let scrubbed = planned |> UnifiedTacticalWorkspace.scrub 7L
    let simulated = scrubbed |> UnifiedTacticalWorkspace.switchModality Simulate
    let accepted = simulated |> UnifiedTacticalWorkspace.acceptThrough 8L
    let committed = accepted |> UnifiedTacticalWorkspace.commitThrough 8L
    let reviewed = committed |> UnifiedTacticalWorkspace.switchModality Review

    require
        (planned.Modality = Plan
         && simulated.Cursor = 7L
         && reviewed.Cursor = 8L
         && [ Authored; Accepted; Committed ]
            |> List.forall (fun channel ->
                reviewed.Segments
                |> List.exists (fun segment -> segment.Channel = channel))
         && UnifiedTacticalWorkspace.nextEditableBoundary reviewed = 9L)
        "Unified modality, cursor, or authored/accepted/committed projection diverged."

    let progressUpdate =
        Unchecked.defaultof<SimulatorProjectionUpdateTransport>
    let authoritativeProgress =
        [ PlanCommitted 1L
          SimulatorStepped progressUpdate
          SimulatorProgress(1, progressUpdate)
          SimulatorRunCompleted progressUpdate
          SimulatorReset progressUpdate
          AuthoritativeRunLoaded("lock", "replay", 12) ]
        |> List.map (fun response ->
            UnifiedTacticalWorkspace.authoritativeProgressBoundary response 12)
    require
        (authoritativeProgress |> List.forall ((=) (Some 12L)))
        "An authoritative planning-worker progress response did not advance the shared committed boundary."

    let immutableBeforeScrub = reviewed.Segments, reviewed.CommittedThrough
    let backward = reviewed |> UnifiedTacticalWorkspace.scrub 0L
    let forward = backward |> UnifiedTacticalWorkspace.scrub 20L
    require
        ((backward.Segments, backward.CommittedThrough) = immutableBeforeScrub
         && (forward.Segments, forward.CommittedThrough) = immutableBeforeScrub
         && backward.Cursor = 0L
         && forward.Cursor = 20L)
        "Timeline scrubbing mutated authored or committed state."

    let committedMove =
        reviewed
        |> UnifiedTacticalWorkspace.moveSegment authored.Id 2L
    let committedRemoval =
        reviewed
        |> UnifiedTacticalWorkspace.removeSegment authored.Id
    require
        (committedMove = Error CommittedInterval
         && committedRemoval = Error CommittedInterval)
        "Committed timeline editing was accepted."

    let predicted =
        { authored with
            Id = "unit-1-predicted"
            StartTick = 9L
            EndTick = 10L
            Channel = Predicted
            Label = "Predicted projection" }
    let withPrediction =
        reviewed
        |> UnifiedTacticalWorkspace.addSegment predicted
        |> Result.defaultWith (fun error -> failwithf "Prediction fixture failed: %A" error)
    require
        (withPrediction.Segments
         |> List.exists (fun segment ->
             segment.Id = predicted.Id && segment.Channel = Predicted)
         && reviewed.Segments
            |> List.forall (fun segment -> segment.Id <> predicted.Id)
         && UnifiedTacticalWorkspace.validate withPrediction = [])
        "Predicted state was not an explicitly separate projection."

    let planning =
        MapEditor.initial.Map.Units
        |> Map.toSeq
        |> Seq.map snd
        |> PlanningWorkspace.initial MapEditor.initial.Revision.Digest
        |> PlanningWorkspace.update (SetPlanningAuthoringTick 12)
        |> PlanningWorkspace.update AddPlanningHold
    let authoredCommand = planning.Commands |> List.exactlyOne
    let moved =
        planning
        |> PlanningWorkspace.update (MoveSelectedPlanningCommandTo 15)
    let movedCommand = moved.Commands |> List.exactlyOne
    let undone = moved |> PlanningWorkspace.update UndoPlanning
    let redone = undone |> PlanningWorkspace.update RedoPlanning
    require
        (authoredCommand.EarliestTick = 12
         && movedCommand.EarliestTick = 15
         && moved.Revision > planning.Revision
         && moved.NextRevision > moved.Revision
         && undone.Commands = planning.Commands
         && redone.Commands = moved.Commands
         && redone.NextRevision = moved.NextRevision)
        "Current-time plan authoring or exact undo/redo identity diverged."

    let committedPlan =
        { moved with
            CommittedRevision = Some moved.Revision
            CommittedTick = Some 15
            AuthoringTick = 16 }
    let committedRemoval =
        committedPlan
        |> PlanningWorkspace.update RemoveSelectedPlanningCommand
    let committedMove =
        committedPlan
        |> PlanningWorkspace.update (MoveSelectedPlanningCommandTo 16)
    let unsafeRedoPlan =
        { undone with
            CommittedRevision = Some undone.Revision
            CommittedTick = Some 15
            AuthoringTick = 16 }
    require
        (committedRemoval = committedPlan
         && committedMove = committedPlan
         && not (PlanningWorkspace.canUndo committedPlan)
         && PlanningWorkspace.update UndoPlanning committedPlan = committedPlan
         && not (PlanningWorkspace.canRedo unsafeRedoPlan)
         && PlanningWorkspace.update RedoPlanning unsafeRedoPlan = unsafeRedoPlan)
        "Committed plan commands remained editable through remove, move, undo, or redo."

    let registry = UnifiedTacticalWorkspace.commandRegistry
    let rebound =
        UnifiedTacticalWorkspace.emptyBindingProfile
        |> UnifiedTacticalWorkspace.setBinding
            registry
            "planning.route"
            (Some "Shift+R")
            false
        |> Result.defaultWith (fun diagnostics ->
            failwithf "Valid binding override failed: %A" diagnostics)
    let exported = UnifiedTacticalWorkspace.exportBindings rebound
    let imported =
        UnifiedTacticalWorkspace.importBindings registry exported
        |> Result.defaultWith (fun diagnostics ->
            failwithf "Exported bindings failed import: %A" diagnostics)
    let migrated =
        exported
            .Replace("\"schemaVersion\":1", "\"schemaVersion\":0")
            .Replace("\"bindings\":", "\"overrides\":")
        |> UnifiedTacticalWorkspace.importBindings registry
        |> Result.defaultWith (fun diagnostics ->
            failwithf "Version-zero binding migration failed: %A" diagnostics)
    require
        (exported =
            "{\"schemaVersion\":1,\"bindings\":[{\"id\":\"planning.route\",\"gesture\":\"Shift+R\"}]}"
         && imported = rebound
         && migrated = rebound
         && UnifiedTacticalWorkspace.validateBindings registry rebound = [])
        "Binding overrides were not deterministic, versioned, or migration-safe."

    let reserved =
        UnifiedTacticalWorkspace.setBinding
            registry
            "planning.route"
            (Some "Ctrl+L")
            false
            rebound
    let conflict =
        UnifiedTacticalWorkspace.setBinding
            registry
            "planning.facing"
            (Some "Shift+R")
            false
            rebound
    let malformed =
        UnifiedTacticalWorkspace.importBindings
            registry
            "{\"schemaVersion\":1,\"bindings\":[{\"gesture\":\"R\"}]}"
    require
        (match reserved with
         | Error diagnostics ->
             diagnostics
             |> List.exists (function ReservedTacticalGesture _ -> true | _ -> false)
         | Ok _ -> false
         && match conflict with
            | Error diagnostics ->
                diagnostics
                |> List.exists (function TacticalBindingConflict _ -> true | _ -> false)
            | Ok _ -> false
         && match malformed with Error _ -> true | Ok _ -> false)
        "Reserved, conflicting, or malformed binding input was accepted."

    let cleared =
        rebound
        |> UnifiedTacticalWorkspace.setBinding
            registry
            "planning.route"
            None
            false
        |> Result.defaultWith (fun diagnostics ->
            failwithf "Clearing a binding failed: %A" diagnostics)
    let restored =
        rebound
        |> UnifiedTacticalWorkspace.restoreCommand "planning.route"
    let clearedGesture =
        registry
        |> List.find (fun command -> command.Id = "planning.route")
        |> UnifiedTacticalWorkspace.effectiveGesture cleared
    require
        (clearedGesture = None
         && restored = UnifiedTacticalWorkspace.emptyBindingProfile)
        "Clear or restore-command binding flow diverged."

    let routeCommand = registry |> List.find (fun command -> command.Id = "planning.route")
    require
        (UnifiedTacticalWorkspace.displayGesture (UnifiedTacticalWorkspace.effectiveGesture rebound routeCommand) = "Shift+R"
         && UnifiedTacticalWorkspace.accessibleGesture (Some "Ctrl/Cmd+←") = Some "Control+ArrowLeft"
         && UnifiedTacticalWorkspace.displayGestureFor MetaPlatform (Some "Ctrl+Shift+2") = "Cmd+Shift+2"
         && UnifiedTacticalWorkspace.accessibleGestureFor MetaPlatform (Some "Ctrl+Shift+2") = Some "Meta+Shift+2"
         && UnifiedTacticalWorkspace.displayGesture None = "Unassigned"
         && UnifiedTacticalWorkspace.accessibleGesture None = None)
        "Visible and accessible shortcut presentation did not derive from the effective registry binding."

    let modalBinding: ModalBinding<ModalCommand> =
        { Id = "editor.fixture.action"
          Context = AnyEditorContext
          Precedence = WorkspaceCommands
          BindingGesture =
            { Key = NormalizedKey.create "x" None
              Modifiers = KeyModifiers.none
              Phase = KeyDown }
          Label = "Fixture action"
          Group = "Fixture"
          Repeat = IgnoreRepeat
          Availability = fun _ -> Available
          Command = ToggleInputHelp }
    let modalRegistry =
        UnifiedTacticalWorkspace.modalCommandDefinitions
            Editor
            [ modalBinding ]
    let modalCleared =
        UnifiedTacticalWorkspace.setBinding
            modalRegistry
            modalBinding.Id
            None
            false
            UnifiedTacticalWorkspace.emptyBindingProfile
        |> Result.defaultWith (fun diagnostics ->
            failwithf "Modal clear failed: %A" diagnostics)
    let modalRebound =
        UnifiedTacticalWorkspace.setBinding
            modalRegistry
            modalBinding.Id
            (Some "Ctrl+Shift+X")
            false
            UnifiedTacticalWorkspace.emptyBindingProfile
        |> Result.defaultWith (fun diagnostics ->
            failwithf "Modal rebound failed: %A" diagnostics)
    let clearedCatalog =
        UnifiedTacticalWorkspace.adaptModalCatalog modalCleared [ modalBinding ]
    let reboundCatalog =
        UnifiedTacticalWorkspace.adaptModalCatalog modalRebound [ modalBinding ]
    let overlappingBinding =
        { modalBinding with
            Id = "editor.fixture.overlap"
            BindingGesture =
                { modalBinding.BindingGesture with
                    Key = NormalizedKey.create "y" None } }
    let overlappingRegistry =
        UnifiedTacticalWorkspace.modalCommandDefinitions
            Editor
            [ modalBinding; overlappingBinding ]
    let overlapConflict =
        UnifiedTacticalWorkspace.setBinding
            overlappingRegistry
            modalBinding.Id
            (Some "Y")
            false
            UnifiedTacticalWorkspace.emptyBindingProfile
    let mutuallyExclusiveBindings =
        [ { modalBinding with
              Id = "editor.fixture.terrain"
              Context = ExactContext(EditorDomain TerrainDomain) }
          { modalBinding with
              Id = "editor.fixture.unit"
              Context = ExactContext(EditorDomain UnitDomain) } ]
    let phaseSeparatedBindings =
        [ modalBinding
          { modalBinding with
              Id = "editor.fixture.key-up"
              BindingGesture =
                { modalBinding.BindingGesture with Phase = KeyUp } } ]
    let nonConflictingProfiles =
        [ mutuallyExclusiveBindings; phaseSeparatedBindings ]
        |> List.map (fun bindings ->
            let registry =
                UnifiedTacticalWorkspace.modalCommandDefinitions Editor bindings
            bindings
            |> List.fold (fun profile binding ->
                profile
                |> Result.bind (
                    UnifiedTacticalWorkspace.setBinding
                        registry
                        binding.Id
                        (Some "Y")
                        false
                ))
                (Ok UnifiedTacticalWorkspace.emptyBindingProfile))
    require
        (List.isEmpty clearedCatalog
         && (reboundCatalog
             |> List.exactlyOne
             |> _.BindingGesture
             |> UnifiedTacticalWorkspace.gestureText) = "Ctrl+Shift+X"
         && Result.isError overlapConflict
         && nonConflictingProfiles |> List.forall Result.isOk)
        "Cleared/rebound modal dispatch or overlapping-context conflict diagnosis diverged."

    let strictFailures =
        [ """{"schemaVersion":2,"bindings":[]}"""
          """{"schemaVersion":1,"schemaVersion":1,"bindings":[]}"""
          """{"schemaVersion":1,"bindings":[],"extra":true}"""
          """{"schemaVersion":1,"bindings":[{"id":"planning.route","gesture":"R"},{"id":"planning.route","gesture":"F"}]}"""
          """{"schemaVersion":1,"bindings":[{"id":"editor.panel.toggl","gesture":"F11"}]}"""
          """{"schemaVersion":1,"bindings":[]} trailing""" ]
        |> List.map (UnifiedTacticalWorkspace.importBindings registry)
    require
        (strictFailures |> List.forall Result.isError)
        "Strict binding schema accepted a future version, duplicate field/ID, unknown field, or trailing content."
    require
        (UnifiedTacticalWorkspace.importBindings
            registry
            """{"schemaVersion":1,"bindings":[{"id":"editor.terrain.gesture.west","gesture":"F11"}]}"""
         |> Result.isOk)
        "A valid inactive modal command ID did not survive stable-registry import."

    let documentationPage slug title category text =
        { Slug = slug
          Title = title
          Category = category
          Status = ImplementedDocumentation
          SourcePath = "docs/" + slug + ".md"
          ApiPath = None
          ContentDigest = slug
          Anchors = [ slug ]
          Headings = [ title, slug ]
          Related = []
          Blocks =
            [ { Kind = "paragraph"
                Level = None
                Anchor = None
                Text = text
                ContentSegments = [ { SegmentKind = "text"; SegmentText = text; TargetSlug = None; Anchor = None; ExternalUrl = None } ]
                Rows = []
                ImageSource = None } ] }
    let lineOfSightPage = documentationPage "line-of-sight" "Line of sight" "Combat" "LOS cover and armor interactions"
    let armorPage = documentationPage "armor" "Armor" "Combat" "Armor mitigation"
    let documentationManifest =
        { Schema = "sir-in-app-docs-v1"
          DefinitionDigest = "qualification"
          Pages = [ armorPage; lineOfSightPage ]
          Sources =
            [ "combat",
              { Repository = "EHotwagner/S.I.R."
                Revision = "0123456789abcdef"
                Path = "src/SIR.Simulation/Combat.fs"
                PageSlug = "line-of-sight"
                Concept = "combat"
                Symbol = Some "resolve"
                Line = Some 42
                ContentDigest = "content"
                LineDigest = "line" } ]
            |> Map.ofList
          SearchTokenCount = 9 }
    let losResults =
        UnifiedTacticalWorkspace.documentationSearch "LOS cover armor" documentationManifest
    let navigated =
        [ 1 .. UnifiedTacticalWorkspace.DocumentationHistoryLimit + 8 ]
        |> List.fold (fun state index ->
            UnifiedTacticalWorkspace.openDocumentationPage ("page-" + string index) None state)
            UnifiedTacticalWorkspace.initialDocumentationNavigation
    let rewound = UnifiedTacticalWorkspace.documentationBack navigated
    let replayed = UnifiedTacticalWorkspace.documentationForward rewound
    require
        ((losResults |> List.map _.Slug) = [ "line-of-sight" ])
        "Documentation search did not require every normalized LOS/cover/armor term."
    require
        (navigated.Back.Length = UnifiedTacticalWorkspace.DocumentationHistoryLimit
         && rewound.Page = Some("page-" + string (UnifiedTacticalWorkspace.DocumentationHistoryLimit + 7))
         && replayed.Page = navigated.Page)
        "Documentation history was not bounded or did not replay deterministically."
    require
        (UnifiedTacticalWorkspace.tryContextualDocumentation (Some " COMBAT ") documentationManifest
         |> Option.exists (fun source -> source.Line = Some 42))
        "A disclosed public concept did not resolve its typed source mapping."
    require
        (UnifiedTacticalWorkspace.tryContextualDocumentation None documentationManifest = None
         && UnifiedTacticalWorkspace.tryContextualDocumentation (Some "undisclosed") documentationManifest = None)
        "Missing or undisclosed contextual input did not fail closed."
