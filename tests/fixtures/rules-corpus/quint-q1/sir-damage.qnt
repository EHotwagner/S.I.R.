module SirDamageSlice {
  type ActionEntry = { id: str, argument: str, reads: Set[str], writes: Set[str] }
  type PropertyEntry = { id: str, kind: str }
  type Observation = { hitPoints: int, lastAction: str, lastAmount: int }

  pure val actions = Set(
    { id: "Initialize", argument: "none", reads: Set(), writes: Set("HitPoints", "LastAction", "LastAmount") },
    { id: "ApplyDamage", argument: "amount:int", reads: Set("HitPoints", "Amount"), writes: Set("HitPoints", "LastAction", "LastAmount") }
  )
  pure val propertyCatalogue = Set(
    { id: "NonNegativeHitPoints", kind: "invariant" },
    { id: "KnownLastAction", kind: "invariant" },
    { id: "DamageCanReachZero", kind: "reachability" }
  )

  pure def clampAtZero(value: int): int = if (value < 0) 0 else value

  var hitPoints: int
  var lastAction: str
  var lastAmount: int

  val observation: Observation = {
    hitPoints: hitPoints,
    lastAction: lastAction,
    lastAmount: lastAmount,
  }

  action init = all {
    hitPoints' = 10,
    lastAction' = "Initialize",
    lastAmount' = 0,
  }

  action applyDamage(amount: int): bool = all {
    amount >= 0,
    hitPoints' = clampAtZero(hitPoints - amount),
    lastAction' = "ApplyDamage",
    lastAmount' = amount,
  }

  action step = {
    nondet amount = 0.to(15).oneOf()
    applyDamage(amount)
  }

  val nonNegativeHitPoints = hitPoints >= 0
  val knownLastAction = actions.exists(a => a.id == lastAction)
  val damageCanReachZero = not(hitPoints == 0)
}
module SirDamageSliceTests {
  import SirDamageSlice.*

  run reviewedWitness =
    init
      .expect(observation == { hitPoints: 10, lastAction: "Initialize", lastAmount: 0 })
      .then(applyDamage(3))
      .expect(observation == { hitPoints: 7, lastAction: "ApplyDamage", lastAmount: 3 })
      .then(applyDamage(20))
      .expect(and {
        observation == { hitPoints: 0, lastAction: "ApplyDamage", lastAmount: 20 },
        nonNegativeHitPoints,
        knownLastAction,
      })
}
