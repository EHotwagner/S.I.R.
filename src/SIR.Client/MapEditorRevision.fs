namespace SIR.Client

[<RequireQualifiedAccess>]
module MapEditorRevision =
    let create number parent document digest =
        { Number = number
          ParentDigest = parent
          Document = document
          Digest = digest }
