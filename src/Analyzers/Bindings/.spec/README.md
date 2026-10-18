---
title: "Specification: Binding identity"
description: "Resolve stable claim and ADR identities using consumer grammars without a test-framework dependency."
type: feature
id: "F1"
epic: "0201"
spec_status: draft
status: done
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
depends_on: []
blocks: ["F2", "F3", "F4", "F5", "F6"]
spikes: []
created: "2026-10-17"
updated: "2026-10-17"
github_issue: null
synced_at: null
---

# Specification: Binding identity

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-17 -->

Prevent ambiguous or silently retargeted code bindings by resolving claim and ADR identities through the consumer's configured owner model, while distinguishing current obligations from historical records independently of test frameworks.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-17 -->

| #   | Persona             | Need                                                                                                 | Pain Point Today                                                 |
| --- | ------------------- | ---------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------- |
| 1   | Consumer maintainer | Resolve stable claim and ADR identities using consumer grammars without a test-framework dependency. | References and test names can drift without an inspectable link. |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                             |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | Originally, area folders were assumed not to select project boundaries. RESOLVED, 2026-10-17: owner review adopts the Analyzers, CodeFixes and deployment CodeAnalysis project homes (ADR-0016); implementation internals remain open. |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-17 -->

| ID    | Claim                                                                                                                                                                                                                                            | Source                                                   | Status  |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | -------------------------------------------------------- | ------- |
| B-001 | Given a manifest defining custom Feature and claim grammars and records using them, this Feature ensures references are validated and resolved using those configured grammars.                                                                  | Epic 0201; decision 0001 in 0201-F1                      | Active  |
| B-002 | Given two Features declaring the same local claim ID, this Feature ensures a qualified claim reference resolves only in its declared owning Feature.                                                                                             | Epic 0201; decision 0001 in 0201-F1                      | Active  |
| B-003 | Given a consumer whose configured model has no epic tier, this Feature ensures a Feature and claim reference resolves without inventing an epic.                                                                                                 | Epic 0201; decision 0001 in 0201-F1                      | Active  |
| B-004 | Given an ADR reference, this Feature selects only repository scope when `feature` is absent or the explicitly named Feature scope when `feature` is present.                                                                                     | Decision 0003; addendum 2026-10-17                       | Amended |
| B-005 | Given the same specification inputs and declared references with no Reqnroll or test-framework assemblies, this Feature ensures reference resolution yields the same identities and findings.                                                    | Epic 0201; decision 0001 in 0201-F1                      | Active  |
| B-006 | Given a claim reference, this Feature represents its identity in separate local `epic`, `feature` and `claim` fields under the consumer's configured grammars.                                                                                   | Decision 0002; addendum 2026-10-17                       | Amended |
| B-007 | Given an ADR missing from its selected owner but declared under another owner, this Feature reports the reference as unresolved without searching another scope.                                                                                 | Decision 0003                                            | Active  |
| B-008 | Given an Active or Amended claim in a draft, in-review or approved Feature, this Feature reports a resolving reference to that claim as eligible for current evidence.                                                                           | Decision 0004                                            | Active  |
| B-009 | Given a resolving reference to a Withdrawn or Superseded claim or a claim in a superseded Feature, this Feature reports the target as ineligible for current evidence while preserving its resolved identity and lifecycle state.                | Decision 0004                                            | Active  |
| B-010 | Given an accepted ADR, this Feature reports a resolving reference to that decision as eligible for current evidence.                                                                                                                             | Decision 0004                                            | Active  |
| B-011 | Given a resolving reference to a proposed, rejected, deprecated or superseded ADR, this Feature reports the target as ineligible for current evidence while preserving its resolved identity and lifecycle state.                                | Decision 0004                                            | Active  |
| B-012 | Given a checked relationship, this Feature permits claim or ADR targets only; Feature and epic identities provide scope rather than additional checked target kinds.                                                                             | Decision 0005                                            | Active  |
| B-013 | Given a claim or Feature-owned ADR reference, this Feature requires its epic component exactly when the consumer's manifest declares an epic tier and rejects that component otherwise.                                                          | Decisions 0002 and 0003; addenda 2026-10-17              | Active  |
| B-014 | Given a persisted member reference missing a required target identity component, this Feature leaves it incomplete regardless of enclosing type defaults.                                                                                        | Decision 0002; addendum 2026-10-17                       | Active  |
| B-015 | Given a complete reference conforming to the consumer grammars, this Feature resolves it only when exactly one record matches in its selected owner, classifying zero matches as unresolved and multiple matches as ambiguous.                   | Decision 0002; addendum 2026-10-17                       | Active  |
| B-016 | Given a specification record moved without changing its declared identity or owner, this Feature preserves the target identity of references to that record.                                                                                     | Decision 0002; addendum 2026-10-17                       | Active  |
| B-017 | Given a resolving reference to a superseded target naming a current replacement, this Feature retains the original target without automatically redirecting the binding.                                                                         | Decision 0004; addendum 2026-10-17                       | Active  |
| B-018 | Given an otherwise eligible current claim with a coverage exemption, this Feature preserves its lifecycle eligibility independently of that exemption.                                                                                           | Decision 0004; addendum 2026-10-17                       | Active  |
| B-019 | Given omitted lifecycle information permitted by the pinned schema, this Feature evaluates lifecycle eligibility using that schema's established omission semantics.                                                                             | Decision 0004; addendum 2026-10-17                       | Active  |
| B-020 | Given a resolving reference with an explicit unrecognized claim, owning Feature or ADR lifecycle state, this Feature reports the target as ineligible while preserving its identity and unrecognized state.                                      | Decision 0004; addendum 2026-10-17                       | Active  |
| B-021 | Given an eligible claim in a draft Feature, this Feature leaves the agreement's approval state unchanged.                                                                                                                                        | Decision 0004; addendum 2026-10-17                       | Active  |
| B-022 | Given supplied Feature text or a Feature or ADR catalog declaration with non-repository-relative or root-escaping provenance, this Feature rejects that input before emitting a binding result with a fixed error that omits the rejected value. | AGENTS.md Invariants; brief § 9; § 7 provenance boundary | Active  |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-17 -->

| ID  | Constraint                                                                                                                                                                      | Rules Out                                                                                       |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------- |
| C-1 | Reuse manifest identity and discovery semantics (0001-F5 C-2); no parallel regex configuration or first-match resolution.                                                       | A second ID parser or implicit first-match lookup.                                              |
| C-2 | Keep format validation distinct from existence and ambiguity checks.                                                                                                            | Treating a regex match as a resolving reference.                                                |
| C-3 | Binding identity depends on no Reqnroll, xUnit or Playwright types.                                                                                                             | Mandatory test-framework packages in identity resolution.                                       |
| C-4 | Record paths and qualification delimiters do not define binding identity.                                                                                                       | Retargeting on specification moves or splitting local IDs by an assumed delimiter.              |
| C-5 | Resolution and lifecycle eligibility establish neither agreement approval nor passing behavior.                                                                                 | Promoting a resolving or eligible binding into an approval or test verdict.                     |
| C-6 | Provenance remains repository-relative and within the root under either path separator, while nested paths and internal parent segments that stay within the root remain valid. | Absolute, drive-qualified or root-escaping output and rejection of harmless internal traversal. |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-17 -->

| #   | Item                                                                                                                                                                                                                               | Exclusion Reason                                                                                                                                                    |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | XML roles, supported declarations and repeated single-target elements are defined in 0201-F2 decisions 0002, 0003 and 0005; strict XML validation is defined by F2 decision 0006. Claim identity field names are defined by B-006. | This agreement does not authorize it.                                                                                                                               |
| 2   | Test discovery, execution and scenario links belong to adapters; a resolved reference alone is no behavioral proof.                                                                                                                | This agreement does not authorize it.                                                                                                                               |
| 3   | Additional checked record kinds beyond claims and ADRs.                                                                                                                                                                            | Decision 0005 limits the initial checked target set.                                                                                                                |
| 4   | Coverage exemption policy and required-evidence reconciliation.                                                                                                                                                                    | 0201-F6 owns exemption policy, configuration and validation; 0201-F5 owns exemption presentation and required-evidence treatment; exemptions do not retire a claim. |
| 5   | Automatic target replacement or migration of bindings to superseding records.                                                                                                                                                      | Replacement meaning requires an explicit choice; 0201-F2 owns invoked repairs.                                                                                      |
| 6   | Filesystem resolution or automatic relativization of rejected provenance, and changes to legacy engine parser APIs.                                                                                                                | F1 validates supplied text and catalog provenance without filesystem or network access; an unknown root cannot safely relativize arbitrary input.                   |

## 6. Concern Separation

<!-- last written by: implementer, 2026-10-17 -->

| Concern                                                    | Classification | Owner and boundary                                                                                                                                              |
| ---------------------------------------------------------- | -------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Explicit local owner components and claim/ADR target kinds | Business       | F1 validates the persisted components without inherited defaults or display-citation parsing.                                                                   |
| Unique resolution in the selected owner                    | Business       | F1 retains duplicate declarations and distinguishes incomplete, invalid, unresolved, ambiguous and resolved references.                                         |
| Current-target lifecycle eligibility                       | Business       | F1 evaluates preserved lifecycle states separately from resolution, approval, exemptions and execution evidence.                                                |
| Effective consumer grammar, role and pin projection        | Both           | The input owner supplies the existing manifest interpretation; F1 validates components using that interpretation rather than parsing a second manifest dialect. |
| Permitted lifecycle omissions                              | Both           | The input owner supplies the pinned schema's established omission interpretation; explicit unknown states always fail eligibility.                              |
| Root-contained provenance                                  | Both           | F1 rejects portable rooted, drive-qualified and escaping provenance before text projection or catalog consumption; valid lexical provenance remains unchanged.  |
| YAML scalar and Markdown table interpretation              | Technical      | The existing engine text-reading implementation is shared at source and characterized against its public readers.                                               |
| Compiler compatibility and dependency closure              | Technical      | Specht.Analysis targets netstandard2.0 under ADR-0017 and does not reference the runtime engine or test frameworks.                                             |
| Input discovery, schema validation and diagnostic policy   | Technical      | F6 owns transport and composition; F1 accepts supplied texts or declarations and performs no filesystem or network work.                                        |

## 7. Technical Design

<!-- last written by: implementer, 2026-10-17 -->

### Component and input boundary

`src/Analyzers/Analyzers.csproj` declares the non-packable `Specht.Analysis`
component, targeting `netstandard2.0` with inherited `TargetFrameworks` cleared
(ADR-0016 and ADR-0017). This item introduces no Roslyn diagnostic, XML parser,
code-fix, discovery adapter or deployment project. Existing engine, CLI and test
targets remain unchanged; ADR-0014's architecture gate remains in force.

The resolver receives an in-memory catalog and an effective pinned policy. The
policy is a projection of the existing `SpecStructure`: `Identifiers`,
`DeclaresEpicGrammar`, `Markers`, `SchemaVersion` and `Roles["claims"]`. In particular, epic
presence comes from `DeclaresEpicGrammar`, which `SpecManifestParser.Parse`
captures before filling omitted identifier defaults. It is not inferred from
the effective dictionary. No new manifest field or parallel JSON parser is added.
The input owner validates the manifest and schema and supplies lifecycle omission
semantics tied to that pin; F1 never makes a missing state current by default.
Shipped schemas requiring `spec_status` retain that requirement. Synthetic pin
policies explicitly supply both omission interpretations for B-019.

F6 will compose validated inputs and own transport and invalid-input diagnostics.
This boundary does not bypass F1 checks: the resolver itself validates required
components, configured grammars, owner selection, declaration cardinality and
lifecycle eligibility. Invalid or unknown explicit lifecycle data cannot produce
eligible evidence. Paths are provenance only, must be root-relative, and never
participate in identity matching.

[`BindingProvenanceGuard`](../BindingProvenanceGuard.cs) validates provenance at
`BindingFeatureReader.Read` entry and when `BindingCatalog` snapshots typed
Feature and ADR declarations. Catalog validation also covers an ADR copied with
an altered init property, so constructor-only checks cannot bypass the boundary.
The immutable validated catalog then supplies resolver results. The guard rejects
POSIX and backslash roots, UNC roots, Windows drive-qualified paths including
`Q:relative`, and any parent segment that traverses above the lexical repository
root under either separator or a mixture. Nested relative paths and parent
segments that remain contained retain their supplied spelling. Empty provenance
is rejected as missing repository-relative provenance.

Rejection throws `ArgumentException` with the fixed message
`Binding provenance must be repository-relative and remain within the root.`
It includes neither the input value nor a parameter-value rendering. Validation
uses lexical segments, with no filesystem access, root argument or automatic
relativization; existing engine parser APIs retain their behavior.

Currently shipped manifest versions declare no ADR grammar or ADR text-record schema. ADR
catalog entries therefore carry their explicit declared local identity, optional
Feature owner and lifecycle state as typed input. F1 requires a nonempty ADR
identity; when the effective policy supplies `Identifiers["adr"]`, that grammar
also validates the whole local ADR ID. This adds no manifest setting, filename
parser or heading/prose convention. F6 owns ADR input projection and introduction
of its agreed ADR grammar through a new schema version. No ownership is inferred from
an enclosing type or document path.

### Data and operations

All new public types live in `Specht.Analysis.Bindings`, beside this agreement.
They are concrete immutable values; collection inputs are copied into read-only
snapshots. There is no service interface, container registration or asynchronous
operation because these operations have no I/O seam.

The compiler owns the public declarations:

| Declaration                                                                                       | Responsibility                                                                                                               |
| ------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------- |
| [`FeatureIdentity`](../FeatureIdentity.cs)                                                        | Ordinal value identity of a Feature owner with separate local components.                                                    |
| [`ClaimReference`](../ClaimReference.cs) and [`AdrReference`](../AdrReference.cs)                 | Explicit persisted targets, including representable incomplete components.                                                   |
| [`LifecycleOmissions`](../LifecycleOmissions.cs) and [`BindingPolicy`](../BindingPolicy.cs)       | Immutable projection of effective pinned identity, marker and omission semantics.                                            |
| [`BindingClaim`](../BindingClaim.cs) and [`BindingFeature`](../BindingFeature.cs)                 | Declared Feature/claim records and lifecycle, retaining replacement and exemption metadata without using it for eligibility. |
| [`BindingAdr`](../BindingAdr.cs)                                                                  | An explicitly declared ADR, with optional Feature owner and replacement metadata.                                            |
| [`BindingCatalog`](../BindingCatalog.cs)                                                          | Read-only declaration inventories retaining duplicates.                                                                      |
| [`BindingTarget`](../BindingTarget.cs) and [`BindingResult`](../BindingResult.cs)                 | Effective selected identity, raw lifecycle, provenance and separate resolution/eligibility findings.                         |
| [`BindingTargetKind`](../BindingTargetKind.cs) and [`BindingResolution`](../BindingResolution.cs) | Closed target-kind boundary and explicit validation/cardinality outcomes.                                                    |
| [`BindingResolver`](../BindingResolver.cs)                                                        | Owner validation, unique resolution, lifecycle eligibility and target-kind interpretation.                                   |
| [`BindingFeatureReader`](../BindingFeatureReader.cs)                                              | Projection of supplied current-format Feature text through shared internal readers.                                          |

`ReadTargetKind` recognizes the local target-kind names `claim` and `adr` only;
this is a kind boundary, not an XML element vocabulary. Nullable identity fields
permit tests and later authoring adapters to represent incomplete references
without obtaining a checked binding. The API has no enclosing-default input.

### Resolution and lifecycle

1. Require the target's local ID and its required owner components. For claims,
   Feature is required. For ADRs, null Feature selects repository ownership;
   otherwise Feature ownership is explicit. A Feature owner requires epic exactly
   when the policy declares that tier. An epic beside repository ownership is
   invalid rather than a new owner kind.
2. Validate supplied owner and claim components with their whole local configured
   grammar, without splitting, trimming or combining them. Missing required
   components produce `Incomplete`; a supplied nonconforming or forbidden
   component produces `Invalid`. Validation precedes catalog lookup.
3. Select declarations with ordinal component equality in that owner only. Keep
   duplicate Feature owners and duplicate target declarations visible; discovery
   order cannot select a winner. Zero target declarations produce `Unresolved`,
   one produces `Resolved`, and multiple matching targets or duplicate matching
   Feature owners produce `Ambiguous`. Return matching provenance in ordinal
   relative-path and line order so input enumeration cannot change findings.
   Without an epic tier, normalize catalog Feature owners and typed Feature ADR
   owners to a null effective epic, ignoring incidental record metadata exactly as
   0001-F5 B-025 requires. Duplicate owners differing only in incidental epic
   metadata remain ambiguous. This record normalization never permits an epic on
   a persisted reference, which remains invalid under B-013.
4. Only a resolved target receives an eligibility value. Active or Amended claims
   require draft, in-review or the effective approved Feature marker; accepted ADRs
   are eligible. Interpret manifest-backed lifecycle markers through the effective
   marker map, retaining the raw state in results. In particular, the configured
   approved marker replaces its default spelling; configured withdrawn and
   superseded claim markers remain historical. Other unknown explicit states are
   ineligible. Null states use only the corresponding pinned omission interpretation.
5. Retain the selected target and raw lifecycle even when ineligible. Do not follow
   `ReplacementId`, consult `CoverageExemption`, change Feature maturity or execute
   tests. Repeating the operation over identical inputs yields identical results.

### Text reuse and compiler closure

The existing source authorities are
[`FrontmatterReader.Parse`](../../../specht/Model/FrontmatterReader.cs),
[`Frontmatter.Text`](../../../specht/Model/Frontmatter.cs),
[`SpecDocument.Parse`](../../../specht/Model/SpecDocument.cs),
[`SpecClaims`](../../../specht/Model/SpecClaims.cs) and
[`MatrixResolver.ResolveClaims`](../../../specht/Rules/Matrix/MatrixResolver.cs).
They define YAML scalar preservation, top-level section and first-table selection,
visible inline/code-span text, trimmed first-cell claim identity, duplicate
retention and fourth-cell claim lifecycle respectively.
[`ApprovalRule`](../../../specht/Rules/ApprovalRule.cs) and
[`RetiredClaimRule`](../../../specht/Rules/Matrix/RetiredClaimRule.cs) define the
existing marker interpretation that the effective policy retains.

The existing text algorithms now live in internal
[`FrontmatterTextReader`](../../../specht/Model/FrontmatterTextReader.cs) and
[`MarkdownTextReader`](../../../specht/Model/MarkdownTextReader.cs), with their
internal result declarations beside them under `src/specht/Model`, their
authoritative home. Existing public engine readers delegate without changing
signatures or interpretation. The engine retains its existing cached Markdown
pipeline and passes it to the instance reader; the compiler projection owns an
instance pipeline with the same pipe-table configuration. The analyzer project
compiles those four internal sources with explicit production source links. Internal shared result types avoid
public duplicate declarations and assembly aliases; this adds no shared-core
assembly, engine project reference, parser rewrite or test-support source link.
Compatibility substitutions use portable equivalents, retaining scalar types,
section/table contents and one-based source lines.

`BindingFeatureReader` projects those shared text results into a `BindingFeature`:
`epic`, `id` and `spec_status` come from typed frontmatter strings; claims come
from the configured claims section's first pipe table, with ID in column one and
lifecycle in column four. All rows remain available for uniqueness checks. It
performs no filesystem discovery, schema evaluation or ADR text parsing. F6 may
supply typed Feature declarations for its configured lifecycle key or claim-status
column; extending this current-format text projection belongs to that input work
rather than adding unused profile options here. Characterization includes
configured approved/retirement markers and incidental epic metadata under a
no-epic policy, with independent expected values and engine baseline protection.

The analyzer references the existing centrally pinned Markdig and YamlDotNet
packages and a centrally declared compatible System.Text.Json package for the
shared YAML representation. Their selected assets must support netstandard2.0;
there is no dependency on Reqnroll, xUnit, Playwright, Spectre, Workspaces or the
runtime engine. F1 verifies the actual compile/dependency closure; compiler-host
loading and package-content checks remain downstream deployment obligations.

The implementer supplied compiling declarations and throwing operation stubs
after design review. The test-writer recorded behavioral-red mechanism and
acceptance runs against that scaffold before the implementer supplied the
production behavior. Execution results and coverage remain owned by § 8 and § 9.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-17 -->

`BindingSteps` exercises the companion scenarios through the public in-memory API. The mechanism tests below use synthetic records, repository-relative provenance and the pinned policy projection from § 7. The claim mappings in § 9 record acceptance and mechanism evidence separately from agreement maturity.

| Claim | Mechanism and unit test                                                                                                                                                                                                         |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| B-001 | Select the consumer's configured grammars and distinguish format validation from target lookup. `BindingResolverUnitTests.CustomLocalGrammars_WhenResolved_ShouldValidateWholeComponentsBeforeLookingUpTargets`.                |
| B-002 | Limit claim lookup to the explicitly declared owning Feature. `BindingResolverUnitTests.RepeatedClaimIds_WhenResolved_ShouldSelectOnlyTheExplicitOwner`.                                                                        |
| B-003 | Resolve Feature scope without creating an epic when the consumer has no epic tier. `BindingResolverUnitTests.ConsumerEpicTier_WhenValidatingReferences_ShouldRequireOrRejectTheEpicComponent`.                                  |
| B-004 | Select repository or Feature ADR ownership from the explicit Feature component. `BindingResolverUnitTests.AdrOwnerComponents_WhenResolved_ShouldSelectOnlyTheDeclaredOwner`.                                                    |
| B-005 | Derive the same identities and findings from identical inputs without test-framework services. `BindingFeatureReaderUnitTests.IdenticalInputsWithoutFrameworkServices_WhenResolvedTwice_ShouldYieldTheSameIdentityAndFindings`. |
| B-006 | Validate separate local identity components without splitting a qualified display citation. `BindingResolverUnitTests.CustomLocalGrammars_WhenResolved_ShouldValidateWholeComponentsBeforeLookingUpTargets`.                    |
| B-007 | Preserve an unresolved result when only another owner declares the ADR. `BindingResolverUnitTests.AdrMissingFromSelectedOwner_WhenResolved_ShouldRemainUnresolved`.                                                             |
| B-008 | Evaluate the Active/Amended claim and draft/in-review/approved Feature combinations. `BindingLifecycleUnitTests.ClaimAndFeatureLifecycle_WhenEvaluated_ShouldSeparateResolutionFromEligibility`.                                |
| B-009 | Retain resolved identity and lifecycle while rejecting historical claim or Feature states. `BindingLifecycleUnitTests.ClaimAndFeatureLifecycle_WhenEvaluated_ShouldSeparateResolutionFromEligibility`.                          |
| B-010 | Admit an accepted ADR for current evidence. `BindingLifecycleUnitTests.AdrLifecycle_WhenEvaluated_ShouldAdmitOnlyAcceptedDecisions`.                                                                                            |
| B-011 | Retain resolved identity and lifecycle while rejecting proposed or historical ADR states. `BindingLifecycleUnitTests.AdrLifecycle_WhenEvaluated_ShouldAdmitOnlyAcceptedDecisions`.                                              |
| B-012 | Reject additional checked target kinds while retaining claims and ADRs. `BindingResolverUnitTests.RequestedTargetKind_WhenInterpreted_ShouldAdmitOnlyClaimsAndAdrs`.                                                            |
| B-013 | Require or reject the epic component according to the consumer's configured tier. `BindingResolverUnitTests.ConsumerEpicTier_WhenValidatingReferences_ShouldRequireOrRejectTheEpicComponent`.                                   |
| B-014 | Keep incomplete persisted identities incomplete despite containing-type defaults. `BindingResolverUnitTests.MissingMemberOwner_WhenResolved_ShouldRemainIncompleteWithoutBorrowingContext`.                                     |
| B-015 | Classify zero, one and multiple matching declarations as unresolved, resolved and ambiguous. `BindingResolverUnitTests.TargetCardinality_WhenResolved_ShouldDistinguishMissingUniqueAndAmbiguous`.                              |
| B-016 | Resolve the same declared identity and owner after its record path changes. `BindingResolverUnitTests.RecordMovedWithoutIdentityChanges_WhenResolved_ShouldPreserveTheTarget`.                                                  |
| B-017 | Retain the original superseded target instead of following its replacement. `BindingLifecycleUnitTests.SupersededTargetWithReplacement_WhenResolved_ShouldRetainTheOriginalTarget`.                                             |
| B-018 | Evaluate lifecycle eligibility independently of a coverage exemption. `BindingLifecycleUnitTests.EligibleClaimWithCoverageExemption_WhenEvaluated_ShouldRemainCurrent`.                                                         |
| B-019 | Apply the pinned schema's established semantics for permitted lifecycle omissions. `BindingLifecycleUnitTests.PermittedMissingLifecycle_WhenEvaluated_ShouldUseOnlyItsPinnedOmissionPolicy`.                                    |
| B-020 | Reject an explicit unknown lifecycle state while retaining identity and that state. `BindingLifecycleUnitTests.UnknownExplicitLifecycle_WhenEvaluated_ShouldRemainResolvedButIneligible`.                                       |
| B-021 | Leave agreement approval unchanged when evaluating an eligible draft claim. `BindingLifecycleUnitTests.EligibleDraftClaim_WhenEvaluated_ShouldPreserveAgreementMaturity`.                                                       |
| B-022 | Validate portable provenance before reader or typed-catalog input can emit a result. `BindingProvenanceUnitTests.ProvenanceAtEachInputBoundary_WhenPrepared_ShouldRejectUnsafePathsAndRetainContainedPaths`.                    |

Additional unit cases pin validation before lookup, duplicate-owner ambiguity, stable provenance ordering, configured lifecycle markers and the optional ADR grammar. `BindingFeatureReaderUnitTests` pins configured-section selection, first-table semantics, duplicate retention, visible code-span IDs, one-based lines and YAML string preservation. Typed ClassData enumerates the lifecycle and identity boundaries; no network or ambient clock participates.

The reviewed API exposes resolution separately from lifecycle eligibility, so historical and unknown targets must remain observable rather than being mislabeled unresolved. Omission fixtures explicitly supply the selected pin's interpretation; six mixed-policy cases make each omitted field's selected boolean differ from the other two, covering both eligibility outcomes while other lifecycle states remain explicit. Source-reading characterization and isolated dependency loading provide the integration tier. The supplied-input reader and resolver require no test-only seam; there is no outstanding F1 testability gap.

`BindingProvenanceUnitTests` constructs unsafe input from descriptive categories inside the test, so discovered theory names carry no rooted paths. All three input boundaries are exercised for seven unsafe categories and three contained-path categories. The unit tier uses eight unsafe cases, including rooted and drive-relative variants of the Windows drive category, while acceptance uses seven unsafe examples. Both tiers include three valid cases. ADR catalog input uses a valid record changed with `with` to prevent constructor-only validation from satisfying the claim. Rejection must use the same path-free error for every boundary, while valid provenance remains unchanged.

On each of `net10.0` and `net11.0`, actual discovery and execution report 102 passing unit tests including the tier guard, five passing integration cases and 69 passing F1 acceptance examples, with no errors or skipped tests. Integration cases exercise independent expected parser outputs through both readers and real analysis loading, text projection and resolution in an isolated context that excludes the runtime engine and test frameworks. Red and green logs are retained under `.artifacts/0238/`; package-content and editor/compiler-host checks remain downstream deployment obligations.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-10-17 -->

Scenario entries identify the claim tag in [bindings.feature](bindings.feature); titles are reading aids. Tests live in `test/analysis.tests/Bindings` and `test/acceptance/Bindings`.

| Claim ID | Scenario                                                                                                      | Test                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                | Status  |
| -------- | ------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------- |
| B-001    | `@B-001` Consumer grammars                                                                                    | `BindingSteps`; `BindingResolverUnitTests.CustomLocalGrammars_WhenResolved_ShouldValidateWholeComponentsBeforeLookingUpTargets`; `BindingResolverUnitTests.MissingOrMalformedComponents_WhenResolved_ShouldFailValidationBeforeCatalogLookup`; `BindingFeatureReaderUnitTests.ConfiguredClaimsSection_WhenRead_ShouldKeepFirstTableDuplicateIdsAndSourceLines`; `EngineTextCompatibilityIntegrationTests.ScalarFrontmatter_WhenReadThroughBothComponents_ShouldPreserveTypedValuesAndSourceLines`; `EngineTextCompatibilityIntegrationTests.ConfiguredClaimsSection_WhenProjected_ShouldRetainFirstTableVisibleIdsDuplicatesAndFourthCellLifecycle` | Covered |
| B-002    | `@B-002` Owning scope                                                                                         | `BindingSteps`; `BindingResolverUnitTests.RepeatedClaimIds_WhenResolved_ShouldSelectOnlyTheExplicitOwner`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           | Covered |
| B-003    | `@B-003` No epic tier                                                                                         | `BindingSteps`; `BindingResolverUnitTests.ConsumerEpicTier_WhenValidatingReferences_ShouldRequireOrRejectTheEpicComponent`; `BindingResolverUnitTests.IncidentalEpicMetadata_WhenEpicTierIsAbsent_ShouldNormalizeOwnersAndRetainAmbiguity`; `EngineTextCompatibilityIntegrationTests.NoEpicTextWithCustomMarkers_WhenResolved_ShouldIgnoreIncidentalEpicAndRetainRawLifecycle`                                                                                                                                                                                                                                                                      | Covered |
| B-004    | `@B-004` ADR ownership                                                                                        | `BindingSteps`; `BindingResolverUnitTests.AdrOwnerComponents_WhenResolved_ShouldSelectOnlyTheDeclaredOwner`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         | Covered |
| B-005    | `@B-005` Framework independence                                                                               | `BindingSteps`; `BindingFeatureReaderUnitTests.IdenticalInputsWithoutFrameworkServices_WhenResolvedTwice_ShouldYieldTheSameIdentityAndFindings`; `CompilerClosureIntegrationTests.PortableComponent_WhenLoadedWithoutRuntimeEngineOrTestFrameworks_ShouldPreserveResolutionAndTextProjection`                                                                                                                                                                                                                                                                                                                                                       | Covered |
| B-006    | `@B-006` Separate claim identity components                                                                   | `BindingSteps`; `BindingResolverUnitTests.CustomLocalGrammars_WhenResolved_ShouldValidateWholeComponentsBeforeLookingUpTargets`; `BindingFeatureReaderUnitTests.FrontmatterScalars_WhenProjected_ShouldPreserveStringsWithoutCoercingOtherYamlTypes`; `EngineTextCompatibilityIntegrationTests.ScalarFrontmatter_WhenReadThroughBothComponents_ShouldPreserveTypedValuesAndSourceLines`                                                                                                                                                                                                                                                             | Covered |
| B-007    | `@B-007` No ADR scope fallback                                                                                | `BindingSteps`; `BindingResolverUnitTests.AdrMissingFromSelectedOwner_WhenResolved_ShouldRemainUnresolved`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          | Covered |
| B-008    | `@B-008` Current claim eligibility                                                                            | `BindingSteps`; `BindingLifecycleUnitTests.ClaimAndFeatureLifecycle_WhenEvaluated_ShouldSeparateResolutionFromEligibility`; `BindingLifecycleUnitTests.ConfiguredApprovedMarker_WhenEvaluated_ShouldReplaceDefaultSpelling`; `EngineTextCompatibilityIntegrationTests.NoEpicTextWithCustomMarkers_WhenResolved_ShouldIgnoreIncidentalEpicAndRetainRawLifecycle`                                                                                                                                                                                                                                                                                     | Covered |
| B-009    | `@B-009` Historical claim eligibility                                                                         | `BindingSteps`; `BindingLifecycleUnitTests.ClaimAndFeatureLifecycle_WhenEvaluated_ShouldSeparateResolutionFromEligibility`; `BindingLifecycleUnitTests.ConfiguredRetirementMarkers_WhenEvaluated_ShouldRetainHistoricalState`; `EngineTextCompatibilityIntegrationTests.NoEpicTextWithCustomMarkers_WhenResolved_ShouldIgnoreIncidentalEpicAndRetainRawLifecycle`                                                                                                                                                                                                                                                                                   | Covered |
| B-010    | `@B-010` Accepted ADR eligibility                                                                             | `BindingSteps`; `BindingLifecycleUnitTests.AdrLifecycle_WhenEvaluated_ShouldAdmitOnlyAcceptedDecisions`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             | Covered |
| B-011    | `@B-011` Historical or proposed ADR eligibility                                                               | `BindingSteps`; `BindingLifecycleUnitTests.AdrLifecycle_WhenEvaluated_ShouldAdmitOnlyAcceptedDecisions`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             | Covered |
| B-012    | `@B-012` Checked target boundary                                                                              | `BindingSteps`; `BindingResolverUnitTests.RequestedTargetKind_WhenInterpreted_ShouldAdmitOnlyClaimsAndAdrs`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         | Covered |
| B-013    | `@B-013` Epic component follows the consumer tier                                                             | `BindingSteps`; `BindingResolverUnitTests.ConsumerEpicTier_WhenValidatingReferences_ShouldRequireOrRejectTheEpicComponent`; `BindingResolverUnitTests.IncidentalEpicMetadata_WhenEpicTierIsAbsent_ShouldNormalizeOwnersAndRetainAmbiguity`                                                                                                                                                                                                                                                                                                                                                                                                          | Covered |
| B-014    | `@B-014` Checked identity is not completed from authoring context                                             | `BindingSteps`; `BindingResolverUnitTests.MissingMemberOwner_WhenResolved_ShouldRemainIncompleteWithoutBorrowingContext`; `BindingResolverUnitTests.MissingOrMalformedComponents_WhenResolved_ShouldFailValidationBeforeCatalogLookup`                                                                                                                                                                                                                                                                                                                                                                                                              | Covered |
| B-015    | `@B-015` Exactly one declared target                                                                          | `BindingSteps`; `BindingResolverUnitTests.TargetCardinality_WhenResolved_ShouldDistinguishMissingUniqueAndAmbiguous`; `BindingResolverUnitTests.IncidentalEpicMetadata_WhenEpicTierIsAbsent_ShouldNormalizeOwnersAndRetainAmbiguity`; `BindingResolverUnitTests.DuplicateTargetsInDifferentOrders_WhenResolved_ShouldKeepStableProvenanceOrder`; `EngineTextCompatibilityIntegrationTests.ConfiguredClaimsSection_WhenProjected_ShouldRetainFirstTableVisibleIdsDuplicatesAndFourthCellLifecycle`                                                                                                                                                   | Covered |
| B-016    | `@B-016` Record moves preserve target identity                                                                | `BindingSteps`; `BindingResolverUnitTests.RecordMovedWithoutIdentityChanges_WhenResolved_ShouldPreserveTheTarget`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   | Covered |
| B-017    | `@B-017` Supersession does not redirect a binding                                                             | `BindingSteps`; `BindingLifecycleUnitTests.SupersededTargetWithReplacement_WhenResolved_ShouldRetainTheOriginalTarget`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | Covered |
| B-018    | `@B-018` Coverage exemption does not retire a claim                                                           | `BindingSteps`; `BindingLifecycleUnitTests.EligibleClaimWithCoverageExemption_WhenEvaluated_ShouldRemainCurrent`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    | Covered |
| B-019    | `@B-019` Permitted lifecycle omission follows the pin                                                         | `BindingSteps`; `BindingLifecycleUnitTests.PermittedMissingLifecycle_WhenEvaluated_ShouldUseOnlyItsPinnedOmissionPolicy`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            | Covered |
| B-020    | `@B-020` Unknown explicit lifecycle state                                                                     | `BindingSteps`; `BindingLifecycleUnitTests.UnknownExplicitLifecycle_WhenEvaluated_ShouldRemainResolvedButIneligible`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                | Covered |
| B-021    | `@B-021` Eligibility does not approve an agreement                                                            | `BindingSteps`; `BindingLifecycleUnitTests.EligibleDraftClaim_WhenEvaluated_ShouldPreserveAgreementMaturity`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | Covered |
| B-022    | `@B-022` Unsafe provenance is rejected before binding results; Repository-contained provenance remains usable | `BindingSteps`; `BindingProvenanceUnitTests.ProvenanceAtEachInputBoundary_WhenPrepared_ShouldRejectUnsafePathsAndRetainContainedPaths`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | Covered |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-17 -->

2026-10-17: Owner adoption of OQ-1 and OQ-2 adds the consumer-tier completeness,
unique-resolution and move-stability boundaries in B-013 through B-016, and the
lifecycle, compatibility and approval boundaries in B-017 through B-021.
B-004 and B-006 retain their IDs with conditional epic qualification clarified;
dated addenda to decisions 0002 through 0004 preserve the earlier choices.

2026-10-17: Final implementation review found that the root-relative provenance
promise in § 7 and brief § 9 had no claim or scenario guarding supplied catalog
paths. B-022 makes rejection and safe errors testable at that boundary;
[lesson 0001](lessons/0001-provenance-must-be-validated-before-emission.md)
records the product gap.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-17 -->

| ID   | Question                                                                                                   | Blocks                        | Resolution                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| ---- | ---------------------------------------------------------------------------------------------------------- | ----------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Which public encoding carries local Feature, optional epic and ADR ownership without assuming a delimiter? | None for reference components | RESOLVED, 2026-10-17: decisions 0002 and 0003 with their dated addenda define separate local identity components and explicit ADR ownership: absent `feature` selects the repository; present `feature` selects only that Feature. A Feature owner requires epic exactly when the manifest declares that tier. Persisted members borrow no missing components; only one matching record resolves, independently of paths. 0201-F2 owns XML roles, shape and repairs.                                                                                                                                                                                                              |
| OQ-2 | Which claim and ADR lifecycle states are eligible targets?                                                 | None for lifecycle policy     | RESOLVED, 2026-10-17: decision 0004 and its dated addendum admit Active or Amended claims in draft, in-review or approved Features and accepted ADRs; retired claims, claims under superseded Features and proposed or retired ADRs remain resolving but ineligible. No automatic replacement occurs; exemptions leave lifecycle unchanged, permitted omissions retain pinned-schema semantics, and unrecognized explicit states are ineligible. Eligibility establishes neither approval nor passing behavior; 0201-F5 owns exemption presentation and required-evidence treatment, while 0201-F6 owns exemption policy, configuration and validation, severity and suppression. |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-17 -->

| Section     | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| ----------- | ------ | ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-12        | 🟡     | Pending       | Owner selected the model; this decomposed Feature has not received detailed agreement or implementation sign-off.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| 1-12 (0238) | 🟢     | spec-reviewer | 2026-10-17: Scoped implementation approved with no remaining findings across B-001 through B-022, decisions 0001–0005 and ADR-0014/0016/0017. Each framework passes 102 unit, 5 integration and 69 F1 acceptance cases without skips or errors. Full repository Format/Compile pass; 2,268 unit and 722 integration tests pass. Full acceptance has 1,034 passes, 836 skips and six existing failures from three published-checker call-site cases on both frameworks (0237); the installed checker rejects manifest tests/owners. The source checker reports zero errors and 142 existing warnings, none in F1. CI has not run for this change; compiler-host deployment remains downstream. |

## Tasks

- [0238: Implement binding identity and record resolution](../.issue/0238-binding-identity.yml)

## Scoring

| Field | Value | Basis           |
| ----- | ----- | --------------- |
| value | 0     | Not yet scored. |
| risk  | 0     | Not yet scored. |
