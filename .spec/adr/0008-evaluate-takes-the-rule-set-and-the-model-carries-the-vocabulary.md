---
title: "ADR-0008: Evaluate takes the rule set, and the model carries the pinned vocabulary"
description: "Supersedes ADR-0001 and ADR-0004 in part, for ADR-0001's stage B (item 0105): SpecCheckRunner.Evaluate takes the rule set as its second argument, so a test passes fake rules, and reads the pinned version's rule vocabulary from model.Schemas, which keeps the SchemaVersion it already selects; ADR-0004's 'no second parameter' is narrowed to the rule settings."
type: adr
---

# ADR-0008: Evaluate takes the rule set, and the model carries the pinned vocabulary

## Status

accepted - 2026-10-09, by the owner. Both calls were put to the owner directly
during item `0105`'s delivery and decided the same day; recorded by
spec-author.

Supersedes in part, each record's body unedited and its Status gaining one
line pointing here:

- **[ADR-0001](0001-resolve-the-engine-from-the-container.md):** the signature
  `Evaluate(SpecModel)` in its Decision (`:201-203`) and its stage B row
  (`:314`). The split, its proof and its edges stand.
- **[ADR-0004](0004-per-rule-settings-are-selection-and-a-map.md):** the
  clause "`Evaluate` takes no second parameter and reads no static" (`:170`),
  narrowed to the rule settings. Selection and the map stand.

## Context

Item `0105` is ADR-0001's stage B: static `SpecCheckRunner` splits into
`Evaluate` and `Run`. Two accepted records cannot both be met at that stage,
and neither says how a third fact reaches `Evaluate`.

1. **The fake-rule proof has no way in.** ADR-0001's stage B proof requires "a
   unit test of counts and `ReportedIds` with fake rules" (`:314`). The rules
   come from the private, reflection-based `Discover()`
   (`src/specht/SpecCheckRunner.cs`), which stage C replaces, not stage B.
   ADR-0004 says "`Evaluate` takes no second parameter and reads no static"
   (`:170`). With one parameter and no static, no fake rule reaches
   `Evaluate`.
2. **The vocabulary has no way in.** Since `0001-F7` B-014,
   `Run(root, SchemaVersions)` keeps only the rules, the violations and the
   count of rule ids evaluated that the pinned version's `RuleIds` hold. A
   `SpecModel` carries only the version's number, in
   `model.Schemas.Structure.SchemaVersion`.
   `SpecSchemas.Load(fileSystem, root, versions)` selects the `SchemaVersion`
   and drops it (`src/specht/SpecSchemas.cs`). Neither ADR-0001 nor ADR-0004
   says how the vocabulary reaches `Evaluate`.

At stake: `0001-F1` B-004 and C-9 (no verdict changes), `0001-F7` B-014 (only
the pinned vocabulary is evaluated, counted and reported), and ADR-0005
Decision (d) (a test substitutes through an argument it can construct, not
through a new interface).

## Considered options

For the rule set:

1. **The rule set is an argument (chosen).**
   `Evaluate(SpecModel model, IEnumerable<ISpecRule> rules)`, still static;
   `Run` loads the model and passes `Discover()`.
2. **A one-parameter `Evaluate`, and a separate public pure function for the
   counts.** Rejected by the owner: the fake rules would reach only the
   counting function, so `Evaluate` itself - selection, collection and order
   together - stays unproven below the integration tier, and a public function
   exists for a test alone.
3. **Defer the fake-rule proof to stage D (item `0107`),** where the rules are
   constructor-injected. Rejected by the owner: stage B would land without the
   proof ADR-0001 names for it, and items `0014` and `0106` build on an
   `Evaluate` no unit test has pinned.

For the vocabulary:

4. **The model carries it (chosen).** `SpecSchemas` keeps the `SchemaVersion`
   it already selects, and `Evaluate` reads the vocabulary from
   `model.Schemas`.
5. **A `SchemaVersion` parameter on `Evaluate`.** Rejected by the owner: the
   caller could pass a version other than the one the model's manifest pins,
   and the pinned version is a fact about the loaded schemas, as the rule
   settings are (ADR-0004).
6. **Leave the filter in `Run`.** Rejected by the owner: `Evaluate` would
   return violations and a count that `0001-F7` B-014 forbids, so the function
   a test calls is not the function the check reports from.

## Decision

- **`Evaluate` takes the rule set as its second argument:**
  `Evaluate(SpecModel model, IEnumerable<ISpecRule> rules)`, static at stage
  B. `Run` loads the model and passes `Discover()`; from stage C (item `0106`)
  it passes the hand-written list.
- **The model carries the pinned vocabulary.** `SpecSchemas` keeps the
  `SchemaVersion` its version-set load selects. `Evaluate` reads the
  vocabulary from `model.Schemas` and applies `0001-F7` B-014's filter there:
  over the rules, over each rule's violations, and over the count of rule ids
  evaluated. `Run` holds no filter.
- **ADR-0004's clause is narrowed, not withdrawn.** Its point stands: the rule
  settings are not a parameter - selection and the severity map read them from
  `model.Schemas.Structure` - and `Evaluate` reads no static. The rule set is
  an input, not a setting.
- **Not decided here, and owed to the `implementer` in `0001-F1` § 7:** what
  the on-disk `SpecSchemas.Load(fileSystem, root)` overload, which takes no
  version set, carries for the vocabulary; and the name and shape of the member
  that exposes it.
- **Not decided here:** whether the instance `Evaluate` of stage D keeps the
  rule-set parameter once the rules are constructor-injected. ADR-0005
  Decision (a)'s sketch is "the shape, not the signatures" and is not amended;
  item `0107` proposes it.

## Consequences

Buys:

- ADR-0001's stage B proof is writable at stage B: a unit test passes fake
  rules and a model built in memory, with no tree and no container.
- `0001-F7` B-014 holds for every caller of `Evaluate`, not only for `Run`.
- No new type, no interface, no static read (ADR-0005 Decision (d)).

Costs:

- A fake rule runs only when the model's vocabulary holds one of its ids, so
  the unit test loads its `SpecSchemas` over a version set it builds.
- `SpecSchemas` gains a member, and its two loads must agree on what it holds.
- The vocabulary filter and ADR-0004's selection are both steps over the rule
  set inside `Evaluate`; item `0014` states their order when it adds selection.
- The records this record amends, each owed and none edited by it beyond a
  Status line:
  - `0001-F1` § 7 ("Who calls it, stage by stage" names `Evaluate(SpecModel)`)
    and § 6: `implementer`, in item `0105`.
  - `0001-F7` § 6 and § 7, which name
    `SpecCheckRunner.Run(string, SchemaVersions)` as where the vocabulary
    decides the run: `implementer`, in item `0105`.
  - `0001-F5` § 6 and § 7, and item `0014`, which write
    `SpecCheckRunner.Evaluate(SpecModel)`: reworded when item `0014` is taken.
  - Item `0106`'s `depends_on` comment, which writes `Evaluate(SpecModel)`.

No verdict changes (`0001-F1` C-9): the golden-report test and item `0012`'s
guard stay green unchanged.
