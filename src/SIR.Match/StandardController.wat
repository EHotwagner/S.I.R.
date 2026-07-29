(module
  ;; Readable reference implementation of the SIR standard plan controller.
  ;; It imports nothing. The host copies immutable SIR-PCFG 1 bytes into the
  ;; explicit configuration buffer once; every tick subsequently uses only the
  ;; public Control ABI input and output buffers.
  (memory (export "memory") 2 2)
  (global $output_length (export "output_length") (mut i32) (i32.const 44))

  (func (export "sir_abi_version") (result i32) i32.const 65536)
  (func (export "sir_input_ptr") (result i32) i32.const 0)
  (func (export "sir_input_capacity") (result i32) i32.const 65536)
  (func (export "sir_output_ptr") (result i32) i32.const 65536)
  (func (export "sir_output_capacity") (result i32) i32.const 16384)
  (func (export "sir_configuration_ptr") (result i32) i32.const 81920)
  (func (export "sir_configuration_capacity") (result i32) i32.const 4096)

  (func $copy (param $source i32) (param $target i32) (param $length i32)
    (local $index i32)
    (block $done
      (loop $next
        local.get $index local.get $length i32.ge_u br_if $done
        local.get $target local.get $index i32.add
        local.get $source local.get $index i32.add i32.load8_u
        i32.store8
        local.get $index i32.const 1 i32.add local.set $index
        br $next)))

  (func (export "sir_decide") (param $input_length i32) (result i32)
    (local $tick i32)
    (local $count i32)
    (local $index i32)
    (local $cursor i32)
    (local $record_tick i32)
    (local $length i32)

    ;; Begin with a canonical empty SIRO envelope (including its required,
    ;; zero-element output-request section) and copy invocation identity.
    i32.const 65536 i32.const 0x4f524953 i32.store
    i32.const 65540 i32.const 0x00200001 i32.store
    i32.const 65544 i32.const 44 i32.store
    i32.const 65548 i32.const 12 i32.load i32.store
    i32.const 65552 i32.const 16 i32.load i32.store
    i32.const 65556 i32.const 0 i32.store
    i32.const 65560 i32.const 24 i32.load i32.store
    i32.const 65564 i32.const 1 i32.store
    i32.const 65568 i32.const 0x00011001 i32.store
    i32.const 65572 i32.const 0 i32.store
    i32.const 65576 i32.const 0 i32.store
    i32.const 44 global.set $output_length

    i32.const 12 i32.load local.set $tick
    i32.const 81928 i32.load local.set $count
    i32.const 81932 local.set $cursor

    (block $done
      (loop $record
        local.get $index local.get $count i32.ge_u br_if $done
        local.get $cursor i32.load local.set $record_tick
        local.get $cursor i32.const 4 i32.add i32.load local.set $length
        local.get $record_tick local.get $tick i32.eq
        (if
          (then
            local.get $cursor i32.const 8 i32.add
            i32.const 65536
            local.get $length
            call $copy
            local.get $length global.set $output_length
            br $done))
        local.get $cursor i32.const 8 i32.add local.get $length i32.add
        local.set $cursor
        local.get $index i32.const 1 i32.add local.set $index
        br $record))
    global.get $output_length))
