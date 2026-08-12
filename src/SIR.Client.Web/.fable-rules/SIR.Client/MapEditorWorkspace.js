
import { Record, Union } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { array_type, uint8_type, class_type, option_type, float64_type, bool_type, string_type, record_type, int32_type, union_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { MapEdgeDirection, MapEdgeDirection_$reflection } from "./MapEditorTypes.js";
import { BattlefieldCamera, BattlefieldCamera_$reflection } from "./Battlefield.js";
import { remove, tryFind, add, toArray, empty } from "../fable_modules/fable-library-js.5.13.0/Map.js";
import { equals, round, min as min_1, compare, max as max_1, int32ToString, numberHash, comparePrimitives } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { toBase64String, isNullOrWhiteSpace, format, join } from "../fable_modules/fable-library-js.5.13.0/String.js";
import { sortBy, truncate, equalsWith, item, map as map_1 } from "../fable_modules/fable-library-js.5.13.0/Array.js";
import { mapIndexed, forAll } from "../fable_modules/fable-library-js.5.13.0/Seq.js";
import { ofArray, contains } from "../fable_modules/fable-library-js.5.13.0/List.js";
import { FSharpResult$2 } from "../fable_modules/fable-library-js.5.13.0/Result.js";
import { sha256 } from "../SIR.Domain/CanonicalHash.js";
import { toArray as toArray_1, ofNullable, defaultArg } from "../fable_modules/fable-library-js.5.13.0/Option.js";
import { isInfinity, min, max } from "../fable_modules/fable-library-js.5.13.0/Double.js";

export class BackgroundFit extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["FitInside", "FillAndCrop", "StretchToBoard", "NativePixels"];
    }
    static FitInside = new BackgroundFit(0, []);
    static FillAndCrop = new BackgroundFit(1, []);
    static StretchToBoard = new BackgroundFit(2, []);
    static NativePixels = new BackgroundFit(3, []);
}

export function BackgroundFit_$reflection() {
    return union_type("SIR.Client.BackgroundFit", [], BackgroundFit, () => [[], [], [], []]);
}

export class BackgroundCrop extends Record {
    constructor(Left, Top, Width, Height) {
        super();
        this.Left = (Left | 0);
        this.Top = (Top | 0);
        this.Width = (Width | 0);
        this.Height = (Height | 0);
    }
}

export function BackgroundCrop_$reflection() {
    return record_type("SIR.Client.BackgroundCrop", [], BackgroundCrop, () => [["Left", int32_type], ["Top", int32_type], ["Width", int32_type], ["Height", int32_type]]);
}

export class LocalRasterBackground extends Record {
    constructor(AssetId, FileName, MediaType, PixelWidth, PixelHeight, ByteLength, DataUrl, Locked, Opacity, Fit, Crop, GridOffsetX, GridOffsetY, PixelsPerCell) {
        super();
        this.AssetId = AssetId;
        this.FileName = FileName;
        this.MediaType = MediaType;
        this.PixelWidth = (PixelWidth | 0);
        this.PixelHeight = (PixelHeight | 0);
        this.ByteLength = (ByteLength | 0);
        this.DataUrl = DataUrl;
        this.Locked = Locked;
        this.Opacity = Opacity;
        this.Fit = Fit;
        this.Crop = Crop;
        this.GridOffsetX = GridOffsetX;
        this.GridOffsetY = GridOffsetY;
        this.PixelsPerCell = PixelsPerCell;
    }
}

export function LocalRasterBackground_$reflection() {
    return record_type("SIR.Client.LocalRasterBackground", [], LocalRasterBackground, () => [["AssetId", string_type], ["FileName", string_type], ["MediaType", string_type], ["PixelWidth", int32_type], ["PixelHeight", int32_type], ["ByteLength", int32_type], ["DataUrl", string_type], ["Locked", bool_type], ["Opacity", float64_type], ["Fit", BackgroundFit_$reflection()], ["Crop", option_type(BackgroundCrop_$reflection())], ["GridOffsetX", float64_type], ["GridOffsetY", float64_type], ["PixelsPerCell", float64_type]]);
}

export class EditorPointerKind extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["MousePointer", "PenPointer", "TouchPointer"];
    }
    static MousePointer = new EditorPointerKind(0, []);
    static PenPointer = new EditorPointerKind(1, []);
    static TouchPointer = new EditorPointerKind(2, []);
}

export function EditorPointerKind_$reflection() {
    return union_type("SIR.Client.EditorPointerKind", [], EditorPointerKind, () => [[], [], []]);
}

export class EditorPointer extends Record {
    constructor(Id, Kind, X, Y, RequestsPan) {
        super();
        this.Id = (Id | 0);
        this.Kind = Kind;
        this.X = X;
        this.Y = Y;
        this.RequestsPan = RequestsPan;
    }
}

export function EditorPointer_$reflection() {
    return record_type("SIR.Client.EditorPointer", [], EditorPointer, () => [["Id", int32_type], ["Kind", EditorPointerKind_$reflection()], ["X", float64_type], ["Y", float64_type], ["RequestsPan", bool_type]]);
}

export class MapCellHit extends Record {
    constructor(Column, Row) {
        super();
        this.Column = (Column | 0);
        this.Row = (Row | 0);
    }
}

export function MapCellHit_$reflection() {
    return record_type("SIR.Client.MapCellHit", [], MapCellHit, () => [["Column", int32_type], ["Row", int32_type]]);
}

export class MapEdgeHit extends Record {
    constructor(Column, Row, Direction, DistancePixels) {
        super();
        this.Column = (Column | 0);
        this.Row = (Row | 0);
        this.Direction = Direction;
        this.DistancePixels = DistancePixels;
    }
}

export function MapEdgeHit_$reflection() {
    return record_type("SIR.Client.MapEdgeHit", [], MapEdgeHit, () => [["Column", int32_type], ["Row", int32_type], ["Direction", MapEdgeDirection_$reflection()], ["DistancePixels", float64_type]]);
}

export class EditorWorkspaceState extends Record {
    constructor(Camera, ViewportWidth, ViewportHeight, InspectorCollapsed, ReducedMotion, CapturedPointers, Background, BackgroundAnnouncement) {
        super();
        this.Camera = Camera;
        this.ViewportWidth = ViewportWidth;
        this.ViewportHeight = ViewportHeight;
        this.InspectorCollapsed = InspectorCollapsed;
        this.ReducedMotion = ReducedMotion;
        this.CapturedPointers = CapturedPointers;
        this.Background = Background;
        this.BackgroundAnnouncement = BackgroundAnnouncement;
    }
}

export function EditorWorkspaceState_$reflection() {
    return record_type("SIR.Client.EditorWorkspaceState", [], EditorWorkspaceState, () => [["Camera", BattlefieldCamera_$reflection()], ["ViewportWidth", float64_type], ["ViewportHeight", float64_type], ["InspectorCollapsed", bool_type], ["ReducedMotion", bool_type], ["CapturedPointers", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [int32_type, EditorPointer_$reflection()])], ["Background", option_type(LocalRasterBackground_$reflection())], ["BackgroundAnnouncement", string_type]]);
}

export class EditorWorkspaceAction extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["ResizeViewport", "PanEditorBy", "ZoomEditorAt", "FitEditorBoard", "FrameEditorSelection", "ResetEditorCamera", "ToggleEditorInspector", "SetEditorReducedMotion", "StartEditorPointer", "MoveEditorPointer", "EndEditorPointer", "LoseEditorPointerCapture", "CancelEditorPointers", "AttachLocalRaster", "RemoveLocalRaster", "ToggleBackgroundLock", "SetBackgroundOpacity", "SetBackgroundFit", "SetBackgroundCrop", "SetBackgroundGridOffset", "NudgeBackgroundGridOffset", "SetBackgroundPixelsPerCell", "AlignBackgroundGrid"];
    }
    static FitEditorBoard = new EditorWorkspaceAction(3, []);
    static FrameEditorSelection = new EditorWorkspaceAction(4, []);
    static ResetEditorCamera = new EditorWorkspaceAction(5, []);
    static ToggleEditorInspector = new EditorWorkspaceAction(6, []);
    static CancelEditorPointers = new EditorWorkspaceAction(12, []);
    static RemoveLocalRaster = new EditorWorkspaceAction(14, []);
    static ToggleBackgroundLock = new EditorWorkspaceAction(15, []);
}

export function EditorWorkspaceAction_$reflection() {
    return union_type("SIR.Client.EditorWorkspaceAction", [], EditorWorkspaceAction, () => [[["width", float64_type], ["height", float64_type]], [["x", float64_type], ["y", float64_type]], [["x", float64_type], ["y", float64_type], ["factor", float64_type]], [], [], [], [], [["Item", bool_type]], [["Item", EditorPointer_$reflection()]], [["Item", EditorPointer_$reflection()]], [["pointerId", int32_type]], [["pointerId", int32_type]], [], [["fileName", string_type], ["declaredMediaType", string_type], ["bytes", array_type(uint8_type)]], [], [], [["Item", float64_type]], [["Item", BackgroundFit_$reflection()]], [["Item", option_type(BackgroundCrop_$reflection())]], [["x", float64_type], ["y", float64_type]], [["x", float64_type], ["y", float64_type]], [["Item", float64_type]], [["firstImageX", float64_type], ["firstImageY", float64_type], ["secondImageX", float64_type], ["secondImageY", float64_type], ["cellsBetween", int32_type]]]);
}

export function MapEditorWorkspace_initial(reducedMotion) {
    return new EditorWorkspaceState(new BattlefieldCamera(36, 36, 1), 960, 640, true, reducedMotion, empty({
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    }), undefined, "No local background selected.");
}

function MapEditorWorkspace_hex(bytes) {
    return join("", map_1((value) => format('{0:' + "x2" + '}', value), bytes));
}

function MapEditorWorkspace_readBigEndian32(bytes, offset) {
    return ((((~~item(offset, bytes) << 24) | (~~item(offset + 1, bytes) << 16)) | (~~item(offset + 2, bytes) << 8)) | ~~item(offset + 3, bytes)) | 0;
}

function MapEditorWorkspace_readLittleEndian24(bytes, offset) {
    return ((~~item(offset, bytes) | (~~item(offset + 1, bytes) << 8)) | (~~item(offset + 2, bytes) << 16)) | 0;
}

function MapEditorWorkspace_readLittleEndian16(bytes, offset) {
    return (~~item(offset, bytes) | (~~item(offset + 1, bytes) << 8)) | 0;
}

function MapEditorWorkspace_tryPngDimensions(bytes) {
    const signature = new Uint8Array([137, 80, 78, 71, 13, 10, 26, 10]);
    if (((bytes.length >= 24) && equalsWith((x, y) => (x === y), bytes.slice(0, 7 + 1), signature)) && equalsWith((x_1, y_1) => (x_1 === y_1), bytes.slice(12, 15 + 1), new Uint8Array([73, 72, 68, 82]))) {
        return [MapEditorWorkspace_readBigEndian32(bytes, 16), MapEditorWorkspace_readBigEndian32(bytes, 20), "image/png"];
    }
    else {
        return undefined;
    }
}

function MapEditorWorkspace_tryWebpDimensions(bytes) {
    const ascii = (offset, value) => {
        if (bytes.length >= (offset + value.length)) {
            return forAll((x) => x, mapIndexed((index, character) => (item(offset + index, bytes) === (character.charCodeAt(0) & 0xFF)), value.split("")));
        }
        else {
            return false;
        }
    };
    if ((bytes.length < 30) ? true : !(ascii(0, "RIFF") && ascii(8, "WEBP"))) {
        return undefined;
    }
    else if (ascii(12, "VP8X")) {
        return [MapEditorWorkspace_readLittleEndian24(bytes, 24) + 1, MapEditorWorkspace_readLittleEndian24(bytes, 27) + 1, "image/webp"];
    }
    else if (ascii(12, "VP8L") && (bytes.length >= 25)) {
        const bits = (((((item(21, bytes) | ((item(22, bytes) << 8) >>> 0)) >>> 0) | ((item(23, bytes) << 16) >>> 0)) >>> 0) | ((item(24, bytes) << 24) >>> 0)) >>> 0;
        return [~~((bits & 16383) >>> 0) + 1, ~~(((bits >>> 14) & 16383) >>> 0) + 1, "image/webp"];
    }
    else if ((ascii(12, "VP8 ") && (bytes.length >= 30)) && equalsWith((x_1, y) => (x_1 === y), bytes.slice(23, 25 + 1), new Uint8Array([157, 1, 42]))) {
        return [MapEditorWorkspace_readLittleEndian16(bytes, 26) & 16383, MapEditorWorkspace_readLittleEndian16(bytes, 28) & 16383, "image/webp"];
    }
    else {
        return undefined;
    }
}

function MapEditorWorkspace_tryJpegDimensions(bytes) {
    if (((bytes.length < 4) ? true : (item(0, bytes) !== 255)) ? true : (item(1, bytes) !== 216)) {
        return undefined;
    }
    else {
        let offset = 2;
        let result = undefined;
        while ((result == null) && ((offset + 3) < bytes.length)) {
            if (item(offset, bytes) !== 255) {
                offset = ((offset + 1) | 0);
            }
            else {
                const marker = ~~item(offset + 1, bytes) | 0;
                if ((marker === 217) ? true : (marker === 218)) {
                    offset = (bytes.length | 0);
                }
                else if ((marker >= 208) && (marker <= 215)) {
                    offset = ((offset + 2) | 0);
                }
                else {
                    const length = ((~~item(offset + 2, bytes) * 256) + ~~item(offset + 3, bytes)) | 0;
                    const isStartOfFrame = contains(marker, ofArray([192, 193, 194, 195, 197, 198, 199, 201, 202, 203, 205, 206, 207]), {
                        Equals: (x, y) => (x === y),
                        GetHashCode: (x) => (numberHash(x) | 0),
                    });
                    if ((length < 2) ? true : (((offset + 2) + length) > bytes.length)) {
                        offset = (bytes.length | 0);
                    }
                    else if (isStartOfFrame && (length >= 7)) {
                        const height = ((~~item(offset + 5, bytes) * 256) + ~~item(offset + 6, bytes)) | 0;
                        const width = ((~~item(offset + 7, bytes) * 256) + ~~item(offset + 8, bytes)) | 0;
                        result = [width, height, "image/jpeg"];
                    }
                    else {
                        offset = (((offset + 2) + length) | 0);
                    }
                }
            }
        }
        return result;
    }
}

export function MapEditorWorkspace_tryCreateLocalRaster(fileName, declaredMediaType, bytes) {
    let width, height, mediaType_1;
    if (bytes.length === 0) {
        return new FSharpResult$2(/* Error */ 1, ["BACKGROUND-EMPTY: the selected file is empty."]);
    }
    else if (bytes.length > 10000000) {
        return new FSharpResult$2(/* Error */ 1, [("BACKGROUND-SIZE: local backgrounds are limited to " + int32ToString(10000000)) + " bytes."]);
    }
    else {
        let dimensions;
        let option_3;
        const option_1 = MapEditorWorkspace_tryPngDimensions(bytes);
        option_3 = ((option_1 != null) ? option_1 : MapEditorWorkspace_tryJpegDimensions(bytes));
        dimensions = ((option_3 != null) ? option_3 : MapEditorWorkspace_tryWebpDimensions(bytes));
        if (dimensions != null) {
            if ((width = (dimensions[0] | 0), (dimensions[2], (height = (dimensions[1] | 0), (((width < 1) ? true : (height < 1)) ? true : (width > 8192)) ? true : (height > 8192))))) {
                const height_2 = dimensions[1] | 0;
                const mediaType_2 = dimensions[2];
                const width_2 = dimensions[0] | 0;
                return new FSharpResult$2(/* Error */ 1, [("BACKGROUND-DIMENSIONS: raster dimensions must be between 1 and " + int32ToString(8192)) + " pixels."]);
            }
            else if ((dimensions[0], (mediaType_1 = dimensions[2], (dimensions[1], !isNullOrWhiteSpace(declaredMediaType) && (declaredMediaType !== mediaType_1))))) {
                const height_3 = dimensions[1] | 0;
                const mediaType_3 = dimensions[2];
                const width_3 = dimensions[0] | 0;
                return new FSharpResult$2(/* Error */ 1, [((("BACKGROUND-MEDIA-TYPE: declared type " + declaredMediaType) + " does not match ") + mediaType_3) + "."]);
            }
            else {
                const height_4 = dimensions[1] | 0;
                const mediaType_4 = dimensions[2];
                const width_4 = dimensions[0] | 0;
                return new FSharpResult$2(/* Ok */ 0, [new LocalRasterBackground("sha256:" + MapEditorWorkspace_hex(sha256(bytes)), defaultArg(ofNullable(fileName), "local-background"), mediaType_4, width_4, height_4, bytes.length, (("data:" + mediaType_4) + ";base64,") + toBase64String(bytes), true, 0.65, BackgroundFit.FitInside, undefined, 0, 0, 24)]);
            }
        }
        else {
            return new FSharpResult$2(/* Error */ 1, ["BACKGROUND-TYPE: only signature-validated PNG, JPEG, and WebP raster files are accepted; SVG and executable content are rejected."]);
        }
    }
}

export function MapEditorWorkspace_backgroundRenderBox(boardWidth, boardHeight, background) {
    let source;
    const matchValue = background.Crop;
    if (matchValue == null) {
        source = [background.PixelWidth, background.PixelHeight];
    }
    else {
        const crop = matchValue;
        source = [crop.Width, crop.Height];
    }
    const sourceWidth = source[0];
    const sourceHeight = source[1];
    const targetWidth = boardWidth * 24;
    const targetHeight = boardHeight * 24;
    let patternInput;
    const matchValue_1 = background.Fit;
    switch (matchValue_1.tag) {
        case 1: {
            const scale_1 = max(targetWidth / sourceWidth, targetHeight / sourceHeight);
            patternInput = [scale_1, scale_1];
            break;
        }
        case 2: {
            patternInput = [targetWidth / sourceWidth, targetHeight / sourceHeight];
            break;
        }
        case 3: {
            const scale_2 = 24 / max(1, background.PixelsPerCell);
            patternInput = [scale_2, scale_2];
            break;
        }
        default: {
            const scale = min(targetWidth / sourceWidth, targetHeight / sourceHeight);
            patternInput = [scale, scale];
        }
    }
    const renderedWidth = sourceWidth * patternInput[0];
    const renderedHeight = sourceHeight * patternInput[1];
    let patternInput_1;
    const matchValue_2 = background.Fit;
    switch (matchValue_2.tag) {
        case 2:
        case 3: {
            patternInput_1 = [0, 0];
            break;
        }
        default:
            patternInput_1 = [(targetWidth - renderedWidth) / 2, (targetHeight - renderedHeight) / 2];
    }
    return [patternInput_1[0] + background.GridOffsetX, patternInput_1[1] + background.GridOffsetY, renderedWidth, renderedHeight];
}

function MapEditorWorkspace_finiteOr(fallback, value) {
    if (Number.isNaN(value) ? true : isInfinity(value)) {
        return fallback;
    }
    else {
        return value;
    }
}

function MapEditorWorkspace_clamp(minimum, maximum, value) {
    return max_1((x_1, y_1) => (compare(x_1, y_1) | 0), minimum, min_1((x, y) => (compare(x, y) | 0), maximum, value));
}

function MapEditorWorkspace_boundedZoom(value) {
    return MapEditorWorkspace_clamp(0.25, 6, MapEditorWorkspace_finiteOr(1, value));
}

export function MapEditorWorkspace_clientToViewportPoint(viewportWidth, viewportHeight, renderedWidth, renderedHeight, localX, localY) {
    const viewportWidth_1 = max(1, MapEditorWorkspace_finiteOr(960, viewportWidth));
    const viewportHeight_1 = max(1, MapEditorWorkspace_finiteOr(640, viewportHeight));
    const renderedWidth_1 = max(1, MapEditorWorkspace_finiteOr(viewportWidth_1, renderedWidth));
    const renderedHeight_1 = max(1, MapEditorWorkspace_finiteOr(viewportHeight_1, renderedHeight));
    const scale = max(1E-06, min(renderedWidth_1 / viewportWidth_1, renderedHeight_1 / viewportHeight_1));
    const offsetX = (renderedWidth_1 - (viewportWidth_1 * scale)) / 2;
    const offsetY = (renderedHeight_1 - (viewportHeight_1 * scale)) / 2;
    return [(MapEditorWorkspace_finiteOr(0, localX) - offsetX) / scale, (MapEditorWorkspace_finiteOr(0, localY) - offsetY) / scale];
}

export function MapEditorWorkspace_screenToBoard(camera, screenX, screenY) {
    return [(screenX - camera.PanX) / camera.Zoom, (screenY - camera.PanY) / camera.Zoom];
}

export function MapEditorWorkspace_boardToScreen(camera, boardX, boardY) {
    return [camera.PanX + (boardX * camera.Zoom), camera.PanY + (boardY * camera.Zoom)];
}

export function MapEditorWorkspace_zoomAt(screenX, screenY, factor, camera) {
    const patternInput = MapEditorWorkspace_screenToBoard(camera, screenX, screenY);
    const zoom = MapEditorWorkspace_boundedZoom(camera.Zoom * MapEditorWorkspace_finiteOr(1, factor));
    return new BattlefieldCamera(screenX - (patternInput[0] * zoom), screenY - (patternInput[1] * zoom), zoom);
}

export function MapEditorWorkspace_panBy(x, y, camera) {
    return new BattlefieldCamera(camera.PanX + MapEditorWorkspace_finiteOr(0, x), camera.PanY + MapEditorWorkspace_finiteOr(0, y), camera.Zoom);
}

export function MapEditorWorkspace_fitBounds(viewportWidth, viewportHeight, minimumX, minimumY, maximumX, maximumY) {
    const viewportWidth_1 = max(1, MapEditorWorkspace_finiteOr(960, viewportWidth));
    const viewportHeight_1 = max(1, MapEditorWorkspace_finiteOr(640, viewportHeight));
    const width = max(1, maximumX - minimumX);
    const height = max(1, maximumY - minimumY);
    const zoom = MapEditorWorkspace_boundedZoom(min(max(1, viewportWidth_1 - (36 * 2)) / width, max(1, viewportHeight_1 - (36 * 2)) / height));
    return new BattlefieldCamera(((viewportWidth_1 - (width * zoom)) / 2) - (minimumX * zoom), ((viewportHeight_1 - (height * zoom)) / 2) - (minimumY * zoom), zoom);
}

export function MapEditorWorkspace_fitBoard(viewportWidth, viewportHeight, boardWidth, boardHeight) {
    return MapEditorWorkspace_fitBounds(viewportWidth, viewportHeight, 0, 0, boardWidth * 24, boardHeight * 24);
}

export function MapEditorWorkspace_frameSelection(viewportWidth, viewportHeight, unit, fallback) {
    if (unit != null) {
        const unit_1 = unit;
        const inset = 24 * 0.35;
        const minimumX = (unit_1.Column * 24) - inset;
        const minimumY = (unit_1.Row * 24) - inset;
        const size = unit_1.Size * 24;
        return MapEditorWorkspace_fitBounds(viewportWidth, viewportHeight, minimumX, minimumY, (minimumX + size) + (inset * 2), (minimumY + size) + (inset * 2));
    }
    else {
        return fallback;
    }
}

export function MapEditorWorkspace_tryHitCell(width, height, camera, screenX, screenY) {
    const patternInput = MapEditorWorkspace_screenToBoard(camera, screenX, screenY);
    const column = ~~Math.floor(patternInput[0] / 24) | 0;
    const row = ~~Math.floor(patternInput[1] / 24) | 0;
    if ((((column >= 0) && (row >= 0)) && (column < width)) && (row < height)) {
        return new MapCellHit(column, row);
    }
    else {
        return undefined;
    }
}

export function MapEditorWorkspace_tryHitEdge(width, height, camera, tolerancePixels, screenX, screenY) {
    const patternInput = MapEditorWorkspace_screenToBoard(camera, screenX, screenY);
    const boardY = patternInput[1];
    const boardX = patternInput[0];
    const column = ~~Math.floor(boardX / 24) | 0;
    const row = ~~Math.floor(boardY / 24) | 0;
    const verticalBoundary = round(boardX / 24) * 24;
    const horizontalBoundary = round(boardY / 24) * 24;
    const verticalDistance = Math.abs(boardX - verticalBoundary) * camera.Zoom;
    const horizontalDistance = Math.abs(boardY - horizontalBoundary) * camera.Zoom;
    const tolerance = max(0, tolerancePixels);
    const verticalColumn = (~~round(boardX / 24) - 1) | 0;
    const horizontalRow = (~~round(boardY / 24) - 1) | 0;
    const vertical = (((((verticalDistance <= tolerance) && (verticalColumn >= 0)) && (verticalColumn < width)) && (row >= 0)) && (row < height)) ? (new MapEdgeHit(verticalColumn, row, MapEdgeDirection.EastEdge, verticalDistance)) : undefined;
    const horizontal = (((((horizontalDistance <= tolerance) && (column >= 0)) && (column < width)) && (horizontalRow >= 0)) && (horizontalRow < height)) ? (new MapEdgeHit(column, horizontalRow, MapEdgeDirection.SouthEdge, horizontalDistance)) : undefined;
    let matchResult, x_1, y_1, x_2, y_2;
    if (vertical != null) {
        if (horizontal != null) {
            if (horizontal.DistancePixels < vertical.DistancePixels) {
                matchResult = 0;
                x_1 = vertical;
                y_1 = horizontal;
            }
            else {
                matchResult = 1;
                x_2 = vertical;
            }
        }
        else {
            matchResult = 1;
            x_2 = vertical;
        }
    }
    else if (horizontal != null) {
        matchResult = 2;
        y_2 = horizontal;
    }
    else {
        matchResult = 3;
    }
    switch (matchResult) {
        case 0:
            return y_1;
        case 1:
            return x_2;
        case 2:
            return y_2;
        default:
            return undefined;
    }
}

function MapEditorWorkspace_touchPair(pointers) {
    let array_1;
    return truncate(2, sortBy((_arg) => (_arg.Id | 0), (array_1 = map_1((tuple) => tuple[1], toArray(pointers)), array_1.filter((pointer) => equals(pointer.Kind, EditorPointerKind.TouchPointer))), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    }));
}

function MapEditorWorkspace_applyTouchMove(previousPointers, nextPointer, camera) {
    const nextPointers = add(nextPointer.Id, nextPointer, previousPointers);
    const before = MapEditorWorkspace_touchPair(previousPointers);
    const after = MapEditorWorkspace_touchPair(nextPointers);
    if ((before.length === 2) && (after.length === 2)) {
        const midpoint = (pair) => [(item(0, pair).X + item(1, pair).X) / 2, (item(0, pair).Y + item(1, pair).Y) / 2];
        const distance = (pair_1) => {
            const dx = item(1, pair_1).X - item(0, pair_1).X;
            const dy = item(1, pair_1).Y - item(0, pair_1).Y;
            return Math.sqrt((dx * dx) + (dy * dy));
        };
        const patternInput = midpoint(before);
        const beforeY = patternInput[1];
        const beforeX = patternInput[0];
        const patternInput_1 = midpoint(after);
        const beforeDistance = distance(before);
        return MapEditorWorkspace_panBy(patternInput_1[0] - beforeX, patternInput_1[1] - beforeY, MapEditorWorkspace_zoomAt(beforeX, beforeY, (beforeDistance < 0.001) ? 1 : (distance(after) / beforeDistance), camera));
    }
    else {
        return camera;
    }
}

export function MapEditorWorkspace_update(map, selected, action, state) {
    let option_1, background_2;
    let matchResult, pointerId;
    switch (action.tag) {
        case 1: {
            matchResult = 1;
            break;
        }
        case 2: {
            matchResult = 2;
            break;
        }
        case 3: {
            matchResult = 3;
            break;
        }
        case 4: {
            matchResult = 4;
            break;
        }
        case 5: {
            matchResult = 5;
            break;
        }
        case 6: {
            matchResult = 6;
            break;
        }
        case 7: {
            matchResult = 7;
            break;
        }
        case 8: {
            matchResult = 8;
            break;
        }
        case 9: {
            matchResult = 9;
            break;
        }
        case 10: {
            matchResult = 10;
            pointerId = action.fields[0];
            break;
        }
        case 11: {
            matchResult = 10;
            pointerId = action.fields[0];
            break;
        }
        case 12: {
            matchResult = 11;
            break;
        }
        case 13: {
            matchResult = 12;
            break;
        }
        case 14: {
            matchResult = 13;
            break;
        }
        case 15: {
            matchResult = 14;
            break;
        }
        case 16: {
            matchResult = 15;
            break;
        }
        case 17: {
            matchResult = 16;
            break;
        }
        case 18: {
            matchResult = 17;
            break;
        }
        case 19: {
            matchResult = 18;
            break;
        }
        case 20: {
            matchResult = 19;
            break;
        }
        case 21: {
            matchResult = 20;
            break;
        }
        case 22: {
            matchResult = 21;
            break;
        }
        default:
            matchResult = 0;
    }
    switch (matchResult) {
        case 0:
            return new EditorWorkspaceState(state.Camera, max(1, MapEditorWorkspace_finiteOr(state.ViewportWidth, action.fields[0])), max(1, MapEditorWorkspace_finiteOr(state.ViewportHeight, action.fields[1])), state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, state.Background, state.BackgroundAnnouncement);
        case 1:
            return new EditorWorkspaceState(MapEditorWorkspace_panBy(action.fields[0], action.fields[1], state.Camera), state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, state.Background, state.BackgroundAnnouncement);
        case 2:
            return new EditorWorkspaceState(MapEditorWorkspace_zoomAt(action.fields[0], action.fields[1], action.fields[2], state.Camera), state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, state.Background, state.BackgroundAnnouncement);
        case 3:
            return new EditorWorkspaceState(MapEditorWorkspace_fitBoard(state.ViewportWidth, state.ViewportHeight, map.Width, map.Height), state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, state.Background, state.BackgroundAnnouncement);
        case 4:
            return new EditorWorkspaceState(MapEditorWorkspace_frameSelection(state.ViewportWidth, state.ViewportHeight, selected, state.Camera), state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, state.Background, state.BackgroundAnnouncement);
        case 5:
            return new EditorWorkspaceState(new BattlefieldCamera(36, 36, 1), state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, empty({
                Compare: (x_2, y_2) => (comparePrimitives(x_2, y_2) | 0),
            }), state.Background, state.BackgroundAnnouncement);
        case 6:
            return new EditorWorkspaceState(state.Camera, state.ViewportWidth, state.ViewportHeight, !state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, state.Background, state.BackgroundAnnouncement);
        case 7:
            return new EditorWorkspaceState(state.Camera, state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, action.fields[0], state.CapturedPointers, state.Background, state.BackgroundAnnouncement);
        case 8: {
            const pointer = action.fields[0];
            return new EditorWorkspaceState(state.Camera, state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, add(pointer.Id, pointer, state.CapturedPointers), state.Background, state.BackgroundAnnouncement);
        }
        case 9: {
            const pointer_1 = action.fields[0];
            const matchValue = tryFind(pointer_1.Id, state.CapturedPointers);
            if (matchValue != null) {
                const previous = matchValue;
                return new EditorWorkspaceState(equals(pointer_1.Kind, EditorPointerKind.TouchPointer) ? MapEditorWorkspace_applyTouchMove(state.CapturedPointers, pointer_1, state.Camera) : (previous.RequestsPan ? MapEditorWorkspace_panBy(pointer_1.X - previous.X, pointer_1.Y - previous.Y, state.Camera) : state.Camera), state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, add(pointer_1.Id, pointer_1, state.CapturedPointers), state.Background, state.BackgroundAnnouncement);
            }
            else {
                return state;
            }
        }
        case 10:
            return new EditorWorkspaceState(state.Camera, state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, remove(pointerId, state.CapturedPointers), state.Background, state.BackgroundAnnouncement);
        case 11:
            return new EditorWorkspaceState(state.Camera, state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, empty({
                Compare: (x_3, y_3) => (comparePrimitives(x_3, y_3) | 0),
            }), state.Background, state.BackgroundAnnouncement);
        case 12: {
            const matchValue_1 = MapEditorWorkspace_tryCreateLocalRaster(action.fields[0], action.fields[1], action.fields[2]);
            if (matchValue_1.tag === 0) {
                const background = matchValue_1.fields[0];
                return new EditorWorkspaceState(state.Camera, state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, background, ((((("Local " + background.MediaType) + " background accepted: ") + int32ToString(background.PixelWidth)) + " by ") + int32ToString(background.PixelHeight)) + " pixels, locked.");
            }
            else {
                return new EditorWorkspaceState(state.Camera, state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, state.Background, matchValue_1.fields[0]);
            }
        }
        case 13:
            return new EditorWorkspaceState(state.Camera, state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, undefined, "Local background removed.");
        case 14: {
            const matchValue_2 = state.Background;
            if (matchValue_2 != null) {
                const background_1 = matchValue_2;
                const next = new LocalRasterBackground(background_1.AssetId, background_1.FileName, background_1.MediaType, background_1.PixelWidth, background_1.PixelHeight, background_1.ByteLength, background_1.DataUrl, !background_1.Locked, background_1.Opacity, background_1.Fit, background_1.Crop, background_1.GridOffsetX, background_1.GridOffsetY, background_1.PixelsPerCell);
                return new EditorWorkspaceState(state.Camera, state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, next, next.Locked ? "Background locked." : "Background unlocked.");
            }
            else {
                return state;
            }
        }
        case 15:
            return new EditorWorkspaceState(state.Camera, state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, (option_1 = state.Background, (option_1 != null) ? ((background_2 = option_1, new LocalRasterBackground(background_2.AssetId, background_2.FileName, background_2.MediaType, background_2.PixelWidth, background_2.PixelHeight, background_2.ByteLength, background_2.DataUrl, background_2.Locked, MapEditorWorkspace_clamp(0, 1, MapEditorWorkspace_finiteOr(background_2.Opacity, action.fields[0])), background_2.Fit, background_2.Crop, background_2.GridOffsetX, background_2.GridOffsetY, background_2.PixelsPerCell))) : undefined), state.BackgroundAnnouncement);
        case 16: {
            const matchValue_3 = state.Background;
            if (matchValue_3 == null) {
                return state;
            }
            else if (!matchValue_3.Locked) {
                const background_4 = matchValue_3;
                return new EditorWorkspaceState(state.Camera, state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, new LocalRasterBackground(background_4.AssetId, background_4.FileName, background_4.MediaType, background_4.PixelWidth, background_4.PixelHeight, background_4.ByteLength, background_4.DataUrl, background_4.Locked, background_4.Opacity, action.fields[0], background_4.Crop, background_4.GridOffsetX, background_4.GridOffsetY, background_4.PixelsPerCell), state.BackgroundAnnouncement);
            }
            else {
                return new EditorWorkspaceState(state.Camera, state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, state.Background, "Unlock the background before changing its fit.");
            }
        }
        case 17: {
            const crop = action.fields[0];
            const matchValue_4 = state.Background;
            if (matchValue_4 == null) {
                return state;
            }
            else if (!matchValue_4.Locked) {
                const background_6 = matchValue_4;
                if (!forAll((value_1) => {
                    if (((((value_1.Left >= 0) && (value_1.Top >= 0)) && (value_1.Width > 0)) && (value_1.Height > 0)) && ((value_1.Left + value_1.Width) <= background_6.PixelWidth)) {
                        return (value_1.Top + value_1.Height) <= background_6.PixelHeight;
                    }
                    else {
                        return false;
                    }
                }, toArray_1(crop))) {
                    return new EditorWorkspaceState(state.Camera, state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, state.Background, "BACKGROUND-CROP: crop bounds must stay within the raster.");
                }
                else {
                    return new EditorWorkspaceState(state.Camera, state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, new LocalRasterBackground(background_6.AssetId, background_6.FileName, background_6.MediaType, background_6.PixelWidth, background_6.PixelHeight, background_6.ByteLength, background_6.DataUrl, background_6.Locked, background_6.Opacity, background_6.Fit, crop, background_6.GridOffsetX, background_6.GridOffsetY, background_6.PixelsPerCell), state.BackgroundAnnouncement);
                }
            }
            else {
                return new EditorWorkspaceState(state.Camera, state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, state.Background, "Unlock the background before cropping it.");
            }
        }
        case 18: {
            const matchValue_5 = state.Background;
            if (matchValue_5 == null) {
                return state;
            }
            else if (!matchValue_5.Locked) {
                const background_8 = matchValue_5;
                return new EditorWorkspaceState(state.Camera, state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, new LocalRasterBackground(background_8.AssetId, background_8.FileName, background_8.MediaType, background_8.PixelWidth, background_8.PixelHeight, background_8.ByteLength, background_8.DataUrl, background_8.Locked, background_8.Opacity, background_8.Fit, background_8.Crop, MapEditorWorkspace_finiteOr(0, action.fields[0]), MapEditorWorkspace_finiteOr(0, action.fields[1]), background_8.PixelsPerCell), state.BackgroundAnnouncement);
            }
            else {
                return new EditorWorkspaceState(state.Camera, state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, state.Background, "Unlock the background before moving it.");
            }
        }
        case 19: {
            const matchValue_6 = state.Background;
            if (matchValue_6 == null) {
                return state;
            }
            else if (!matchValue_6.Locked) {
                const background_10 = matchValue_6;
                return new EditorWorkspaceState(state.Camera, state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, new LocalRasterBackground(background_10.AssetId, background_10.FileName, background_10.MediaType, background_10.PixelWidth, background_10.PixelHeight, background_10.ByteLength, background_10.DataUrl, background_10.Locked, background_10.Opacity, background_10.Fit, background_10.Crop, background_10.GridOffsetX + MapEditorWorkspace_finiteOr(0, action.fields[0]), background_10.GridOffsetY + MapEditorWorkspace_finiteOr(0, action.fields[1]), background_10.PixelsPerCell), state.BackgroundAnnouncement);
            }
            else {
                return new EditorWorkspaceState(state.Camera, state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, state.Background, "Unlock the background before moving it.");
            }
        }
        case 20: {
            const pixels = action.fields[0];
            const matchValue_7 = state.Background;
            if (matchValue_7 == null) {
                return state;
            }
            else if (!matchValue_7.Locked) {
                const background_12 = matchValue_7;
                if ((pixels < 1) ? true : (pixels > 8192)) {
                    return new EditorWorkspaceState(state.Camera, state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, state.Background, "BACKGROUND-GRID-SCALE: pixels per cell must be within the supported raster dimensions.");
                }
                else {
                    return new EditorWorkspaceState(state.Camera, state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, new LocalRasterBackground(background_12.AssetId, background_12.FileName, background_12.MediaType, background_12.PixelWidth, background_12.PixelHeight, background_12.ByteLength, background_12.DataUrl, background_12.Locked, background_12.Opacity, background_12.Fit, background_12.Crop, background_12.GridOffsetX, background_12.GridOffsetY, pixels), state.BackgroundAnnouncement);
                }
            }
            else {
                return new EditorWorkspaceState(state.Camera, state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, state.Background, "Unlock the background before changing its scale.");
            }
        }
        default: {
            const secondY = action.fields[3];
            const secondX = action.fields[2];
            const firstY = action.fields[1];
            const firstX = action.fields[0];
            const cellsBetween = action.fields[4] | 0;
            const matchValue_8 = state.Background;
            if (matchValue_8 == null) {
                return state;
            }
            else if (!matchValue_8.Locked && (cellsBetween > 0)) {
                const background_14 = matchValue_8;
                const horizontal = Math.abs(secondY - firstY) < 1E-06;
                const vertical = Math.abs(secondX - firstX) < 1E-06;
                const pixelsPerCell = (horizontal ? Math.abs(secondX - firstX) : (vertical ? Math.abs(secondY - firstY) : 0)) / cellsBetween;
                if (!horizontal && !vertical) {
                    const BackgroundAnnouncement_10 = "BACKGROUND-ALIGNMENT: rotated source grids are not supported; choose two points on one horizontal or vertical grid line.";
                    return new EditorWorkspaceState(state.Camera, state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, state.Background, BackgroundAnnouncement_10);
                }
                else if (pixelsPerCell < 1) {
                    return new EditorWorkspaceState(state.Camera, state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, state.Background, "BACKGROUND-ALIGNMENT: alignment points must span at least one pixel per cell.");
                }
                else {
                    return new EditorWorkspaceState(state.Camera, state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, new LocalRasterBackground(background_14.AssetId, background_14.FileName, background_14.MediaType, background_14.PixelWidth, background_14.PixelHeight, background_14.ByteLength, background_14.DataUrl, background_14.Locked, background_14.Opacity, BackgroundFit.NativePixels, background_14.Crop, (-firstX * 24) / pixelsPerCell, (-firstY * 24) / pixelsPerCell, pixelsPerCell), ("Background grid aligned at " + pixelsPerCell.toString()) + " pixels per cell.");
                }
            }
            else {
                return new EditorWorkspaceState(state.Camera, state.ViewportWidth, state.ViewportHeight, state.InspectorCollapsed, state.ReducedMotion, state.CapturedPointers, state.Background, "Unlock the background before aligning it.");
            }
        }
    }
}

