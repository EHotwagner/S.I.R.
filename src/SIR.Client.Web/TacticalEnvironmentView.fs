module SIR.Client.Web.TacticalEnvironmentView

open Feliz
open Fable.Core
open Fable.Core.JsInterop
open Browser.Types
open SIR.Client
open SIR.Domain
open SIR.Client.Web.AppTypes

[<Emit("""
const blob = new Blob([$0], { type: "text/plain;charset=utf-8" });
const url = URL.createObjectURL(blob);
const anchor = document.createElement("a");
anchor.href = url;
anchor.download = "tactical-environment.sir-parcel";
anchor.click();
URL.revokeObjectURL(url);
""")>]
let downloadDocument (_content: string) : unit = jsNative

[<Emit("(() => { const t = $0; const tag = t && typeof t.tagName === 'string' ? t.tagName.toLowerCase() : ''; return tag === 'input' || tag === 'button' || tag === 'summary' || (tag === 'a' && t.hasAttribute('href')) || tag === 'textarea' || tag === 'select' || (t && t.isContentEditable); })()")>]
let private isNativeInteractiveTarget (_target: EventTarget) : bool = jsNative

let acceptsGlobalKeyboardTarget target allowModifiedShortcut =
    allowModifiedShortcut || not (isNativeInteractiveTarget target)

let editorDomain = function
    | TerrainTools -> TerrainDomain
    | UnitTools -> UnitDomain
    | EdgeTools -> EdgeDomain
    | ZoneTools -> RegionDomain
    | TacticalEnvironmentTools
    | DocumentTools -> DocumentDomain

let private commandButton properties =
    let hasRegistryBinding =
        properties
        |> List.exists (fun property ->
            let name, _ = unbox<string * obj> property
            name = "aria-keyshortcuts")
    Html.button (
        if hasRegistryBinding then properties
        else
            properties
            @ [ prop.custom ("data-binding-state", "unassigned")
                prop.custom ("aria-description", "Shortcut: Unassigned") ])

let view
    (state: MapEditorState)
    (tactical: TacticalParcelEditor.TacticalParcelEditorState)
    (tacticalImportText: string)
    (dispatch: Msg -> unit)
    =
    let preview =
        MapEditorSimulator.tacticalEnvironmentPreview tactical
    let loadFixture (plot, variants) =
        dispatch (
            TacticalParcelChanged(
                TacticalParcelEditor.ReplaceTacticalDocument
                    { TacticalPlot = plot
                      TacticalVariants = variants }
            )
        )
    let stateLabel = function
        | EnvironmentFeatureState.Intact -> "intact"
        | EnvironmentFeatureState.Closed -> "closed"
        | EnvironmentFeatureState.Open -> "open"
        | EnvironmentFeatureState.Damaged -> "damaged"
        | EnvironmentFeatureState.Breached -> "breached"
        | EnvironmentFeatureState.Destroyed -> "destroyed"
    let actionValue = function
        | "open" -> Some EnvironmentAction.Open
        | "close" -> Some EnvironmentAction.Close
        | "damage" -> Some(EnvironmentAction.Damage 25)
        | "breach" -> Some(EnvironmentAction.Breach 1)
        | "destroy" -> Some EnvironmentAction.Destroy
        | _ -> None
    Html.section [
        prop.ariaLabel "Tactical environment authoring"
        prop.custom ("data-testid", "tactical-environment-authoring")
        prop.children [
            Html.h3 "Tactical environment"
            Html.p [
                prop.role.status
                prop.ariaLive.polite
                prop.custom ("data-testid", "tactical-preview-status")
                prop.text tactical.TacticalAnnouncement
            ]
            Html.div [
                prop.className "control-row"
                prop.role.toolbar
                prop.ariaLabel "Tactical parcel documents"
                prop.children [
                    commandButton [
                        prop.type'.button
                        prop.text "Exterior fixture"
                        prop.custom ("data-testid", "tactical-load-exterior")
                        prop.onClick (fun _ -> loadFixture SIR.Simulation.TacticalEnvironment.exteriorParcelSet)
                    ]
                    commandButton [
                        prop.type'.button
                        prop.text "Interior breach fixture"
                        prop.custom ("data-testid", "tactical-load-interior")
                        prop.onClick (fun _ -> loadFixture SIR.Simulation.TacticalEnvironment.interiorBreachParcelSet)
                    ]
                    commandButton [
                        prop.type'.button
                        prop.text "Migrate current map"
                        prop.onClick (fun _ ->
                            state.Map
                            |> TacticalParcelEditor.migrateLegacyTacticalEnvironment
                            |> TacticalParcelEditor.ReplaceTacticalDocument
                            |> TacticalParcelChanged
                            |> dispatch)
                    ]
                    commandButton [
                        prop.type'.button
                        prop.text "Undo"
                        prop.disabled tactical.TacticalUndo.IsEmpty
                        prop.custom ("data-testid", "tactical-undo")
                        prop.onClick (fun _ -> dispatch (TacticalParcelChanged TacticalParcelEditor.UndoTacticalParcelEdit))
                    ]
                    commandButton [
                        prop.type'.button
                        prop.text "Redo"
                        prop.disabled tactical.TacticalRedo.IsEmpty
                        prop.custom ("data-testid", "tactical-redo")
                        prop.onClick (fun _ -> dispatch (TacticalParcelChanged TacticalParcelEditor.RedoTacticalParcelEdit))
                    ]
                    commandButton [
                        prop.type'.button
                        prop.text "Refresh preview"
                        prop.custom ("data-testid", "tactical-refresh-preview")
                        prop.onClick (fun _ -> dispatch (TacticalParcelChanged TacticalParcelEditor.RefreshTacticalPreview))
                    ]
                ]
            ]
            Html.p [
                prop.children [
                    Html.span "Canonical content identity: "
                    Html.code [
                        prop.custom ("data-testid", "tactical-content-identity")
                        prop.text (preview.TacticalPreviewIdentity |> Option.defaultValue "validation-failed")
                    ]
                ]
            ]
            Html.p [ prop.custom ("data-testid", "tactical-editor-revision"); prop.text (string state.Revision.Number) ]
            Html.p [ prop.custom ("data-testid", "tactical-editor-history"); prop.text (string state.UndoHistory.Length + " undo · " + string state.RedoHistory.Length + " redo") ]
            commandButton [ prop.type'.button; prop.text "Enter simulation"; prop.custom ("data-testid", "tactical-enter-simulate"); prop.onClick (fun _ -> dispatch (WorkspaceChanged SimulatorWorkspace)) ]
            Html.p (string preview.TacticalPreviewFeatureCount + " features · " + string preview.TacticalPreviewWalkableCellCount + " walkable cells · revision " + string preview.TacticalPreviewSpatialRevision)
            for message in preview.TacticalPreviewFindingMessages do
                Html.p [ prop.role.alert; prop.text message ]
            for variant in tactical.TacticalDocument.TacticalVariants do
                for feature in variant.ParcelFeatures do
                    Html.fieldSet [
                        prop.custom ("data-feature-id", feature.EnvironmentFeatureId)
                        prop.children [
                            Html.legend (feature.EnvironmentFeatureId + " · " + string feature.EnvironmentKind)
                            Html.div [
                                prop.className "control-row"
                                prop.role.group
                                prop.ariaLabel ("Authored state for " + feature.EnvironmentFeatureId)
                                prop.children [
                                    for featureState in [ EnvironmentFeatureState.Intact; EnvironmentFeatureState.Closed; EnvironmentFeatureState.Open; EnvironmentFeatureState.Damaged; EnvironmentFeatureState.Breached; EnvironmentFeatureState.Destroyed ] do
                                        let label = stateLabel featureState
                                        commandButton [
                                            prop.type'.button
                                            prop.text label
                                            prop.ariaPressed (feature.EnvironmentState = featureState)
                                            prop.custom ("data-testid", "tactical-state-" + feature.EnvironmentFeatureId + "-" + label)
                                            prop.onClick (fun _ -> dispatch (TacticalParcelChanged(TacticalParcelEditor.SetTacticalFeatureState(feature.EnvironmentFeatureId, featureState))))
                                        ]
                                ]
                            ]
                            Html.div [
                                prop.className "control-row"
                                prop.role.group
                                prop.ariaLabel ("Simulator actions for " + feature.EnvironmentFeatureId)
                                prop.children [
                                    for capability in feature.CapabilityDescriptors do
                                        match actionValue capability.DescriptorAction with
                                        | Some environmentAction ->
                                            commandButton [
                                                prop.type'.button
                                                prop.text capability.DescriptorAction
                                                prop.custom ("data-testid", "tactical-action-" + feature.EnvironmentFeatureId + "-" + capability.DescriptorAction)
                                                prop.onClick (fun _ -> dispatch (TacticalParcelChanged(TacticalParcelEditor.RunTacticalEnvironmentAction(feature.EnvironmentFeatureId, environmentAction))))
                                            ]
                                        | None -> ()
                                ]
                            ]
                            Html.div [
                                prop.className "control-row"
                                prop.role.group
                                prop.ariaLabel ("Permeability for " + feature.EnvironmentFeatureId)
                                prop.children [
                                    for label, modality, enabled in
                                        [ "movement", EnvironmentModality.Movement, feature.ModalityPermeability.AllowsMovement
                                          "sight", EnvironmentModality.Sight, feature.ModalityPermeability.AllowsSight
                                          "projectile", EnvironmentModality.Projectile, feature.ModalityPermeability.AllowsProjectile
                                          "area effect", EnvironmentModality.AreaEffect, feature.ModalityPermeability.AllowsAreaEffect
                                          "sound", EnvironmentModality.Sound, feature.ModalityPermeability.AllowsSound
                                          "cover", EnvironmentModality.Cover, feature.ModalityPermeability.ProvidesCover
                                          "interaction", EnvironmentModality.Interaction "editor", feature.ModalityPermeability.AllowsInteraction ] do
                                        commandButton [
                                            prop.type'.button
                                            prop.text label
                                            prop.ariaPressed enabled
                                            prop.custom ("data-testid", "tactical-permeability-" + feature.EnvironmentFeatureId + "-" + label.Replace(" ", "-"))
                                            prop.onClick (fun _ -> dispatch (TacticalParcelChanged(TacticalParcelEditor.SetTacticalPermeability(feature.EnvironmentFeatureId, modality, not enabled))))
                                        ]
                                ]
                            ]
                            match feature.DirectionalCover with
                            | Some cover ->
                                Html.label [
                                    prop.children [
                                        Html.span ("Cover integrity " + string cover.CoverIntegrity + " / " + string cover.CoverMaximumIntegrity)
                                        Html.input [
                                            prop.type'.range
                                            prop.min 0
                                            prop.max cover.CoverMaximumIntegrity
                                            prop.value cover.CoverIntegrity
                                            prop.ariaLabel ("Cover integrity for " + feature.EnvironmentFeatureId)
                                            prop.onChange (fun (value: int) -> dispatch (TacticalParcelChanged(TacticalParcelEditor.SetTacticalCoverIntegrity(feature.EnvironmentFeatureId, int32 value))))
                                        ]
                                    ]
                                ]
                            | None -> ()
                        ]
                    ]
            Html.label [
                prop.children [
                    Html.span "Tactical parcel interchange"
                    Html.textarea [
                        prop.custom ("data-testid", "tactical-parcel-interchange")
                        prop.ariaLabel "Tactical parcel interchange"
                        prop.value tacticalImportText
                        prop.onChange (TacticalParcelImportTextChanged >> dispatch)
                    ]
                ]
            ]
            Html.div [
                prop.className "control-row"
                prop.children [
                    commandButton [ prop.type'.button; prop.text "Import parcel"; prop.custom ("data-testid", "tactical-import"); prop.onClick (fun _ -> dispatch ImportTacticalParcelDocument) ]
                    commandButton [ prop.type'.button; prop.text "Export parcel"; prop.custom ("data-testid", "tactical-export"); prop.onClick (fun _ -> dispatch ExportTacticalParcelDocument) ]
                ]
            ]
        ]
    ]

let simulationView (simulator: SimulatorHandoff) (dispatch: Msg -> unit) =
    let actionValue = function
        | "open" -> Some EnvironmentAction.Open
        | "close" -> Some EnvironmentAction.Close
        | "damage" -> Some(EnvironmentAction.Damage 25)
        | "breach" -> Some(EnvironmentAction.Breach 1)
        | "destroy" -> Some EnvironmentAction.Destroy
        | _ -> None
    Html.section [
        prop.ariaLabel "Tactical environment simulation"
        prop.custom ("data-testid", "tactical-environment-simulation")
        prop.children [
            Html.h3 "Tactical environment simulation"
            Html.p [ prop.role.status; prop.ariaLive.polite; prop.custom ("data-testid", "tactical-runtime-status"); prop.text (simulator.LastEvents |> List.tryLast |> Option.defaultValue "Authoritative tactical environment transfer ready.") ]
            Html.p [ prop.custom ("data-testid", "tactical-runtime-revision"); prop.text (string simulator.Revision.Number) ]
            Html.code [ prop.custom ("data-testid", "tactical-runtime-assembly-identity"); prop.text simulator.RuntimeEnvironment.EnvironmentAssemblyIdentity ]
            Html.code [ prop.custom ("data-testid", "tactical-runtime-initial-identity"); prop.text simulator.InitialEnvironment.EnvironmentContentIdentity ]
            Html.code [ prop.custom ("data-testid", "tactical-runtime-identity"); prop.text simulator.RuntimeEnvironment.EnvironmentContentIdentity ]
            Html.div [
                prop.role.toolbar
                prop.ariaLabel "Tactical simulation playback"
                prop.children [
                    commandButton [ prop.type'.button; prop.text "Step"; prop.custom ("data-testid", "tactical-runtime-step"); prop.onClick (fun _ -> dispatch (SimulatorChanged StepSimulator)) ]
                    commandButton [ prop.type'.button; prop.text "Reset"; prop.custom ("data-testid", "tactical-runtime-reset"); prop.onClick (fun _ -> dispatch ResetSimulator) ]
                    commandButton [ prop.type'.button; prop.text "Replay actions"; prop.custom ("data-testid", "tactical-runtime-replay"); prop.onClick (fun _ -> dispatch (SimulatorChanged ReplaySimulatorEnvironment)) ]
                ]
            ]
            for feature in simulator.RuntimeEnvironment.EnvironmentFeatures do
                let authoredId = feature.EnvironmentFeatureId.Split(':') |> Array.last
                Html.fieldSet [
                    prop.custom ("data-runtime-feature-id", feature.EnvironmentFeatureId)
                    prop.custom ("data-feature-id", authoredId)
                    prop.children [
                        Html.legend (authoredId + " · " + string feature.EnvironmentState)
                        Html.span [ prop.custom ("data-testid", "tactical-runtime-state-" + authoredId); prop.text ((string feature.EnvironmentState).ToLowerInvariant()) ]
                        for capability in feature.CapabilityDescriptors do
                            match actionValue capability.DescriptorAction with
                            | Some action ->
                                commandButton [
                                    prop.type'.button
                                    prop.text capability.DescriptorAction
                                    prop.custom ("data-testid", "tactical-runtime-action-" + authoredId + "-" + capability.DescriptorAction)
                                    prop.onClick (fun _ -> dispatch (SimulatorChanged(ApplySimulatorEnvironmentAction(authoredId, action))))
                                ]
                            | None -> ()
                    ]
                ]
        ]
    ]
