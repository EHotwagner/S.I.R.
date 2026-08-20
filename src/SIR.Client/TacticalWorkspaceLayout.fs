namespace SIR.Client

open System

type SidebarSide =
    | Left
    | Right

type TacticalPanelDefinition =
    { Id: string
      Label: string
      DefaultSide: SidebarSide
      DefaultOrder: int
      DefaultVisible: bool
      DefaultCollapsed: bool }

type PanelPlacement =
    { PanelId: string
      Side: SidebarSide
      Order: int
      Visible: bool
      Collapsed: bool }

type SidebarLayout =
    { Width: int
      DrawerOpen: bool }

type BottomPanelLayout =
    { Visible: bool
      Height: int
      CollapsedInEditor: bool
      CollapsedOutsideEditor: bool }

type TacticalLayoutProfile =
    { SchemaVersion: int
      Placements: PanelPlacement list
      LeftSidebar: SidebarLayout
      RightSidebar: SidebarLayout
      BottomPanel: BottomPanelLayout }

type TacticalLayoutDiagnostic =
    | UnknownPanel of string
    | DuplicatePanel of string
    | MalformedLayoutProfile of string
    | UnsupportedLayoutSchema of int
    | InvalidLayoutDimension of name: string * value: int

[<RequireQualifiedAccess>]
module TacticalWorkspaceLayout =
    [<Literal>]
    let SchemaVersion = 1

    let panelRegistry =
        [ { Id = "roster"; Label = "Roster / outliner"; DefaultSide = Left
            DefaultOrder = 0; DefaultVisible = true; DefaultCollapsed = false }
          { Id = "tools"; Label = "Tools"; DefaultSide = Left
            DefaultOrder = 1; DefaultVisible = true; DefaultCollapsed = false }
          { Id = "layers"; Label = "Layers"; DefaultSide = Left
            DefaultOrder = 2; DefaultVisible = true; DefaultCollapsed = true }
          { Id = "samples"; Label = "Samples"; DefaultSide = Left
            DefaultOrder = 3; DefaultVisible = false; DefaultCollapsed = false }
          { Id = "selection"; Label = "Selection inspector"; DefaultSide = Right
            DefaultOrder = 0; DefaultVisible = true; DefaultCollapsed = false }
          { Id = "validation"; Label = "Validation"; DefaultSide = Right
            DefaultOrder = 1; DefaultVisible = true; DefaultCollapsed = true }
          { Id = "document"; Label = "Document / revision"; DefaultSide = Right
            DefaultOrder = 2; DefaultVisible = true; DefaultCollapsed = true }
          { Id = "rules"; Label = "Rules"; DefaultSide = Right
            DefaultOrder = 3; DefaultVisible = false; DefaultCollapsed = false }
          { Id = "data"; Label = "Data"; DefaultSide = Right
            DefaultOrder = 4; DefaultVisible = false; DefaultCollapsed = false }
          { Id = "diagnostics"; Label = "Diagnostics"; DefaultSide = Right
            DefaultOrder = 5; DefaultVisible = false; DefaultCollapsed = true } ]

    let private defaultPlacement panel =
        { PanelId = panel.Id
          Side = panel.DefaultSide
          Order = panel.DefaultOrder
          Visible = panel.DefaultVisible
          Collapsed = panel.DefaultCollapsed }

    let fieldFocus =
        { SchemaVersion = SchemaVersion
          Placements = panelRegistry |> List.map defaultPlacement
          LeftSidebar = { Width = 208; DrawerOpen = false }
          RightSidebar = { Width = 224; DrawerOpen = false }
          BottomPanel =
            { Visible = false
              Height = 152
              CollapsedInEditor = false
              CollapsedOutsideEditor = false } }

    let private normalize profile =
        let placements =
            [ for side in [ Left; Right ] do
                  let onSide =
                      profile.Placements
                      |> List.filter (fun placement -> placement.Side = side)
                      |> List.sortBy (fun placement -> placement.Order, placement.PanelId)
                  for order, placement in onSide |> List.indexed do
                      yield { placement with Order = order } ]
        { profile with Placements = placements }

    let panelsOn side profile =
        profile.Placements
        |> List.filter (fun placement -> placement.Side = side)
        |> List.sortBy (fun placement -> placement.Order, placement.PanelId)

    let bottomVisible profile = profile.BottomPanel.Visible

    let bottomCollapsed modality profile =
        if modality = Editor then profile.BottomPanel.CollapsedInEditor
        else profile.BottomPanel.CollapsedOutsideEditor

    let private updatePanel panelId change profile =
        { profile with
            Placements =
                profile.Placements
                |> List.map (fun placement ->
                    if placement.PanelId = panelId then change placement
                    else placement) }
        |> normalize

    let togglePanelVisibility panelId profile =
        updatePanel
            panelId
            (fun placement -> { placement with Visible = not placement.Visible })
            profile

    let togglePanelCollapsed panelId profile =
        updatePanel
            panelId
            (fun placement -> { placement with Collapsed = not placement.Collapsed })
            profile

    let movePanel panelId side profile =
        let nextOrder =
            panelsOn side profile
            |> List.map _.Order
            |> function
                | [] -> 0
                | orders -> List.max orders + 1
        updatePanel panelId (fun placement ->
            { placement with Side = side; Order = nextOrder }) profile

    let reorderPanel panelId delta profile =
        match profile.Placements |> List.tryFind (fun panel -> panel.PanelId = panelId) with
        | None -> profile
        | Some selected ->
            let ordered = panelsOn selected.Side profile
            let current = ordered |> List.findIndex (fun panel -> panel.PanelId = panelId)
            let target = max 0 (min (ordered.Length - 1) (current + delta))
            if target = current then profile
            else
                let other = ordered[target]
                { profile with
                    Placements =
                        profile.Placements
                        |> List.map (fun panel ->
                            if panel.PanelId = selected.PanelId then
                                { panel with Order = other.Order }
                            elif panel.PanelId = other.PanelId then
                                { panel with Order = selected.Order }
                            else panel) }
                |> normalize

    let toggleDrawer side profile =
        match side with
        | Left ->
            { profile with
                LeftSidebar =
                    { profile.LeftSidebar with
                        DrawerOpen = not profile.LeftSidebar.DrawerOpen } }
        | Right ->
            { profile with
                RightSidebar =
                    { profile.RightSidebar with
                        DrawerOpen = not profile.RightSidebar.DrawerOpen } }

    let toggleBottomPanelVisibility profile =
        { profile with
            BottomPanel =
                { profile.BottomPanel with
                    Visible = not profile.BottomPanel.Visible } }

    let toggleBottomPanel modality profile =
        let bottom =
            if modality = Editor then
                { profile.BottomPanel with
                    CollapsedInEditor = not profile.BottomPanel.CollapsedInEditor }
            else
                { profile.BottomPanel with
                    CollapsedOutsideEditor = not profile.BottomPanel.CollapsedOutsideEditor }
        { profile with BottomPanel = bottom }

    let resizeBottomPanel height profile =
        { profile with
            BottomPanel =
                { profile.BottomPanel with
                    Height = max 96 (min 480 height) } }

    let reset (_: TacticalLayoutProfile) = fieldFocus

    let private sideText = function Left -> "left" | Right -> "right"
    let private boolText value = if value then "true" else "false"
    let private placementJson placement =
        "{\"panelId\":\"" + placement.PanelId
        + "\",\"side\":\"" + sideText placement.Side
        + "\",\"order\":" + string placement.Order
        + ",\"visible\":" + boolText placement.Visible
        + ",\"collapsed\":" + boolText placement.Collapsed + "}"
    let private sidebarJson sidebar =
        "{\"width\":" + string sidebar.Width
        + ",\"drawerOpen\":" + boolText sidebar.DrawerOpen + "}"
    let private bottomJson bottom =
        "{\"visible\":" + boolText bottom.Visible
        + ",\"height\":" + string bottom.Height
        + ",\"collapsedInEditor\":" + boolText bottom.CollapsedInEditor
        + ",\"collapsedOutsideEditor\":" + boolText bottom.CollapsedOutsideEditor + "}"

    let exportProfile profile =
        let normalized = normalize profile
        "{\"schemaVersion\":1,\"placements\":["
        + (normalized.Placements
           |> List.sortBy (fun placement -> placement.Side, placement.Order, placement.PanelId)
           |> List.map placementJson
           |> String.concat ",")
        + "],\"leftSidebar\":" + sidebarJson normalized.LeftSidebar
        + ",\"rightSidebar\":" + sidebarJson normalized.RightSidebar
        + ",\"bottomPanel\":" + bottomJson normalized.BottomPanel + "}"

    type private Json =
        | Null
        | String of string
        | Number of int
        | Boolean of bool
        | Array of Json list
        | Object of Map<string, Json>

    type private ResultBuilder() =
        member _.Bind(value, binder) = Result.bind binder value
        member _.Return value = Ok value
        member _.ReturnFrom value = value

    let private result = ResultBuilder()

    let private parseJson (source: string) =
        let mutable index = 0
        let malformed detail = Error(MalformedLayoutProfile detail)
        let isAsciiDigit character =
            character >= '0' && character <= '9'
        let isJsonWhitespace = function
            | ' ' | '\t' | '\r' | '\n' -> true
            | _ -> false
        let skip () =
            while index < source.Length && isJsonWhitespace source[index] do
                index <- index + 1
        let rec value () =
            skip ()
            if index >= source.Length then malformed "Unexpected end of JSON."
            else
                match source[index] with
                | '"' -> stringValue () |> Result.map String
                | '{' -> objectValue ()
                | '[' -> arrayValue ()
                | 't' when source.Substring(index).StartsWith("true", StringComparison.Ordinal) ->
                    index <- index + 4; Ok(Boolean true)
                | 'f' when source.Substring(index).StartsWith("false", StringComparison.Ordinal) ->
                    index <- index + 5; Ok(Boolean false)
                | 'n' when source.Substring(index).StartsWith("null", StringComparison.Ordinal) ->
                    index <- index + 4; Ok Null
                | token when token = '-' || isAsciiDigit token ->
                    numberValue () |> Result.map Number
                | token -> malformed ("Unexpected JSON token " + string token + ".")
        and stringValue () =
            index <- index + 1
            let mutable parsed = ""
            let mutable done' = false
            let mutable error = None
            while index < source.Length && not done' && error.IsNone do
                match source[index] with
                | '"' -> done' <- true; index <- index + 1
                | '\\' ->
                    index <- index + 1
                    if index >= source.Length then error <- Some "Incomplete JSON escape."
                    else
                        match source[index] with
                        | '"' | '\\' | '/' as escaped ->
                            parsed <- parsed + string escaped
                            index <- index + 1
                        | _ -> error <- Some "Unsupported JSON escape."
                | character when Char.IsControl character ->
                    error <- Some "Control character in JSON string."
                | character ->
                    parsed <- parsed + string character
                    index <- index + 1
            match error with
            | Some detail -> malformed detail
            | None when not done' -> malformed "Unterminated JSON string."
            | None -> Ok parsed
        and numberValue () =
            let start = index
            if source[index] = '-' then index <- index + 1
            if index >= source.Length || not (isAsciiDigit source[index]) then
                malformed "JSON integer requires an ASCII digit after its optional minus."
            elif source[index] = '0' then
                index <- index + 1
                if index < source.Length && isAsciiDigit source[index] then
                    malformed "JSON integer cannot contain a leading zero."
                else
                    match Int32.TryParse(source.Substring(start, index - start)) with
                    | true, parsed -> Ok parsed
                    | _ -> malformed "JSON number must be a 32-bit integer."
            else
                while index < source.Length && isAsciiDigit source[index] do
                    index <- index + 1
                match Int32.TryParse(source.Substring(start, index - start)) with
                | true, parsed -> Ok parsed
                | _ -> malformed "JSON number must be a 32-bit integer."
        and arrayValue () =
            index <- index + 1
            skip ()
            let mutable items = []
            let mutable done' = false
            let mutable afterComma = false
            let mutable error = None
            while not done' && error.IsNone do
                skip ()
                if index < source.Length && source[index] = ']' then
                    if afterComma then
                        error <- Some(MalformedLayoutProfile "Trailing comma in JSON array.")
                    else
                        done' <- true
                        index <- index + 1
                else
                    match value () with
                    | Error detail -> error <- Some detail
                    | Ok item ->
                        afterComma <- false
                        items <- item :: items
                        skip ()
                        if index < source.Length && source[index] = ',' then
                            index <- index + 1
                            afterComma <- true
                        elif index < source.Length && source[index] = ']' then ()
                        else error <- Some(MalformedLayoutProfile "Expected ',' or ']'.")
            match error with Some detail -> Error detail | None -> Ok(Array(List.rev items))
        and objectValue () =
            index <- index + 1
            skip ()
            let mutable fields = Map.empty
            let mutable done' = false
            let mutable afterComma = false
            let mutable error = None
            while not done' && error.IsNone do
                skip ()
                if index < source.Length && source[index] = '}' then
                    if afterComma then
                        error <- Some(MalformedLayoutProfile "Trailing comma in JSON object.")
                    else
                        done' <- true
                        index <- index + 1
                elif index >= source.Length || source[index] <> '"' then
                    error <- Some(MalformedLayoutProfile "Expected JSON object field name.")
                else
                    match stringValue () with
                    | Error detail -> error <- Some detail
                    | Ok name ->
                        skip ()
                        if index >= source.Length || source[index] <> ':' then
                            error <- Some(MalformedLayoutProfile "Expected ':'.")
                        elif Map.containsKey name fields then
                            error <- Some(MalformedLayoutProfile("Duplicate field " + name + "."))
                        else
                            index <- index + 1
                            match value () with
                            | Error detail -> error <- Some detail
                            | Ok parsed ->
                                afterComma <- false
                                fields <- Map.add name parsed fields
                                skip ()
                                if index < source.Length && source[index] = ',' then
                                    index <- index + 1
                                    afterComma <- true
                                elif index < source.Length && source[index] = '}' then ()
                                else error <- Some(MalformedLayoutProfile "Expected ',' or '}'.")
            match error with Some detail -> Error detail | None -> Ok(Object fields)
        match value () with
        | Error diagnostic -> Error diagnostic
        | Ok parsed ->
            skip ()
            if index <> source.Length then malformed "Trailing JSON content."
            else Ok parsed

    let private exactFields expected fields =
        let actual = fields |> Map.toSeq |> Seq.map fst |> Set.ofSeq
        if actual = Set.ofList expected then Ok fields
        else Error(MalformedLayoutProfile "Missing or unknown layout field.")
    let private field name fields =
        match Map.tryFind name fields with
        | Some value -> Ok value
        | None -> Error(MalformedLayoutProfile("Missing field " + name + "."))
    let private asObject = function Object value -> Ok value | _ -> Error(MalformedLayoutProfile "Expected object.")
    let private asArray = function Array value -> Ok value | _ -> Error(MalformedLayoutProfile "Expected array.")
    let private asString = function String value -> Ok value | _ -> Error(MalformedLayoutProfile "Expected string.")
    let private asNumber = function Number value -> Ok value | _ -> Error(MalformedLayoutProfile "Expected number.")
    let private asBool = function Boolean value -> Ok value | _ -> Error(MalformedLayoutProfile "Expected boolean.")

    let private parseSidebar json =
        result {
            let! fields = asObject json
            let! fields = exactFields [ "width"; "drawerOpen" ] fields
            let! width = field "width" fields |> Result.bind asNumber
            let! drawer = field "drawerOpen" fields |> Result.bind asBool
            return { Width = width; DrawerOpen = drawer }
        }

    let private parseBottom json =
        result {
            let! fields = asObject json
            let! fields =
                exactFields
                    [ "visible"; "height"; "collapsedInEditor"; "collapsedOutsideEditor" ]
                    fields
            let! visible = field "visible" fields |> Result.bind asBool
            let! height = field "height" fields |> Result.bind asNumber
            let! editor = field "collapsedInEditor" fields |> Result.bind asBool
            let! outside = field "collapsedOutsideEditor" fields |> Result.bind asBool
            return
                { Visible = visible; Height = height
                  CollapsedInEditor = editor; CollapsedOutsideEditor = outside }
        }

    let private parsePlacement json =
        result {
            let! fields = asObject json
            let! fields =
                exactFields [ "panelId"; "side"; "order"; "visible"; "collapsed" ] fields
            let! panelId = field "panelId" fields |> Result.bind asString
            let! sideText = field "side" fields |> Result.bind asString
            let! side =
                match sideText with
                | "left" -> Ok Left
                | "right" -> Ok Right
                | _ -> Error(MalformedLayoutProfile "Panel side must be left or right.")
            let! order = field "order" fields |> Result.bind asNumber
            let! visible = field "visible" fields |> Result.bind asBool
            let! collapsed = field "collapsed" fields |> Result.bind asBool
            return
                { PanelId = panelId; Side = side; Order = order
                  Visible = visible; Collapsed = collapsed }
        }

    let private validate profile =
        let known = panelRegistry |> List.map _.Id |> Set.ofList
        let ids = profile.Placements |> List.map _.PanelId
        let duplicates =
            ids |> List.countBy id |> List.choose (fun (id, count) ->
                if count > 1 then Some(DuplicatePanel id) else None)
        let unknown =
            ids |> List.choose (fun id -> if Set.contains id known then None else Some(UnknownPanel id))
        let dimensions =
            [ "left width", profile.LeftSidebar.Width, 160, 480
              "right width", profile.RightSidebar.Width, 160, 480
              "bottom height", profile.BottomPanel.Height, 96, 480 ]
            |> List.choose (fun (name, value, minimum, maximum) ->
                if value < minimum || value > maximum then
                    Some(InvalidLayoutDimension(name, value))
                else None)
        let diagnostics = duplicates @ unknown @ dimensions
        if List.isEmpty diagnostics then
            let missing =
                panelRegistry
                |> List.filter (fun panel -> not (List.contains panel.Id ids))
                |> List.map defaultPlacement
            Ok(normalize { profile with Placements = profile.Placements @ missing })
        else Error diagnostics

    let private current fields =
        result {
            let! fields =
                exactFields
                    [ "schemaVersion"; "placements"; "leftSidebar"; "rightSidebar"; "bottomPanel" ]
                    fields
            let! placementsJson = field "placements" fields |> Result.bind asArray
            let! placements =
                placementsJson
                |> List.map parsePlacement
                |> List.fold (fun state item ->
                    result {
                        let! collected = state
                        let! parsed = item
                        return parsed :: collected
                    }) (Ok [])
                |> Result.map List.rev
            let! left = field "leftSidebar" fields |> Result.bind parseSidebar
            let! right = field "rightSidebar" fields |> Result.bind parseSidebar
            let! bottom = field "bottomPanel" fields |> Result.bind parseBottom
            return
                { SchemaVersion = SchemaVersion; Placements = placements
                  LeftSidebar = left; RightSidebar = right; BottomPanel = bottom }
        }

    let private migrateZero fields =
        result {
            let! fields =
                exactFields
                    [ "schemaVersion"; "panels"; "leftWidth"; "rightWidth"; "timelineHeight" ]
                    fields
            let! panels = field "panels" fields |> Result.bind asArray
            let! placements =
                panels
                |> List.map parsePlacement
                |> List.fold (fun state item ->
                    result {
                        let! collected = state
                        let! parsed = item
                        return parsed :: collected
                    }) (Ok [])
                |> Result.map List.rev
            let! leftWidth = field "leftWidth" fields |> Result.bind asNumber
            let! rightWidth = field "rightWidth" fields |> Result.bind asNumber
            let! height = field "timelineHeight" fields |> Result.bind asNumber
            return
                { fieldFocus with
                    Placements = placements
                    LeftSidebar = { fieldFocus.LeftSidebar with Width = leftWidth }
                    RightSidebar = { fieldFocus.RightSidebar with Width = rightWidth }
                    BottomPanel = { fieldFocus.BottomPanel with Height = height } }
        }

    let importProfile source =
        match parseJson source with
        | Error diagnostic -> Error [ diagnostic ]
        | Ok json ->
            match asObject json with
            | Error diagnostic -> Error [ diagnostic ]
            | Ok fields ->
                match field "schemaVersion" fields |> Result.bind asNumber with
                | Error diagnostic -> Error [ diagnostic ]
                | Ok version ->
                    let parsed =
                        match version with
                        | 0 -> migrateZero fields
                        | SchemaVersion -> current fields
                        | unsupported -> Error(UnsupportedLayoutSchema unsupported)
                    match parsed with
                    | Error diagnostic -> Error [ diagnostic ]
                    | Ok profile -> validate profile
