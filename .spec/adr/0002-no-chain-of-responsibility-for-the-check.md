---
title: "ADR-0002: No chain of responsibility for the check pipeline or the rules"
description: "The check stays a fixed, typed sequence of stages that runs every rule and sorts once; a chain of handlers is turned down for the stages and deferred for the rules until a third per-rule behaviour beyond disable and re-grade exists."
type: adr
---

# ADR-0002: No chain of responsibility for the check pipeline or the rules

## Status

accepted - 2026-10-08, by the owner: option 1, with option 3 the named move
when a third per-rule behaviour appears. Raised by the owner on pull request #13
([review comment](https://github.com/RLittlesII/specht/pull/13#discussion_r4226016028),
on `src/specht/SpecCheckRunner.cs`).

Partially superseded by [ADR-0004](0004-per-rule-settings-are-selection-and-a-map.md), 2026-10-09: the trigger clause for option 3, and item `0014`'s shape. The rest stands.

## Context

The owner's comment: "I am also wondering if we should create a chain of
responsibilities."

The check today, in `SpecCheckRunner.Run(root)`:

```
SpecManifest.Load  ->  SpecSchemas.Load  ->  SpecDiscovery  ->  SpecDocument.Parse / FrontmatterReader
  ->  SpecModel  ->  every ISpecRule.Evaluate(model)  ->  sort  ->  SpecCheckReport
```

Two places a chain could sit:

- **The stages.** Each consumes the previous stage's typed output: the manifest
  gives the structure the documents are parsed against, discovery gives the
  locations the readers open, the readers give the model the rules read. The
  order is a data dependency, and part of it is a constraint: the manifest is
  validated whole before any rule runs (`0001-F5` C-5, B-018), and a rejected
  manifest ends the run with exit `3` (item `0013`). `SpecModel.Load` reads it
  before the tree; the short-circuit is `SpechtManifestException`.
- **The rules.** Eight `ISpecRule`s, each evaluated over the whole model;
  every rule runs and every violation is reported. Their order does not reach
  the output: the runner sorts once by severity, file, line and rule id, so the
  report is independent of evaluation order (`0001-F1` C-9). The behaviours
  that will act on each rule's output are per-rule disable and severity
  override from the manifest (`0001-F5` B-010, B-011, item `0014`). Nothing may
  add a rule: no rule type, library or body from a manifest
  (`0001-F5` C-3, B-017; decision `0001-F2` 0001, which turned down a plugin
  model).

The direction in brief § 5 moves literals into the manifest, one rule at a
time, with the default manifest reproducing `hooked`'s baseline report. It
makes the rules read roles instead of literals; it does not make the stages or
the rule set configurable.

The classic chain of responsibility passes a request along handlers until one
handles it. A checker that must report every violation is the opposite
shape: every handler handles, nothing stops the walk. The ordered
handler-calls-next form (middleware) fits cross-cutting work wrapped around a
call - logging, timing, settings.

## Decision drivers

1. Every rule runs and every violation is reported; the output order does not
   depend on registration order (`0001-F1` C-9).
2. The manifest is accepted before anything else runs (`0001-F5` C-5).
3. The stage sequence stays readable in one method.
4. A pattern is used only when the problem already has its shape
   (`coding-conventions` § Design); a link with one behaviour is a function.
5. Fits ADR-0001 if it is accepted, and does not need it if it is not.

## Considered options

1. **Keep the direct design.** The runner calls the stages in order and
   evaluates every rule; disable and re-grade (item `0014`) are one step in the
   runner over the collected violations, read from the manifest. Cost: the
   runner grows by one step; a fourth cross-cutting behaviour would be a
   further edit to the runner.
2. **A chain over the stages.** Each stage is a handler over a shared context
   (`SpecCheckContext` carrying root, structure, locations, model, violations),
   calling the next. Cost: each stage's typed input and output become nullable
   fields of a context bag, so a stage that runs before its input exists fails
   at run time rather than compile time; C-5 becomes a registration order that
   a misordered line breaks silently; the order is fixed by data, so the
   flexibility bought is one nothing asks for; the copied engine is rewritten
   in a new shape (brief § 9).
3. **A chain over each rule's evaluation.** A decorator or middleware chain
   around `ISpecRule.Evaluate` - a disable link, a re-grade link - resolved from
   the container (needs ADR-0001). Cost: two links, each a one-line filter or
   map over a rule's violations; an interface, a registration order and a
   decorator per rule for what option 1 does in one step; the order of the
   links is itself a new place for a verdict to change.
4. **A chain over the rules, first-handler-wins.** Cost: wrong shape - a check
   that stops at the first rule to report under-reports by design and breaks
   `0001-F2` B-001.

## Decision

Proposed: option 1, with option 3 kept as the named move when it is earned.

- The stages stay a fixed, typed sequence in the runner (or in the runner
  ADR-0001 resolves). No chain, no context bag.
- The rule set stays a collection every member of which runs, sorted once.
- Item `0014` builds disable and re-grade as one step over each rule's
  violations, inside the runner.
- The trigger for option 3 is a third per-rule behaviour that wraps a rule's
  evaluation - not a third rule, and not a stage. When it appears, a
  superseding ADR introduces the chain around `ISpecRule`, resolved from the
  container.

## Consequences

Buys:

- The check reads top to bottom in one method; C-5's ordering is code, not
  registration order.
- No new types, no context object, no change to the copied engine beyond what
  the claims already require.
- Item `0014` stays the size its claims are.

Costs:

- A cross-cutting concern that is not per-rule - timing, tracing - has no
  insertion point; it is an edit to the runner. Nothing claims one today.
- If the owner wants stage-level substitution for tests, it comes from
  ADR-0001's seams (the file system, discovery, the rule set), not from a
  chain; rejecting ADR-0001 leaves the stages substitutable only through the
  file system.
- A future reader may still want a chain at the rules; this record names when
  it is earned so the question is not reopened case by case.

Work on acceptance: none beyond item `0014`'s existing scope, which cites this
ADR for where disable and re-grade sit. If the owner instead accepts option 3,
items cut on acceptance: the decorator seam around `ISpecRule` (after
ADR-0001's rule-set stage), then `0014` rebuilt on it.
