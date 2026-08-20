module SIR.Client.Web.SidebarResizeView

open Browser.Types
open Fable.Core
open Feliz
open SIR.Client

[<Emit("$0.setPointerCapture($1)")>]
let private capturePointer (target: EventTarget) (pointerId: int) : unit = jsNative

[<Emit("$0.releasePointerCapture($1)")>]
let private releasePointer (target: EventTarget) (pointerId: int) : unit = jsNative

[<Emit("Math.round($1 - $0.closest('.tactical-sidebar').getBoundingClientRect().left)")>]
let private leftWidthFromPointer (target: EventTarget) (clientX: float) : int = jsNative

[<Emit("Math.round($0.closest('.tactical-sidebar').getBoundingClientRect().right - $1)")>]
let private rightWidthFromPointer (target: EventTarget) (clientX: float) : int = jsNative

let view
    (side: SidebarSide)
    (currentWidth: int)
    (maximumWidth: int)
    (resizeActive: bool)
    beginResize
    resize
    endResize
    resizeKeyboard
    =
    let sideName = if side = Left then "left" else "right"
    let clamp width = max 160 (min maximumWidth width)
    Html.div [
        prop.id ("tactical-sidebar-" + sideName + "-resize")
        prop.className ("tactical-sidebar-resize tactical-sidebar-" + sideName + "-resize")
        prop.role.separator
        prop.tabIndex 0
        prop.ariaLabel ("Resize " + sideName + " tactical sidebar")
        prop.custom ("aria-orientation", "vertical")
        prop.ariaValueMin 160
        prop.ariaValueMax maximumWidth
        prop.ariaValueNow currentWidth
        prop.onPointerDown (fun event ->
            event.preventDefault ()
            capturePointer event.currentTarget (int event.pointerId)
            beginResize side)
        prop.onPointerMove (fun event ->
            if resizeActive then
                let requested =
                    if side = Left then
                        leftWidthFromPointer event.currentTarget event.clientX
                    else
                        rightWidthFromPointer event.currentTarget event.clientX
                resize side (clamp requested))
        prop.onPointerUp (fun event ->
            releasePointer event.currentTarget (int event.pointerId)
            endResize ())
        prop.onPointerCancel (fun event ->
            releasePointer event.currentTarget (int event.pointerId)
            endResize ())
        prop.onKeyDown (fun event ->
            let requested =
                match side, event.key with
                | Left, "ArrowLeft"
                | Right, "ArrowRight" -> Some(currentWidth - 16)
                | Left, "ArrowRight"
                | Right, "ArrowLeft" -> Some(currentWidth + 16)
                | _, "PageDown" -> Some(currentWidth - 64)
                | _, "PageUp" -> Some(currentWidth + 64)
                | _, "Home" -> Some 160
                | _, "End" -> Some maximumWidth
                | _ -> None
            requested
            |> Option.iter (fun width ->
                event.preventDefault ()
                event.stopPropagation ()
                resizeKeyboard side (clamp width)))
    ]
