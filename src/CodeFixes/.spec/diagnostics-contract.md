---
title: "0201-F2: Roslyn diagnostics and repair design"
description: "Diagnostic registry, compilation analysis and deterministic invoked repairs"
type: design
---

# Roslyn diagnostics and repair design

This is the implementation contract for the adopted XML decisions. IDs become
published when the package ships; keep retired entries rather than reassigning
an ID. The existing SPEC registry is unchanged.

| ID       | Finding                                                       | Default | Repair                                                                               |
| -------- | ------------------------------------------------------------- | ------- | ------------------------------------------------------------------------------------ |
| SPCT0001 | Required analysis input missing, invalid or incompatible      | Error   | None                                                                                 |
| SPCT0002 | Reserved XML shape, placement or explicit identity incomplete | Warning | Materialize an unambiguous default, or explicit existing-target selection where safe |
| SPCT0003 | Identifier violates its consumer grammar                      | Warning | Explicit existing-target selection where safe; no guessed correction                 |
| SPCT0004 | Referenced target absent from its selected owner              | Warning | Explicit existing-target selection where safe                                        |
| SPCT0005 | Referenced identity resolves ambiguously                      | Warning | Explicit existing-target selection where safe                                        |
| SPCT0006 | Resolved target ineligible for current evidence               | Warning | Explicit eligible-target selection where safe                                        |
| SPCT0007 | Required authored relationship missing in selected scope      | Warning | Explicit existing-target selection                                                   |
| SPCT0008 | Partial type authoring defaults conflict                      | Warning | None                                                                                 |
| SPCT0009 | Duplicate valid symbol/role/target relationship               | Info    | Remove redundant copies, retaining one complete relationship                         |

Use static readonly DiagnosticDescriptors, localizable resources and stable help
links. Native Roslyn configuration owns effective severity and suppression.
Locate a finding on the offending XML name/attribute/value; locate missing XML on
the declaration identifier. Input findings use the supplied AdditionalFile
location where available, otherwise Location.None with a root-relative input
property. Diagnostic suppression never changes the underlying binding verdict.

## Read-only analysis

Enable concurrent execution. A compilation-start action reads immutable
AdditionalFile inputs into F1's catalog and caches required symbols. Register
only declaration kinds selected by the role-placement matrix. Analyze structured
XML documentation trivia attached to declarations, preserving original locations
and unrelated trivia; do not infer targets from summary prose or names.

Normalize declarations to their logical Roslyn symbol, including partial types.
Keep each reserved element's raw target and separate shape, format, resolution
and lifecycle verdicts. Validate explicit targets without applying defaults.
Valid repeated relationships are keyed by symbol, role and resolved target;
duplicates count once. Multi-variable field/event declarations apply their
shared documentation to the declared symbols under CSharp semantics.

Resolve type defaults from the nearest lexical annotated type, using the whole
inner default. Read all partial parts before selecting a shared identical default
or reporting conflict. Never walk base types. Invalid defaults supply no context.
Generated code has no mandatory-authoring diagnostics; explicit bindings still
follow the agreed validation policy. This requires narrowly handling explicit
documentation in generated syntax rather than a blanket skip of generated files.
Honor cancellation and retain no mutable analyzer instance or cross-compilation
state. The analyzer performs no filesystem discovery, network access or writes.

## Invoked repairs

Keep CodeFixProvider and Workspaces dependencies in Specht.Codefixes. Use
DocumentEditor/SyntaxEditor to edit documentation trivia only. Offer default
materialization when a claim-only element has exactly one eligible existing
target under one valid default. Persist the entire target; moving the member
later does not change its binding.

For missing/replacement targets, offer named actions for existing eligible
records and require the user's explicit choice. Defaults may narrow the choices;
they never authorize selecting one silently. Unknown attributes or body content
have no destructive cleanup action. Preserve summary, remarks and custom XML.

Duplicate cleanup keeps the first complete relationship by ordinal relative file
path and source position; remove only redundant reserved elements. Apply changes
to partial declarations atomically across documents. Reanalyze current documents
before applying each action; stale or changed preconditions remove the action.

Use separate equivalence keys for default materialization and duplicate cleanup.
Fix All admits only those deterministic, choice-free actions and recomputes each
local precondition. Target-selection actions do not participate in Fix All.

## Verification

Use Rocket.Surgery.Extensions.Testing.SourceGenerators with direct
AwesomeAssertions, following decision 0001. Assert exact diagnostic counts, IDs,
severity and spans, and independently assert compiler errors. Assert fixed source
text and reanalysis directly, including preserved unrelated XML. No Verify or
snapshot files. Use in-memory Roslyn workspaces for repairs and actual compiler
loading/packed-consumer checks for assembly separation; a whole-engine retarget
attempt is not an analyzer-loading test.
