module Wat

/// Guest memory layout used by every module here.
///
///   input area (host writes before each invocation)
///     0   i32  tick
///     4   i32  own hp
///     8   i32  own ammunition
///    12   i32  own x
///    16   i32  own y
///    20   i32  own facing
///    24   i32  own action state
///    28   i32  contact count
///    32   contacts, 32 bytes each:
///            +0  id          +4  x           +8  y        +12 classification
///           +16  threat     +20  age ticks  +24 flags     +28 reserved
///
///   output area (guest writes, host reads)
///  4096   i32  chosen target id
///  4100   i32  action code
///  4104   i32  score
///  4108   i32  contacts examined
///
///   instance-private state (proves isolation)
///  8192   i32  invocation counter
[<Literal>]
let InputBase = 0

[<Literal>]
let ContactBase = 32

[<Literal>]
let ContactStride = 32

[<Literal>]
let ContactCountOffset = 28

[<Literal>]
let OutputBase = 4096

[<Literal>]
let StateCounterOffset = 8192

/// Minimum possible guest: measures pure host-to-guest transition cost.
let trivial =
    """
(module
  (memory (export "memory") 1 1)
  (func (export "tick") (param $work i32) (result i32)
    (i32.const 0)))
"""

/// Representative control logic: scan known contacts, score each by threat,
/// Chebyshev distance and staleness, select the best, write a decision.
/// `work` repeats the scan to model heavier doctrine without changing shape.
let representative =
    """
(module
  (memory (export "memory") 1 1)
  (func (export "tick") (param $work i32) (result i32)
    (local $n i32) (local $i i32) (local $w i32) (local $base i32)
    (local $ox i32) (local $oy i32) (local $cx i32) (local $cy i32)
    (local $dx i32) (local $dy i32) (local $dist i32)
    (local $threat i32) (local $age i32) (local $score i32)
    (local $best i32) (local $bestId i32) (local $seen i32)

    (local.set $ox (i32.load (i32.const 12)))
    (local.set $oy (i32.load (i32.const 16)))
    (local.set $n  (i32.load (i32.const 28)))
    (local.set $best (i32.const -2147483647))
    (local.set $bestId (i32.const -1))

    (block $wdone
      (loop $wloop
        (br_if $wdone (i32.ge_s (local.get $w) (local.get $work)))
        (local.set $i (i32.const 0))
        (block $done
          (loop $scan
            (br_if $done (i32.ge_s (local.get $i) (local.get $n)))
            (local.set $base
              (i32.add (i32.const 32) (i32.mul (local.get $i) (i32.const 32))))
            (local.set $cx (i32.load (i32.add (local.get $base) (i32.const 4))))
            (local.set $cy (i32.load (i32.add (local.get $base) (i32.const 8))))
            (local.set $threat (i32.load (i32.add (local.get $base) (i32.const 16))))
            (local.set $age (i32.load (i32.add (local.get $base) (i32.const 20))))

            (local.set $dx (i32.sub (local.get $cx) (local.get $ox)))
            (local.set $dx
              (select (local.get $dx) (i32.sub (i32.const 0) (local.get $dx))
                      (i32.ge_s (local.get $dx) (i32.const 0))))
            (local.set $dy (i32.sub (local.get $cy) (local.get $oy)))
            (local.set $dy
              (select (local.get $dy) (i32.sub (i32.const 0) (local.get $dy))
                      (i32.ge_s (local.get $dy) (i32.const 0))))
            (local.set $dist
              (select (local.get $dx) (local.get $dy)
                      (i32.ge_s (local.get $dx) (local.get $dy))))

            (local.set $score
              (i32.sub
                (i32.sub (i32.mul (local.get $threat) (i32.const 1000))
                         (i32.mul (local.get $dist) (i32.const 4)))
                (i32.mul (local.get $age) (i32.const 2))))

            (if (i32.gt_s (local.get $score) (local.get $best))
              (then
                (local.set $best (local.get $score))
                (local.set $bestId (i32.load (local.get $base)))))

            (local.set $seen (i32.add (local.get $seen) (i32.const 1)))
            (local.set $i (i32.add (local.get $i) (i32.const 1)))
            (br $scan)))
        (local.set $w (i32.add (local.get $w) (i32.const 1)))
        (br $wloop)))

    (i32.store (i32.const 4096) (local.get $bestId))
    (i32.store (i32.const 4100) (i32.const 1))
    (i32.store (i32.const 4104) (local.get $best))
    (i32.store (i32.const 4108) (local.get $seen))
    (local.get $bestId)))
"""

/// Unbounded-ish spin used to force fuel exhaustion deterministically.
let fuelBurner =
    """
(module
  (memory (export "memory") 1 1)
  (func (export "tick") (param $n i32) (result i32)
    (local $i i32)
    (block $d
      (loop $l
        (br_if $d (i32.ge_s (local.get $i) (local.get $n)))
        (i32.store (i32.const 4096) (local.get $i))
        (local.set $i (i32.add (local.get $i) (i32.const 1)))
        (br $l)))
    (local.get $i)))
"""

/// Increments a counter in its own linear memory. Two instances of this
/// module must never observe one another's count.
let statefulCounter =
    """
(module
  (memory (export "memory") 1 1)
  (func (export "tick") (param $work i32) (result i32)
    (i32.store (i32.const 8192)
      (i32.add (i32.load (i32.const 8192)) (i32.const 1)))
    (i32.store (i32.const 4096) (i32.load (i32.const 8192)))
    (i32.load (i32.const 8192))))
"""
