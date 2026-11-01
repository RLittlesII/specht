---
title: "ADR-0027: ADR-0010's structural stages proceed on a no-regression guard"
description: "The repository owner lifted ADR-0014's benefit-evidence gate for ADR-0010's remaining structural stages. Each stage's pull request shows instead that the whole check has not regressed against main, at 100 and 1,000 specifications, judged by the review screens item 0211 built; a flag holds the merge until the owner rules, and nothing automated blocks. Order: this record, then 0100, then the stage-3 splits, then 0153, then the later stages."
type: adr
---

# ADR-0027: ADR-0010's structural stages proceed on a no-regression guard

## Status

accepted - 2026-10-31. Decider: the repository owner, who chose, in a session
the same day, one option on each of four questions, in these words:

1. Gate policy: "Lift + no-regression (Recommended)".
2. Threshold: "0211 rule, whole check (Recommended)".
3. Enforcement: "Owner decides (Recommended)".
4. Order: "ADR, then 0100, then splits (Recommended)".

Supersedes in part
[ADR-0014](0014-architecture-rollout-requires-benchmark-evidence.md) and
[ADR-0010](0010-layered-engine-boundaries-with-io-at-the-edge-and-a-mediator.md),
the clauses named under "What this supersedes". Each body is unedited and each
Status gains one line pointing here, as ADR-0026's partial supersession did.
[ADR-0026](0026-the-source-generated-mediator-measured-no-benefit-and-direct-calls-stay.md)
stands whole.

## Context

ADR-0014 holds ADR-0010's rollout until a repository benchmark comparison shows
a benefit the owner judges worth its cost, with no numerical threshold.
ADR-0026 answered that for the mediator, unfavourably, and left the other
stages gated, pending the owner. Stages 2 (item `0155`) and 5 (item `0165`)
are done. What remains:

| Stage | Items             |
| ----- | ----------------- |
| 1     | `0100`            |
| 3     | `0156` to `0163`  |
| 4     | `0164`            |
| 6     | `0153` and `0166` |
| 7     | `0167`            |
| 8     | `0144`            |

None of them is dispatch, so ADR-0026's measurements say nothing about them.
Each is a refactor already proven by ADR-0010's per-stage proof: the golden
report `test/tests/Engine/Baseline/engine-e7dba24.json` byte-identical and
the whole suite green.

Item `0211` built a regression rule over BenchmarkDotNet evidence:
`.performance/comparison/Evaluation/ComparisonEvaluation.cs` under
`ComparisonPolicy.Initial` (`.performance/comparison/Model/ComparisonPolicy.cs`),
the screens of `0109-F5` B-011 and B-012.

## Decision drivers

- ADR-0010 owner decision 7: start-up time is a driver, so a stage must not
  make the check slower or hungrier unseen.
- A structural stage's benefit is structure - ADR-0010's Consequences,
  "Buys" - which a benchmark does not measure. **Judgement**: benefit evidence
  per stage would hold every stage indefinitely.
- An existing, tested rule is preferred to a new threshold.
- `0109-F5` C-9, performance reporting is advisory, and the benchmarking
  reference's "Nothing gates on time".

## Considered options

1. Keep ADR-0014's gate: each stage brings benefit evidence the owner judges
   worth its cost. Not chosen.
2. Lift the gate; each stage shows no regression on the whole check, judged
   by item `0211`'s rule, and the owner rules on a flag. **Chosen.**
3. Lift the gate with no guard; ADR-0010's per-stage proof alone. Not chosen.
4. Lift the gate behind an automated guard that fails the build. Not chosen.

## Decision

ADR-0010's remaining structural stages proceed without benefit evidence. Each
stage's pull request keeps ADR-0010's proof and adds the guard below.

### The guard

- **What is measured.** `CheckBenchmarks.Check`
  (`.performance/benchmarks/Check/CheckBenchmarks.cs`), the whole check, at 100
  and 1,000 specifications, BenchmarkDotNet's default job.
- **Against what.** The `main` commit the branch is cut from, and the branch
  head, on the same host, back to back. Each checkout runs its own
  `CheckBenchmarks`.
- **The rule.** `0109-F5` B-011 and B-012, as `ComparisonEvaluation` applies
  them under `ComparisonPolicy.Initial`. Today a metric is flagged when:
  - the mean rises more than 10% **and** the head's confidence interval lies
    strictly above the base's; or
  - the allocation rises more than 10% over a non-zero base.

  BenchmarkDotNet's `Error` column is the half-width of the 99.9% confidence
  interval, so an interval is `Mean ± Error`. A later policy version changes
  the numbers; the rule is the policy's, not this record's.

- **What is recorded.** For each run: the commit, the host (processor, OS, SDK
  and runtime from the summary header), the load average, and at each size
  `Mean`, `Error` and `Allocated`, with the judgement per metric. The numbers
  go in the pull request description and in a `decisions` entry of the
  delivering item, so they survive the squash.

```sh
base=$(git merge-base origin/main HEAD)
git worktree add --detach ../guard-base "$base"
# in ../guard-base, then in the branch checkout:
uptime
./build.sh Benchmark --benchmark-filter '*CheckBenchmarks.Check(Specifications: 100)'
./build.sh Benchmark --benchmark-filter '*CheckBenchmarks.Check(Specifications: 1000)'
```

The filter matches BenchmarkDotNet's per-case name, so each command runs one
size (measured 2026-10-31: a `Dry` run of the first ran only the 100 row). Each
run rewrites `.artifacts/benchmarks/results/`, so the rows are recorded before
the next run. `git worktree remove ../guard-base` cleans up.

### Enforcement

A flag holds the merge until the owner accepts or rejects it; the ruling is
written into the delivering item's `decisions`. Nothing automated blocks: no
CI step and no build target fails on the guard.

### Order

This record, then `0100` alone, then the stage-3 splits `0156` to `0163`, then
`0153`, then the later stages in ADR-0010's order.

### What this supersedes

| Record   | Superseded                                                                                                                                                                                                           | Stands                                                                                                                                                                                                           |
| -------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| ADR-0014 | For ADR-0010's remaining structural stages: Decision ¶ 1, the wait for benefit evidence; ¶ 2's "No numerical adoption threshold is invented here"; Consequences' "Structural engine/host work waits on item `0216`". | Trial code does not merge as production before a decision; no handwritten intermediary; `0167` redesigned from the evidence; the PR #185 paragraph; an accepted target is not permission to skip a prerequisite. |
| ADR-0010 | § "The order of work": stage 6's "After" column for item `0153`, which now also follows stage 3.                                                                                                                     | Every stage's scope and proof, the deltas owed, tracks T, R and P, and the rest of "What runs in parallel".                                                                                                      |

## Consequences

- No code changes. Each stage's pull request runs the guard.
- Items, each with a `decisions` entry on this record and a criterion for the
  guard:

  | Item             | Status                                  | Why                                                                                                                                      |
  | ---------------- | --------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- |
  | `0100`           | `blocked` to `ready`                    | `0150` and `0216` are done                                                                                                               |
  | `0156` to `0163` | `blocked` to `ready-for-implementation` | each gains `0100`, by the order                                                                                                          |
  | `0153`           | `blocked` to `ready-for-architecture`   | its criteria 3 and 5 are still open; it gains `0156` to `0163`, by the order                                                             |
  | `0164`, `0166`   | `blocked` to `ready-for-implementation` | the design is ADR-0010's; dependencies are open                                                                                          |
  | `0167`, `0144`   | `blocked`, unchanged                    | the rescoping ADR-0026 requires was written 2026-10-31 with judgements for the owner's review; this record lifts the benchmark gate only |

- `0216` stays in each `depends_on` as the record of the evaluation.
- Guidance revised: `AGENTS.md` § Architecture, the `specht-conventions`
  coding and benchmarking references, the `dotnet-tool` vertical-slice
  reference, and the roadmap's 0.4.0 milestone.
- Item `0218`'s summary and out-of-scope row describe `0153` as gated by
  ADR-0014. Named, not edited.
- Lesson 0009 is history and is not edited. No Feature specification is
  edited.
