

export function hasSupportedDimensions(map) {
    if (((map.Width >= 4) && (map.Width <= 40)) && (map.Height >= 4)) {
        return map.Height <= 40;
    }
    else {
        return false;
    }
}

