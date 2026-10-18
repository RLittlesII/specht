---
title: "Specification: Binding identity"
description: "Resolve stable claim and ADR identities using consumer grammars without a test-framework dependency."
type: feature
id: "F1"
epic: "0201"
spec_status: draft
status: ready-for-architecture
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

| ID    | Claim                                                                                                                                                                                                                             | Source                                      | Status  |
| ----- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------- | ------- |
| B-001 | Given a manifest defining custom Feature and claim grammars and records using them, this Feature ensures references are validated and resolved using those configured grammars.                                                   | Epic 0201; decision 0001 in 0201-F1         | Active  |
| B-002 | Given two Features declaring the same local claim ID, this Feature ensures a qualified claim reference resolves only in its declared owning Feature.                                                                              | Epic 0201; decision 0001 in 0201-F1         | Active  |
| B-003 | Given a consumer whose configured model has no epic tier, this Feature ensures a Feature and claim reference resolves without inventing an epic.                                                                                  | Epic 0201; decision 0001 in 0201-F1         | Active  |
| B-004 | Given an ADR reference, this Feature selects only repository scope when `feature` is absent or the explicitly named Feature scope when `feature` is present.                                                                      | Decision 0003; addendum 2026-10-17          | Amended |
| B-005 | Given the same specification inputs and declared references with no Reqnroll or test-framework assemblies, this Feature ensures reference resolution yields the same identities and findings.                                     | Epic 0201; decision 0001 in 0201-F1         | Active  |
| B-006 | Given a claim reference, this Feature represents its identity in separate local `epic`, `feature` and `claim` fields under the consumer's configured grammars.                                                                    | Decision 0002; addendum 2026-10-17          | Amended |
| B-007 | Given an ADR missing from its selected owner but declared under another owner, this Feature reports the reference as unresolved without searching another scope.                                                                  | Decision 0003                               | Active  |
| B-008 | Given an Active or Amended claim in a draft, in-review or approved Feature, this Feature reports a resolving reference to that claim as eligible for current evidence.                                                            | Decision 0004                               | Active  |
| B-009 | Given a resolving reference to a Withdrawn or Superseded claim or a claim in a superseded Feature, this Feature reports the target as ineligible for current evidence while preserving its resolved identity and lifecycle state. | Decision 0004                               | Active  |
| B-010 | Given an accepted ADR, this Feature reports a resolving reference to that decision as eligible for current evidence.                                                                                                              | Decision 0004                               | Active  |
| B-011 | Given a resolving reference to a proposed, rejected, deprecated or superseded ADR, this Feature reports the target as ineligible for current evidence while preserving its resolved identity and lifecycle state.                 | Decision 0004                               | Active  |
| B-012 | Given a checked relationship, this Feature permits claim or ADR targets only; Feature and epic identities provide scope rather than additional checked target kinds.                                                              | Decision 0005                               | Active  |
| B-013 | Given a claim or Feature-owned ADR reference, this Feature requires its epic component exactly when the consumer's manifest declares an epic tier and rejects that component otherwise.                                           | Decisions 0002 and 0003; addenda 2026-10-17 | Active  |
| B-014 | Given a persisted member reference missing a required target identity component, this Feature leaves it incomplete regardless of enclosing type defaults.                                                                         | Decision 0002; addendum 2026-10-17          | Active  |
| B-015 | Given a complete reference conforming to the consumer grammars, this Feature resolves it only when exactly one record matches in its selected owner, classifying zero matches as unresolved and multiple matches as ambiguous.    | Decision 0002; addendum 2026-10-17          | Active  |
| B-016 | Given a specification record moved without changing its declared identity or owner, this Feature preserves the target identity of references to that record.                                                                      | Decision 0002; addendum 2026-10-17          | Active  |
| B-017 | Given a resolving reference to a superseded target naming a current replacement, this Feature retains the original target without automatically redirecting the binding.                                                          | Decision 0004; addendum 2026-10-17          | Active  |
| B-018 | Given an otherwise eligible current claim with a coverage exemption, this Feature preserves its lifecycle eligibility independently of that exemption.                                                                            | Decision 0004; addendum 2026-10-17          | Active  |
| B-019 | Given omitted lifecycle information permitted by the pinned schema, this Feature evaluates lifecycle eligibility using that schema's established omission semantics.                                                              | Decision 0004; addendum 2026-10-17          | Active  |
| B-020 | Given a resolving reference with an explicit unrecognized claim, owning Feature or ADR lifecycle state, this Feature reports the target as ineligible while preserving its identity and unrecognized state.                       | Decision 0004; addendum 2026-10-17          | Active  |
| B-021 | Given an eligible claim in a draft Feature, this Feature leaves the agreement's approval state unchanged.                                                                                                                         | Decision 0004; addendum 2026-10-17          | Active  |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-17 -->

| ID  | Constraint                                                                                                                | Rules Out                                                                          |
| --- | ------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------- |
| C-1 | Reuse manifest identity and discovery semantics (0001-F5 C-2); no parallel regex configuration or first-match resolution. | A second ID parser or implicit first-match lookup.                                 |
| C-2 | Keep format validation distinct from existence and ambiguity checks.                                                      | Treating a regex match as a resolving reference.                                   |
| C-3 | Binding identity depends on no Reqnroll, xUnit or Playwright types.                                                       | Mandatory test-framework packages in identity resolution.                          |
| C-4 | Record paths and qualification delimiters do not define binding identity.                                                 | Retargeting on specification moves or splitting local IDs by an assumed delimiter. |
| C-5 | Resolution and lifecycle eligibility establish neither agreement approval nor passing behavior.                           | Promoting a resolving or eligible binding into an approval or test verdict.        |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-17 -->

| #   | Item                                                                                                                                                                                                                               | Exclusion Reason                                                                                                                                                    |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | XML roles, supported declarations and repeated single-target elements are defined in 0201-F2 decisions 0002, 0003 and 0005; strict XML validation is defined by F2 decision 0006. Claim identity field names are defined by B-006. | This agreement does not authorize it.                                                                                                                               |
| 2   | Test discovery, execution and scenario links belong to adapters; a resolved reference alone is no behavioral proof.                                                                                                                | This agreement does not authorize it.                                                                                                                               |
| 3   | Additional checked record kinds beyond claims and ADRs.                                                                                                                                                                            | Decision 0005 limits the initial checked target set.                                                                                                                |
| 4   | Coverage exemption policy and required-evidence reconciliation.                                                                                                                                                                    | 0201-F6 owns exemption policy, configuration and validation; 0201-F5 owns exemption presentation and required-evidence treatment; exemptions do not retire a claim. |
| 5   | Automatic target replacement or migration of bindings to superseding records.                                                                                                                                                      | Replacement meaning requires an explicit choice; 0201-F2 owns invoked repairs.                                                                                      |

## 6. Concern Separation

<!-- last written by: implementer, 2026-10-17 -->

Not yet designed; no production implementation is authorized by this draft.

## 7. Technical Design

<!-- last written by: implementer, 2026-10-17 -->

Decisions 0002 through 0005 settle identity components, ADR ownership, lifecycle eligibility and checked target kinds. Detailed implementation design remains to be written; the isolated spike is not production architecture.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-17 -->

The companion scenarios state the acceptance obligations; production step definitions and mechanism tests are not written. Each mechanism below needs a unit test in addition to its acceptance scenario. Detailed test seams and member names await the implementation design in § 7; this is a design-stage gap, not an exemption from the unit tier. Spike results are not production coverage.

| Claim | Mechanism to pin with a unit test                                                               |
| ----- | ----------------------------------------------------------------------------------------------- |
| B-001 | Select the consumer's configured grammars and distinguish format validation from target lookup. |
| B-002 | Limit claim lookup to the explicitly declared owning Feature.                                   |
| B-003 | Resolve Feature scope without creating an epic when the consumer has no epic tier.              |
| B-004 | Select repository or Feature ADR ownership from the explicit Feature component.                 |
| B-005 | Derive the same identities and findings from identical inputs without test-framework services.  |
| B-006 | Validate separate local identity components without splitting a qualified display citation.     |
| B-007 | Preserve an unresolved result when only another owner declares the ADR.                         |
| B-008 | Evaluate the Active/Amended claim and draft/in-review/approved Feature combinations.            |
| B-009 | Retain resolved identity and lifecycle while rejecting historical claim or Feature states.      |
| B-010 | Admit an accepted ADR for current evidence.                                                     |
| B-011 | Retain resolved identity and lifecycle while rejecting proposed or historical ADR states.       |
| B-012 | Reject additional checked target kinds while retaining claims and ADRs.                         |
| B-013 | Require or reject the epic component according to the consumer's configured tier.               |
| B-014 | Keep incomplete persisted identities incomplete despite containing-type defaults.               |
| B-015 | Classify zero, one and multiple matching declarations as unresolved, resolved and ambiguous.    |
| B-016 | Resolve the same declared identity and owner after its record path changes.                     |
| B-017 | Retain the original superseded target instead of following its replacement.                     |
| B-018 | Evaluate lifecycle eligibility independently of a coverage exemption.                           |
| B-019 | Apply the pinned schema's established semantics for permitted lifecycle omissions.              |
| B-020 | Reject an explicit unknown lifecycle state while retaining identity and that state.             |
| B-021 | Leave agreement approval unchanged when evaluating an eligible draft claim.                     |

Use synthetic, repository-relative records and in-memory inputs for the unit obligations. The design must expose resolution and lifecycle results separately so assertions can distinguish unresolved, ambiguous and resolved-but-ineligible targets. B-019 requires fixtures declaring each pinned omission interpretation; an invented default must not substitute for that schema contract. Test discovery and acceptance wiring must be verified when production tests are authored.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-10-17 -->

Scenario entries identify the claim tag in [bindings.feature](bindings.feature); titles are reading aids.

| Claim ID | Scenario                                                          | Test    | Status  |
| -------- | ----------------------------------------------------------------- | ------- | ------- |
| B-001    | `@B-001` Consumer grammars                                        | Missing | Missing |
| B-002    | `@B-002` Owning scope                                             | Missing | Missing |
| B-003    | `@B-003` No epic tier                                             | Missing | Missing |
| B-004    | `@B-004` ADR ownership                                            | Missing | Missing |
| B-005    | `@B-005` Framework independence                                   | Missing | Missing |
| B-006    | `@B-006` Separate claim identity components                       | Missing | Missing |
| B-007    | `@B-007` No ADR scope fallback                                    | Missing | Missing |
| B-008    | `@B-008` Current claim eligibility                                | Missing | Missing |
| B-009    | `@B-009` Historical claim eligibility                             | Missing | Missing |
| B-010    | `@B-010` Accepted ADR eligibility                                 | Missing | Missing |
| B-011    | `@B-011` Historical or proposed ADR eligibility                   | Missing | Missing |
| B-012    | `@B-012` Checked target boundary                                  | Missing | Missing |
| B-013    | `@B-013` Epic component follows the consumer tier                 | Missing | Missing |
| B-014    | `@B-014` Checked identity is not completed from authoring context | Missing | Missing |
| B-015    | `@B-015` Exactly one declared target                              | Missing | Missing |
| B-016    | `@B-016` Record moves preserve target identity                    | Missing | Missing |
| B-017    | `@B-017` Supersession does not redirect a binding                 | Missing | Missing |
| B-018    | `@B-018` Coverage exemption does not retire a claim               | Missing | Missing |
| B-019    | `@B-019` Permitted lifecycle omission follows the pin             | Missing | Missing |
| B-020    | `@B-020` Unknown explicit lifecycle state                         | Missing | Missing |
| B-021    | `@B-021` Eligibility does not approve an agreement                | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-17 -->

2026-10-17: Owner adoption of OQ-1 and OQ-2 adds the consumer-tier completeness,
unique-resolution and move-stability boundaries in B-013 through B-016, and the
lifecycle, compatibility and approval boundaries in B-017 through B-021.
B-004 and B-006 retain their IDs with conditional epic qualification clarified;
dated addenda to decisions 0002 through 0004 preserve the earlier choices.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-17 -->

| ID   | Question                                                                                                   | Blocks                        | Resolution                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| ---- | ---------------------------------------------------------------------------------------------------------- | ----------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Which public encoding carries local Feature, optional epic and ADR ownership without assuming a delimiter? | None for reference components | RESOLVED, 2026-10-17: decisions 0002 and 0003 with their dated addenda define separate local identity components and explicit ADR ownership: absent `feature` selects the repository; present `feature` selects only that Feature. A Feature owner requires epic exactly when the manifest declares that tier. Persisted members borrow no missing components; only one matching record resolves, independently of paths. 0201-F2 owns XML roles, shape and repairs.                                                                                                                                                                                                              |
| OQ-2 | Which claim and ADR lifecycle states are eligible targets?                                                 | None for lifecycle policy     | RESOLVED, 2026-10-17: decision 0004 and its dated addendum admit Active or Amended claims in draft, in-review or approved Features and accepted ADRs; retired claims, claims under superseded Features and proposed or retired ADRs remain resolving but ineligible. No automatic replacement occurs; exemptions leave lifecycle unchanged, permitted omissions retain pinned-schema semantics, and unrecognized explicit states are ineligible. Eligibility establishes neither approval nor passing behavior; 0201-F5 owns exemption presentation and required-evidence treatment, while 0201-F6 owns exemption policy, configuration and validation, severity and suppression. |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-17 -->

| Section | Status | Reviewer | Note                                                                                                              |
| ------- | ------ | -------- | ----------------------------------------------------------------------------------------------------------------- |
| 1-12    | 🟡     | Pending  | Owner selected the model; this decomposed Feature has not received detailed agreement or implementation sign-off. |

## Tasks

- [0238: Implement binding identity and record resolution](../.issue/0238-binding-identity.yml)

## Scoring

| Field | Value | Basis           |
| ----- | ----- | --------------- |
| value | 0     | Not yet scored. |
| risk  | 0     | Not yet scored. |
