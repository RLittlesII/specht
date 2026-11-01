---
title: "Decision 0005: Revision-declared identity and the self-hosted pool"
description: "Each measured revision's identity mappings come from the workload identity its own benchmarks declare, with no fallback, and the comparison job runs on the self-hosted pool that runs build and release"
type: decision
---

# Decision 0005: Revision-declared identity and the self-hosted pool

**Date:** 2026-11-01
**Decided by:** repository owner, asked directly while item `0214` was in
progress; spec-author recorded the consequences.

## The call

**Identity mappings (resolves OQ-3).** A benchmark declares the workload
identity it measures, on the benchmark itself. The comparator derives each
measured revision's identity mappings from that revision's own declarations and
from nothing else. A revision whose benchmarks declare no workload identity -
every release published before the declarations land - has no mappings: its
measurement inventory is unknown and the comparison is `Incomplete` with that
reason. No hand-written mapping stands in for a missing declaration, and no
mapping is carried from one revision to another (B-025).

**Comparison runner.** The comparison job runs on the owner's self-hosted pool,
`[self-hosted, macOS, ARM64]`, the label the `build` and `release` jobs already
use (`0055-F2` decision 0005). C-12 holds it there.

## Why

The owner chose between the options below and gave no further reason. What the
chosen options keep: a declaration lives in the revision it describes, so a
workload's identity is read from that revision rather than reconstructed beside
it, and a missing one stays visible under C-5; the pool measures on the hardware
the rest of the release run uses, with no new runner provider.

## Rejected

Each option the owner was shown and did not choose:

- **A hand-audited mapping file in each measured revision.** It too would be
  absent from releases published before it. Cost of rejection: none for
  history; the identity sits on the benchmark instead of in a second file.
- **A hand-audited registry keyed by product commit in the comparator's tree.**
  It could have described releases published before the declarations. Cost of
  rejection: a comparison whose previous release predates the declarations is
  never complete.
- **A GitHub-hosted Linux runner for the comparison job.**
- **A GitHub-hosted macOS runner for the comparison job.** Cost of rejecting
  both hosted runners: the comparison competes with build and release for the
  pool, as the consequences below state.

## Consequences

- **The first comparisons are `Incomplete`.** Until both sides of a release pair
  carry declarations, every comparison reports an unknown inventory. The owner
  was told this widens item `0214`.
- **A declaration is the audit.** It is reviewed with the benchmark it describes.
  A workload change that leaves its declaration unchanged pairs results the
  declaration calls equivalent; nothing beyond review checks that a declaration
  still matches its workload.
- **The rendezvous needs two jobs at once on the pool.** The release job holds a
  runner while its bounded wait for the result runs (§ 7). The rendezvous
  completes only when the pool runs two or more jobs at once; with one runner,
  every release ends `Incomplete` after that wait - advisory, never a failed
  build (B-018, C-1, C-9).
- **With one runner, the comparison job may run first.** Both jobs need only
  `build`, and which takes a single free runner is not fixed. If the comparison
  job takes it, publication waits for that job's bounded receipt wait to end.
  OQ-4 asks the owner whether that delay is acceptable.
- **C-10 on a shared host.** The release job and the read-only comparison job
  can run on one machine of a pool that keeps its working directory between
  jobs; § 7 shows that no publishing credential reaches a measured revision
  there.

## Direction for § 7

Not part of the call; the implementer decides the mechanism and records it in
§ 7. The coordinator's recommendation on 2026-11-01: the declaration type lives
in the benchmark project; the comparator reads declarations from the revision's
built benchmark assembly as metadata, so no code from a measured revision runs
in the comparator, and the harness command line and the `native-v1` export stay
unchanged; each mapping's source file is repository-relative and its source
digest is the SHA-256 of that file at the measured commit, as § 7's inventory
rule already requires.

## Affects

`0109-F5` B-005 to B-008, B-018, B-020, B-024, B-025; A-4; C-5, C-7, C-10,
C-12; OQ-3, OQ-4; § 5 rows 10 and 11; ADR-0001 (narrowed, not superseded: its
"third runner" is a runner of the same pool). `0109-F1` § 5 row 2. The
measuring Features `0109-F2` to `0109-F4` keep their measurement boundaries
(C-3); their benchmarks gain declarations only.

## Reversal

None.
