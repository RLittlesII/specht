---
title: "Decision 0002: Include prereleases and use ten percent review screens"
description: "The predecessor includes published prereleases; timing review requires more than ten percent growth with separated confidence intervals, while allocation review requires more than ten percent growth"
type: decision
---

# Decision 0002: Include prereleases and use ten percent review screens

**Date:** 2026-10-13
**Decided by:** repository owner for release eligibility and initial review thresholds; spec-author elaborated arithmetic and selection boundaries in the draft agreement.

## The call

The owner selected **Use these initial thresholds** when asked whether to flag
a slowdown only above 10% with non-overlapping timing confidence intervals,
and managed-allocation growth above 10%. The owner selected **Include
prereleases** when asked whether only stable releases qualify as predecessors.

Published prereleases and stable releases participate in one sequence. Drafts
and unpublished tags do not. The previous release is the immediately preceding
eligible publication before the current release's first publication, regardless
of whether its version is numerically higher or lower. A rerun retains that pair;
the first publication in the sequence receives `No baseline`.

For positive baseline values, percentage change is computed from unrounded
measurements as `100 × (current − previous) / previous`.

- Time requests review only if the mean rises strictly more than 10% and the current interval's lower bound is strictly above the previous interval's upper bound. Both intervals use the same confidence level. Overlap, touching bounds and exactly 10% growth do not flag.
- Managed bytes per operation request review only above a strict 10% increase. Exactly 10% growth does not flag; timing uncertainty is not an extra allocation condition.
- Missing or invalid measurements receive `Insufficient evidence`. A zero timing baseline is invalid. Allocation staying at zero is unchanged; zero-to-positive allocation has no defined percentage and receives `Insufficient evidence`, with both values and the absolute change still visible.

The last boundary avoids inventing a percentage threshold for a zero baseline;
it is an explicit draft elaboration, not a separate numeric threshold selected
by the owner. Reports record the policy applied. A later change needs a recorded
policy revision rather than silently changing historical findings.

## Why

The percentage screen keeps small changes from creating review work. Separated
timing intervals add an uncertainty screen for noisy measurements. These are
initial advisory criteria, not a statistical test or a promise that an unflagged
release has unchanged performance. Including prereleases makes the report follow
the actual publication sequence the owner chose.

## Rejected

- **Stable releases only:** the owner explicitly chose to include prereleases. Cost of rejection: a stable release may compare against a prerelease rather than the preceding stable version.
- **Flagging every numerical increase:** the owner accepted the initial 10% screens. Cost of rejection: smaller regressions remain visible in the report but do not automatically request review.
- **Timing review from percentage growth alone:** the owner accepted the confidence-interval condition. Cost of rejection: a large but uncertain timing difference may receive no regression flag.
- **Failing the build on a flag:** contradicts the owner and decision 0001. Cost of rejection: review happens without preventing publication.

## Affects

`0109-F5` B-001, B-002, B-003, B-010, B-011, B-012, B-013,
B-016, B-019 to B-021; OQ-1 and OQ-2.
