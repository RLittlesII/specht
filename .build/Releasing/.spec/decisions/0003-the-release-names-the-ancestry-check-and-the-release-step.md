---
title: "Decision 0003: the release names, the ancestry check and the Release step"
description: "Records the release names, ancestry check and second-job publication; a later owner correction rejects the attribution of the no-publish-target inference"
type: decision
---

# Decision 0003: the release names, the ancestry check and the Release step

**Date:** 2026-10-11
**Decided by:** the repository owner, 2026-10-11, in the planning session for item `0084`; recorded by spec-author

Four rulings, and one closing rule approved with the plan. This record
supersedes [decision 0001](0001-the-push-runs-in-a-second-job-of-the-same-workflow.md)
on two points only: the names (OQ-6), and "No `Publish` target is built". The
two-job shape, the hand-off as a run artifact and the three readings of decision
0001 stand.

## The call

### 1. `VerifyTag` checks ancestry (OQ-12)

`VerifyTag` fails unless the tagged commit is reachable from `origin/main`. A
correctly named `v*` tag on a commit that is not on `main` publishes nothing
(B-011).

Stated limit: a tag's run takes its workflow file and its build project from
the tagged commit. The check guards against a mistake. It does not guard
against a commit that removed the check.

### 2. The target is `Release`, and it is an aggregate

The NUKE target `Publish` is renamed `Release`. It stays an aggregate of
`VerifyTag` and `Pack` with nothing to execute. The package push and the GitHub
Release creation are run steps of the second job. No target pushes a package or
creates a Release.

### 3. The rename reaches everything

| Was                             | Is                              |
| ------------------------------- | ------------------------------- |
| target `Publish`                | target `Release`                |
| `.build/SpechtBuild.Publish.cs` | `.build/SpechtBuild.Release.cs` |
| `PublishMiddleware`             | `ReleaseMiddleware`             |
| workflow `publish.yml`          | workflow `release.yml`          |
| job `publish`                   | job `release`                   |

The `build` job and the target `VerifyTag` keep their names.

### 4. `gh` creates the GitHub Release, in the second job

`gh release create`, with generated notes and tag verification, attaching the
package, runs as a step after the push in the `release` job (B-004, B-006,
B-007). That job therefore gains `contents: write` beside `packages: write`.
The workflow-level token and the `build` job hold neither (C-7).

### The closing rule for item `0084` (OQ-13)

A default derived in planning and approved by the owner as part of the plan,
not a ruling given in the owner's words.

- The pull request that changes the workflow merges with item `0084`
  `in-review`.
- The owner starts a dry run on `main`, cuts the tag with `dotnet nbgv tag` and
  pushes it.
- A follow-up pull request records the dry run and the tag's run in § 8,
  settles the § 9 rows and writes the item `done` with its `closed:` date.

Under § 5 #9 no § 9 row will name a test. What settling the rows means, and on
what record the item is written `done`, is OQ-16: open, and not put to the
owner.

## Why

The owner's answers, as given on 2026-10-11:

- On OQ-12: "VerifyTag checks ancestry".
- On the drift between decision 0001 and § 5 #7, which say no `Publish` target
  is built, and commit `dc59b81`, which added an aggregate `Publish` target that
  pushes nothing: "Change Publish to Release". Asked what the target then does:
  "Aggregate only; gh step".
- On how far the rename reaches: "Everything".

Ruling 2 is consistent with `0055-F1` C-1 as decision 0001 reads it: a workflow
line that is not a gate needs no target.

The closing rule follows from how GitHub runs a tag: from the tagged commit's
own workflow file. The tag is pushed only after the workflow change is on
`main`, so the pull request that changes the workflow cannot record the publish
that proves it.

## Rejected

**On OQ-12: a tag-protection repository setting alone.** Cost of rejecting: the
check lives in the tagged commit's own build project, which is the stated
limit above.

**On OQ-12: accepting the risk and leaving it to `0055-F5` A-1.** Cost of
rejecting: one more comparison in `VerifyTag`, and a run that needs `main`'s
history, which the full-depth checkout already fetches.

**A target that pushes.** Decision 0001 rejected it and this record keeps that:
C-1 rules out a publish one command away on a developer's machine. Cost of
rejecting: two run lines in a workflow that no target runs.

**A third job holding `contents: write` alone.** Cost of rejecting: the one job
that pushes the package also holds the token that writes a Release, so the two
grants are not separated from each other. Both stay out of a dry run and out of
the job that runs the repository's code.

**A third-party release action pinned in build source.** Cost of rejecting: the
Release step is a command line assembled in the enhancement, as the push is.

## Affects

- B-011: added (ruling 1). Its scenario is in `release.feature`.
- B-012: added by spec-author, derived from B-006 for a tag with no earlier
  `v*` tag. Not one of the owner's rulings, and not observed before the first
  tag.
- B-008: the job it names is `release`.
- C-5: the workflow's own token is also the credential that creates the
  Release.
- C-7: `packages: write` and `contents: write` belong to the `release` job
  only. Ruling 4 is the owner's confirmation that the grant lives in the second
  job, which decision 0001's addendum records as awaited.
- § 5 #7: reworded. No target pushes; `Release` is an aggregate.
- OQ-6: amended with the new names. OQ-12: resolved. OQ-13: added and
  resolved. OQ-14: added, open. OQ-15 and OQ-16: added after spec-reviewer
  round 5 (§ 12), open.
- Decision 0001: superseded on the names and on "No `Publish` target is built";
  its Reversal section points here. Decision 0002: unchanged.
- § 6 and § 7 (`implementer`), § 8 and § 9 (`test-writer`): rewritten in the
  same change to the shape this record gives, and reviewed in § 12 round 5
  (`spec-reviewer`).
- Items `0083` and `0171`: unchanged, as records of their day.
- Item `0083`'s post-merge dry run, which decision 0002 asks for, was never
  recorded: § 8 still says it has not happened. The dry run of the closing rule
  pays it.

Consequence, partial failure: the push and the Release creation are two steps.
The push step gains `--skip-duplicate`, so a rerun after a failed Release step
reaches that step. It replaces nothing, so § 5 #4 holds. Not one of the owner's
rulings: the flag was proposed in planning and built by `implementer`,
2026-10-11, and the owner has not been asked. Whether it needs a claim is
OQ-14.

## Correction - 2026-10-20

The owner clarified in review of item `0250` that the exclusion of NUKE publish
targets was not their decision. The quoted planning exchange above named an
aggregate `Release` target and a `gh` step for that implementation; it did not
settle a lasting rule against targets for publication. The no-target reading
in this record and in decision 0001 was a design inference by the recorder.
Item `0250` keeps the two-job permission boundary and moves the push and
GitHub Release creation into guarded NUKE targets.

## Reversal

Item `0250` supersedes the no-publish-target design inference. The two-job
shape, release names, ancestry check and permission boundary remain in force.

## Addendum - 2026-10-14: close the first publication on its evidence

The owner directed: "Reconcile the tracker with what shipped in 0.1.90,
close the items the evidence satisfies, and rerun next so we can see the
actual remaining work."

For this reconciliation, OQ-16's proposed record-based closing rule is
adopted for B-003, B-004, B-006, B-007, B-011 and B-012. The published
`v0.1.90`, its successful run, the existing ancestry-refusal hand run and
the manual dry run on `main` are recorded in § 8. Their § 9 rows remain
`Missing`: these are records, not automated tests. This is the same scoped
exception decision 0002 gives the earlier workflow claims, not a change to
the traceability vocabulary or a general waiver for other items.

The publication happened on 2026-10-13. Its follow-up dry run and tracker
reconciliation occur on 2026-10-14; the record does not imply that the dry
run preceded the tag. No new tag, package or Release is required to close
the historical first-publication item. The original ancestry check retains
its record-based proof, including its non-zero-exit decision; no new test
requirement is introduced for B-011.

The tagged tree already contains the completed upstream-source prerequisites
and both setup-guide items. OQ-17 and OQ-18 therefore hold no remaining
first-release work. This observation chooses no general rule about future
release scope. Feed installation, build and workflow consumption, the
package-version independence proof and later capabilities remain separate
items; none is closed from a successful publication alone.
