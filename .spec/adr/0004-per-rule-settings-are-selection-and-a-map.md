---
title: "ADR-0004: Per-rule settings are selection and a map, not decoration"
description: "Supersedes ADR-0002 in part - its trigger clause and item 0014's shape: per-rule settings - disable, re-grade, a minimum schema version, a default severity - are selection over the rule set and a severity map over the violations inside Evaluate, never decorators, however many accumulate; decorators around ISpecRule are earned only by behaviour that wraps a rule's evaluation and keeps its identity."
type: adr
---

# ADR-0004: Per-rule settings are selection and a map, not decoration

## Status

proposed - 2026-10-08. Raised by the owner on 2026-10-08: "Given we are now
inverting control, maybe this leads us to a different approach?"

Supersedes [ADR-0002](0002-no-chain-of-responsibility-for-the-check.md) in
part:

- **Replaced:** ADR-0002's trigger clause for option 3, stated three times -
  its frontmatter description ("until a third per-rule behaviour beyond
  disable and re-grade exists", `:3`), its Status line ("option 3 the named
  move when a third per-rule behaviour appears", `:11-12`) and its last
  Decision bullet (`:104-107`); and its third Decision bullet (`:102-103`),
  item `0014` as one step over each rule's violations inside the runner.
- **Narrowed:** ADR-0002's second Decision bullet (`:101`) and its driver 1
  (`:62`), from "every rule runs" to every _selected_ rule runs.
- **Re-affirmed:** everything else - option 1 for the stages, the rejection of
  a chain over the stages (option 2) and of a first-handler-wins chain over
  the rules (option 4), and its Consequences.

ADR-0002's body is not edited; its Status gains one line pointing here. The
ADR template (`.spec/templates/adr.md:19-21`) says a new ADR supersedes an
accepted one entirely; this partial supersession is deliberate, and the owner
chose it.

## Context

ADR-0002 is accepted (pull request #16): option 1, one step in the runner,
with option 3 - decorators around `ISpecRule` - "the named move when a third
per-rule behaviour appears". Its Decision reads: "The trigger for option 3 is a
third per-rule behaviour that wraps a rule's evaluation - not a third rule, and
not a stage. When it appears, a superseding ADR introduces the chain around
`ISpecRule`, resolved from the container." Its Decision block still opens
"Proposed:" although its Status is accepted; this record does not edit it.

ADR-0001 and ADR-0003 are before the owner in pull request #21. This record
rests on ADR-0001 only for where the two functions below sit - stage B's
`Evaluate(SpecModel)`, item `0105` - and on ADR-0003 only for how a decorator
would be registered if one were ever earned (by hand, ADR-0003 option 8). If
ADR-0001 were rejected, the same two functions sit in `Run(root)`, so the
verdict stands.

The owner asked whether inverting control changes ADR-0002's answer. The
architect's reassessment:

- **IoC does not change the verdict.** A chain of responsibility is the wrong
  shape for a checker that reports every violation. The container pays for
  registration; it does not pay for per-rule decorators or remove their
  ordering risk.
- **Four per-rule behaviours are now specified**, so ADR-0002's count of three
  has been passed. The count is why the question reopened, not why the answer
  is what it is; the answer rests on each behaviour's shape:
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
apply", already drops a disabled rule's id from the count, but said "every rule
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
   function of registration order, and nothing substitutes a stage or calls
   the list a second way. The output's independence from evaluation order is
   the single sort's - item `0103`'s order function - which `0001-F1` C-9
   guards; a stage list puts a second, registration-defined order in front of
   it.
5. **A strategy injected into each rule.** Each rule receives its settings and
   filters or re-grades itself. Rejected: eight rules change under C-9, one
   switch is scattered eight ways, and the count is still the runner's. What it
   gets right - that a rule's default severity and its first schema version
   are facts about the rule - is kept, as members of `ISpecRule` (see
   Decision).
6. **The version gate as a manifest setting.** Each version's manifest lists
   the rules it enables, or the minimum version of each. Rejected: the rule
   vocabulary is the pinned version's, and the manifest may only disable a rule
   or lower its severity (`0001-F5` C-3); which version a rule belongs to is
   the engine's fact, not a consumer's setting a manifest edit could change.
7. **Do nothing: ADR-0002 stands unamended.** Item `0014` is one step over the
   collected violations, every rule evaluates, and the third per-rule
   behaviour triggers decorators around `ISpecRule`. Rejected: the trigger has
   already fired on its letter (four behaviours), so standing still commits
   the engine to decorators for settings a decorator cannot express (option
   3), and run-then-discard evaluates a version 2 rule against a version 1
   tree (`0001-F5` C-5). Cost of rejecting: this record, and a partial
   supersession of an accepted one.
8. **Filter at the rule-set seam.** ADR-0001 already cuts the rule set as a
   seam; an `ISpecRuleSelector`, or a registered
   `Func<IEnumerable<ISpecRule>, IEnumerable<ISpecRule>>` resolved from the
   container, narrows the set before `Evaluate` sees it. Rejected on its
   merits: one implementation and no second caller today (`coding-conventions`
   § Design); the check is selection's only caller, and a registered function
   nothing substitutes is a configuration point nothing configures. The
   condition that would earn it is `0101-F5` OQ-3: if `format` honours the
   manifest's rule settings, selection has a second caller.

## Decision

Proposed: option 1, re-cut.

- **ADR-0002 option 1 is re-affirmed.** The stages stay a fixed, typed
  sequence; every selected rule runs and is reported in full; one sort.
- **Item `0014` is two functions inside `Evaluate(SpecModel)`** (ADR-0001
  stage B, item `0105`), not "one step over the collected violations":
  - **selection**, over the rule set before evaluation: a disabled rule is not
    evaluated, and the count of rules evaluated is summed over the selected
    set (`0001-F5` B-011). Both functions read the settings from
    `model.Schemas.Structure` (`src/specht/SpecSchemas.cs:42`), which item
    `0014` extends with the rule settings and `0001-F7` with `schemaVersion`.
    `Evaluate` takes no second parameter and reads no static;
  - **severity resolution**, a map over the violations that runs before item
    `0103`'s order function, because that function sorts by severity first,
    so a re-graded rule's violations carry the manifest's severity (`0001-F5`
    B-010).
- **The stage pipeline is rejected** (option 4), not deferred; so is the
  rule-set filter (option 8), until `0101-F5` OQ-3 gives selection a second
  caller.
- **`DefaultSeverity` and `MinimumSchemaVersion` become `ISpecRule` members**,
  read by selection and the map. They land with epic `0101`'s first item,
  which is gated on `0001-F7` and schema version 2. They are not added before
  then: today every rule has one value for each - error, version 1 - and a
  member with one value is a configuration point nothing configures
  (`coding-conventions` § Design). The manifest-keyed alternative is rejected
  (option 6, `0001-F5` C-3). This conflicts with `0101-F1` C-6 as written
  (`src/specht/Rules/Ordering/.spec/README.md:86`: "The default severity is
  warning, set through `0001-F5`'s rule settings"): from epic `0101`'s first
  item the default is the rule's own `DefaultSeverity`, and the manifest's
  rule settings override it. `0101-F1` decision 0001 is preserved - a member is
  not a switch, and these rules still add no switch of their own - but C-6's
  wording is owed an amendment, with a `0101-F1` § 10 delta.
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
- The `SpecSeverity.Error` yields in `src/specht/Rules/` (30 across eight
  rules today) stay as they are until epic `0101`; `DefaultSeverity` does not
  replace them before a rule exists whose default differs.
- Risk: selection and the severity map are two places that read the manifest's
  rule settings; a rule id unknown to the pinned vocabulary must still be
  rejected once, when the manifest is loaded (`0001-F5` C-5), not in each.
- Risk: when option 3 is earned, a rule left undecorated is silent unless the
  decoration-coverage test exists; it is part of that move, not optional.
- The records this record amends, each owed on acceptance and none edited by
  it:
  - `0101-F1` C-6 (`src/specht/Rules/Ordering/.spec/README.md:86`): the
    default is the rule's `DefaultSeverity`, overridden by the manifest's rule
    settings, with a `0101-F1` § 10 delta recording why.
  - ADR-0001's cost bullet about item `0014`
    (`.spec/adr/0001-resolve-the-engine-from-the-container.md:279-282`): both
    halves are superseded - "one step in the runner over the collected
    violations" and "a decorator around `ISpecRule` stays deferred until
    ADR-0002 names it earned". ADR-0001 is accepted, so it is owed a
    superseding note, not an edit.
  - Item `0014` (`src/specht/Manifest/.issue/0014-rule-settings.yml`), as
    below.
  - `0001-F5` § 10: a dated line recording that § 7's "Where rule settings
    apply" moved from "every rule still evaluates" and "one step over the
    collected violations" to selection and a map, citing this record.

What would change this: `0101-F5` OQ-3 - whether `format` honours the
manifest's rule settings. If it does, selection gains a second caller outside
the check, and its home moves from inside `Evaluate` to a function both the
check and `format` call - option 8's seam, earned. That is a relocation of
selection, not a reason for decorators or a pipeline.

Work on acceptance: item `0014` keeps its claims (B-010, B-011, B-013, B-017),
and its shape is restated as selection plus a map inside `Evaluate`:

- `depends_on` gains `0105` (pull request #22) and, explicitly, `0103`, whose
  order function the map runs before. Its `priority`, its `rank` and the
  `blocks` of everything it depends on are derived, and are recomputed, never
  hand-edited.
- A new dated `decisions` entry supersedes the 2026-10-08 one ("one step in
  `SpecCheckRunner` over each rule's collected violations").
- The `out_of_scope` bullet on a chain of handlers cites ADR-0004, as
  rejected, not deferred.
- The B-011 acceptance line becomes "disabled: nothing reported, not evaluated
  at all, so not counted".
- `DefaultSeverity` and `MinimumSchemaVersion` are cut with epic `0101`'s first
  item, not with `0014`.
