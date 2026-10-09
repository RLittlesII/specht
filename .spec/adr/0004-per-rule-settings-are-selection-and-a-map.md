---
title: "ADR-0004: Per-rule settings are selection and a map, not decoration"
description: "Supersedes only ADR-0002's trigger clause: per-rule settings - disable, re-grade, a minimum schema version, a default severity - are selection over the rule set and a severity map over the violations inside Evaluate, never decorators, however many accumulate; decorators around ISpecRule are earned only by behaviour that wraps a rule's evaluation and keeps its identity."
type: adr
---

# ADR-0004: Per-rule settings are selection and a map, not decoration

## Status

proposed - 2026-10-08. Raised by the owner on 2026-10-08: "Given we are now
inverting control, maybe this leads us to a different approach?"

Supersedes the trigger clause of
[ADR-0002](0002-no-chain-of-responsibility-for-the-check.md) only - its last
Decision bullet, "The trigger for option 3 is a third per-rule behaviour that
wraps a rule's evaluation". The rest of ADR-0002 is re-affirmed. ADR-0002 is
accepted and is not edited by this record.

## Context

ADR-0002 is accepted (pull request #16): option 1, one step in the runner,
with option 3 - decorators around `ISpecRule` - "the named move when a third
per-rule behaviour appears". Its Decision reads: "The trigger for option 3 is a
third per-rule behaviour that wraps a rule's evaluation - not a third rule, and
not a stage. When it appears, a superseding ADR introduces the chain around
`ISpecRule`, resolved from the container." Its Decision block still opens
"Proposed:" although its Status is accepted; this record does not edit it.

Since then ADR-0001 and ADR-0003 were accepted (pull request #21). ADR-0001
moves the engine into the container in stages: stage B splits
`SpecCheckRunner` into `Evaluate(SpecModel)` and `Run(root)` on item `0103`'s
order function, stage C replaces reflection with a hand-written rule list and
an assembly-coverage test, and stage D makes the engine instance classes behind
`AddSpechtEngine()`. ADR-0003 option 8 turned down Scrutor: ADR-0002's earned
move, decoration around `ISpecRule`, is registered by hand in plain
Microsoft.Extensions.DependencyInjection.

The owner asked whether inverting control changes ADR-0002's answer. The
architect's reassessment:

- **IoC does not change the verdict.** A chain of responsibility is the wrong
  shape for a checker that reports every violation. The container pays for
  registration; it does not pay for per-rule decorators or remove their
  ordering risk.
- **Four per-rule behaviours are now specified**, so ADR-0002's count of three
  has been passed:
  1. disable - `0001-F5` B-011, item `0014`;
  2. re-grade - `0001-F5` B-010, item `0014`;
  3. a minimum schema version - `0101-F1` B-012 and C-5, `0101-F2` B-010 and
     C-4, `0101-F3` B-009 and C-4, `0101-F4` B-007 and C-3; `0101-F1`
     decision 0001;
  4. a default severity of warning - `0101-F1` C-6 and its siblings
     (`0101-F2` C-4, `0101-F3` C-4, `0101-F4` C-3).

  None of the four wraps an evaluation. Disable and the version gate are
  selection: whether a rule runs at all. Re-grade and the default severity are
  data: a severity per rule id.

- **A decorator cannot express "do not run".** It wraps a call that the runner
  still makes, and the runner's count of rules evaluated is
  `rules.Sum(r => r.ReportedIds.Count)` (`src/specht/SpecCheckRunner.cs:35`). A
  disabling decorator cannot take its rule out of that sum unless it also
  overrides `ReportedIds`, duplicating the rule's identity in a second place
  (B-011).
- **An ordered post-evaluation stage list** - an `IViolationStage`, an
  `IEnumerable<IReportStep>` - makes the verdict a function of registration
  order, against `0001-F1` C-9, and has no second caller and no substitution.
- **A strategy injected into each rule** - each rule handed its settings and
  deciding for itself - touches eight rules under C-9, scatters one switch
  across them, and still cannot fix the count, which the runner owns.

One nuance in the existing design text: `0001-F5` § 7, "Where rule settings
apply", already drops a disabled rule's id from the count, but says "every rule
still evaluates". That clashes with epic `0101`'s version gate: run-then-discard
evaluates a version 2 rule against a version 1 tree, which may lack the
manifest roles the rule reads, and could fail inside the rule (`0001-F5` C-5
rules out a failure inside a rule).

## Decision drivers

1. Every rule that runs is reported in full, and the output order does not
   depend on registration order (`0001-F1` C-9).
2. The count of rules evaluated excludes a rule that did not run (`0001-F5`
   B-011).
3. The manifest may disable a rule or lower its severity, and may not add a
   rule or declare one as data (`0001-F5` C-3).
4. The manifest is accepted whole before any rule runs; nothing fails inside a
   rule for want of a manifest value (`0001-F5` C-5).
5. A pattern only when the problem has its shape, and a member only when a
   second value exists (`coding-conventions` § Design).

## Considered options

1. **Re-affirm one step in the runner, re-cut as selection plus a severity
   map.** Inside `Evaluate(SpecModel)`: choose the rules to run, evaluate them,
   map each violation's severity, sort once. Cost: `Evaluate` carries two
   small functions instead of one; a new kind of setting is an edit there.
2. **A chain over the stages.** Rejected, as in ADR-0002 option 2: the stage
   order is a data dependency, and a context bag trades compile-time for
   run-time failure and turns `0001-F5` C-5 into a registration order.
3. **Decorators over `ISpecRule`.** Rejected for per-rule settings: a
   decorator cannot stop a rule running or take it out of the count without
   duplicating `ReportedIds`, and the order of decorators is a new place for a
   verdict to change. Kept for behaviour that wraps a rule's evaluation and
   keeps its identity (timing, tracing, wrapping an exception with the rule's
   id), registered by hand (ADR-0003 option 8).
4. **An ordered stage pipeline after evaluation** (`IViolationStage`,
   `IEnumerable<IReportStep>`). Rejected, not deferred: the verdict becomes a
   function of registration order, against `0001-F1` C-9, and nothing
   substitutes a stage or calls the list a second way.
5. **A strategy injected into each rule.** Each rule receives its settings and
   filters or re-grades itself. Rejected: eight rules change under C-9, one
   switch is scattered eight ways, and the count is still the runner's. What it
   gets right - that a rule's default severity and its first schema version
   are facts about the rule - is kept, as members of `ISpecRule` (decision
   4).
6. **The version gate as a manifest setting.** Each version's manifest lists
   the rules it enables, or the minimum version of each. Rejected: the rule
   vocabulary is the pinned version's, and the manifest may only disable a rule
   or lower its severity (`0001-F5` C-3); which version a rule belongs to is
   the engine's fact, not a consumer's setting a manifest edit could change.

## Decision

Proposed: option 1, re-cut.

- **ADR-0002 option 1 is re-affirmed.** The stages stay a fixed, typed
  sequence; every selected rule runs and is reported in full; one sort.
- **Item `0014` is two functions inside `Evaluate(SpecModel)`** (ADR-0001
  stage B), not "one step over the collected violations":
  - **selection**, over the rule set before evaluation: a disabled rule is not
    evaluated, and the count of rules evaluated is summed over the selected
    set (`0001-F5` B-011);
  - **severity resolution**, a map over the violations before the single sort
    (item `0103`'s order function), so a re-graded rule's violations carry
    the manifest's severity (`0001-F5` B-010).
- **The stage pipeline is rejected** (option 4), not deferred.
- **`DefaultSeverity` and `MinimumSchemaVersion` become `ISpecRule` members**,
  read by selection and the map. They land with epic `0101`'s first item,
  which is gated on `0001-F7` and schema version 2. They are not added before
  then: today every rule has one value for each - error, version 1 - and a
  member with one value is a configuration point nothing configures
  (`coding-conventions` § Design). The manifest-keyed alternative is rejected
  (option 6, `0001-F5` C-3).
- **The trigger for option 3 is replaced.** Only behaviour that wraps a rule's
  evaluation and preserves its identity (timing, tracing, exception-wrapping)
  counts toward decorators around `ISpecRule`. Per-rule settings never do,
  however many accumulate. When such behaviour is earned, the decorators are
  registered by hand (ADR-0003 option 8), and a **decoration-coverage test**
  lands with them: ADR-0001 stage C's assembly-coverage test proves every rule
  is registered, not that every registered rule is decorated.

## Consequences

Buys:

- Item `0014` is the size its claims are, and B-011's count is right by
  construction: a rule that is not selected neither runs nor counts.
- A disabled or version-gated rule never runs against a tree its manifest has
  no roles for, so nothing fails inside a rule (`0001-F5` C-5).
- Epic `0101`'s two per-rule facts have one home each - the rule - read by the
  same two functions; no new type, no ordering of handlers, no decorator.
- The question "is this the third behaviour?" is closed: settings are
  selection or data, whatever their number.

Costs:

- `Evaluate` grows by two functions, and each new kind of per-rule setting is
  an edit to one of them rather than a new registration.
- The 30 `SpecSeverity.Error` yields in `src/specht/Rules/` stay as they are
  until epic `0101`; `DefaultSeverity` does not replace them before a rule
  exists whose default differs.
- `0001-F5` § 7's "every rule still evaluates" is withdrawn; item `0014`'s text,
  which still describes one step over the collected violations, is restated by
  the `spec-author` when the item is next touched.
- Risk: selection and the severity map are two places that read the manifest's
  rule settings; a rule id unknown to the pinned vocabulary must still be
  rejected once, when the manifest is loaded (`0001-F5` C-5), not in each.
- Risk: when option 3 is earned, a rule left undecorated is silent unless the
  decoration-coverage test exists; it is part of that move, not optional.

What would change this: `0101-F5` OQ-3 - whether `format` honours the
manifest's rule settings. If it does, selection gains a second caller outside
the check, and its home moves from inside `Evaluate` to a function both the
check and `format` call. That is a relocation of selection, not a reason for
decorators or a pipeline.

Work on acceptance: none beyond item `0014`'s existing scope, whose
description the `spec-author` restates to cite this record. `DefaultSeverity`
and `MinimumSchemaVersion` are cut with epic `0101`'s first item.
