
import { isNullOrWhiteSpace } from "../fable_modules/fable-library-js.5.13.0/String.js";

/**
 * Produces the canonical registry representation for a keyboard gesture.
 */
export function canonicalGesture(gesture) {
    if (isNullOrWhiteSpace(gesture)) {
        return "";
    }
    else {
        return gesture.trim().toUpperCase();
    }
}

