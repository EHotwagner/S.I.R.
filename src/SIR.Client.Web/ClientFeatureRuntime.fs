module SIR.Client.Web.ClientFeatureRuntime

open Elmish
open Feliz
open SIR.Client.Web.AppTypes
open SIR.Client.Web.BrowserInfrastructure
open SIR.Client.Web.EnvironmentFeatureContract

let update message model =
    match message with
    | FeatureLoader.Request feature ->
        let identity = FeatureLoader.identityFor feature
        match FeatureLoader.stateFor feature model.ClientFeatures with
        | FeatureLoader.Loaded _ ->
            { model with
                DocumentationOpen = model.DocumentationOpen || feature = FeatureLoader.docs
                FeatureLoaderDiagnostic = None },
            Cmd.none
        | FeatureLoader.Loading _ ->
            { model with DocumentationOpen = model.DocumentationOpen || feature = FeatureLoader.docs }, Cmd.none
        | FeatureLoader.Idle
        | FeatureLoader.Failed _ ->
            { model with
                ClientFeatures = FeatureLoader.beginLoad identity model.ClientFeatures
                DocumentationOpen = model.DocumentationOpen || feature = FeatureLoader.docs
                FeatureLoaderDiagnostic = None },
            Cmd.OfAsync.perform loadClientFeature identity (fun result ->
                ClientFeatureMessage(FeatureLoader.ImportCompleted(identity, result)))
    | FeatureLoader.ImportCompleted(expected, result) ->
        let current = FeatureLoader.stateFor expected.Feature model.ClientFeatures
        match FeatureLoader.reconcile expected result current with
        | FeatureLoader.Applied state ->
            { model with
                ClientFeatures = Map.add expected.Feature state model.ClientFeatures
                FeatureLoaderDiagnostic =
                    match state with
                    | FeatureLoader.Failed(_, failure) -> Some(FeatureLoader.describeFailure failure)
                    | _ -> None },
            Cmd.none
        | FeatureLoader.IgnoredStale failure ->
            { model with FeatureLoaderDiagnostic = Some(FeatureLoader.describeFailure failure) }, Cmd.none

let private actionButton (text: string) (label: string) onClick =
    Html.button [
        prop.type'.button
        prop.className "command-button"
        prop.text text
        prop.ariaLabel label
        prop.custom ("data-binding-state", "unassigned")
        prop.custom ("aria-description", "Keyboard binding unassigned.")
        prop.onClick onClick
    ]

[<ReactLazyComponent>]
let private LazyDocumentationPanel () =
    React.DynamicImported("../../docs-feature.js")

let documentation model dispatch =
    if not model.DocumentationOpen then Html.none
    else
        Html.aside [
            prop.id "client-documentation-drawer"
            prop.className "client-documentation-drawer"
            prop.ariaLabel "Documentation drawer"
            prop.children [
                actionButton "Close" "Close documentation" (fun _ -> dispatch CloseDocumentation)
                match FeatureLoader.stateFor FeatureLoader.docs model.ClientFeatures with
                | FeatureLoader.Loaded _ ->
                    React.Suspense(
                        [ LazyDocumentationPanel() ],
                        fallback = Html.p [ prop.role.status; prop.text "Rendering documentation…" ]
                    )
                | FeatureLoader.Loading _ ->
                    Html.p [ prop.role.status; prop.ariaLive.polite; prop.text "Loading documentation…" ]
                | FeatureLoader.Failed(_, failure) ->
                    Html.section [
                        prop.ariaLabel "Documentation load failure"
                        prop.children [
                            Html.p [ prop.role.alert; prop.text (FeatureLoader.describeFailure failure) ]
                            actionButton "Retry" "Retry documentation" (fun _ ->
                                dispatch (ClientFeatureMessage(FeatureLoader.Request FeatureLoader.docs)))
                        ]
                    ]
                | FeatureLoader.Idle ->
                    Html.p [ prop.role.status; prop.text "Documentation is ready to load." ]
            ]
        ]

let toolbar model dispatch =
    Html.nav [
        prop.className "client-feature-toolbar"
        prop.ariaLabel "Client features"
        prop.children [
            Html.button [
                prop.type'.button
                prop.className "command-button"
                prop.text "Docs"
                prop.ariaPressed model.DocumentationOpen
                prop.ariaControls "client-documentation-drawer"
                prop.custom ("data-binding-state", "unassigned")
                prop.custom ("aria-description", "Keyboard binding unassigned.")
                prop.onClick (fun _ ->
                    dispatch (ClientFeatureMessage(FeatureLoader.Request FeatureLoader.docs)))
            ]
        ]
    ]

let requestSupportingPanel panelId opened focused =
    if panelId = "rules" || panelId = "data" || panelId = "samples" || panelId = "tools" then
        let feature =
            if panelId = "rules" then FeatureLoader.rulesWorkbench
            elif panelId = "data" then FeatureLoader.rulesExplorer
            elif panelId = "samples" then FeatureLoader.samples
            else FeatureLoader.tacticalEnvironment
        let requested, load = update (FeatureLoader.Request feature) opened
        requested, Cmd.batch [ focused; load ]
    else opened, focused

[<ReactLazyComponent>]
let private LazyRulesExplorer simulator selectedUnit bootstrap =
    React.DynamicImported("../RulesExplorer.js")

let rulesExplorer model dispatch =
    match FeatureLoader.stateFor FeatureLoader.rulesExplorer model.ClientFeatures with
    | FeatureLoader.Loaded _ ->
        React.Suspense(
            [ LazyRulesExplorer model.Simulator model.SimulatorSelectedUnit model.Live.Bootstrap ],
            fallback = Html.p "Rendering authoritative data…"
        )
    | FeatureLoader.Loading _ ->
        Html.p [ prop.role.status; prop.ariaLive.polite; prop.text "Loading authoritative data…" ]
    | FeatureLoader.Failed(_, failure) ->
        Html.section [
            prop.ariaLabel "Rules Explorer load failure"
            prop.children [
                Html.p [ prop.role.alert; prop.text (FeatureLoader.describeFailure failure) ]
                actionButton "Retry" "Retry Rules Explorer" (fun _ ->
                    dispatch (ClientFeatureMessage(FeatureLoader.Request FeatureLoader.rulesExplorer)))
            ]
        ]
    | FeatureLoader.Idle ->
        actionButton "Load data" "Load Rules Explorer" (fun _ ->
            dispatch (ClientFeatureMessage(FeatureLoader.Request FeatureLoader.rulesExplorer)))

let samplesPanel model dispatch =
    match FeatureLoader.stateFor FeatureLoader.samples model.ClientFeatures with
    | FeatureLoader.Loaded _ ->
        Html.div [
            prop.ariaLabel "Curated samples feature"
            prop.ref (fun root ->
                if not (isNull root) then
                    renderSamplesFeature root (fun action editor simulator replay frames ->
                        if action = "map" then dispatch (LoadMapSample(editor, simulator))
                        elif action = "simulation" then dispatch (LoadSimulationSample(editor, simulator))
                        else
                            let separator = replay.IndexOf '\n'
                            dispatch (LoadReplaySample(replay.Substring(0, separator), replay.Substring(separator + 1), frames)))
            )
        ]
    | FeatureLoader.Loading _ ->
        Html.p [ prop.role.status; prop.ariaLive.polite; prop.text "Loading curated samples…" ]
    | FeatureLoader.Failed(_, failure) ->
        Html.section [
            prop.ariaLabel "Samples load failure"
            prop.children [
                Html.p [ prop.role.alert; prop.text (FeatureLoader.describeFailure failure) ]
                actionButton "Retry" "Retry curated samples" (fun _ ->
                    dispatch (ClientFeatureMessage(FeatureLoader.Request FeatureLoader.samples)))
            ]
        ]
    | FeatureLoader.Idle ->
        actionButton "Load samples" "Load curated samples" (fun _ ->
            dispatch (ClientFeatureMessage(FeatureLoader.Request FeatureLoader.samples)))

[<ReactLazyComponent>]
let private LazyRulesWorkbenchPanel model evidence dispatch =
    React.DynamicImported("./RulesWorkbenchView.js")

let rulesWorkbenchPanel model evidence dispatch =
    match FeatureLoader.stateFor FeatureLoader.rulesWorkbench model.ClientFeatures with
    | FeatureLoader.Loaded _ ->
        React.Suspense(
            [ LazyRulesWorkbenchPanel model evidence dispatch ],
            fallback = Html.p "Rendering Rules workbench…"
        )
    | FeatureLoader.Loading _ ->
        Html.p [ prop.role.status; prop.ariaLive.polite; prop.text "Loading Rules workbench…" ]
    | FeatureLoader.Failed(_, failure) ->
        Html.section [
            prop.ariaLabel "Rules workbench load failure"
            prop.children [
                Html.p [ prop.role.alert; prop.text (FeatureLoader.describeFailure failure) ]
                actionButton "Retry" "Retry Rules workbench" (fun _ ->
                    dispatch (ClientFeatureMessage(FeatureLoader.Request FeatureLoader.rulesWorkbench)))
            ]
        ]
    | FeatureLoader.Idle ->
        actionButton "Load rules" "Load Rules workbench" (fun _ ->
            dispatch (ClientFeatureMessage(FeatureLoader.Request FeatureLoader.rulesWorkbench)))

let private environmentCallbacks dispatch =
    { ParcelChanged = TacticalParcelChanged >> dispatch
      EnterSimulation = fun () -> dispatch (WorkspaceChanged SimulatorWorkspace)
      ImportTextChanged = TacticalParcelImportTextChanged >> dispatch
      ImportDocument = fun () -> dispatch ImportTacticalParcelDocument
      ExportDocument = fun () -> dispatch ExportTacticalParcelDocument
      SimulatorChanged = SimulatorChanged >> dispatch
      ResetSimulator = fun () -> dispatch ResetSimulator }

[<ReactLazyComponent>]
let private LazyTacticalEnvironmentPanel editor tactical importText simulator callbacks =
    React.DynamicImported("./TacticalEnvironmentView.js")

let tacticalEnvironmentPanel clientFeatures editor tactical importText simulator dispatch =
    match FeatureLoader.stateFor FeatureLoader.tacticalEnvironment clientFeatures with
    | FeatureLoader.Loaded _ ->
        React.Suspense(
            [ LazyTacticalEnvironmentPanel editor tactical importText simulator (environmentCallbacks dispatch) ],
            fallback = Html.p "Rendering tactical environment…"
        )
    | FeatureLoader.Loading _ ->
        Html.p [ prop.role.status; prop.ariaLive.polite; prop.text "Loading tactical environment…" ]
    | FeatureLoader.Failed(_, failure) ->
        Html.section [
            prop.ariaLabel "Tactical environment load failure"
            prop.children [
                Html.p [ prop.role.alert; prop.text (FeatureLoader.describeFailure failure) ]
                actionButton "Retry" "Retry tactical environment" (fun _ ->
                    dispatch (ClientFeatureMessage(FeatureLoader.Request FeatureLoader.tacticalEnvironment)))
            ]
        ]
    | FeatureLoader.Idle ->
        actionButton "Load tools" "Load tactical environment tools" (fun _ ->
            dispatch (ClientFeatureMessage(FeatureLoader.Request FeatureLoader.tacticalEnvironment)))
