namespace SIR.Client

[<RequireQualifiedAccess>]
module MapEditorRevision =
    let create number parent document tacticalDocument tacticalSeed digest =
        { Number = number
          ParentDigest = parent
          Document = document
          TacticalDocument = tacticalDocument
          TacticalSeed = tacticalSeed
          Digest = digest }
