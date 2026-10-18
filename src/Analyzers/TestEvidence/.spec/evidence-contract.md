---
title: "0201-F3: Runner evidence contract"
description: "Version-one framework-independent discovery, binding and result envelopes with stable correlation and freshness rules"
type: design
---

# Runner evidence contract

This planned version-one contract implements decision 0001 and F6 decision 0004.
Collection adapters are producers; the checker never launches a runner to obtain
missing data. Framework names are descriptive metadata, never an allowlist.

## Envelope

Use UTF-8 JSON with `format: "specht.test-evidence"` and integer `formatVersion: 1`.
Reject unsupported versions explicitly. Required properties are below; nested
objects are closed for version one. Additions requiring different interpretation
need a new format version, independently of package and manifest schema versions.

| Property            | Contract                                                                                                                                                                                                |
| ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `project`           | Root-relative `.csproj` path from the declared project set.                                                                                                                                             |
| `targetFramework`   | Exact declared TFM string.                                                                                                                                                                              |
| `configuration`     | Build configuration string and compiler-option/reference content digest.                                                                                                                                |
| `assembly`          | Assembly name and SHA-256 content hash; path, if present, is root-relative.                                                                                                                             |
| `sourceFingerprint` | SHA-256 over sorted root-relative source paths and exact compiler-input content hashes, including generated inputs used by the collector. No mtime or machine path.                                     |
| `policyFingerprint` | SHA-256 over the manifest/schema/record input index and their exact content hashes.                                                                                                                     |
| `adapter`           | Producer ID, producer version and contract version. IDs identify integration producers, not permitted frameworks.                                                                                       |
| `runId`             | Required opaque runner invocation identity for result evidence; discovery may use its own discovery-session ID. Correlation is explicitly within the declared assembly/source/configuration identities. |
| `discovery`         | `{ state, methods, cases }`; state is `complete`, `deferred`, `unavailable` or `unsupportedContract`.                                                                                                   |
| `bindings`          | Explicit code relationships, their validity and source locations, including invalid/unresolved references.                                                                                              |
| `results`           | Runner outcomes keyed by case identity and run, or an explicitly absent result set.                                                                                                                     |
| `inputs`            | Sorted `{ path, sha256, kind }` entries proving the supplied source/manifest/record/schema scope.                                                                                                       |

Hash the length-prefixed UTF-8 path, kind and hash values in ordinal path/kind
order. Content hashes use the bytes supplied to the compiler/collector, not a
later ambient reread. Identical logical inputs on different machines produce the
same fingerprints. Run identity is supplied execution data, not an analyzer clock.

## Methods and cases

A method record carries `symbolId` (Roslyn documentation-comment ID), declaring
type metadata name, method metadata name, generic arity and parameter type
identities. Source location is `{ path, line, column }`, one-based and relative
to the root; generated/unsourceable locations are explicitly absent, never guessed.
Use these fields to disambiguate overloads and generic methods.

A case record carries `caseId`, `methodSymbolId`, optional opaque row identity,
and a display name used only for presentation. The case key is
`(project, targetFramework, assembly hash, adapter ID, caseId)`; it is not the
display name. A runner's unstable per-run result ID cannot substitute for its
stable case ID. The producer must correlate such IDs before exporting evidence.

A method can have zero, one or many known cases. Runtime-only rows have
`discovery.state: "deferred"` until actual runner events enumerate them. Do not
create a synthetic passing row or claim a known case count. Metadata that cannot
uniquely map a method is unresolved evidence, not a guessed method-name match.

A result carries `caseId`, `runId`, `outcome` and optional runner detail. Outcomes
are `passed`, `failed`, `skipped`, `notRun` or `aborted`; absence remains distinct.
Duplicate conflicting results for one case/run make that input invalid; identical
repeats do not create additional executions. Different run IDs remain separate;
never select the latest by ambient time. The CLI can select a run explicitly.

## Binding records

A binding record carries `symbolId`, `role`, its own target components, source
location, and separate `syntaxValidity`, `resolution`, `eligibility` fields.
Target kinds are `claim` and `adr`; authoring defaults are context, not bindings.
Preserve invalid/raw target fields for findings without treating them as evidence.
Deduplicate valid relationships by symbol, role and resolved target under F2's
rules. No enclosing type context or neighboring binding completes checked IDs.

Collection uses current compiler/documentation metadata during the normal build
and runner integration during discovery/execution. Compiler-produced XML member
IDs are suitable identity inputs; display names and assertion text are not.
The shared binding resolver validates records even if diagnostic presentation is
suppressed. Export through an explicitly invoked build/collector step into its
selected intermediate/output destination; DiagnosticAnalyzer writes no files.

## Scenarios and producer extensions

Optional `scenarios` records carry the scenario source path, a stable generator
scenario ID, explicit effective target scope, local claim IDs, optional outline
row ID and the associated runner case key. F4 defines scope and metadata mapping.
A consumer bridge can produce these records without Reqnroll. Arbitrary producer
extensions cannot add new checked target kinds or silently change version-one
semantics.

## Freshness and aggregation

Join only evidence with matching project/TFM, source, assembly and configuration
identities. Discovery and a later execution can have different session IDs but
must agree on stable case keys and relevant current identities. A policy change
requires revalidating bindings against the current record catalog; an old binding
verdict cannot be carried forward as current evidence without that evaluation.

Retain stale/mismatched artifacts visibly and exclude their discovery/results
from current evidence. A valid current source binding remains independently
resolvable. Missing a declared compilation makes repository scope incomplete;
missing/deferred cases or results never become passing evidence.

## Collection boundary and conformance

Implement adapters around test-platform/runner discovery and execution surfaces,
with an exporter entry point that accepts the common records for other producers.
Do not add framework-specific reference grammars to the core. The compatibility
matrix records tested producer/platform versions and metadata capabilities.
Conformance cases include xUnit facts/theories, NUnit, MSTest, Playwright .NET,
custom producer records, identical display names, overloads, deferred data,
missing metadata and stale artifacts. No compatibility claim is made merely
because a framework name appears in a fixture.
