namespace SIR.Client

open System
open SIR.Domain

[<RequireQualifiedAccess>]
module TacticalParcelEditor =
    type TacticalParcelEditorState =
        { TacticalDocument: TacticalParcelDocument
          TacticalSeed: uint64
          TacticalPreview: Result<AssembledEnvironment, EnvironmentValidationFinding list>
          TacticalUndo: TacticalParcelDocument list
          TacticalRedo: TacticalParcelDocument list
          TacticalAnnouncement: string }

    type TacticalParcelEditorAction =
        | ReplaceTacticalDocument of TacticalParcelDocument
        | SetTacticalSeed of uint64
        | SetTacticalFeatureState of featureId: string * EnvironmentFeatureState
        | SetTacticalPermeability of featureId: string * EnvironmentModality * enabled: bool
        | SetTacticalCoverIntegrity of featureId: string * integrity: int32
        | RunTacticalEnvironmentAction of featureId: string * EnvironmentAction
        | UndoTacticalParcelEdit
        | RedoTacticalParcelEdit
        | RefreshTacticalPreview

    [<Literal>]
    let MaximumTacticalHistoryCommands = 100

    let private legacyEdgeName = function
        | EastEdge -> "east"
        | SouthEdge -> "south"

    let private tacticalToken (value: string) = Uri.EscapeDataString value
    let private tacticalUntoken (value: string) = Uri.UnescapeDataString value
    let private tacticalBool value = if value then "1" else "0"
    let private tacticalStateName = function
        | EnvironmentFeatureState.Intact -> "intact"
        | EnvironmentFeatureState.Closed -> "closed"
        | EnvironmentFeatureState.Open -> "open"
        | EnvironmentFeatureState.Damaged -> "damaged"
        | EnvironmentFeatureState.Breached -> "breached"
        | EnvironmentFeatureState.Destroyed -> "destroyed"
    let private tacticalKindName = function
        | EnvironmentFeatureKind.Door -> "door"
        | EnvironmentFeatureKind.Window -> "window"
        | EnvironmentFeatureKind.Wall -> "wall"
        | EnvironmentFeatureKind.Cover -> "cover"
    let private tacticalEdgeName = function
        | EnvironmentEdgeDirection.East -> "east"
        | EnvironmentEdgeDirection.South -> "south"
    let private tacticalDirectionName = function
        | North -> "n" | NorthEast -> "ne" | East -> "e" | SouthEast -> "se"
        | South -> "s" | SouthWest -> "sw" | West -> "w" | NorthWest -> "nw"

    /// Exports the authored plot and parcel catalog, rather than an assembled
    /// result, so a round trip preserves every editable choice.
    let exportTacticalParcelDocument document =
        let plot = document.TacticalPlot
        let lines = ResizeArray<string>()
        lines.Add("SIR-TACTICAL-ENVIRONMENT|1")
        lines.Add(String.concat "|" [ "plot"; string plot.PlotSchemaVersion; tacticalToken plot.AuthoredPlotId; string plot.PlotWidth; string plot.PlotHeight ])
        for slot in plot.PlotSlots |> List.sortBy _.PlotSlotId do
            lines.Add(String.concat "|" [ "slot"; tacticalToken slot.PlotSlotId; tacticalToken slot.PlotSlotRole; string slot.PlotSlotOrigin.EnvironmentColumn; string slot.PlotSlotOrigin.EnvironmentRow; string slot.PlotSlotWidth; string slot.PlotSlotHeight; tacticalBool slot.PlotSlotRequiresRoute; slot.ConnectedPlotSlotIds |> List.sort |> List.map tacticalToken |> String.concat "," ])
        for variant in document.TacticalVariants |> List.sortBy _.ParcelVariantId do
            lines.Add(String.concat "|" [ "variant"; tacticalToken variant.ParcelVariantId; tacticalToken variant.ParcelRole; string variant.ParcelWidth; string variant.ParcelHeight ])
            for cell in variant.ParcelWalkableCells |> List.sort do lines.Add(String.concat "|" [ "walk"; tacticalToken variant.ParcelVariantId; string cell.EnvironmentColumn; string cell.EnvironmentRow ])
            for cell in variant.ParcelObjectiveCells |> List.sort do lines.Add(String.concat "|" [ "objective"; tacticalToken variant.ParcelVariantId; string cell.EnvironmentColumn; string cell.EnvironmentRow ])
            for connection in variant.ParcelConnections |> List.sortBy _.ConnectionId do
                lines.Add(String.concat "|" [ "connection"; tacticalToken variant.ParcelVariantId; tacticalToken connection.ConnectionId; string connection.ConnectionCell.EnvironmentColumn; string connection.ConnectionCell.EnvironmentRow; tacticalEdgeName connection.ConnectionDirection; tacticalToken connection.ConnectionRole ])
            for feature in variant.ParcelFeatures |> List.sortBy _.EnvironmentFeatureId do
                let coverMaterial, integrity, maximum, penetration, directions =
                    match feature.DirectionalCover with
                    | None -> "", "0", "0", "0", ""
                    | Some cover ->
                        tacticalToken cover.CoverMaterial, string cover.CoverIntegrity, string cover.CoverMaximumIntegrity,
                        string cover.CoverPenetrationResistance,
                        (cover.CoverProtectedDirections |> List.map tacticalDirectionName |> String.concat ",")
                let permeability = feature.ModalityPermeability
                lines.Add(String.concat "|" [ "feature"; tacticalToken variant.ParcelVariantId; tacticalToken feature.EnvironmentFeatureId; tacticalKindName feature.EnvironmentKind; tacticalStateName feature.EnvironmentState; string feature.EnvironmentEdge.EdgeCell.EnvironmentColumn; string feature.EnvironmentEdge.EdgeCell.EnvironmentRow; tacticalEdgeName feature.EnvironmentEdge.EdgeDirection; tacticalBool permeability.AllowsMovement; tacticalBool permeability.AllowsSight; tacticalBool permeability.AllowsProjectile; tacticalBool permeability.AllowsAreaEffect; tacticalBool permeability.AllowsSound; tacticalBool permeability.ProvidesCover; tacticalBool permeability.AllowsInteraction; coverMaterial; integrity; maximum; penetration; directions; feature.QueryDependencyKeys |> List.sort |> List.map tacticalToken |> String.concat "," ])
                lines.Add(String.concat "|" [ "feature-cells"; tacticalToken variant.ParcelVariantId; tacticalToken feature.EnvironmentFeatureId ])
                for cell in feature.EnvironmentFeatureCells |> List.distinct |> List.sort do
                    lines.Add(String.concat "|" [ "feature-cell"; tacticalToken variant.ParcelVariantId; tacticalToken feature.EnvironmentFeatureId; string cell.EnvironmentColumn; string cell.EnvironmentRow ])
                for capability in feature.CapabilityDescriptors |> List.sortBy _.DescriptorId do
                    lines.Add(String.concat "|" [ "capability"; tacticalToken variant.ParcelVariantId; tacticalToken feature.EnvironmentFeatureId; tacticalToken capability.DescriptorId; tacticalToken capability.DescriptorAction; string capability.DescriptorCost; capability.RequiredKnowledgeFact |> Option.map tacticalToken |> Option.defaultValue "" ])
        String.concat "\n" lines + "\n"

    let private parseTacticalState = function
        | "intact" -> Some EnvironmentFeatureState.Intact | "closed" -> Some EnvironmentFeatureState.Closed
        | "open" -> Some EnvironmentFeatureState.Open | "damaged" -> Some EnvironmentFeatureState.Damaged
        | "breached" -> Some EnvironmentFeatureState.Breached | "destroyed" -> Some EnvironmentFeatureState.Destroyed | _ -> None
    let private parseTacticalKind = function
        | "door" -> Some EnvironmentFeatureKind.Door | "window" -> Some EnvironmentFeatureKind.Window
        | "wall" -> Some EnvironmentFeatureKind.Wall | "cover" -> Some EnvironmentFeatureKind.Cover | _ -> None
    let private parseTacticalEdge = function "east" -> Some EnvironmentEdgeDirection.East | "south" -> Some EnvironmentEdgeDirection.South | _ -> None
    let private parseTacticalDirection = function
        | "n" -> Some North | "ne" -> Some NorthEast | "e" -> Some East | "se" -> Some SouthEast
        | "s" -> Some South | "sw" -> Some SouthWest | "w" -> Some West | "nw" -> Some NorthWest | _ -> None
    let private splitTacticalList (value: string) =
        if String.IsNullOrWhiteSpace value then [] else value.Split(',') |> Array.toList |> List.map tacticalUntoken

    /// Imports the canonical schema-v1 authoring envelope. Future schema
    /// versions fail closed and never reinterpret their records as v1.
    let tryImportTacticalParcelDocument (text: string) =
        try
            let rows = text.Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries) |> Array.map (fun line -> line.Split('|'))
            if rows.Length = 0 || rows[0] <> [| "SIR-TACTICAL-ENVIRONMENT"; "1" |] then Error "Unsupported tactical environment envelope."
            else
                let plotRow = rows |> Array.tryFind (fun row -> row.Length = 5 && row[0] = "plot")
                match plotRow with
                | None -> Error "The tactical environment plot record is missing."
                | Some row ->
                    let slots =
                        [ for fields in rows do
                            if fields.Length = 9 && fields[0] = "slot" then
                                yield { PlotSlotId = tacticalUntoken fields[1]; PlotSlotRole = tacticalUntoken fields[2]; PlotSlotOrigin = { EnvironmentColumn = Int32.Parse fields[3]; EnvironmentRow = Int32.Parse fields[4] }; PlotSlotWidth = Int32.Parse fields[5]; PlotSlotHeight = Int32.Parse fields[6]; PlotSlotRequiresRoute = fields[7] = "1"; ConnectedPlotSlotIds = splitTacticalList fields[8] } ]
                    let variantRows = rows |> Array.filter (fun fields -> fields.Length = 5 && fields[0] = "variant")
                    let variants =
                        [ for variantFields in variantRows do
                            let variantId = tacticalUntoken variantFields[1]
                            let cells kind : EnvironmentCell list = [ for fields in rows do if fields.Length = 4 && fields[0] = kind && tacticalUntoken fields[1] = variantId then yield { EnvironmentColumn = Int32.Parse fields[2]; EnvironmentRow = Int32.Parse fields[3] } ]
                            let connections =
                                [ for fields in rows do
                                    if fields.Length = 7 && fields[0] = "connection" && tacticalUntoken fields[1] = variantId then
                                        match parseTacticalEdge fields[5] with
                                        | Some direction -> yield { ConnectionId = tacticalUntoken fields[2]; ConnectionCell = { EnvironmentColumn = Int32.Parse fields[3]; EnvironmentRow = Int32.Parse fields[4] }; ConnectionDirection = direction; ConnectionRole = tacticalUntoken fields[6] }
                                        | None -> failwith "Unknown connection direction." ]
                            let features =
                                [ for fields in rows do
                                    if fields.Length = 21 && fields[0] = "feature" && tacticalUntoken fields[1] = variantId then
                                        let featureId = tacticalUntoken fields[2]
                                        let kind = parseTacticalKind fields[3] |> Option.defaultWith (fun () -> failwith "Unknown feature kind.")
                                        let state = parseTacticalState fields[4] |> Option.defaultWith (fun () -> failwith "Unknown feature state.")
                                        let direction = parseTacticalEdge fields[7] |> Option.defaultWith (fun () -> failwith "Unknown feature direction.")
                                        let directions = splitTacticalList fields[19] |> List.map (fun value -> parseTacticalDirection value |> Option.defaultWith (fun () -> failwith "Unknown cover direction."))
                                        let cover = if fields[15] = "" then None else Some { CoverMaterial = tacticalUntoken fields[15]; CoverIntegrity = Int32.Parse fields[16]; CoverMaximumIntegrity = Int32.Parse fields[17]; CoverPenetrationResistance = Int32.Parse fields[18]; CoverProtectedDirections = directions }
                                        let capabilities =
                                            [ for capability in rows do
                                                if capability.Length = 7 && capability[0] = "capability" && tacticalUntoken capability[1] = variantId && tacticalUntoken capability[2] = featureId then
                                                    yield { DescriptorId = tacticalUntoken capability[3]; DescriptorAction = tacticalUntoken capability[4]; DescriptorCost = Int32.Parse capability[5]; RequiredKnowledgeFact = if capability[6] = "" then None else Some(tacticalUntoken capability[6]) } ]
                                        let edgeCell = { EnvironmentColumn = Int32.Parse fields[5]; EnvironmentRow = Int32.Parse fields[6] }
                                        let featureCells =
                                            [ for cell in rows do
                                                if cell.Length = 5 && cell[0] = "feature-cell" && tacticalUntoken cell[1] = variantId && tacticalUntoken cell[2] = featureId then
                                                    yield { EnvironmentColumn = Int32.Parse cell[3]; EnvironmentRow = Int32.Parse cell[4] } ]
                                        let hasFeatureCells = rows |> Array.exists (fun cell -> cell.Length = 3 && cell[0] = "feature-cells" && tacticalUntoken cell[1] = variantId && tacticalUntoken cell[2] = featureId)
                                        yield { EnvironmentFeatureId = featureId; EnvironmentKind = kind; EnvironmentState = state; EnvironmentEdge = { EdgeCell = edgeCell; EdgeDirection = direction }; EnvironmentFeatureCells = (if hasFeatureCells then featureCells else [ edgeCell ]); ModalityPermeability = { AllowsMovement = fields[8] = "1"; AllowsSight = fields[9] = "1"; AllowsProjectile = fields[10] = "1"; AllowsAreaEffect = fields[11] = "1"; AllowsSound = fields[12] = "1"; ProvidesCover = fields[13] = "1"; AllowsInteraction = fields[14] = "1" }; DirectionalCover = cover; CapabilityDescriptors = capabilities; QueryDependencyKeys = splitTacticalList fields[20] } ]
                            yield { ParcelVariantId = variantId; ParcelRole = tacticalUntoken variantFields[2]; ParcelWidth = Int32.Parse variantFields[3]; ParcelHeight = Int32.Parse variantFields[4]; ParcelWalkableCells = cells "walk"; ParcelObjectiveCells = cells "objective"; ParcelConnections = connections; ParcelFeatures = features } ]
                    let plot = { PlotSchemaVersion = Int32.Parse row[1]; AuthoredPlotId = tacticalUntoken row[2]; PlotWidth = Int32.Parse row[3]; PlotHeight = Int32.Parse row[4]; PlotSlots = slots }
                    Ok { TacticalPlot = plot; TacticalVariants = variants }
        with ex -> Error("Invalid tactical environment document: " + ex.Message)

    /// Explicitly migrates map-format-v4 semantic edges to schema-v1
    /// environment features without changing the legacy map document.
    let migrateLegacyTacticalEnvironment (map: MapDefinition) =
        let capability id action = { DescriptorId = id; DescriptorAction = action; DescriptorCost = 1; RequiredKnowledgeFact = None }
        let features =
            [ for KeyValue((column, row, direction), (kind, isOpen)) in map.Edges do
                let featureId = "legacy-edge-" + string column + "-" + string row + "-" + legacyEdgeName direction
                let environmentKind, state, capabilities =
                    match kind with
                    | MapEdgeKind.Wall -> EnvironmentFeatureKind.Wall, EnvironmentFeatureState.Intact, [ capability (featureId + ":breach") "breach"; capability (featureId + ":destroy") "destroy" ]
                    | MapEdgeKind.Door -> EnvironmentFeatureKind.Door, (if isOpen then EnvironmentFeatureState.Open else EnvironmentFeatureState.Closed), [ capability (featureId + ":open") "open"; capability (featureId + ":close") "close"; capability (featureId + ":breach") "breach" ]
                    | MapEdgeKind.Window -> EnvironmentFeatureKind.Window, EnvironmentFeatureState.Closed, [ capability (featureId + ":open") "open"; capability (featureId + ":breach") "breach" ]
                let edgeDirection = match direction with EastEdge -> EnvironmentEdgeDirection.East | SouthEdge -> EnvironmentEdgeDirection.South
                let protectedDirections = match direction with EastEdge -> [ East; West ] | SouthEdge -> [ North; South ]
                let cover =
                    match environmentKind with
                    | EnvironmentFeatureKind.Wall -> Some { CoverMaterial = "legacy-masonry"; CoverIntegrity = 100; CoverMaximumIntegrity = 100; CoverPenetrationResistance = 40; CoverProtectedDirections = protectedDirections }
                    | _ -> None
                let edgeCell = { EnvironmentColumn = column; EnvironmentRow = row }
                yield { EnvironmentFeatureId = featureId; EnvironmentKind = environmentKind; EnvironmentState = state; EnvironmentEdge = { EdgeCell = edgeCell; EdgeDirection = edgeDirection }; EnvironmentFeatureCells = [ edgeCell ]; ModalityPermeability = SIR.Simulation.TacticalEnvironment.defaultPermeability environmentKind state; DirectionalCover = cover; CapabilityDescriptors = capabilities; QueryDependencyKeys = [ "feature:" + featureId ] } ]
        let walkable : EnvironmentCell list = [ for row in 0 .. map.Height - 1 do for column in 0 .. map.Width - 1 do if Map.tryFind (column, row) map.Terrain <> Some Blocked then yield { EnvironmentColumn = column; EnvironmentRow = row } ]
        let objectives : EnvironmentCell list = map.Terrain |> Map.toList |> List.choose (fun ((column, row), terrain) -> if terrain = Objective then Some { EnvironmentColumn = column; EnvironmentRow = row } else None)
        { TacticalPlot = { PlotSchemaVersion = SIR.Domain.TacticalEnvironment.schemaVersion; AuthoredPlotId = "migrated-map-v4"; PlotWidth = map.Width; PlotHeight = map.Height; PlotSlots = [ { PlotSlotId = "map"; PlotSlotRole = "legacy-map"; PlotSlotOrigin = { EnvironmentColumn = 0; EnvironmentRow = 0 }; PlotSlotWidth = map.Width; PlotSlotHeight = map.Height; ConnectedPlotSlotIds = []; PlotSlotRequiresRoute = true } ] }
          TacticalVariants = [ { ParcelVariantId = "migrated-map-v4"; ParcelRole = "legacy-map"; ParcelWidth = map.Width; ParcelHeight = map.Height; ParcelWalkableCells = walkable; ParcelObjectiveCells = objectives; ParcelConnections = []; ParcelFeatures = features } ] }

    let validateTacticalParcelDocument document =
        SIR.Simulation.TacticalEnvironment.validate document.TacticalPlot document.TacticalVariants

    let previewTacticalParcelDocument seed document =
        SIR.Simulation.TacticalEnvironment.assemble seed document.TacticalPlot document.TacticalVariants

    let private replaceTacticalFeature featureId transform document =
        { document with TacticalVariants = document.TacticalVariants |> List.map (fun variant -> { variant with ParcelFeatures = variant.ParcelFeatures |> List.map (fun feature -> if feature.EnvironmentFeatureId = featureId then transform feature else feature) }) }

    let private refreshTacticalState announcement state =
        { state with TacticalPreview = previewTacticalParcelDocument state.TacticalSeed state.TacticalDocument; TacticalAnnouncement = announcement }

    let createTacticalParcelEditor seed document =
        { TacticalDocument = document; TacticalSeed = seed; TacticalPreview = previewTacticalParcelDocument seed document; TacticalUndo = []; TacticalRedo = []; TacticalAnnouncement = "Tactical parcel preview ready." }

    let fromCanonicalEditor announcement (editor: MapEditorState) =
        { createTacticalParcelEditor editor.TacticalSeed editor.TacticalDocument with
            TacticalUndo = if editor.UndoHistory.IsEmpty then [] else [ editor.TacticalDocument ]
            TacticalRedo = if editor.RedoHistory.IsEmpty then [] else [ editor.TacticalDocument ]
            TacticalAnnouncement = announcement }

    let exteriorInitial seed =
        let plot, variants = SIR.Simulation.TacticalEnvironment.exteriorParcelSet
        let state = createTacticalParcelEditor seed { TacticalPlot = plot; TacticalVariants = variants }
        state, exportTacticalParcelDocument state.TacticalDocument

    let updateTacticalParcelEditor action state =
        let commit announcement document =
            { state with TacticalDocument = document; TacticalUndo = (state.TacticalDocument :: state.TacticalUndo) |> List.truncate MaximumTacticalHistoryCommands; TacticalRedo = [] }
            |> refreshTacticalState announcement
        let updatePermeability modality enabled value =
            match modality with
            | EnvironmentModality.Movement -> { value with AllowsMovement = enabled }
            | EnvironmentModality.Sight -> { value with AllowsSight = enabled }
            | EnvironmentModality.Projectile -> { value with AllowsProjectile = enabled }
            | EnvironmentModality.AreaEffect -> { value with AllowsAreaEffect = enabled }
            | EnvironmentModality.Sound -> { value with AllowsSound = enabled }
            | EnvironmentModality.Cover -> { value with ProvidesCover = enabled }
            | EnvironmentModality.Interaction _ -> { value with AllowsInteraction = enabled }
        match action with
        | ReplaceTacticalDocument document -> commit "Tactical parcel document loaded." document
        | SetTacticalSeed seed -> { state with TacticalSeed = seed } |> refreshTacticalState ("Preview seed set to " + string seed + ".")
        | SetTacticalFeatureState(featureId, featureState) ->
            state.TacticalDocument |> replaceTacticalFeature featureId (fun feature -> { feature with EnvironmentState = featureState; ModalityPermeability = SIR.Simulation.TacticalEnvironment.defaultPermeability feature.EnvironmentKind featureState }) |> commit ("Feature " + featureId + " changed to " + tacticalStateName featureState + ".")
        | SetTacticalPermeability(featureId, modality, enabled) ->
            state.TacticalDocument |> replaceTacticalFeature featureId (fun feature -> { feature with ModalityPermeability = updatePermeability modality enabled feature.ModalityPermeability }) |> commit ("Feature " + featureId + " permeability changed.")
        | SetTacticalCoverIntegrity(featureId, integrity) ->
            state.TacticalDocument |> replaceTacticalFeature featureId (fun feature -> { feature with DirectionalCover = feature.DirectionalCover |> Option.map (fun cover -> { cover with CoverIntegrity = max 0 (min cover.CoverMaximumIntegrity integrity) }) }) |> commit ("Feature " + featureId + " cover integrity changed.")
        | RunTacticalEnvironmentAction(featureId, environmentAction) ->
            match state.TacticalPreview with
            | Error _ -> refreshTacticalState "Tactical action unavailable while validation fails." state
            | Ok environment ->
                let runtimeFeatureId =
                    environment.EnvironmentFeatures
                    |> List.tryFind (fun feature ->
                        feature.EnvironmentFeatureId = featureId
                        || feature.EnvironmentFeatureId.EndsWith(
                            ":" + featureId,
                            StringComparison.Ordinal
                        ))
                    |> Option.map _.EnvironmentFeatureId
                    |> Option.defaultValue featureId
                let knowledge =
                    { EnvironmentKnowledgeIdentity = "map-editor-preview"
                      EnvironmentKnowledgeRevision = 0L
                      KnownEnvironmentFeatureIds = environment.EnvironmentFeatures |> List.map _.EnvironmentFeatureId |> Set.ofList
                      KnownEnvironmentStateFeatureIds = environment.EnvironmentFeatures |> List.map _.EnvironmentFeatureId |> Set.ofList
                      KnownEnvironmentFacts = Set.empty }
                match SIR.Simulation.TacticalEnvironment.applyAction knowledge environment.EnvironmentContentIdentity runtimeFeatureId environmentAction environment with
                | Ok result -> { state with TacticalPreview = Ok result.UpdatedEnvironment; TacticalAnnouncement = "Feature " + featureId + " action applied in the simulator preview." }
                | Error failure -> { state with TacticalAnnouncement = "Tactical action rejected: " + string failure + "." }
        | UndoTacticalParcelEdit ->
            match state.TacticalUndo with
            | previous :: rest -> { state with TacticalDocument = previous; TacticalUndo = rest; TacticalRedo = state.TacticalDocument :: state.TacticalRedo } |> refreshTacticalState "Tactical parcel edit undone."
            | [] -> state
        | RedoTacticalParcelEdit ->
            match state.TacticalRedo with
            | next :: rest -> { state with TacticalDocument = next; TacticalUndo = state.TacticalDocument :: state.TacticalUndo; TacticalRedo = rest } |> refreshTacticalState "Tactical parcel edit redone."
            | [] -> state
        | RefreshTacticalPreview -> refreshTacticalState "Tactical parcel validation and preview refreshed." state

    let updateWithExport action state =
        let next = updateTacticalParcelEditor action state
        next, exportTacticalParcelDocument next.TacticalDocument
