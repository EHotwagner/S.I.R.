namespace SIR.Domain

/// Portable SHA-256 for canonical replay identities in .NET and Fable.
[<RequireQualifiedAccess>]
module CanonicalHash =
    let private roundConstants =
        [| 0x428a2f98u; 0x71374491u; 0xb5c0fbcfu; 0xe9b5dba5u
           0x3956c25bu; 0x59f111f1u; 0x923f82a4u; 0xab1c5ed5u
           0xd807aa98u; 0x12835b01u; 0x243185beu; 0x550c7dc3u
           0x72be5d74u; 0x80deb1feu; 0x9bdc06a7u; 0xc19bf174u
           0xe49b69c1u; 0xefbe4786u; 0x0fc19dc6u; 0x240ca1ccu
           0x2de92c6fu; 0x4a7484aau; 0x5cb0a9dcu; 0x76f988dau
           0x983e5152u; 0xa831c66du; 0xb00327c8u; 0xbf597fc7u
           0xc6e00bf3u; 0xd5a79147u; 0x06ca6351u; 0x14292967u
           0x27b70a85u; 0x2e1b2138u; 0x4d2c6dfcu; 0x53380d13u
           0x650a7354u; 0x766a0abbu; 0x81c2c92eu; 0x92722c85u
           0xa2bfe8a1u; 0xa81a664bu; 0xc24b8b70u; 0xc76c51a3u
           0xd192e819u; 0xd6990624u; 0xf40e3585u; 0x106aa070u
           0x19a4c116u; 0x1e376c08u; 0x2748774cu; 0x34b0bcb5u
           0x391c0cb3u; 0x4ed8aa4au; 0x5b9cca4fu; 0x682e6ff3u
           0x748f82eeu; 0x78a5636fu; 0x84c87814u; 0x8cc70208u
           0x90befffau; 0xa4506cebu; 0xbef9a3f7u; 0xc67178f2u |]

    let private rotateRight count value =
        (value >>> count) ||| (value <<< (32 - count))

    let private bigEndian value =
        [| byte (value >>> 24)
           byte (value >>> 16)
           byte (value >>> 8)
           byte value |]

    /// Computes the 32-byte SHA-256 digest of canonical bytes.
    let sha256 (bytes: byte array) =
        let byteLength = uint32 bytes.Length
        let bitLengthHigh = byteLength >>> 29
        let bitLengthLow = byteLength <<< 3
        let zeroCount = (56 - ((bytes.Length + 1) % 64) + 64) % 64

        let padded =
            CanonicalEncoding.concatenate
                [ bytes
                  [| 0x80uy |]
                  Array.zeroCreate zeroCount
                  bigEndian bitLengthHigh
                  bigEndian bitLengthLow ]

        let hash =
            [| 0x6a09e667u; 0xbb67ae85u; 0x3c6ef372u; 0xa54ff53au
               0x510e527fu; 0x9b05688cu; 0x1f83d9abu; 0x5be0cd19u |]

        for blockStart in 0 .. 64 .. padded.Length - 64 do
            let schedule = Array.zeroCreate<uint32> 64

            for index in 0 .. 15 do
                let offset = blockStart + index * 4

                schedule[index] <-
                    (uint32 padded[offset] <<< 24)
                    ||| (uint32 padded[offset + 1] <<< 16)
                    ||| (uint32 padded[offset + 2] <<< 8)
                    ||| uint32 padded[offset + 3]

            for index in 16 .. 63 do
                let previous15 = schedule[index - 15]
                let previous2 = schedule[index - 2]

                let sigma0 =
                    rotateRight 7 previous15
                    ^^^ rotateRight 18 previous15
                    ^^^ (previous15 >>> 3)

                let sigma1 =
                    rotateRight 17 previous2
                    ^^^ rotateRight 19 previous2
                    ^^^ (previous2 >>> 10)

                schedule[index] <-
                    schedule[index - 16]
                    + sigma0
                    + schedule[index - 7]
                    + sigma1

            let mutable a = hash[0]
            let mutable b = hash[1]
            let mutable c = hash[2]
            let mutable d = hash[3]
            let mutable e = hash[4]
            let mutable f = hash[5]
            let mutable g = hash[6]
            let mutable h = hash[7]

            for index in 0 .. 63 do
                let choice = (e &&& f) ^^^ ((~~~e) &&& g)

                let sum1 =
                    rotateRight 6 e
                    ^^^ rotateRight 11 e
                    ^^^ rotateRight 25 e

                let temporary1 =
                    h + sum1 + choice + roundConstants[index] + schedule[index]

                let majority = (a &&& b) ^^^ (a &&& c) ^^^ (b &&& c)

                let sum0 =
                    rotateRight 2 a
                    ^^^ rotateRight 13 a
                    ^^^ rotateRight 22 a

                let temporary2 = sum0 + majority
                h <- g
                g <- f
                f <- e
                e <- d + temporary1
                d <- c
                c <- b
                b <- a
                a <- temporary1 + temporary2

            hash[0] <- hash[0] + a
            hash[1] <- hash[1] + b
            hash[2] <- hash[2] + c
            hash[3] <- hash[3] + d
            hash[4] <- hash[4] + e
            hash[5] <- hash[5] + f
            hash[6] <- hash[6] + g
            hash[7] <- hash[7] + h

        hash |> Array.collect bigEndian
