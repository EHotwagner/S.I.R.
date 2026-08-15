namespace SIR.Simulation

open System
open System.Text
open FS.GG.Game.Core
open SIR.Domain

type EnvironmentWorkloadCounters = { WorkloadSlots: int32; WorkloadVariantsInspected: int32; WorkloadFindings: int32; WorkloadFeatures: int32; DependencyEntriesInspected: int32; DependencyEntriesInvalidated: int32; WorkloadQueryCount: int32 }

[<RequireQualifiedAccess>]
module TacticalEnvironment =
    let maximumSlots = 64
    let maximumVariantsPerRole = 32
    let maximumFindings = 512
    let maximumTargetedActionCost = 100

    let private mutationEnabled name =
#if FABLE_COMPILER
        false
#else
        String.Equals(Environment.GetEnvironmentVariable name, "1", StringComparison.Ordinal)
#endif

    let defaultPermeability kind state =
        let openLike = state = EnvironmentFeatureState.Open || state = EnvironmentFeatureState.Breached || state = EnvironmentFeatureState.Destroyed
        match kind with
        | EnvironmentFeatureKind.Door -> { AllowsMovement = openLike; AllowsSight = openLike; AllowsProjectile = openLike; AllowsAreaEffect = openLike; AllowsSound = state <> EnvironmentFeatureState.Closed; ProvidesCover = not openLike; AllowsInteraction = state <> EnvironmentFeatureState.Destroyed }
        | EnvironmentFeatureKind.Window -> { AllowsMovement = state = EnvironmentFeatureState.Breached || state = EnvironmentFeatureState.Destroyed; AllowsSight = state <> EnvironmentFeatureState.Closed; AllowsProjectile = openLike; AllowsAreaEffect = openLike; AllowsSound = true; ProvidesCover = not openLike; AllowsInteraction = state <> EnvironmentFeatureState.Destroyed }
        | EnvironmentFeatureKind.Wall -> { AllowsMovement = openLike; AllowsSight = openLike; AllowsProjectile = openLike; AllowsAreaEffect = openLike; AllowsSound = openLike; ProvidesCover = not openLike; AllowsInteraction = state <> EnvironmentFeatureState.Destroyed }
        | EnvironmentFeatureKind.Cover -> { AllowsMovement = state = EnvironmentFeatureState.Destroyed; AllowsSight = state = EnvironmentFeatureState.Destroyed; AllowsProjectile = state = EnvironmentFeatureState.Destroyed; AllowsAreaEffect = state = EnvironmentFeatureState.Destroyed; AllowsSound = true; ProvidesCover = state <> EnvironmentFeatureState.Destroyed; AllowsInteraction = state <> EnvironmentFeatureState.Destroyed }

    let private finding code subject message = { ValidationCode = code; ValidationSubject = subject; ValidationMessage = message }
    let private duplicates values = values |> List.countBy id |> List.choose (fun (value, count) -> if count > 1 then Some value else None)
    let private within width height (cell: EnvironmentCell) = cell.EnvironmentColumn >= 0 && cell.EnvironmentRow >= 0 && cell.EnvironmentColumn < width && cell.EnvironmentRow < height

    let validate (plot: AuthoredPlot) (variants: ParcelVariant list) =
        let findings = ResizeArray<EnvironmentValidationFinding>()
        let add code subject message = if findings.Count < maximumFindings then findings.Add(finding code subject message)
        if plot.PlotSchemaVersion <> SIR.Domain.TacticalEnvironment.schemaVersion then add EnvironmentValidationCode.InvalidSchema plot.AuthoredPlotId "Plot schema version is unsupported."
        if plot.PlotWidth <= 0 || plot.PlotHeight <= 0 || plot.PlotSlots.Length > maximumSlots then add EnvironmentValidationCode.InvalidBounds plot.AuthoredPlotId "Plot dimensions or slot count exceed schema-v1 bounds."
        for duplicate in duplicates (plot.PlotSlots |> List.map _.PlotSlotId) do add EnvironmentValidationCode.DuplicateId duplicate "Slot id is duplicated."
        for duplicate in duplicates (variants |> List.map _.ParcelVariantId) do add EnvironmentValidationCode.DuplicateId duplicate "Variant id is duplicated."
        for role, count in variants |> List.countBy _.ParcelRole |> List.sortBy fst do
            if count > maximumVariantsPerRole then add EnvironmentValidationCode.InvalidBounds role "Variant count exceeds the per-role bound."
        let slotIds = plot.PlotSlots |> List.map _.PlotSlotId |> Set.ofList
        for slot in plot.PlotSlots |> List.sortBy _.PlotSlotId do
            if slot.PlotSlotWidth <= 0 || slot.PlotSlotHeight <= 0 || slot.PlotSlotOrigin.EnvironmentColumn < 0 || slot.PlotSlotOrigin.EnvironmentRow < 0 || slot.PlotSlotOrigin.EnvironmentColumn + slot.PlotSlotWidth > plot.PlotWidth || slot.PlotSlotOrigin.EnvironmentRow + slot.PlotSlotHeight > plot.PlotHeight then add EnvironmentValidationCode.ImpossibleFootprint slot.PlotSlotId "Slot footprint is outside the plot."
            for target in slot.ConnectedPlotSlotIds |> List.distinct |> List.sort do
                if not (Set.contains target slotIds) then add EnvironmentValidationCode.DisconnectedSlot slot.PlotSlotId ("Connection targets missing slot " + target + ".")
                elif not (plot.PlotSlots |> List.exists (fun other -> other.PlotSlotId = target && List.contains slot.PlotSlotId other.ConnectedPlotSlotIds)) then add EnvironmentValidationCode.ConnectorMismatch slot.PlotSlotId ("Connection to " + target + " is not reciprocal.")
            let compatible = variants |> List.filter (fun variant -> variant.ParcelRole = slot.PlotSlotRole && ((variant.ParcelWidth <= slot.PlotSlotWidth && variant.ParcelHeight <= slot.PlotSlotHeight) || (variant.ParcelHeight <= slot.PlotSlotWidth && variant.ParcelWidth <= slot.PlotSlotHeight)))
            if List.isEmpty compatible then add EnvironmentValidationCode.ImpossibleFootprint slot.PlotSlotId "No compatible parcel variant fits this slot."
            if slot.PlotSlotRequiresRoute && compatible |> List.forall (fun variant -> List.isEmpty variant.ParcelWalkableCells) then add EnvironmentValidationCode.UnreachableRoute slot.PlotSlotId "Every compatible parcel has an empty required route."
            if not slot.ConnectedPlotSlotIds.IsEmpty && compatible |> List.exists (fun variant -> variant.ParcelConnections.IsEmpty) then add EnvironmentValidationCode.ConnectorMismatch slot.PlotSlotId "A connected slot has a compatible variant without a connector."
        if not plot.PlotSlots.IsEmpty then
            let rec visit pending seen =
                match pending with
                | [] -> seen
                | id :: rest when Set.contains id seen -> visit rest seen
                | id :: rest ->
                    let neighbours = plot.PlotSlots |> List.tryFind (fun slot -> slot.PlotSlotId = id) |> Option.map _.ConnectedPlotSlotIds |> Option.defaultValue []
                    visit (neighbours @ rest) (Set.add id seen)
            let reached = visit [ plot.PlotSlots.Head.PlotSlotId ] Set.empty
            for id in Set.difference slotIds reached |> Set.toList do add EnvironmentValidationCode.DisconnectedSlot id "Slot is disconnected from the plot graph."
        for variant in variants |> List.sortBy _.ParcelVariantId do
            if variant.ParcelWidth <= 0 || variant.ParcelHeight <= 0 || variant.ParcelWidth > plot.PlotWidth || variant.ParcelHeight > plot.PlotHeight then add EnvironmentValidationCode.ImpossibleFootprint variant.ParcelVariantId "Variant footprint is invalid."
            for cell in variant.ParcelWalkableCells @ variant.ParcelObjectiveCells do if not (within variant.ParcelWidth variant.ParcelHeight cell) then add EnvironmentValidationCode.ImpossibleFootprint variant.ParcelVariantId "Variant cell is outside its footprint."
            if variant.ParcelObjectiveCells |> List.exists (fun cell -> not (List.contains cell variant.ParcelWalkableCells)) then add EnvironmentValidationCode.BlockedObjective variant.ParcelVariantId "Objective cell is not walkable."
            for duplicate in duplicates (variant.ParcelConnections |> List.map _.ConnectionId) do add EnvironmentValidationCode.DuplicateId (variant.ParcelVariantId + ":" + duplicate) "Connection id is duplicated."
            for connection in variant.ParcelConnections do
                if not (within variant.ParcelWidth variant.ParcelHeight connection.ConnectionCell) || String.IsNullOrWhiteSpace connection.ConnectionRole then add EnvironmentValidationCode.ConnectorMismatch connection.ConnectionId "Connection is outside the parcel footprint or has no role."
            for duplicate in duplicates (variant.ParcelFeatures |> List.map _.EnvironmentFeatureId) do add EnvironmentValidationCode.DuplicateId (variant.ParcelVariantId + ":" + duplicate) "Feature id is duplicated."
            for feature in variant.ParcelFeatures |> List.sortBy _.EnvironmentFeatureId do
                if String.IsNullOrWhiteSpace feature.EnvironmentFeatureId || feature.QueryDependencyKeys |> List.exists String.IsNullOrWhiteSpace then add EnvironmentValidationCode.InvalidDependency feature.EnvironmentFeatureId "Feature and dependency ids must be non-empty."
                if List.isEmpty feature.QueryDependencyKeys then add EnvironmentValidationCode.InvalidDependency feature.EnvironmentFeatureId "Feature declares no query dependency."
                match feature.DirectionalCover with
                | Some cover when cover.CoverMaximumIntegrity <= 0 || cover.CoverIntegrity < 0 || cover.CoverIntegrity > cover.CoverMaximumIntegrity || cover.CoverPenetrationResistance < 0 -> add EnvironmentValidationCode.InvalidPermeability feature.EnvironmentFeatureId "Cover integrity or penetration bounds are invalid."
                | Some cover when feature.EnvironmentState <> EnvironmentFeatureState.Destroyed && List.isEmpty cover.CoverProtectedDirections -> add EnvironmentValidationCode.CoverGap feature.EnvironmentFeatureId "Standing cover protects no direction."
                | _ -> ()
                let legalState =
                    match feature.EnvironmentKind, feature.EnvironmentState with
                    | EnvironmentFeatureKind.Door, (EnvironmentFeatureState.Closed | EnvironmentFeatureState.Open | EnvironmentFeatureState.Damaged | EnvironmentFeatureState.Breached | EnvironmentFeatureState.Destroyed)
                    | EnvironmentFeatureKind.Window, (EnvironmentFeatureState.Closed | EnvironmentFeatureState.Open | EnvironmentFeatureState.Damaged | EnvironmentFeatureState.Breached | EnvironmentFeatureState.Destroyed)
                    | EnvironmentFeatureKind.Wall, (EnvironmentFeatureState.Intact | EnvironmentFeatureState.Damaged | EnvironmentFeatureState.Breached | EnvironmentFeatureState.Destroyed)
                    | EnvironmentFeatureKind.Cover, (EnvironmentFeatureState.Intact | EnvironmentFeatureState.Damaged | EnvironmentFeatureState.Destroyed) -> true
                    | _ -> false
                if not legalState then add EnvironmentValidationCode.InvalidPermeability feature.EnvironmentFeatureId "Feature state is not legal for its kind."
                if feature.CapabilityDescriptors |> List.exists (fun capability -> String.IsNullOrWhiteSpace capability.DescriptorId || String.IsNullOrWhiteSpace capability.DescriptorAction || capability.DescriptorCost <= 0 || capability.DescriptorCost > maximumTargetedActionCost) then add EnvironmentValidationCode.InvalidDependency feature.EnvironmentFeatureId "Capability descriptor is invalid or exceeds the bounded action cost."
                if feature.ModalityPermeability <> defaultPermeability feature.EnvironmentKind feature.EnvironmentState then add EnvironmentValidationCode.InvalidPermeability feature.EnvironmentFeatureId "Feature permeability differs from its schema-v1 state contract."
        findings |> Seq.toList |> List.sortBy (fun value -> value.ValidationCode, value.ValidationSubject, value.ValidationMessage) |> List.truncate maximumFindings

    let private transformDimensions transform width height =
        match transform with ParcelTransform.Identity | ParcelTransform.Rotate180 -> width, height | ParcelTransform.Rotate90 | ParcelTransform.Rotate270 -> height, width
    let private transformCell transform width height (cell: EnvironmentCell) =
        match transform with
        | ParcelTransform.Identity -> cell
        | ParcelTransform.Rotate90 -> { EnvironmentColumn = height - 1 - cell.EnvironmentRow; EnvironmentRow = cell.EnvironmentColumn }
        | ParcelTransform.Rotate180 -> { EnvironmentColumn = width - 1 - cell.EnvironmentColumn; EnvironmentRow = height - 1 - cell.EnvironmentRow }
        | ParcelTransform.Rotate270 -> { EnvironmentColumn = cell.EnvironmentRow; EnvironmentRow = width - 1 - cell.EnvironmentColumn }
    let private translate (origin: EnvironmentCell) (cell: EnvironmentCell) = { EnvironmentColumn = origin.EnvironmentColumn + cell.EnvironmentColumn; EnvironmentRow = origin.EnvironmentRow + cell.EnvironmentRow }
    let private adjacent (edge: EnvironmentEdge) = match edge.EdgeDirection with EnvironmentEdgeDirection.East -> { edge.EdgeCell with EnvironmentColumn = edge.EdgeCell.EnvironmentColumn + 1 } | EnvironmentEdgeDirection.South -> { edge.EdgeCell with EnvironmentRow = edge.EdgeCell.EnvironmentRow + 1 }
    let private edgeBetween (left: EnvironmentCell) (right: EnvironmentCell) : EnvironmentEdge =
        if left.EnvironmentRow = right.EnvironmentRow then
            { EdgeCell = (if left.EnvironmentColumn < right.EnvironmentColumn then left else right)
              EdgeDirection = EnvironmentEdgeDirection.East }
        else
            { EdgeCell = (if left.EnvironmentRow < right.EnvironmentRow then left else right)
              EdgeDirection = EnvironmentEdgeDirection.South }
    let private transformFeature slotId transform width height origin (feature: EnvironmentFeature) =
        let left = transformCell transform width height feature.EnvironmentEdge.EdgeCell |> translate origin
        let right = adjacent feature.EnvironmentEdge |> transformCell transform width height |> translate origin
        let placedId = slotId + ":" + feature.EnvironmentFeatureId
        { feature with EnvironmentFeatureId = placedId; EnvironmentEdge = edgeBetween left right; QueryDependencyKeys = feature.QueryDependencyKeys |> List.map (fun key -> key + ":" + placedId) |> List.distinct |> List.sort }

    // The published Game.Core Fable profile classifies sequential RNG as
    // DotNetOnly. Parcel selection is therefore product-owned addressed
    // randomness: seed + stable slot id -> portable SHA-256 -> one index.
    let private selectionIndex (seed: uint64) (slotId: string) (count: int32) =
        let seedBytes = [| for shift in 0 .. 8 .. 56 -> byte (seed >>> shift) |]
        let digest = CanonicalEncoding.concatenate [ seedBytes; Encoding.UTF8.GetBytes slotId ] |> CanonicalHash.sha256
        let value =
            uint64 digest[0]
            ||| (uint64 digest[1] <<< 8)
            ||| (uint64 digest[2] <<< 16)
            ||| (uint64 digest[3] <<< 24)
            ||| (uint64 digest[4] <<< 32)
            ||| (uint64 digest[5] <<< 40)
            ||| (uint64 digest[6] <<< 48)
            ||| (uint64 digest[7] <<< 56)
        int (value % uint64 count)

    let assemble seed plot variants =
        match validate plot variants with
        | findings when not findings.IsEmpty -> Error findings
        | _ ->
            let mutable inspected = 0
            let choices =
                [ for slot in plot.PlotSlots |> List.sortBy _.PlotSlotId do
                    let candidates =
                        [ for variant in variants |> List.filter (fun value -> value.ParcelRole = slot.PlotSlotRole) |> List.sortBy _.ParcelVariantId do
                            inspected <- inspected + 1
                            for transform in [ ParcelTransform.Identity; ParcelTransform.Rotate90; ParcelTransform.Rotate180; ParcelTransform.Rotate270 ] do
                                let width, height = transformDimensions transform variant.ParcelWidth variant.ParcelHeight
                                if width <= slot.PlotSlotWidth && height <= slot.PlotSlotHeight then yield variant, transform ]
                    let index = selectionIndex seed slot.PlotSlotId candidates.Length
                    yield slot, fst candidates[index], snd candidates[index] ]
            let placements = choices |> List.map (fun (slot, variant, transform) -> { PlacementSlotId = slot.PlotSlotId; PlacementVariantId = variant.ParcelVariantId; PlacementTransform = transform; PlacementOrigin = slot.PlotSlotOrigin })
            let cells = choices |> List.collect (fun (slot, variant, transform) -> variant.ParcelWalkableCells |> List.map (transformCell transform variant.ParcelWidth variant.ParcelHeight >> translate slot.PlotSlotOrigin)) |> List.distinct |> List.sort
            let objectives = choices |> List.collect (fun (slot, variant, transform) -> variant.ParcelObjectiveCells |> List.map (transformCell transform variant.ParcelWidth variant.ParcelHeight >> translate slot.PlotSlotOrigin)) |> List.distinct |> List.sort
            let features = choices |> List.collect (fun (slot, variant, transform) -> variant.ParcelFeatures |> List.map (transformFeature slot.PlotSlotId transform variant.ParcelWidth variant.ParcelHeight slot.PlotSlotOrigin)) |> List.sortBy _.EnvironmentFeatureId
            { EnvironmentSchemaVersion = SIR.Domain.TacticalEnvironment.schemaVersion; AssembledPlotId = plot.AuthoredPlotId; AssemblySeed = seed; ParcelPlacements = placements; AssembledWalkableCells = cells; AssembledObjectiveCells = objectives; EnvironmentFeatures = features; EnvironmentContentIdentity = ""; EnvironmentSpatialRevision = 0L; AssemblyCostCounters = { SlotsVisited = plot.PlotSlots.Length; VariantsInspected = inspected; Selections = choices.Length; PlacedCells = cells.Length; PlacedFeatures = features.Length } }
            |> SIR.Domain.TacticalEnvironment.withContentIdentity |> Ok

    let observe (knowledge: EnvironmentKnowledge) (environment: AssembledEnvironment) featureId =
        environment.EnvironmentFeatures |> List.tryFind (fun feature -> feature.EnvironmentFeatureId = featureId) |> Option.bind (fun feature ->
            if not (Set.contains featureId knowledge.KnownEnvironmentFeatureIds) then None else
            let capabilities = feature.CapabilityDescriptors |> List.filter (fun cap -> cap.RequiredKnowledgeFact |> Option.forall (fun fact -> Set.contains fact knowledge.KnownEnvironmentFacts)) |> List.sortBy _.DescriptorId
            Some { ObservationSchemaVersion = SIR.Domain.TacticalEnvironment.schemaVersion; ObservationFeatureId = feature.EnvironmentFeatureId; ObservationKind = feature.EnvironmentKind; ObservedState = Some feature.EnvironmentState; AvailableCapabilities = capabilities; ObservationSpatialRevision = environment.EnvironmentSpatialRevision; ObservationKnowledgeIdentity = knowledge.EnvironmentKnowledgeIdentity; ObservationKnowledgeRevision = knowledge.EnvironmentKnowledgeRevision })

    let private actionName = function EnvironmentAction.Open -> "open" | EnvironmentAction.Close -> "close" | EnvironmentAction.Damage _ -> "damage" | EnvironmentAction.Breach _ -> "breach" | EnvironmentAction.Destroy -> "destroy"
    let private nextState (feature: EnvironmentFeature) action =
        match action, feature.EnvironmentKind, feature.EnvironmentState with
        | EnvironmentAction.Open, (EnvironmentFeatureKind.Door | EnvironmentFeatureKind.Window), (EnvironmentFeatureState.Closed | EnvironmentFeatureState.Damaged) -> Ok EnvironmentFeatureState.Open
        | EnvironmentAction.Close, (EnvironmentFeatureKind.Door | EnvironmentFeatureKind.Window), EnvironmentFeatureState.Open -> Ok EnvironmentFeatureState.Closed
        | EnvironmentAction.Breach cost, _, state when cost <= 0 -> Error EnvironmentActionFailure.InvalidCost
        | EnvironmentAction.Breach _, (EnvironmentFeatureKind.Door | EnvironmentFeatureKind.Window | EnvironmentFeatureKind.Wall), (EnvironmentFeatureState.Intact | EnvironmentFeatureState.Closed | EnvironmentFeatureState.Damaged) -> Ok EnvironmentFeatureState.Breached
        | EnvironmentAction.Damage amount, _, _ when amount <= 0 -> Error EnvironmentActionFailure.InvalidCost
        | EnvironmentAction.Damage _, _, EnvironmentFeatureState.Destroyed -> Ok EnvironmentFeatureState.Destroyed
        | EnvironmentAction.Damage amount, _, _ -> match feature.DirectionalCover with Some cover when amount >= cover.CoverIntegrity -> Ok EnvironmentFeatureState.Destroyed | _ -> Ok EnvironmentFeatureState.Damaged
        | EnvironmentAction.Destroy, _, EnvironmentFeatureState.Destroyed -> Ok EnvironmentFeatureState.Destroyed
        | EnvironmentAction.Destroy, _, _ -> Ok EnvironmentFeatureState.Destroyed
        | _ -> Error EnvironmentActionFailure.UnsupportedAction

    let applyAction (knowledge: EnvironmentKnowledge) expectedContentIdentity featureId action (environment: AssembledEnvironment) =
        if not (mutationEnabled "SIR_TACTICAL_MUTATE_CONTENT_IDENTITY") && not (String.Equals(expectedContentIdentity, environment.EnvironmentContentIdentity, StringComparison.Ordinal) && SIR.Domain.TacticalEnvironment.identityMatches environment) then Error EnvironmentActionFailure.StaleContentIdentity
        elif not (Set.contains featureId knowledge.KnownEnvironmentFeatureIds) then Error EnvironmentActionFailure.HiddenFeature
        else
            match environment.EnvironmentFeatures |> List.tryFindIndex (fun feature -> feature.EnvironmentFeatureId = featureId) with
            | None -> Error EnvironmentActionFailure.MissingFeature
            | Some index ->
                let feature = environment.EnvironmentFeatures[index]
                let suppliedCost = match action with EnvironmentAction.Breach cost | EnvironmentAction.Damage cost -> cost | _ -> 1
                let allowed = feature.CapabilityDescriptors |> List.exists (fun capability -> String.Equals(capability.DescriptorAction, actionName action, StringComparison.Ordinal) && suppliedCost >= capability.DescriptorCost && suppliedCost <= maximumTargetedActionCost && capability.RequiredKnowledgeFact |> Option.forall (fun fact -> Set.contains fact knowledge.KnownEnvironmentFacts))
                if not allowed then Error EnvironmentActionFailure.UnsupportedAction else
                nextState feature action |> Result.map (fun state ->
                    let changed = state <> feature.EnvironmentState
                    let cover =
                        match feature.DirectionalCover, action with
                        | Some value, EnvironmentAction.Damage amount -> Some { value with CoverIntegrity = max 0 (value.CoverIntegrity - amount) }
                        | Some value, EnvironmentAction.Destroy -> Some { value with CoverIntegrity = 0 }
                        | value, _ -> value
                    let nextFeature = { feature with EnvironmentState = state; ModalityPermeability = defaultPermeability feature.EnvironmentKind state; DirectionalCover = cover }
                    let mutatePropagation = mutationEnabled "SIR_TACTICAL_MUTATE_DESTRUCTION_BOUND"
                    let features = environment.EnvironmentFeatures |> List.mapi (fun i value -> if i = index then nextFeature elif mutatePropagation then { value with EnvironmentState = EnvironmentFeatureState.Destroyed; ModalityPermeability = defaultPermeability value.EnvironmentKind EnvironmentFeatureState.Destroyed } else value)
                    let next = if changed then { environment with EnvironmentFeatures = features; EnvironmentSpatialRevision = environment.EnvironmentSpatialRevision + 1L; EnvironmentContentIdentity = "" } |> SIR.Domain.TacticalEnvironment.withContentIdentity else environment
                    let dependencies = if changed then feature.QueryDependencyKeys |> Set.ofList else Set.empty
                    { UpdatedEnvironment = next; ChangedQueryDependencies = dependencies; ActionCostCounters = { FeaturesInspected = 1; FeaturesChanged = (if changed then 1 else 0); DependenciesEmitted = dependencies.Count; PropagatedChanges = (if mutatePropagation then max 0 (features.Length - 1) else 0) } })

    let private toCell (value: EnvironmentCell) : Cell = { Col = value.EnvironmentColumn; Row = value.EnvironmentRow }
    let toSpatialWorld rulesetIdentity (knowledge: EnvironmentKnowledge) (environment: AssembledEnvironment) : ProjectedSpatialWorld =
        let boundaries : SpatialBoundary list =
            environment.EnvironmentFeatures |> List.choose (fun feature ->
                let permeability = if mutationEnabled "SIR_TACTICAL_MUTATE_EDGE_STATE" then { feature.ModalityPermeability with AllowsMovement = true; AllowsSight = true; AllowsProjectile = true } else feature.ModalityPermeability
                Edges.edgeBetween (toCell feature.EnvironmentEdge.EdgeCell) (toCell (adjacent feature.EnvironmentEdge)) |> Option.map (fun routingEdge -> { Edge = routingEdge; Permeability = ({ Ground = permeability.AllowsMovement; Vision = permeability.AllowsSight; Projectile = permeability.AllowsProjectile } : SpatialPermeability); RevisionToken = feature.QueryDependencyKeys |> List.sort |> List.tryHead |> Option.defaultValue ("feature:" + feature.EnvironmentFeatureId) }))
        let cells = environment.AssembledWalkableCells |> List.map toCell
        let minimum = if cells.IsEmpty then { Col = 0; Row = 0 } else { Col = cells |> List.minBy _.Col |> _.Col; Row = cells |> List.minBy _.Row |> _.Row }
        let maximum = if cells.IsEmpty then minimum else { Col = cells |> List.maxBy _.Col |> _.Col; Row = cells |> List.maxBy _.Row |> _.Row }
        { Identity = { MapIdentity = environment.EnvironmentContentIdentity; RulesetIdentity = rulesetIdentity; SpatialRevision = environment.EnvironmentSpatialRevision; KnowledgeIdentity = knowledge.EnvironmentKnowledgeIdentity; KnowledgeRevision = knowledge.EnvironmentKnowledgeRevision }
          Minimum = minimum; Maximum = maximum; Terrain = cells |> List.map (fun cell -> cell, SpatialTerrain.Open) |> Map.ofList; Boundaries = boundaries; Occupancy = Map.empty; DisclosedRevisionTokens = boundaries |> List.map _.RevisionToken |> Set.ofList }

    let invalidateCache (result: EnvironmentActionResult) (cache: SpatialCache) =
        let inspected = cache.DynamicEntries.Length
        let dependencies = if mutationEnabled "SIR_TACTICAL_MUTATE_DEPENDENCY_LOCALITY" then Set.empty else result.ChangedQueryDependencies
        let next = SpatialQuery.invalidate dependencies cache
        next, inspected, inspected - next.DynamicEntries.Length

    let private capability id action cost = { DescriptorId = id; DescriptorAction = action; DescriptorCost = cost; RequiredKnowledgeFact = None }
    let private feature id kind state cell direction cover capabilities : EnvironmentFeature = { EnvironmentFeatureId = id; EnvironmentKind = kind; EnvironmentState = state; EnvironmentEdge = { EdgeCell = cell; EdgeDirection = direction }; ModalityPermeability = defaultPermeability kind state; DirectionalCover = cover; CapabilityDescriptors = capabilities; QueryDependencyKeys = [ "feature:" + id ] }
    let private cover material directions = Some { CoverMaterial = material; CoverIntegrity = 100; CoverMaximumIntegrity = 100; CoverPenetrationResistance = 40; CoverProtectedDirections = directions }
    let private cells width height = [ for row in 0 .. height - 1 do for col in 0 .. width - 1 -> { EnvironmentColumn = col; EnvironmentRow = row } ]
    let private singlePlot id role = { PlotSchemaVersion = 1; AuthoredPlotId = id; PlotWidth = 8; PlotHeight = 8; PlotSlots = [ { PlotSlotId = "slot-1"; PlotSlotRole = role; PlotSlotOrigin = { EnvironmentColumn = 0; EnvironmentRow = 0 }; PlotSlotWidth = 8; PlotSlotHeight = 8; ConnectedPlotSlotIds = []; PlotSlotRequiresRoute = true } ] }
    let exteriorParcelSet =
        let plot = singlePlot "exterior-yard" "exterior"
        let variant = { ParcelVariantId = "exterior-cover-yard-a"; ParcelRole = "exterior"; ParcelWidth = 8; ParcelHeight = 8; ParcelWalkableCells = cells 8 8; ParcelObjectiveCells = [ { EnvironmentColumn = 7; EnvironmentRow = 7 } ]; ParcelConnections = []; ParcelFeatures = [ feature "yard-door" EnvironmentFeatureKind.Door EnvironmentFeatureState.Closed { EnvironmentColumn = 3; EnvironmentRow = 3 } EnvironmentEdgeDirection.East None [ capability "open-door" "open" 1; capability "breach-door" "breach" 2 ]; feature "yard-cover" EnvironmentFeatureKind.Cover EnvironmentFeatureState.Intact { EnvironmentColumn = 5; EnvironmentRow = 4 } EnvironmentEdgeDirection.South (cover "sandbag" [ Direction8.North ]) [ capability "damage-cover" "damage" 1; capability "destroy-cover" "destroy" 1 ] ] }
        plot, [ variant ]
    let interiorBreachParcelSet =
        let plot = singlePlot "interior-breach" "interior"
        let variant = { ParcelVariantId = "interior-breach-a"; ParcelRole = "interior"; ParcelWidth = 8; ParcelHeight = 8; ParcelWalkableCells = cells 8 8; ParcelObjectiveCells = [ { EnvironmentColumn = 6; EnvironmentRow = 6 } ]; ParcelConnections = []; ParcelFeatures = [ feature "interior-wall" EnvironmentFeatureKind.Wall EnvironmentFeatureState.Intact { EnvironmentColumn = 3; EnvironmentRow = 3 } EnvironmentEdgeDirection.East (cover "masonry" [ Direction8.East; Direction8.West ]) [ capability "breach-wall" "breach" 3; capability "damage-wall" "damage" 1; capability "destroy-wall" "destroy" 1 ]; feature "interior-window" EnvironmentFeatureKind.Window EnvironmentFeatureState.Closed { EnvironmentColumn = 2; EnvironmentRow = 2 } EnvironmentEdgeDirection.South None [ capability "open-window" "open" 1; capability "breach-window" "breach" 2 ] ] }
        plot, [ variant ]

    let workload seed plot variants cache =
        let findings = validate plot variants
        if not findings.IsEmpty then Error findings else
        assemble seed plot variants |> Result.map (fun environment ->
            let known = { EnvironmentKnowledgeIdentity = "workload"; EnvironmentKnowledgeRevision = 0L; KnownEnvironmentFeatureIds = environment.EnvironmentFeatures |> List.map _.EnvironmentFeatureId |> Set.ofList; KnownEnvironmentFacts = Set.empty }
            let inspected, invalidated =
                match environment.EnvironmentFeatures |> List.tryHead with
                | None -> cache.DynamicEntries.Length, 0
                | Some feature ->
                    match applyAction known environment.EnvironmentContentIdentity feature.EnvironmentFeatureId (if feature.CapabilityDescriptors |> List.exists (fun cap -> cap.DescriptorAction = "destroy") then EnvironmentAction.Destroy else EnvironmentAction.Damage 1) environment with
                    | Ok result -> let _, seen, removed = invalidateCache result cache in seen, removed
                    | Error _ -> cache.DynamicEntries.Length, 0
            { WorkloadSlots = environment.AssemblyCostCounters.SlotsVisited; WorkloadVariantsInspected = environment.AssemblyCostCounters.VariantsInspected; WorkloadFindings = findings.Length; WorkloadFeatures = environment.AssemblyCostCounters.PlacedFeatures; DependencyEntriesInspected = inspected; DependencyEntriesInvalidated = invalidated; WorkloadQueryCount = 0 })
