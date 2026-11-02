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

## Addendum - 2026-11-01: one release at a time on a shared host

**Asked** directly in the session while item `0214` was in review: when
`release` and `comparison` run at the same time on the pool, what keeps a
measured revision away from the release job's write token? The options offered
were separate operating-system accounts, separate machines, or the same
operating-system account.

**The owner's answer**, verbatim (read "teo" as "two"):

> This is the same problem you'd have without everything co-located on the same machine. You have to explicitly NOT kick off teo releases to ensure this. :shrug:

### The call

- The design does not rely on isolation between the pool's runners on one
  host; no separate operating-system account or machine per runner is assumed.
- The owner accepts the exposure that remains when a release run's `release`
  and `comparison` jobs share a host as no different from the exposure without
  co-location.
- The safeguard the owner names is that two releases are never started at
  once. A-5 records it.

### Rejected

The owner chose none of the three host arrangements and named the release
practice instead. Separate operating-system accounts and separate machines are
therefore not requirements of the design. Cost of rejection: within one release
run, nothing in the host keeps a measured revision from the release job's step
environment while `attach` waits.

### Consequences

- **The "C-10 on a shared host" consequence above is corrected.** § 7 shows the
  job-scope boundary: the comparison job is granted no publishing credential,
  its measured processes receive none, and no artifact carries one. It does not
  show a host boundary. C-10's meaning is unchanged: no publishing credential is
  granted to measurement or handed from one job to another.
- **ADR-0001 is not edited.** Its credential separation is the separate job's
  permission boundary, which still holds; it never claimed a host boundary.
- **The one-runner consequence above is corrected.** "Every release ends
  `Incomplete` after that wait" holds when the release job takes the single
  runner first. When the comparison job takes it, the comparison ends at its own
  receipt wait and the release job reports that result at once (§ 7). A-4 and
  OQ-4's default say so.

### Affects

`0109-F5` A-4 and OQ-4's default (corrected), A-5 (added), C-10 (unchanged),
and the § 7 sentence on what keeps the release job's token from a measured
revision on the same host.

### Entry - 2026-11-01: the nuget.org OIDC credential

**Asked**, after `0055-F6` item 0327 gave the `release` job
`id-token: write` and item 0214 put `attach`'s wait in the same job: "Does
A-5's accepted exposure also cover the nuget.org OIDC credential?"

**The owner's first answer**, verbatim:

> Yes, accept it

**Asked** "Which answer stands for the NuGet credential?", **the owner's
answer**, verbatim:

> Separate publish job

The second answer supersedes the first. The exposure this addendum accepts is
the workflow token's only. `0055-F6`
[decision 0008](../../../../.build/Releasing/.spec/decisions/0008-the-push-runs-in-its-own-publish-job.md)
moves the login and the push into a `publish` job, the only one granted
`id-token: write`, which needs `build` and finishes before the `release` job
uploads the receipt that starts collection; the `release` job keeps contents
write, actions read and the comparison steps, with `id-token: none`.

**Rejected:** accepting the exposure for the OIDC credential, the first
answer.

**Affects:** A-5, amended to the workflow token; C-10, clarified with a dated
note, its meaning unchanged.
