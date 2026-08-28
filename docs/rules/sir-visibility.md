---
title: Grid Visibility in Quint
category: Rules
categoryindex: 2
index: 49
description: A bounded executable Quint translation of S.I.R.'s grid supercover line-of-sight mechanism.
---

# Grid Visibility in Quint

This literate source translates the discrete visibility path used by
`SIR.Simulation.SpatialQuery`: canonical endpoint ordering, integer supercover
traversal, endpoint exemption, vision-blocking terrain and edges, footprint-pair
aggregation, and the declared crossed-item work bound. It deliberately excludes
continuous visibility polygons, symmetric-shadowcasting field of view, caching,
wire encoding, and hidden-knowledge projection.

The fences concatenate in order into `sir-visibility.qnt`. The implementation
correspondence is:

- `FS.GG.Game.Core.Los.supercover` and `Los.lineOfSightBy Supercover` own the
  integer traversal and symmetric point-to-point predicate;
- `SIR.Simulation.SpatialQuery.observedTrace` owns footprint pairs, disclosed
  terrain and boundary permeability, visibility aggregation, and exhaustion;
- this Quint model owns a bounded, reviewable statement of those observable
  semantics, not production performance or floating-point visibility polygons.

```quint sir-visibility.qnt +=
module SirVisibility {
  type Cell = { col: int, row: int }
  type Edge = { lo: Cell, hi: Cell }
  type CellPair = { source: Cell, target: Cell }

  type SupercoverState = {
    current: Cell,
    ix: int,
    iy: int,
    nx: int,
    ny: int,
    sx: int,
    sy: int,
    cells: List[Cell],
  }

  type VisibilityQuery = {
    origin: Cell,
    target: Cell,
    footprint: Set[Cell],
    opaqueCells: Set[Cell],
    opaqueEdges: Set[Edge],
    maximumCrossedItems: int,
  }

  type VisibilityResult = {
    visible: bool,
    visibleSamples: int,
    totalSamples: int,
    declaredWork: int,
    supercoverWork: int,
    truncated: bool,
  }

  pure def absolute(value: int): int = if (value < 0) -value else value

  pure def maximum(left: int, right: int): int = if (left > right) left else right

  pure def cellLeq(left: Cell, right: Cell): bool =
    left.col < right.col or (left.col == right.col and left.row <= right.row)

  pure def canonicalPair(left: Cell, right: Cell): CellPair =
    if (cellLeq(left, right)) { source: left, target: right }
    else { source: right, target: left }

  pure def canonicalEdge(left: Cell, right: Cell): Edge = {
    val pair = canonicalPair(left, right)
    { lo: pair.source, hi: pair.target }
  }

  pure def addCell(anchor: Cell, offset: Cell): Cell =
    { col: anchor.col + offset.col, row: anchor.row + offset.row }

  pure def initialSupercover(left: Cell, right: Cell): SupercoverState = {
    val pair = canonicalPair(left, right)
    val dx = pair.target.col - pair.source.col
    val dy = pair.target.row - pair.source.row
    {
      current: pair.source,
      ix: 0,
      iy: 0,
      nx: absolute(dx),
      ny: absolute(dy),
      sx: if (dx > 0) 1 else -1,
      sy: if (dy > 0) 1 else -1,
      cells: List(pair.source),
    }
  }

  pure def advanceSupercover(state: SupercoverState, _step: int): SupercoverState = {
    val comparison = (1 + 2 * state.ix) * state.ny - (1 + 2 * state.iy) * state.nx
    val stepX = state.iy >= state.ny or (state.ix < state.nx and comparison <= 0)
    val nextCell =
      if (stepX)
        { col: state.current.col + state.sx, row: state.current.row }
      else
        { col: state.current.col, row: state.current.row + state.sy }
    {
      ...state,
      current: nextCell,
      ix: if (stepX) state.ix + 1 else state.ix,
      iy: if (stepX) state.iy else state.iy + 1,
      cells: state.cells.append(nextCell),
    }
  }

  pure def supercover(left: Cell, right: Cell): List[Cell] = {
    val initial = initialSupercover(left, right)
    range(0, initial.nx + initial.ny)
      .foldl(initial, (state, step) => advanceSupercover(state, step))
      .cells
  }

  pure def sampledLine(left: Cell, right: Cell): List[Cell] = {
    val deltaCol = right.col - left.col
    val deltaRow = right.row - left.row
    val steps = maximum(absolute(deltaCol), absolute(deltaRow))
    if (steps == 0) List(left)
    else range(0, steps + 1).foldl(List(), (cells, index) => {
      val candidate = {
        col: left.col + deltaCol * index / steps,
        row: left.row + deltaRow * index / steps,
      }
      if (cells.indices().exists(position => cells.nth(position) == candidate)) cells
      else cells.append(candidate)
    })
  }

  pure def cellTransparent(
    cells: List[Cell],
    opaqueCells: Set[Cell]
  ): bool =
    cells.indices().forall(index =>
      index == 0
        or index == cells.length() - 1
        or not(opaqueCells.contains(cells.nth(index))))

  pure def edgesTransparent(
    cells: List[Cell],
    opaqueEdges: Set[Edge]
  ): bool =
    cells.indices().forall(index =>
      index == cells.length() - 1
        or absolute(cells.nth(index + 1).col - cells.nth(index).col)
          + absolute(cells.nth(index + 1).row - cells.nth(index).row) != 1
        or not(opaqueEdges.contains(canonicalEdge(cells.nth(index), cells.nth(index + 1)))))

  pure def lineVisible(
    opaqueCells: Set[Cell],
    opaqueEdges: Set[Edge],
    left: Cell,
    right: Cell
  ): bool = {
    val supercoverCells = supercover(left, right)
    val reportedCells = sampledLine(left, right)
    cellTransparent(supercoverCells, opaqueCells) and edgesTransparent(reportedCells, opaqueEdges)
  }

  pure def absoluteFootprint(anchor: Cell, footprint: Set[Cell]): Set[Cell] =
    footprint.map(offset => addCell(anchor, offset))

  pure def tracePairs(query: VisibilityQuery): Set[CellPair] = {
    val origins = absoluteFootprint(query.origin, query.footprint)
    val targets = absoluteFootprint(query.target, query.footprint)
    origins.fold(Set(), (pairs, source) =>
      targets.fold(pairs, (inner, target) => inner.union(Set({ source: source, target: target }))))
  }

  pure def declaredPairWork(pair: CellPair): int =
    maximum(
      absolute(pair.target.col - pair.source.col),
      absolute(pair.target.row - pair.source.row)) + 1

  pure def supercoverPairWork(pair: CellPair): int =
    absolute(pair.target.col - pair.source.col)
      + absolute(pair.target.row - pair.source.row)
      + 1

  pure def declaredTraceWork(query: VisibilityQuery): int =
    tracePairs(query).fold(0, (work, pair) => work + declaredPairWork(pair))

  pure def supercoverTraceWork(query: VisibilityQuery): int =
    tracePairs(query).fold(0, (work, pair) => work + supercoverPairWork(pair))

  pure def visibleSampleCount(query: VisibilityQuery): int =
    tracePairs(query).fold(0, (count, pair) =>
      if (lineVisible(query.opaqueCells, query.opaqueEdges, pair.source, pair.target)) count + 1
      else count)

  pure def evaluateVisibility(query: VisibilityQuery): VisibilityResult = {
    val pairs = tracePairs(query)
    val declared = declaredTraceWork(query)
    val supercoverSteps = supercoverTraceWork(query)
    val invalid = query.footprint.size() == 0 or query.maximumCrossedItems <= 0
    val exhausted = pairs.size() > query.maximumCrossedItems or declared > query.maximumCrossedItems
    val truncated = invalid or exhausted
    val visibleSamples = if (truncated) 0 else visibleSampleCount(query)
    {
      visible: not(truncated) and visibleSamples > 0,
      visibleSamples: visibleSamples,
      totalSamples: pairs.size(),
      declaredWork: declared,
      supercoverWork: supercoverSteps,
      truncated: truncated,
    }
  }

  pure def fourConnected(cells: List[Cell]): bool =
    cells.indices().forall(index =>
      index == cells.length() - 1
        or absolute(cells.nth(index + 1).col - cells.nth(index).col)
          + absolute(cells.nth(index + 1).row - cells.nth(index).row) == 1)

  pure def endpointsPreserved(left: Cell, right: Cell): bool = {
    val pair = canonicalPair(left, right)
    val cells = supercover(left, right)
    cells.head() == pair.source and cells.nth(cells.length() - 1) == pair.target
  }

  pure def symmetricVisibility(query: VisibilityQuery): bool =
    tracePairs(query).forall(pair =>
      lineVisible(query.opaqueCells, query.opaqueEdges, pair.source, pair.target)
        == lineVisible(query.opaqueCells, query.opaqueEdges, pair.target, pair.source))

  pure def resultIsConsistent(query: VisibilityQuery): bool = {
    val result = evaluateVisibility(query)
    and {
      result.visibleSamples >= 0,
      result.visibleSamples <= result.totalSamples,
      result.visible == (not(result.truncated) and result.visibleSamples > 0),
      result.truncated implies result.visibleSamples == 0,
    }
  }
}
```

The companion module keeps concrete witnesses separate from the functional
model. Every scenario is bounded and deterministic.

```quint sir-visibility.qnt +=
module SirVisibilityTests {
  import SirVisibility.*

  pure val zero = { col: 0, row: 0 }
  pure val oneRight = { col: 1, row: 0 }
  pure val oneUpRight = { col: 1, row: 1 }
  pure val twoRightOneUp = { col: 2, row: 1 }
  pure val footprint2 = Set(zero, oneRight)

  pure val openQuery = {
    origin: zero,
    target: twoRightOneUp,
    footprint: Set(zero),
    opaqueCells: Set(),
    opaqueEdges: Set(),
    maximumCrossedItems: 8,
  }

  pure val blockedQuery = {
    ...openQuery,
    opaqueCells: Set(oneRight),
  }

  pure val footprintQuery = {
    origin: zero,
    target: { col: 3, row: 0 },
    footprint: footprint2,
    opaqueCells: Set({ col: 1, row: 0 }),
    opaqueEdges: Set(),
    maximumCrossedItems: 32,
  }

  pure val exhaustedQuery = {
    ...footprintQuery,
    maximumCrossedItems: 4,
  }

  run openLineIsVisible = all {
    assert(evaluateVisibility(openQuery).visible),
    assert(evaluateVisibility(openQuery).visibleSamples == 1),
    assert(supercover(zero, twoRightOneUp) ==
      List(zero, oneRight, { col: 1, row: 1 }, twoRightOneUp)),
  }

  run xFirstCornerTieIsStable =
    assert(supercover(zero, oneUpRight) == List(zero, oneRight, oneUpRight))

  run interiorCellBlocks = all {
    assert(not(evaluateVisibility(blockedQuery).visible)),
    assert(evaluateVisibility(blockedQuery).visibleSamples == 0),
  }

  run endpointsDoNotBlock = all {
    assert(lineVisible(Set(zero, twoRightOneUp), Set(), zero, twoRightOneUp)),
  }

  run opaqueEdgeBlocks = all {
    assert(not(lineVisible(Set(), Set(canonicalEdge(zero, oneRight)), zero, twoRightOneUp))),
  }

  run boundaryChecksUseTheSampledLine = all {
    assert(sampledLine(zero, twoRightOneUp) == List(zero, oneRight, twoRightOneUp)),
    assert(lineVisible(
      Set(),
      Set(canonicalEdge(oneRight, { col: 1, row: 1 })),
      zero,
      twoRightOneUp)),
  }

  run supercoverClosesCornerGap = all {
    assert(not(lineVisible(Set(oneRight), Set(), zero, twoRightOneUp))),
    assert(not(lineVisible(Set({ col: 1, row: 1 }), Set(), zero, twoRightOneUp))),
  }

  run canonicalizationMakesVisibilitySymmetric = all {
    assert(symmetricVisibility(blockedQuery)),
    assert(lineVisible(Set(oneRight), Set(), zero, twoRightOneUp)
      == lineVisible(Set(oneRight), Set(), twoRightOneUp, zero)),
  }

  run oneFootprintPairCanExposeTarget = all {
    assert(evaluateVisibility(footprintQuery).visible),
    assert(evaluateVisibility(footprintQuery).visibleSamples > 0),
    assert(evaluateVisibility(footprintQuery).visibleSamples < evaluateVisibility(footprintQuery).totalSamples),
  }

  run crossedItemBoundExhaustsBeforeEvaluation = all {
    assert(evaluateVisibility(exhaustedQuery).truncated),
    assert(not(evaluateVisibility(exhaustedQuery).visible)),
    assert(evaluateVisibility(exhaustedQuery).visibleSamples == 0),
  }

  run structuralPropertiesHold = all {
    assert(fourConnected(supercover(zero, twoRightOneUp))),
    assert(endpointsPreserved(zero, twoRightOneUp)),
    assert(symmetricVisibility(openQuery)),
    assert(resultIsConsistent(openQuery)),
    assert(resultIsConsistent(blockedQuery)),
    assert(resultIsConsistent(footprintQuery)),
    assert(resultIsConsistent(exhaustedQuery)),
  }

  run workAccountingBoundaryIsExplicit = all {
    assert(declaredPairWork({ source: zero, target: twoRightOneUp }) == 3),
    assert(supercoverPairWork({ source: zero, target: twoRightOneUp }) == 4),
    assert(evaluateVisibility(openQuery).declaredWork == 3),
    assert(evaluateVisibility(openQuery).supercoverWork == 4),
  }

  run boundedGridPropertiesHold = {
    val cells = range(-2, 3).foldl(Set(), (known, col) =>
      range(-2, 3).foldl(known, (inner, row) => inner.union(Set({ col: col, row: row }))))
    assert(cells.forall(left => cells.forall(right => and {
      fourConnected(supercover(left, right)),
      endpointsPreserved(left, right),
      lineVisible(Set(), Set(), left, right) == lineVisible(Set(), Set(), right, left),
      supercover(left, right).length()
        == absolute(right.col - left.col) + absolute(right.row - left.row) + 1,
    })))
  }
}
```

The simulation adapter gives `quint run` a small nondeterministic state machine.
It does not claim that production stores this state; it exists to prove that
visible, blocked, and exhausted outcomes are all reachable while every stored
result remains the exact output of the pure evaluator.

```quint sir-visibility.qnt +=
module SirVisibilitySimulation {
  import SirVisibility.*

  type Scenario = { id: int, query: VisibilityQuery }
  type EvaluationState = {
    scenarioId: int,
    query: VisibilityQuery,
    result: VisibilityResult,
  }

  pure val origin = { col: 0, row: 0 }
  pure val target = { col: 2, row: 1 }
  pure val offset = { col: 0, row: 0 }

  pure val openScenario = {
    id: 1,
    query: {
      origin: origin,
      target: target,
      footprint: Set(offset),
      opaqueCells: Set(),
      opaqueEdges: Set(),
      maximumCrossedItems: 8,
    },
  }

  pure val blockedScenario = {
    id: 2,
    query: {
      ...openScenario.query,
      opaqueCells: Set({ col: 1, row: 0 }),
    },
  }

  pure val exhaustedScenario = {
    id: 3,
    query: {
      ...openScenario.query,
      maximumCrossedItems: 2,
    },
  }

  var evaluation: EvaluationState

  pure def evaluated(scenario: Scenario): EvaluationState =
    {
      scenarioId: scenario.id,
      query: scenario.query,
      result: evaluateVisibility(scenario.query),
    }

  action init: bool =
    evaluation' = evaluated(openScenario)

  action evaluateScenario(scenario: Scenario): bool = all {
    scenario.id > 0,
    evaluation' = evaluated(scenario),
  }

  action step: bool = {
    nondet scenario = Set(openScenario, blockedScenario, exhaustedScenario).oneOf()
    evaluateScenario(scenario)
  }

  val storedResultMatchesPureEvaluation: bool =
    evaluation.result == evaluateVisibility(evaluation.query)

  val truncatedResultIsNeverVisible: bool =
    evaluation.result.truncated implies not(evaluation.result.visible)

  val visibleEvaluationReached: bool =
    evaluation.scenarioId == 1 and evaluation.result.visible

  val blockedEvaluationReached: bool =
    evaluation.scenarioId == 2 and not(evaluation.result.visible) and not(evaluation.result.truncated)

  val exhaustedEvaluationReached: bool =
    evaluation.scenarioId == 3 and evaluation.result.truncated
}
```

## Claim boundary

Passing these witnesses establishes the behavior of this Quint translation for
the exercised finite inputs. It does not prove correspondence for every integer
coordinate, reproduce the package's allocation or overflow behavior, cover
continuous `Visibility.polygon`, or establish that the public crossed-item bound
equals the number of internal supercover cells. Runtime correspondence requires
separate replay against the pinned package and `SpatialQuery` implementation.
