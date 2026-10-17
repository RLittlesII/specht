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
updated: "2026-10-17"
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
This is an epic draft: no Feature is agreed and no analyzer is delivered. The
candidate cut and open questions guide decomposition; the executable binding
comparison lives in the [spike investigation](spikes/0236/README.md).

### Binding approaches under evaluation

On 2026-10-17 the owner requested that the two XML documentation approaches be
compared for consumer value, then added a third combining their strengths. No
production contract is selected. The comparison
separates three choices: how a declaration obtains its specification scope,
whether it references a path or a stable identity, and whether the binding says
that code implements or tests a claim. Those choices need not travel together.

#### Approach A: specification scope on the type

The original illustration declares a specification location on a type and a
requirement on a member:

```csharp
/// <spec path="/.spec/features/Authentication" />
public sealed class Authentication
{
    /// <requirement>@REQ-AU-054</requirement>
    public void Foo() {}
}
```

Its value is low repetition when a type's members belong to one specification.
The cost is contextual resolution: moving a member to another type can change
its target even when the member's comment is unchanged. OQ-12 must settle nested
types, partial declarations, missing scope and any member override. The leading
slash is unresolved example syntax, not permission to accept filesystem-absolute
paths; directory-form resolution and the meaning of `@` also remain open.

Applied to the same test illustrated in Approach B, this shape would be:

```csharp
/// <spec path="src/specht/.spec/README.md" />
public sealed class TraceabilityTests
{
    /// <summary>Reports a claim missing its traceability row.</summary>
    /// <requirement>B-010</requirement>
    [Fact]
    public void MissingTraceabilityRowProducesViolation() {}
}
```

Here the path selects `0001-F1`; the test role would need to come from test
recognition or an additional binding rule. The relative file path and unadorned
claim are comparison choices, not resolutions of the original example's syntax.

#### Approach B: explicit binding on the member

A test declares the Feature and claim together, with a tag expressing its role:

```csharp
/// <summary>Reports a claim missing its traceability row.</summary>
/// <verifies feature="0001-F1" claim="B-010" />
[Fact]
public void MissingTraceabilityRowProducesViolation() {}
```

Its value is that the relationship can be read and preserved with the method,
including in a test class serving several Features. The cost is repeated scope
on every member. `0001-F1` illustrates a qualified Feature identity; its encoding
must respect the consumer's optional epic tier and configured identity forms.
`verifies` describes a declared test relationship, not proof that the claim is
satisfied. Implementation and ADR bindings still need their own agreed meaning.

#### Approach C: type defaults with explicit member bindings

A type supplies a stable Feature identity. A member can use that default or
state a complete identity when it verifies another Feature:

```csharp
/// <spec feature="0001-F1" />
public sealed class TraceabilityTests
{
    /// <verifies claim="B-010" />
    [Fact]
    public void MissingTraceabilityRowProducesViolation() {}

    /// <verifies feature="0001-F2" claim="B-001" />
    [Fact]
    public void AnotherFeatureBehavior() {}
}
```

This combines A's reduced scope repetition with B's stable references and
member-level overrides. It adds precedence and conflict rules, and an inherited
member can still retarget when moved. The spike also evaluates using the type
only as an authoring default: a code fix writes explicit IDs on the member and
an analyzer requires those persisted IDs before the binding counts as evidence.
That refinement retains code-fix context while making checked bindings portable.

All three examples are candidate contracts, not production declarations or
coverage evidence. Path versus ID references, scope placement and role tags
remain separate choices. The spike uses separate local Feature and optional
epic fields to test custom grammars without assuming a qualification delimiter;
that encoding does not resolve OQ-3.

#### Shared requirements for all approaches

- Consumer grammars come from `identifiers.feature` and `identifiers.claim` in
  the pinned manifest, using the existing resolution rules (`0001-F5` C-2).
  A local Feature grammar and its qualified reference form are distinct; do not
  apply a local `F1` grammar directly to `0001-F1`. Frontmatter schema patterns
  remain the consumer's copy to keep aligned with the manifest.
- Format validation and record resolution are separate checks. A valid-looking
  ID must resolve to one declared record in its owning scope. Code fixes select
  existing records; a regex cannot identify which claim a method tests.
- The analyzer diagnoses without writing. A separately invoked code fix can
  insert or repair XML bindings while preserving existing documentation. An
  automatic selection needs one authoritative target; ambiguous candidates need
  separate choices. Generating claim prose, inventing IDs and editing the
  specification are outside that repair. Source generation adds source; a code
  fix edits the existing method's documentation.
- Inputs must be explicit and consistent in editor and build. Supplying the
  manifest, specifications and Gherkin as Roslyn additional files is a candidate
  for OQ-8, not an agreed input or packaging contract.
- A Gherkin claim tag, a generated scenario test and a handwritten step method
  are separate records. Shared steps can participate in several scenarios;
  documenting one step is insufficient to establish the scenario-to-test link.
- Reconciliation must declare its project scope and expose missing inputs.
  A single compilation cannot establish complete coverage across omitted test
  projects. Resolved bindings, complete required links and passing test results
  remain distinct evidence (R5, R7).

#### Value comparison

These are design expectations to test, not measured findings. Judge value by
the trustworthiness of the traceability chain, the effort to maintain it and
the clarity of diagnostics and repairs. Shorter XML alone is insufficient.

| Consumer concern                 | Approach A: type scope                                  | Approach B: explicit member                        | Approach C: hybrid                                               |
| -------------------------------- | ------------------------------------------------------- | -------------------------------------------------- | ---------------------------------------------------------------- |
| One type implements one Feature  | Scope declared once                                     | Repeats scope per bound member                     | Default declared once; materialization repeats persisted scope   |
| Mixed-Feature test class         | Needs separate types or an additional override contract | Each member names its target                       | Explicit members override the default                            |
| Moving a member                  | Inherited target can change silently                    | Declared identity travels with the member          | Inherited target can change; materialized identity travels       |
| Moving a specification           | Illustrated path needs repair                           | Stable identity survives discovery at the new path | Stable identity survives discovery at the new path               |
| Code fixes                       | Type scope narrows local claim choices                  | Choices need an explicit target                    | Type default narrows choices; fixes can persist explicit targets |
| Consumer-defined ID formats      | Common grammar and resolver                             | Same grammar and resolver                          | Same grammar and resolver                                        |
| Coverage and behavioral evidence | Reconciliation and tests required                       | Same evidence required                             | Same evidence required                                           |
| Extra contract rules             | Containing scope and partial/nested types               | Explicit qualification                             | Defaults, overrides, conflicts and materialization policy        |

Path stability is an identity-choice benefit, not an intrinsic benefit of
member placement. Role clarity can also be added to either approach. Neither
XML shape establishes a scenario link or proves behavior on its own.

#### Evaluation before selection

Use the same synthetic specifications, claims and code for all three candidates:

| Case                                                                                | Evidence to collect                                                                                             |
| ----------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------- |
| Cohesive implementation type and mixed-Feature test class                           | Bindings and scope declarations a maintainer must author; errors after adding a method                          |
| Move a member between types whose Features both declare `B-001`                     | Whether its intended target is preserved; whether any silent retargeting is detectable from the declared inputs |
| Move a specification without changing its identity                                  | References requiring edits and the precision of offered repairs                                                 |
| Custom Feature/claim grammars, with and without epics, and repeated local claim IDs | Consistent format validation, qualification and unambiguous resolution                                          |
| Missing, malformed and ambiguous bindings; one method verifies several claims       | Diagnostic location, preserved documentation, available fixes and unsafe automatic selections avoided           |
| Shared Reqnroll steps, scenario outlines and an omitted test project                | Trace from each tagged scenario to its discovered tests, with unresolved links and incomplete scope visible     |

Compare diagnostic correctness first, then maintenance edits, reading effort
and implementation complexity. Record actual observations and limitations;
do not assign numerical value scores without evidence. Exercise all three candidates
against identical cases before authoring the binding agreement.

The [spike investigation](spikes/0236/README.md) records executable evidence,
limitations and a proposed selection. It informs OQ-2, OQ-3, OQ-6, OQ-8 and
OQ-12; those questions remain open until the owning Feature agreements adopt
their answers.

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

## Analyzer testing direction

Owner direction, 2026-10-17: use
`Rocket.Surgery.Extensions.Testing.SourceGenerators` for the production analyzer
and generator test harness, following the direct-assertion tests in
[Airframe's Rsa1001Tests](https://github.com/RocketSurgeonsGuild/Airframe/blob/main/test/Analyzers.Tests/Diagnostics/Usage/Rsa1001Tests.cs).
The demonstrated flow is `GeneratorTestContextBuilder.Create()`, `AddSources`,
`WithAnalyzer<TAnalyzer>()` and `GenerateAsync()`, followed by inspection of
`AnalyzerResults`.

Use direct assertions with the repository's assertion library on diagnostic
counts, IDs, severities and source spans, plus the absence of unexpected
diagnostics. Negative cases must assert that diagnostics actually exist; an
`All` predicate over an empty collection is insufficient. Code-fix tests must
assert the resulting source and reanalyze it; generator tests must assert the
relevant generated source and compilation result directly.

Do not adopt the example's `GivenSource_WhenAnalyze_ThenVerify` test,
`Verifier.Verify` calls, Verify/Verify.Xunit package references or Verify snapshot
files for this capability. The test-context builder and diagnostic results are
the reuse point; snapshot verification is excluded. The standalone binding
spike is mechanical evidence, not the production test-harness choice.

## Candidate Features

**A candidate cut for grooming.** No candidate has a Feature id or a place in
`children`; a child is added only when its co-located specification exists.

| Candidate | Name                        | Invariant it would carry                                                                                                                    | Serves     |
| --------- | --------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------- | ---------- |
| A         | Binding contract            | A declared target has one kind, one qualified identity and one resolution meaning across code, scenarios and specification records          | R1, R3, R6 |
| B         | Roslyn binding diagnostics  | Each code binding resolves under the agreed contract or produces a precise source diagnostic; invoked code fixes repair only known bindings | R2, R6, R8 |
| C         | Scenario binding validation | Each scenario's Feature and claim bindings resolve under the same contract used for code                                                    | R3, R4, R8 |
| D         | Traceability reconciliation | The declared analysis scope yields an inspectable chain of claim, scenario, test and implementation bindings with explicit gaps             | R5, R7, R8 |
| E         | Consumer adoption           | The consumer's declared scope and policy determine where binding enforcement applies in editor and build                                    | R9         |

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

| OQ    | Question                                                                                                                                                                                                                                                                                                                                                | Who answers       |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------- |
| OQ-1  | Is `spec.code` the public product or package name, or a working name for a `specht` capability? How is the analyzer installed and versioned relative to the tool?                                                                                                                                                                                       | Owner             |
| OQ-2  | How are code bindings declared: type-scoped XML (Approach A), explicit member XML (Approach B), hybrid defaults (Approach C), attributes or another form? Evaluate the cases above before selecting. Which languages, declaration kinds, binding roles and multiple-target forms are supported, and which repairs can an invoked code fix safely offer? | Owner with design |
| OQ-3  | What is the common reference form for Feature specifications, claims and repository- or Feature-scoped ADRs, including repositories without an epic tier? Which additional record kinds does "other such things" include?                                                                                                                               | Owner             |
| OQ-4  | How do Gherkin Feature-level and Scenario-level bindings interact with existing local claim tags? Are bindings inherited, repeated or qualified explicitly?                                                                                                                                                                                             | Owner             |
| OQ-5  | Which claims require implementation, scenario and test bindings, and which code requires a claim binding? How are shared infrastructure, generated code, test code and exemptions treated?                                                                                                                                                              | Owner             |
| OQ-6  | How are scenario-to-test bindings established, and which inputs and aggregation boundary prove completeness across projects and compilations?                                                                                                                                                                                                           | Design after OQ-5 |
| OQ-7  | Which document and ADR lifecycle states are valid binding targets, and what happens when a referenced claim is withdrawn or an ADR is superseded?                                                                                                                                                                                                       | Owner             |
| OQ-8  | How are the pinned manifest and specification records supplied to Roslyn analysis, and how are missing or invalid inputs diagnosed consistently in editor and build?                                                                                                                                                                                    | Design            |
| OQ-9  | Which diagnostic namespace, severities, suppression rules and versioning policy apply to analyzer findings and reconciliation gaps?                                                                                                                                                                                                                     | Owner with design |
| OQ-10 | Does adoption reuse or extract the consumer's existing claim bridge, and which repository retains ownership of its convention analyzers?                                                                                                                                                                                                                | Owner             |
| OQ-11 | Where is the reconciled evidence presented, and does it extend the existing report contract or have its own surface?                                                                                                                                                                                                                                    | Owner with design |
| OQ-12 | How does a member obtain its specification scope: an enclosing type's binding, an explicit member binding or another rule? If both are supported, what are the precedence, conflict and move semantics, including nested and partial types?                                                                                                             | Owner with design |

## Out of this epic

| Item                                                                            | Boundary                                                                                                                                               |
| ------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Proving arbitrary code behavior or architectural conformance from an annotation | Tests and review remain the evidence; this epic checks bindings and their completeness under a declared policy.                                        |
| Generic coding-convention analyzers                                             | No transfer of the consumer's convention analyzer scope is implied; OQ-10 addresses ownership.                                                         |
| Replacing the Markdown specification checker                                    | Epic `0001` continues to own repository-wide specification validation.                                                                                 |
| Creating claims or auto-approving specifications from code                      | The specification-first artifact chain remains authoritative.                                                                                          |
| Code fixes that invent references or edit specification records                 | Candidate repairs insert or amend code bindings to known records only; the selection and safe-repair contract remain open in OQ-2.                     |
| Runtime enforcement or remote traceability services                             | The requested capability is static, deterministic and offline.                                                                                         |
| Production analyzer delivery from this draft                                    | The isolated binding spike supplies evidence; each production Feature starts with an agreed specification and scenarios before delivery items are cut. |

## Placement and next step

This epic lives at `epics/0201-spec-code-analysis/epic.md`, where the existing
epic discovery glob finds it. Feature specifications will live beside the areas
they specify; analyzer and aggregation project placement is not decided here.

The epic is `deferred` at the owner's request on 2026-10-13: this is a later
problem. When it is taken up, groom the candidate boundaries and binding policy,
then author each agreed Feature's specification and companion scenarios. Open
questions must be resolved before implementation depends on their answers. The
binding investigation does not lift the production deferral.
