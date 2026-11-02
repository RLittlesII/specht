---
title: "ADR-0029: Rule ids take four digits in schema 0.3.0, in place, and the published versions keep theirs"
description: "The owner's rulings for item 0169 (ADR-0010 track R): no published package carries schema 0.3.0, so its rule ids become SPEC and four digits in place, by the approved map, and 0.3.0 is relocked; 0.1.0, 0.2.0 and 0.2.1 keep their three-digit ids. The four 0001-F12 rules still to enter 0.3.0 take SPEC4007 to SPEC4010. The 0.1.0 golden report stays and a 0.3.0 one is added. A rule then reports a different public id per pinned version, which the implementer designs and records back."
type: adr
---

# ADR-0029: Rule ids take four digits in schema 0.3.0, in place, and the published versions keep theirs

## Status

accepted - 2026-11-02. Decider: the repository owner, in two sessions:
2026-10-11 (item [`0154`](../../.issue/0154-engine-boundaries.yml), recorded
in [ADR-0010](0010-layered-engine-boundaries-with-io-at-the-edge-and-a-mediator.md)
§ "Rule ids") and 2026-11-02 (item
[`0169`](../../.issue/0169-renumber-the-rule-ids-by-group.yml)). Recorded by
spec-author. It edits no accepted record: ADR-0010 stands, and this record
answers what ADR-0010 left to track R, its open point 7 aside, which it hands
to the implementer (§ "Open points").

## Context

ADR-0010 § "Rule ids" makes every rule id `SPEC` and four digits, the
thousands digit naming the group, and leaves the map and the transition to
track R. Item `0169` is that track. Its criteria 1 to 4 ask, before any id
changes, for this record, the constraint deltas, the approved map and the
claims; criterion 11 covered the other order, a package published first.

What stood on 2026-11-02:

- Schema versions `0.1.0`, `0.2.0`, `0.2.1` and `0.3.0` are embedded
  (`src/tool/schema/`) and locked (`src/tool/schema/versions.lock.json`).
- `0.3.0` was cut by item `0339` (part of `0321`) in `2e849065` and no
  published `specht.tool` package carries it
  ([lesson 0016](../lessons/0016-a-schema-version-is-published-only-when-a-package-carries-it.md)).
- `0001-F7` C-1 freezes a version once a published package carries it; C-17
  lets a locked version no published package has carried be relocked
  (`0001-F7` decision 0014).
- `0.3.0`'s vocabulary is `0.2.1`'s 23 ids: the 21 of ADR-0010's table and
  `SPEC034` and `SPEC035`, the matrix waiver and retired-claim rules
  (`0001-F12`), which ADR-0010's table does not place. Item `0334` then
  added `SPEC032` (below), making 24.
- `0001-F12` decision 0003 has four more rules, `SPEC022`, `SPEC023`,
  `SPEC032` and `SPEC033`, enter `0.3.0` in place, one relock each (items
  `0334` to `0337`). Item `0334` built `SPEC032` and added it to `0.3.0` in
  `9a7174bb`, so this change maps it to `SPEC4009`; the other three are not
  built. The owner's map of ruling 3 did not cover them; ruling 5 does.
- `0001-F1` C-5, C-9 and C-10, `0001-F7` C-11, brief § 4 and AGENTS.md held
  the three-digit vocabulary fixed.

## Decision drivers

1. A published version never changes in place (`0001-F7` C-1).
2. A version no package carries is open to change, and spending a new
   version on a change it can hold leaves a version no consumer can pin
   (lesson 0016).
3. One public id per rule per version: the owner rejected aliases from the
   old ids (item `0154`, 2026-10-11).

## Considered options

1. **Rewrite `0.3.0`'s vocabulary in place and relock it.** Chosen.
2. **A new `0.minor` version, `0.4.0`, beside a frozen `0.3.0`** - item
   `0169` criterion 11's order. Rejected by the owner on 2026-11-02: no
   package carries `0.3.0`, so the order criterion 11 describes does not
   apply (lesson 0016).
3. **Rewrite `0.1.0` in place**, the in-place order item `0169` named when
   it was cut. Ruled out by C-1: `0.2.1`, and the versions below it, are
   carried by a published package.
4. **A new major version.** Rejected by the owner on 2026-10-11 ("Clean
   break. No need for major version schema ... we haven't shipped anything
   ...", item `0154`).
5. **Aliases**: a version that accepts both forms of an id. Rejected by the
   owner on 2026-10-11 (item `0154`).

## Decision

### The rulings

1. **2026-10-11, the owner**: rule ids become four digits with the thousands
   digit naming a group - "Also SPEC0000 would be better. 4 digits and some
   grouping of error types" - renumbered as their own track after the rule
   split, as a clean break: "Clean break. No need for major version schema
   ... we haven't shipped anything ...". Rejected: a new major schema
   version; aliases from the old ids (item `0154`; ADR-0010 § "Rule ids").
2. **2026-11-02, the owner**: the newest schema version in a published
   `specht.tool` package is `0.2.1`. `0.3.0` is unpublished, so the
   four-digit ids are written into `0.3.0` in place. This is the in-place
   order of item `0169`'s criteria 1 and 2, applied to `0.3.0`, not `0.1.0`.
   `0.1.0`, `0.2.0` and `0.2.1` keep their three-digit ids, because `0.2.1`
   is published (`0001-F7` C-1). Criterion 11's "new `0.minor`" does not
   apply.
3. **2026-11-02, the owner**: the map below, numbered sequentially within
   each group in the old order, is approved.
4. **2026-11-02, the owner**: one pull request delivers item `0169`.
5. **2026-11-02, the owner**, answering `0001-F12` OQ-5 after this record
   was first written: the four `0001-F12` rules still to enter `0.3.0` take
   `SPEC4007` to `SPEC4010`, continuing the claims group in the order of
   their old ids (the four rules below).
6. **2026-11-02, the owner**: "a schema `0.2.0` manifest" in `0001-F5`
   B-007, B-036, B-037, B-053 and B-054 means `0.2.0` or later, so those
   claims name the four-digit ids under the reading below.

### Which order applies, and why

The in-place order, applied to `0.3.0`. The order is read from whether a
published package carries the version that changes (`0001-F7` C-1, decision
0007), and the owner confirmed that none carries `0.3.0`. The `0.minor`
order would spend `0.4.0` on a change `0.3.0` can hold and publish `0.3.0`
without it (lesson 0016).

### The map

| Group                    | Old id    | New id     | Rule                                                     |
| ------------------------ | --------- | ---------- | -------------------------------------------------------- |
| Frontmatter              | `SPEC001` | `SPEC1001` | Specification artifacts open with frontmatter            |
| Frontmatter              | `SPEC002` | `SPEC1002` | Feature frontmatter satisfies its schema                 |
| Frontmatter              | `SPEC003` | `SPEC1003` | Item frontmatter satisfies its schema                    |
| Frontmatter              | `SPEC004` | `SPEC1004` | Epic frontmatter satisfies its schema                    |
| Structure                | `SPEC010` | `SPEC2001` | Contracted sections appear once and in order             |
| Structure                | `SPEC013` | `SPEC2002` | Contracted tables carry their headers                    |
| Identity                 | `SPEC011` | `SPEC3001` | Frontmatter identity matches the containing directory    |
| Identity                 | `SPEC012` | `SPEC3002` | A Feature identity is specified in one place             |
| Claims and feature files | `SPEC020` | `SPEC4001` | A specification has one companion feature file           |
| Claims and feature files | `SPEC021` | `SPEC4002` | Scenario claim tags resolve to a claim                   |
| Claims and feature files | `SPEC030` | `SPEC4003` | Claim ids are well formed and unique                     |
| Claims and feature files | `SPEC031` | `SPEC4004` | Every claim has one traceability row                     |
| Claims and feature files | `SPEC034` | `SPEC4005` | A waiver names a signed-off approver and date            |
| Claims and feature files | `SPEC035` | `SPEC4006` | A retired claim is no longer carried                     |
| Work items               | `SPEC040` | `SPEC5001` | Declared children resolve to one item file               |
| Work items               | `SPEC041` | `SPEC5002` | Declared spikes resolve to one spike                     |
| Work items               | `SPEC043` | `SPEC5003` | An item's id and parent match where it sits              |
| Work items               | `SPEC044` | `SPEC5004` | Item ids are unique and contiguous per epic              |
| Dependencies             | `SPEC050` | `SPEC6001` | Dependency edges name a discovered Feature               |
| Dependencies             | `SPEC051` | `SPEC6002` | Dependency edges are declared from both ends             |
| Dependencies             | `SPEC052` | `SPEC6003` | A Feature does not depend on itself or on a cycle        |
| Approval                 | `SPEC060` | `SPEC7001` | A done specification has no missing coverage             |
| Approval                 | `SPEC061` | `SPEC7002` | An approved specification has no unapproved sign-off row |

Each new id's thousands digit is its group's in ADR-0010 § "Rule ids"; no
`SPEC0xxx` id is assigned and no new id is given to two rules. ADR-0010's
table does not place `SPEC034` and `SPEC035`; placing them in group `4xxx`
is derived - proposed by the agent in the 2026-11-02 session and approved by
the owner as part of this map.

### The four `0001-F12` rules still to enter `0.3.0`

The owner's answer of 2026-11-02 to `0001-F12` OQ-5 (ruling 5). Each takes
its id when its item adds it to `0.3.0`'s vocabulary, with a relock
(`0001-F12` decision 0003). `SPEC4009` is built: item `0334` added `SPEC032`
to `0.3.0` in `9a7174bb`, and `0.3.0`'s map gives it `SPEC4009`. `SPEC4007`,
`SPEC4008` and `SPEC4010` are not built. No published version holds an old
id, so each is a working name only, and `SPEC032` is `SPEC4009`'s key.

| Group                    | Old id    | New id     | Rule                                       | Item   |
| ------------------------ | --------- | ---------- | ------------------------------------------ | ------ |
| Claims and feature files | `SPEC022` | `SPEC4007` | A Scenario cell title is a tagged scenario | `0335` |
| Claims and feature files | `SPEC023` | `SPEC4008` | A live claim is carried by a scenario      | `0336` |
| Claims and feature files | `SPEC032` | `SPEC4009` | A test name resolves to a declared test    | `0334` |
| Claims and feature files | `SPEC033` | `SPEC4010` | A § 9 status is borne out by the tree      | `0337` |

### The published versions keep their ids

`0.1.0`, `0.2.0` and `0.2.1` keep their three-digit vocabularies, rule pages,
manifests and lock digests byte for byte (`0001-F7` C-1, B-013, B-015). A
consumer pinned to one of them sees no change. No version holds both forms
of an id (ruling 1: no aliases).

### `0.3.0` is relocked

`0.3.0`'s entry in `src/tool/schema/versions.lock.json` is relocked under
`0001-F7` C-17, since no published package carries `0.3.0`, by decision
0014's procedure: the entry is removed by hand in the reviewed change and the
lock target records it again (`0001-F7` B-092). The other three entries are
not touched.

### The golden reports

`test/tests/Engine/Baseline/engine-e7dba24.json` stays, the verdict of the
baseline tree pinned to `0.1.0` (`0001-F1` B-004, C-10). A second golden
report is added, the same tree's verdict pinned to `0.3.0` (`0001-F1` B-018,
C-10). The pull request delivering item `0169` records its comparison with
the first: it differs in rule ids by the map, in the order the last sort key,
the rule id, gives, and in the verdicts `0.3.0` changes over `0.1.0` on that
tree - `SPEC060`'s delivery axis (`0001-F7` decision 0015) among them - and
in nothing else.

### How a specification names a rule

Spec-author's readings, not stated by the owner:

- A claim, scenario or constraint that describes behaviour under `0.3.0`, or
  under no named pin, names the four-digit id. Under a pin below `0.3.0` the
  same text reads as the three-digit id the map gives (brief § 4). This lets
  one claim hold at every pin, so no claim is duplicated per version.
- A claim explicitly about `0.1.0`, `0.2.0` or `0.2.1`, or about the
  `e7dba24` baseline, keeps the three-digit id.
- Other text that names a three-digit id and no pin - a constraint, a
  design section, a user page - names the same rule under any pin, and is
  renamed when its section is next amended. Item `0169` renames only the
  claims and scenarios (its criterion 4) and the constraints its criterion 2
  lists.
- Review history (§ 12 rows) and decision records keep the ids they were
  written with.
- A three-digit name for a rule no shipped version holds is a working name:
  `SPEC014`, `SPEC015`, `SPEC024` to `SPEC027`, `SPEC070` and the
  `SPEC08x` and `SPEC09x` proposals. Each takes a four-digit id in its group
  when it enters a version, on the owner's approval, as item `0169`'s
  out-of-scope list already says of epic `0101`'s rules. `SPEC022`,
  `SPEC023`, `SPEC032` and `SPEC033` have theirs (ruling 5), and a
  specification that states their `0.3.0` behaviour names the four-digit id.

### The deltas

Amended in this change, each keeping its id: brief § 4; `0001-F1` C-5, C-9,
C-10 and B-001; `0001-F7` C-11 and B-123; `0001-F16` C-3, which said a new
verdict takes a `SPEC###` id; AGENTS.md's invariant, its id schemes and its
CLI examples; `specht-conventions`. The claims and scenarios that name a
mapped id are amended in place (item `0169` criterion 4). Added: `0001-F1`
B-018, the `0.3.0` golden report; `0001-F7` B-151 and B-152, the id a rule
reports under `0.3.0` and under each published version; a `@boundary`
scenario of `0001-F5` B-013, a three-digit id under `0.3.0` being unknown.
On ruling 5: `0001-F12` A-4, B-002, B-006 to B-017 and B-021, and
`0001-F3` B-036, B-037, B-039 and B-040, name `SPEC4007` to `SPEC4010`;
`0001-F12` OQ-5 is resolved; items `0334` to `0337` are retitled. On
ruling 6: `0001-F5` B-007, B-036, B-037, B-053 and B-054 name the
four-digit ids and say `0.2.0` or later.

## Consequences

- A consumer of a published package sees nothing; one that pins `0.3.0`
  reads, configures and explains rules by the four-digit ids, and a
  `0.3.0` manifest naming a three-digit id in `rules` is rejected as any
  unknown id is (`0001-F5` B-013).
- The rule pages of `0.3.0` are renamed and retitled; those of the other
  three versions are not.
- The engine reports one rule under two public ids, depending on the pin.
  Today it cannot (open point 1).
- Once a package carries `0.3.0`, its ids are frozen like any published
  version's (`0001-F7` C-1).

## Open points

1. **A rule's public id per pinned version - ADR-0010 open point 7.** Handed
   to the implementer as this problem:

   > Today `SchemaVersions.Shipped()`
   > (`src/specht/Versioning/SchemaVersions.cs`) returns one id set for
   > `0.2.0`, `0.2.1` and `0.3.0`, and each rule class states one `Id`,
   > which the evaluator uses to select the rule against the pinned
   > vocabulary and stamps on its findings (`0001-F7` B-014). After this
   > change `0.3.0` names its 24 rules by four-digit ids while `0.1.0`,
   > `0.2.0` and `0.2.1` keep the three-digit ones. The same rule must then
   > be selected by, stamped with, levelled by in the manifest's `rules`,
   > ordered by, listed by `specht rules`, set by `specht rules set`,
   > reported under on a fault (`0001-F1` B-013), and explained by
   > `--explain` under the id of the pinned version - `SPEC031` under
   > `0.2.1` and `SPEC4004` under `0.3.0` - and no version may accept both
   > forms. Settle where a rule's per-version id lives, whether the
   > vocabulary stays written twice and held equal by the composition test
   > or one statement is derived from the other, and record the answer here
   > and in `0001-F1` § 7.

   A direction, proposed by spec-author and not a ruling: each rule's `Id`
   becomes its `0.3.0` id; the versions below `0.3.0` carry one map from the
   new id to the old, this record's table as data in `Versioning`; and the
   evaluator selects and stamps by the pinned version's id. The implementer
   settles it in the code and records it back.

   Settled 2026-11-02 by the implementer, item `0169`, in the other
   direction:

   - **A rule's `Id` stays its key: the three-digit id the baseline and
     the published versions give it.** The test-writer's tests for item
     `0169` hold it there. The composition test names each rule by its
     three-digit id, in ordinal registration order, a fault fixture states
     `SPEC030`, and the rule unit tests assert `rule.Id`. Making `Id` the
     `0.3.0` id would have changed those tests to match the code.
   - **Each version maps the key to its own id.** `SchemaVersion.RuleNames`
     holds the map, and `SchemaVersion.RuleId(key)` returns that version's
     id, or the key for a rule the version does not map
     (`src/specht/Versioning/SchemaVersion.cs`). The map is data in the
     existing `SchemaVersions.Shipped` switch
     (`src/specht/Versioning/SchemaVersions.cs`). For `0.1.0`, `0.2.0` and
     `0.2.1` it is the identity on their ids, and for `0.3.0` it is this
     record's table.
   - **A version's vocabulary is derived from its map.** `RuleIds` is the
     set of the map's values, so `0.3.0`'s four-digit ids are written once,
     in the map. No version holds both forms. A three-digit id is not among
     `0.3.0`'s values, so a `0.3.0` manifest naming one in `rules` is
     rejected as unknown, by the same `RuleIds` check as before.
   - **The evaluator selects, levels and stamps by the pinned id.**
     `SpechtEvaluator` (`src/specht/Evaluation/SpechtEvaluator.cs`) reads
     `version.RuleId(rule.Id)` once per rule. Selection uses `RuleIds`, the
     level comes from the manifest's `rules` under that id, and the stamp
     carries it. The order sorts the stamped id, so its fourth key is the
     pinned id. `FaultIsolatedRule` (`src/specht/Rules/FaultIsolatedRule.cs`)
     names a fault by the pinned id (`0001-F1` B-013).
   - **The readers needed no change.** `specht rules`, `specht rules set`,
     `--explain` and the manifest's `rules` validation already read
     `RuleIds` and `RulePages`, and both are in the pinned form.
   - **Open point 7: the rule keys are written twice, the public ids
     once.** The keys appear in the rule classes and in `Shipped`, and no
     test compares the two: the composition test holds the rule classes'
     keys to a list of its own, and `ShippedVocabularies` holds each
     version's ids. A map key that names no rule class leaves that rule
     unselected under its version, which the `0.3.0` golden report shows
     only for a rule the baseline tree breaks. `0.3.0`'s public ids are
     written only in its map.
   - **Rejected: `Id` as the `0.3.0` id with a map down to the old ids.**
     It would also have kept the vocabulary written twice. Its cost was
     rewriting the tests above and every rule class while changing no
     verdict.
   - **No statics, interfaces or options types were added.** The map is a
     property of the version record the loader already builds.

2. **The four `0001-F12` rules that are to enter `0.3.0`.** `SPEC022`,
   `SPEC023`, `SPEC032` and `SPEC033` are to enter `0.3.0` in place
   (`0001-F12` decision 0003), and after this change `0.3.0` holds no
   three-digit id. Their four-digit ids are not in the approved map. Open
   as `0001-F12` OQ-5, for the owner. Resolved 2026-11-02 by the owner
   (ruling 5): `SPEC4007` to `SPEC4010`, in the four rules' table.
