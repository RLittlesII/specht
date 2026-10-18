---
title: "0201-F5: Trace command and report contract"
description: "Concrete offline reconciliation command, separately versioned report and default passing-outcome gate"
type: design
---

# Trace command and report contract

This is a planned command/report contract, not current CLI help. It implements
F5 decision 0001, F6's coverage/input decisions and the owner's explicit decision
to require passing outcomes by default.

## Command

```text
specht trace [--root <dir>] --evidence <path>...
             [--run <id>] [--json] [--report <path>]
```

The root defaults consistently with the existing tool command. Evidence paths
are explicit files or caller-named artifact directories; enumeration is bounded
to those supplied paths, never a repository-wide evidence search. `--run` selects
a result invocation. Without it, accept the sole matching run; multiple matching
runs require an explicit choice, not a timestamp heuristic.

Default stdout is one human summary with root-relative findings and separate
input, binding, link, exemption and execution counts. `--json` emits only JSON.
`--report` writes the identical JSON to the caller-named path, the only write
performed by this command. Both forms are offline and consume supplied evidence;
they do not build projects, execute tests or launch browsers.

## Exit behavior

| Exit | Meaning                                                                                                                                                         |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `0`  | Required current inputs are complete; every non-exempt required link is satisfied; all current cases supplying required test evidence passed.                   |
| `1`  | Inputs can be evaluated, but required links are missing or required current outcomes are failed, skipped, notRun, aborted or absent.                            |
| `2`  | Required compilation/metadata inputs are unavailable, incomplete, stale or incompatible, including unresolved runtime-only enumeration. Never label this clean. |
| `3`  | Invalid manifest/schema/analysis configuration, consistent with the existing tool's configuration-error category.                                               |
| `4`  | An explicitly named evidence path or requested run cannot be found.                                                                                             |

If failures overlap, select configuration `3`, explicit selection `4`, input
availability `2`, then policy/outcome `1`; still report all findings that can be
established safely. Requiring passing outcomes does not merge evidence states:
a resolved binding with a skipped result remains a valid binding and a
nonpassing execution. No `--require-passing` flag is needed because this is the
default. A links-only success mode is outside this agreement.

Only cases associated with non-exempt required test evidence participate in the
trace outcome gate. Other supplied results remain visible; they do not create
extra required links. Normal test-workflow exit behavior is independent of this
command. Suppression cannot fill required links or turn nonpassing evidence into
passing evidence.

## JSON version one

Use `format: "specht.trace-report"`, integer `formatVersion: 1`, and these required
properties. This is separate from the existing checker report and the evidence
producer format.

| Property     | Contract                                                                                                                                                                 |
| ------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `scope`      | Declared projects/TFMs, selected Feature identities and actual supplied combinations; no machine-absolute paths.                                                         |
| `inputState` | `complete`, `incomplete`, `unavailable` or `invalid`, with typed input findings and fingerprints.                                                                        |
| `claims`     | Every selected current claim, including claims no method references. Identity, lifecycle, source location, required-link states, explicit exemptions and evidence links. |
| `bindings`   | Valid/invalid/unresolved/ambiguous relationships with independent resolution and eligibility states; historical links retained.                                          |
| `cases`      | Actual discovered cases, associated method/binding identities and current or stale discovery state.                                                                      |
| `runs`       | Explicit run identities and per-case outcomes, preserving absent, skipped and stale states.                                                                              |
| `findings`   | Typed reconciliation gaps/input failures plus supplied SPCT analyzer findings; native diagnostic visibility is distinct from evidence validity.                          |
| `summary`    | Counts by the independent states above and the computed exit code. Empty Feature selection is reported as no selected claims, never 100% coverage.                       |

Each required link is `satisfied`, `missing`, `exempt` or `unavailable`; each
exemption carries its explicit reason. A claim with an exemption is visibly
exempt and never increments the covered-claim count. Remaining non-exempt links
and their outcomes are still evaluated. `passing` is an execution state, not a
synonym for link completeness or approval.

Reconciliation gap kinds include `missingRequiredLink`, `missingCompilation`,
`deferredCases`, `missingResult`, `staleEvidence`, `ambiguousRun` and
`invalidInput`. These structured facts are not additional SPEC rules. Supplied
analyzer diagnostics retain their stable SPCT IDs and effective presentation
metadata. The CLI gate evaluates the underlying required evidence independently
of whether such diagnostics were suppressed.

Sort identities by epic presence/value, Feature, target kind and local ID using
ordinal comparison, then project/TFM/symbol/case/run. Sort findings by relative
path, line, kind and identity. Do not include generated-at time, discovery time
or an ambient latest-run choice. Byte-identical logical inputs produce the same
JSON. Keep escaped text and multiline messages valid JSON.

## Composition and ownership

Add `src/tool/Features/Trace/` as a thin Spectre command. It reads explicit
artifacts and current specification inputs, reconciles the supplied records and
folds the report into the documented exit code. Use the existing engine's
manifest/catalog and JSON infrastructure where applicable, preserving the old
checker DTO and serialization. This is CLI-side artifact reconciliation.

The command cannot use the analyzer directly. It does not load the analyzer
assembly, invoke a DiagnosticAnalyzer, create a Roslyn Workspace or compile a
consumer project. Source bindings and their verdicts must already be exported
by build-time collection; test discovery/results come from normal test-workflow
collectors. Absent exports are unavailable inputs, never an empty complete run.
The analyzer's compilation state and syntax objects do not cross this boundary.

The F5 agreement stays at its adopted specification home; this does not require
putting the CLI reconciler into the compiler-loaded assembly. Core CodeAnalysis
implementation and package delivery do not depend on delivering this command.
This adds no mediator, CQRS pipeline or new shared-core assembly.
