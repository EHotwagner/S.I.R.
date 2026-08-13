# Awareness/reaction contract v1

Simulation owns versioned `SensorProfile`, `Stimulus`, `AwarenessState`, `LastKnownContact`, `EngagementTarget`, `EngagementPhase`, `ReactionTrigger`, `ReactionWindow`, `AwarenessReactionFact`, `AwarenessReactionReason`, limits, counters, and pure update/canonical-encoding functions. Public `.fsi` declarations precede implementation.

Body facing, attention/sensor direction, posture, and movement direction are distinct. For the infantry profile, eight-direction distance 0-1 is forward, 2 peripheral, and 3-4 rear; visual contributions are 4/2/1 toward threshold 8, decay is 2 per missed tick, retention is 20 ticks, range is 60 cells, and at most four exposure samples are evaluated. Exact `SpatialQuery` LOS is evidence for a stimulus, never an identification assignment.

Each unit has zero or one engagement targeting a locally known unit, an ordered unique area of at most 256 cells, or one canonical semantic edge bound to spatial revision. Preparation/commitment/resolution/recovery durations are 2/1/1/4 ticks. The public tick order is input admission; movement/spatial transition; stimuli/awareness; engagement maintenance; trigger snapshot; reactor-id/engagement-id/trigger-kind/source-id commitment order; physical reaction resolution and interruption; ordinary physical actions; event emission; recovery.

Match observations contain bounded local sectors, stimuli, awareness, last-known facts, engagement/window phase and reason codes. They contain no undisclosed unit/world state or authority-only geometry. Client and Web project these facts and send ordinary player controls; they do not evaluate sectors, LOS, awareness, eligibility, or outcomes.
