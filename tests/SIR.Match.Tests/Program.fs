module SIR.Match.Tests

open SIR.Domain
open SIR.Match
open SIR.Simulation
open SIR.ControlAbi
open Wasmtime
open System
open System.Diagnostics

let private require condition message =
    if not condition then failwith message

let private mutationEnabled name =
    String.Equals(Environment.GetEnvironmentVariable name, "1", StringComparison.Ordinal)

let mutable private enforceProductPerformanceBudgets = true

let private containsSubsequence (needle: byte array) (haystack: byte array) =
    if needle.Length = 0 then
        true
    else
        [ 0 .. haystack.Length - needle.Length ]
        |> List.exists (fun offset ->
            haystack[offset .. offset + needle.Length - 1] = needle)

let private tacticalEnvironmentEvidence () =
    let plot, variants = SIR.Simulation.TacticalEnvironment.exteriorParcelSet
    let assemble seed =
        SIR.Simulation.TacticalEnvironment.assemble seed plot variants
        |> Result.defaultWith (fun findings -> failwithf "Tactical environment did not assemble: %A" findings)

    let first = assemble 0x186UL
    let replay = assemble 0x186UL
    let unicodeRole = "rôle-漢-😀"
    let unicodePlot =
        { plot with
            AuthoredPlotId = "extérieur-漢-😀"
            PlotSlots =
                plot.PlotSlots
                |> List.map (fun slot ->
                    { slot with
                        PlotSlotId = "slot-ä-😀"
                        PlotSlotRole = unicodeRole }) }
    let unicodeVariants =
        variants
        |> List.map (fun variant ->
            { variant with
                ParcelVariantId = "variante-漢-😀"
                ParcelRole = unicodeRole
                ParcelFeatures =
                    variant.ParcelFeatures
                    |> List.mapi (fun index feature ->
                        { feature with
                            EnvironmentFeatureId = sprintf "élément-漢-😀-%d" index
                            QueryDependencyKeys = [ sprintf "dépendance-漢-😀-%d" index ] }) })
    let unicodeEnvironment =
        SIR.Simulation.TacticalEnvironment.assemble 0x186UL unicodePlot unicodeVariants
        |> Result.defaultWith (fun findings -> failwithf "Unicode tactical identity fixture failed: %A" findings)
    require
        (unicodeEnvironment.EnvironmentAssemblyIdentity = "c214e1d82c8a33f30cf6218be3744f1e1834bbef547d387d98a8740618b99ca9")
        "The byte-array writer diverged from the legacy schema-v1 UTF-8 authored-input grammar."
    let firstBytes = SIR.Domain.TacticalEnvironment.canonicalBytes first
    let replayBytes = SIR.Domain.TacticalEnvironment.canonicalBytes replay
    printfn "Exterior authored-input identity: %s." first.EnvironmentAssemblyIdentity
    require
        (firstBytes = replayBytes)
        "Equal tactical-environment seeds did not produce byte-identical canonical environments."
    require
        (first.EnvironmentAssemblyIdentity = "4e32081ea4a1fa44c4e04ef8ba1bc99d5efba22fc3766f1a9cdb6af95e5a1263"
         && first.EnvironmentContentIdentity = replay.EnvironmentContentIdentity
         && SIR.Domain.TacticalEnvironment.identityMatches first)
        "Deterministic tactical-environment authored/content identity was not byte-for-byte stable."

    let modalityStates =
        [ EnvironmentFeatureKind.Door, EnvironmentFeatureState.Closed,
          (false, false, false, false, false, true, true)
          EnvironmentFeatureKind.Door, EnvironmentFeatureState.Open,
          (true, true, true, true, true, false, true)
          EnvironmentFeatureKind.Window, EnvironmentFeatureState.Closed,
          (false, false, false, false, true, true, true)
          EnvironmentFeatureKind.Wall, EnvironmentFeatureState.Intact,
          (false, false, false, false, false, true, true)
          EnvironmentFeatureKind.Cover, EnvironmentFeatureState.Destroyed,
          (true, true, true, true, true, false, false) ]
    for kind, state, expected in modalityStates do
        let permeability = SIR.Simulation.TacticalEnvironment.defaultPermeability kind state
        let actual =
            permeability.AllowsMovement,
            permeability.AllowsSight,
            permeability.AllowsProjectile,
            permeability.AllowsAreaEffect,
            permeability.AllowsSound,
            permeability.ProvidesCover,
            permeability.AllowsInteraction
        require (actual = expected) (sprintf "Every-modality contract diverged for %A/%A: %A." kind state actual)

    let invalidPlot = { plot with PlotSchemaVersion = 999 }
    let invalidVariant =
        { variants.Head with
            ParcelObjectiveCells = [ { EnvironmentColumn = 7; EnvironmentRow = 7 } ]
            ParcelWalkableCells = [ { EnvironmentColumn = 0; EnvironmentRow = 0 } ]
            ParcelFeatures =
                variants.Head.ParcelFeatures
                |> List.map (fun feature ->
                    { feature with
                        ModalityPermeability =
                            { feature.ModalityPermeability with AllowsMovement = true } }) }
    let validationCodes =
        SIR.Simulation.TacticalEnvironment.validate invalidPlot [ invalidVariant ]
        |> List.map _.ValidationCode
        |> Set.ofList
    require
        (Set.contains EnvironmentValidationCode.InvalidSchema validationCodes
         && Set.contains EnvironmentValidationCode.BlockedObjective validationCodes
         && Set.contains EnvironmentValidationCode.InvalidPermeability validationCodes)
        "Tactical-environment validation did not report independent schema, objective, and permeability categories."

    let disconnectedPlot =
        { plot with
            AuthoredPlotId = "disconnected-required-route"
            PlotWidth = 16
            PlotSlots =
                [ plot.PlotSlots.Head
                  { plot.PlotSlots.Head with
                      PlotSlotId = "slot-2"
                      PlotSlotOrigin = { EnvironmentColumn = 8; EnvironmentRow = 0 } } ] }
    let disconnectedCodes =
        SIR.Simulation.TacticalEnvironment.validate disconnectedPlot variants
        |> List.map _.ValidationCode
        |> Set.ofList
    require
        (Set.contains EnvironmentValidationCode.DisconnectedSlot disconnectedCodes)
        "Disconnected required-route slots were accepted."

    let connectorPlot =
        { disconnectedPlot with
            AuthoredPlotId = "misaligned-connectors"
            PlotSlots =
                [ { disconnectedPlot.PlotSlots[0] with ConnectedPlotSlotIds = [ "slot-2" ] }
                  { disconnectedPlot.PlotSlots[1] with ConnectedPlotSlotIds = [ "slot-1" ] } ] }
    let connectorVariant =
        { variants.Head with
            ParcelConnections =
                [ { ConnectionId = "wrong-facing-role"
                    ConnectionCell = { EnvironmentColumn = 0; EnvironmentRow = 0 }
                    ConnectionDirection = EnvironmentEdgeDirection.South
                    ConnectionRole = "unmatched-role" } ] }
    let connectorCodes =
        SIR.Simulation.TacticalEnvironment.validate connectorPlot [ connectorVariant ]
        |> List.map _.ValidationCode
        |> Set.ofList
    require
        (Set.contains EnvironmentValidationCode.ConnectorMismatch connectorCodes)
        "Transform/role-misaligned connected parcels were accepted."

    let unusedCatalogVariant = { variants.Head with ParcelVariantId = "unused-catalog-variant" }
    let sameSelectionWithExtraCatalog =
        [ 0UL .. 128UL ]
        |> List.tryPick (fun seed ->
            match SIR.Simulation.TacticalEnvironment.assemble seed plot [ variants.Head; unusedCatalogVariant ] with
            | Ok value when value.ParcelPlacements.Head.PlacementVariantId = variants.Head.ParcelVariantId -> Some(seed, value)
            | _ -> None)
        |> Option.defaultWith (fun () -> failwith "Could not find a stable selected parcel for the catalog-identity fixture.")
    let catalogSeed, expandedCatalog = sameSelectionWithExtraCatalog
    let originalCatalog = assemble catalogSeed
    require
        (originalCatalog.ParcelPlacements = expandedCatalog.ParcelPlacements
         && originalCatalog.EnvironmentContentIdentity <> expandedCatalog.EnvironmentContentIdentity)
        "Catalog/input identity did not change when an unused authored parcel changed."

    let knowledge =
        { EnvironmentKnowledgeIdentity = "test-observer"
          EnvironmentKnowledgeRevision = 3L
          KnownEnvironmentFeatureIds = Set [ "slot-1:yard-door" ]
          KnownEnvironmentStateFeatureIds = Set [ "slot-1:yard-door" ]
          KnownEnvironmentFacts = Set.empty }
    require
        ((SIR.Simulation.TacticalEnvironment.observe knowledge first "slot-1:yard-cover").IsNone)
        "Environment observation disclosed a feature outside requester knowledge."
    let observed =
        SIR.Simulation.TacticalEnvironment.observe knowledge first "slot-1:yard-door"
        |> Option.defaultWith (fun () ->
            failwithf
                "Known tactical feature was not observable; assembled ids were %A."
                (first.EnvironmentFeatures |> List.map _.EnvironmentFeatureId))
    require
        (observed.ObservedState = Some EnvironmentFeatureState.Closed
         && observed.ObservationKnowledgeIdentity = knowledge.EnvironmentKnowledgeIdentity)
        "Environment observation did not retain disclosed state and knowledge provenance."
    let closedDoorFeature =
        first.EnvironmentFeatures
        |> List.find (fun feature -> feature.EnvironmentFeatureId = "slot-1:yard-door")
    let modalityAdapterResults =
        [ EnvironmentModality.Movement
          EnvironmentModality.Sight
          EnvironmentModality.Projectile
          EnvironmentModality.AreaEffect
          EnvironmentModality.Sound
          EnvironmentModality.Cover
          EnvironmentModality.Interaction "open-door"
          EnvironmentModality.Interaction "missing-capability" ]
        |> List.map (fun modality -> SIR.Simulation.TacticalEnvironment.allowsModality modality closedDoorFeature)
    require
        (modalityAdapterResults = [ false; false; false; false; false; true; true; false ])
        "The every-modality adapter did not distinguish permeability, cover, and named interaction capability."
    let authoredCoverFeature =
        first.EnvironmentFeatures
        |> List.find (fun feature -> feature.EnvironmentFeatureId = "slot-1:yard-cover")
    require
        ((SIR.Simulation.TacticalEnvironment.coverAt Direction8.North authoredCoverFeature).IsSome
         && (SIR.Simulation.TacticalEnvironment.coverAt Direction8.South authoredCoverFeature).IsNone)
        "Directional cover adapter did not respect authored protected directions."
    let partialStateObservation =
        SIR.Simulation.TacticalEnvironment.observe
            { knowledge with KnownEnvironmentStateFeatureIds = Set.empty }
            first
            "slot-1:yard-door"
        |> Option.get
    require
        (partialStateObservation.ObservedState.IsNone
         && not partialStateObservation.AvailableCapabilities.IsEmpty)
        "Partial feature knowledge did not redact state independently of known interaction capabilities."

    let gatedCapability =
        { DescriptorId = "door-secret"
          DescriptorAction = "breach"
          DescriptorCost = 2
          RequiredKnowledgeFact = Some "fact:breach-training" }
    let statefulVariant =
        { variants.Head with
            ParcelFeatures =
                variants.Head.ParcelFeatures
                |> List.map (fun feature ->
                    if feature.EnvironmentFeatureId = "yard-door" then
                        { feature with
                            CapabilityDescriptors =
                                feature.CapabilityDescriptors
                                @ [ { DescriptorId = "close-door"; DescriptorAction = "close"; DescriptorCost = 1; RequiredKnowledgeFact = None }
                                    gatedCapability ] }
                    else feature) }
    let statefulEnvironment =
        SIR.Simulation.TacticalEnvironment.assemble 0x186UL plot [ statefulVariant ]
        |> Result.defaultWith (fun findings -> failwithf "Stateful capability fixture failed: %A" findings)
    let statefulFeatureId = "slot-1:yard-door"
    let statefulKnowledge =
        { knowledge with KnownEnvironmentFeatureIds = Set [ statefulFeatureId ] }
    let partialCapabilities =
        SIR.Simulation.TacticalEnvironment.observe statefulKnowledge statefulEnvironment statefulFeatureId
        |> Option.get
        |> _.AvailableCapabilities
        |> List.map _.DescriptorId
        |> Set.ofList
    require
        (not (Set.contains gatedCapability.DescriptorId partialCapabilities))
        "Partial knowledge exposed a capability whose fact was not known."
    let fullCapabilities =
        SIR.Simulation.TacticalEnvironment.observe
            { statefulKnowledge with KnownEnvironmentFacts = Set [ "fact:breach-training" ] }
            statefulEnvironment
            statefulFeatureId
        |> Option.get
        |> _.AvailableCapabilities
        |> List.map _.DescriptorId
        |> Set.ofList
    require
        (Set.contains gatedCapability.DescriptorId fullCapabilities)
        "Supplying the required knowledge fact did not disclose its capability."

    let world = SIR.Simulation.TacticalEnvironment.toSpatialWorld "test-rules@1" knowledge first
    let door = first.EnvironmentFeatures |> List.find (fun feature -> feature.EnvironmentFeatureId = "slot-1:yard-door")
    let doorOrigin : FS.GG.Game.Core.Cell = { Col = door.EnvironmentEdge.EdgeCell.EnvironmentColumn; Row = door.EnvironmentEdge.EdgeCell.EnvironmentRow }
    let doorTarget =
        match door.EnvironmentEdge.EdgeDirection with
        | EnvironmentEdgeDirection.East -> { doorOrigin with Col = doorOrigin.Col + 1 }
        | EnvironmentEdgeDirection.South -> { doorOrigin with Row = doorOrigin.Row + 1 }
    let request =
        { QueryId = "door-crossing"
          QueryKind = SpatialQueryKind.ExactLineOfSight
          Origin = doorOrigin
          Target = doorTarget
          Footprint = [ { Col = 0; Row = 0 } ]
          Profile =
            { ProfileId = "ground"
              Modality = SpatialModality.GroundMovement
              Stance = "standing"
              HeightBand = 1
              Facing = Direction8.East }
          Bounds = SpatialQuery.defaultBounds }
    let closedResult, populatedCache, source = SpatialQuery.evaluateCached SpatialQuery.emptyCache world request
    require
        (source = SpatialEvaluationSource.Uncached
         && not closedResult.Visible
         && populatedCache.DynamicEntries.Length = 1)
        "Closed door did not produce an uncached, dependency-tracked blocked sight line."

    let coverFeature = first.EnvironmentFeatures |> List.find (fun feature -> feature.EnvironmentFeatureId = "slot-1:yard-cover")
    let coverOrigin : FS.GG.Game.Core.Cell = { Col = coverFeature.EnvironmentEdge.EdgeCell.EnvironmentColumn; Row = coverFeature.EnvironmentEdge.EdgeCell.EnvironmentRow }
    let coverTarget =
        match coverFeature.EnvironmentEdge.EdgeDirection with
        | EnvironmentEdgeDirection.East -> { coverOrigin with Col = coverOrigin.Col + 1 }
        | EnvironmentEdgeDirection.South -> { coverOrigin with Row = coverOrigin.Row + 1 }
    let coverRequest = { request with QueryId = "cover-crossing"; Origin = coverOrigin; Target = coverTarget }
    let _, localityCache, _ = SpatialQuery.evaluateCached populatedCache world coverRequest
    require (localityCache.DynamicEntries.Length = 2) "Local invalidation fixture did not retain two independent dependency receipts."

    let opened =
        SIR.Simulation.TacticalEnvironment.applyAction
            knowledge
            first.EnvironmentContentIdentity
            "slot-1:yard-door"
            EnvironmentAction.Open
            first
        |> Result.defaultWith (fun failure -> failwithf "Known door could not be opened: %A" failure)
    require
        (opened.UpdatedEnvironment.EnvironmentSpatialRevision = first.EnvironmentSpatialRevision + 1L
         && opened.ActionCostCounters.FeaturesInspected = 1
         && opened.ActionCostCounters.FeaturesChanged = 1
         && opened.ActionCostCounters.PropagatedChanges = 0
         && opened.ChangedQueryDependencies = Set [ "feature:yard-door:slot-1:yard-door" ])
        "Door transition did not emit bounded work and exact spatial dependencies."
    let openedStateful =
        SIR.Simulation.TacticalEnvironment.applyAction
            statefulKnowledge
            statefulEnvironment.EnvironmentContentIdentity
            statefulFeatureId
            EnvironmentAction.Open
            statefulEnvironment
        |> Result.defaultWith (fun failure -> failwithf "Stateful door could not be opened: %A" failure)
    let currentCapabilityActions =
        SIR.Simulation.TacticalEnvironment.observe statefulKnowledge openedStateful.UpdatedEnvironment statefulFeatureId
        |> Option.get
        |> _.AvailableCapabilities
        |> List.map _.DescriptorAction
        |> Set.ofList
    require
        (Set.contains "close" currentCapabilityActions
         && not (Set.contains "open" currentCapabilityActions))
        "Opened feature did not expose only its current state-specific Close capability."
    match
        SIR.Simulation.TacticalEnvironment.applyAction
            knowledge
            first.EnvironmentContentIdentity
            "slot-1:yard-door"
            EnvironmentAction.Close
            opened.UpdatedEnvironment
    with
    | Error EnvironmentActionFailure.StaleContentIdentity -> ()
    | result -> failwithf "Stale tactical content identity was not rejected: %A" result

    let invalidated, inspected, removed =
        SIR.Simulation.TacticalEnvironment.invalidateCache opened localityCache
    require
        (inspected = 2 && removed = 1 && invalidated.DynamicEntries.Length = 1)
        "Door transition did not selectively invalidate its dependent spatial query."
    let openedWorld =
        SIR.Simulation.TacticalEnvironment.toSpatialWorld "test-rules@1" knowledge opened.UpdatedEnvironment
    let openResult, _, _ = SpatialQuery.evaluateCached invalidated openedWorld request
    require
        (openResult.Outcome = SpatialOutcome.Found && openResult.Visible)
        "Opening the door did not change the authoritative sight-line result."
    let _, retainedAfterChange, retainedSource =
        SpatialQuery.evaluateCached invalidated openedWorld coverRequest
    require
        (retainedSource = SpatialEvaluationSource.Cached
         && retainedAfterChange.DynamicEntries.Length = 1)
        "An unrelated dependency receipt did not remain a cache hit after a local feature change."

    let dependencySaturatedCache =
        [ 0 .. 255 ]
        |> List.fold (fun cache index ->
            let saturatedRequest = { request with QueryId = sprintf "door-receipt-%03d" index }
            let _, next, _ = SpatialQuery.evaluateCached cache world saturatedRequest
            next) SpatialQuery.emptyCache
    require
        (dependencySaturatedCache.DynamicEntries.Length = 256)
        "The exact 256-receipt invalidation fixture was not populated."
    SIR.Simulation.TacticalEnvironment.invalidateCache opened dependencySaturatedCache |> ignore
    let invalidationClock = Stopwatch.StartNew()
    let saturatedInvalidated, saturatedInspected, saturatedRemoved =
        SIR.Simulation.TacticalEnvironment.invalidateCache opened dependencySaturatedCache
    invalidationClock.Stop()
    require
        (saturatedInspected = 256
         && saturatedRemoved = 256
         && saturatedInvalidated.DynamicEntries.IsEmpty
         && (not enforceProductPerformanceBudgets || invalidationClock.Elapsed.TotalMilliseconds < 10.0))
        "Exact 256-receipt invalidation did not inspect/remove every dependent entry within 10 ms."

    let coverKnowledge =
        { knowledge with KnownEnvironmentFeatureIds = Set [ "slot-1:yard-cover" ] }
    let firstDamage =
        SIR.Simulation.TacticalEnvironment.applyAction
            coverKnowledge
            first.EnvironmentContentIdentity
            "slot-1:yard-cover"
            (EnvironmentAction.Damage 60)
            first
        |> Result.defaultWith (fun failure -> failwithf "First cumulative cover hit failed: %A" failure)
    let damagedCover =
        firstDamage.UpdatedEnvironment.EnvironmentFeatures
        |> List.find (fun feature -> feature.EnvironmentFeatureId = "slot-1:yard-cover")
        |> _.DirectionalCover
        |> Option.get
    require
        (damagedCover.CoverIntegrity = 40
         && damagedCover.CoverMaterial = "sandbag"
         && damagedCover.CoverPenetrationResistance = 40
         && damagedCover.CoverProtectedDirections = [ Direction8.North ]
         && firstDamage.UpdatedEnvironment.EnvironmentSpatialRevision = 1L)
        "First cover hit did not consume integrity while retaining material, penetration, direction, and revision evidence."
    let secondDamage =
        SIR.Simulation.TacticalEnvironment.applyAction
            coverKnowledge
            firstDamage.UpdatedEnvironment.EnvironmentContentIdentity
            "slot-1:yard-cover"
            (EnvironmentAction.Damage 60)
            firstDamage.UpdatedEnvironment
        |> Result.defaultWith (fun failure -> failwithf "Second cumulative cover hit failed: %A" failure)
    let destroyedCoverFeature =
        secondDamage.UpdatedEnvironment.EnvironmentFeatures
        |> List.find (fun feature -> feature.EnvironmentFeatureId = "slot-1:yard-cover")
    require
        (destroyedCoverFeature.EnvironmentState = EnvironmentFeatureState.Destroyed
         && destroyedCoverFeature.DirectionalCover |> Option.exists (fun cover -> cover.CoverIntegrity = 0)
         && secondDamage.UpdatedEnvironment.EnvironmentSpatialRevision = 2L
         && secondDamage.UpdatedEnvironment.EnvironmentContentIdentity <> firstDamage.UpdatedEnvironment.EnvironmentContentIdentity)
        "Repeated damage did not accumulate to destruction with a second revision and identity."
    require
        ((SIR.Simulation.TacticalEnvironment.coverAt Direction8.North destroyedCoverFeature).IsNone
         && SIR.Simulation.TacticalEnvironment.allowsModality EnvironmentModality.Movement destroyedCoverFeature
         && not (SIR.Simulation.TacticalEnvironment.allowsModality EnvironmentModality.Cover destroyedCoverFeature))
        "Destroyed cover remained directional cover or failed to open movement permeability."

    let clock = Stopwatch.StartNew()
    let counters =
        SIR.Simulation.TacticalEnvironment.workload 0x186UL plot variants populatedCache
        |> Result.defaultWith (fun findings -> failwithf "Tactical workload failed: %A" findings)
    clock.Stop()
    let elapsed = clock.Elapsed.TotalMilliseconds
    printfn "Representative tactical workload counters: %A in %.3f ms." counters elapsed
    require
        (counters.WorkloadSlots <= SIR.Simulation.TacticalEnvironment.maximumSlots
         && counters.WorkloadVariantsInspected <= 4 * SIR.Simulation.TacticalEnvironment.maximumVariantsPerRole
         && counters.WorkloadFindings <= SIR.Simulation.TacticalEnvironment.maximumFindings
         && counters.DependencyEntriesInspected >= populatedCache.DynamicEntries.Length
         && counters.DependencyEntriesInspected <= 256
         && counters.DependencyEntriesInvalidated > 0
         && counters.DependencyEntriesInvalidated <= counters.DependencyEntriesInspected
         && counters.WorkloadQueryCount > 0
         && counters.WorkloadQueryCount <= counters.WorkloadFeatures
         && (not enforceProductPerformanceBudgets || elapsed < 1_000.0))
        "Tactical environment workload exceeded a declared work or smoke-time bound."

    let noisyInvalidVariant =
        { variants.Head with
            ParcelFeatures =
                [ for index in 0 .. 599 ->
                    { variants.Head.ParcelFeatures.Head with
                        EnvironmentFeatureId = ""
                        QueryDependencyKeys = [ "" ] } ] }
    let validationClock = Stopwatch.StartNew()
    let boundedFindings = SIR.Simulation.TacticalEnvironment.validate plot [ noisyInvalidVariant ]
    validationClock.Stop()
    require
        (boundedFindings.Length = SIR.Simulation.TacticalEnvironment.maximumFindings
         && (not enforceProductPerformanceBudgets || validationClock.Elapsed.TotalMilliseconds < 100.0))
        "Maximum validation did not truncate exactly at 512 findings within its 100 ms smoke budget."

    let maximumPlot =
        { plot with
            AuthoredPlotId = "maximum-authored-environment"
            PlotWidth = 8
            PlotSlots =
                [ for index in 0 .. 63 ->
                    { PlotSlotId = sprintf "slot-%02d" index
                      PlotSlotRole = "maximum"
                      PlotSlotOrigin = { EnvironmentColumn = 0; EnvironmentRow = 0 }
                      PlotSlotWidth = 8
                      PlotSlotHeight = 8
                      ConnectedPlotSlotIds =
                        [ if index > 0 then sprintf "slot-%02d" (index - 1)
                          if index < 63 then sprintf "slot-%02d" (index + 1) ]
                      PlotSlotRequiresRoute = true } ] }
    let maximumVariants =
        [ for index in 0 .. 31 ->
            { variants.Head with
                ParcelVariantId = sprintf "maximum-%02d" index
                ParcelRole = "maximum"
                ParcelConnections =
                    [ { ConnectionId = "route"
                        ConnectionCell = { EnvironmentColumn = 0; EnvironmentRow = 0 }
                        ConnectionDirection = EnvironmentEdgeDirection.East
                        ConnectionRole = "maximum" } ] } ]
    SIR.Simulation.TacticalEnvironment.assemble 0x185UL maximumPlot maximumVariants
    |> Result.defaultWith (fun findings -> failwithf "Maximum tactical warmup failed: %A" findings)
    |> ignore
    let timedP80 sample =
        let measured =
            [| for _ in 1 .. 5 do
                   let clock = Stopwatch.StartNew()
                   let result = sample ()
                   clock.Stop()
                   yield clock.Elapsed.TotalMilliseconds, result |]
        let p80 = measured |> Array.map fst |> Array.sort |> fun samples -> samples[3]
        let sampleText = measured |> Array.map (fst >> sprintf "%.3f") |> String.concat ","
        p80, (measured |> Array.last |> snd), sampleText
    let maximumValidationP80, maximumValidationFindings, maximumValidationSamples =
        timedP80 (fun () ->
            SIR.Simulation.TacticalEnvironment.validate maximumPlot maximumVariants)
    require maximumValidationFindings.IsEmpty "Maximum tactical fixture did not stay valid after warmup."
    require
        (not enforceProductPerformanceBudgets || maximumValidationP80 < 50.0)
        (sprintf "Maximum 64-slot/32-variant validation exceeded its separate 50 ms p80 gate: %.3f ms; samples [%s]." maximumValidationP80 maximumValidationSamples)
    let maximumAssemblyP80, maximumEnvironment, maximumAssemblySamples =
        timedP80 (fun () ->
            SIR.Simulation.TacticalEnvironment.assemble 0x186UL maximumPlot maximumVariants
            |> Result.defaultWith (fun findings -> failwithf "Maximum tactical assembly failed: %A" findings))
    printfn "Maximum tactical assembly counters: %A; validation p80 %.3f ms [%s]; assembly including validation p80 %.3f ms [%s]." maximumEnvironment.AssemblyCostCounters maximumValidationP80 maximumValidationSamples maximumAssemblyP80 maximumAssemblySamples
    require
        (maximumEnvironment.AssemblyCostCounters.SlotsVisited = 64
         && maximumEnvironment.AssemblyCostCounters.VariantsInspected = 2_048
         && maximumEnvironment.AssemblyCostCounters.Selections = 64
         && (not enforceProductPerformanceBudgets || maximumAssemblyP80 < 25.0))
        (sprintf "Maximum 64-slot/32-variant assembly exceeded its structural or 25 ms p80 timing budget: %.3f ms; samples [%s]." maximumAssemblyP80 maximumAssemblySamples)

    let previewCells =
        [ for row in 0 .. 79 do
            for column in 0 .. 79 ->
                { EnvironmentColumn = column; EnvironmentRow = row } ]
    let previewFeatures =
        [ for index in 0 .. 2047 ->
            let cell = { EnvironmentColumn = index % 79; EnvironmentRow = index / 79 }
            { variants.Head.ParcelFeatures[1] with
                EnvironmentFeatureId = sprintf "preview-cover-%04d" index
                EnvironmentEdge = { EdgeCell = cell; EdgeDirection = EnvironmentEdgeDirection.East }
                EnvironmentFeatureCells = [ cell ]
                QueryDependencyKeys = [ sprintf "preview-cover:%04d" index ] } ]
    let previewPlot =
        { plot with
            AuthoredPlotId = "maximum-editor-preview"
            PlotWidth = 80
            PlotHeight = 80
            PlotSlots =
                [ { plot.PlotSlots.Head with
                      PlotSlotRole = "preview"
                      PlotSlotWidth = 80
                      PlotSlotHeight = 80 } ] }
    let previewVariants =
        [ { variants.Head with
              ParcelVariantId = "maximum-editor-preview"
              ParcelRole = "preview"
              ParcelWidth = 80
              ParcelHeight = 80
              ParcelWalkableCells = previewCells
              ParcelObjectiveCells = [ previewCells[previewCells.Length - 1] ]
              ParcelFeatures = previewFeatures } ]
    SIR.Simulation.TacticalEnvironment.assemble 0x186UL previewPlot previewVariants
    |> Result.defaultWith (fun findings -> failwithf "Maximum editor preview warmup failed: %A" findings)
    |> ignore
    let previewValidationP80, _, previewValidationSamples =
        timedP80 (fun () ->
            SIR.Simulation.TacticalEnvironment.validate previewPlot previewVariants)
    require
        (not enforceProductPerformanceBudgets || previewValidationP80 < 50.0)
        (sprintf "Maximum 80x80/2,048-feature validation exceeded its separate 50 ms p80 gate: %.3f ms; samples [%s]." previewValidationP80 previewValidationSamples)
    let previewSamples =
        [| for _ in 1 .. 5 do
               let allocatedBefore = GC.GetAllocatedBytesForCurrentThread()
               let clock = Stopwatch.StartNew()
               let environment =
                   SIR.Simulation.TacticalEnvironment.assemble 0x186UL previewPlot previewVariants
                   |> Result.defaultWith (fun findings -> failwithf "Maximum editor preview failed: %A" findings)
               clock.Stop()
               let allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore
               yield clock.Elapsed.TotalMilliseconds, allocated, environment |]
    let previewP80 = previewSamples |> Array.map (fun (elapsed, _, _) -> elapsed) |> Array.sort |> fun samples -> samples[3]
    let previewSampleText = previewSamples |> Array.map (fun (elapsed, _, _) -> sprintf "%.3f" elapsed) |> String.concat ","
    let previewAllocatedBytes = previewSamples |> Array.map (fun (_, allocated, _) -> allocated) |> Array.max
    let _, _, previewEnvironment = Array.last previewSamples
    eprintfn "Maximum tactical editor preview: validation p80 %.3f ms [%s]; assembly p80 %.3f ms [%s]; maximum %d allocated bytes; authored input %s." previewValidationP80 previewValidationSamples previewP80 previewSampleText previewAllocatedBytes previewEnvironment.EnvironmentAssemblyIdentity
    Console.Error.Flush()
    require
        (previewEnvironment.AssembledWalkableCells.Length = 6_400
         && previewEnvironment.EnvironmentFeatures.Length = 2_048)
        "Maximum 80x80/2,048-feature editor preview changed its 6,400-cell/2,048-feature workload."
    require
        (previewEnvironment.EnvironmentAssemblyIdentity = "51eedfe20ceb51fad17d33ddacfe68ce0e95cd7df031f1ef5e176226249e0a68")
        "Maximum 80x80/2,048-feature editor preview changed its authored identity."
    require
        (previewAllocatedBytes < 16_000_000L)
        (sprintf "Maximum 80x80/2,048-feature editor preview exceeded its 16000000-byte allocation bound: %d bytes." previewAllocatedBytes)
    require
        (not enforceProductPerformanceBudgets || previewP80 < 50.0)
        (sprintf "Maximum 80x80/2,048-feature editor preview exceeded its 50 ms p80 timing budget: %.3f ms; samples [%s]." previewP80 previewSampleText)

    let interactionFeatures =
        [ for index in 0 .. 49 ->
            let cell = { EnvironmentColumn = index; EnvironmentRow = 1 }
            { variants.Head.ParcelFeatures[1] with
                EnvironmentFeatureId = sprintf "combat-cover-%02d" index
                EnvironmentEdge = { EdgeCell = cell; EdgeDirection = EnvironmentEdgeDirection.East }
                EnvironmentFeatureCells = [ cell ]
                QueryDependencyKeys = [ sprintf "combat-cover:%02d" index ] } ]
    let interactionPlot =
        { plot with
            AuthoredPlotId = "representative-combat-environment"
            PlotWidth = 100
            PlotHeight = 2
            PlotSlots =
                [ { plot.PlotSlots.Head with
                      PlotSlotRole = "representative-combat"
                      PlotSlotWidth = 100
                      PlotSlotHeight = 2 } ] }
    let interactionVariants =
        [ { variants.Head with
              ParcelVariantId = "representative-combat-environment"
              ParcelRole = "representative-combat"
              ParcelWidth = 100
              ParcelHeight = 2
              ParcelWalkableCells =
                  [ for row in 0 .. 1 do
                      for column in 0 .. 99 ->
                          { EnvironmentColumn = column; EnvironmentRow = row } ]
              ParcelObjectiveCells = [ { EnvironmentColumn = 99; EnvironmentRow = 0 } ]
              ParcelFeatures = interactionFeatures } ]
    let interactionEnvironment =
        SIR.Simulation.TacticalEnvironment.assemble 0x186UL interactionPlot interactionVariants
        |> Result.defaultWith (fun findings -> failwithf "Representative combat environment failed: %A" findings)
    let interactionKnowledge =
        { EnvironmentKnowledgeIdentity = "representative-combat"
          EnvironmentKnowledgeRevision = 1L
          KnownEnvironmentFeatureIds = interactionEnvironment.EnvironmentFeatures |> List.map _.EnvironmentFeatureId |> Set.ofList
          KnownEnvironmentStateFeatureIds = interactionEnvironment.EnvironmentFeatures |> List.map _.EnvironmentFeatureId |> Set.ofList
          KnownEnvironmentFacts = Set.empty }
    let baseCombatUnits =
        [ for index in 0 .. 99 ->
            let id = sprintf "unit-%03d" index
            id,
            { EntityId = id
              Faction = (if index % 2 = 0 then "blue" else "red")
              Cell = { Col = index; Row = 0 }
              Facing = (if index % 2 = 0 then Direction8.East else Direction8.West)
              Health = 100
              Armor = { FrontRating = 0; RearRating = 0; Integrity = 100 }
              Wounds = []
              Incapacitated = false
              Suppression = 0 } ]
        |> Map.ofList
    let mutationObserver =
        { baseCombatUnits["unit-099"] with
            EntityId = "mutation-observer"
            Cell = { Col = 99; Row = 1 } }
    let combatUnits =
        if mutationEnabled "SIR_TACTICAL_MUTATE_REP_SOURCE_UNITS" then
            baseCombatUnits |> Map.add mutationObserver.EntityId mutationObserver
        else
            baseCombatUnits
    let initialCombatants =
        if mutationEnabled "SIR_TACTICAL_MUTATE_REP_FINAL_UNITS" then
            combatUnits |> Map.add mutationObserver.EntityId mutationObserver
        else
            combatUnits
    let combatWorld =
        { Spatial =
            SIR.Simulation.TacticalEnvironment.toSpatialWorld
                "representative-combat@1"
                interactionKnowledge
                interactionEnvironment
          Combatants = initialCombatants
          Covers = Combat.environmentCovers interactionEnvironment }
    let runInteractionBatch verifySteps =
        ((interactionEnvironment, combatWorld, Set.empty, 0, 0, 0), [ 0 .. 49 ])
        ||> List.fold (fun (current, combat, participants, propagated, queries, crossed) index ->
            let attackerId = sprintf "unit-%03d" (index * 2)
            let targetId = sprintf "unit-%03d" (index * 2 + 1)
            let featureId = sprintf "slot-1:combat-cover-%02d" index
            let environmentResult =
                SIR.Simulation.TacticalEnvironment.applyAction interactionKnowledge current.EnvironmentContentIdentity featureId EnvironmentAction.Destroy current
                |> Result.defaultWith (fun failure -> failwithf "Representative interaction %s/%s failed: %A" attackerId featureId failure)
            if verifySteps then
                require (environmentResult.ActionCostCounters.FeaturesChanged = 1 && environmentResult.ActionCostCounters.PropagatedChanges = 0) "Representative interaction changed more than its target."
            let combatResult =
                Combat.resolve
                    combat
                    { AttackId = sprintf "representative-attack-%02d" index
                      AttackerId = attackerId
                      AimCell = { Col = index * 2 + 1; Row = 0 }
                      Weapon = WeaponProfile.Rifle
                      Limits = Combat.defaultLimits }
                |> Result.defaultWith (fun rejection -> failwithf "Representative combat query %s -> %s failed: %A" attackerId targetId rejection)
            let changedTargets =
                combatResult.Facts
                |> List.choose (function CombatFact.HealthChanged(id, _, _) -> Some id | _ -> None)
            if verifySteps then
                require (changedTargets = [ targetId ]) "Representative combat query did not retain exactly one target."
            let queryCount, crossedCount =
                combatResult.SpatialEvidence
                |> Option.map (fun evidence -> 1, evidence.Explanation.CrossedCells.Length)
                |> Option.defaultValue (0, 0)
            environmentResult.UpdatedEnvironment,
            combatResult.World,
            (participants |> Set.add attackerId
             |> fun observed ->
                 if mutationEnabled "SIR_TACTICAL_MUTATE_REP_PARTICIPANTS" then observed
                 else observed |> Set.add targetId),
            propagated
            + environmentResult.ActionCostCounters.PropagatedChanges
            + (if mutationEnabled "SIR_TACTICAL_MUTATE_REP_PROPAGATED" then 1 else 0),
            queries + (if mutationEnabled "SIR_TACTICAL_MUTATE_REP_QUERIES" then 0 else queryCount),
            crossed + (if mutationEnabled "SIR_TACTICAL_MUTATE_REP_CROSSED" then 0 else crossedCount))
    runInteractionBatch true |> ignore
    // Measure product work rather than assertion/reporting overhead. A full verification
    // run immediately before and after the samples retains the per-interaction evidence.
    // Collect before each sample so unrelated qualification allocations cannot decide this
    // fixed-workload gate; collection itself remains outside the timed region.
    let interactionSamples =
        [| for _ in 1 .. 5 do
               GC.Collect()
               GC.WaitForPendingFinalizers()
               GC.Collect()
               let interactionClock = Stopwatch.StartNew()
               if mutationEnabled "SIR_TACTICAL_MUTATE_REP_TIMING" then
                   System.Threading.Thread.Sleep 60
               let result = runInteractionBatch false
               interactionClock.Stop()
               yield interactionClock.Elapsed.TotalMilliseconds, result |]
    let interactionP80 =
        interactionSamples
        |> Array.map fst
        |> Array.sort
        |> fun samples -> samples[3]
    let _, finalCombat, participants, propagated, queryCount, crossedCount =
        runInteractionBatch true
    let interactionSampleText =
        interactionSamples
        |> Array.map (fst >> sprintf "%.3f")
        |> String.concat ","
    eprintfn
        "Representative combat/spatial batch: source units %d; final units %d; participants %d; propagated changes %d; spatial queries %d; crossed cells %d; p80 %.3f ms; samples [%s]."
        combatUnits.Count
        finalCombat.Combatants.Count
        participants.Count
        propagated
        queryCount
        crossedCount
        interactionP80
        interactionSampleText
    Console.Error.Flush()
    require
        (combatUnits.Count = 100)
        "Representative combat/spatial batch changed its 100-unit source workload."
    require
        (finalCombat.Combatants.Count = 100)
        "Representative combat/spatial batch changed its 100-unit final workload."
    require
        (participants.Count = 100)
        "Representative combat/spatial batch did not retain exactly 100 participants."
    require
        (propagated = 0)
        "Representative combat/spatial batch propagated changes beyond each targeted feature."
    require
        (queryCount = 50)
        "Representative combat/spatial batch did not execute exactly 50 spatial queries."
    require
        (crossedCount > 0)
        "Representative combat/spatial batch did not traverse any spatial cells."
    require
        (not enforceProductPerformanceBudgets || interactionP80 < 50.0)
        (sprintf "Representative 100-unit/50-interaction production combat/spatial batch exceeded its 50 ms timing budget at p80: %.3f ms; samples [%s]." interactionP80 interactionSampleText)

    printfn
        "Tactical environment evidence: identity %s; closed/open path %A/%A; %d cache entry invalidated in %.3f ms; representative %.3f ms; maximum assembly %.3f ms; maximum preview %.3f ms; 100-unit/50-interaction batch %.3f ms."
        first.EnvironmentContentIdentity
        closedResult.Outcome
        openResult.Outcome
        removed
        invalidationClock.Elapsed.TotalMilliseconds
        elapsed
        maximumAssemblyP80
        previewP80
        interactionP80

let private controlAbiOutput () =
    V1Codec.encodeOutput
        42
        7
        0u
        1000u
        [ { Kind = RequestKind.Sleep
            ModuleRequestId = 9u
            Payload = [| 100uy; 0uy; 0uy; 0uy |] }
          { Kind = RequestKind.SetAttention
            ModuleRequestId = 7u
            Payload = [| 2uy |] } ]
        []
    |> Result.defaultWith (fun error -> failwithf "%A" error)

let private executeReferenceControlModule expectedOutput =
    let data =
        expectedOutput
        |> Array.map (fun value -> sprintf "\\%02x" value)
        |> String.concat ""

    let wat =
        $"""(module
          (memory (export "memory") 2)
          (data (i32.const 65536) "{data}")
          (func (export "sir_abi_version") (result i32) i32.const 65536)
          (func (export "sir_input_ptr") (result i32) i32.const 0)
          (func (export "sir_input_capacity") (result i32) i32.const 65536)
          (func (export "sir_output_ptr") (result i32) i32.const 65536)
          (func (export "sir_output_capacity") (result i32) i32.const 16384)
          (func (export "sir_decide") (param i32) (result i32)
            i32.const {expectedOutput.Length}))"""

    use engine = new Engine()
    use compiled = Module.FromText(engine, "control-abi-v1-reference", wat)
    use linker = new Linker(engine)
    use store = new Store(engine)
    let instance = linker.Instantiate(store, compiled)
    let memory =
        match instance.GetMemory("memory") with
        | null -> failwith "Reference ABI module did not export memory."
        | value -> value

    let decide =
        match instance.GetFunction("sir_decide") with
        | null -> failwith "Reference ABI module did not export sir_decide."
        | value -> value

    let input =
        { Kind = MessageKind.Input
          MinorVersion = 0uy
          Tick = 42
          UnitId = 7
          Flags = 0u
          Budget = 1000u
          Sections =
            [ { Tag = V1Constants.OwnStateTag
                Required = true
                ElementCount = 1
                Payload = [| 1uy |] } ] }
        |> V1Codec.encode
        |> Result.defaultWith (fun error -> failwithf "%A" error)

    System.ReadOnlySpan<byte>(input).CopyTo(memory.GetSpan(0L, input.Length))

    let outputLength =
        match decide.Invoke(input.Length) with
        | :? int as value -> value
        | value -> failwithf "Unexpected reference ABI result: %A" value

    memory.GetSpan(65536L, outputLength).ToArray()

let private abiInput tick unitId =
    { Kind = MessageKind.Input
      MinorVersion = V1Constants.Minor
      Tick = tick
      UnitId = unitId
      Flags = 0u
      Budget = uint32 ControlHost.defaultProfile.FuelPerInvocation
      Sections =
        [ { Tag = V1Constants.OwnStateTag
            Required = true
            ElementCount = 1
            Payload = [| 1uy |] } ] }
    |> V1Codec.encode
    |> Result.defaultWith (fun error -> failwithf "Could not encode host input: %A" error)

let private watBytes (wat: string) = Module.ConvertText wat

let private watData (bytes: byte array) =
    bytes
    |> Array.map (fun value -> sprintf "\\%02x" value)
    |> String.concat ""

let private statefulControllerWat requests =
    let output =
        V1Codec.encodeOutput 0 0 0u 0u requests []
        |> Result.defaultWith (fun error -> failwithf "Could not encode controller output: %A" error)

    $"""(module
      (memory (export "memory") 2 2)
      (global $counter (export "counter") (mut i32) (i32.const 0))
      (data (i32.const 65536) "{watData output}")
      (func (export "sir_abi_version") (result i32) i32.const 65536)
      (func (export "sir_input_ptr") (result i32) i32.const 0)
      (func (export "sir_input_capacity") (result i32) i32.const 65536)
      (func (export "sir_output_ptr") (result i32) i32.const 65536)
      (func (export "sir_output_capacity") (result i32) i32.const 16384)
      (func (export "sir_decide") (param i32) (result i32)
        global.get $counter i32.const 1 i32.add global.set $counter
        i32.const 65548 i32.const 12 i32.load i32.store
        i32.const 65552 i32.const 16 i32.load i32.store
        i32.const 65556 global.get $counter i32.store
        i32.const {output.Length}))"""

let private runControlHostQualifications () =
    let artifactBytes = statefulControllerWat [] |> watBytes
    use artifact =
        ControlHost.compile ControlHost.defaultProfile "standard-reference" artifactBytes
    use first = ControlHost.instantiate artifact 7 [| 1uy; 2uy |]
    use second = ControlHost.instantiate artifact 8 [| 3uy |]

    let firstTick =
        match ControlHost.invoke 1 (abiInput 1 7) first with
        | Accepted(output, journal) ->
            require (output.Envelope.Flags = 1u) "First instance did not advance its private global."
            output, journal
        | result -> failwithf "Reference controller failed: %A" result

    match ControlHost.invoke 1 (abiInput 1 8) second with
    | Accepted(output, _) ->
        require (output.Envelope.Flags = 1u) "Instances shared mutable global state."
    | result -> failwithf "Second reference controller failed: %A" result

    let snapshot = ControlHost.snapshot first
    let checkpointed = ControlHost.checkpointJournal first (snd firstTick)
    require checkpointed.ModuleState.IsSome "Replay checkpoint omitted resumable module state."

    let mutable incompleteSnapshotRejected = false
    try
        use _incomplete =
            ControlHost.resume
                artifact
                7
                [| 1uy; 2uy |]
                { snapshot with MutableGlobals = [] }
        ()
    with :? ArgumentException ->
        incompleteSnapshotRejected <- true
    require
        incompleteSnapshotRejected
        "An incomplete mutable-global snapshot was accepted."

    let originalTickTwo =
        match ControlHost.invoke 2 (abiInput 2 7) first with
        | Accepted(output, journal) -> output, journal
        | result -> failwithf "Original controller continuation failed: %A" result

    use resumed = ControlHost.resume artifact 7 [| 1uy; 2uy |] snapshot

    let resumedTickTwo =
        match ControlHost.invoke 2 (abiInput 2 7) resumed with
        | Accepted(output, journal) -> output, journal
        | result -> failwithf "Resumed controller continuation failed: %A" result

    require
        ((fst originalTickTwo).Envelope.Flags = (fst resumedTickTwo).Envelope.Flags
         && (snd originalTickTwo).OutputHash = (snd resumedTickTwo).OutputHash
         && (snd originalTickTwo).ModuleStateHash = (snd resumedTickTwo).ModuleStateHash)
        "Snapshot/resume did not reproduce controller output and state hashes."

    let malformedWat =
        statefulControllerWat []
        |> fun wat -> wat.Replace(
            $"i32.const 65548 i32.const 12 i32.load i32.store",
            $"i32.const 65536 i32.const 0 i32.store"
        )

    use malformedArtifact =
        ControlHost.compile ControlHost.defaultProfile "malformed-reference" (watBytes malformedWat)
    use malformed = ControlHost.instantiate malformedArtifact 7 [||]

    match ControlHost.invoke 1 (abiInput 1 7) malformed with
    | Failed(ControlFailure.MalformedOutput, journal) ->
        require (List.isEmpty journal.Requests) "Malformed output retained partial requests."
    | result -> failwithf "Malformed output was not rejected atomically: %A" result

    let dynamicRangeWat =
        statefulControllerWat []
        |> fun wat ->
            wat.Replace(
                "(func (export \"sir_input_ptr\") (result i32) i32.const 0)",
                """(func (export "sir_input_ptr") (result i32)
                  (if (result i32)
                    (i32.gt_s (global.get $counter) (i32.const 0))
                    (then (i32.const 200000))
                    (else (i32.const 0))))"""
            )
    use dynamicRangeArtifact =
        ControlHost.compile
            ControlHost.defaultProfile
            "dynamic-range-reference"
            (watBytes dynamicRangeWat)
    use dynamicRange =
        ControlHost.instantiate dynamicRangeArtifact 7 [||]

    match ControlHost.invoke 1 (abiInput 1 7) dynamicRange with
    | Accepted _ -> ()
    | result -> failwithf "Initial dynamic-range invocation failed: %A" result

    match ControlHost.invoke 2 (abiInput 2 7) dynamicRange with
    | Failed(ControlFailure.MemoryLimit, journal) ->
        require
            (List.isEmpty journal.Requests)
            "A dynamic out-of-range buffer retained accepted requests."
    | result ->
        failwithf
            "A dynamic out-of-range buffer was not rejected atomically: %A"
            result

    let fuelWat =
        $"""(module
          (memory (export "memory") 2 2)
          (func (export "sir_abi_version") (result i32) i32.const 65536)
          (func (export "sir_input_ptr") (result i32) i32.const 0)
          (func (export "sir_input_capacity") (result i32) i32.const 65536)
          (func (export "sir_output_ptr") (result i32) i32.const 65536)
          (func (export "sir_output_capacity") (result i32) i32.const 16384)
          (func (export "sir_decide") (param i32) (result i32)
            (loop $forever br $forever) i32.const 0))"""

    use fuelArtifact =
        ControlHost.compile ControlHost.defaultProfile "fuel-reference" (watBytes fuelWat)
    use fuel = ControlHost.instantiate fuelArtifact 7 [||]

    match ControlHost.invoke 1 (abiInput 1 7) fuel with
    | Failed(ControlFailure.FuelExhaustion, journal) ->
        require
            (List.isEmpty journal.Requests
             && journal.Budget.FuelConsumed = journal.Budget.FuelAllowance)
            "Fuel exhaustion retained requests or an incomplete budget."
    | result -> failwithf "Fuel exhaustion was not isolated atomically: %A" result

    let sleepRequest =
        { Kind = RequestKind.Sleep
          ModuleRequestId = 1u
          Payload = BitConverter.GetBytes 3 }
    use sleepArtifact =
        ControlHost.compile
            ControlHost.defaultProfile
            "sleep-reference"
            (statefulControllerWat [ sleepRequest ] |> watBytes)
    use sleeping = ControlHost.instantiate sleepArtifact 7 [||]

    match ControlHost.invoke 1 (abiInput 1 7) sleeping with
    | Accepted _ -> ()
    | result -> failwithf "Sleep request failed: %A" result
    match ControlHost.invoke 2 (abiInput 2 7) sleeping with
    | SleepingUntil 3 -> ()
    | result -> failwithf "Sleep schedule was not enforced: %A" result

    let growthWat =
        statefulControllerWat []
        |> fun wat -> wat.Replace(
            "(memory (export \"memory\") 2 2)",
            "(memory (export \"memory\") 2)"
        )
        |> fun wat -> wat.Replace(
            "global.get $counter i32.const 1 i32.add global.set $counter",
            "i32.const 1 memory.grow drop global.get $counter i32.const 1 i32.add global.set $counter"
        )
    use growthArtifact =
        ControlHost.compile ControlHost.defaultProfile "growth-reference" (watBytes growthWat)
    use growth = ControlHost.instantiate growthArtifact 7 [||]
    match ControlHost.invoke 1 (abiInput 1 7) growth with
    | Accepted(_, journal) ->
        require
            (journal.Budget.MemoryBytes = ControlHost.defaultProfile.MaximumMemoryBytes)
            "Store memory limiter permitted growth beyond the profile."
    | result -> failwithf "Bounded memory-growth controller failed unexpectedly: %A" result

    let wasiWat =
        """(module
          (import "wasi_snapshot_preview1" "random_get"
            (func $random_get (param i32 i32) (result i32)))
          (memory (export "memory") 2))"""
    let mutable wasiRejected = false
    try
        use _artifact =
            ControlHost.compile ControlHost.defaultProfile "wasi-reference" (watBytes wasiWat)
        ()
    with :? ArgumentException ->
        wasiRejected <- true
    require wasiRejected "Ambient WASI import was accepted."

    let hiddenGlobalWat =
        statefulControllerWat []
        |> fun wat -> wat.Replace(
            "(global $counter (export \"counter\") (mut i32) (i32.const 0))",
            "(global $counter (mut i32) (i32.const 0))"
        )
    let mutable hiddenGlobalRejected = false
    try
        use _artifact =
            ControlHost.compile
                ControlHost.defaultProfile
                "hidden-global-reference"
                (watBytes hiddenGlobalWat)
        ()
    with :? ArgumentException ->
        hiddenGlobalRejected <- true
    require hiddenGlobalRejected "A hidden mutable global escaped snapshot qualification."

    let state =
        { Tick = 0
          Board =
            { Width = 3
              Height = 3
              Terrain = Map.empty
              Edges = Map.empty }
          Units =
            Map.ofList
                [ 7,
                  { Id = 7
                    Side = 1
                    ClassId = "rifleman"
                    Cell = MapScale.cell 1 1
                    Size = 1
                    Health = 10
                    Controller = ManualController
                    Script = []
                    ScriptIndex = 0
                    BodyFacing = North
                    AttentionDirection = North } ]
          MovementCreditsMillimeters = Map.empty
          MovementProgress = Map.empty
          MovementIntents = Map.empty
          PlannedRoutes = Map.empty
          Engagements = Map.empty }

    let kernelRequests =
        [ { Kind = RequestKind.SetMovementIntent
            ModuleRequestId = 1u
            Payload = [| Direction8.toCode East |] }
          { Kind = RequestKind.SetFacing
            ModuleRequestId = 2u
            Payload = [| Direction8.toCode South |] }
          { Kind = RequestKind.SetAttention
            ModuleRequestId = 3u
            Payload = [| Direction8.toCode West |] } ]

    let fedState =
        ControlHost.applyToMapScale state 7 kernelRequests
        |> Result.defaultWith failwith
    require
        (Map.find 7 fedState.MovementIntents = East
         && (Map.find 7 fedState.Units).BodyFacing = South
         && (Map.find 7 fedState.Units).AttentionDirection = West)
        "Accepted host requests were not fed into MapScale."

    let instances =
        [| for unitId in 1 .. 200 ->
               ControlHost.instantiate artifact unitId [||] |]
    try
        for unitId in 1 .. 200 do
            ControlHost.invoke 1 (abiInput 1 unitId) instances[unitId - 1] |> ignore

        let samples =
            [| for tick in 2 .. 6 do
                   let started = Stopwatch.GetTimestamp()
                   for unitId in 1 .. 200 do
                       match ControlHost.invoke tick (abiInput tick unitId) instances[unitId - 1] with
                       | Accepted _ -> ()
                       | result -> failwithf "200-instance qualification failed: %A" result
                   yield Stopwatch.GetElapsedTime(started).TotalMilliseconds |]
        let best = Array.min samples
        let qualificationBudgetMs = 50.0
        require
            (not enforceProductPerformanceBudgets || best < qualificationBudgetMs)
            $"200 controllers exceeded the configuration's {qualificationBudgetMs:F0} ms qualification budget (best {best:F3} ms)."
        best
    finally
        instances |> Array.iter (fun instance -> (instance :> IDisposable).Dispose())

let private runPlanQualifications () =
    let artifactBytes = StandardController.artifactBytes ()
    require
        (not (StandardController.source.Contains("(import", StringComparison.Ordinal)))
        "The standard controller source introduced a non-public host import."
    let firstMove = Guid.ParseExact("10000000000000000000000000000001", "N")
    let firstFace = Guid.ParseExact("10000000000000000000000000000002", "N")
    let firstAttend = Guid.ParseExact("10000000000000000000000000000003", "N")
    let firstSync = Guid.ParseExact("10000000000000000000000000000004", "N")
    let firstEngage = Guid.ParseExact("10000000000000000000000000000005", "N")
    let firstHold = Guid.ParseExact("10000000000000000000000000000006", "N")

    let command commandId predecessors kind annotation =
        { CommandId = commandId
          EarliestStartTick = 1
          Predecessors = predecessors
          InterruptionPolicy = ApplyFallback
          Fallback = HoldPosition
          Kind = kind
          Annotation = annotation }

    let planUnit unitId origin destination target (prefix: byte) =
        let id (source: Guid) =
            let bytes = source.ToByteArray()
            bytes[15] <- bytes[15] + prefix
            Guid bytes

        let move = id firstMove
        let face = id firstFace
        let attend = id firstAttend
        let sync = id firstSync
        let engage = id firstEngage
        let hold = id firstHold

        { UnitId = unitId
          ControllerArtifact = artifactBytes
          Commands =
            [| command move [||] (MovePath([| origin; destination |], Balanced)) "route annotation"
               command face [| move |] (SetFacingIntent(FaceFixed East)) "face annotation"
               command attend [| face |] (SetAttentionIntent(AttendRelativeToBody North)) ""
               command
                   sync
                   [| attend |]
                   (Synchronize
                       { MarkerId = "line-ready"
                         Mode = PreloadedClock 30
                         DeadlineTick = 35
                         Timeout = Continue })
                   "clock synchronization"
               command engage [| sync |] (EngageUnit(target, "rifle")) "point engagement"
               command hold [| engage |] Hold "" |]
          Fallback = AbortUnitPlan }

    let state =
        { Tick = 0
          Board =
            { Width = 6
              Height = 3
              Terrain = Map.empty
              Edges = Map.empty }
          Units =
            Map.ofList
                [ 1,
                  { Id = 1
                    Side = 1
                    ClassId = "rifleman"
                    Cell = MapScale.cell 0 1
                    Size = 1
                    Health = 10
                    Controller = GeneralController
                    Script = []
                    ScriptIndex = 0
                    BodyFacing = North
                    AttentionDirection = North }
                  2,
                  { Id = 2
                    Side = 1
                    ClassId = "rifleman"
                    Cell = MapScale.cell 5 1
                    Size = 1
                    Health = 10
                    Controller = GeneralController
                    Script = []
                    ScriptIndex = 0
                    BodyFacing = North
                    AttentionDirection = North } ]
          MovementCreditsMillimeters = Map.empty
          MovementProgress = Map.empty
          MovementIntents = Map.empty
          PlannedRoutes = Map.empty
          Engagements = Map.empty }

    let mapDigest = Array.init 32 byte
    let document =
        { FormatVersion = SirPlan.FormatVersion
          PlanId = Guid.ParseExact("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "N")
          Revision = 4L
          ParentDigest = Some(Array.create 32 0x11uy)
          MapRevisionDigest = mapDigest
          RulesetIdentity = "prototype-rules-1"
          StartTick = 1
          HorizonTicks = 100
          UnitPlans =
            [| planUnit 1 (MapScale.cell 0 1) (MapScale.cell 1 1) 2 0uy
               planUnit 2 (MapScale.cell 5 1) (MapScale.cell 4 1) 1 16uy |] }

    let context =
        { Map = state
          RulesetIdentity = document.RulesetIdentity
          MapRevisionDigest = mapDigest
          MaximumConfigurationBytes = ControlHost.defaultProfile.MaximumConfigurationBytes }

    let encoded =
        SirPlan.encode document
        |> Result.defaultWith failwith
    let decoded =
        SirPlan.decode encoded
        |> Result.defaultWith failwith
    require
        (SirPlan.encode decoded = Ok encoded)
        "SIR-PLAN 1 canonical source did not round-trip exactly."

    let annotationEdit =
        { document with
            UnitPlans =
                document.UnitPlans
                |> Array.mapi (fun index unit ->
                    if index <> 0 then unit
                    else
                        { unit with
                            Commands =
                                unit.Commands
                                |> Array.mapi (fun commandIndex item ->
                                    if commandIndex = 0 then
                                        { item with Annotation = "edited only" }
                                    else item) }) }
    require
        (SirPlan.semanticDigest annotationEdit = SirPlan.semanticDigest document
         && SirPlan.sourceDigest annotationEdit <> SirPlan.sourceDigest document)
        "Annotations changed execution identity or failed to change source identity."

    let compiled =
        SirPlan.compile context document
        |> Result.defaultWith (fun issues -> failwithf "Coordinated plan failed validation: %A" issues)
    require
        (compiled.Units.Length = 2
         && compiled.Units |> Array.forall (fun unit -> unit.Configuration.Length <= 4096))
        "The coordinated plan did not compile to two bounded configurations."

    let cyclic =
        let unit = document.UnitPlans[0]
        let first = unit.Commands[0]
        let second = unit.Commands[1]
        { document with
            UnitPlans =
                [| { unit with
                       Commands =
                           [| { first with
                                  Predecessors = [| second.CommandId |]
                                  Kind =
                                    MovePath(
                                        [| MapScale.cell 0 1
                                           MapScale.cell 4 1 |],
                                        Balanced) }
                              { second with Predecessors = [| first.CommandId |] } |] } |] }

    let fallbackCyclic =
        let unit = document.UnitPlans[0]
        let first = unit.Commands[0]
        let second = unit.Commands[1]
        { document with
            UnitPlans =
                [| { unit with
                       Commands =
                           [| { first with
                                  Predecessors = [||]
                                  Fallback = JumpTo second.CommandId }
                              { second with
                                  Predecessors = [||]
                                  Fallback = JumpTo first.CommandId } |] } |] }

    let diagnosticSignature candidate =
        SirPlan.validate context candidate
        |> Array.map (fun diagnostic ->
            diagnostic.Code, diagnostic.UnitId, diagnostic.CommandId, diagnostic.Fields)

    let firstDiagnostics = diagnosticSignature cyclic
    let secondDiagnostics = diagnosticSignature cyclic
    require
        (firstDiagnostics = secondDiagnostics
         && firstDiagnostics
            |> Array.exists (fun (code, _, commandId, _) ->
                code = "SIR.PLAN.SCHEDULE.CYCLE" && commandId.IsSome)
         && firstDiagnostics
            |> Array.exists (fun (code, _, commandId, _) ->
                code = "SIR.PLAN.MAP.NON_ADJACENT_PATH" && commandId.IsSome))
        "Invalid/cyclic plans did not produce stable command-scoped diagnostics."
    require
        (diagnosticSignature fallbackCyclic
         |> Array.exists (fun (code, _, commandId, _) ->
             code = "SIR.PLAN.SCHEDULE.CYCLE" && commandId.IsSome))
        "Fallback JumpTo cycles escaped dependency validation."

    use artifact =
        ControlHost.compile
            ControlHost.defaultProfile
            "sir-standard-controller"
            artifactBytes

    let run () =
        let instances =
            compiled.Units
            |> Array.map (fun unit ->
                unit.UnitId,
                ControlHost.instantiate artifact unit.UnitId unit.Configuration)
        try
            [| for tick in document.StartTick .. document.StartTick + document.HorizonTicks - 1 do
                   for unitId, instance in instances do
                       match ControlHost.invoke tick (abiInput tick unitId) instance with
                       | Accepted(output, _) when not output.Requests.IsEmpty ->
                           yield
                               tick,
                               unitId,
                               output.Requests
                               |> List.map (fun request ->
                                   request.Kind,
                                   request.ModuleRequestId,
                                   Convert.ToHexString request.Payload)
                       | Accepted _ -> ()
                       | SleepingUntil _ -> ()
                       | result ->
                           failwithf
                               "Standard controller failed at tick %d for unit %d: %A"
                               tick
                               unitId
                               result |]
        finally
            instances
            |> Array.iter (fun (_, instance) ->
                (instance :> IDisposable).Dispose())

    let firstRun = run ()
    let secondRun = run ()
    require
        (firstRun = secondRun
         && firstRun
            |> Array.exists (fun (_, _, requests) ->
                requests
                |> List.exists (fun (kind, _, _) ->
                    kind = RequestKind.SetMovementIntent))
         && firstRun
            |> Array.exists (fun (_, _, requests) ->
                requests
                |> List.exists (fun (kind, _, _) ->
                    kind = RequestKind.SetEngagement)))
        "Repeated coordinated native runs did not produce identical movement and engagement requests."

let private runCapabilityQualifications () =
    let descriptorIds =
        HumanCapabilities.descriptors |> Array.map _.CapabilityId

    require
        (HumanCapabilities.descriptors.Length = 7
         && descriptorIds |> Array.distinct |> Array.length = 7
         && HumanCapabilities.descriptors
            |> Array.map _.PlanningDecision
            |> Array.distinct
            |> Array.length = 7)
        "The seven human weapon roles did not each retain a distinct planning decision."

    require
        (Direction8.all.Length = 8
         && Enum.GetValues<RequestKind>()
            = [| RequestKind.SetMovementIntent
                 RequestKind.SetFacing
                 RequestKind.SetAttention
                 RequestKind.SetStance
                 RequestKind.SetEngagement
                 RequestKind.StartCapability
                 RequestKind.CancelAction
                 RequestKind.SendMessage
                 RequestKind.RequestService
                 RequestKind.SetEmissionPolicy
                 RequestKind.SetFormationIntent
                 RequestKind.Sleep |])
        "Capability integration added an ABI request kind or a fourth direction authority."

    let loadout unitId (descriptor: HumanWeaponCapabilityDescriptor) =
        HumanCapabilities.createLoadout unitId descriptor.Role [| descriptor.CapabilityId |]
        |> Result.defaultWith failwith

    let targetLoadout =
        HumanCapabilities.createLoadout 100 "target" [| "human.weapon.rifle" |]
        |> Result.defaultWith failwith

    let attackers =
        HumanCapabilities.descriptors
        |> Array.mapi (fun index descriptor ->
            let unitId = index + 1
            unitId,
            { Loadout = loadout unitId descriptor
              Cell = 0, index
              Attention = North
              Ammunition = Map.ofList [ descriptor.CapabilityId, 20 ]
              PreservedPreparation = Map.empty
              Engagement = None })
        |> Map.ofArray

    let target =
        { Loadout = targetLoadout
          Cell = 2, 3
          Attention = West
          Ammunition = Map.ofList [ "human.weapon.rifle", 20 ]
          PreservedPreparation = Map.empty
          Engagement = None }

    let initial =
        { Tick = 0
          Units = Map.add 100 target attackers
          Areas = Map.ofList [ 900, (2, 4) ] }

    let journal =
        HumanCapabilities.descriptors
        |> Array.mapi (fun index descriptor ->
            let target =
                match descriptor.TargetContract with
                | CapabilityTargetContract.PointTarget -> PointCapabilityTarget 100
                | CapabilityTargetContract.AreaTarget -> AreaCapabilityTarget 900
            { Tick = 0
              UnitId = index + 1
              Request =
                CapabilityExecution.engagementRequest
                    (uint32 (index + 1))
                    target
                    descriptor.CapabilityId })
        |> Array.toList

    let mutable state = initial
    let mutable events = []
    for tick in 0 .. 31 do
        let requests =
            journal
            |> List.choose (fun entry ->
                if entry.Tick = tick then Some(entry.UnitId, [ entry.Request ])
                else None)
        let result = ControlHost.applyToCapabilities state requests
        state <- result.State
        events <- result.Events @ events

    for index, descriptor in HumanCapabilities.descriptors |> Array.indexed do
        let unit = Map.find (index + 1) state.Units
        let expected = 20 - descriptor.AmmunitionPerResolution
        let targetCell =
            match descriptor.TargetContract with
            | CapabilityTargetContract.PointTarget -> target.Cell
            | CapabilityTargetContract.AreaTarget -> Map.find 900 initial.Areas
        let expectedAttention =
            Direction8.tryFromDelta
                (compare (fst targetCell) (fst unit.Cell) |> int32)
                (compare (snd targetCell) (snd unit.Cell) |> int32)
            |> Option.defaultWith (fun () ->
                failwith "Capability qualification target had no direction.")
        require
            (Map.find descriptor.CapabilityId unit.Ammunition = expected
             && unit.Attention = expectedAttention)
            ("Ammunition semantics did not execute for " + descriptor.CapabilityId)

        require
            (events
             |> List.exists (function
                 | PointEngagementResolved(unitId, _, capabilityId, _)
                     when descriptor.TargetContract = CapabilityTargetContract.PointTarget ->
                     unitId = index + 1 && capabilityId = descriptor.CapabilityId
                 | AreaEngagementResolved(unitId, _, capabilityId, _)
                     when descriptor.TargetContract = CapabilityTargetContract.AreaTarget ->
                     unitId = index + 1 && capabilityId = descriptor.CapabilityId
                 | _ -> false))
            ("Target-shape execution did not resolve for " + descriptor.CapabilityId)

    require
        (events
         |> List.exists (function CapabilityTraversing(_, _, ticks) -> ticks > 0 | _ -> false))
        "Attention alignment did not produce descriptor-owned traverse time."
    require
        (events |> List.exists (function CapabilityPrepared _ -> true | _ -> false))
        "Capability preparation never completed."

    let pointAndAreaJournal =
        journal
        |> List.filter (fun entry -> entry.UnitId = 2 || entry.UnitId = 5)
    let expectedReplay =
        CapabilityExecution.replay initial 32 pointAndAreaJournal
    match
        CapabilityExecution.verifyReplay
            initial
            32
            pointAndAreaJournal
            expectedReplay
    with
    | Ok verified ->
        require
            (verified.Length = 32)
            "Capability replay omitted deterministic point/area frames."
    | Error tick ->
        failwithf "Point/area capability replay diverged at tick %d." tick

    let alternateTarget =
        { initial with
            Units =
                initial.Units
                |> Map.add 101 { target with Cell = 3, 3 } }
    let firstPointRequest = pointAndAreaJournal |> List.find (fun entry -> entry.UnitId = 2)
    let alternateJournal =
        [ { firstPointRequest with
              Request =
                CapabilityExecution.engagementRequest
                    2u
                    (PointCapabilityTarget 101)
                    "human.weapon.rifle" } ]
    let originalTargetState =
        CapabilityExecution.runTick initial [ 2, firstPointRequest.Request ]
    let alternateTargetState =
        CapabilityExecution.runTick alternateTarget [ 2, alternateJournal.Head.Request ]
    require
        (CapabilityExecution.stateDigest originalTargetState.State
         <> CapabilityExecution.stateDigest alternateTargetState.State)
        "Capability replay state identity omitted the engagement target."

    match
        CapabilityExecution.verifyReplay
            initial
            32
            pointAndAreaJournal
            (expectedReplay |> List.take 31)
    with
    | Error 32 -> ()
    | other ->
        failwithf "Truncated capability replay did not report frame 32: %A" other

    let interruptedInitial =
        { initial with
            Units =
                initial.Units
                |> Map.add 1 { (Map.find 1 initial.Units) with Attention = East }
                |> Map.add 4 { (Map.find 4 initial.Units) with Attention = East } }
    let start (descriptor: HumanWeaponCapabilityDescriptor) unitId =
        unitId,
        CapabilityExecution.engagementRequest
            (uint32 unitId)
            (PointCapabilityTarget 100)
            descriptor.CapabilityId
    let started =
        CapabilityExecution.runTick
            interruptedInitial
            [ start HumanCapabilities.descriptors[0] 1
              start HumanCapabilities.descriptors[3] 4 ]
    let cancelled =
        CapabilityExecution.runTick
            started.State
            [ 1, CapabilityExecution.cancelRequest 101u
              4, CapabilityExecution.cancelRequest 104u ]
    require
        (cancelled.Events
         |> List.contains
             (CapabilityInterrupted(
                 1,
                 HumanCapabilities.descriptors[0].CapabilityId,
                 false
             ))
         && cancelled.Events
            |> List.contains
                (CapabilityInterrupted(
                    4,
                    HumanCapabilities.descriptors[3].CapabilityId,
                    true
                ))
         && (Map.find 1 cancelled.State.Units).PreservedPreparation.IsEmpty
         && Map.find
                HumanCapabilities.descriptors[3].CapabilityId
                (Map.find 4 cancelled.State.Units).PreservedPreparation
            = 1
         && (Map.find 4 cancelled.State.Units).Engagement.IsNone)
        "Descriptor-owned interruption rules did not distinguish lost and preserved preparation."

    printfn
        "Capability roles qualified: 7 descriptors, %d deterministic point/area replay frames, 8 directions, 12 unchanged ABI request kinds."
        expectedReplay.Length

let private runLiveIntegrationQualifications () =
    let measure action =
        let timer = Stopwatch.StartNew()
        let result = action ()
        timer.Stop()
        result, timer.Elapsed.TotalMilliseconds

    let qualification, fullTickMs =
        measure LiveIntegration.qualify

    require
        (qualification.KernelTicks = 40
         && qualification.ControllerInvocations = 80
         && qualification.CapabilityEvents > 0
         && qualification.Replay.Frames
            |> Array.mapi (fun index frame ->
                frame.Tick = index + 1
                && frame.ServerSequence = int64 (index + 1)
                && frame.ProjectionRevision = int64 (index + 1))
            |> Array.forall id)
        "Live qualification was not canonical continuous per-tick execution."
    require
        (LiveIntegration.verify qualification.Replay)
        "The authoritative live replay did not reproduce through the same path."
    let firstFrame = qualification.Replay.Frames[0]
    let firstUnit = firstFrame.VisibleUnits[0]
    let tamperedFrames = Array.copy qualification.Replay.Frames
    tamperedFrames[0] <-
        { firstFrame with
            VisibleUnits =
                [| { firstUnit with
                       DisplayColumn = firstUnit.DisplayColumn + 1 }
                   yield! firstFrame.VisibleUnits[1..] |] }
    require
        (not (
            LiveIntegration.verify
                { qualification.Replay with Frames = tamperedFrames }
        ))
        "A tampered disclosed replay projection retained authoritative verification."

    let identities = qualification.Replay.Identities
    require
        (identities.MapRevision = qualification.Artifact.MapRevision
         && identities.PlanSemantic = qualification.Artifact.SemanticIdentity
         && identities.PlanSource = qualification.Artifact.SourceIdentity
         && identities.Ruleset = qualification.Artifact.Ruleset
         && identities.DescriptorSet = "sir.human-weapons@1"
         && identities.ControllerArtifact.Length = 32
         && identities.Engine.Length = 32
         && identities.Replay.Length = 32
         && identities.MatchLock = qualification.Artifact.MatchLock)
        "Replay and diagnostics did not pin every live identity."

    let session =
        LiveIntegration.admit
            "session-qualification"
            "blue-player"
            qualification.Artifact
            qualification.Artifact
        |> Result.defaultWith failwith

    match LiveIntegration.reconnect session qualification.Replay 36L 36L with
    | Ok(ResumeWith frames) ->
        require
            (frames.Length = 4 && frames[0].Tick = 37)
            "Reconnect did not resume from retained projection envelopes."
    | result -> failwithf "Valid reconnect was rejected: %A" result

    match LiveIntegration.reconnect session qualification.Replay 0L 0L with
    | Ok(ReplaceWithSnapshot frame) ->
        require
            (frame.Tick = 40)
            "Long reconnect gap did not replace state with the latest snapshot."
    | result -> failwithf "Snapshot reconnect was rejected: %A" result

    let forgedLock = Array.copy qualification.Artifact.MatchLock
    forgedLock[0] <- forgedLock[0] ^^^ 1uy
    let forged = { qualification.Artifact with MatchLock = forgedLock }
    let forgedAdmission =
        LiveIntegration.admit
            "session-forged"
            "blue-player"
            forged
            qualification.Artifact
    require
        (forgedAdmission = Error "SIR.LIVE.ADMISSION.ARTIFACT_MISMATCH")
        "Session admission accepted a plan outside the match lock."
    let inconsistentReconnect =
        LiveIntegration.reconnect session qualification.Replay 39L 38L
    require
        (inconsistentReconnect = Error "SIR.LIVE.RECONNECT.PROJECTION_GAP")
        "Reconnect accepted inconsistent server/projection cursors."

    let projectionBytes =
        qualification.Replay.Frames
        |> Array.map LiveIntegration.serializeProjection
    let emptyDisclosureIdentity = CanonicalHash.sha256 [||]
    require
        (qualification.Replay.Frames
         |> Array.forall (fun frame ->
             frame.VisibleUnits
             |> Array.forall (fun unit -> unit.UnitId <> 20)))
        "The player projection disclosed the opposing unit."
    require
        (qualification.Replay.JournalIdentity
         <> qualification.Replay.ProjectionIdentity
         && qualification.Replay.Frames
            |> Array.forall (fun frame ->
                let visibleIdentity =
                    frame.VisibleUnits
                    |> Array.collect (fun unit ->
                        CanonicalEncoding.concatenate
                            [ CanonicalEncoding.int32LittleEndian unit.UnitId
                              CanonicalEncoding.int32LittleEndian unit.DisplayColumn
                              CanonicalEncoding.int32LittleEndian unit.DisplayRow
                              CanonicalEncoding.int32LittleEndian unit.Health ])
                    |> CanonicalHash.sha256
                frame.StateIdentity = visibleIdentity
                && frame.EventIdentity = emptyDisclosureIdentity))
        "The browser projection carried an identity derived from undisclosed authoritative state."
    require
        (projectionBytes
         |> Array.forall (fun bytes ->
             not (containsSubsequence (StandardController.artifactBytes ()) bytes)))
        "The player projection disclosed the controller artifact."

    let _, previewMs =
        measure (fun () ->
            qualification.Replay.Frames
            |> Array.take 20
            |> Array.map (fun frame -> frame.Tick, frame.VisibleUnits))
    let serialized, serializationMs =
        measure (fun () -> projectionBytes |> Array.collect id)
    let _, workerMs =
        measure (fun () -> serialized |> Array.copy)
    let _, renderingMs =
        measure (fun () ->
            qualification.Replay.Frames
            |> Array.sumBy (fun frame ->
                frame.VisibleUnits
                |> Array.sumBy (fun unit ->
                    unit.DisplayColumn + unit.DisplayRow + unit.Health)))

    require
        (not enforceProductPerformanceBudgets
         || (fullTickMs < 5_000.0
             && previewMs < 100.0
             && serializationMs < 100.0
             && workerMs < 100.0
             && renderingMs < 100.0))
        "Live vertical-slice performance exceeded its qualification budgets."

    printfn
        "Live integration qualified: 40 continuous ticks in %.3f ms; preview %.3f ms; serialization %.3f ms; worker transfer %.3f ms; rendering projection %.3f ms; %d projection bytes; replay %s."
        fullTickMs
        previewMs
        serializationMs
        workerMs
        renderingMs
        serialized.Length
        (Convert.ToHexString(identities.Replay).ToLowerInvariant())

[<EntryPoint>]
let main args =
    let functionalCrossRuntime = Array.contains "--functional-cross-runtime" args
    let unknownArguments =
        args
        |> Array.filter (fun argument -> argument <> "--print-tactical-environment" && argument <> "--functional-cross-runtime")
    require (unknownArguments.Length = 0) (sprintf "Unknown match qualification arguments: %A." unknownArguments)
    enforceProductPerformanceBudgets <- not functionalCrossRuntime
    if Array.contains "--print-tactical-environment" args then
        let plot, variants = SIR.Simulation.TacticalEnvironment.exteriorParcelSet
        let environment =
            SIR.Simulation.TacticalEnvironment.assemble 0x186UL plot variants
            |> Result.defaultWith (fun findings -> failwithf "%A" findings)
        printfn "%s" (Convert.ToHexString(SIR.Domain.TacticalEnvironment.canonicalBytes environment).ToLowerInvariant())
        0
    else
    tacticalEnvironmentEvidence ()
    let controllerTickMs = runControlHostQualifications ()
    runPlanQualifications ()
    runCapabilityQualifications ()
    runLiveIntegrationQualifications ()
    let expectedControlOutput = controlAbiOutput ()
    let referenceControlOutput =
        executeReferenceControlModule expectedControlOutput

    require
        (referenceControlOutput = expectedControlOutput)
        "Reference WASM module and F# Control ABI v1 codec disagree."

    let qualification = MatchReplay.qualify ()
    let fullBytes = Replay.encode qualification.FullPackage
    let perspectiveBytes = Replay.encode qualification.PerspectivePackage
    let expectedEngine = qualification.FullPackage.EngineHash

    match
        Replay.runKernelReplay
            Replay.defaultLimits
            expectedEngine
            qualification.FullPackage
    with
    | Ok(BrowserKernelVerified browserResult) ->
        match
            Replay.verifyAuthoritative
                Replay.defaultLimits
                expectedEngine
                (Some qualification.ReexecutedOutputs)
                qualification.FullPackage
        with
        | Ok(AuthoritativeVerified authoritativeResult) ->
            require
                (browserResult.StateHash = authoritativeResult.StateHash
                 && browserResult.EventHash = authoritativeResult.EventHash)
                "Browser-kernel and authoritative hashes differ."
        | result ->
            failwithf "Exact-artifact authoritative verification failed: %A" result
    | result -> failwithf "Full browser-kernel replay failed: %A" result

    let changedOutputs =
        qualification.ReexecutedOutputs
        |> List.map (fun output ->
            if output.Tick = 2 then
                { output with
                    Input =
                        Move(
                            Simulation.unitId 10,
                            { Col = 0; Row = 1 }
                        ) }
            else
                output)

    match
        Replay.verifyAuthoritative
            Replay.defaultLimits
            expectedEngine
            (Some changedOutputs)
            qualification.FullPackage
    with
    | Error(WasmOutputDivergence(2, 2)) -> ()
    | result ->
        failwithf "Changed WASM output did not identify its first divergence: %A" result

    let corrupted =
        match qualification.FullPackage.Content with
        | PerspectivePlayback _ -> failwith "Expected an authorized full replay."
        | AuthorizedFullReplay full ->
            let checkpoints =
                full.Checkpoints
                |> List.map (fun checkpoint ->
                    if checkpoint.Tick = 2 then
                        let changed = Array.copy checkpoint.EventHash
                        changed[0] <- changed[0] ^^^ 1uy
                        { checkpoint with EventHash = changed }
                    else
                        checkpoint)

            { qualification.FullPackage with
                Content =
                    AuthorizedFullReplay
                        { full with Checkpoints = checkpoints } }

    match Replay.runKernelReplay Replay.defaultLimits expectedEngine corrupted with
    | Error(ReplayDivergence(2, "checkpoint event hash")) -> ()
    | result ->
        failwithf "Corrupt replay lost first-tick divergence diagnostics: %A" result

    match
        Replay.runKernelReplay
            Replay.defaultLimits
            expectedEngine
            qualification.PerspectivePackage
    with
    | Ok(PerspectiveReady frames) ->
        require (frames.Length = 5) "Perspective playback omitted a committed frame."
    | result -> failwithf "Perspective playback qualification failed: %A" result

    match Replay.requireKernel qualification.PerspectivePackage with
    | Error PerspectiveHasNoKernel -> ()
    | result -> failwithf "Perspective playback exposed kernel material: %A" result

    let hiddenFinalHash =
        match qualification.FullPackage.Content with
        | AuthorizedFullReplay full -> full.FinalResult.StateHash
        | PerspectivePlayback _ -> failwith "Expected an authorized full replay."

    require
        (not (containsSubsequence hiddenFinalHash perspectiveBytes))
        "Perspective bytes contain the hidden final-state hash."
    require
        (not (
            containsSubsequence
                qualification.Artifact.ArtifactBytes
                perspectiveBytes
        ))
        "Perspective bytes contain the opponent control artifact."
    require
        (perspectiveBytes.Length < fullBytes.Length)
        "Perspective package is not a reduced disclosure."

    let observer = Simulation.unitId 10
    let otherObserver = Simulation.unitId 20
    let observerUnit = Simulation.initialState.Units[observer]
    let otherUnit = Simulation.initialState.Units[otherObserver]
    let retainedSector =
        if Environment.GetEnvironmentVariable("SIR_AWARENESS_STIMULUS_HISTORY_MUTATE_SUBJECT") = "1" then
            ObservationSector.Peripheral
        else
            ObservationSector.Forward
    let suspected =
        { AwarenessReaction.emptyContact otherObserver with
            Level = AwarenessLevel.Suspected
            Acquisition = 4
            LastStimulusTick = Some 3
            LastStimulus =
                Some
                    { Tick = 3
                      Modality = SpatialModality.Vision
                      Source = "SIR.Simulation.SpatialQuery.evaluate"
                      Origin = observerUnit.Cell
                      SubjectCell = otherUnit.Cell
                      Sector = retainedSector
                      SpatialRevision = 7L
                      KnowledgeIdentity = "observer-10"
                      KnowledgeRevision = 9L }
            LastKnownCell = Some otherUnit.Cell
            Reason = AwarenessReason.StimulusAccumulated }
    let differentlyInformed =
        { AwarenessReaction.emptyContact observer with
            Level = AwarenessLevel.Acquired
            Acquisition = 8
            LastStimulusTick = Some 2
            LastStimulus =
                Some
                    { Tick = 2
                      Modality = SpatialModality.Vision
                      Source = "other-observer"
                      Origin = otherUnit.Cell
                      SubjectCell = observerUnit.Cell
                      Sector = ObservationSector.Rear
                      SpatialRevision = 8L
                      KnowledgeIdentity = "observer-20"
                      KnowledgeRevision = 10L }
            LastKnownCell = Some observerUnit.Cell
            Reason = AwarenessReason.IdentificationThresholdReached }
    let localState =
        { Simulation.initialState with
            Tick = 3
            Units = Simulation.initialState.Units |> Map.change observer (Option.map (fun unit -> { unit with AttentionDirection = North }))
            Awareness = Map.ofList [ (observer, otherObserver), suspected; (otherObserver, observer), differentlyInformed ] }
    let local = AwarenessProjection.forObserver observer localState
    require (local.Contacts.Length = 1 && local.Contacts.Head.Level = AwarenessLevel.Suspected) "Observer-local projection mixed differently informed observers."
    require (local.Stimuli.Length = 1 && local.Stimuli.Head.Tick = 3 && local.Stimuli.Head.Reason = AwarenessReason.StimulusAccumulated) "Current suspected stimulus fact was not projected."
    require (local.Stimuli.Head.Sector = ObservationSector.Forward) "Later East-to-North attention rewrote the historical factual stimulus sector."
    require (local.Stimuli.Head.Modality = SpatialModality.Vision && local.Stimuli.Head.Source = "SIR.Simulation.SpatialQuery.evaluate" && local.Stimuli.Head.SpatialRevision = 7L && local.Stimuli.Head.KnowledgeIdentity = "observer-10") "Projected stimulus omitted retained factual provenance."

    printfn
        "Full match replay qualified: %d full bytes, %d perspective bytes, 4 exact WASM outputs; %d Control ABI v1 reference-module bytes agree; 200 isolated reusable-host instances in %.3f ms."
        fullBytes.Length
        perspectiveBytes.Length
        referenceControlOutput.Length
        controllerTickMs

    0
