---
title: "ADR-0001: Read-only collection with a release-job rendezvous"
description: "Keep measurement on a read-only runner while the existing release job persists the pair and attaches advisory evidence through bounded same-run artifact handoffs"
type: adr
---

# ADR-0001: Read-only collection with a release-job rendezvous

## Status

Accepted design, 2026-10-13. Codex self-review; no independent review or live
workflow execution is claimed. Implementation and its verification remain pending.

## Context

`0109-F5` B-001 and B-004 require collection after successful publication, with
both exact published revisions measured together. B-002 fixes the predecessor on
reruns; B-018 keeps every post-publication reporting failure advisory; B-022 keeps
the evidence with the Release. C-10 and `0055-F6` C-7 restrict write authority to
the existing release job. The latter's generator is Rocket.Surgery.Nuke 2.9.12,
with explicit job and step middleware; replacing it is not an option for this
Feature. The build currently hands the package from `build` to `release`.

## Considered options

1. Make collection depend on `release`, then add an attachment job. Rejected:
   attachment needs contents-write outside the existing release job (C-10).
2. Make `release` depend on collection. Rejected: measurements would precede
   successful publication, or create a completion dependency cycle (B-001).
3. Run measured revisions inside the credentialed release runner with scrubbed
   process variables. Rejected: environment scrubbing alone does not isolate the
   job token, persisted checkout credentials or runner command channels (C-10).
4. Start another workflow or use a stored dispatch token. Rejected: adds authority
   and abandons the required release-run ownership without agreement.
5. Start a read-only peer job after `build`, with publication and result receipts
   exchanged while `release` is still running. Chosen: the publication-to-collection
   sequence is explicit without a job completion cycle or a credential exception.

## Decision

The implementation contract is [§ 7 of the specification](../README.md#7-technical-design).
Both `release` and the read-only `comparison` job depend on `build`. After
publication, `release` persists an insert-only pair asset and uploads a same-run,
same-attempt receipt. Only that receipt starts collection. The collector returns
a self-contained bundle through a second same-run artifact; `release` validates
and attaches it. Pair persistence is independent of both temporary artifacts.
Bounded waits and advisory outcomes handle missing peers, failed stages and
cancellation. Exact revision builds and measurements run only in `comparison`.

The comparator is compiled separately from benchmarks and product code. Its pure
selection, pairing and evaluation functions consume immutable data; existing
filesystem, HTTP-handler, process-delegate and clock seams support offline tests.
Reports and evidence are one versioned, hashed closure. Every later stage retains
known flags separately from reporting completion.

### Feasibility evidence and limits

On 2026-10-13 a disposable local change to `ReleaseMiddleware` appended a
`comparison-probe` job with `Needs = ["build"]`, contents-read/actions-read,
packages-none, the existing tag-push condition and a `ContinueOnError` run step.
`./build.sh Compile --skip Format` compiled the build assembly and regenerated
`release.yml` with exactly that job. A local assertion verified the third job's
read-only scopes and advisory step, and exactly one contents-write/package-write
grant still in `release`. No workflow was pushed or run. The temporary source and
all generated files were restored byte-for-byte; the invocation later stalled
in solution restore, so this probe is not a claim that the Compile target passed.
Its local output is retained with the delivery report under `.artifacts/0209/`.

The [v4 upload-artifact contract](https://raw.githubusercontent.com/actions/upload-artifact/v4/README.md)
states that uploaded immutable artifacts become available before the workflow
finishes. The [run-artifact API](https://docs.github.com/en/rest/actions/artifacts)
allows listing and downloading with actions-read. These support the rendezvous;
job outputs that require a producer's completion do not. The design uses those
provider contracts, not an observed concurrent runner execution. Implementation
must verify exact artifact availability/identity and generated step order; an
unsupported provider response is advisory incomplete, never a reason to add write
authority. No disposable release is allowed as validation.

The probe establishes the generator's third-job/scopes/step extension points.
It does not prove live artifact transport, queue timing, job timeout properties,
publication, credential isolation by experiment, or historical harness compatibility.
The separate job's permission boundary, rather than a same-job environment edit,
provides the credential separation. An explicit user cancellation or hard platform
kill can prevent final summary/attachment and cannot be made a successful run;
persisted pair and available summary remain recovery evidence.

## Consequences

C-10 and `0055-F6` C-7 stand without amendment. Collection waits for positive
publication evidence, the baseline survives retries, and reporting cannot block
publication. A third runner may wait idle and the release runner remains occupied
while collecting. Artifact polling requires actions-read in the two participating
jobs and bounded asynchronous waits, with an injected clock in tests.

Unknown historical workloads/exporters or unavailable revisions can make early
comparisons incomplete. The implementation cannot hide that by transplanting
workloads, substituting an older release, or weakening a measuring Feature's
boundary. Live platform availability remains a delivery observation, separate
from offline comparator proof. The testability plan is [§ 8](../README.md#8-testing-strategy).
