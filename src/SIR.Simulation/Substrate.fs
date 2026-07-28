namespace SIR.Simulation

open FS.GG.Game.Core
open SIR.Domain

/// The minimal published-package seam used before the first simulation slice.
[<RequireQualifiedAccess>]
module Substrate =
    let cellBytes (cell: Cell) =
        CanonicalEncoding.concatenate
            [ CanonicalEncoding.int32LittleEndian cell.Col
              CanonicalEncoding.int32LittleEndian cell.Row ]

    let origin = { Col = 0; Row = 0 }
