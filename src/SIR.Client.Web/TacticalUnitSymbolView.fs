module SIR.Client.Web.TacticalUnitSymbolView

open System
open Feliz
open SIR.Client

let private directionChannel
    (channel: string)
    (color: string)
    (dash: string option)
    (length: float)
    (pipRadius: float)
    (centerX: float)
    (centerY: float)
    heading
    (visualSystem: TacticalVisualSystem)
    =
    let radians = HeadingRadians.value heading
    let targetX = centerX + Math.Cos(radians) * length
    let targetY = centerY + Math.Sin(radians) * length
    [ Svg.line [
          svg.custom ("data-unit-heading-underlay", channel)
          svg.x1 centerX; svg.y1 centerY; svg.x2 targetX; svg.y2 targetY
          svg.stroke visualSystem.UnitBody; svg.strokeWidth 8
          match dash with Some value -> svg.custom ("stroke-dasharray", value) | None -> ()
          svg.custom ("pointer-events", "none")
      ]
      Svg.line [
          svg.custom ("data-unit-heading", channel)
          svg.x1 centerX; svg.y1 centerY; svg.x2 targetX; svg.y2 targetY
          svg.stroke color; svg.strokeWidth 4
          match dash with Some value -> svg.custom ("stroke-dasharray", value) | None -> ()
          svg.custom ("pointer-events", "none")
      ]
      Svg.circle [
          svg.custom ("data-unit-heading-end", channel)
          svg.custom ("data-unit-direction-pip", channel)
          svg.cx targetX; svg.cy targetY; svg.r pipRadius
          svg.fill color; svg.stroke visualSystem.UnitBody; svg.strokeWidth 4
          svg.custom ("pointer-events", "none")
      ] ]

let private healthChannel
    presentationX
    presentationY
    width
    depth
    (visualSystem: TacticalVisualSystem)
    healthDisclosure
    =
    match healthDisclosure with
    | Disclosed health ->
        let remaining = HealthVisual.remaining health
        let maximum = HealthVisual.maximum health
        let activeSegments =
            int ((int64 remaining * 12L + int64 maximum - 1L) / int64 maximum)
            |> max 0
            |> min 12
        let common attributes =
            Svg.g [
                svg.custom ("data-unit-health", string remaining)
                svg.custom ("data-unit-health-maximum", string maximum)
                svg.custom ("pointer-events", "none")
                svg.children attributes
            ]
        match visualSystem.Density with
        | OrdinaryDensity ->
            let gap = 2.0
            let inset = 12.0
            let segmentWidth = max 2.0 ((width - inset * 2.0 - gap * 11.0) / 12.0)
            common [
                for index in 0 .. 11 do
                    Svg.rect [
                        svg.custom ("data-unit-health-segment", string index)
                        svg.custom ("data-unit-health-state", if index < activeSegments then "active" else "depleted")
                        svg.x (presentationX + inset + float index * (segmentWidth + gap))
                        svg.y (presentationY + depth - 17.0)
                        svg.width segmentWidth; svg.height 7; svg.rx 1
                        svg.fill (if index < activeSegments then visualSystem.Palette.HealthActive else visualSystem.Palette.HealthDepleted)
                    ]
            ]
        | DenseDensity
        | StressDensity ->
            let inset = 8.0
            let available = max 4.0 (width - inset * 2.0)
            common [
                Svg.rect [
                    svg.custom ("data-unit-health-density", "compact")
                    svg.x (presentationX + inset); svg.y (presentationY + depth - 12.0)
                    svg.width available; svg.height 5; svg.rx 1
                    svg.fill visualSystem.Palette.HealthDepleted
                ]
                Svg.rect [
                    svg.custom ("data-unit-health-fill", string remaining)
                    svg.x (presentationX + inset); svg.y (presentationY + depth - 12.0)
                    svg.width (available * float remaining / float maximum); svg.height 5; svg.rx 1
                    svg.fill visualSystem.Palette.HealthActive
                ]
            ]
    | NotPresent
    | NotApplicable
    | ExplicitlyUnknown -> Html.none

let private textChannel
    (attributeName: string)
    (value: string)
    (x: float)
    (y: float)
    (anchor: string)
    (color: string)
    (fontSize: int)
    (visualSystem: TacticalVisualSystem)
    =
    Svg.text [
        svg.custom (attributeName, value)
        svg.custom ("text-anchor", anchor)
        svg.custom ("paint-order", "stroke")
        svg.x x; svg.y y
        svg.fill color
        svg.stroke visualSystem.UnitBody
        svg.strokeWidth 4
        svg.fontSize fontSize
        svg.custom ("pointer-events", "none")
        svg.text value
    ]

let private informationChannels
    presentationX
    presentationY
    width
    depth
    (visualSystem: TacticalVisualSystem)
    (visual: UnitVisual)
    =
    match visualSystem.Density with
    | OrdinaryDensity ->
        [ match visual.Level with
          | Disclosed level ->
              textChannel
                  "data-unit-elevation-label"
                  ("L" + string level)
                  (presentationX + width - 9.0)
                  (presentationY + 18.0)
                  "end"
                  visualSystem.Palette.Text
                  10
                  visualSystem
          | _ -> ()
          if visual.StatusIds.Length > 0 then
              let status = String.concat " · " visual.StatusIds
              textChannel
                  "data-unit-status-label"
                  status
                  (presentationX + 9.0)
                  (presentationY + 18.0)
                  "start"
                  visualSystem.Intent
                  10
                  visualSystem
          match visual.StanceId with
          | Disclosed stance ->
              let glyph =
                  let normalized = stance.Trim()
                  if String.IsNullOrWhiteSpace normalized then "?"
                  else normalized.Substring(0, 1).ToUpperInvariant()
              textChannel
                  "data-unit-stance-glyph"
                  glyph
                  (presentationX + 9.0)
                  (presentationY + depth - 25.0)
                  "start"
                  visualSystem.Recovery
                  14
                  visualSystem
              |> fun element ->
                  Svg.g [
                      svg.custom ("data-unit-stance", stance)
                      svg.custom ("data-unit-stance-label", stance)
                      svg.custom ("aria-label", "Stance " + stance)
                      svg.custom ("pointer-events", "none")
                      svg.children [ element ]
                  ]
          | _ -> () ]
    | DenseDensity
    | StressDensity ->
        // At overview densities, one marker preserves the presence of supplementary
        // state without multiplying labels across hundreds of overlapping symbols.
        let disclosedCount =
            (match visual.Level with Disclosed _ -> 1 | _ -> 0)
            + (match visual.StanceId with Disclosed _ -> 1 | _ -> 0)
            + (if visual.StatusIds.Length > 0 then 1 else 0)
        if disclosedCount = 0 then []
        else
            [ Svg.circle [
                  svg.custom ("data-unit-information-marker", string disclosedCount)
                  svg.cx (presentationX + width - 8.0)
                  svg.cy (presentationY + 8.0)
                  svg.r 3
                  svg.fill visualSystem.Intent
                  svg.custom ("pointer-events", "none")
              ] ]

let channels
    (visualSystem: TacticalVisualSystem)
    presentationX
    presentationY
    width
    depth
    (visual: UnitVisual)
    =
    let centerX = presentationX + width / 2.0
    let centerY = presentationY + depth / 2.0
    let symbolSize = min width depth
    [ match visual.BodyHeading with
      | Disclosed heading ->
          yield!
              directionChannel
                  "facing"
                  visualSystem.Palette.Text
                  None
                  (max 14.0 (symbolSize / 2.0 - 12.0))
                  9.0
                  centerX
                  centerY
                  heading
                  visualSystem
      | _ -> ()
      match visual.SecondaryHeading with
      | Disclosed heading ->
          yield!
              directionChannel
                  "attention"
                  visualSystem.Intent
                  (Some "5 4")
                  (max 12.0 (symbolSize / 2.0 - 32.0))
                  6.0
                  centerX
                  centerY
                  heading.Radians
                  visualSystem
      | _ -> ()
      healthChannel presentationX presentationY width depth visualSystem visual.Health
      yield! informationChannels presentationX presentationY width depth visualSystem visual ]
