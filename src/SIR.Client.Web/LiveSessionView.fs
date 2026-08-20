namespace SIR.Client.Web

open Feliz

[<RequireQualifiedAccess>]
module LiveSessionView =

    let menuGroup (state: LiveSession.State) onAdvance onDisconnect onReconnect =
        let command (label: string) (text: string) (onClick: unit -> unit) =
            Html.button [
                prop.type'.button
                prop.custom ("role", "menuitem")
                prop.tabIndex -1
                prop.disabled state.Connection.IsNone
                prop.text text
                prop.ariaLabel label
                prop.onClick (fun _ -> onClick ())
            ]

        Html.section [
            prop.id "sir-live-session"
            prop.className "live-session-menu-group"
            prop.ariaLabel "Authoritative live session"
            prop.custom ("data-status", state.Status)
            prop.custom ("data-tick", state.Snapshot |> Option.map _.Tick |> Option.defaultValue 0 |> string)
            prop.custom ("data-server-sequence", state.Snapshot |> Option.map _.ServerSequence |> Option.defaultValue 0 |> string)
            prop.custom ("data-resync-count", string state.ResyncCount)
            prop.custom ("data-session-id", state.Bootstrap |> Option.map _.SessionId |> Option.defaultValue "")
            prop.children [
                Html.p (
                    "live " + state.Status
                    + " · tick " + string (state.Snapshot |> Option.map _.Tick |> Option.defaultValue 0)
                )
                command "Send the next player-visible live advance command" "Advance live session" onAdvance
                command "Disconnect the player-visible live session" "Disconnect live session" onDisconnect
                command "Reconnect and request the authoritative live snapshot" "Reconnect live session" onReconnect
            ]
        ]
