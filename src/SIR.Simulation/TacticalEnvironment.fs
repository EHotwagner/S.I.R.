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
    let private canonicalSortedBy key values =
        let rec ordered previous remaining =
            match remaining with
            | [] -> true
            | head :: tail when compare previous (key head) <= 0 -> ordered (key head) tail
            | _ -> false
        match values with
        | [] | [ _ ] -> values
        | head :: tail when ordered (key head) tail -> values
        | _ -> values |> List.sortBy key
    let private canonicalSortedDistinct values =
        let rec ordered previous remaining =
            match remaining with
            | [] -> true
            | head :: tail when compare previous head < 0 -> ordered head tail
            | _ -> false
        match values with
        | [] | [ _ ] -> values
        | head :: tail when ordered head tail -> values
        | _ ->
            // List.distinct followed by List.sort builds a Set-shaped allocation graph and then a
            // second list. Dense editor cells commonly arrive row-major while their canonical record
            // order is column-major, so that fallback dominated hosted preview allocation. Sort one
            // compact array in place and rebuild the required distinct list once.
            let sorted = List.toArray values
            Array.sortInPlace sorted
            let mutable distinct = []
            for index in sorted.Length - 1 .. -1 .. 0 do
                if index = sorted.Length - 1 || compare sorted[index] sorted[index + 1] <> 0 then
                    distinct <- sorted[index] :: distinct
            distinct
    let private compareCells (left: EnvironmentCell) (right: EnvironmentCell) =
        let column = compare left.EnvironmentColumn right.EnvironmentColumn
        if column <> 0 then column else compare left.EnvironmentRow right.EnvironmentRow
    let private canonicalSortedDistinctCells values =
        let rec ordered previous remaining =
            match remaining with
            | [] -> true
            | head :: tail when compareCells previous head < 0 -> ordered head tail
            | _ -> false
        match values with
        | [] | [ _ ] -> values
        | head :: tail when ordered head tail -> values
        | _ ->
            let mutable minimumColumn = Int32.MaxValue
            let mutable maximumColumn = Int32.MinValue
            let mutable minimumRow = Int32.MaxValue
            let mutable maximumRow = Int32.MinValue
            for cell in values do
                minimumColumn <- min minimumColumn cell.EnvironmentColumn
                maximumColumn <- max maximumColumn cell.EnvironmentColumn
                minimumRow <- min minimumRow cell.EnvironmentRow
                maximumRow <- max maximumRow cell.EnvironmentRow
            let width = int64 maximumColumn - int64 minimumColumn + 1L
            let height = int64 maximumRow - int64 minimumRow + 1L
            let area = width * height
            if area = int64 values.Length && area <= 1_048_576L then
                // A complete dense rectangle needs no comparison sort. Mark it once to reject a
                // duplicate-plus-gap impostor, then emit the exact column/row record order directly.
                let present = Array.zeroCreate<bool> (int area)
                let mutable dense = true
                for cell in values do
                    let offset =
                        (cell.EnvironmentRow - minimumRow) * int width
                        + cell.EnvironmentColumn - minimumColumn
                    if present[offset] then dense <- false else present[offset] <- true
                if dense then
                    [ for column in minimumColumn .. maximumColumn do
                        for row in minimumRow .. maximumRow ->
                            { EnvironmentColumn = column; EnvironmentRow = row } ]
                else
                    let sorted = List.toArray values
                    Array.sortInPlaceWith compareCells sorted
                    let mutable distinct = []
                    for index in sorted.Length - 1 .. -1 .. 0 do
                        if index = sorted.Length - 1 || compareCells sorted[index] sorted[index + 1] <> 0 then
                            distinct <- sorted[index] :: distinct
                    distinct
            else
                let sorted = List.toArray values
                Array.sortInPlaceWith compareCells sorted
                let mutable distinct = []
                for index in sorted.Length - 1 .. -1 .. 0 do
                    if index = sorted.Length - 1 || compareCells sorted[index] sorted[index + 1] <> 0 then
                        distinct <- sorted[index] :: distinct
                distinct
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
        let inVariant = within variant.ParcelWidth variant.ParcelHeight
        let canUseDenseGrid =
            variant.ParcelWidth > 0
            && variant.ParcelHeight > 0
            && int64 variant.ParcelWidth * int64 variant.ParcelHeight <= 1_048_576L
            && variant.ParcelWalkableCells |> List.forall inVariant

        if canUseDenseGrid then
            // Authored parcels are bounded dense grids. Use their declared footprint directly for the
            // common valid case instead of hashing records and allocating four neighbour records per
            // visited cell. Invalid/out-of-bounds input retains the general fallback below so validation
            // findings and malformed-input semantics remain unchanged.
            let width = variant.ParcelWidth
            let index (cell: EnvironmentCell) = cell.EnvironmentRow * width + cell.EnvironmentColumn
            let walkable = Array.zeroCreate<bool> (width * variant.ParcelHeight)
            variant.ParcelWalkableCells |> List.iter (index >> fun cellIndex -> walkable[cellIndex] <- true)
            let isWalkable cell = inVariant cell && walkable[index cell]
            let starts =
                variant.ParcelConnections
                |> List.map _.ConnectionCell
                |> List.filter isWalkable
                |> function [] -> variant.ParcelWalkableCells |> List.sort |> List.truncate 1 | values -> values
            let reached = Array.zeroCreate<bool> walkable.Length
            let pending = Collections.Generic.Queue<int32>()
            starts |> List.iter (index >> pending.Enqueue)
            while pending.Count > 0 do
                let cellIndex = pending.Dequeue()
                if not reached[cellIndex] then
                    reached[cellIndex] <- true
                    let column = cellIndex % width
                    let row = cellIndex / width
                    let enqueue candidate =
                        if walkable[candidate] && not reached[candidate] then pending.Enqueue candidate
                    if column > 0 then enqueue (cellIndex - 1)
                    if column + 1 < width then enqueue (cellIndex + 1)
                    if row > 0 then enqueue (cellIndex - width)
                    if row + 1 < variant.ParcelHeight then enqueue (cellIndex + width)
            not starts.IsEmpty
            && variant.ParcelObjectiveCells |> List.forall (fun cell -> inVariant cell && reached[index cell])
        else
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
        { feature with EnvironmentFeatureId = placedId; EnvironmentEdge = edgeBetween left right; EnvironmentFeatureCells = feature.EnvironmentFeatureCells |> List.map (transformCell transform width height >> translate origin) |> canonicalSortedDistinctCells; QueryDependencyKeys = feature.QueryDependencyKeys |> List.map (fun key -> key + ":" + placedId) |> canonicalSortedDistinct }

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
        // This identity is computed on every editor assembly, and the maximum preview writes thousands
        // of cells, features, and dependency strings. A generic List<byte> made every scalar a method
        // call and copied the full payload again through ToArray; that allocation/JIT shape was unstable
        // on hosted runners. Keep the schema-v1 grammar exact while streaming into one Fable-compatible
        // exactly sized byte array with no generic per-byte collection path or final full-payload copy.
        // Count UTF-8 bytes from UTF-16 code units without Encoding.GetByteCount, which Fable does not
        // support. Isolated surrogates match UTF-8's three-byte replacement scalar; valid pairs use four.
        let utf8ByteCount (value: string) =
            let mutable count = 0
            let mutable index = 0
            while index < value.Length do
                let code = int value[index]
                if code <= 0x7F then
                    count <- count + 1
                    index <- index + 1
                elif code <= 0x7FF then
                    count <- count + 2
                    index <- index + 1
                elif code >= 0xD800 && code <= 0xDBFF && index + 1 < value.Length then
                    let following = int value[index + 1]
                    if following >= 0xDC00 && following <= 0xDFFF then
                        count <- count + 4
                        index <- index + 2
                    else
                        count <- count + 3
                        index <- index + 1
                else
                    count <- count + 3
                    index <- index + 1
            count
        let textSize (value: string) = 4 + utf8ByteCount value
        let listSize size values = 4 + (values |> List.sumBy size)
        let cellSize (_: EnvironmentCell) = 8
        let coverSize = function
            | None -> 1
            | Some cover -> 1 + textSize cover.CoverMaterial + 12 + listSize (fun _ -> 4) cover.CoverProtectedDirections
        let capabilitySize capability =
            textSize capability.DescriptorId
            + textSize capability.DescriptorAction
            + 4
            + 1
            + (capability.RequiredKnowledgeFact |> Option.map textSize |> Option.defaultValue 0)
        let featureSize feature =
            textSize feature.EnvironmentFeatureId
            + 4
            + 4
            + 8
            + 4
            + listSize cellSize feature.EnvironmentFeatureCells
            + 7
            + coverSize feature.DirectionalCover
            + listSize capabilitySize feature.CapabilityDescriptors
            + listSize textSize feature.QueryDependencyKeys
        let connectionSize connection =
            textSize connection.ConnectionId + 8 + 4 + textSize connection.ConnectionRole
        let slotSize slot =
            textSize slot.PlotSlotId
            + textSize slot.PlotSlotRole
            + 8
            + 4
            + 4
            + listSize textSize slot.ConnectedPlotSlotIds
            + 1
        let variantSize variant =
            textSize variant.ParcelVariantId
            + textSize variant.ParcelRole
            + 4
            + 4
            + listSize cellSize variant.ParcelWalkableCells
            + listSize cellSize variant.ParcelObjectiveCells
            + listSize connectionSize variant.ParcelConnections
            + listSize featureSize variant.ParcelFeatures
        let exactByteCount =
            textSize "SIR-TACTICAL-AUTHORED-CATALOG"
            + 4
            + textSize plot.AuthoredPlotId
            + 4
            + 4
            + listSize slotSize plot.PlotSlots
            + listSize variantSize variants
        let bytes = Array.zeroCreate<byte> exactByteCount
        let mutable byteCount = 0
        let appendByte value =
            bytes[byteCount] <- value
            byteCount <- byteCount + 1
        let appendBytes (values: byte array) =
            Array.blit values 0 bytes byteCount values.Length
            byteCount <- byteCount + values.Length
        let appendInt32 (value: int32) =
            appendByte (byte value)
            appendByte (byte (value >>> 8))
            appendByte (byte (value >>> 16))
            appendByte (byte (value >>> 24))
        let appendBool value = appendByte (if value then 1uy else 0uy)
        let appendText (value: string) =
            let encoded = Encoding.UTF8.GetBytes value
            appendInt32 encoded.Length
            appendBytes encoded
        let appendCell (cell: EnvironmentCell) =
            appendInt32 cell.EnvironmentColumn
            appendInt32 cell.EnvironmentRow
        let directionCode = function EnvironmentEdgeDirection.East -> 0 | EnvironmentEdgeDirection.South -> 1
        let kindCode = function EnvironmentFeatureKind.Door -> 0 | EnvironmentFeatureKind.Window -> 1 | EnvironmentFeatureKind.Wall -> 2 | EnvironmentFeatureKind.Cover -> 3
        let stateCode = function EnvironmentFeatureState.Intact -> 0 | EnvironmentFeatureState.Closed -> 1 | EnvironmentFeatureState.Open -> 2 | EnvironmentFeatureState.Damaged -> 3 | EnvironmentFeatureState.Breached -> 4 | EnvironmentFeatureState.Destroyed -> 5
        let transformCode = function ParcelTransform.Identity -> 0 | ParcelTransform.Rotate90 -> 1 | ParcelTransform.Rotate180 -> 2 | ParcelTransform.Rotate270 -> 3
        let appendList append (values: 'a list) =
            appendInt32 values.Length
            values |> List.iter append
        let appendPermeability value =
            appendBool value.AllowsMovement
            appendBool value.AllowsSight
            appendBool value.AllowsProjectile
            appendBool value.AllowsAreaEffect
            appendBool value.AllowsSound
            appendBool value.ProvidesCover
            appendBool value.AllowsInteraction
        let appendCover = function
            | None -> appendByte 0uy
            | Some cover ->
                appendByte 1uy
                appendText cover.CoverMaterial
                appendInt32 cover.CoverIntegrity
                appendInt32 cover.CoverMaximumIntegrity
                appendInt32 cover.CoverPenetrationResistance
                cover.CoverProtectedDirections
                |> List.map (Direction8.toCode >> int32)
                |> canonicalSortedDistinct
                |> appendList appendInt32
        let appendCapability capability =
            appendText capability.DescriptorId
            appendText capability.DescriptorAction
            appendInt32 capability.DescriptorCost
            match capability.RequiredKnowledgeFact with
            | None -> appendByte 0uy
            | Some fact -> appendByte 1uy; appendText fact
        let appendFeature feature =
            appendText feature.EnvironmentFeatureId
            appendInt32 (kindCode feature.EnvironmentKind)
            appendInt32 (stateCode feature.EnvironmentState)
            appendCell feature.EnvironmentEdge.EdgeCell
            appendInt32 (directionCode feature.EnvironmentEdge.EdgeDirection)
            feature.EnvironmentFeatureCells |> canonicalSortedDistinctCells |> appendList appendCell
            appendPermeability feature.ModalityPermeability
            appendCover feature.DirectionalCover
            feature.CapabilityDescriptors |> canonicalSortedBy _.DescriptorId |> appendList appendCapability
            feature.QueryDependencyKeys |> canonicalSortedDistinct |> appendList appendText

        appendText "SIR-TACTICAL-AUTHORED-CATALOG"
        appendInt32 plot.PlotSchemaVersion
        appendText plot.AuthoredPlotId
        appendInt32 plot.PlotWidth
        appendInt32 plot.PlotHeight
        plot.PlotSlots
        |> canonicalSortedBy _.PlotSlotId
        |> appendList (fun slot ->
            appendText slot.PlotSlotId
            appendText slot.PlotSlotRole
            appendCell slot.PlotSlotOrigin
            appendInt32 slot.PlotSlotWidth
            appendInt32 slot.PlotSlotHeight
            slot.ConnectedPlotSlotIds |> canonicalSortedDistinct |> appendList appendText
            appendBool slot.PlotSlotRequiresRoute)
        variants
        |> canonicalSortedBy _.ParcelVariantId
        |> appendList (fun variant ->
            appendText variant.ParcelVariantId
            appendText variant.ParcelRole
            appendInt32 variant.ParcelWidth
            appendInt32 variant.ParcelHeight
            variant.ParcelWalkableCells |> canonicalSortedDistinctCells |> appendList appendCell
            variant.ParcelObjectiveCells |> canonicalSortedDistinctCells |> appendList appendCell
            variant.ParcelConnections
            |> canonicalSortedBy _.ConnectionId
            |> appendList (fun connection ->
                appendText connection.ConnectionId
                appendCell connection.ConnectionCell
                appendInt32 (directionCode connection.ConnectionDirection)
                appendText connection.ConnectionRole)
            variant.ParcelFeatures |> canonicalSortedBy _.EnvironmentFeatureId |> appendList appendFeature)
        if byteCount <> exactByteCount then invalidOp "Authored tactical input byte count diverged from its schema-v1 grammar."
        bytes |> sha256Hex

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
            let cells = choices |> List.collect (fun (slot, variant, transform) -> variant.ParcelWalkableCells |> List.map (transformCell transform variant.ParcelWidth variant.ParcelHeight >> translate slot.PlotSlotOrigin)) |> canonicalSortedDistinctCells
            let objectives = choices |> List.collect (fun (slot, variant, transform) -> variant.ParcelObjectiveCells |> List.map (transformCell transform variant.ParcelWidth variant.ParcelHeight >> translate slot.PlotSlotOrigin)) |> canonicalSortedDistinctCells
            let features = choices |> List.collect (fun (slot, variant, transform) -> variant.ParcelFeatures |> List.map (transformFeature slot.PlotSlotId transform variant.ParcelWidth variant.ParcelHeight slot.PlotSlotOrigin)) |> canonicalSortedBy _.EnvironmentFeatureId
            let assemblyIdentity = authoredInputIdentity plot variants
            let environment =
                { EnvironmentSchemaVersion = SIR.Domain.TacticalEnvironment.schemaVersion; AssembledPlotId = plot.AuthoredPlotId; AssemblySeed = seed; ParcelPlacements = placements; AssembledWalkableCells = cells; AssembledObjectiveCells = objectives; EnvironmentFeatures = features; EnvironmentAssemblyIdentity = assemblyIdentity; EnvironmentContentIdentity = ""; EnvironmentSpatialRevision = 0L; AssemblyCostCounters = { SlotsVisited = plot.PlotSlots.Length; VariantsInspected = inspected; Selections = choices.Length; PlacedCells = cells.Length; PlacedFeatures = features.Length } }
                |> SIR.Domain.TacticalEnvironment.withContentIdentity
            Ok environment

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
