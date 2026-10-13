---
title: "Epic 0201: spec.code analysis"
description: "Roslyn analyzers and scenario bindings connect implementation and executable specifications to claims, Feature specifications and ADRs, with explicit evidence of unresolved references and coverage gaps"
id: "0201"
type: epic
status: deferred
priority: med
milestone: null
children: []
created: "2026-10-13"
updated: "2026-10-13"
github_issue: null
---

# Epic 0201: spec.code analysis

## Summary

The specification tree connects claims to scenarios and tests, but the code
that implements a claim has no checked binding back to that agreement. A
reference in a comment can become stale, a scenario can cite the wrong Feature's
claim, and a coverage row can name a test without identifying the implementation
it exercises. The existing check cannot close those gaps from Markdown alone.

This epic adds code-level traceability through Roslyn analyzers and connects
those bindings to the claims tagged in executable specification files. Code can
declare the claims, Feature specifications and ADRs it satisfies or follows;
the checks resolve those declarations to the actual records. Reconciliation
then exposes the chain from a claim through its scenario and test to its
implementation, including missing or unresolved links.

The repository owner requested this expansion on 2026-10-13, using the working
name `spec.code`. That name does not yet choose a package, assembly or command.
This is an epic draft: no Feature is agreed, no work item is cut and no analyzer
is implemented here. The candidate cut and open questions guide decomposition.

### Illustrative code binding

XML documentation comments can express the intended shape: a type names its
specification location and a member cites a requirement within that agreement.

```csharp
/// <spec path="/.spec/features/Authentication" />
public sealed class Authentication
{
    /// <requirement>@REQ-AU-054</requirement>
    public void Foo() {}
}
```

This example informs OQ-2 and OQ-3. The exact XML contract, the meaning of the
leading slash and directory-form path, and the relationship between requirement
identifiers such as `REQ-AU-054` and the configured claim grammar remain open.
OQ-12 covers how the member obtains its specification scope.

## Business Value

**The problem.** A structurally valid specification and a green build can coexist
with code that has no traceable connection to the claims it purports to deliver.
Reviewers must reconstruct that connection from names, prose and memory, and
discovering an obsolete reference depends on someone noticing it.

**The solved state.** A maintainer can follow a claim to the scenarios, tests and
implementation bound to it, and follow a code declaration back to its Feature,
claim or architectural decision. A broken reference or missing required link
is observable in the build instead of remaining an unverified assertion.

| Persona                          | Standing  | Goal                                                                                           |
| -------------------------------- | --------- | ---------------------------------------------------------------------------------------------- |
| A developer implementing a claim | Primary   | Bind the implementation to the agreement and receive a diagnostic when that reference breaks   |
| A specification author           | Primary   | Bind executable scenarios to the same claims that the implementation cites                     |
| A reviewer                       | Primary   | Inspect the evidence connecting the agreement, its tests, its implementation and relevant ADRs |
| A coding agent                   | Secondary | Repair a missing or unresolved binding from a precise diagnostic and the referenced record     |

## Success criteria

These are epic outcomes, not numbered Feature claims. Their exact contracts
belong in the Feature specifications produced during decomposition.

| ID  | Criterion                                                                                                                                        | Tier | Notes                                                                                                                                                                |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------ | ---- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| R1  | Code can declare bindings to individual claims, Feature specifications and ADRs using references that resolve to repository records              | Must | Declaration syntax, supported code elements and ADR qualification are open.                                                                                          |
| R2  | Roslyn analysis reports an invalid or unresolved code binding at the declaration that contains it                                                | Must | A reference that resolves to more than one record is not silently accepted.                                                                                          |
| R3  | Executable specification scenarios can bind to the same Feature and claim identities as code                                                     | Must | Here, executable specifications are Gherkin `.feature` files; a Feature specification is their companion agreement. Existing local `@B-00n` tags must be considered. |
| R4  | Scenario bindings are checked against the specification records rather than inferred from scenario titles                                        | Must | Markdown and Gherkin validation must have an explicit owner; Roslyn need not own it.                                                                                 |
| R5  | For the declared analysis scope, reconciliation exposes each claim's bound scenarios, tests and implementation, including missing required links | Must | The coverage policy is open. An incomplete project set cannot be presented as complete repository coverage.                                                          |
| R6  | A code binding to an ADR resolves the intended decision even when different Features have ADRs with the same local number                        | Must | The lifecycle policy for proposed, superseded and rejected records is open.                                                                                          |
| R7  | A maintainer can distinguish a valid reference, complete required bindings and passing behavioral evidence                                       | Must | A tag does not prove that an implementation meets a claim or obeys an ADR.                                                                                           |
| R8  | The checks are deterministic, offline and read-only, and diagnostics identify the offending source location and target reference                 | Must | Paths exposed by product diagnostics and reports are repository-relative.                                                                                            |
| R9  | A consumer can adopt the analysis with an explicit scope and enforcement policy, with the same binding rules used in the editor and build        | Must | Installation, configuration, severity defaults and incremental adoption are open.                                                                                    |

## Candidate Features

**A candidate cut for grooming.** No candidate has a Feature id or a place in
`children`; a child is added only when its co-located specification exists.

| Candidate | Name                        | Invariant it would carry                                                                                                           | Serves     |
| --------- | --------------------------- | ---------------------------------------------------------------------------------------------------------------------------------- | ---------- |
| A         | Binding contract            | A declared target has one kind, one qualified identity and one resolution meaning across code, scenarios and specification records | R1, R3, R6 |
| B         | Roslyn binding diagnostics  | Each code binding resolves under the agreed contract or produces a precise source diagnostic                                       | R2, R6, R8 |
| C         | Scenario binding validation | Each scenario's Feature and claim bindings resolve under the same contract used for code                                           | R3, R4, R8 |
| D         | Traceability reconciliation | The declared analysis scope yields an inspectable chain of claim, scenario, test and implementation bindings with explicit gaps    | R5, R7, R8 |
| E         | Consumer adoption           | The consumer's declared scope and policy determine where binding enforcement applies in editor and build                           | R9         |

The cuts separate failure modes: a reference contract can be sound while a
Roslyn diagnostic points at the wrong declaration; scenario bindings can resolve
while implementation coverage is incomplete; and adoption policy can select the
wrong scope while every binding within it is valid. Candidate D owns completeness
across the selected inputs, rather than assuming one compilation sees the whole
repository. Candidate C must reuse or extend existing scenario validation rather
than establish a competing interpretation of claim tags.

No candidate name joins separate capabilities with `&` or a coordinating
conjunction. Packaging and presentation are not implicitly part of the binding
contract.

## Constraints

| Constraint                                                                                    | Source                                                                    | What it rules out                                                                                   |
| --------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------- |
| The specification remains the artifact of record                                              | AGENTS.md § Specification-Driven Development                              | Code annotations creating claims or silently amending agreements                                    |
| Claim, Feature and ADR identities remain stable and qualified by their owning scope           | AGENTS.md § Stable IDs; `spec-and-traceability` § Claims and traceability | Resolving a bare local claim or ADR number by whichever file is discovered first                    |
| The repository-wide specification check remains usable without compiling consumer code        | Brief § 5; `0001-F2` decision 0001                                        | Replacing the existing check with a Roslyn-only implementation                                      |
| Checks are deterministic, offline and read-only                                               | AGENTS.md § Invariants                                                    | Fetching specification records, querying GitHub or rewriting code or specifications during analysis |
| Paths in emitted diagnostics and reports are relative to the declared repository root         | AGENTS.md § Invariants; brief § 9                                         | Leaking machine-specific paths into the traceability evidence                                       |
| A binding is a declaration of traceability, not proof of behavior or architectural compliance | AGENTS.md § The artifact chain                                            | Treating a resolving annotation as a passing test or review sign-off                                |
| Rule identifiers and configuration must respect the existing schema-version contract          | `0001-F7` C-11; AGENTS.md § Invariants                                    | Quietly adding analyzer diagnostics to an existing pinned `SPEC###` vocabulary                      |

## Boundaries with existing specifications

The owner has requested a new capability beyond the original extraction scope.
The old records are identified here so that decomposition carries their
amendments to the owning specifications; this epic does not retrospectively
change what the extraction delivered.

| Record                                                          | Existing boundary                                                                             | Effect of this epic                                                                                                                                                                         |
| --------------------------------------------------------------- | --------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Brief § 2; `0001-F1` § 5 row 9; epic `0001` § Out of this epic  | Anything Roslyn stays in a consumer repository                                                | A new epic owns reusable code bindings here; the extraction and repository-wide checker remain separate. The blanket exclusion needs narrowing when the new Feature agreements are written. |
| `0001-F2` decision 0001 § Rejected                              | Roslyn was rejected as the host for repository-wide `SPEC###` checks                          | That rejection still holds. Code binding analysis is a separate responsibility; any changed ownership of the consumer's claim bridge needs an explicit decision.                            |
| `specht-conventions` § Questions; `dotnet-tool` § Project setup | The companion says there is no Roslyn scope and project setup marks analyzers as inapplicable | Update those scope statements alongside the agreed Feature boundaries, before implementation relies on them.                                                                                |
| `0001-F1`, `0001-F5`, `0001-F6`                                 | Existing claim-tag resolution, identity grammars and specification discovery                  | Candidate A and C must reconcile with these contracts rather than introduce independent grammars or discovery rules.                                                                        |
| `0001-F7`                                                       | Pinned schema versions own their rule vocabulary                                              | Decide analyzer diagnostic versioning and its relationship to schema pins before assigning rule ids.                                                                                        |

## Open questions

None of these choices is settled by the epic. The answers belong in the Feature
agreement that owns them, with decisions recording rejected alternatives and
their costs.

| OQ    | Question                                                                                                                                                                                                                                  | Who answers       |
| ----- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------- |
| OQ-1  | Is `spec.code` the public product or package name, or a working name for a `specht` capability? How is the analyzer installed and versioned relative to the tool?                                                                         | Owner             |
| OQ-2  | How are code bindings declared: XML documentation comments as illustrated above, attributes, other structured comments or another form? Which languages and code elements are supported, and can one declaration bind to several targets? | Owner with design |
| OQ-3  | What is the common reference form for Feature specifications, claims and repository- or Feature-scoped ADRs, including repositories without an epic tier? Which additional record kinds does "other such things" include?                 | Owner             |
| OQ-4  | How do Gherkin Feature-level and Scenario-level bindings interact with existing local claim tags? Are bindings inherited, repeated or qualified explicitly?                                                                               | Owner             |
| OQ-5  | Which claims require implementation, scenario and test bindings, and which code requires a claim binding? How are shared infrastructure, generated code, test code and exemptions treated?                                                | Owner             |
| OQ-6  | How are scenario-to-test bindings established, and which inputs and aggregation boundary prove completeness across projects and compilations?                                                                                             | Design after OQ-5 |
| OQ-7  | Which document and ADR lifecycle states are valid binding targets, and what happens when a referenced claim is withdrawn or an ADR is superseded?                                                                                         | Owner             |
| OQ-8  | How are the pinned manifest and specification records supplied to Roslyn analysis, and how are missing or invalid inputs diagnosed consistently in editor and build?                                                                      | Design            |
| OQ-9  | Which diagnostic namespace, severities, suppression rules and versioning policy apply to analyzer findings and reconciliation gaps?                                                                                                       | Owner with design |
| OQ-10 | Does adoption reuse or extract the consumer's existing claim bridge, and which repository retains ownership of its convention analyzers?                                                                                                  | Owner             |
| OQ-11 | Where is the reconciled evidence presented, and does it extend the existing report contract or have its own surface?                                                                                                                      | Owner with design |
| OQ-12 | How does a member’s requirement reference obtain its specification scope: from an enclosing type’s `<spec>` binding, an explicit member binding or another rule?                                                                          | Owner with design |

## Out of this epic

| Item                                                                            | Boundary                                                                                                        |
| ------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------- |
| Proving arbitrary code behavior or architectural conformance from an annotation | Tests and review remain the evidence; this epic checks bindings and their completeness under a declared policy. |
| Generic coding-convention analyzers                                             | No transfer of the consumer's convention analyzer scope is implied; OQ-10 addresses ownership.                  |
| Replacing the Markdown specification checker                                    | Epic `0001` continues to own repository-wide specification validation.                                          |
| Creating claims or auto-approving specifications from code                      | The specification-first artifact chain remains authoritative.                                                   |
| Code fixes that invent references or edit specification records                 | No such writing capability is requested.                                                                        |
| Runtime enforcement or remote traceability services                             | The requested capability is static, deterministic and offline.                                                  |
| Production code, tests and delivery work items in this draft                    | Each agreed Feature starts with its specification and scenarios before work items are cut.                      |

## Placement and next step

This epic lives at `epics/0201-spec-code-analysis/epic.md`, where the existing
epic discovery glob finds it. Feature specifications will live beside the areas
they specify; analyzer and aggregation project placement is not decided here.

The epic is `deferred` at the owner's request on 2026-10-13: this is a later
problem. When it is taken up, groom the candidate boundaries and binding policy,
then author each agreed Feature's specification and companion scenarios. Open
questions must be resolved before implementation depends on their answers.
