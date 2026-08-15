namespace SIR.Client

[<RequireQualifiedAccess>]
module MapEditorValidation =
    let hasSupportedDimensions (map: MapDefinition) =
        map.Width >= 4 && map.Width <= 80 && map.Height >= 4 && map.Height <= 80
