namespace SIR.Client

open System
open SIR.Domain

/// The reason a presentation field has no disclosed value.
type Disclosure<'value> =
    | NotPresent
    | NotApplicable
    | ExplicitlyUnknown
    | Disclosed of 'value

/// A stable identifier from the built-in unit glyph catalog.
type UnitClassId = private UnitClassId of string

[<RequireQualifiedAccess>]
module UnitClassId =
    let value (UnitClassId value) = value

    let private known =
        Set.ofList
            [ "rifleman"
              "gunner"
              "marksman"
              "engineer"
              "medic"
              "signaller"
              "observation-drone"
              "relay-drone"
              "goblin"
              "orc"
              "troll"
              "senior-caster"
              "magical-assistant"
              "ambient-critter" ]

    let placeholder = UnitClassId "unknown-unit"

    /// Resolves untrusted class text to a built-in identifier.
    let resolve value =
        if Set.contains value known then
            UnitClassId value
        else
            placeholder

/// A normalized absolute heading in radians.
type HeadingRadians = private HeadingRadians of float

[<RequireQualifiedAccess>]
module HeadingRadians =
    let private fullTurn = Math.PI * 2.0

    let tryCreate value =
        if Double.IsNaN value || Double.IsInfinity value then
            None
        else
            let normalized = ((value % fullTurn) + fullTurn) % fullTurn
            Some(HeadingRadians normalized)

    let value (HeadingRadians value) = value

    let ofDirection8 direction =
        let radians =
            match direction with
            | North -> -Math.PI / 2.0
            | NorthEast -> -Math.PI / 4.0
            | East -> 0.0
            | SouthEast -> Math.PI / 4.0
            | South -> Math.PI / 2.0
            | SouthWest -> Math.PI * 3.0 / 4.0
            | West -> Math.PI
            | NorthWest -> Math.PI * 5.0 / 4.0

        tryCreate radians
        |> Option.defaultWith (fun () -> failwith "Canonical direction produced an invalid heading.")

/// A positive cell extent used by an authoritative footprint.
type CellExtent = private CellExtent of int32

[<RequireQualifiedAccess>]
module CellExtent =
    let tryCreate value =
        if value > 0 then Some(CellExtent value) else None

    let value (CellExtent value) = value

type FactionVisual =
    | Human
    | Arcane
    | Neutral
    | OtherFaction of stableId: string

type HealthVisual =
    private
        { Remaining: int32
          Maximum: int32 }

[<RequireQualifiedAccess>]
module HealthVisual =
    let tryCreate remaining maximum =
        if maximum > 0 && remaining >= 0 && remaining <= maximum then
            Some
                { Remaining = remaining
                  Maximum = maximum }
        else
            None

    let remaining health = health.Remaining
    let maximum health = health.Maximum

type UnitVisual =
    { Id: int32
      AnchorColumn: int32
      AnchorRow: int32
      FootprintWidth: CellExtent
      FootprintDepth: CellExtent
      ClassId: UnitClassId
      Faction: FactionVisual
      Health: Disclosure<HealthVisual>
      Level: Disclosure<int32>
      StanceId: Disclosure<string>
      BodyHeading: Disclosure<HeadingRadians>
      SecondaryHeading: Disclosure<SecondaryHeadingVisual>
      ShortLabel: Disclosure<string>
      StatusIds: string array }

/// The accepted gameplay channel that explicitly disclosed attention or a
/// legacy capability-specific secondary heading.
and SecondaryHeadingSource =
    | AttentionHeading
    | WeaponHeading
    | SensorHeading

and SecondaryHeadingVisual =
    { Radians: HeadingRadians
      Source: SecondaryHeadingSource }

type BoardVisual =
    { MinimumColumn: int32
      MinimumRow: int32
      MaximumColumn: int32
      MaximumRow: int32 }

type EdgeVisual =
    { Id: string
      Kind: string
      State: string
      StartColumn: int32
      StartRow: int32
      EndColumn: int32
      EndRow: int32 }

type OverlayScope =
    | SelectedUnitOverlay of unitId: int32
    | WholeForceOverlay

type OverlayVisual =
    { Id: string
      Kind: string
      Scope: OverlayScope
      GeometryRevision: int32
      Points: float array
      Label: Disclosure<string> }

type RenderEventVisual =
    { Id: int32
      Tick: int32
      Kind: string
      SourceUnitId: Disclosure<int32>
      TargetUnitId: Disclosure<int32>
      Summary: Disclosure<string> }

type DisclosureLabel =
    | FullReplayDisclosure
    | PerspectiveDisclosure
    | SandboxDisclosure

/// One independently drawable, committed presentation frame.
type RenderFrame =
    { Tick: int32
      Board: BoardVisual
      Units: UnitVisual array
      Edges: EdgeVisual array
      Overlays: OverlayVisual array
      Events: RenderEventVisual array
      Disclosure: DisclosureLabel }

/// Scalar/array-only wire representation for the browser structured-clone boundary.
type UnitVisualTransport =
    { Id: int32
      AnchorColumn: int32
      AnchorRow: int32
      FootprintWidth: int32
      FootprintDepth: int32
      ClassId: string
      FactionKind: int32
      FactionId: string option
      HealthKind: int32
      HealthRemaining: int32 option
      HealthMaximum: int32 option
      LevelKind: int32
      Level: int32 option
      StanceKind: int32
      StanceId: string option
      BodyHeadingKind: int32
      BodyHeadingRadians: float option
      SecondaryHeadingKind: int32
      SecondaryHeadingRadians: float option
      SecondaryHeadingSource: int32 option
      ShortLabelKind: int32
      ShortLabel: string option
      StatusIds: string array }

type EdgeVisualTransport =
    { Id: string
      Kind: string
      State: string
      StartColumn: int32
      StartRow: int32
      EndColumn: int32
      EndRow: int32 }

type OverlayVisualTransport =
    { Id: string
      Kind: string
      ScopeKind: int32
      ScopeUnitId: int32 option
      GeometryRevision: int32
      Points: float array
      LabelKind: int32
      Label: string option }

type RenderEventVisualTransport =
    { Id: int32
      Tick: int32
      Kind: string
      SourceUnitIdKind: int32
      SourceUnitId: int32 option
      TargetUnitIdKind: int32
      TargetUnitId: int32 option
      SummaryKind: int32
      Summary: string option }

type RenderFrameTransport =
    { Tick: int32
      BoardMinimumColumn: int32
      BoardMinimumRow: int32
      BoardMaximumColumn: int32
      BoardMaximumRow: int32
      Units: UnitVisualTransport array
      Edges: EdgeVisualTransport array
      Overlays: OverlayVisualTransport array
      Events: RenderEventVisualTransport array
      Disclosure: int32 }

[<RequireQualifiedAccess>]
module RenderFrameTransport =
    let private disclosureToTransport value =
        match value with
        | NotPresent -> 0, None
        | NotApplicable -> 1, None
        | ExplicitlyUnknown -> 2, None
        | Disclosed value -> 3, Some value

    let private disclosureFromTransport field kind value =
        match kind, value with
        | 0, None -> NotPresent
        | 1, None -> NotApplicable
        | 2, None -> ExplicitlyUnknown
        | 3, Some value -> Disclosed value
        | _ -> invalidArg field "Invalid disclosure tag/value combination."

    let private headingFromTransport field kind value =
        let disclosed =
            value
            |> Option.map (fun raw ->
                HeadingRadians.tryCreate raw
                |> Option.defaultWith (fun () ->
                    invalidArg field "Heading must be finite."))

        disclosureFromTransport field kind disclosed

    let private factionToTransport faction =
        match faction with
        | Human -> 0, None
        | Arcane -> 1, None
        | Neutral -> 2, None
        | OtherFaction id -> 3, Some id

    let private factionFromTransport kind id =
        match kind, id with
        | 0, None -> Human
        | 1, None -> Arcane
        | 2, None -> Neutral
        | 3, Some value when not (String.IsNullOrWhiteSpace value) ->
            OtherFaction value
        | _ -> invalidArg "FactionKind" "Invalid faction transport."

    let private unitToTransport unit =
        let healthKind, health = disclosureToTransport unit.Health
        let levelKind, level = disclosureToTransport unit.Level
        let stanceKind, stance = disclosureToTransport unit.StanceId
        let bodyKind, body = disclosureToTransport unit.BodyHeading
        let secondaryKind, secondary = disclosureToTransport unit.SecondaryHeading
        let labelKind, label = disclosureToTransport unit.ShortLabel
        let factionKind, factionId = factionToTransport unit.Faction

        { Id = unit.Id
          AnchorColumn = unit.AnchorColumn
          AnchorRow = unit.AnchorRow
          FootprintWidth = CellExtent.value unit.FootprintWidth
          FootprintDepth = CellExtent.value unit.FootprintDepth
          ClassId = UnitClassId.value unit.ClassId
          FactionKind = factionKind
          FactionId = factionId
          HealthKind = healthKind
          HealthRemaining = health |> Option.map HealthVisual.remaining
          HealthMaximum = health |> Option.map HealthVisual.maximum
          LevelKind = levelKind
          Level = level
          StanceKind = stanceKind
          StanceId = stance
          BodyHeadingKind = bodyKind
          BodyHeadingRadians = body |> Option.map HeadingRadians.value
          SecondaryHeadingKind = secondaryKind
          SecondaryHeadingRadians =
            secondary |> Option.map (fun value -> HeadingRadians.value value.Radians)
          SecondaryHeadingSource =
            secondary
            |> Option.map (fun value ->
                match value.Source with
                | WeaponHeading -> 0
                | SensorHeading -> 1
                | AttentionHeading -> 2)
          ShortLabelKind = labelKind
          ShortLabel = label
          StatusIds = Array.copy unit.StatusIds }

    let private unitFromTransport unit =
        let extent field value =
            CellExtent.tryCreate value
            |> Option.defaultWith (fun () ->
                invalidArg field "Footprint extent must be positive.")

        let health =
            match unit.HealthKind, unit.HealthRemaining, unit.HealthMaximum with
            | 0, None, None -> NotPresent
            | 1, None, None -> NotApplicable
            | 2, None, None -> ExplicitlyUnknown
            | 3, Some remaining, Some maximum
                when maximum > 0 && remaining >= 0 && remaining <= maximum ->
                HealthVisual.tryCreate remaining maximum
                |> Option.map Disclosed
                |> Option.defaultWith (fun () ->
                    invalidArg "HealthKind" "Invalid health bounds.")
            | _ ->
                invalidArg
                    "HealthKind"
                    "Invalid health disclosure or bounds."

        { Id = unit.Id
          AnchorColumn = unit.AnchorColumn
          AnchorRow = unit.AnchorRow
          FootprintWidth = extent "FootprintWidth" unit.FootprintWidth
          FootprintDepth = extent "FootprintDepth" unit.FootprintDepth
          ClassId = UnitClassId.resolve unit.ClassId
          Faction = factionFromTransport unit.FactionKind unit.FactionId
          Health = health
          Level = disclosureFromTransport "LevelKind" unit.LevelKind unit.Level
          StanceId =
            disclosureFromTransport "StanceKind" unit.StanceKind unit.StanceId
          BodyHeading =
            headingFromTransport
                "BodyHeadingKind"
                unit.BodyHeadingKind
                unit.BodyHeadingRadians
          SecondaryHeading =
            match
                unit.SecondaryHeadingKind,
                unit.SecondaryHeadingRadians,
                unit.SecondaryHeadingSource
            with
            | 0, None, None -> NotPresent
            | 1, None, None -> NotApplicable
            | 2, None, None -> ExplicitlyUnknown
            | 3, Some radians, Some source ->
                let heading =
                    HeadingRadians.tryCreate radians
                    |> Option.defaultWith (fun () ->
                        invalidArg "SecondaryHeadingRadians" "Heading must be finite.")
                let acceptedSource =
                    match source with
                    | 0 -> WeaponHeading
                    | 1 -> SensorHeading
                    | 2 -> AttentionHeading
                    | _ ->
                        invalidArg
                            "SecondaryHeadingSource"
                            "Unknown secondary-heading gameplay source."
                Disclosed
                    { Radians = heading
                      Source = acceptedSource }
            | _ ->
                invalidArg
                    "SecondaryHeadingKind"
                    "A second heading requires a disclosed angle and accepted typed source."
          ShortLabel =
            disclosureFromTransport
                "ShortLabelKind"
                unit.ShortLabelKind
                unit.ShortLabel
          StatusIds = Array.copy unit.StatusIds }

    let toTransport (frame: RenderFrame) : RenderFrameTransport =
        { Tick = frame.Tick
          BoardMinimumColumn = frame.Board.MinimumColumn
          BoardMinimumRow = frame.Board.MinimumRow
          BoardMaximumColumn = frame.Board.MaximumColumn
          BoardMaximumRow = frame.Board.MaximumRow
          Units = frame.Units |> Array.map unitToTransport
          Edges =
            frame.Edges
            |> Array.map (fun edge ->
                { Id = edge.Id
                  Kind = edge.Kind
                  State = edge.State
                  StartColumn = edge.StartColumn
                  StartRow = edge.StartRow
                  EndColumn = edge.EndColumn
                  EndRow = edge.EndRow })
          Overlays =
            frame.Overlays
            |> Array.map (fun overlay ->
                let labelKind, label = disclosureToTransport overlay.Label
                let scopeKind, scopeUnit =
                    match overlay.Scope with
                    | SelectedUnitOverlay unitId -> 0, Some unitId
                    | WholeForceOverlay -> 1, None
                { Id = overlay.Id
                  Kind = overlay.Kind
                  ScopeKind = scopeKind
                  ScopeUnitId = scopeUnit
                  GeometryRevision = overlay.GeometryRevision
                  Points = Array.copy overlay.Points
                  LabelKind = labelKind
                  Label = label })
          Events =
            frame.Events
            |> Array.map (fun event ->
                let sourceKind, source = disclosureToTransport event.SourceUnitId
                let targetKind, target = disclosureToTransport event.TargetUnitId
                let summaryKind, summary = disclosureToTransport event.Summary
                { Id = event.Id
                  Tick = event.Tick
                  Kind = event.Kind
                  SourceUnitIdKind = sourceKind
                  SourceUnitId = source
                  TargetUnitIdKind = targetKind
                  TargetUnitId = target
                  SummaryKind = summaryKind
                  Summary = summary })
          Disclosure =
            match frame.Disclosure with
            | FullReplayDisclosure -> 0
            | PerspectiveDisclosure -> 1
            | SandboxDisclosure -> 2 }

    let fromTransport (frame: RenderFrameTransport) : RenderFrame =
        { Tick = frame.Tick
          Board =
            { MinimumColumn = frame.BoardMinimumColumn
              MinimumRow = frame.BoardMinimumRow
              MaximumColumn = frame.BoardMaximumColumn
              MaximumRow = frame.BoardMaximumRow }
          Units = frame.Units |> Array.map unitFromTransport
          Edges =
            frame.Edges
            |> Array.map (fun edge ->
                { Id = edge.Id
                  Kind = edge.Kind
                  State = edge.State
                  StartColumn = edge.StartColumn
                  StartRow = edge.StartRow
                  EndColumn = edge.EndColumn
                  EndRow = edge.EndRow })
          Overlays =
            frame.Overlays
            |> Array.map (fun overlay ->
                { Id = overlay.Id
                  Kind = overlay.Kind
                  Scope =
                    match overlay.ScopeKind, overlay.ScopeUnitId with
                    | 0, Some unitId -> SelectedUnitOverlay unitId
                    | 1, None -> WholeForceOverlay
                    | _ ->
                        invalidArg
                            "ScopeKind"
                            "Invalid overlay scope tag/value combination."
                  GeometryRevision = overlay.GeometryRevision
                  Points = Array.copy overlay.Points
                  Label =
                    disclosureFromTransport
                        "LabelKind"
                        overlay.LabelKind
                        overlay.Label })
          Events =
            frame.Events
            |> Array.map (fun event ->
                { Id = event.Id
                  Tick = event.Tick
                  Kind = event.Kind
                  SourceUnitId =
                    disclosureFromTransport
                        "SourceUnitIdKind"
                        event.SourceUnitIdKind
                        event.SourceUnitId
                  TargetUnitId =
                    disclosureFromTransport
                        "TargetUnitIdKind"
                        event.TargetUnitIdKind
                        event.TargetUnitId
                  Summary =
                    disclosureFromTransport
                        "SummaryKind"
                        event.SummaryKind
                        event.Summary })
          Disclosure =
            match frame.Disclosure with
            | 0 -> FullReplayDisclosure
            | 1 -> PerspectiveDisclosure
            | 2 -> SandboxDisclosure
            | value ->
                invalidArg
                    "Disclosure"
                    ("Unknown frame disclosure value: " + string value) }
