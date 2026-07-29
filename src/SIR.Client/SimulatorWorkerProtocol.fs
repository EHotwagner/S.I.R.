namespace SIR.Client

/// Identity carried by every simulator request and response. A response is
/// applicable only when all five values still describe the active workspace.
type SimulatorCorrelation =
    { Operation: int32
      Session: string
      MapRevision: string
      PlanRevision: int64
      Tick: int32 }

type SimulatorPreviewLabel =
    | DeterministicPreview
    | AssumptionBasedPreview
    | IntentOnlyPreview

type SimulatorInitializationTransport =
    { InitialProjection: InspectionProjectionTransport
      MaximumHorizonTicks: int32 }

type SimulatorPlanTransport =
    { EncodedDocument: byte array
      HorizonTicks: int32
      PreviewLabel: SimulatorPreviewLabel
      Assumptions: string array
      Intents: string array }

type SimulatorDiagnosticTransport =
    { Code: string
      Field: string option
      CommandId: string option
      Fields: StringEntry array
      Detail: string }

and StringEntry =
    { Key: string
      Value: string }

/// Snapshot/delta transport reuses the bounded inspection projection schema.
/// Deltas contain only fields changed at their tick; empty arrays mean no
/// disclosed change, never "copy the hidden state".
type SimulatorProjectionUpdateTransport =
    { IsSnapshot: bool
      Projection: InspectionProjectionTransport }

type SimulatorRequest =
    | InitializeSession of SimulatorInitializationTransport
    | ValidatePlan of SimulatorPlanTransport
    | PreviewPlan of
        plan: SimulatorPlanTransport *
        fromTick: int32 *
        toTick: int32
    | CommitPlan of SimulatorPlanTransport
    | Step of tickCount: int32
    | RunTo of targetTick: int32
    | Reset
    | CancelOperation of targetOperation: int32

type SimulatorResponse =
    | SessionInitialized of SimulatorProjectionUpdateTransport
    | PlanValidated of
        acceptedRevision: int64 option *
        SimulatorDiagnosticTransport array
    | PlanPreviewed of
        label: SimulatorPreviewLabel *
        disclosures: string array *
        SimulatorProjectionUpdateTransport array
    | PlanCommitted of acceptedRevision: int64
    | SimulatorStepped of SimulatorProjectionUpdateTransport
    | SimulatorProgress of
        completedBatches: int32 *
        SimulatorProjectionUpdateTransport
    | SimulatorRunCompleted of SimulatorProjectionUpdateTransport
    | SimulatorReset of SimulatorProjectionUpdateTransport
    | SimulatorOperationCancelled of targetOperation: int32
    | SimulatorRequestRejected of code: string * detail: string

type SimulatorRequestEnvelope =
    { Kind: string
      ProtocolVersion: int32
      Correlation: SimulatorCorrelation
      Request: SimulatorRequest }

type SimulatorResponseEnvelope =
    { Kind: string
      ProtocolVersion: int32
      Correlation: SimulatorCorrelation
      CurrentTick: int32
      Response: SimulatorResponse }

type SimulatorWorkspaceGuard =
    { Active: SimulatorCorrelation option
      PendingOperations: Set<int32> }

[<RequireQualifiedAccess>]
module SimulatorProtocol =
    [<Literal>]
    let Kind = "sir-simulator-session"

    [<Literal>]
    let CurrentVersion = 1

    [<Literal>]
    let BatchSize = 256

    [<Literal>]
    let MaximumPlanBytes = 262_144

    [<Literal>]
    let MaximumHorizonTicks = 6_000

    [<Literal>]
    let MaximumPreviewTicks = 1_200

    [<Literal>]
    let MaximumProjectionMessages = 24

    let batchEnds startTick targetTick =
        [| let mutable tick = startTick
           while tick < targetTick do
               tick <- min targetTick (tick + int32 BatchSize)
               yield tick |]

    let diagnostics maximumHorizon (plan: SimulatorPlanTransport) =
        let header =
            [| 0x53uy; 0x49uy; 0x52uy; 0x2duy; 0x50uy
               0x4cuy; 0x41uy; 0x4euy; 0x20uy; 0x31uy |]

        [| if plan.EncodedDocument.Length = 0 then
               yield
                   { Code = "SIR.SIMULATOR.PLAN.EMPTY"
                     Field = Some "EncodedDocument"
                     CommandId = None
                     Fields = [||]
                     Detail = "The canonical SIR-PLAN document is empty." }
           elif
               plan.EncodedDocument.Length <= header.Length
               || plan.EncodedDocument[0 .. header.Length - 1] <> header
               || plan.EncodedDocument[header.Length] <> 0x0auy
               || plan.EncodedDocument[plan.EncodedDocument.Length - 1] <> 0x0auy
           then
               yield
                   { Code = "SIR.PLAN.STRUCTURAL.BAD_HEADER"
                     Field = Some "EncodedDocument"
                     CommandId = None
                     Fields = [||]
                     Detail = "The plan is not a canonical SIR-PLAN 1 line document." }
           if plan.EncodedDocument.Length > MaximumPlanBytes then
               yield
                   { Code = "SIR.SIMULATOR.PLAN.SIZE"
                     Field = Some "EncodedDocument"
                     CommandId = None
                     Fields = [||]
                     Detail = "The canonical SIR-PLAN document exceeds the worker limit." }
           if plan.HorizonTicks <= 0 || plan.HorizonTicks > maximumHorizon then
               yield
                   { Code = "SIR.SIMULATOR.PLAN.HORIZON"
                     Field = Some "HorizonTicks"
                     CommandId = None
                     Fields = [||]
                     Detail = "The planning horizon is outside the initialized session limit." }
           match plan.PreviewLabel with
           | DeterministicPreview when plan.Assumptions.Length <> 0 ->
               yield
                   { Code = "SIR.SIMULATOR.PREVIEW.DETERMINISTIC_ASSUMPTIONS"
                     Field = Some "Assumptions"
                     CommandId = None
                     Fields = [||]
                     Detail = "A deterministic preview cannot carry assumptions." }
           | AssumptionBasedPreview when plan.Assumptions.Length = 0 ->
               yield
                   { Code = "SIR.SIMULATOR.PREVIEW.ASSUMPTIONS_REQUIRED"
                     Field = Some "Assumptions"
                     CommandId = None
                     Fields = [||]
                     Detail = "An assumption-based preview must disclose its assumptions." }
           | IntentOnlyPreview when plan.Intents.Length = 0 ->
               yield
                   { Code = "SIR.SIMULATOR.PREVIEW.INTENTS_REQUIRED"
                     Field = Some "Intents"
                     CommandId = None
                     Fields = [||]
                     Detail = "An intent-only preview must disclose at least one intent." }
           | _ -> () |]

    let activate correlation =
        { Active = Some correlation
          PendingOperations = Set.singleton correlation.Operation }

    let beginOperation correlation guard =
        { guard with
            Active = Some correlation
            PendingOperations =
                Set.add correlation.Operation guard.PendingOperations }

    let completeOperation operation guard =
        { guard with
            PendingOperations = Set.remove operation guard.PendingOperations }

    /// The browser applies this before dispatching into workspace state.
    let accepts (envelope: SimulatorResponseEnvelope) guard =
        envelope.Kind = Kind
        && envelope.ProtocolVersion = CurrentVersion
        && Set.contains envelope.Correlation.Operation guard.PendingOperations
        && (guard.Active
            |> Option.exists (fun active ->
                active.Operation = envelope.Correlation.Operation
                && active.Session = envelope.Correlation.Session
                && active.MapRevision = envelope.Correlation.MapRevision
                && active.PlanRevision = envelope.Correlation.PlanRevision
                && active.Tick = envelope.Correlation.Tick))
