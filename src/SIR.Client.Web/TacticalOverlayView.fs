module SIR.Client.Web.TacticalOverlayView

open Feliz
open SIR.Client

let private pointString cellSize points =
    points
    |> Array.chunkBySize 2
    |> Array.choose (fun pair ->
        if pair.Length = 2 then Some(string (pair[0] * cellSize) + "," + string (pair[1] * cellSize))
        else None)
    |> String.concat " "

let payloadChildren (cellSize: float) (payload: TacticalOverlayPayload) =
    [ match payload.Geometry with
      | FootprintGeometry(centerX, centerY, width, depth) ->
          Svg.rect [ svg.x ((centerX - width / 2.0) * cellSize); svg.y ((centerY - depth / 2.0) * cellSize); svg.width (width * cellSize); svg.height (depth * cellSize); svg.fill "none"; svg.stroke "currentColor"; svg.strokeWidth 2 ]
      | DirectionGeometry(originX, originY, heading, length, arc) ->
          Svg.line [ svg.x1 (originX * cellSize); svg.y1 (originY * cellSize); svg.x2 ((originX + cos heading * length) * cellSize); svg.y2 ((originY + sin heading * length) * cellSize); svg.stroke "currentColor"; svg.strokeWidth 3; svg.custom ("data-direction-arc", string arc) ]
      | PathGeometry(points, movementCost, blockerIds) ->
          Svg.polyline [ svg.points (pointString cellSize points); svg.fill "none"; svg.stroke "currentColor"; svg.strokeWidth 2; svg.custom ("stroke-dasharray", "6 3"); svg.custom ("data-movement-cost", string movementCost) ]
          for index, blocker in Array.indexed blockerIds do
              let anchor = max 0 (points.Length - 2)
              Svg.circle [ svg.cx ((points[anchor] + float index * 0.08) * cellSize); svg.cy (points[anchor + 1] * cellSize); svg.r 4; svg.fill "currentColor"; svg.custom ("data-blocker-id", blocker) ]
      | AreaGeometry(centerX, centerY, radius) ->
          Svg.circle [ svg.cx (centerX * cellSize); svg.cy (centerY * cellSize); svg.r (radius * cellSize); svg.fill "none"; svg.stroke "currentColor"; svg.strokeWidth 2 ]
      | TraceGeometry(points, impactX, impactY) ->
          Svg.polyline [ svg.points (pointString cellSize points); svg.fill "none"; svg.stroke "currentColor"; svg.strokeWidth 2 ]
          Svg.circle [ svg.cx (impactX * cellSize); svg.cy (impactY * cellSize); svg.r 5; svg.fill "currentColor"; svg.custom ("data-impact", "authoritative") ]
      | StatusGeometry(anchorX, anchorY, current, maximum, tokens) ->
          Svg.circle [ svg.cx (anchorX * cellSize); svg.cy (anchorY * cellSize); svg.r 6; svg.fill "none"; svg.stroke "currentColor"; svg.strokeWidth 2; svg.custom ("data-status-current", current |> Option.map string |> Option.defaultValue ""); svg.custom ("data-status-maximum", maximum |> Option.map string |> Option.defaultValue ""); svg.custom ("data-status-tokens", String.concat ";" tokens) ] ]
