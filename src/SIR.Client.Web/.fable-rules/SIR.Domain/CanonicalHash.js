
import { concatenate } from "./CanonicalEncoding.js";
import { disposeSafe, getEnumerator } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { rangeDouble } from "../fable_modules/fable-library-js.5.13.0/Range.js";
import { collect, setItem, item } from "../fable_modules/fable-library-js.5.13.0/Array.js";

const roundConstants = new Uint32Array([1116352408, 1899447441, 3049323471, 3921009573, 961987163, 1508970993, 2453635748, 2870763221, 3624381080, 310598401, 607225278, 1426881987, 1925078388, 2162078206, 2614888103, 3248222580, 3835390401, 4022224774, 264347078, 604807628, 770255983, 1249150122, 1555081692, 1996064986, 2554220882, 2821834349, 2952996808, 3210313671, 3336571891, 3584528711, 113926993, 338241895, 666307205, 773529912, 1294757372, 1396182291, 1695183700, 1986661051, 2177026350, 2456956037, 2730485921, 2820302411, 3259730800, 3345764771, 3516065817, 3600352804, 4094571909, 275423344, 430227734, 506948616, 659060556, 883997877, 958139571, 1322822218, 1537002063, 1747873779, 1955562222, 2024104815, 2227730452, 2361852424, 2428436474, 2756734187, 3204031479, 3329325298]);

function rotateRight(count, value) {
    return ((value >>> count) | ((value << (32 - count)) >>> 0)) >>> 0;
}

function bigEndian(value) {
    return new Uint8Array([(value >>> 24) & 0xFF, (value >>> 16) & 0xFF, (value >>> 8) & 0xFF, value & 0xFF]);
}

/**
 * Computes the 32-byte SHA-256 digest of canonical bytes.
 */
export function sha256(bytes) {
    const byteLength = bytes.length >>> 0;
    const bitLengthHigh = byteLength >>> 29;
    const bitLengthLow = (byteLength << 3) >>> 0;
    const padded = concatenate([bytes, new Uint8Array([128]), new Uint8Array(((56 - ((bytes.length + 1) % 64)) + 64) % 64), bigEndian(bitLengthHigh), bigEndian(bitLengthLow)]);
    const hash = new Uint32Array([1779033703, 3144134277, 1013904242, 2773480762, 1359893119, 2600822924, 528734635, 1541459225]);
    const enumerator = getEnumerator(rangeDouble(0, 64, padded.length - 64));
    try {
        while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
            const blockStart = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]() | 0;
            const schedule = new Uint32Array(64);
            for (let index = 0; index <= 15; index++) {
                const offset = (blockStart + (index * 4)) | 0;
                setItem(schedule, index, (((((((item(offset, padded) << 24) >>> 0) | ((item(offset + 1, padded) << 16) >>> 0)) >>> 0) | ((item(offset + 2, padded) << 8) >>> 0)) >>> 0) | item(offset + 3, padded)) >>> 0);
            }
            for (let index_1 = 16; index_1 <= 63; index_1++) {
                const previous15 = item(index_1 - 15, schedule);
                const previous2 = item(index_1 - 2, schedule);
                const sigma0 = (rotateRight(7, previous15) ^ ((rotateRight(18, previous15) ^ (previous15 >>> 3)) >>> 0)) >>> 0;
                const sigma1 = (rotateRight(17, previous2) ^ ((rotateRight(19, previous2) ^ (previous2 >>> 10)) >>> 0)) >>> 0;
                setItem(schedule, index_1, ((item(index_1 - 16, schedule) + sigma0) + item(index_1 - 7, schedule)) + sigma1);
            }
            let a = item(0, hash);
            let b = item(1, hash);
            let c = item(2, hash);
            let d = item(3, hash);
            let e = item(4, hash);
            let f = item(5, hash);
            let g = item(6, hash);
            let h = item(7, hash);
            for (let index_2 = 0; index_2 <= 63; index_2++) {
                const choice = (((e & f) >>> 0) ^ (((~e >>> 0) & g) >>> 0)) >>> 0;
                const sum1 = (rotateRight(6, e) ^ ((rotateRight(11, e) ^ rotateRight(25, e)) >>> 0)) >>> 0;
                const temporary1 = (((h + sum1) + choice) + item(index_2, roundConstants)) + item(index_2, schedule);
                const majority = (((a & b) >>> 0) ^ ((((a & c) >>> 0) ^ ((b & c) >>> 0)) >>> 0)) >>> 0;
                const temporary2 = ((rotateRight(2, a) ^ ((rotateRight(13, a) ^ rotateRight(22, a)) >>> 0)) >>> 0) + majority;
                h = g;
                g = f;
                f = e;
                e = (d + temporary1);
                d = c;
                c = b;
                b = a;
                a = (temporary1 + temporary2);
            }
            setItem(hash, 0, item(0, hash) + a);
            setItem(hash, 1, item(1, hash) + b);
            setItem(hash, 2, item(2, hash) + c);
            setItem(hash, 3, item(3, hash) + d);
            setItem(hash, 4, item(4, hash) + e);
            setItem(hash, 5, item(5, hash) + f);
            setItem(hash, 6, item(6, hash) + g);
            setItem(hash, 7, item(7, hash) + h);
        }
    }
    finally {
        disposeSafe(enumerator);
    }
    return collect(bigEndian, hash, Uint8Array);
}

