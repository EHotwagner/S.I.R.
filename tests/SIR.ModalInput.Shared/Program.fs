module SIR.ModalInput.Program

[<EntryPoint>]
let main _ =
    SIR.ModalInput.Fixtures.evaluate ()
    |> printfn "%s"

    0
