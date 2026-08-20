module SIR.Client.TestsTacticalWorkspaceLayoutQualification

open SIR.Client

let private require condition message =
    if not condition then failwith message

let run () =
    let baseline = TacticalWorkspaceLayout.fieldFocus
    let exported = TacticalWorkspaceLayout.exportProfile baseline
    let imported =
        TacticalWorkspaceLayout.importProfile exported
        |> Result.defaultWith (fun diagnostics ->
            failwithf "Field Focus export failed strict import: %A" diagnostics)

    require
        (imported = baseline
         && baseline.LeftSidebar.Width = 208
         && baseline.RightSidebar.Width = 224
         && baseline.BottomPanel.Height = 152
         && not (TacticalWorkspaceLayout.bottomVisible baseline)
         && not (TacticalWorkspaceLayout.bottomCollapsed Editor baseline)
         && not (TacticalWorkspaceLayout.bottomCollapsed Plan baseline)
         && (TacticalWorkspaceLayout.panelsOn Left baseline
             |> List.map _.PanelId
             |> List.take 3) = [ "roster"; "tools"; "layers" ]
         && (TacticalWorkspaceLayout.panelsOn Right baseline
             |> List.map _.PanelId
             |> List.take 3) = [ "selection"; "validation"; "document" ])
        "Field Focus defaults or deterministic round-trip diverged."

    let shownBottom =
        baseline
        |> TacticalWorkspaceLayout.toggleBottomPanelVisibility
    let shownBottomRoundTrip =
        shownBottom
        |> TacticalWorkspaceLayout.exportProfile
        |> TacticalWorkspaceLayout.importProfile
        |> Result.defaultWith (fun diagnostics ->
            failwithf "Hidden bottom-panel profile failed round-trip: %A" diagnostics)
    require
        (TacticalWorkspaceLayout.bottomVisible shownBottom
         && shownBottomRoundTrip = shownBottom
         && not (shownBottom
             |> TacticalWorkspaceLayout.toggleBottomPanelVisibility
             |> TacticalWorkspaceLayout.bottomVisible)
         && TacticalWorkspaceLayout.reset shownBottom = baseline)
        "Bottom-panel false/true visibility, persistence, or reset semantics diverged."

    let resized =
        baseline |> TacticalWorkspaceLayout.resizeBottomPanel 327
    let resizedRoundTrip =
        resized
        |> TacticalWorkspaceLayout.exportProfile
        |> TacticalWorkspaceLayout.importProfile
    require
        (resized.BottomPanel.Height = 327
         && (baseline |> TacticalWorkspaceLayout.resizeBottomPanel 1).BottomPanel.Height = 96
         && (baseline |> TacticalWorkspaceLayout.resizeBottomPanel 999).BottomPanel.Height = 480
         && resizedRoundTrip = Ok resized)
        "Bottom-panel resizing did not clamp or persist deterministically."

    let resizedSidebars =
        baseline
        |> TacticalWorkspaceLayout.resizeSidebar Left 312
        |> TacticalWorkspaceLayout.resizeSidebar Right 544
    let resizedSidebarsRoundTrip =
        resizedSidebars
        |> TacticalWorkspaceLayout.exportProfile
        |> TacticalWorkspaceLayout.importProfile
    require
        (resizedSidebars.LeftSidebar.Width = 312
         && resizedSidebars.RightSidebar.Width = 544
         && (baseline |> TacticalWorkspaceLayout.resizeSidebar Left 1).LeftSidebar.Width = 160
         && resizedSidebarsRoundTrip = Ok resizedSidebars)
        "Sidebar resizing did not retain a functional minimum or persist scalable widths deterministically."

    let configured =
        baseline
        |> TacticalWorkspaceLayout.togglePanelCollapsed "tools"
        |> TacticalWorkspaceLayout.togglePanelVisibility "layers"
        |> TacticalWorkspaceLayout.movePanel "validation" Left
        |> TacticalWorkspaceLayout.reorderPanel "validation" -1
        |> TacticalWorkspaceLayout.toggleDrawer Left
        |> TacticalWorkspaceLayout.toggleBottomPanel Editor

    require
        ((configured.Placements
          |> List.find (fun panel -> panel.PanelId = "tools"))
             .Collapsed
         && not (
             (configured.Placements
              |> List.find (fun panel -> panel.PanelId = "layers"))
                 .Visible
         )
         && (configured.Placements
             |> List.find (fun panel -> panel.PanelId = "validation"))
             .Side = Left
         && configured.LeftSidebar.DrawerOpen
         && TacticalWorkspaceLayout.bottomCollapsed Editor configured
         && TacticalWorkspaceLayout.reset configured = baseline)
        "Panel show/hide, collapse, move, order, drawer, timeline, or reset diverged."

    let versionZero =
        exported
            .Replace("\"schemaVersion\":1,\"placements\":", "\"schemaVersion\":0,\"panels\":")
            .Replace(",\"leftSidebar\":{\"width\":208,\"drawerOpen\":false}", ",\"leftWidth\":208")
            .Replace(",\"rightSidebar\":{\"width\":224,\"drawerOpen\":false}", ",\"rightWidth\":224")
            .Replace(
                ",\"bottomPanel\":{\"visible\":false,\"height\":152,\"collapsedInEditor\":false,\"collapsedOutsideEditor\":false}",
                ",\"timelineHeight\":152"
            )
    let migrated =
        TacticalWorkspaceLayout.importProfile versionZero
        |> Result.defaultWith (fun diagnostics ->
            failwithf "Version-zero layout migration failed: %A" diagnostics)
    require (migrated = baseline) "Version-zero layout did not migrate deterministically."

    let missingNewPanel =
        exported.Replace(
            ",{\"panelId\":\"diagnostics\",\"side\":\"right\",\"order\":5,\"visible\":false,\"collapsed\":true}",
            ""
        )
    let withDefaultedPanel =
        TacticalWorkspaceLayout.importProfile missingNewPanel
        |> Result.defaultWith (fun diagnostics ->
            failwithf "Missing newly registered panel failed safe defaulting: %A" diagnostics)
    require
        (withDefaultedPanel.Placements
         |> List.exists (fun panel ->
             panel.PanelId = "diagnostics"
             && not panel.Visible
             && panel.Side = Right))
        "A newly introduced panel did not receive its deterministic default."

    let invalidIntegerGrammar =
        [ exported.Replace("\"schemaVersion\":1", "\"schemaVersion\":01")
          exported.Replace("\"order\":0", "\"order\":00")
          exported.Replace("\"width\":208", "\"width\":0208")
          exported.Replace("\"schemaVersion\":1", "\"schemaVersion\":-01")
          exported.Replace("\"order\":0", "\"order\":-00")
          "{\"schemaVersion\":-}"
          "{\"schemaVersion\":١}"
          "\u00A0" + exported ]
        |> List.map TacticalWorkspaceLayout.importProfile
    require
        (invalidIntegerGrammar
         |> List.forall (function
             | Error diagnostics ->
                 diagnostics
                 |> List.exists (function
                     | MalformedLayoutProfile _ -> true
                     | _ -> false)
             | Ok _ -> false))
        "Non-ASCII, leading-zero, malformed-negative integer, or non-JSON whitespace input was not rejected as malformed."
    require
        (match TacticalWorkspaceLayout.importProfile "{\"schemaVersion\":-1}" with
         | Error [ UnsupportedLayoutSchema -1 ] -> true
         | _ -> false)
        "A grammatically valid negative JSON integer did not reach schema validation."

    let rejected =
        [ exported.Replace("\"schemaVersion\":1", "\"schemaVersion\":99")
          exported.Replace("\"panelId\":\"roster\"", "\"panelId\":\"unknown\"")
          exported.Replace("\"width\":208", "\"width\":12")
          exported.Replace("\"bottomPanel\":", "\"unexpected\":0,\"bottomPanel\":")
          exported.Replace("\"panelId\":\"tools\"", "\"panelId\":\"roster\"")
          exported.Replace("],\"leftSidebar\":", ",],\"leftSidebar\":")
          exported.Substring(0, exported.Length - 1) + ",}"
          exported.Substring(0, exported.Length - 17)
          "{\"schemaVersion\":1,\"placements\":[}"
          "{\"schemaVersion\":" ]
        |> List.map TacticalWorkspaceLayout.importProfile
    require
        (rejected |> List.forall (function Error _ -> true | Ok _ -> false))
        "Future, unknown, invalid, duplicate, trailing-comma, malformed, or truncated layout input was accepted."

    printfn
        "Tactical layout qualification passed: %d registered panels, deterministic Field Focus JSON, migration, safe defaults, strict ASCII integer/malformed/trailing-comma rejection, reset, placement, drawer, and bottom-panel visibility/collapse behavior."
        TacticalWorkspaceLayout.panelRegistry.Length
