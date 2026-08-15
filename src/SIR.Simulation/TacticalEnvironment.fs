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

    let allowsModality modality (feature: EnvironmentFeature) =
        match modality with
        | EnvironmentModality.Movement -> feature.ModalityPermeability.AllowsMovement
        | EnvironmentModality.Sight -> feature.ModalityPermeability.AllowsSight
        | EnvironmentModality.Projectile -> feature.ModalityPermeability.AllowsProjectile
        | EnvironmentModality.AreaEffect -> feature.ModalityPermeability.AllowsAreaEffect
        | EnvironmentModality.Sound -> feature.ModalityPermeability.AllowsSound
        | EnvironmentModality.Cover -> feature.ModalityPermeability.ProvidesCover
        | EnvironmentModality.Interaction capabilityId ->
            feature.ModalityPermeability.AllowsInteraction
            && feature.CapabilityDescriptors |> List.exists (fun capability -> capability.DescriptorId = capabilityId)

    let coverAt direction (feature: EnvironmentFeature) =
        feature.DirectionalCover
        |> Option.filter (fun cover ->
            feature.ModalityPermeability.ProvidesCover
            && cover.CoverIntegrity > 0
            && List.contains direction cover.CoverProtectedDirections)

    let private finding code subject message = { ValidationCode = code; ValidationSubject = subject; ValidationMessage = message }
    let private duplicates values = values |> List.countBy id |> List.choose (fun (value, count) -> if count > 1 then Some value else None)
    let private within width height (cell: EnvironmentCell) = cell.EnvironmentColumn >= 0 && cell.EnvironmentRow >= 0 && cell.EnvironmentColumn < width && cell.EnvironmentRow < height
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
    let private transformedConnection (slot: PlotSlot) transform (variant: ParcelVariant) (connection: ParcelConnection) =
        let left = transformCell transform variant.ParcelWidth variant.ParcelHeight connection.ConnectionCell |> translate slot.PlotSlotOrigin
        let source = { EdgeCell = connection.ConnectionCell; EdgeDirection = connection.ConnectionDirection }
        let right = adjacent source |> transformCell transform variant.ParcelWidth variant.ParcelHeight |> translate slot.PlotSlotOrigin
        connection.ConnectionRole, edgeBetween left right
    let private fittingCandidates (slot: PlotSlot) variants =
        [ for variant in variants |> List.filter (fun value -> value.ParcelRole = slot.PlotSlotRole) |> List.sortBy _.ParcelVariantId do
            for transform in [ ParcelTransform.Identity; ParcelTransform.Rotate90; ParcelTransform.Rotate180; ParcelTransform.Rotate270 ] do
                let width, height = transformDimensions transform variant.ParcelWidth variant.ParcelHeight
                if width <= slot.PlotSlotWidth && height <= slot.PlotSlotHeight then yield variant, transform ]
    let private variantsConnect leftSlot (leftVariant, leftTransform) rightSlot (rightVariant, rightTransform) =
        let left = leftVariant.ParcelConnections |> List.map (transformedConnection leftSlot leftTransform leftVariant)
        let right = rightVariant.ParcelConnections |> List.map (transformedConnection rightSlot rightTransform rightVariant)
        left |> List.exists (fun (leftRole, leftEdge) -> right |> List.exists (fun (rightRole, rightEdge) -> leftRole = rightRole && leftEdge = rightEdge))
    let private requiredRouteReachable (variant: ParcelVariant) =
        let walkable = Collections.Generic.HashSet<EnvironmentCell>(variant.ParcelWalkableCells)
        let starts =
            variant.ParcelConnections
            |> List.map _.ConnectionCell
            |> List.filter walkable.Contains
            |> function [] -> variant.ParcelWalkableCells |> List.sort |> List.truncate 1 | values -> values
        let reached = Collections.Generic.HashSet<EnvironmentCell>()
        let pending = Collections.Generic.Queue<EnvironmentCell>()
        starts |> List.iter pending.Enqueue
        while pending.Count > 0 do
            let cell = pending.Dequeue()
            if reached.Add cell then
                [ { cell with EnvironmentColumn = cell.EnvironmentColumn - 1 }
                  { cell with EnvironmentColumn = cell.EnvironmentColumn + 1 }
                  { cell with EnvironmentRow = cell.EnvironmentRow - 1 }
                  { cell with EnvironmentRow = cell.EnvironmentRow + 1 } ]
                |> List.iter (fun candidate -> if walkable.Contains candidate && not (reached.Contains candidate) then pending.Enqueue candidate)
        not starts.IsEmpty && variant.ParcelObjectiveCells |> List.forall reached.Contains

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
        let fittingBySlot = plot.PlotSlots |> List.map (fun slot -> slot.PlotSlotId, fittingCandidates slot variants) |> Map.ofList
        for slot in plot.PlotSlots |> List.sortBy _.PlotSlotId do
            if slot.PlotSlotWidth <= 0 || slot.PlotSlotHeight <= 0 || slot.PlotSlotOrigin.EnvironmentColumn < 0 || slot.PlotSlotOrigin.EnvironmentRow < 0 || slot.PlotSlotOrigin.EnvironmentColumn + slot.PlotSlotWidth > plot.PlotWidth || slot.PlotSlotOrigin.EnvironmentRow + slot.PlotSlotHeight > plot.PlotHeight then add EnvironmentValidationCode.ImpossibleFootprint slot.PlotSlotId "Slot footprint is outside the plot."
            for target in slot.ConnectedPlotSlotIds |> List.distinct |> List.sort do
                if not (Set.contains target slotIds) then add EnvironmentValidationCode.DisconnectedSlot slot.PlotSlotId ("Connection targets missing slot " + target + ".")
                elif not (plot.PlotSlots |> List.exists (fun other -> other.PlotSlotId = target && List.contains slot.PlotSlotId other.ConnectedPlotSlotIds)) then add EnvironmentValidationCode.ConnectorMismatch slot.PlotSlotId ("Connection to " + target + " is not reciprocal.")
            let compatible = variants |> List.filter (fun variant -> variant.ParcelRole = slot.PlotSlotRole && ((variant.ParcelWidth <= slot.PlotSlotWidth && variant.ParcelHeight <= slot.PlotSlotHeight) || (variant.ParcelHeight <= slot.PlotSlotWidth && variant.ParcelWidth <= slot.PlotSlotHeight)))
            if List.isEmpty compatible then add EnvironmentValidationCode.ImpossibleFootprint slot.PlotSlotId "No compatible parcel variant fits this slot."
            if not slot.ConnectedPlotSlotIds.IsEmpty && compatible |> List.exists (fun variant -> variant.ParcelConnections.IsEmpty) then add EnvironmentValidationCode.ConnectorMismatch slot.PlotSlotId "A connected slot has a compatible variant without a connector."
            for targetId in slot.ConnectedPlotSlotIds |> List.filter (fun id -> String.CompareOrdinal(slot.PlotSlotId, id) < 0) |> List.distinct |> List.sort do
                match plot.PlotSlots |> List.tryFind (fun candidate -> candidate.PlotSlotId = targetId) with
                | None -> ()
                | Some target ->
                    let leftCandidates = Map.find slot.PlotSlotId fittingBySlot
                    let rightCandidates = Map.find target.PlotSlotId fittingBySlot
                    if leftCandidates.IsEmpty || rightCandidates.IsEmpty || not (leftCandidates |> List.exists (fun left -> rightCandidates |> List.exists (variantsConnect slot left target))) then
                        add EnvironmentValidationCode.ConnectorMismatch (slot.PlotSlotId + ":" + targetId) "Connected slots have no transform-aware connector with the same role and global edge."
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
            if plot.PlotSlots |> List.exists (fun slot -> slot.PlotSlotRequiresRoute && slot.PlotSlotRole = variant.ParcelRole) && not (requiredRouteReachable variant) then add EnvironmentValidationCode.UnreachableRoute variant.ParcelVariantId "Parcel does not provide a contiguous required route to every objective."
            for cell in variant.ParcelWalkableCells @ variant.ParcelObjectiveCells @ (variant.ParcelFeatures |> List.collect _.EnvironmentFeatureCells) do if not (within variant.ParcelWidth variant.ParcelHeight cell) then add EnvironmentValidationCode.ImpossibleFootprint variant.ParcelVariantId "Variant cell is outside its footprint."
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

    let private transformFeature slotId transform width height origin (feature: EnvironmentFeature) =
        let left = transformCell transform width height feature.EnvironmentEdge.EdgeCell |> translate origin
        let right = adjacent feature.EnvironmentEdge |> transformCell transform width height |> translate origin
        let placedId = slotId + ":" + feature.EnvironmentFeatureId
        { feature with EnvironmentFeatureId = placedId; EnvironmentEdge = edgeBetween left right; EnvironmentFeatureCells = feature.EnvironmentFeatureCells |> List.map (transformCell transform width height >> translate origin) |> List.distinct |> List.sort; QueryDependencyKeys = feature.QueryDependencyKeys |> List.map (fun key -> key + ":" + placedId) |> List.distinct |> List.sort }

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

    let private sha256Hex (bytes: byte array) =
        bytes |> CanonicalHash.sha256 |> Array.map (fun value -> value.ToString("x2")) |> String.concat ""

    let private authoredInputIdentity (plot: AuthoredPlot) (variants: ParcelVariant list) =
        let cellText cell = $"{cell.EnvironmentColumn},{cell.EnvironmentRow}"
        let boolText value = if value then "1" else "0"
        let edgeDirectionText = function EnvironmentEdgeDirection.East -> "0" | EnvironmentEdgeDirection.South -> "1"
        let kindText = function EnvironmentFeatureKind.Door -> "0" | EnvironmentFeatureKind.Window -> "1" | EnvironmentFeatureKind.Wall -> "2" | EnvironmentFeatureKind.Cover -> "3"
        let stateText = function EnvironmentFeatureState.Intact -> "0" | EnvironmentFeatureState.Closed -> "1" | EnvironmentFeatureState.Open -> "2" | EnvironmentFeatureState.Damaged -> "3" | EnvironmentFeatureState.Breached -> "4" | EnvironmentFeatureState.Destroyed -> "5"
        let edgeText edge = $"{cellText edge.EdgeCell},{edgeDirectionText edge.EdgeDirection}"
        let permeabilityText value =
            [ value.AllowsMovement; value.AllowsSight; value.AllowsProjectile; value.AllowsAreaEffect; value.AllowsSound; value.ProvidesCover; value.AllowsInteraction ]
            |> List.map boolText
            |> String.concat ""
        let coverText = function
            | None -> "-"
            | Some cover ->
                let directions = cover.CoverProtectedDirections |> List.map (Direction8.toCode >> string) |> List.sort |> String.concat ","
                $"{cover.CoverMaterial},{cover.CoverIntegrity},{cover.CoverMaximumIntegrity},{cover.CoverPenetrationResistance},{directions}"
        let capabilityText capability =
            let requiredFact = Option.defaultValue "" capability.RequiredKnowledgeFact
            $"{capability.DescriptorId},{capability.DescriptorAction},{capability.DescriptorCost},{requiredFact}"
        let featureText feature =
            let volumeCells = feature.EnvironmentFeatureCells |> List.distinct |> List.sort |> List.map cellText |> String.concat ";"
            let capabilities = feature.CapabilityDescriptors |> List.sortBy _.DescriptorId |> List.map capabilityText |> String.concat ";"
            let dependencies = feature.QueryDependencyKeys |> List.distinct |> List.sort |> String.concat ";"
            $"feature|{feature.EnvironmentFeatureId}|{kindText feature.EnvironmentKind}|{stateText feature.EnvironmentState}|{edgeText feature.EnvironmentEdge}|{volumeCells}|{permeabilityText feature.ModalityPermeability}|{coverText feature.DirectionalCover}|{capabilities}|{dependencies}"
        let slotLines =
            plot.PlotSlots
            |> List.sortBy _.PlotSlotId
            |> List.map (fun slot ->
                let connected = slot.ConnectedPlotSlotIds |> List.distinct |> List.sort |> String.concat ";"
                $"slot|{slot.PlotSlotId}|{slot.PlotSlotRole}|{cellText slot.PlotSlotOrigin}|{slot.PlotSlotWidth},{slot.PlotSlotHeight}|{connected}|{boolText slot.PlotSlotRequiresRoute}")
        let variantLines =
            variants
            |> List.sortBy _.ParcelVariantId
            |> List.collect (fun variant ->
                let walkable = variant.ParcelWalkableCells |> List.distinct |> List.sort |> List.map cellText |> String.concat ";"
                let objectives = variant.ParcelObjectiveCells |> List.distinct |> List.sort |> List.map cellText |> String.concat ";"
                [ yield $"variant|{variant.ParcelVariantId}|{variant.ParcelRole}|{variant.ParcelWidth},{variant.ParcelHeight}|{walkable}|{objectives}"
                  for connection in variant.ParcelConnections |> List.sortBy _.ConnectionId do
                      yield $"connection|{variant.ParcelVariantId}|{connection.ConnectionId}|{cellText connection.ConnectionCell}|{edgeDirectionText connection.ConnectionDirection}|{connection.ConnectionRole}"
                  for feature in variant.ParcelFeatures |> List.sortBy _.EnvironmentFeatureId do
                      yield $"{variant.ParcelVariantId}|{featureText feature}" ])
        ($"plot|{plot.PlotSchemaVersion}|{plot.AuthoredPlotId}|{plot.PlotWidth},{plot.PlotHeight}" :: slotLines @ variantLines)
        |> String.concat "\n"
        |> fun value -> Encoding.UTF8.GetBytes(value: string)
        |> sha256Hex

    let assemble seed plot variants =
        match validate plot variants with
        | findings when not findings.IsEmpty -> Error findings
        | _ ->
            let mutable inspected = 0
            let choices =
                (([], plot.PlotSlots |> List.sortBy _.PlotSlotId)
                 ||> List.fold (fun selected slot ->
                     let fitting = fittingCandidates slot variants
                     let candidates =
                         if slot.ConnectedPlotSlotIds.IsEmpty
                         then fitting |> List.groupBy (fst >> _.ParcelVariantId) |> List.map (snd >> List.head)
                         else fitting
                     inspected <- inspected + (candidates |> List.map (fst >> _.ParcelVariantId) |> List.distinct |> List.length)
                     let priorConnections =
                         slot.ConnectedPlotSlotIds
                         |> List.choose (fun connectedId -> selected |> List.tryFind (fun (otherSlot, _, _) -> otherSlot.PlotSlotId = connectedId))
                     let compatible =
                         candidates
                         |> List.filter (fun candidate ->
                             priorConnections
                             |> List.forall (fun (otherSlot, otherVariant, otherTransform) ->
                                 variantsConnect slot candidate otherSlot (otherVariant, otherTransform)))
                     let pool = if compatible.IsEmpty then candidates else compatible
                     let index = selectionIndex seed slot.PlotSlotId pool.Length
                     let variant, transform = pool[index]
                     selected @ [ slot, variant, transform ]))
            let placements = choices |> List.map (fun (slot, variant, transform) -> { PlacementSlotId = slot.PlotSlotId; PlacementVariantId = variant.ParcelVariantId; PlacementTransform = transform; PlacementOrigin = slot.PlotSlotOrigin })
            let cells = choices |> List.collect (fun (slot, variant, transform) -> variant.ParcelWalkableCells |> List.map (transformCell transform variant.ParcelWidth variant.ParcelHeight >> translate slot.PlotSlotOrigin)) |> List.distinct |> List.sort
            let objectives = choices |> List.collect (fun (slot, variant, transform) -> variant.ParcelObjectiveCells |> List.map (transformCell transform variant.ParcelWidth variant.ParcelHeight >> translate slot.PlotSlotOrigin)) |> List.distinct |> List.sort
            let features = choices |> List.collect (fun (slot, variant, transform) -> variant.ParcelFeatures |> List.map (transformFeature slot.PlotSlotId transform variant.ParcelWidth variant.ParcelHeight slot.PlotSlotOrigin)) |> List.sortBy _.EnvironmentFeatureId
            { EnvironmentSchemaVersion = SIR.Domain.TacticalEnvironment.schemaVersion; AssembledPlotId = plot.AuthoredPlotId; AssemblySeed = seed; ParcelPlacements = placements; AssembledWalkableCells = cells; AssembledObjectiveCells = objectives; EnvironmentFeatures = features; EnvironmentAssemblyIdentity = authoredInputIdentity plot variants; EnvironmentContentIdentity = ""; EnvironmentSpatialRevision = 0L; AssemblyCostCounters = { SlotsVisited = plot.PlotSlots.Length; VariantsInspected = inspected; Selections = choices.Length; PlacedCells = cells.Length; PlacedFeatures = features.Length } }
            |> SIR.Domain.TacticalEnvironment.withContentIdentity |> Ok

    let private actionName = function EnvironmentAction.Open -> "open" | EnvironmentAction.Close -> "close" | EnvironmentAction.Damage _ -> "damage" | EnvironmentAction.Breach _ -> "breach" | EnvironmentAction.Destroy -> "destroy"
    let private actionSupported (feature: EnvironmentFeature) action =
        match action, feature.EnvironmentKind, feature.EnvironmentState with
        | "open", (EnvironmentFeatureKind.Door | EnvironmentFeatureKind.Window), (EnvironmentFeatureState.Closed | EnvironmentFeatureState.Damaged)
        | "close", (EnvironmentFeatureKind.Door | EnvironmentFeatureKind.Window), EnvironmentFeatureState.Open
        | "breach", (EnvironmentFeatureKind.Door | EnvironmentFeatureKind.Window | EnvironmentFeatureKind.Wall), (EnvironmentFeatureState.Intact | EnvironmentFeatureState.Closed | EnvironmentFeatureState.Damaged)
        | "damage", _, (EnvironmentFeatureState.Intact | EnvironmentFeatureState.Closed | EnvironmentFeatureState.Open | EnvironmentFeatureState.Damaged | EnvironmentFeatureState.Breached)
        | "destroy", _, (EnvironmentFeatureState.Intact | EnvironmentFeatureState.Closed | EnvironmentFeatureState.Open | EnvironmentFeatureState.Damaged | EnvironmentFeatureState.Breached) -> true
        | _ -> false

    let observe (knowledge: EnvironmentKnowledge) (environment: AssembledEnvironment) featureId =
        environment.EnvironmentFeatures |> List.tryFind (fun feature -> feature.EnvironmentFeatureId = featureId) |> Option.bind (fun feature ->
            if not (Set.contains featureId knowledge.KnownEnvironmentFeatureIds) then None else
            let capabilities = feature.CapabilityDescriptors |> List.filter (fun cap -> actionSupported feature cap.DescriptorAction && cap.RequiredKnowledgeFact |> Option.forall (fun fact -> Set.contains fact knowledge.KnownEnvironmentFacts)) |> List.sortBy _.DescriptorId
            Some
                { ObservationSchemaVersion = SIR.Domain.TacticalEnvironment.schemaVersion
                  ObservationFeatureId = feature.EnvironmentFeatureId
                  ObservationKind = feature.EnvironmentKind
                  ObservedState = (if Set.contains featureId knowledge.KnownEnvironmentStateFeatureIds then Some feature.EnvironmentState else None)
                  AvailableCapabilities = capabilities
                  ObservationSpatialRevision = environment.EnvironmentSpatialRevision
                  ObservationKnowledgeIdentity = knowledge.EnvironmentKnowledgeIdentity
                  ObservationKnowledgeRevision = knowledge.EnvironmentKnowledgeRevision })

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
                let allowed = actionSupported feature (actionName action) && feature.CapabilityDescriptors |> List.exists (fun capability -> String.Equals(capability.DescriptorAction, actionName action, StringComparison.Ordinal) && suppliedCost >= capability.DescriptorCost && suppliedCost <= maximumTargetedActionCost && capability.RequiredKnowledgeFact |> Option.forall (fun fact -> Set.contains fact knowledge.KnownEnvironmentFacts))
                if not allowed then Error EnvironmentActionFailure.UnsupportedAction else
                nextState feature action |> Result.map (fun state ->
                    let cover =
                        match feature.DirectionalCover, action with
                        | Some value, EnvironmentAction.Damage amount -> Some { value with CoverIntegrity = max 0 (value.CoverIntegrity - amount) }
                        | Some value, EnvironmentAction.Destroy -> Some { value with CoverIntegrity = 0 }
                        | value, _ -> value
                    let changed = state <> feature.EnvironmentState || cover <> feature.DirectionalCover
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
        let volumeTerrain =
            environment.EnvironmentFeatures
            |> List.collect (fun feature ->
                feature.EnvironmentFeatureCells
                |> List.map (fun cell -> toCell cell, if feature.ModalityPermeability.AllowsMovement then SpatialTerrain.Open else SpatialTerrain.Blocked))
            |> Map.ofList
        let minimum = if cells.IsEmpty then { Col = 0; Row = 0 } else { Col = cells |> List.minBy _.Col |> _.Col; Row = cells |> List.minBy _.Row |> _.Row }
        let maximum = if cells.IsEmpty then minimum else { Col = cells |> List.maxBy _.Col |> _.Col; Row = cells |> List.maxBy _.Row |> _.Row }
        let terrain =
            volumeTerrain
            |> Map.fold (fun projected cell value -> Map.add cell value projected) (cells |> List.map (fun cell -> cell, SpatialTerrain.Open) |> Map.ofList)
        { Identity = { MapIdentity = environment.EnvironmentAssemblyIdentity; RulesetIdentity = rulesetIdentity; SpatialRevision = 0L; KnowledgeIdentity = knowledge.EnvironmentKnowledgeIdentity; KnowledgeRevision = knowledge.EnvironmentKnowledgeRevision }
          Minimum = minimum; Maximum = maximum; Terrain = terrain; Boundaries = boundaries; Occupancy = Map.empty; DisclosedRevisionTokens = boundaries |> List.map _.RevisionToken |> Set.ofList }

    let invalidateCache (result: EnvironmentActionResult) (cache: SpatialCache) =
        let inspected = cache.DynamicEntries.Length
        let dependencies = if mutationEnabled "SIR_TACTICAL_MUTATE_DEPENDENCY_LOCALITY" then Set.empty else result.ChangedQueryDependencies
        let next = SpatialQuery.invalidate dependencies cache
        next, inspected, inspected - next.DynamicEntries.Length

    let private capability id action cost = { DescriptorId = id; DescriptorAction = action; DescriptorCost = cost; RequiredKnowledgeFact = None }
    let private feature id kind state cell direction cover capabilities : EnvironmentFeature =
        { EnvironmentFeatureId = id
          EnvironmentKind = kind
          EnvironmentState = state
          EnvironmentEdge = { EdgeCell = cell; EdgeDirection = direction }
          EnvironmentFeatureCells = (if kind = EnvironmentFeatureKind.Cover then [ cell ] else [])
          ModalityPermeability = defaultPermeability kind state
          DirectionalCover = cover
          CapabilityDescriptors = capabilities
          QueryDependencyKeys = [ "feature:" + id ] }
    let private cover material directions = Some { CoverMaterial = material; CoverIntegrity = 100; CoverMaximumIntegrity = 100; CoverPenetrationResistance = 40; CoverProtectedDirections = directions }
    let private cells width height = [ for row in 0 .. height - 1 do for col in 0 .. width - 1 -> { EnvironmentColumn = col; EnvironmentRow = row } ]
    let private singlePlot id role = { PlotSchemaVersion = 1; AuthoredPlotId = id; PlotWidth = 8; PlotHeight = 8; PlotSlots = [ { PlotSlotId = "slot-1"; PlotSlotRole = role; PlotSlotOrigin = { EnvironmentColumn = 0; EnvironmentRow = 0 }; PlotSlotWidth = 8; PlotSlotHeight = 8; ConnectedPlotSlotIds = []; PlotSlotRequiresRoute = true } ] }
    let exteriorParcelSet =
        let plot = singlePlot "exterior-yard" "exterior"
        let variant = { ParcelVariantId = "exterior-cover-yard-a"; ParcelRole = "exterior"; ParcelWidth = 8; ParcelHeight = 8; ParcelWalkableCells = cells 8 8; ParcelObjectiveCells = [ { EnvironmentColumn = 7; EnvironmentRow = 7 } ]; ParcelConnections = []; ParcelFeatures = [ feature "yard-door" EnvironmentFeatureKind.Door EnvironmentFeatureState.Closed { EnvironmentColumn = 3; EnvironmentRow = 3 } EnvironmentEdgeDirection.East None [ capability "open-door" "open" 1; capability "close-door" "close" 1; capability "breach-door" "breach" 2 ]; feature "yard-cover" EnvironmentFeatureKind.Cover EnvironmentFeatureState.Intact { EnvironmentColumn = 5; EnvironmentRow = 4 } EnvironmentEdgeDirection.South (cover "sandbag" [ Direction8.North ]) [ capability "damage-cover" "damage" 1; capability "destroy-cover" "destroy" 1 ] ] }
        plot, [ variant ]
    let interiorBreachParcelSet =
        let plot = singlePlot "interior-breach" "interior"
        let variant = { ParcelVariantId = "interior-breach-a"; ParcelRole = "interior"; ParcelWidth = 8; ParcelHeight = 8; ParcelWalkableCells = cells 8 8; ParcelObjectiveCells = [ { EnvironmentColumn = 6; EnvironmentRow = 6 } ]; ParcelConnections = []; ParcelFeatures = [ feature "interior-wall" EnvironmentFeatureKind.Wall EnvironmentFeatureState.Intact { EnvironmentColumn = 3; EnvironmentRow = 3 } EnvironmentEdgeDirection.East (cover "masonry" [ Direction8.East; Direction8.West ]) [ capability "breach-wall" "breach" 3; capability "damage-wall" "damage" 1; capability "destroy-wall" "destroy" 1 ]; feature "interior-window" EnvironmentFeatureKind.Window EnvironmentFeatureState.Closed { EnvironmentColumn = 2; EnvironmentRow = 2 } EnvironmentEdgeDirection.South None [ capability "open-window" "open" 1; capability "close-window" "close" 1; capability "breach-window" "breach" 2 ] ] }
        plot, [ variant ]

    let workload seed plot variants cache =
        let findings = validate plot variants
        if not findings.IsEmpty then Error findings else
        assemble seed plot variants |> Result.map (fun environment ->
            let known = { EnvironmentKnowledgeIdentity = "workload"; EnvironmentKnowledgeRevision = 0L; KnownEnvironmentFeatureIds = environment.EnvironmentFeatures |> List.map _.EnvironmentFeatureId |> Set.ofList; KnownEnvironmentStateFeatureIds = environment.EnvironmentFeatures |> List.map _.EnvironmentFeatureId |> Set.ofList; KnownEnvironmentFacts = Set.empty }
            let world = toSpatialWorld "tactical-workload-v1" known environment
            let queriedCache, queryCount =
                ((cache, 0), environment.EnvironmentFeatures)
                ||> List.fold (fun (currentCache, count) feature ->
                    let origin = toCell feature.EnvironmentEdge.EdgeCell
                    let target = adjacent feature.EnvironmentEdge |> toCell
                    let request =
                        { QueryId = "workload:" + feature.EnvironmentFeatureId
                          QueryKind = SpatialQueryKind.ExactLineOfSight
                          Origin = origin
                          Target = target
                          Footprint = [ { Col = 0; Row = 0 } ]
                          Profile = { ProfileId = "workload-ground"; Modality = SpatialModality.GroundMovement; Stance = "standing"; HeightBand = 1; Facing = Direction8.North }
                          Bounds = SpatialQuery.defaultBounds }
                    let _, nextCache, _ = SpatialQuery.evaluateCached currentCache world request
                    nextCache, count + 1)
            let inspected, invalidated =
                match environment.EnvironmentFeatures |> List.tryHead with
                | None -> queriedCache.DynamicEntries.Length, 0
                | Some feature ->
                    match applyAction known environment.EnvironmentContentIdentity feature.EnvironmentFeatureId (if feature.CapabilityDescriptors |> List.exists (fun cap -> cap.DescriptorAction = "destroy") then EnvironmentAction.Destroy else EnvironmentAction.Damage 1) environment with
                    | Ok result -> let _, seen, removed = invalidateCache result queriedCache in seen, removed
                    | Error _ -> queriedCache.DynamicEntries.Length, 0
            { WorkloadSlots = environment.AssemblyCostCounters.SlotsVisited; WorkloadVariantsInspected = environment.AssemblyCostCounters.VariantsInspected; WorkloadFindings = findings.Length; WorkloadFeatures = environment.AssemblyCostCounters.PlacedFeatures; DependencyEntriesInspected = inspected; DependencyEntriesInvalidated = invalidated; WorkloadQueryCount = queryCount })
