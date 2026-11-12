---
title: "ADR-0033: Hand-cited ids are minted by Nano ID; Snowflake is the second strategy, with random fields"
description: "Item 0256's proposal, carrying its own evidence. The sequence, Snowflake as supplied and with derived or random fields, and Nano ID at four sizes were run through a collision simulation of parallel branches cut from one main, per record kind, on an unmerged trial of the owner's Strategy/Decorator design. Proposed: Nano ID in lower-case base32 for every hand-cited type, Snowflake with random datacenter, worker and sequence start as the second built-in, item 0128 kept as the backstop, and online allocation left to ADR-0028. Accepted on the owner's ruling of 2026-11-07."
type: adr
---

# ADR-0033: Hand-cited ids are minted by Nano ID; Snowflake is the second strategy, with random fields

## Status

accepted - 2026-11-07, by the owner, in session, selecting the option "Accept
as proposed". Recorded by spec-author. Decider: the repository owner. Item `0256`
([`.issue/0256-evaluate-snowflake-record-ids.yml`](../../.issue/0256-evaluate-snowflake-record-ids.yml)),
which quotes the owner's request of 2026-10-23 and direction of 2026-11-06.
The design trialled is the owner's, recorded in `0001-F19`
[decision 0001](../../src/specht/Identifiers/.spec/decisions/0001-ids-are-minted-by-a-strategy-with-decorators-opted-in-per-type.md).
Every recommendation below is this record's, not the owner's. The record
amends no claim, renumbers nothing and edits no accepted record. It answers
`0001-F19` OQ-1 to OQ-10 and OQ-12
([`src/specht/Identifiers/.spec/README.md`](../../src/specht/Identifiers/.spec/README.md)
§ 11); settling them in that specification is a separate change. OQ-11 is not
this spike's.

No-default-grammar-change judgement, the OQ-12 answer and the top-level
`mint` key's shape superseded 2026-11-12 from schema `0.4.0` by
[ADR-0036](0036-schema-0-4-0-holds-one-id-record-per-type.md).

## Context

### How to read the evidence

- **Read from a commit**: specification, script and schema text, and the
  volume counts. Baseline is `f8817170` (`main`, 2026-11-06).
- **Measured by a trial**: a build, test or simulation result, produced by a
  command run on the trial commit on 2026-11-06 and 2026-11-07 and not re-run
  for this record. The branch is never merged (item `0256`) and may be deleted
  once the owner rules, so the figures that matter are quoted here.
- **Arithmetic**: computed from the measured or read figures; the formula is
  shown.
- **Judgement**: labelled where it stands. Every judgement is this record's.

| Trial                       | Branch                          | Commit     | Cut from   | Candidates    |
| --------------------------- | ------------------------------- | ---------- | ---------- | ------------- |
| Strategy/Decorator and sims | `0256/trial-strategy-decorator` | `cf626a3a` | `f8817170` | (a), (b), (g) |

Commands, run on `cf626a3a` (macOS, SDK `11.0.100-rc.2.26504.105`):

```sh
dotnet build specht.slnx
test/tests/bin/Debug/net11.0/specht.tests -class "Specht.Tests.IdGeneration.IdGenerationUnitTests"
dotnet run --project trials/0256/IdCollisionSimulation -- stats --root <checkout of f8817170> --repetitions 1000 --seed 256
dotnet run --project trials/0256/IdCollisionSimulation -- git --seed 256
dotnet run --project trials/0256/IdCollisionSimulation -- held --root <checkout of f8817170>
```

The build gave 0 warnings and 0 errors on both target frameworks; the unit
class ran 10 tests, 0 failed. The three runs wrote `trials/0256/stats.md`,
`trials/0256/git.md` and `trials/0256/held.txt` on the branch. Nothing else
was run on the trial: no acceptance tier, no `Pack`, no Windows. No candidate
failed to build or pass.

### What the trial built

Under `src/specht/IdGeneration/` on the branch: `IIdGenerator`, the Strategy
(`string Next()`); `NanoIdGenerator` with `NanoIdSettings` (alphabet, size);
`SnowflakeGenerator` with `SnowflakeSettings` (epoch, datacenter, worker and
sequence widths, datacenter, worker, sequence start), which refuses a clock
read earlier than its last mint and waits out a full sequence; three
decorators, `FormattingDecorator` (prefix, encoding `none` or `base32`),
`ValidationDecorator` (the type's grammar from the manifest's
`identifiers`) and `CollisionGuardDecorator` (draws again while the tree holds
the candidate, case-insensitive, up to a number of attempts); `IdMinter`,
which reads a manifest key `mint.<type>`, refuses a type the key does not name
or `identifiers` holds no grammar for, and composes guard, then validation,
then formatting, around the strategy; `HeldIds`, where the guard reads each
type's ids; and an injected random source, cryptographic in production and
seeded in the simulation. `trials/0256/IdCollisionSimulation/` is a console
project referencing the engine, not in `specht.slnx`.

### Volume

Read at `f8817170`, the first commit being 2026-10-07: about 30 days and 385
squash-merged pull requests.

| Kind                   | Count | Kind            | Count |
| ---------------------- | ----- | --------------- | ----- |
| work items             | 353   | decisions       | 236   |
| epics                  | 13    | claims          | 1,384 |
| Feature specifications | 57    | constraints     | 442   |
| root ADRs              | 32    | open questions  | 426   |
| Feature ADRs           | 7     | scenarios       | 1,638 |
| root lessons           | 16    | Feature lessons | 13    |

`.issue/.sequence` reads `0411`. One duplicate item id is on `main`, `0220`,
two files ([ADR-0025](0025-rule-pages-stay-whole-snapshots-and-single-source-is-the-candidate-if-growth-must-go.md)
§ "Where the item's text is stale"). The largest per-Feature scopes are 154
claims and 74 open questions in
[`src/tool/.spec/README.md`](../../src/tool/.spec/README.md), 22 decisions in
`src/specht/Manifest/.spec/decisions/` and 170 scenarios in
`src/tool/.spec/versioning.feature`.

## Decision drivers

1. Two branches cut from one `main` never take the same id for a type the
   consumer opts in, with no coordinator (`0001-F19` C-7) and no state between
   requests (`0001-F19` C-1).
2. An id stays something a person reads, types and cites: in prose, commit
   subjects, `depends_on`, file names and branch names.
3. A type the consumer does not opt in, and every id already in a tree, is
   untouched (`0001-F19` C-2, C-11; AGENTS.md § Stable IDs).
4. The check stays deterministic and offline, and a consumer that sets no key
   sees no change of verdict (`0001-F19` C-6, C-8, B-014).
5. The owner's design stands: a Strategy with Decorators, Nano ID and
   Snowflake each with its own settings, opt-in per type, no sequence default
   or fallback (decision 0001).

## Considered options

The trial added two variants of (b), each a setting of the supplied
generator, not a new strategy.

| Option | What it is                                                                                       | Status in this record                    |
| ------ | ------------------------------------------------------------------------------------------------ | ---------------------------------------- |
| (a)    | Today's sequence and lesson 0005's procedure                                                     | the measured baseline the owner replaces |
| (b)    | Snowflake as supplied: datacenter 0, worker 0, sequence from 0                                   | measured                                 |
| (b-h)  | Snowflake, datacenter a hash of the machine name, worker a hash of the worktree path             | measured                                 |
| (b-r)  | Snowflake, datacenter, worker and sequence start random per mint                                 | measured                                 |
| (c)    | Time plus random without a worker id, ULID- or UUIDv7-shaped                                     | not built; (b-r) is its shape in 63 bits |
| (d)    | Provisional branch-local ids renumbered by CI on merge                                           | reasoned, not built                      |
| (e)    | A check failing on a duplicate across `main` and the pull request, item 0128                     | reasoned; item 0128's scope              |
| (f)    | Online allocation from one shared sequence                                                       | answered in ADR-0028                     |
| (g)    | Nano ID: random ids from a configured alphabet and size; base32 at sizes 4, 5, 6, 8; url-safe 21 | measured                                 |

- **(a)**: collides `N - 1` times per id per kind whenever `N` branches run in
  parallel (measured, below); the procedure is enforced by review only. The
  owner's direction replaces it (item `0256`, 2026-11-06).
- **(b)**: collides whenever two print-only mints share a millisecond, since
  every field and the sequence are zero; the unit test
  `TwoSuppliedGenerators_WhenEachMintsInTheSameMillisecond_ShouldMintTheSameId`
  shows it. No coordinator exists to hand out distinct fields.
- **(b-h)**: removes collisions on one machine at low concurrency, not at 64
  branches, and depends on identity the owner ruled must not come from git
  (ADR-0028 § "The allocation analysis", point 6) and that CI runners share.
- **(b-r)**: 22 random bits per millisecond; measured 0 collisions everywhere.
  Uniqueness becomes probabilistic, of the same kind as (g), and creation-time
  order is kept. 13 base32 characters.
- **(c)**: ULID is 26 characters and UUIDv7 36, for the shape (b-r) gives in 13. Judgement: covered by (b-r), not built separately.
- **(d)**: an id is cited before the merge, in review threads, commit messages
  (the squash body is built from them), branch names and `@B-` tags;
  rewriting it at merge breaks those citations, needs a bot with write access
  around the merge, and is renumbering by another name (AGENTS.md § Stable
  IDs). Judgement: rejected.
- **(e)**: detection, not prevention. Judgement: still needed for types not
  opted in, for the duplicates already on `main` (`0118`'s history, `0220`) and
  as a backstop for opted-in types. Item
  [`0128`](../../.issue/0128-detect-an-id-claimed-twice.yml) keeps its scope.
- **(f)**: see finding 6.
- **(g)**: measured 0 collisions at size 6 and 8 up to 64 branches; no clock,
  no worker id, no state.

## Findings

### 1. Collisions

The model (`stats` mode): `N` branches are cut from the real tree at
`f8817170`; per-Feature types are scoped to `src/specht/Manifest` (`0001-F5`),
where lesson 0005's incidents happened. Each branch mints one pull request's
worth, 16 ids: epic 1, feature 1, task 2, adr 1, lesson 1, claim 3,
constraint 1, openQuestion 2, decision 1, scenario 3, all per-Feature ids in
one shared Feature, lesson 0005's worst case. The trial's `task` reads
`.issue/` item names and its `epic` reads epic files and item names, the
shared number space. Each mint is a fresh generator, since the command is
print-only (`0001-F19` C-1). The guard sees `main` plus the branch's own ids,
never another branch's. A branch's first mint falls uniformly in a window `W`
after a common start, each next one 0 to 999 ms later. A merge is the union;
a collision is each copy of an id beyond the first. 1,000 repetitions, seed
256; (a) takes the highest held number plus one on the branch's copy.

Runs with at least one collision, out of 1,000, and collisions per run, `W` =
1 s (measured):

| Candidate              | N = 5      | N = 16     | N = 64       |
| ---------------------- | ---------- | ---------- | ------------ |
| (a) sequence           | 1000, 64   | 1000, 240  | 1000, 1008   |
| (b) as supplied        | 100, 0.102 | 690, 1.196 | 1000, 19.872 |
| (b-h) machine and path | 0          | 0          | 349, 0.427   |
| (b-r) random fields    | 0          | 0          | 0            |
| (g) base32, size 4     | 0          | 5, 0.005   | 53, 0.054    |
| (g) base32, size 5     | 0          | 0          | 1, 0.001     |
| (g) base32, size 6     | 0          | 0          | 0            |
| (g) base32, size 8     | 0          | 0          | 0            |
| (g) url-safe, size 21  | 0          | 0          | 0            |

Wider windows, runs with a collision out of 1,000 (measured): (b) at `W` =
60 s, 8, 54 and 619 for `N` = 5, 16, 64; at `W` = 3,600 s, 0, 1 and 23. (b-h)
at `W` = 60 s, 15 at `N` = 64; at 3,600 s, 0. (b-r) 0 in every cell.

Per kind, collisions per run against lesson 0005's incidents (measured; the
lesson column is read, as extra copies):

| Kind         | Scope       | Lesson 0005                      | (a) N=5 | (b) N=64 | (b-h) N=64 | (g) 4, N=64 | (b-r), (g) 6 and 8, N=64 |
| ------------ | ----------- | -------------------------------- | ------- | -------- | ---------- | ----------- | ------------------------ |
| epic         | repository  | 0                                | 4       | 1.349    | 0.021      | 0.001       | 0                        |
| feature      | its epic    | 0                                | 4       | 1.083    | 0.023      | 0           | 0                        |
| task (item)  | repository  | 2, and 1 on `main`; later `0220` | 8       | 3.291    | 0.065      | 0.008       | 0                        |
| adr          | repository  | 0                                | 4       | 0.738    | 0.018      | 0.002       | 0                        |
| lesson       | repository  | 0                                | 4       | 0.726    | 0.016      | 0.005       | 0                        |
| claim        | its Feature | 5                                | 12      | 5.312    | 0.124      | 0.015       | 0                        |
| constraint   | its Feature | 1                                | 4       | 0.584    | 0.014      | 0.004       | 0                        |
| openQuestion | its Feature | 2                                | 8       | 2.145    | 0.050      | 0.010       | 0                        |
| decision     | its Feature | 2                                | 4       | 0.541    | 0.008      | 0.002       | 0                        |
| scenario     | its Feature | 0                                | 12      | 4.103    | 0.088      | 0.007       | 0                        |

Lesson 0005's column: item `0118` claimed by three pull requests, one of the
duplicates reaching `main`, and `0220` later; `0001-F5` decision 0003 claimed
three times; `0001-F5` B-024 to B-026 and `0001-F7` B-037 and B-038 twice;
`0001-F5` C-10 twice; `0001-F5` OQ-2 and OQ-3 twice. (a) at `N` = 5 reproduces
that pattern: every id of every kind taken `N - 1` extra times.

Guard refusals per run (measured): (g) size 4 at `N` = 64, 0.053, digit-only
candidates matching existing `0001`-style ids; (b) and (b-h), 0.03 to 0.393,
two mints on one branch in one millisecond, rescued because the retry
advances the in-process sequence. Across two print-only runs on one branch
the guard rescues only when the first id is already written into the tree.

`git` mode (measured, `trials/0256/git.md`): five real branches from one
synthetic `main`, each mint read by `HeldIds` from that branch's working tree;
file-named records written as `<id>-<branch>.md`, claims, constraints and
questions appended as table rows, scenarios as tags; merged in order, a
conflicted file resolved with `git merge-file --union`.

| Candidate          | Merges git stopped on | Files in conflict | Duplicate ids after merge |
| ------------------ | --------------------- | ----------------- | ------------------------- |
| (a) sequence       | 4 of 5                | 8                 | 60                        |
| (b) as supplied    | 4 of 5                | 8                 | 1 (a claim)               |
| (g) base32, size 6 | 4 of 5                | 8                 | 0                         |

Git stopped only on the appended table rows and `.feature` tags, the same
text conflict for every candidate, never on an id. Every file-named duplicate
merged silently, as lesson 0005 says.

### 2. Worker ids and the clock

Snowflake is unique only while every concurrent generator holds distinct
datacenter and worker values, 10 bits in the supplied layout. Nothing here
coordinates clones, worktrees or agents.

| Assignment            | What shares it, and the cost                                                                                                     |
| --------------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| machine name          | every worktree and agent on one machine; equivalent to (b), measured 100 of 1,000 runs at `N` = 5, `W` = 1 s                     |
| branch hash           | two sessions on one branch; a 10-bit hash reaches a 50% birthday at about 38 branches (arithmetic); reading the branch reads git |
| author                | one author runs many worktrees and agents; as machine name                                                                       |
| worktree path         | identical across machines with one layout, CI runners among them; 32 values in 5 bits; measured 349 of 1,000 runs at `N` = 64    |
| random per mint (b-r) | nothing; 22 random bits per millisecond; measured 0                                                                              |

The supplied 12-bit sequence buys nothing when printing only: each run mints
once, so the sequence is always its start value. Judgement: datacenter and
worker accept a number, for a consumer with a real coordinator, or `random`,
and both default to `random`, as does the sequence start. The trial also
built `machine` and `path` hashes only to measure them; this record proposes
not offering them.

Clock moved back (OQ-2): within one run the generator refuses (unit test
`ASnowflakeGenerator_WhenTheClockMovesBack_ShouldRefuse`). Across runs,
with no state (C-1), nothing can detect it. A backward step re-enters used
milliseconds, and a duplicate needs the same fields and sequence in that same
millisecond: 1 in 2^22 under (b-r), and the guard catches it when the earlier
id is already in the tree. A state file to detect it is ruled out by C-1.

### 3. Citation ergonomics

Snowflake at 2026-11-07T12:00Z with zero fields is decimal
`112521432268800000`, 18 digits, and base32 `033y1mp000000`, 13 characters
(arithmetic). It takes 19 digits from 2033-07-22, and the 41-bit time field
ends 2095-09-07. **Illustrative**: the Snowflake base32 values below carry
nonzero fields in their last six characters, and every Nano ID value is
invented; only the zero-field values above are computed.

| Today                  | Nano ID (repository 8, Feature 6) | Snowflake base32              | Snowflake decimal              |
| ---------------------- | --------------------------------- | ----------------------------- | ------------------------------ |
| `ADR-0002`             | `ADR-7k3m9qzp`                    | `ADR-033y1mp4k2r7v`           | `ADR-112521432268800000`       |
| `0001-F1 B-001`        | `0001-F1 B-7k3m9q`                | `0001-F1 B-033y1mp4k2r7v`     | `0001-F1 B-112521432268800000` |
| lesson 0005            | lesson `4mtx8c2q`                 | lesson `033y1mp4k2r7v`        | lesson `112521432268800000`    |
| `fix(0255): ...`       | `fix(4mtx8c2q): ...`              | `fix(033y1mp4k2r7v): ...`     | `fix(112521432268800000): ...` |
| `depends_on: ["0255"]` | `["4mtx8c2q"]`                    | `["033y1mp4k2r7v"]`           | 18 to 19 digits                |
| `0255-some-slug.yml`   | `4mtx8c2q-some-slug.yml`          | `033y1mp4k2r7v-some-slug.yml` | 18 to 19 digits and the slug   |
| branch `0255/short`    | `4mtx8c2q/short`                  | `033y1mp4k2r7v/short`         | 18 to 19 digits                |

- Lengths: today 4, or 3 after `B-`; Nano ID 8 or 6; Snowflake base32 13,
  fixed; decimal 18, then 19.
- Lower-case Crockford base32, `0123456789abcdefghjkmnpqrstvwxyz` (no `i`,
  `l`, `o`, `u`), reads aloud and types, and has no case to fold, so two ids
  never name one file on a case-insensitive file system (APFS's default, NTFS).
  Nano ID's url-safe default is mixed case and holds `-` and `_`: two ids
  differing in case are one file there, and `-` breaks the `<id>-<slug>` name.
  Judgement: no url-safe default.
- Fixed at 13 characters, Snowflake base32 keeps text order equal to numeric,
  creation, order.
- A shorter Snowflake keeps the collision property only if it keeps the bits:
  dropping high time bits wraps the id after the shortened period; dropping
  the fields leaves time alone, which collides within one millisecond. Base32
  is lossless, so it keeps it. Nano ID's size is its only length lever; the
  probability section prices it.
- Judgement (OQ-4): two encodings, `none`, the strategy's own text (Nano ID
  characters, Snowflake decimal), and `base32`, for Snowflake only. The trial
  refuses `base32` on a Nano ID candidate, which is not decimal.
- The prefix is the form the tree holds, the one the guard reads: the trial
  used `F`, `B-`, `C-`, `OQ-` and `S-`, and none for file-named types. `ADR-`
  and `lesson` stay citation words in prose, as today.

### 4. Ordering

Read at `f8817170`, in
[`next.py`](../../.agents/skills/specht-conventions/scripts/next.py):

| Line | Code                                | What a minted id does there                                        |
| ---- | ----------------------------------- | ------------------------------------------------------------------ |
| 64   | `re.match(r"\d{4}-.*\.yml$", name)` | an item file named by a minted id is skipped; it leaves the queue  |
| 70   | `"id": name[:4]`                    | an 8-character id is truncated to four; two can share a key        |
| 79   | `re.findall(r'"(\d{4})"', ...)`     | minted `depends_on` entries are dropped                            |
| 115  | `order` ends on `item["id"]`        | a string tie-break: random under Nano ID, creation under Snowflake |
| 125  | `re.fullmatch(r"\d{4}", line)`      | a minted id under a milestone in `.issue/.goal` is ignored         |
| 171  | `-int(t[1])`                        | a non-numeric Feature id raises `ValueError`; the script stops     |

`.issue/.sequence` holds the last id claimed; an opted-in type has no counter
and the command writes none (C-1), so the file freezes for that type. A
reader taking numbers as a reading order (ADR-0002 before ADR-0003) loses it
under Nano ID; Snowflake base32 keeps creation order, not merge order. This
repository's own records are out of `0001-F19`'s scope (§ 5 row 8).

### 5. The tool and the schema version

Read at `f8817170`:

- **Frontmatter schemas** under `.specht/schema/frontmatter/` hardcode id
  patterns apart from `identifiers`: Feature `id` `^F[0-9]+[a-z]?$` and
  `epic` `^[0-9]{4}$` (`feature.schema.json` lines 42, 47); task `id`
  `^[0-9]{4}-[0-9]{2}$`, its Feature and epic references (`task.schema.json`
  21, 25, 29); epic `id` `^[0-9]{4}$` (`epic.schema.json` 29); and the
  reference patterns of `children`, `depends_on` and `blocks`. Opting epics,
  Features or tasks in needs the consumer's own disk schemas
  (`schemaSource: disk`) widened, or a shipped schema change.
- **Manifest `identifiers`** (schema `0.3.0`): claim `^B-[0-9]{3}[a-z]?$`,
  constraint `^C-[0-9]+$`, openQuestion `^OQ-[0-9]+$`, task
  `^(?<epic>[0-9]{4})-(?<sequence>[0-9]{2})$`, feature `^F[0-9]+[a-z]?$`, epic
  `^[0-9]{4}$`. No key exists for ADRs, lessons, decisions or scenarios.
- **Readers of those grammars**: claim, `ClaimIdRule` (`SPEC030` below
  `0.3.0`, `SPEC4003` from it), the model, the manifest parser and the
  `.feature` tag parser when the manifest's claim tag form is set; task,
  `ItemSequenceRule` (`SPEC044`, `SPEC5004`), `ItemPlacementRule` and the
  model reader and builder; epic and Feature, `PathIdentityRule`.
- **Engine literals beside the grammars**: `ItemSequenceRule.cs` line 41
  `^\d{4}-(\d{2})$`, so a minted task id numbers 0 and gets no gap check;
  `FeatureTagParser.cs` line 128 `@(B-\d{3}[a-z]?)\b`, the legacy claim tag, so
  a minted claim tag is not read unless the claim tag form is set (`0001-F5`,
  item [`0134`](../../src/specht/Manifest/.issue/0134-configurable-claim-tags.yml)).
  The analyzer under `src/analyzers` was not traced for claim-id parsing; the
  Feature owes that check.
- **Not checked by `specht` at all**: this repository's `.issue/` items (the
  check reads consumers' task files), and ADR, lesson and decision file
  numbers.
- **Consumers**: unaffected unless they opt in. The key is optional and the
  check never mints (`0001-F19` B-014, C-6, C-8).
- **Schema version**: an optional key is a patch
  ([`0001-F7` decision 0022](../../src/tool/.spec/decisions/0022-an-opt-in-addition-below-1-0-0-is-a-patch.md)).
  Shipped: `0.1.0`, `0.2.0`, `0.2.1`, `0.3.0`. Item
  [`0388`](../../src/tool/.issue/0388-schema-0-3-1.yml) (`ready`) cuts `0.3.1`
  and says it adds no key. The key enters the patch the first package to carry
  it ships: `0.3.2` if `0.3.1` ships first, or `0.3.1` if amended before any
  package carries it
  ([lesson 0016](../lessons/0016-a-schema-version-is-published-only-when-a-package-carries-it.md)).
- A change to a shipped default grammar is a schema change (`0001-F7` C-11).
  Judgement (OQ-12): none changes. A consumer that opts a type in widens its
  own, for example claim `^B-([0-9]{3}[a-z]?|[0-9a-hjkmnp-tv-z]{6})$`, which
  keeps every existing id valid.

### 6. Online allocation

Direction (f) is answered in
[ADR-0028](0028-files-stay-the-reading-path-and-online-allocation-waits.md)
§ "The allocation analysis": uniqueness across machines needs one
linearizable authority, and that record recommends against adopting online
allocation now. Its point 3(d) names machine-partitioned ids as this spike's;
finding 2 answers them. This record adds nothing to (f).

### Nano ID collision probability

Arithmetic. Base32 is 32 characters, 5 bits each, so size `L` gives `32^L`
ids: `L` = 4, 2^20 = 1,048,576; 5, 2^25 = 33,554,432; 6, 2^30 =
1,073,741,824; 8, 2^40 = 1,099,511,627,776. The url-safe default, 64
characters at size 21, is 2^126, about 8.5e37.

Two bounds over a scope's life of `n` ids:

- **Without credit for the guard** (birthday): `p ≈ 1 - exp(-n(n-1) / (2 · 32^L))`.
- **With the guard**, which refuses anything already on the branch's tree, an
  escape needs two ids minted on different open branches at once. With `c`
  ids of the scope open on other branches at each mint, `p ≈ 1 - exp(-n·c / 32^L)`.
  `c` = 192, 64 open branches of 3 each, pessimistic against today's 0 to 5.

The projection: a repository-wide scope at the last 30 days' pace for 10
years (`× 3650 / 30`, about 121.7); a per-Feature scope at four times today's
largest.

| Scope                   | Today | n in 10 years | L=5 birthday | L=6 birthday | L=8 birthday | L=8, guard, c=192 |
| ----------------------- | ----- | ------------- | ------------ | ------------ | ------------ | ----------------- |
| items and epics         | 366   | 44,530        | 1.0          | 0.60         | 9.0e-4       | 7.8e-6            |
| root ADRs               | 32    | 3,893         | 0.20         | 7.0e-3       | 6.9e-6       | 6.8e-7            |
| lessons                 | 29    | 3,528         | 0.17         | 5.8e-3       | 5.7e-6       | 6.2e-7            |
| claims, largest Feature | 154   | 616           | 5.6e-3       | 1.8e-4       | 1.7e-7       | 1.1e-7            |
| scenarios, largest file | 170   | 680           | 6.9e-3       | 2.2e-4       | 2.1e-7       | 1.2e-7            |

Worked: items at `L` = 8 with the guard, `44,530 × 192 / 1,099,511,627,776 =
7.8e-6`. Per-Feature at `L` = 6 with the guard, `616 × 192 / 2^30 ≈ 1.1e-4`
over a Feature's life. Inverse, `n ≈ sqrt(2 · 32^L · p)`: a 1e-6 birthday
risk at 46 ids for `L` = 6, 1,483 for 8 and 47,453 for 10; a 1% risk at
4,634, 148,291 and 4,745,313.

Judgement (OQ-3): size 8 for repository-wide types, size 6 for per-Feature
types.

## Decision

Proposed. Each part is a judgement resting on the evidence above; the owner
decides. Nothing is adopted until then.

- **Nano ID for every hand-cited type - judgement.** Lower-case Crockford
  base32, size 8 for repository-wide types (items and epics, Features, tasks,
  ADRs, lessons) and size 6 for per-Feature types (claims, constraints, open
  questions, decisions, scenarios). It measured no collision at those sizes up
  to 64 parallel branches, needs no clock, worker id or state, and is the
  shortest form whose 10-year risk stays at or below 1e-3 without the guard.
- **Snowflake as the second built-in, with random fields - judgement.** Kept
  because the owner directed both strategies. Datacenter, worker and sequence
  start default to `random`; a number is accepted for a consumer that has a
  coordinator. Base32 encoding, 13 characters. Recommended where creation
  order matters more than length, such as epic `0324`'s lifetime keys, not
  for hand-cited types. The supplied fixed fields are not recommended: they
  collide whenever two print-only mints share a millisecond.
- **Item 0128 stays - judgement.** Detection remains the guard for types not
  opted in and for the duplicates already on `main`, and a backstop for
  opted-in ones.
- **(c) is covered by (b-r); (d) is rejected; (f) stays with ADR-0028 -
  judgement.**
- **No shipped default grammar changes - judgement.** A consumer that opts a
  type in widens its own grammar; four optional `identifiers` keys, `adr`,
  `lesson`, `decision`, `scenario`, enter with no shipped default.

## Answers to `0001-F19`'s open questions

Proposals for the owner; each question is resolved in `0001-F19` § 11 only
after the owner rules.

| OQ    | Proposed answer                                                                                                                                                                                                                                | Evidence       |
| ----- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------- |
| OQ-1  | Datacenter and worker are a number or `random`, default `random`; machine, branch, author and path assignment are not offered.                                                                                                                 | findings 1, 2  |
| OQ-2  | Within a run the generator refuses; across runs nothing detects it, a duplicate needs the same 22 random bits in a re-entered millisecond, and the guard catches one already in the tree. No state file.                                       | finding 2      |
| OQ-3  | Alphabet `0123456789abcdefghjkmnpqrstvwxyz`, size 8 repository-wide and 6 per Feature; the alternative is no default, every type names both.                                                                                                   | probability    |
| OQ-4  | Encodings `none` and `base32`, base32 for Snowflake only; base32 is lossless and keeps the collision property; a truncated Snowflake does not.                                                                                                 | finding 3      |
| OQ-5  | A Gherkin tag `@S-<id>` on the scenario, numbered per Feature, cited `0001-F19 S-7k3m9q` (illustrative); the guard reads `@S-` tags in the Feature's `.feature` files; epic `0324`'s scenario key is the Feature key plus this id.             | finding 1      |
| OQ-6  | The six `identifiers` keys, `epic`, `feature`, `task`, `claim`, `constraint`, `openQuestion`, plus `adr`, `lesson`, `decision`, `scenario`. A work item is `task`, the consumer's task files; this repository's items are not.                 | finding 5      |
| OQ-7  | A top-level key `mint`, an object keyed by type (shape below), in a patch: `0.3.2`, or `0.3.1` amended before it ships.                                                                                                                        | finding 5      |
| OQ-8  | `specht mint <type> [--feature <dir>] [--root <dir>]`, one id per request.                                                                                                                                                                     | Feature, below |
| OQ-9  | Where `HeldIds` looks, per type below; case-insensitive; 8 attempts by default; every attempt held refuses the request, exit code OQ-11's.                                                                                                     | finding 1      |
| OQ-10 | A type opted in needs a grammar in `identifiers`; the patch adds `adr`, `lesson`, `decision` and `scenario` with no shipped default; opting a type in without one is an invalid manifest (B-015, exit `3`). The grammar must admit minted ids. | finding 5      |
| OQ-12 | No shipped default grammar changes; the consumer widens its own.                                                                                                                                                                               | finding 5      |

On OQ-9's attempts: at items' 10-year volume and size 8, one candidate is
held with probability about `44,530 / 2^40 ≈ 4e-8` (arithmetic), so eight
attempts are not spent in practice. A retired id no longer in the tree is
invisible to the guard; under a random strategy its re-issue odds are the
collision odds.

## The Feature to specify next

`0001-F19` already exists, `draft`, at
[`src/specht/Identifiers/.spec/`](../../src/specht/Identifiers/.spec/README.md),
in epic `0001`, for the command. The MCP tool is `0132-F4` B-023 in epic
`0132`, at
[`src/mcp/Features/Reading/.spec/README.md`](../../src/mcp/Features/Reading/.spec/README.md).
What follows is this record's proposal for their open shape.

- **Command**: `specht mint <type> [--feature <dir>] [--root <dir>]`. One id
  per request. `--feature` names the Feature folder for a per-Feature type
  (claim, constraint, openQuestion, decision, scenario) and is refused for the
  others.
- **MCP tool**: `mint` (`0132-F4` C-7, the command's words after `specht`),
  arguments `type` and `feature`, answering with the command's standard output.
- **Manifest key**: top-level `mint`, checker-owned (ADR-0023), keyed by type.
  Each value: `strategy` (`nanoid` or `snowflake`); `nanoid` (`alphabet`,
  `size`); `snowflake` (`epoch`, `datacenterBits`, `workerBits`,
  `sequenceBits`, `datacenter`, `worker`, `sequenceStart`); `prefix`;
  `encoding` (`none` or `base32`); `attempts`. A strategy's settings live only
  in its own sub-object (`0001-F19` C-3). Snowflake defaults: epoch
  2026-01-01, widths 5, 5 and 12, and `random` for the three fields.
- **Schema version**: the patch that first ships the key, `0.3.2` or an
  amended `0.3.1`, with the four optional `identifiers` keys.
- **Each type's opt-in**:

| Type           | Scope       | Proposed strategy, size | In-tree prefix | Guard reads                                          | Grammar                             | Also needs                                        |
| -------------- | ----------- | ----------------------- | -------------- | ---------------------------------------------------- | ----------------------------------- | ------------------------------------------------- |
| `epic`         | repository  | Nano ID, 8              | none           | epic files' `id:` and item file names                | `identifiers.epic`, widened         | the frontmatter schema patterns widened           |
| `feature`      | its epic    | Nano ID, 8              | `F`            | specifications' `id:`                                | `identifiers.feature`, widened      | the frontmatter schema patterns widened           |
| `task`         | repository  | Nano ID, 8              | none           | task file names                                      | `identifiers.task`, widened         | the schema patterns; `ItemSequenceRule`'s literal |
| `adr`          | repository  | Nano ID, 8              | none           | every `adr/` folder's `<id>-*.md`                    | new `identifiers.adr`               | none                                              |
| `lesson`       | repository  | Nano ID, 8              | none           | every `lessons/` folder                              | new `identifiers.lesson`            | none                                              |
| `decision`     | its Feature | Nano ID, 6              | none           | the Feature's `.spec/decisions/`                     | new `identifiers.decision`          | none                                              |
| `claim`        | its Feature | Nano ID, 6              | `B-`           | first-column rows of the Feature's `.spec/README.md` | `identifiers.claim`, widened        | the claim tag form set                            |
| `constraint`   | its Feature | Nano ID, 6              | `C-`           | as claim                                             | `identifiers.constraint`, widened   | none                                              |
| `openQuestion` | its Feature | Nano ID, 6              | `OQ-`          | as claim                                             | `identifiers.openQuestion`, widened | none                                              |
| `scenario`     | its Feature | Nano ID, 6              | `S-`           | `@S-` tags in the Feature's `.feature` files         | new `identifiers.scenario`          | no rule reads `@S-` tags today                    |

Whether the first slice offers `epic`, `feature` and `task`, which need the
frontmatter schema patterns widened, is the owner's call (decision 5 below).

## Consequences

Under the proposal, these records would be amended or answered; each is owed,
none is made here, and an accepted record is answered by a new one:

- `0001-F19` § 11: OQ-1 to OQ-10 and OQ-12 resolved in place; claims for the
  answers (the `--feature` option, random fields, the encodings, the guard's
  attempts, the scenario tag) added by `spec-author`.
- `0132-F4` B-023 and C-7: the tool's name and arguments.
- Item [`0128`](../../.issue/0128-detect-an-id-claimed-twice.yml): scope
  unchanged; its cases gain minted types as a backstop.
- [`specht-conventions` § Delivery](../../.agents/skills/specht-conventions/references/delivery.md)
  "Reserve an id before a branch claims it": applies only to types not opted
  in.
- [Lesson 0005](../lessons/0005-an-id-is-not-reserved-until-it-merges.md)'s
  remedy: a new record names the mint command as the remedy for opted-in
  types; lesson 0005 is not edited.
- `.specht/manifest.json` and the shipped manifests: the optional `mint` key
  and the four optional `identifiers` keys, in the patch the owner names
  (item [`0388`](../../src/tool/.issue/0388-schema-0-3-1.yml) if `0.3.1`).
- A follow-up item for this repository's own adoption: `next.py`'s patterns,
  `.issue/.sequence`, `.issue/.goal`, the item template's id and AGENTS.md
  § Stable IDs wording.

Untouched under every option: every id already in a tree (`0001-F19` C-11),
the check's verdict for a manifest with no `mint` key (B-014), and the write
invariant (C-1).

## Decisions asked of the owner

1. Adopt the design with Nano ID for every hand-cited type and Snowflake as the
   second built-in with random datacenter, worker and sequence start, or keep
   the supplied fixed fields.
2. Nano ID's defaults: base32 at size 8, and 6 per Feature, or no defaults.
3. The key's name, `mint`, and its shape.
4. The schema patch: `0.3.2`, or `0.3.1` amended before it ships (item
   `0388`).
5. Whether the first slice offers `epic`, `feature` and `task`, which need the
   frontmatter schema patterns widened, or only `adr`, `lesson`, `decision`,
   `claim`, `constraint`, `openQuestion` and `scenario`.
6. The command, `specht mint <type> [--feature <dir>] [--root <dir>]`, and the
   MCP tool `mint` with `type` and `feature`.
7. The scenario id as an `@S-<id>` tag.
8. Whether, and when, this repository moves its own records to minted ids, as
   a follow-up item.
