---
title: "Specification: CSharp binding authoring"
description: "Use type defaults to author explicit declaration bindings that remain stable across moves."
type: feature
id: "F2"
epic: "0201"
spec_status: draft
status: in-progress
priority: med
value: 0
risk: 0
rank: 0
scored_by: null
scored_on: null
domain: "Code traceability"
author: "spec-author"
milestone: null
children: []
depends_on: ["F1"]
blocks: ["F3", "F6"]
spikes: []
created: "2026-10-17"
updated: "2026-10-30"
github_issue: null
synced_at: null
---

# Specification: CSharp binding authoring

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-17 -->

Use type defaults to author explicit declaration bindings that remain stable across moves. This draft decomposes the owner-selected model; unresolved public contracts block implementation, not the recorded adoption decision.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-17 -->

| #   | Persona             | Need                                                                                       | Pain Point Today                                                 |
| --- | ------------------- | ------------------------------------------------------------------------------------------ | ---------------------------------------------------------------- |
| 1   | Consumer maintainer | Use type defaults to author explicit declaration bindings that remain stable across moves. | References and test names can drift without an inspectable link. |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                             |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | Originally, area folders were assumed not to select project boundaries. RESOLVED, 2026-10-17: owner review adopts the Analyzers, CodeFixes and deployment CodeAnalysis project homes (ADR-0016); implementation internals remain open. |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-30 -->

| ID    | Claim                                                                                                                                                                                                                                                          | Source                                             | Status  |
| ----- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------- | ------- |
| B-001 | Given a supported declaration carrying a complete explicit Feature and claim binding, this Feature ensures checked traceability uses that declaration identity regardless of its containing type default.                                                      | Epic 0201; decision 0001 in 0201-F1; decision 0003 | Amended |
| B-002 | Given a member carrying only a claim under a type default, this Feature ensures analysis reports that the member lacks explicit identity and does not count it as checked evidence.                                                                            | Epic 0201; decision 0001 in 0201-F1                | Active  |
| B-003 | Given a claim-only member with one unambiguous type default and an invoked repair, this Feature ensures the repair persists its complete target identity while preserving other XML documentation.                                                             | Epic 0201; decision 0001 in 0201-F1                | Active  |
| B-004 | Given missing, conflicting or ambiguous default targets, this Feature ensures no repair silently selects a target.                                                                                                                                             | Epic 0201; decision 0001 in 0201-F1                | Active  |
| B-005 | Given a malformed, unresolved or ambiguous reference on a supported declaration, this Feature ensures analysis reports a diagnostic at that declaration with its target reference.                                                                             | Epic 0201; decision 0001 in 0201-F1; decision 0003 | Amended |
| B-006 | Given a supported declaration with repeated `verifies`, `implements` or `follows` elements each carrying one complete target, this Feature retains each distinct declared relationship independently.                                                          | Epic 0201; decision 0001 in 0201-F1; decision 0005 | Amended |
| B-007 | Given a selected test method with no binding documentation, this Feature ensures analysis reports the missing binding and an invoked fix offers existing claim choices without inventing the target.                                                           | Epic 0201; decision 0001 in 0201-F1                | Active  |
| B-008 | Given a `<spec>` element on a type, this Feature uses it as authoring-default context without treating it as checked claim or ADR evidence.                                                                                                                    | Decision 0002                                      | Active  |
| B-009 | Given a `<verifies>` element on a supported declaration, this Feature interprets its target as a declared test-to-claim verification relationship.                                                                                                             | Decision 0002                                      | Active  |
| B-010 | Given an `<implements>` element on a supported declaration, this Feature interprets its target as a declared implementation-to-claim relationship.                                                                                                             | Decision 0002                                      | Active  |
| B-011 | Given a `<follows>` element on a supported declaration, this Feature interprets its target as a declared code-to-ADR relationship.                                                                                                                             | Decision 0002                                      | Active  |
| B-012 | Given one of the four XML roles, this Feature supports the CSharp type and member placements defined in decision 0003.                                                                                                                                         | Decision 0003                                      | Active  |
| B-013 | Given an explicit claim or ADR binding on a type, this Feature binds only that type and supplies no implicit binding to its members.                                                                                                                           | Decision 0003                                      | Active  |
| B-014 | Given nested types with authoring defaults, this Feature selects the nearest enclosing type declaring a default as the authoring context.                                                                                                                      | Decision 0004                                      | Active  |
| B-015 | Given a nested type declaring its own default, this Feature uses that entire default without merging any fields from an outer type.                                                                                                                            | Decision 0004                                      | Active  |
| B-016 | Given a partial type with one default or identical repeated defaults across its parts, this Feature exposes that default as one authoring context for all parts.                                                                                               | Decision 0004                                      | Active  |
| B-017 | Given conflicting defaults across parts of a partial type, this Feature reports the conflict and blocks automatic materialization from those defaults.                                                                                                         | Decision 0004                                      | Active  |
| B-018 | Given a base type with an authoring default, this Feature does not inherit that default into a derived type.                                                                                                                                                   | Decision 0004                                      | Active  |
| B-019 | Given an identity attribute on a relationship element, this Feature treats its value as one consumer-grammar identifier without splitting it into a list of targets.                                                                                           | Decision 0005                                      | Active  |
| B-020 | Given a reserved XML element with missing or unknown attributes, nonempty content, malformed XML or an unsupported placement, this Feature reports a validation diagnostic.                                                                                    | Decision 0006                                      | Active  |
| B-021 | Given an invalid `verifies`, `implements` or `follows` element, this Feature excludes that relationship from checked evidence.                                                                                                                                 | Decision 0006                                      | Active  |
| B-022 | Given an invalid `spec` element, this Feature excludes that default from authoring context.                                                                                                                                                                    | Decision 0006                                      | Active  |
| B-023 | Given unrelated XML documentation including other custom tags, this Feature preserves it during analysis and invoked binding repairs.                                                                                                                          | Decision 0006                                      | Active  |
| B-024 | Given an empty reserved XML element with the defined attributes on a supported declaration, this Feature accepts its XML shape independently of target resolution and lifecycle eligibility.                                                                   | Decision 0006                                      | Active  |
| B-025 | Given valid relationship elements with the same code symbol, role and resolved target, this Feature represents them as one relationship.                                                                                                                       | Decision 0007                                      | Active  |
| B-026 | Given duplicate valid relationship elements on the same code symbol, this Feature reports a duplicate diagnostic.                                                                                                                                              | Decision 0007                                      | Active  |
| B-027 | Given duplicate valid relationship elements and an invoked duplicate-removal fix, this Feature removes redundant copies while preserving one complete binding.                                                                                                 | Decision 0007                                      | Active  |
| B-028 | Given valid relationships on one code symbol with different roles and the same resolved target, this Feature retains the roles as distinct relationships.                                                                                                      | Decision 0007                                      | Active  |
| B-029 | Given valid relationships on different code symbols with the same role and resolved target, this Feature retains a separate relationship for each symbol.                                                                                                      | Decision 0007                                      | Active  |
| B-030 | Given a binding repair that needs a target choice or replacement, this Feature edits the target only after explicit selection of an existing record.                                                                                                           | Decision 0008                                      | Active  |
| B-031 | Given an ID or attribute needing correction, this Feature does not guess a replacement.                                                                                                                                                                        | Decision 0008                                      | Active  |
| B-032 | Given unknown attributes or body content in a reserved element, this Feature does not offer deletion of that material merely to make the element valid.                                                                                                        | Decision 0008                                      | Active  |
| B-033 | Given an invoked Fix All operation, this Feature applies only supported deterministic repairs that require no target choice.                                                                                                                                   | Decision 0008                                      | Active  |
| B-034 | Given a target selected for one declaration, this Feature does not propagate that choice across unrelated declarations through Fix All.                                                                                                                        | Decision 0008                                      | Active  |
| B-035 | Given an analyzer finding, this Feature identifies it with an `SPCT` diagnostic ID followed by four decimal digits.                                                                                                                                            | Decision 0009                                      | Active  |
| B-036 | Given the same published diagnostic in a later analyzer release, this Feature retains its diagnostic ID.                                                                                                                                                       | Decision 0009                                      | Active  |
| B-037 | Given a retired diagnostic ID, this Feature never assigns that ID to another diagnostic.                                                                                                                                                                       | Decision 0009                                      | Active  |
| B-038 | Given an analyzer-only rule addition, this Feature leaves the checker's pinned `SPEC` rule vocabulary unchanged.                                                                                                                                               | Decision 0009                                      | Active  |
| B-039 | Given an invoked repair that writes a binding element into a documentation comment, this Feature leaves that comment's same-role binding elements on lines of their own ordered by epic, then feature, then claim or ADR id, comparing digit runs numerically. | Decision 0010                                      | Active  |
| B-040 | Given a binding repair that offers target choices, this Feature lists those choices ordered by epic, then feature, then claim or ADR id, comparing digit runs numerically.                                                                                     | Decision 0010                                      | Active  |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-17 -->

| ID  | Constraint                                                                                                                             | Rules Out                                                      |
| --- | -------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------- |
| C-1 | Type defaults supply authoring context only; checked evidence never inherits identity.                                                 | Implicit inheritance supplying checked evidence.               |
| C-2 | An explicit declaration target supplies its complete identity; no partial merge with type defaults.                                    | Combining an explicit target with a different enclosing scope. |
| C-3 | Analysis is read-only. Only an invoked code fix edits existing documentation; it cannot create claims or change specification records. | Unsolicited writes or invented specification content.          |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-30 -->

| #   | Item                                                                                                                         | Exclusion Reason                                                                                 |
| --- | ---------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------ |
| 1   | Selecting assertion adequacy or proving a method implements a claim.                                                         | This agreement does not authorize it.                                                            |
| 2   | Shipped source generation and automatic specification editing.                                                               | This agreement does not authorize it.                                                            |
| 3   | Other languages and code outside CSharp XML-documentable types and members.                                                  | This agreement does not authorize it.                                                            |
| 4   | A diagnostic for unordered binding elements, and reordering a documentation comment no repair writes a binding element into. | Decision 0010: no new `SPCT` ID was requested, and untouched documentation is preserved (B-023). |

## 6. Concern Separation

<!-- last written by: implementer, 2026-10-30 -->

| Concern                                                  | Classification | Home                                                                                          |
| -------------------------------------------------------- | -------------- | --------------------------------------------------------------------------------------------- |
| Explicit identity, XML roles and relationship uniqueness | Business       | `DocumentationBindingReader` delegates target verdicts to `BindingResolver`.                  |
| Compiler syntax, locations and diagnostic presentation   | Technical      | `DocumentationBindingAnalyzer` reads supplied policy and reports native Roslyn diagnostics.   |
| Published diagnostic identity                            | Both           | `DocumentationDiagnostics` validates publication history and owns the SPCT registry.          |
| Authoring defaults                                       | Both           | `DocumentationBindingReader` selects lexical context and diagnoses partial conflicts.         |
| Invoked repairs                                          | Both           | `DocumentationCodeFixProvider` offers validated plans and guards current workspace snapshots. |
| Binding order                                            | Both           | `DocumentationOrder` compares identities naturally and permutes written same-role elements.   |

## 7. Technical Design

<!-- last written by: implementer, 2026-10-30 -->

The read-only implementation is in
[`DocumentationBindingReader`](../../analyzers/Documentation/DocumentationBindingReader.cs),
[`DocumentationBindingAnalyzer`](../../analyzers/Documentation/DocumentationBindingAnalyzer.cs)
and [`DocumentationDiagnostics`](../../analyzers/Documentation/DocumentationDiagnostics.cs).
The reader retains raw elements, separate shape/resolution/eligibility verdicts,
unique eligible relationships and original syntax locations. It reuses F1's
`BindingResolver` and F6's `AnalysisInputAdapter` and `AnalysisPolicy`; suppression
does not alter the reader's verdict. Strict XML validation checks the reserved
element independently of whether compiler documentation warnings are enabled.

The analyzer receives root-relative source paths and the project/TFM build
properties. Supplied version-one test-evidence method symbol IDs provide test
recognition; source names and assertion text do not. Full collection, freshness
and unavailable-evidence reporting remain F3's integration responsibilities.
`DocumentationAnalysis.AuthoringDefault` exposes authoring context separately
from checked relationships. The reader reuses its declaration/XML parser for
all parts of each lexical type; it selects the nearest annotated type and
compares complete attribute dictionaries ordinally, independently of XML layout
and attribute order. Conflicting valid defaults produce SPCT0008 on each
participating `spec` name, only when analyzing the declaring type. Members get
no context from a conflict, and outer defaults cannot resolve it. Invalid
defaults supply no context. Base types are never traversed. Explicit bindings
continue to resolve independently.

This Feature spans the two component projects selected in
[ADR-0018](../../../.spec/adr/0018-lowercase-analyzer-project-home.md).
`Specht.Analysis` in `src/analyzers` owns read-only diagnostics; `Specht.Codefixes`
in `src/CodeFixes` owns the invoked `CodeFixProvider` repairs and references the
analyzer project. Analyzers must not reference the code-fix assembly. One Feature
agreement covers the shared binding verdict; no claim or scenario is duplicated
across project homes.

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

### Read-only analysis

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

### Invoked repairs

[`DocumentationCodeFixProvider`](../Documentation/DocumentationCodeFixProvider.cs)
exports the invoked repairs. Workspaces dependencies stay in Specht.Codefixes.
[`DocumentationRepairs`](../Documentation/DocumentationRepairs.cs) reuses the
binding reader, resolver, input adapter, shared XML well-formedness check and
supplied test-method recognition. It produces edits limited to reserved XML or
new documentation on the selected declaration.
[`DocumentationEdits`](../Documentation/DocumentationEdits.cs) applies those edits
through DocumentEditor as one returned Solution, preserving every other byte.
No workspace or specification write happens during analysis or action discovery.

Offer default
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
[`DocumentationFixAllProvider`](../Documentation/DocumentationFixAllProvider.cs)
supports document, project and solution scopes, deduplicates repeated edits and
returns no action for target-selection keys. Individual and bulk actions reject
changed project snapshots and recompute the current binding verdict before
applying an edit. Duplicate cleanup returns changes across partial documents
atomically; unknown attributes, body content and malformed XML have no cleanup
action. Native workspace tests normalize editorconfig host paths at the Roslyn
boundary while retaining relative source and consumer-provenance fixtures.

[`DocumentationOrder`](../Documentation/DocumentationOrder.cs) holds the identity
order of [decision 0010](decisions/0010-binding-element-order.md). It compares
digit runs numerically after trimming leading zeros and other characters
ordinally, puts a missing value first and falls back to ordinal comparison on
equality. `DocumentationEdits` records the spans a repair wrote, those changes
with new text, shifted into the updated text, and then runs the order pass over
the result before reparsing, so individual actions and Fix All share it. For each
documentation comment overlapping a written span, the pass stably sorts each
role's whole-line empty `verifies`, `implements` and `follows` elements by epic,
feature, then claim or ADR id; other content, other roles and other comments keep
their positions, and duplicate removal writes nothing to order (B-039).
`DocumentationRepairs` lists target choices by the same comparer over epic,
feature and id, then relative path and line (B-040).

### Verification

Use Rocket.Surgery.Extensions.Testing.SourceGenerators with direct
AwesomeAssertions, following decision 0001. Assert exact diagnostic counts, IDs,
severity and spans, and independently assert compiler errors. Assert fixed source
text and reanalysis directly, including preserved unrelated XML. No Verify or
snapshot files. Use in-memory Roslyn workspaces for repairs and actual compiler
loading/packed-consumer checks for assembly separation; a whole-engine retarget
attempt is not an analyzer-loading test.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-30 (rows B-039, B-040) -->

`DocumentationSteps` executes the read-only scenarios against compiled synthetic
declarations and the production reader/resolver. `DocumentationBindingReaderUnitTests`
checks declaration identity, role placement, partial-symbol deduplication and
independent target verdicts. `DocumentationBindingAnalyzerUnitTests` uses the
[decision 0001](decisions/0001-analyzer-testing.md) harness with direct assertions
for diagnostic counts, severity, locations, supplied method recognition and
generated-authoring boundaries. `DocumentationDiagnosticsUnitTests` pins the
registry and rejects reassigned, removed or revived published identities.
`TypeDefaultsUnitTests` pins whole lexical selection, partial agreement,
conflict ordering across files, base isolation and invalid-default exclusion;
the compiler harness pins SPCT0008 count, warning severity and XML-name spans.
The existing default scenarios now execute through `DocumentationSteps`.
`DocumentationSteps` also invokes individual code actions and Fix All in native
in-memory workspaces, asserting fixed source and reanalysis. Unsafe-default
examples cover missing, conflicting and ambiguous contexts separately.
`DocumentationCodeFixUnitTests` pins repair planning, lifecycle and ambiguity
gates, preservation, partial cleanup, explicit selection, stale-action rejection,
line endings and each supported Fix All scope. `DocumentationOrderUnitTests`
pins the identity order over a synthetic epic-tier record set whose paths and
table rows run opposite to identity order: a selected replacement, a Fix All
materialization, the content, roles and comments outside the order, duplicate
removal, and the target choices. Ties of equal keys are not separately pinned.
The read-only compiler harness
and portable-component integration tests continue to guard assembly separation.

The corresponding unit tests are named once, in § 9. The implemented mechanisms are:

| Claim | Mechanism                                                                                                                                                                           |
| ----- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| B-001 | Raw element attributes go directly to BindingResolver, independently of type defaults.                                                                                              |
| B-002 | Required identity attributes gate evidence before resolution.                                                                                                                       |
| B-005 | Resolution verdicts map to located SPCT findings without guessing a target.                                                                                                         |
| B-006 | Each repeated element resolves its own complete target.                                                                                                                             |
| B-009 | The verifies role requires the supplied recognized-method fact.                                                                                                                     |
| B-010 | The implements role retains its own implementation relationship.                                                                                                                    |
| B-011 | The follows role uses the existing ADR ownership resolver.                                                                                                                          |
| B-012 | Compiler declaration references and the role-placement check cover documentable types and members.                                                                                  |
| B-013 | A read visits only the logical symbol’s declaration references, without traversing its members.                                                                                     |
| B-019 | Attribute text remains scalar and the resolver validates its complete consumer identifier.                                                                                          |
| B-020 | XML well-formedness, empty content, attribute names and placement are checked separately.                                                                                           |
| B-021 | IsEvidence excludes invalid, unresolved, ambiguous and ineligible relationships.                                                                                                    |
| B-024 | ShapeValid is computed independently of the target-resolution verdict.                                                                                                              |
| B-025 | Deduplication compares role and resolved identity within one logical symbol.                                                                                                        |
| B-026 | A redundant valid element produces SPCT0009 at its original XML name.                                                                                                               |
| B-028 | Role participates in the relationship key.                                                                                                                                          |
| B-029 | Separate reads preserve separate symbol identities.                                                                                                                                 |
| B-035 | The localizable registry supplies only SPCT diagnostic descriptors.                                                                                                                 |
| B-036 | Publication-history validation and the fixed registry test retain the assigned finding identity.                                                                                    |
| B-037 | Publication-history validation rejects an active use of a previously retired ID.                                                                                                    |
| B-038 | DocumentationDiagnostics creates SPCT descriptors and never mutates SchemaVersions; its unit test pins that boundary and the existing checker baseline remains the regression test. |
| B-008 | AuthoringDefault is separate from Relationships and supplies no checked evidence.                                                                                                   |
| B-014 | ContainingType traversal selects the nearest lexical type declaring a default.                                                                                                      |
| B-015 | Selection returns the complete inner attribute dictionary without merging fields.                                                                                                   |
| B-016 | All declaration references of a partial symbol share one ordinally identical default.                                                                                               |
| B-017 | Different complete defaults block context and report SPCT0008 only on the type’s default names.                                                                                     |
| B-018 | Selection traverses lexical ContainingType only, never BaseType.                                                                                                                    |
| B-022 | XML-shape failures exclude defaults from authoring context.                                                                                                                         |
| B-003 | Claim-only shape plus one eligible resolved default gates complete identity materialization.                                                                                        |
| B-004 | Missing/conflicting defaults and ambiguous/ineligible resolver verdicts withhold automatic materialization.                                                                         |
| B-007 | Selected recognized methods get named eligible target actions; line endings and indentation govern insertion only.                                                                  |
| B-023 | Reserved-element spans constrain replacements; all other documentation text is retained exactly.                                                                                    |
| B-027 | Reader ordering retains the ordinal first complete binding; one Solution removes redundant partial copies.                                                                          |
| B-030 | Only invocation of a named existing-record action changes a choice-dependent target.                                                                                                |
| B-031 | Target enumeration uses resolver-approved records; misspelled attributes have no automatic correction.                                                                              |
| B-032 | XML well-formedness, empty-body and known-attribute gates exclude destructive cleanup.                                                                                              |
| B-033 | Separate deterministic keys replan every local verdict for document/project/solution scopes and reject stale snapshots.                                                             |
| B-034 | Fix All rejects every target-selection equivalence key.                                                                                                                             |
| B-039 | The order pass permutes only whole-line same-role elements of comments a repair wrote into, using the natural comparer.                                                             |
| B-040 | Target enumeration orders by the shared comparer, with relative path and line as tiebreak.                                                                                          |

## 9. Traceability Matrix

<!-- last written by: implementer, 2026-10-30 (status B-039, B-040) -->

| Claim ID | Scenario                             | Test                                                                                                                                                                                                                                                                                                                                                                                      | Status  |
| -------- | ------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------- |
| B-001    | Explicit evidence                    | DocumentationSteps.ThenExplicit; TypeBindingsAndDefaults_WhenReadingMembers_ShouldNeverSupplyImplicitIdentity                                                                                                                                                                                                                                                                             | Covered |
| B-002    | Inherited binding                    | DocumentationSteps.ThenIncomplete; TypeBindingsAndDefaults_WhenReadingMembers_ShouldNeverSupplyImplicitIdentity                                                                                                                                                                                                                                                                           | Covered |
| B-003    | Materialize default                  | DocumentationSteps.ThenRepaired; ClaimOnlyBinding_WhenMaterialized_ShouldPersistIdentityAndPreserveOtherXml                                                                                                                                                                                                                                                                               | Covered |
| B-004    | Unsafe repair                        | DocumentationSteps.ThenNoSilentRepair; UnsafeBinding_WhenRepairsRequested_ShouldNeverGuessOrDestroyMaterial; AmbiguousOrIneligibleDefault_WhenRepairRequested_ShouldWithholdMaterialization                                                                                                                                                                                               | Covered |
| B-005    | Invalid binding                      | DocumentationSteps.ThenDiagnostic; TargetVerdict_WhenRead_ShouldRemainSeparateFromValidXmlShape                                                                                                                                                                                                                                                                                           | Covered |
| B-006    | Multiple targets                     | DocumentationSteps.ThenMultiple; DistinctTargets_WhenRepeated_ShouldRetainEachCompleteRelationship                                                                                                                                                                                                                                                                                        | Covered |
| B-007    | Missing member binding               | DocumentationSteps.ThenClaimChoices; MissingBinding_WhenTargetSelected_ShouldUseOnlyAnExistingEligibleRecord                                                                                                                                                                                                                                                                              | Covered |
| B-008    | Type authoring defaults              | DocumentationSteps.ThenAuthoringOnly; LexicalAndPartialDefaults_WhenSelected_ShouldSupplyWholeContextWithoutEvidence                                                                                                                                                                                                                                                                      | Covered |
| B-009    | Test verification role               | DocumentationSteps.ThenVerification; DocumentableDeclaration_WhenRead_ShouldBindItsOwnSymbol                                                                                                                                                                                                                                                                                              | Covered |
| B-010    | Implementation role                  | DocumentationSteps.ThenImplementation; DocumentableDeclaration_WhenRead_ShouldBindItsOwnSymbol                                                                                                                                                                                                                                                                                            | Covered |
| B-011    | ADR relationship role                | DocumentationSteps.ThenAdr; DocumentableDeclaration_WhenRead_ShouldBindItsOwnSymbol                                                                                                                                                                                                                                                                                                       | Covered |
| B-012    | Supported declaration scope          | DocumentationSteps.ThenValidShape; DocumentableDeclaration_WhenRead_ShouldBindItsOwnSymbol                                                                                                                                                                                                                                                                                                | Covered |
| B-013    | Type binding isolation               | DocumentationSteps.ThenTypeOnly; TypeBindingsAndDefaults_WhenReadingMembers_ShouldNeverSupplyImplicitIdentity                                                                                                                                                                                                                                                                             | Covered |
| B-014    | Nearest nested default               | DocumentationSteps.ThenNearestDefault; LexicalAndPartialDefaults_WhenSelected_ShouldSupplyWholeContextWithoutEvidence                                                                                                                                                                                                                                                                     | Covered |
| B-015    | Whole nested default                 | DocumentationSteps.ThenWholeDefault; DocumentationSteps.ThenNoEpicMerge; LexicalAndPartialDefaults_WhenSelected_ShouldSupplyWholeContextWithoutEvidence                                                                                                                                                                                                                                   | Covered |
| B-016    | Shared partial default               | DocumentationSteps.ThenSharedDefault; LexicalAndPartialDefaults_WhenSelected_ShouldSupplyWholeContextWithoutEvidence                                                                                                                                                                                                                                                                      | Covered |
| B-017    | Conflicting partial defaults         | DocumentationSteps.ThenConflictingDefaults; DocumentationSteps.ThenNoDefault; ConflictingPartialDefaults_WhenReadAcrossFiles_ShouldBlockContextWithoutChangingExplicitEvidence; ConflictingTypeDefaults_WhenAnalyzed_ShouldReportEachDefaultWithoutMemberDuplicates                                                                                                                       | Covered |
| B-018    | No base type default                 | DocumentationSteps.ThenNoDefault; LexicalAndPartialDefaults_WhenSelected_ShouldSupplyWholeContextWithoutEvidence                                                                                                                                                                                                                                                                          | Covered |
| B-019    | Scalar identity values               | DocumentationSteps.ThenScalar; ScalarAttribute_WhenRead_ShouldValidateTheEntireConsumerIdentifier                                                                                                                                                                                                                                                                                         | Covered |
| B-020    | Invalid reserved XML                 | DocumentationSteps.ThenShapeFinding; InvalidReservedXml_WhenAnalyzed_ShouldReportOnePreciseFinding                                                                                                                                                                                                                                                                                        | Covered |
| B-021    | Invalid relationship exclusion       | DocumentationSteps.ThenNoEvidence; TargetVerdict_WhenRead_ShouldRemainSeparateFromValidXmlShape                                                                                                                                                                                                                                                                                           | Covered |
| B-022    | Invalid default exclusion            | DocumentationSteps.ThenNoDefault; InvalidDefault_WhenSelectingContext_ShouldRemainExcluded                                                                                                                                                                                                                                                                                                | Covered |
| B-023    | Unrelated documentation preservation | DocumentationSteps.ThenPreserved; ClaimOnlyBinding_WhenMaterialized_ShouldPersistIdentityAndPreserveOtherXml; ReplacementTarget_WhenExplicitlySelected_ShouldReplaceOnlyTheReservedElement                                                                                                                                                                                                | Covered |
| B-024    | Empty reserved elements              | DocumentationSteps.ThenValidShape; DocumentableDeclaration_WhenRead_ShouldBindItsOwnSymbol                                                                                                                                                                                                                                                                                                | Covered |
| B-025    | Duplicate relationship identity      | DocumentationSteps.ThenDeduplicated; RepeatedPartialRelationships_WhenRead_ShouldDeduplicateBySymbolRoleAndResolvedTarget                                                                                                                                                                                                                                                                 | Covered |
| B-026    | Duplicate relationship diagnostic    | DocumentationSteps.ThenDuplicate; RepeatedPartialRelationships_WhenRead_ShouldDeduplicateBySymbolRoleAndResolvedTarget                                                                                                                                                                                                                                                                    | Covered |
| B-027    | Remove redundant relationship copies | DocumentationSteps.ThenRepaired; PartialDuplicates_WhenRemoved_ShouldKeepOrdinalFirstAndPreserveOtherXml                                                                                                                                                                                                                                                                                  | Covered |
| B-028    | Distinct relationship roles          | DocumentationSteps.ThenDistinctRoles; RepeatedPartialRelationships_WhenRead_ShouldDeduplicateBySymbolRoleAndResolvedTarget                                                                                                                                                                                                                                                                | Covered |
| B-029    | Distinct code symbols                | DocumentationSteps.ThenDistinctSymbols; RepeatedPartialRelationships_WhenRead_ShouldDeduplicateBySymbolRoleAndResolvedTarget                                                                                                                                                                                                                                                              | Covered |
| B-030    | Explicit repair target selection     | DocumentationSteps.ThenSelection; DocumentationSteps.ThenNoSelection; ReplacementTarget_WhenExplicitlySelected_ShouldReplaceOnlyTheReservedElement                                                                                                                                                                                                                                        | Covered |
| B-031    | No guessed corrections               | DocumentationSteps.ThenNoGuess; UnsafeBinding_WhenRepairsRequested_ShouldNeverGuessOrDestroyMaterial                                                                                                                                                                                                                                                                                      | Covered |
| B-032    | No destructive validation cleanup    | DocumentationSteps.ThenNoDeletion; UnsafeBinding_WhenRepairsRequested_ShouldNeverGuessOrDestroyMaterial                                                                                                                                                                                                                                                                                   | Covered |
| B-033    | Deterministic Fix All                | DocumentationSteps.ThenRepaired; MixedBindings_WhenFixAllInvoked_ShouldRecheckEachBindingAndRespectScope; StaleAction_WhenWorkspaceChanged_ShouldPreserveTheCurrentDocuments                                                                                                                                                                                                              | Covered |
| B-034    | No bulk target propagation           | DocumentationSteps.ThenNoPropagation; ReplacementTarget_WhenExplicitlySelected_ShouldReplaceOnlyTheReservedElement                                                                                                                                                                                                                                                                        | Covered |
| B-035    | Analyzer diagnostic namespace        | DocumentationSteps.ThenNamespace; AgreedRegistry_WhenLoaded_ShouldKeepEveryAssignedDiagnosticAndItsSeverity                                                                                                                                                                                                                                                                               | Covered |
| B-036    | Stable published diagnostic IDs      | DocumentationSteps.ThenPublished; PublishedHistory_WhenValidated_ShouldRejectReassignmentRemovalAndRevival                                                                                                                                                                                                                                                                                | Covered |
| B-037    | Retired diagnostic IDs               | DocumentationSteps.ThenRetired; PublishedHistory_WhenValidated_ShouldRejectReassignmentRemovalAndRevival                                                                                                                                                                                                                                                                                  | Covered |
| B-038    | Checker vocabulary isolation         | DocumentationSteps.ThenCheckerUnchanged; AgreedRegistry_WhenLoaded_ShouldKeepEveryAssignedDiagnosticAndItsSeverity; TheBaselineTree_WhenChecked_ShouldGiveTheGoldenReportsVerdictsFieldForFieldInOrder                                                                                                                                                                                    | Covered |
| B-039    | Ordered binding elements             | DocumentationSteps.ThenOrdered; DocumentationSteps.ThenPrecedes; DocumentationSteps.ThenOutsideOrderKept; SelectedReplacement_WhenWrittenAmongSameRoleBindings_ShouldTakeItsSortedPosition; ClaimOnlyBindings_WhenFixAllMaterializes_ShouldEndInClaimOrder; RepairedComment_WhenOrdered_ShouldLeaveOtherContentRolesAndCommentsInPlace; DuplicateRemoval_WhenInvoked_ShouldReorderNothing | Covered |
| B-040    | Ordered target choices               | DocumentationSteps.ThenChoiceOrder; TargetChoices_WhenListed_ShouldFollowEpicFeatureAndClaimOrder                                                                                                                                                                                                                                                                                         | Covered |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-30 -->

The unsafe-repair scenario now exercises missing, conflicting and ambiguous
defaults as separate examples under the unchanged B-004 agreement.

B-039 and B-040 add an identity order for the binding elements a repair writes
and for the target choices it offers ([decision 0010](decisions/0010-binding-element-order.md)).

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-17 -->

| ID   | Question                                                                                                                            | Blocks                                  | Resolution                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |
| ---- | ----------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| OQ-1 | Which exact XML tags, role names and CSharp declaration kinds are public?                                                           | None for the XML and repair contract    | RESOLVED, 2026-10-17: decisions 0002 and 0003 define roles and declarations; decision 0005 defines repeated single-target elements; decision 0006 defines strict XML validation; decision 0007 defines duplicates and their invoked removal. Decision 0008 permits invoked unambiguous default materialization, requires explicit selection for target choices, rejects guessed or destructive corrections and restricts Fix All to deterministic repairs without propagating selected targets. Diagnostic/version policy remains in OQ-2. |
| OQ-2 | Which diagnostic namespace and schema/version policy govern these findings?                                                         | None for diagnostic identity/versioning | RESOLVED, 2026-10-17: decision 0009 adopts `SPCT####`, starting at `SPCT0001`, stable published IDs and no reuse of retired IDs. Analyzer rules follow analyzer package releases; manifest/schema contract changes retain schema-version rules, and the checker's pinned `SPEC` vocabulary is unchanged. Severity defaults are defined in 0201-F6 decision 0001; standard suppression is defined in 0201-F6 decision 0002.                                                                                                                 |
| OQ-3 | What are the nested and partial type default conflict rules? The spike nearest-scope behavior is evidence, not an adopted contract. | None for default selection              | RESOLVED, 2026-10-17: decision 0004 selects the nearest enclosing annotated type, whole-default replacement at inner types, one shared default across partial parts when single or identical, diagnostics and no automatic materialization for conflicting partial defaults, and no base-type default inheritance. Checked bindings remain explicit and independent of defaults.                                                                                                                                                           |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-18 -->

| Section               | Status | Reviewer      | Note                                                                                                                                                                                                                     |
| --------------------- | ------ | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 1-12                  | 🟡     | Pending       | Owner selected the model; this decomposed Feature has not received detailed agreement or implementation sign-off.                                                                                                        |
| Read-only diagnostics | 🟢     | spec-reviewer | Self-review of the read-only claims named in § 9: compiler-harness and mechanism tests pass on both target frameworks; the full build passes. Defaults, invoked repairs and deployment integration remain separate work. |
| Authoring defaults    | 🟢     | spec-reviewer | Self-review of B-008, B-014 through B-018 and B-022: all nine acceptance cases and the mechanism/compiler-harness tests pass on both target frameworks; the full build passes. Invoked repairs remain separate work.     |

## Tasks

- [0239: Implement XML binding diagnostics](../.issue/0239-xml-binding-diagnostics.yml)
- [0240: Implement nested and partial type authoring defaults](../.issue/0240-type-authoring-defaults.yml)
- [0241: Implement safe XML binding code fixes](../.issue/0241-binding-code-fixes.yml)
- [0320: Order binding elements and target choices](../.issue/0320-order-binding-elements.yml)

## Scoring

| Field | Value | Basis           |
| ----- | ----- | --------------- |
| value | 0     | Not yet scored. |
| risk  | 0     | Not yet scored. |
