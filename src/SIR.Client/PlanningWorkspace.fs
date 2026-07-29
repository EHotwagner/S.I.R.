namespace SIR.Client

open System
open System.Text
open SIR.Domain

type PlanningTool =
    | RouteTool
    | FacingTool
    | AttentionTool
    | StanceTool
    | HoldTool
    | EngagementTool
    | SynchronizationTool

type PlanningCommandKind =
    | PlannedRoute of (int32 * int32) array
    | PlannedFacing of Direction8
    | PlannedAttention of Direction8
    | PlannedStance of string
    | PlannedHold
    | PlannedEngagement of targetUnitId: int32 * capabilityId: string
    | PlannedSynchronization of marker: string * deadlineTick: int32

type PlanningCommand =
    { Id: string
      UnitId: int32
      EarliestTick: int32
      Kind: PlanningCommandKind }

type PlanningRosterMember =
    { UnitId: int32
      Name: string
      Side: string
      Role: string
      Equipment: string array
      CapabilityIds: string array
      Column: int32
      Row: int32 }

type PlanningIssue =
    { Code: string
      CommandId: string option
      UnitId: int32 option
      Detail: string }

type PlanningPreview =
    { Revision: int64
      Label: SimulatorPreviewLabel
      Disclosures: string array }

type PlanningSnapshot =
    { Commands: PlanningCommand list
      Revision: int64
      Digest: string }

type PlanningWorkspaceState =
    { SessionId: string
      MapRevision: string
      Roster: PlanningRosterMember array
      SelectedUnit: int32 option
      SelectedCommand: string option
      Tool: PlanningTool
      Commands: PlanningCommand list
      Revision: int64
      NextRevision: int64
      Digest: string
      Past: PlanningSnapshot list
      Future: PlanningSnapshot list
      NextCommand: int32
      NextOperation: int32
      Issues: PlanningIssue array
      FocusedIssue: int option
      Predicted: PlanningPreview option
      AcceptedRevision: int64 option
      CommittedRevision: int64 option
      CommittedTick: int32 option
      WorkerStatus: string }

type PlanningAction =
    | SelectPlanningUnit of int32
    | SelectPlanningCommand of string
    | ChoosePlanningTool of PlanningTool
    | AddRouteWaypoint of column: int32 * row: int32
    | SetPlanningFacing of Direction8
    | SetPlanningAttention of Direction8
    | SetPlanningStance of string
    | AddPlanningHold
    | AddPlanningEngagement of targetUnitId: int32 * capabilityId: string
    | AddPlanningSynchronization of marker: string * deadlineTick: int32
    | RemoveSelectedPlanningCommand
    | UndoPlanning
    | RedoPlanning
    | FocusPlanningIssue of int

[<RequireQualifiedAccess>]
module PlanningWorkspace =
    [<Literal>]
    let IntendedRosterSize = 200

    let private hex (bytes: byte array) =
        bytes
        |> Array.map (fun value -> value.ToString("x2"))
        |> String.concat ""

    let private direction direction =
        string (Direction8.toCode direction)

    let private commandText (command: PlanningCommand) =
        let kind =
            match command.Kind with
            | PlannedRoute cells ->
                "route:"
                + (cells
                   |> Array.map (fun (column, row) -> string column + "," + string row)
                   |> String.concat ";")
            | PlannedFacing value -> "facing:" + direction value
            | PlannedAttention value -> "attention:" + direction value
            | PlannedStance value -> "stance:" + value
            | PlannedHold -> "hold"
            | PlannedEngagement(target, capability) ->
                "engage:" + string target + ":" + capability
            | PlannedSynchronization(marker, deadline) ->
                "sync:" + marker + ":" + string deadline

        String.concat
            "|"
            [ command.Id
              string command.UnitId
              string command.EarliestTick
              kind ]

    let canonicalText (commands: PlanningCommand list) =
        commands
        |> List.sortBy (fun command -> command.UnitId, command.EarliestTick, command.Id)
        |> List.map commandText
        |> fun lines -> String.concat "\n" lines + (if List.isEmpty lines then "" else "\n")

    let digest (commands: PlanningCommand list) =
        commands
        |> canonicalText
        |> Encoding.UTF8.GetBytes
        |> CanonicalHash.sha256
        |> hex

    let private snapshot (state: PlanningWorkspaceState) =
        { Commands = state.Commands
          Revision = state.Revision
          Digest = state.Digest }

    let private edit
        (command: PlanningCommand list -> PlanningCommand list)
        (state: PlanningWorkspaceState)
        =
        let commands = command state.Commands
        if commands = state.Commands then state
        else
            { state with
                Commands = commands
                Revision = state.NextRevision
                NextRevision = state.NextRevision + 1L
                Digest = digest commands
                Past = snapshot state :: state.Past
                Future = []
                AcceptedRevision = None
                Predicted = None
                Issues = [||]
                FocusedIssue = None }

    let private selected (state: PlanningWorkspaceState) =
        state.SelectedUnit

    let private append kind (state: PlanningWorkspaceState) =
        match selected state with
        | None -> state
        | Some unitId ->
            let id = "command-" + state.NextCommand.ToString("D4")
            { edit
                (fun commands ->
                    commands
                    @ [ { Id = id
                          UnitId = unitId
                          EarliestTick = 0
                          Kind = kind } ])
                state with
                SelectedCommand = Some id
                NextCommand = state.NextCommand + 1 }

    let initial (mapRevision: string) (units: EditorUnit seq) =
        let roster =
            units
            |> Seq.sortBy _.Id
            |> Seq.map (fun unit ->
                let loadout =
                    HumanCapabilities.defaultLoadout unit.Id unit.ClassId

                { UnitId = unit.Id
                  Name = unit.ClassId + " " + string unit.Id
                  Side = string unit.Side
                  Role = loadout.Role
                  Equipment = loadout.Equipment
                  CapabilityIds = loadout.CapabilityIds
                  Column = unit.Column
                  Row = unit.Row })
            |> Seq.toArray

        { SessionId = "planning-" + mapRevision.Substring(0, min 12 mapRevision.Length)
          MapRevision = mapRevision
          Roster = roster
          SelectedUnit = roster |> Array.tryHead |> Option.map _.UnitId
          SelectedCommand = None
          Tool = RouteTool
          Commands = []
          Revision = 0L
          NextRevision = 1L
          Digest = digest []
          Past = []
          Future = []
          NextCommand = 1
          NextOperation = 1
          Issues = [||]
          FocusedIssue = None
          Predicted = None
          AcceptedRevision = None
          CommittedRevision = None
          CommittedTick = None
          WorkerStatus = "Not connected" }

    let update action (state: PlanningWorkspaceState) =
        match action with
        | SelectPlanningUnit unitId when state.Roster |> Array.exists (fun unit -> unit.UnitId = unitId) ->
            { state with SelectedUnit = Some unitId; SelectedCommand = None }
        | SelectPlanningUnit _ -> state
        | SelectPlanningCommand commandId when state.Commands |> List.exists (fun command -> command.Id = commandId) ->
            let command = state.Commands |> List.find (fun value -> value.Id = commandId)
            { state with SelectedUnit = Some command.UnitId; SelectedCommand = Some commandId }
        | SelectPlanningCommand _ -> state
        | ChoosePlanningTool tool -> { state with Tool = tool }
        | AddRouteWaypoint(column, row) ->
            match state.SelectedUnit with
            | None -> state
            | Some unitId ->
                let existing =
                    state.Commands
                    |> List.tryFindBack (fun command ->
                        command.UnitId = unitId
                        && match command.Kind with PlannedRoute _ -> true | _ -> false)

                match existing with
                | Some route ->
                    edit
                        (List.map (fun command ->
                            if command.Id = route.Id then
                                match command.Kind with
                                | PlannedRoute cells ->
                                    { command with Kind = PlannedRoute(Array.append cells [| column, row |]) }
                                | _ -> command
                            else command))
                        state
                | None -> append (PlannedRoute [| column, row |]) state
        | SetPlanningFacing value -> append (PlannedFacing value) state
        | SetPlanningAttention value -> append (PlannedAttention value) state
        | SetPlanningStance value -> append (PlannedStance value) state
        | AddPlanningHold -> append PlannedHold state
        | AddPlanningEngagement(target, capability) ->
            let updated = append (PlannedEngagement(target, capability)) state
            match
                state.SelectedUnit
                |> Option.bind (fun unitId ->
                    state.Roster
                    |> Array.tryFind (fun unit -> unit.UnitId = unitId))
            with
            | Some unit when not (Array.contains capability unit.CapabilityIds) ->
                { updated with
                    Issues =
                        [| { Code = "SIR.PLAN.CAPABILITY.NOT_IN_LOADOUT"
                             CommandId = updated.SelectedCommand
                             UnitId = Some unit.UnitId
                             Detail =
                                 capability
                                 + " is not present in "
                                 + unit.Name
                                 + "'s explicit loadout." } |]
                    FocusedIssue = Some 0 }
            | _ -> updated
        | AddPlanningSynchronization(marker, deadline) ->
            append (PlannedSynchronization(marker, deadline)) state
        | RemoveSelectedPlanningCommand ->
            match state.SelectedCommand with
            | Some id ->
                { edit (List.filter (fun command -> command.Id <> id)) state with SelectedCommand = None }
            | None -> state
        | UndoPlanning ->
            match state.Past with
            | previous :: remaining ->
                { state with
                    Commands = previous.Commands
                    Revision = previous.Revision
                    Digest = previous.Digest
                    Past = remaining
                    Future = snapshot state :: state.Future
                    SelectedCommand = None
                    Predicted = None
                    AcceptedRevision = None }
            | [] -> state
        | RedoPlanning ->
            match state.Future with
            | next :: remaining ->
                { state with
                    Commands = next.Commands
                    Revision = next.Revision
                    Digest = next.Digest
                    Past = snapshot state :: state.Past
                    Future = remaining
                    SelectedCommand = None
                    Predicted = None
                    AcceptedRevision = None }
            | [] -> state
        | FocusPlanningIssue index when index >= 0 && index < state.Issues.Length ->
            let issue = state.Issues[index]
            { state with
                FocusedIssue = Some index
                SelectedUnit = issue.UnitId |> Option.orElse state.SelectedUnit
                SelectedCommand = issue.CommandId |> Option.orElse state.SelectedCommand }
        | FocusPlanningIssue _ -> state

    let correlation tick (state: PlanningWorkspaceState) =
        { Operation = state.NextOperation
          Session = state.SessionId
          MapRevision = state.MapRevision
          PlanRevision = state.Revision
          Tick = tick }

    let planTransport (state: PlanningWorkspaceState) =
        let loadouts =
            state.Roster
            |> Array.sortBy _.UnitId
            |> Array.map (fun unit ->
                "loadout|"
                + string unit.UnitId
                + "|"
                + unit.Role
                + "|"
                + String.concat "," unit.Equipment
                + "|"
                + String.concat "," unit.CapabilityIds)
            |> String.concat "\n"

        let document =
            "SIR-PLAN 1\n"
            + "workspace|"
            + state.Digest
            + "|"
            + string state.Revision
            + "\n"
            + loadouts
            + (if String.IsNullOrEmpty loadouts then "" else "\n")
            + canonicalText state.Commands

        { EncodedDocument = Encoding.UTF8.GetBytes document
          HorizonTicks = SimulatorProtocol.MaximumHorizonTicks
          PreviewLabel = IntentOnlyPreview
          Assumptions = [||]
          Intents = state.Commands |> List.map commandText |> List.toArray }

    let receive (envelope: SimulatorResponseEnvelope) (state: PlanningWorkspaceState) =
        let advanced =
            { state with
                NextOperation = max state.NextOperation (envelope.Correlation.Operation + 1)
                WorkerStatus = "Worker responded at tick " + string envelope.CurrentTick }

        match envelope.Response with
        | SessionInitialized _ -> { advanced with WorkerStatus = "Planning worker ready" }
        | PlanValidated(accepted, diagnostics) ->
            { advanced with
                AcceptedRevision = accepted
                Issues =
                    diagnostics
                    |> Array.map (fun issue ->
                        { Code = issue.Code
                          CommandId = issue.CommandId
                          UnitId = None
                          Detail = issue.Detail })
                FocusedIssue = None
                WorkerStatus =
                    if diagnostics.Length = 0 then "Revision accepted by worker validation"
                    else string diagnostics.Length + " validation issues" }
        | PlanPreviewed(label, disclosures, _) ->
            { advanced with
                Predicted =
                    Some
                        { Revision = envelope.Correlation.PlanRevision
                          Label = label
                          Disclosures = disclosures }
                WorkerStatus = "Intent-only prediction ready" }
        | PlanCommitted revision ->
            { advanced with
                CommittedRevision = Some revision
                CommittedTick = Some envelope.CurrentTick
                WorkerStatus = "Plan committed to simulator session" }
        | SimulatorStepped _
        | SimulatorRunCompleted _
        | SimulatorReset _ ->
            { advanced with CommittedTick = Some envelope.CurrentTick }
        | SimulatorProgress(_, _) ->
            { advanced with
                CommittedTick = Some envelope.CurrentTick
                WorkerStatus = "Committed execution progressing" }
        | SimulatorOperationCancelled _ ->
            { advanced with WorkerStatus = "Worker operation cancelled" }
        | SimulatorRequestRejected(code, detail) ->
            { advanced with
                Issues =
                    [| { Code = code
                         CommandId = None
                         UnitId = None
                         Detail = detail } |]
                FocusedIssue = Some 0
                WorkerStatus = "Worker rejected request" }

    let acceptsResponse (envelope: SimulatorResponseEnvelope) (state: PlanningWorkspaceState) =
        envelope.Correlation.Session = state.SessionId
        && envelope.Correlation.MapRevision = state.MapRevision
        && envelope.Correlation.PlanRevision = state.Revision

    let reviewArtifact (state: PlanningWorkspaceState) =
        let loadouts =
            state.Roster
            |> Array.sortBy _.UnitId
            |> Array.map (fun unit ->
                "loadout|"
                + string unit.UnitId
                + "|"
                + unit.Role
                + "|"
                + String.concat "," unit.Equipment
                + "|"
                + String.concat "," unit.CapabilityIds)

        String.concat
            "\n"
            ([ "SIR-PLANNING-REVIEW 1"
               ("map|" + state.MapRevision)
               ("authored|" + string state.Revision + "|" + state.Digest)
               ("predicted|"
                + (state.Predicted
                   |> Option.map (fun value -> string value.Revision + "|" + string value.Label)
                   |> Option.defaultValue "-"))
               ("accepted|" + (state.AcceptedRevision |> Option.map string |> Option.defaultValue "-"))
               ("committed|"
                + (match state.CommittedRevision, state.CommittedTick with
                   | Some revision, Some tick -> string revision + "|" + string tick
                   | _ -> "-"))
               ("conflicts|" + string state.Issues.Length) ]
             @ Array.toList loadouts
             @ [ canonicalText state.Commands ])
        + "\n"
