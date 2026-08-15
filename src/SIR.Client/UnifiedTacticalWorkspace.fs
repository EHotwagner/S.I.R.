namespace SIR.Client

open System

/// The four modalities that share the mounted tactical battlefield and time axis.
type TacticalModality =
    | Editor
    | Plan
    | Simulate
    | Review

/// Disclosure channels shown on the unified timeline.
type TacticalTimeChannel =
    | Authored
    | Predicted
    | Accepted
    | Committed

/// A non-authoritative timeline projection. Domain state remains owned by the
/// editor, planner, simulator, and replay models.
type TacticalTimelineSegment =
    { Id: string
      UnitId: int32 option
      StartTick: int64
      EndTick: int64
      Channel: TacticalTimeChannel
      Label: string
      Issue: string option }

type TacticalTimelineState =
    { Modality: TacticalModality
      Cursor: int64
      Horizon: int64
      CommittedThrough: int64
      IsPlaying: bool
      SelectedSegment: string option
      Segments: TacticalTimelineSegment list }

type TimelineEditError =
    | InvalidTimelineRange
    | CommittedInterval
    | DuplicateTimelineSegment of string
    | TimelineSegmentNotFound of string

type TacticalCommandAvailability =
    | AlwaysAvailable
    | TimelineEditable
    | TimelineSelectionRequired
    | PredictionRequired
    | CommittedHistoryRequired
    | HelpOpenRequired
    | PlanningAcceptedRequired
    | PlanningIssuesRequired
    | ReplayLoadedRequired
    | ReplayEventsRequired
    | ReplayOperationRequired

/// Inspectable command metadata shared by dispatch, menus, help, conflict
/// validation, and binding configuration.
type TacticalCommandDefinition =
    { Id: string
      Label: string
      Category: string
      Modalities: Set<TacticalModality>
      DefaultGesture: string option
      PointerAvailable: bool
      Precedence: int
      ModalContext: ModalContextSelector option
      ModalPhase: InputPhase option
      Availability: TacticalCommandAvailability }

type TacticalBindingOverride =
    { CommandId: string
      Gesture: string option }

type TacticalBindingProfile =
    { SchemaVersion: int
      Overrides: Map<string, string option> }

type TacticalBindingDiagnostic =
    | UnknownTacticalCommand of string
    | ReservedTacticalGesture of commandId: string * gesture: string
    | TacticalBindingConflict of
        firstCommandId: string *
        secondCommandId: string *
        gesture: string
    | MalformedTacticalBindingProfile of string
    | UnsupportedTacticalBindingSchema of int

type ShortcutPlatform =
    | ControlPlatform
    | MetaPlatform

type DocumentationStatus =
    | CanonicalDocumentation
    | ImplementedDocumentation
    | ProvisionalDocumentation
    | ResearchDocumentation

type DocumentationSource =
    { Repository: string
      Revision: string
      Path: string
      PageSlug: string
      Concept: string
      Symbol: string option
      Line: int option }

type DocumentationBlock =
    { Kind: string
      Level: int option
      Anchor: string option
      Text: string }

type DocumentationPage =
    { Slug: string
      Title: string
      Category: string
      Status: DocumentationStatus
      SourcePath: string
      ContentDigest: string
      Headings: (string * string) list
      Related: string list
      Blocks: DocumentationBlock list }

type DocumentationManifest =
    { Schema: string
      DefinitionDigest: string
      Pages: DocumentationPage list
      Sources: Map<string, DocumentationSource>
      SearchTokenCount: int }

type DocumentationNavigation =
    { Page: string option
      Anchor: string option
      Query: string
      Back: (string * string option) list
      Forward: (string * string option) list }

[<RequireQualifiedAccess>]
module UnifiedTacticalWorkspace =
    [<Literal>]
    let BindingSchemaVersion = 1

    let emptyBindingProfile =
        { SchemaVersion = BindingSchemaVersion
          Overrides = Map.empty }

    let commandRegistry =
        let all =
            Set.ofList [ Editor; Plan; Simulate; Review ]
        let command id label category modalities gesture pointer precedence availability =
            { Id = id
              Label = label
              Category = category
              Modalities = modalities
              DefaultGesture = gesture
              PointerAvailable = pointer
              Precedence = precedence
              ModalContext = None
              ModalPhase = None
              Availability = availability }
        [ command "workspace.editor" "Switch to Editor" "Modality" all (Some "Ctrl+Shift+1") true 10 AlwaysAvailable
          command "workspace.plan" "Switch to Plan" "Modality" all (Some "Ctrl+Shift+2") true 10 AlwaysAvailable
          command "workspace.simulate" "Switch to Simulate" "Modality" all (Some "Ctrl+Shift+3") true 10 AlwaysAvailable
          command "workspace.review" "Switch to Review" "Modality" all (Some "Ctrl+Shift+4") true 10 AlwaysAvailable
          command "workspace.docs" "Open documentation" "Modality" all (Some "Ctrl+Shift+5") true 10 AlwaysAvailable
          command "docs.back" "Documentation back" "Documentation" all (Some "Alt+ArrowLeft") true 15 AlwaysAvailable
          command "docs.forward" "Documentation forward" "Documentation" all (Some "Alt+ArrowRight") true 15 AlwaysAvailable
          command "docs.home" "Documentation home" "Documentation" all None true 15 AlwaysAvailable
          command "docs.search" "Search documentation" "Documentation" all (Some "Ctrl+K") true 15 AlwaysAvailable
          command "timeline.play-toggle" "Play or pause" "Timeline" all (Some "Space") true 20 AlwaysAvailable
          command "timeline.step-back" "Step backward" "Timeline" all (Some "Ctrl+ArrowLeft") true 20 AlwaysAvailable
          command "timeline.step-forward" "Step forward" "Timeline" all (Some "Ctrl+ArrowRight") true 20 AlwaysAvailable
          command "timeline.home" "Go to timeline start" "Timeline" all (Some "Ctrl+Home") true 20 AlwaysAvailable
          command "timeline.end" "Go to timeline end" "Timeline" all (Some "Ctrl+End") true 20 AlwaysAvailable
          command "timeline.move-command" "Move selected command to current time" "Timeline" (Set.singleton Plan) None true 30 TimelineSelectionRequired
          command "timeline.remove-command" "Remove selected command" "Timeline" (Set.singleton Plan) (Some "Delete") true 30 TimelineSelectionRequired
          command "planning.undo" "Undo plan edit" "Plan" (Set.singleton Plan) (Some "Ctrl+Z") true 30 TimelineEditable
          command "planning.redo" "Redo plan edit" "Plan" (Set.singleton Plan) (Some "Ctrl+Shift+Z") true 30 TimelineEditable
          command "planning.route" "Choose route tool" "Plan" (Set.singleton Plan) (Some "R") true 30 TimelineEditable
          command "planning.facing" "Choose facing tool" "Plan" (Set.singleton Plan) (Some "F") true 30 TimelineEditable
          command "planning.attention" "Choose attention tool" "Plan" (Set.singleton Plan) (Some "A") true 30 TimelineEditable
          command "planning.stance" "Choose stance tool" "Plan" (Set.singleton Plan) (Some "S") true 30 TimelineEditable
          command "planning.hold" "Choose hold tool" "Plan" (Set.singleton Plan) (Some "H") true 30 TimelineEditable
          command "planning.engagement" "Choose engagement tool" "Plan" (Set.singleton Plan) (Some "E") true 30 TimelineEditable
          command "planning.synchronization" "Choose synchronization tool" "Plan" (Set.singleton Plan) (Some "M") true 30 TimelineEditable
          command "planning.validate" "Validate authored revision" "Plan" (Set.singleton Plan) None true 40 TimelineEditable
          command "planning.preview" "Preview intent-only prediction" "Plan" (Set.singleton Plan) None true 40 TimelineEditable
          command "planning.commit" "Commit accepted revision" "Plan" (Set.singleton Plan) None true 40 PlanningAcceptedRequired
          command "planning.issue.previous" "Previous validation issue" "Plan" (Set.singleton Plan) (Some "[") true 40 PlanningIssuesRequired
          command "planning.issue.next" "Next validation issue" "Plan" (Set.singleton Plan) (Some "]") true 40 PlanningIssuesRequired
          command "review.previous-event" "Previous disclosed event" "Review" (Set.singleton Review) (Some "[") true 40 ReplayEventsRequired
          command "review.next-event" "Next disclosed event" "Review" (Set.singleton Review) (Some "]") true 40 ReplayEventsRequired
          command "review.cancel" "Cancel replay operation" "Review" (Set.singleton Review) (Some "Escape") true 50 ReplayOperationRequired
          command "input.help" "Show contextual actions" "Help" all (Some "?") true 100 AlwaysAvailable
          command "input.help.close" "Close contextual actions" "Help" all (Some "Escape") true 110 HelpOpenRequired
          command "input.bindings" "Configure command bindings" "Help" all None true 100 AlwaysAvailable ]

    let effectiveGesture profile command =
        match Map.tryFind command.Id profile.Overrides with
        | Some gesture -> gesture
        | None -> command.DefaultGesture

    let isRebound profile command =
        Map.containsKey command.Id profile.Overrides

    /// Formats the registry's effective binding for visible command presentation.
    let displayGesture (gesture: string option) = gesture |> Option.defaultValue "Unassigned"

    /// Formats the portable registry gesture for the platform that will activate it.
    let displayGestureFor platform gesture =
        gesture
        |> displayGesture
        |> fun value ->
            match platform with
            | ControlPlatform -> value.Replace("Ctrl/Cmd", "Ctrl")
            | MetaPlatform -> value.Replace("Ctrl/Cmd", "Cmd").Replace("Ctrl", "Cmd")

    /// Converts a registry gesture to the token form expected by aria-keyshortcuts.
    let accessibleGesture (gesture: string option) =
        gesture
        |> Option.map (fun (value: string) ->
            value
                .Replace("Ctrl/Cmd", "Control")
                .Replace("Ctrl", "Control")
                .Replace("Cmd", "Meta")
                .Replace("Esc", "Escape")
                .Replace("←", "ArrowLeft")
                .Replace("→", "ArrowRight")
                .Replace("↑", "ArrowUp")
                .Replace("↓", "ArrowDown"))

    /// Formats ARIA shortcut tokens for the same platform-specific presentation.
    let accessibleGestureFor platform gesture =
        gesture
        |> Option.map (fun (value: string) ->
            match platform with
            | ControlPlatform -> value.Replace("Ctrl/Cmd", "Control").Replace("Ctrl", "Control")
            | MetaPlatform -> value.Replace("Ctrl/Cmd", "Meta").Replace("Ctrl", "Meta"))
        |> accessibleGesture

    let private normalizedGesture (gesture: string) =
        gesture.Trim().ToUpperInvariant()

    let gestureText (gesture: InputGesture) =
        let key =
            match NormalizedKey.value gesture.Key with
            | "Space" -> "Space"
            | value when value.Length = 1 -> value.ToUpperInvariant()
            | value -> value
        [ if gesture.Modifiers.ControlOrMeta then "Ctrl"
          if gesture.Modifiers.Alt then "Alt"
          if gesture.Modifiers.Shift && NormalizedKey.value gesture.Key <> "?" then "Shift"
          key ]
        |> String.concat "+"

    let tryParseGesture (text: string) =
        let parts =
            text.Split('+', StringSplitOptions.RemoveEmptyEntries)
            |> Array.map _.Trim()
            |> Array.filter (String.IsNullOrWhiteSpace >> not)
        if Array.isEmpty parts then None
        else
            let key = parts[parts.Length - 1]
            let modifiers =
                parts
                |> Array.take (parts.Length - 1)
                |> Array.map _.ToUpperInvariant()
                |> Set.ofArray
            let known =
                modifiers
                |> Set.forall (fun value ->
                    value = "CTRL" || value = "CONTROL" || value = "CMD"
                    || value = "META" || value = "ALT" || value = "SHIFT")
            if not known || String.IsNullOrWhiteSpace key then None
            else
                Some
                    { Key = NormalizedKey.create key None
                      Modifiers =
                        { ControlOrMeta =
                            Set.contains "CTRL" modifiers
                            || Set.contains "CONTROL" modifiers
                            || Set.contains "CMD" modifiers
                            || Set.contains "META" modifiers
                          Alt = Set.contains "ALT" modifiers
                          Shift = Set.contains "SHIFT" modifiers }
                      Phase = KeyDown }

    let adaptModalCatalog profile (catalog: ModalBinding<ModalCommand> list) =
        catalog
        |> List.choose (fun binding ->
            match Map.tryFind binding.Id profile.Overrides with
            | Some None -> None
            | Some(Some text) ->
                tryParseGesture text
                |> Option.map (fun gesture ->
                    { binding with
                        BindingGesture =
                            { gesture with
                                Phase = binding.BindingGesture.Phase } })
            | None -> Some binding)

    let modalCommandDefinitions modality (catalog: ModalBinding<ModalCommand> list) =
        catalog
        |> List.distinctBy _.Id
        |> List.map (fun binding ->
            { Id = binding.Id
              Label = binding.Label
              Category = binding.Group
              Modalities = Set.singleton modality
              DefaultGesture = Some(gestureText binding.BindingGesture)
              PointerAvailable = true
              Precedence = ModalInput.precedenceRank binding.Precedence
              ModalContext = Some binding.Context
              ModalPhase = Some binding.BindingGesture.Phase
              Availability = AlwaysAvailable })

    let private reservedGesture gesture =
        match normalizedGesture gesture with
        | "CTRL+L"
        | "CTRL+T"
        | "CTRL+W"
        | "CTRL+R"
        | "CTRL+SHIFT+R"
        | "ALT+F4"
        | "F5" -> true
        | "F6"
        | "ALT+ARROWLEFT"
        | "ALT+ARROWRIGHT"
        | "ALT+HOME"
        | "ALT+END"
        | "CTRL+N"
        | "CTRL+P"
        | "CTRL+S"
        | "CTRL+O" -> true
        | _ -> false

    let validateBindings registry profile =
        let known = registry |> List.map _.Id |> Set.ofList
        [ for KeyValue(id, gesture) in profile.Overrides do
              if
                  not (Set.contains id known)
                  && not (ModalInput.isKnownCommandId id)
              then
                  UnknownTacticalCommand id
              match gesture with
              | Some value when String.IsNullOrWhiteSpace value ->
                  MalformedTacticalBindingProfile("Empty gesture for " + id + ".")
              | Some value when reservedGesture value ->
                  ReservedTacticalGesture(id, value)
              | _ -> ()

          let effective =
              registry
              |> List.choose (fun command ->
                  effectiveGesture profile command
                  |> Option.map (fun gesture -> command, normalizedGesture gesture))
          for firstIndex in 0 .. effective.Length - 1 do
              for secondIndex in firstIndex + 1 .. effective.Length - 1 do
                  let first, firstGesture = effective[firstIndex]
                  let second, secondGesture = effective[secondIndex]
                  if
                      firstGesture = secondGesture
                      && not (Set.intersect first.Modalities second.Modalities |> Set.isEmpty)
                      && first.Precedence = second.Precedence
                      && (match first.ModalContext, second.ModalContext with
                          | Some left, Some right ->
                              ModalInput.selectorsOverlap left right
                          | _ -> true)
                      && (match first.ModalPhase, second.ModalPhase with
                          | Some left, Some right -> left = right
                          | _ -> true)
                  then
                      TacticalBindingConflict(first.Id, second.Id, firstGesture) ]

    let setBinding registry commandId gesture replaceConflict profile =
        let known = registry |> List.exists (fun command -> command.Id = commandId)
        if not known then Error [ UnknownTacticalCommand commandId ]
        else
            let candidate =
                { profile with
                    Overrides = Map.add commandId gesture profile.Overrides }
            let diagnostics = validateBindings registry candidate
            let blocking =
                diagnostics
                |> List.filter (function
                    | TacticalBindingConflict _ when replaceConflict -> false
                    | _ -> true)
            if not (List.isEmpty blocking) then Error diagnostics
            elif replaceConflict then
                let target =
                    registry |> List.find (fun command -> command.Id = commandId)
                let targetGesture =
                    effectiveGesture candidate target
                    |> Option.map normalizedGesture
                let conflictingIds =
                    registry
                    |> List.choose (fun command ->
                        if command.Id = commandId then None
                        else
                            match targetGesture, effectiveGesture candidate command with
                            | Some left, Some right
                                when left = normalizedGesture right
                                     && not (Set.intersect target.Modalities command.Modalities |> Set.isEmpty)
                                     && target.Precedence = command.Precedence
                                     && (match target.ModalContext, command.ModalContext with
                                         | Some left, Some right ->
                                             ModalInput.selectorsOverlap left right
                                         | _ -> true)
                                     && (match target.ModalPhase, command.ModalPhase with
                                         | Some left, Some right -> left = right
                                         | _ -> true) ->
                                Some command.Id
                            | _ -> None)
                Ok
                    { candidate with
                        Overrides =
                            conflictingIds
                            |> List.fold (fun overrides id ->
                                Map.add id None overrides) candidate.Overrides }
            else Ok candidate

    let restoreCommand commandId profile =
        { profile with Overrides = Map.remove commandId profile.Overrides }

    let restoreModality registry modality profile =
        let identifiers =
            registry
            |> List.filter (fun command -> Set.contains modality command.Modalities)
            |> List.map _.Id
        { profile with
            Overrides =
                identifiers
                |> List.fold (fun overrides id -> Map.remove id overrides) profile.Overrides }

    let restoreAll _ = emptyBindingProfile

    let private escapeJson (value: string) =
        value.Replace("\\", "\\\\").Replace("\"", "\\\"")

    let exportBindings profile =
        let bindings =
            profile.Overrides
            |> Map.toList
            |> List.map (fun (id, gesture) ->
                "{\"id\":\""
                + escapeJson id
                + "\",\"gesture\":"
                + (gesture
                   |> Option.map (fun value -> "\"" + escapeJson value + "\"")
                   |> Option.defaultValue "null")
                + "}")
            |> String.concat ","
        "{\"schemaVersion\":"
        + string BindingSchemaVersion
        + ",\"bindings\":["
        + bindings
        + "]}"

    type private StrictJson =
        | JsonNull
        | JsonString of string
        | JsonNumber of int
        | JsonArray of StrictJson list
        | JsonObject of Map<string, StrictJson>

    let private parseStrictJson (source: string) =
        let mutable index = 0
        let malformed detail = Error(MalformedTacticalBindingProfile detail)
        let skipWhitespace () =
            while index < source.Length && Char.IsWhiteSpace source[index] do
                index <- index + 1
        let rec parseValue () =
            skipWhitespace ()
            if index >= source.Length then malformed "Unexpected end of JSON."
            else
                match source[index] with
                | '"' -> parseString () |> Result.map JsonString
                | '{' -> parseObject ()
                | '[' -> parseArray ()
                | 'n' when source.Substring(index).StartsWith("null", StringComparison.Ordinal) ->
                    index <- index + 4
                    Ok JsonNull
                | value when value = '-' || Char.IsDigit value ->
                    parseNumber () |> Result.map JsonNumber
                | value -> malformed ("Unexpected JSON token " + string value + ".")
        and parseString () =
            index <- index + 1
            // Keep the parser in the .NET/Fable shared subset. The retained
            // Fable test runtime does not expose System.Text.StringBuilder's
            // generated helper surface.
            let mutable parsed = ""
            let mutable finished = false
            let mutable error = None
            while index < source.Length && not finished && error.IsNone do
                match source[index] with
                | '"' ->
                    finished <- true
                    index <- index + 1
                | '\\' ->
                    index <- index + 1
                    if index >= source.Length then error <- Some "Incomplete JSON escape."
                    else
                        let escaped =
                            match source[index] with
                            | '"' -> Some '"'
                            | '\\' -> Some '\\'
                            | '/' -> Some '/'
                            | 'b' -> Some '\b'
                            | 'f' -> Some '\f'
                            | 'n' -> Some '\n'
                            | 'r' -> Some '\r'
                            | 't' -> Some '\t'
                            | _ -> None
                        match escaped with
                        | Some value ->
                            parsed <- parsed + string value
                            index <- index + 1
                        | None -> error <- Some "Unsupported or malformed JSON escape."
                | value when Char.IsControl value ->
                    error <- Some "Control character in JSON string."
                | value ->
                    parsed <- parsed + string value
                    index <- index + 1
            match error with
            | Some detail -> malformed detail
            | None when not finished -> malformed "Unterminated JSON string."
            | None -> Ok parsed
        and parseNumber () =
            let start = index
            if source[index] = '-' then index <- index + 1
            while index < source.Length && Char.IsDigit source[index] do
                index <- index + 1
            match Int32.TryParse(source.Substring(start, index - start)) with
            | true, value -> Ok value
            | _ -> malformed "JSON number must be a 32-bit integer."
        and parseArray () =
            index <- index + 1
            skipWhitespace ()
            let mutable values = []
            let mutable done' = false
            let mutable error = None
            if index < source.Length && source[index] = ']' then
                index <- index + 1
                done' <- true
            while not done' && error.IsNone do
                match parseValue () with
                | Error detail -> error <- Some detail
                | Ok value ->
                    values <- value :: values
                    skipWhitespace ()
                    if index < source.Length && source[index] = ',' then index <- index + 1
                    elif index < source.Length && source[index] = ']' then
                        index <- index + 1
                        done' <- true
                    else error <- Some(MalformedTacticalBindingProfile "Expected ',' or ']' in JSON array.")
            match error with
            | Some detail -> Error detail
            | None -> Ok(JsonArray(List.rev values))
        and parseObject () =
            index <- index + 1
            skipWhitespace ()
            let mutable fields = Map.empty
            let mutable done' = false
            let mutable error = None
            if index < source.Length && source[index] = '}' then
                index <- index + 1
                done' <- true
            while not done' && error.IsNone do
                match parseString () with
                | Error detail -> error <- Some detail
                | Ok name when Map.containsKey name fields ->
                    error <- Some(MalformedTacticalBindingProfile("Duplicate JSON field " + name + "."))
                | Ok name ->
                    skipWhitespace ()
                    if index >= source.Length || source[index] <> ':' then
                        error <- Some(MalformedTacticalBindingProfile "Expected ':' in JSON object.")
                    else
                        index <- index + 1
                        match parseValue () with
                        | Error detail -> error <- Some detail
                        | Ok value ->
                            fields <- Map.add name value fields
                            skipWhitespace ()
                            if index < source.Length && source[index] = ',' then
                                index <- index + 1
                                skipWhitespace ()
                            elif index < source.Length && source[index] = '}' then
                                index <- index + 1
                                done' <- true
                            else error <- Some(MalformedTacticalBindingProfile "Expected ',' or '}' in JSON object.")
            match error with
            | Some detail -> Error detail
            | None -> Ok(JsonObject fields)
        match parseValue () with
        | Error detail -> Error detail
        | Ok value ->
            skipWhitespace ()
            if index = source.Length then Ok value
            else malformed "Trailing content after JSON value."

    let importBindings registry (json: string) =
        let invalid detail = Error [ MalformedTacticalBindingProfile detail ]
        match parseStrictJson json with
        | Error diagnostic -> Error [ diagnostic ]
        | Ok(JsonObject root) ->
            let version =
                match Map.tryFind "schemaVersion" root with
                | Some(JsonNumber value) -> Some value
                | _ -> None
            match version with
            | None -> invalid "schemaVersion must be an integer."
            | Some value when value <> 0 && value <> BindingSchemaVersion ->
                Error [ UnsupportedTacticalBindingSchema value ]
            | Some value ->
                let bindingsField = if value = 0 then "overrides" else "bindings"
                let allowed = Set.ofList [ "schemaVersion"; bindingsField ]
                if root |> Map.exists (fun name _ -> not (Set.contains name allowed)) then
                    invalid "Unknown root binding-profile field."
                else
                    match Map.tryFind bindingsField root with
                    | Some(JsonArray entries) ->
                        let parseEntry = function
                            | JsonObject fields
                                when fields.Count = 2
                                     && Map.containsKey "id" fields
                                     && Map.containsKey "gesture" fields ->
                                match fields["id"], fields["gesture"] with
                                | JsonString id, JsonString gesture -> Ok(id, Some gesture)
                                | JsonString id, JsonNull -> Ok(id, None)
                                | _ -> Error "Binding id must be a string and gesture must be a string or null."
                            | _ -> Error "Each binding must contain exactly id and gesture."
                        let parsed = entries |> List.map parseEntry
                        match parsed |> List.tryPick (function Error detail -> Some detail | _ -> None) with
                        | Some detail -> invalid detail
                        | None ->
                            let values = parsed |> List.choose (function Ok item -> Some item | _ -> None)
                            let ids = values |> List.map fst
                            if Set.count (Set.ofList ids) <> ids.Length then
                                invalid "Duplicate binding command ID."
                            else
                                let profile =
                                    { SchemaVersion = BindingSchemaVersion
                                      Overrides = Map.ofList values }
                                let diagnostics = validateBindings registry profile
                                if List.isEmpty diagnostics then Ok profile else Error diagnostics
                    | _ -> invalid (bindingsField + " must be an array.")
        | Ok _ -> invalid "Binding profile root must be an object."

    let private clamp minimum maximum value =
        max minimum (min maximum value)

    let initial horizon =
        let horizon = max 0L horizon
        { Modality = Editor
          Cursor = 0L
          Horizon = horizon
          CommittedThrough = -1L
          IsPlaying = false
          SelectedSegment = None
          Segments = [] }

    let switchModality modality (state: TacticalTimelineState) =
        { state with Modality = modality }

    [<Literal>]
    let DocumentationHistoryLimit = 128

    let initialDocumentationNavigation =
        { Page = None
          Anchor = None
          Query = ""
          Back = []
          Forward = [] }

    let private normalizeDocumentationText (value: string) =
        value.Trim().ToLowerInvariant()

    let documentationSearch (query: string) (manifest: DocumentationManifest) =
        let terms =
            (normalizeDocumentationText query)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            |> Array.distinct
        manifest.Pages
        |> List.choose (fun page ->
            let haystack =
                [ page.Title
                  page.Category
                  page.SourcePath
                  yield! page.Headings |> List.collect (fun (title, anchor) -> [ title; anchor ])
                  yield! page.Blocks |> List.map _.Text ]
                |> String.concat " "
                |> normalizeDocumentationText
            if Array.forall (fun (term: string) -> haystack.Contains term) terms then
                let score =
                    terms
                    |> Array.sumBy (fun (term: string) ->
                        (if (normalizeDocumentationText page.Title).Contains(term) then 4 else 0)
                        + (if page.Headings |> List.exists (fun (title, _) -> (normalizeDocumentationText title).Contains(term)) then 2 else 0)
                        + (if haystack.Contains term then 1 else 0))
                Some(score, page)
            else None)
        |> List.sortBy (fun (score, page) -> -score, page.Title, page.Slug)
        |> List.truncate 200
        |> List.map snd

    let openDocumentationPage slug anchor state =
        let previous = state.Page |> Option.map (fun page -> page, state.Anchor)
        let destination = Some(slug, anchor)
        { state with
            Page = Some slug
            Anchor = anchor
            Back =
                match previous with
                | Some item when previous <> destination ->
                    item :: state.Back |> List.truncate DocumentationHistoryLimit
                | _ -> state.Back
            Forward = [] }

    let documentationBack state =
        match state.Back with
        | [] -> state
        | (page, anchor) :: rest ->
            { state with
                Page = Some page
                Anchor = anchor
                Back = rest
                Forward =
                    match state.Page with
                    | Some current -> (current, state.Anchor) :: state.Forward |> List.truncate DocumentationHistoryLimit
                    | None -> state.Forward }

    let documentationForward state =
        match state.Forward with
        | [] -> state
        | (page, anchor) :: rest ->
            { state with
                Page = Some page
                Anchor = anchor
                Back =
                    match state.Page with
                    | Some current -> (current, state.Anchor) :: state.Back |> List.truncate DocumentationHistoryLimit
                    | None -> state.Back
                Forward = rest }

    let setDocumentationQuery query state = { state with Query = query }

    let tryContextualDocumentation disclosedConcept manifest =
        disclosedConcept
        |> Option.bind (fun concept ->
            let key = normalizeDocumentationText concept
            if String.IsNullOrWhiteSpace key then None else Map.tryFind key manifest.Sources)

    /// Scrubbing changes projection only. Segments and the committed boundary
    /// are deliberately copied unchanged.
    let scrub tick (state: TacticalTimelineState) =
        { state with
            Cursor = clamp 0L state.Horizon tick
            IsPlaying = false }

    let step delta state =
        scrub (state.Cursor + delta) state

    let home state = scrub 0L state
    let finish state = scrub state.Horizon state

    let setPlaying playing (state: TacticalTimelineState) =
        { state with IsPlaying = playing && state.Cursor < state.Horizon }

    let pulse (state: TacticalTimelineState) =
        if not state.IsPlaying then state
        elif state.Cursor >= state.Horizon then { state with IsPlaying = false }
        else
            let next = min state.Horizon (state.Cursor + 1L)
            { state with
                Cursor = next
                IsPlaying = next < state.Horizon }

    let nextEditableBoundary (state: TacticalTimelineState) =
        max 0L (state.CommittedThrough + 1L)

    let canEditAt tick state =
        tick >= nextEditableBoundary state && tick <= state.Horizon

    let private validSegment (state: TacticalTimelineState) segment =
        segment.StartTick >= 0L
        && segment.EndTick >= segment.StartTick
        && segment.EndTick <= state.Horizon

    let addSegment segment state =
        if not (validSegment state segment) then Error InvalidTimelineRange
        elif segment.StartTick <= state.CommittedThrough then Error CommittedInterval
        elif state.Segments |> List.exists (fun current -> current.Id = segment.Id) then
            Error(DuplicateTimelineSegment segment.Id)
        else
            Ok
                { state with
                    Segments =
                        segment :: state.Segments
                        |> List.sortBy (fun current ->
                            current.StartTick,
                            current.EndTick,
                            current.UnitId,
                            current.Id)
                    SelectedSegment = Some segment.Id }

    let moveSegment id startTick state =
        match state.Segments |> List.tryFind (fun segment -> segment.Id = id) with
        | None -> Error(TimelineSegmentNotFound id)
        | Some current ->
            let moved =
                { current with
                    StartTick = startTick
                    EndTick = startTick + (current.EndTick - current.StartTick) }
            if not (validSegment state moved) then Error InvalidTimelineRange
            elif moved.StartTick <= state.CommittedThrough then Error CommittedInterval
            else
                Ok
                    { state with
                        Segments =
                            state.Segments
                            |> List.map (fun segment ->
                                if segment.Id = id then moved else segment)
                            |> List.sortBy (fun segment ->
                                segment.StartTick,
                                segment.EndTick,
                                segment.UnitId,
                                segment.Id) }

    let removeSegment id state =
        match state.Segments |> List.tryFind (fun segment -> segment.Id = id) with
        | None -> Error(TimelineSegmentNotFound id)
        | Some segment when segment.StartTick <= state.CommittedThrough ->
            Error CommittedInterval
        | Some _ ->
            Ok
                { state with
                    Segments =
                        state.Segments
                        |> List.filter (fun segment -> segment.Id <> id)
                    SelectedSegment =
                        if state.SelectedSegment = Some id then None
                        else state.SelectedSegment }

    let acceptThrough tick state =
        let boundary = clamp state.CommittedThrough state.Horizon tick
        let accepted =
            state.Segments
            |> List.choose (fun segment ->
                let acceptedId = "accepted:" + segment.Id
                if
                    segment.Channel = Authored
                    && segment.EndTick <= boundary
                    && not (state.Segments |> List.exists (fun current -> current.Id = acceptedId))
                then
                    Some
                        { segment with
                            Id = acceptedId
                            Channel = Accepted
                            Label = "Accepted · " + segment.Label }
                else None)
        { state with
            Segments = state.Segments @ accepted }

    let commitThrough tick state =
        let boundary = clamp state.CommittedThrough state.Horizon tick
        let authored =
            state.Segments
            |> List.filter (fun segment ->
                segment.Channel = Authored && segment.EndTick <= boundary)
        let committed =
            authored
            |> List.choose (fun segment ->
                let committedId = "committed:" + segment.Id
                if state.Segments |> List.exists (fun current -> current.Id = committedId) then
                    None
                else
                    Some
                        { segment with
                            Id = committedId
                            Channel = Committed
                            Label = "Committed · " + segment.Label })
        { state with
            CommittedThrough = boundary
            Cursor = max state.Cursor boundary
            Segments = state.Segments @ committed }

    let authoritativeProgressBoundary response currentTick =
        match response with
        | PlanCommitted _
        | SimulatorStepped _
        | SimulatorProgress _
        | SimulatorRunCompleted _
        | SimulatorReset _
        | AuthoritativeRunLoaded _ -> Some(int64 currentTick)
        | _ -> None

    let projectAt tick state =
        let cursor = clamp 0L state.Horizon tick
        state.Segments
        |> List.filter (fun segment ->
            segment.StartTick <= cursor && cursor <= segment.EndTick)

    let validate state =
        [ if state.Horizon < 0L then "Timeline horizon cannot be negative."
          if state.Cursor < 0L || state.Cursor > state.Horizon then
              "Timeline cursor is outside the bounded horizon."
          if state.CommittedThrough >= state.Horizon && state.Horizon >= 0L then
              "Committed boundary leaves no editable interval."
          for segment in state.Segments do
              if not (validSegment state segment) then
                  "Invalid timeline segment " + segment.Id + "." ]
