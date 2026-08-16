module SIR.Client.Web.ClientFeatureRuntime

open Elmish
open Feliz
open SIR.Client.Web.AppTypes
open SIR.Client.Web.BrowserInfrastructure

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
    if panelId = "data" then
        let requested, load = update (FeatureLoader.Request FeatureLoader.rulesExplorer) opened
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
