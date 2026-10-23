---
title: "Epic 0201: spec.code analysis"
description: "Roslyn analyzers and scenario bindings connect implementation and executable specifications to claims, Feature specifications and ADRs, with explicit evidence of unresolved references and coverage gaps"
id: "0201"
type: epic
status: deferred
priority: med
milestone: null
children:
  ["0201-F1", "0201-F2", "0201-F3", "0201-F4", "0201-F5", "0201-F6", "0201-F7"]
created: "2026-10-13"
updated: "2026-10-23"
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
The owner selected the hardened hybrid on 2026-10-17. Seven draft Feature
agreements now decompose it; no production analyzer is delivered. The remaining
public-contract questions still block implementation; the executable binding
comparison lives in the [spike investigation](spikes/0236/README.md).

### Binding decision: hardened hybrid

On 2026-10-17 the owner requested that the two XML documentation approaches be
compared for consumer value, then added a third combining their strengths. No
production syntax was selected by the initial comparison. The owner has now
adopted the hardened hybrid described below. The comparison
separates three choices: how a declaration obtains its specification scope,
whether it references a path or a stable identity, and whether the binding says
that code implements or tests a claim. Those choices need not travel together.

#### Adopted model, 2026-10-17

Use type-level defaults for authoring and require explicit member identities for
checked traceability. A code fix uses the default to narrow existing claim
choices and writes the full target on the member. An explicit member identity
wins in full; it never borrows missing identity fields from an enclosing type.
Inherited-only bindings produce a diagnostic and are not checked evidence.
Moving an explicitly bound method preserves its target. Missing or ambiguous
context cannot silently select a claim. This adopts the semantics of hardened
Approach C, not the spike's illustrative XML encoding or diagnostic IDs.

Enforce the relationship from **both ends**. Code analysis reports selected
methods with missing, invalid or ambiguous bindings and offers invoked repairs.
Reconciliation enumerates the specification's declared claims, including claims
that no method names, and reports missing evidence under the selected policy.
A consumer need not remember every annotation to discover a gap. A fix can
supply known IDs after target selection; it cannot infer assertion adequacy or
choose intent from a method name. Generating new test skeletons is not adopted
by this decision; it is a possible later authoring extension.

Keep the binding model framework-independent. Reqnroll is optional: direct
traceability is claim → explicit method binding → discovered test case → result.
xUnit facts and theories and Playwright .NET tests must have value without any
Reqnroll package or Gherkin input. Framework adapters supply discovery and result
correlation; they do not change identity grammar or make XML runtime metadata.
The optional Reqnroll bridge augments that chain with tagged scenario and
outline-row identity. It must not add a mandatory dependency to the core model.
This product choice does not change this repository's Gherkin-first development
policy.

Owner decision, 2026-10-17: canonical claim references use separate `epic`,
`feature` and `claim` fields, with `epic` optional. `feature` carries the local
Feature ID, validated by the consumer's grammar; it is not combined with the
epic using a fixed delimiter. [0201-F1 decision 0002](../../src/analyzers/Bindings/.spec/decisions/0002-separate-claim-identity-fields.md)
records the choice and alternative. XML role names are defined by 0201-F2 decision 0002; the comparison examples below are historical candidates.

Owner decision, 2026-10-17: ADR references carry `adr` and select repository
ownership when `feature` is absent, or the explicitly named Feature when it is
present; `epic` remains optional. They never inherit a type default or search
another owner when the selected target is missing.
[0201-F1 decision 0003](../../src/analyzers/Bindings/.spec/decisions/0003-explicit-adr-ownership.md)
records the ownership rule. Additional record kinds remain open; target
lifecycle is defined by decision 0004 below.

Owner decision, 2026-10-17: current claims in draft, in-review or approved
Features and accepted ADRs are eligible targets for current evidence. Withdrawn
or Superseded claims, claims in superseded Features, and proposed, rejected,
deprecated or superseded ADRs retain resolving historical links but do not count
toward current evidence. Reference resolution and lifecycle eligibility remain
separate. [0201-F1 decision 0004](../../src/analyzers/Bindings/.spec/decisions/0004-target-lifecycle-eligibility.md)
defines the policy; 0201-F6 decision 0001 below defines target diagnostic severity.

Owner decision, 2026-10-17: use `spec` for type-level authoring defaults,
`verifies` for declared test-to-claim relationships, `implements` for declared
implementation-to-claim relationships, and `follows` for declared code-to-ADR
relationships. [0201-F2 decision 0002](../../src/codefixes/.spec/decisions/0002-xml-binding-roles.md)
records the four roles. Defaults do not count as checked evidence; declarations
do not prove behavior or architectural compliance.

Owner decision, 2026-10-17: support CSharp XML-documentable types and members.
`spec` applies to types, `verifies` to recognized test methods, and `implements`
and `follows` to types and documentable members, including constructors,
methods, properties, indexers, fields and events. An explicit type binding
applies only to that type; it never binds its members implicitly.
[0201-F2 decision 0003](../../src/codefixes/.spec/decisions/0003-csharp-declaration-scope.md)
records the scope and alternatives. Remaining XML forms and safe repairs stay
open in OQ-2.

Owner decision, 2026-10-17: authoring defaults use the nearest enclosing type
with a `spec` default; an inner default replaces the whole outer default.
Partial declarations share one default when single or identical; conflicting
defaults produce a diagnostic and block automatic materialization. Base classes
do not supply defaults. Explicit checked bindings remain independent of this
context. [0201-F2 decision 0004](../../src/codefixes/.spec/decisions/0004-nested-partial-type-defaults.md)
records the nested and partial rules, resolving OQ-12.

Owner decision, 2026-10-17: represent multiple relationships by repeating
`verifies`, `implements` or `follows`, with one complete target per element.
Each relationship carries its own identity; identity attributes are scalar IDs
under consumer grammars and are never split into target lists.
[0201-F2 decision 0005](../../src/codefixes/.spec/decisions/0005-repeated-single-target-elements.md)
records the form and alternatives. Strict XML validation is defined below;
duplicate handling is defined by decision 0007 below; code-fix safety is defined by decision 0008 below.

Owner decision, 2026-10-17: strictly validate the four reserved XML elements.
Require each role's defined attributes, empty content and supported declaration
placement; diagnose missing or unknown attributes, body text, nested elements,
malformed XML and unsupported placements. Invalid relationships supply no
checked evidence; invalid defaults supply no authoring context. Preserve
unrelated documentation, including other custom tags.
[0201-F2 decision 0006](../../src/codefixes/.spec/decisions/0006-strict-xml-validation.md)
records the contract. Duplicate handling and code-fix safety are defined below,
as are severity defaults in 0201-F6 decision 0001.

Owner decision, 2026-10-17: valid relationship elements with the same code
symbol, role and resolved target count as one relationship and produce a
duplicate diagnostic. Offer an invoked fix that removes redundant copies while
preserving one complete binding. Different roles and different symbols remain
separate relationships; identical partial-type authoring defaults retain their
existing policy.
[0201-F2 decision 0007](../../src/codefixes/.spec/decisions/0007-duplicate-relationships.md)
records the duplicate policy. Code-fix safety and severity defaults are defined
below.

Owner decision, 2026-10-17: permit invoked unambiguous default materialization
and valid duplicate removal. Require explicit selection of an existing record
when a repair needs a target choice or replacement. Do not guess ID or attribute
corrections, or delete unknown attributes or body content merely to make XML
valid. Fix All is limited to supported deterministic repairs with independently
satisfied preconditions; it must never propagate one selected target across
unrelated declarations.
[0201-F2 decision 0008](../../src/codefixes/.spec/decisions/0008-code-fix-safety.md)
records the repair contract, resolving OQ-2. Diagnostic/version and enforcement
policies remain in their existing open questions.

Owner decision, 2026-10-17: use `SPCT####` analyzer diagnostics, starting with
`SPCT0001`. Published IDs remain stable and retired IDs are never reused.
Analyzer rules follow analyzer package releases; manifest/schema contract
changes still follow schema-version rules, and analyzer-only additions leave
the checker's pinned `SPEC` vocabulary unchanged.
[0201-F2 decision 0009](../../src/codefixes/.spec/decisions/0009-diagnostic-identity-versioning.md)
records the identity/versioning policy. Severity defaults are defined below;
standard reporting controls are defined below, while the package's version
relationship to the tool remains open.

Owner decision, 2026-10-17: default binding defects, unresolved or ambiguous and
ineligible targets, conflicting defaults, and missing required bindings within
the selected enforcement scope to warning. Default valid duplicate relationships
to information. Consumers can adjust severity per rule, including promotion to
error; severity does not change evidence validity.
[0201-F6 decision 0001](../../src/codeanalysis/.spec/decisions/0001-diagnostic-severity-defaults.md)
records the defaults. Standard reporting controls are defined below; exclusions,
domain/input configuration and the analysis-input contract remain open.

Owner decision, 2026-10-17: honor standard Roslyn diagnostic controls, including
`.editorconfig`, `#pragma warning` and `SuppressMessage`, with no custom
suppression XML or parallel suppression system. Suppression changes diagnostic
visibility only; it never changes evidence validity or eligibility, satisfies a
missing required link, or changes declared analysis scope.
[0201-F6 decision 0002](../../src/codeanalysis/.spec/decisions/0002-standard-roslyn-suppression.md)
records the policy. Coverage and enforcement are defined below; domain/input
configuration remains open.

Owner decision, 2026-10-17: every non-exempt current claim in selected Feature
scope requires a valid test binding. Implementation and scenario links are
optional unless explicitly required. Validate explicit bindings in analyzed
projects; require missing authored bindings only in explicitly selected
enforcement scopes. Generated code requires no authored bindings, and shared
infrastructure has no blanket requirement to bind every method. Exemptions
require explicit reasons and remain visible as exempt, never covered.
[0201-F6 decision 0003](../../src/codeanalysis/.spec/decisions/0003-coverage-policy.md)
records the policy, resolving OQ-5. Configuration encoding and analysis input
contracts remain open.

Input-data requirement clarified by the owner, 2026-10-17: bindings must resolve
against the contents of the consumer's actual specification and ADR records,
including their declared identities, ownership and lifecycle values. Schemas
alone cannot supply that record inventory. Any chosen input-delivery mechanism
must read those consumer-owned records; the AdditionalFiles proposal means
supplying their actual contents, not embedding records with the analyzer.
Owner decision, 2026-10-17: build integration discovers those consumer records
and supplies their contents, the pinned manifest and configured schemas through
Roslyn AdditionalFiles, consistently in editor and build. The analyzer reads and
parses the supplied data, stays offline and does no repository crawling.
Missing, invalid or incompatible required inputs produce error diagnostics by
default and unavailable/incomplete analysis. Changed source/specification/configuration
inputs invalidate affected analysis. Completeness requires current inputs for
every declared project/target-framework combination. Discovery/results must
match source, assembly and configuration identities; stale/mismatched artifacts
remain visibly stale and cannot establish current discovery/execution, while
binding validity stays separate and absent results never count as passing.
[0201-F6 decision 0004](../../src/codeanalysis/.spec/decisions/0004-consumer-inputs-completeness.md)
records the input/completeness contract. Detailed discovery configuration,
evidence encoding, adapters and report surface remain with their owning Features.

Owner decision, 2026-10-17: use a common framework-independent .NET runner-evidence
contract and extensible collection adapters, rather than a framework-name or
version allowlist. xUnit, NUnit, MSTest and Playwright .NET are validation cases.
Missing integration/metadata remains unavailable or unresolved evidence. Runtime-only
cases remain unenumerated until supplied by the runner. Versioned JSON carries
runner case IDs, method metadata, project/framework identity, source/assembly/configuration
fingerprints, adapter version and run identity. Collect through normal test
workflows; analysis/reconciliation does not launch tests.
[0201-F3 decision 0001](../../src/analyzers/TestEvidence/.spec/decisions/0001-framework-independent-runner-evidence.md)
records the corrected framework scope and evidence contract.

Owner decision, 2026-10-17; command revised 2026-10-19: expose reconciliation through `specht evidence` with a
human summary and separate versioned JSON contract; preserve the existing checker
report contract. [0201-F5 decision 0001](../../src/analyzers/Reconciliation/.spec/decisions/0001-trace-report-surface.md)
records the surface, resolving OQ-11's product choice. Detailed evidence/report
schemas, adapter integrations and CLI behavior remain technical design.

Owner decision, 2026-10-17: checked targets are claims and ADRs; Feature and epic
components establish scope, with other record kinds deferred.
[0201-F1 decision 0005](../../src/analyzers/Bindings/.spec/decisions/0005-checked-target-boundaries.md)
records the target boundary. Gherkin Feature scope supplies the default; an
explicit Scenario/Outline scope replaces it completely without merging fields.
Local claim tags resolve in effective scope. Map scenarios to cases using
generator/runner metadata through the common evidence contract, reusing the
consumer claim bridge while its convention analyzers remain consumer-owned.
Reqnroll remains optional.
[0201-F4 decision 0001](../../src/analyzers/Scenarios/.spec/decisions/0001-scenario-scope-bridge-ownership.md)
records the scope and bridge policies. Exact tag/configuration encoding and
metadata extraction remain technical design.

Owner decision, 2026-10-17: deploy `Specht.CodeAnalysis` privately through NuGet,
aligned with the repository/tool release version while checking interoperability
through schema/evidence contracts. Owner correction, 2026-10-18: consume the
consumer repository's supplied schemas, versions and existing ID grammars,
including bring-your-own schemas. Knowledge of Specht `0.2.0` does not require a
consumer at `0.1.0` to upgrade or identify its schema as Specht-owned. Analyzer
adoption adds a `codeAnalysis` manifest section without introducing schema `0.3.0`
or changing the consumer's pin. Scope, required links, exemptions and record
discovery remain explicit policy;
explicit mappings read it from existing consumer configuration (F6 OQ-1). MSBuild owns
root/input delivery, and Roslyn owns severity/suppression. [0201-F6 decision 0005](../../src/codeanalysis/.spec/decisions/0005-deployment-configuration.md)
records the call. [ADR-0017](../../.spec/adr/0017-compiler-loaded-components-target-netstandard.md)
records the approved `netstandard2.0` exception for compiler-loaded analyzer and
code-fix components; CLI, collector and test targets retain `net10.0;net11.0`.

The [decision record](../../src/analyzers/Bindings/.spec/decisions/0001-hardened-hybrid.md)
records alternatives and costs. The [extended spike](spikes/0236/README.md)
validates the non-Reqnroll cases; its results remain experimental evidence.

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
its target even when the member's comment is unchanged. The adopted hybrid
avoids that checked-evidence risk through explicit identity; 0201-F2 decision
0004 now defines nested and partial authoring defaults. The leading
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

The examples illustrate the compared candidates, not production declarations or
coverage evidence. Path versus ID references, scope placement and role tags
remain separate choices. The spike uses separate local Feature and optional
epic fields to test custom grammars without assuming a qualification delimiter;
the owner has now adopted that claim identity field structure (0201-F1
decision 0002). XML role names are defined by 0201-F2 decision 0002; ADR owner qualification is defined by
0201-F1 decision 0003.

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

#### Evaluation and extension

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
do not assign numerical value scores without evidence. The initial run exercised all three candidates against identical cases before
the owner selected the hardened hybrid. Extend the evidence with real xUnit
facts, multi-row theories and Playwright .NET browser tests in projects with no
Reqnroll dependency. Check missing bindings as well as valid ones, correlate
method identity to discovered cases and results, and keep skipped, failed and
missing outcomes distinct. Do not treat static method counts as runner discovery.

The [spike investigation](spikes/0236/README.md) records executable evidence,
limitations and the adopted model. OQ-2 and OQ-12 are resolved for the
authoring-versus-evidence decision; their remaining syntax and scope details
are carried into the owning Feature drafts.

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
| R1  | Code can declare bindings to individual claims, Feature specifications and ADRs using references that resolve to repository records              | Must | XML, binding identity, code-fix safety and diagnostic identity/versioning and severity defaults and standard Roslyn reporting controls are decided.                  |
| R2  | Roslyn analysis reports an invalid or unresolved code binding at the declaration that contains it                                                | Must | A reference that resolves to more than one record is not silently accepted.                                                                                          |
| R3  | Executable specification scenarios can bind to the same Feature and claim identities as code                                                     | Must | Here, executable specifications are Gherkin `.feature` files; a Feature specification is their companion agreement. Existing local `@B-00n` tags must be considered. |
| R4  | Scenario bindings are checked against the specification records rather than inferred from scenario titles                                        | Must | Markdown and Gherkin validation must have an explicit owner; Roslyn need not own it.                                                                                 |
| R5  | For the declared analysis scope, reconciliation exposes each claim's bound scenarios, tests and implementation, including missing required links | Must | Required-link and exemption policy is defined in 0201-F6 decision 0003; incomplete inputs or project sets cannot imply complete coverage.                            |
| R6  | A code binding to an ADR resolves the intended decision even when different Features have ADRs with the same local number                        | Must | ADR ownership and lifecycle eligibility are defined in 0201-F1 decisions 0003 and 0004.                                                                              |
| R7  | A maintainer can distinguish a valid reference, complete required bindings and passing behavioral evidence                                       | Must | A tag does not prove that an implementation meets a claim or obeys an ADR.                                                                                           |
| R8  | The checks are deterministic, offline and read-only, and diagnostics identify the offending source location and target reference                 | Must | Paths exposed by product diagnostics and reports are repository-relative.                                                                                            |
| R9  | A consumer can adopt the analysis with an explicit scope and enforcement policy, with the same binding rules used in the editor and build        | Must | Reporting controls, coverage/enforcement and input-delivery behavior are decided; installation and detailed domain/input configuration remain open.                  |

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

## Feature decomposition

The seven co-located draft specifications below are the decomposition of the
adopted model. Their companion scenarios state the obligations before production
tests or code exist; traceability rows remain Missing until implementation. Delivery items are now cut
from the agreed claims; the implementation queue is below. Owner review selected the three
project homes below; this does not select a mediator or change the engine
architecture.

| Feature | Agreement                                                                         | Invariant                                                                                                                       | Serves     | Depends on     |
| ------- | --------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------- | ---------- | -------------- |
| 0201-F1 | [Binding identity](../../src/analyzers/Bindings/.spec/README.md)                  | A qualified identity resolves under consumer grammars independently of frameworks.                                              | R1, R3, R6 | None           |
| 0201-F2 | [CSharp binding authoring](../../src/codefixes/.spec/README.md)                   | Type defaults assist invoked fixes; explicit declaration IDs alone supply checked evidence, and missing bindings are diagnosed. | R1, R2, R8 | F1             |
| 0201-F3 | [Test evidence adapters](../../src/analyzers/TestEvidence/.spec/README.md)        | Facts, theory cases and Playwright .NET tests correlate to bound methods without Reqnroll.                                      | R5, R7     | F1, F2         |
| 0201-F4 | [Optional scenario bridge](../../src/analyzers/Scenarios/.spec/README.md)         | Tagged scenarios and generated cases share the identity contract; shared steps do not stand in for scenario identity.           | R3, R4     | F1, F3         |
| 0201-F5 | [Traceability reconciliation](../../src/analyzers/Reconciliation/.spec/README.md) | Every required claim is considered, with explicit missing links, incomplete scope and distinct execution outcomes.              | R5, R7, R8 | F1, F3         |
| 0201-F6 | [Consumer analysis adoption](../../src/codeanalysis/.spec/README.md)              | Declared inputs, scope and policy produce consistent editor/build verdicts without requiring Reqnroll.                          | R9         | F1, F2, F3, F5 |
| 0201-F7 | [Native Gherkin claim catalogue](../../src/analyzers/Catalogue/.spec/README.md)   | Claims tagged in a consumer's Gherkin enter the same catalogue, diagnostics and enforcement as claims in a Markdown table.      | R9         | F1, F6         |

F4 is an optional extension; F5 and F6 do not depend on it. F2 keeps diagnostics
and their repairs together because a repair's safety depends on the same binding
verdict. F3 separates runner identity and cases from reference resolution; F5
owns completeness across inputs rather than assuming one compilation sees all
projects. Existing scenario validation is reused, not replaced by a competing
claim-tag interpretation. F7, added for consumer request #256, reads a
consumer's claims from tagged Gherkin through that same claim-tag grammar; F4
resolves scenario tags against the catalogue it builds, and F3 B-010 to B-019
cover the xUnit v2 and VSTest evidence the same request needs. No Feature name bundles unrelated capabilities with a
coordinating conjunction.

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

| Record                                                          | Existing boundary                                                                             | Effect of this epic                                                                                                                                                                                                                      |
| --------------------------------------------------------------- | --------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Brief § 2; `0001-F1` § 5 row 9; epic `0001` § Out of this epic  | Anything Roslyn stays in a consumer repository                                                | A new epic owns reusable code bindings here; the extraction and repository-wide checker remain separate. The extraction boundary is narrowed to hosting Roslyn in the repository-wide checker; the new binding Features remain separate. |
| `0001-F2` decision 0001 § Rejected                              | Roslyn was rejected as the host for repository-wide `SPEC###` checks                          | That rejection still holds. Code binding analysis is a separate responsibility; any changed ownership of the consumer's claim bridge needs an explicit decision.                                                                         |
| `specht-conventions` § Questions; `dotnet-tool` § Project setup | The companion says there is no Roslyn scope and project setup marks analyzers as inapplicable | The scope statements now distinguish the standalone checker from the adopted binding model; packaging remains open.                                                                                                                      |
| `0001-F1`, `0001-F5`, `0001-F6`                                 | Existing claim-tag resolution, identity grammars and specification discovery                  | Candidate A and C must reconcile with these contracts rather than introduce independent grammars or discovery rules.                                                                                                                     |
| `0001-F7`                                                       | Pinned schema versions own their rule vocabulary                                              | Decide analyzer diagnostic versioning and its relationship to schema pins before assigning rule ids.                                                                                                                                     |

## Open questions

The owner decisions below resolve the model selection in place. Remaining
questions belong to the Feature agreement that owns them; no open public contract
is silently settled by prototype behavior.

| OQ    | Question                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | Who answers       |
| ----- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------- |
| OQ-1  | Is spec.code the public product/package name, or a working name for a specht capability? RESOLVED, 2026-10-17: ADR-0016 selects the component/deployment identities; F6 decision 0005 publishes private Specht.CodeAnalysis with aligned repository release versions, contract-based interoperability, manifest-owned policy, MSBuild input delivery and standard Roslyn reporting controls. ADR-0017 selects the compiler-target exception.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    | Owner             |
| OQ-2  | Which public XML encoding and code elements implement the adopted hybrid? RESOLVED, 2026-10-17: owner adopted hardened C, type defaults for authoring and explicit member IDs for evidence (0201-F1 decision 0001). Which languages, declaration kinds, binding roles and multiple-target forms are supported, and which repairs can an invoked code fix safely offer? XML roles RESOLVED, 2026-10-17: `spec`, `verifies`, `implements` and `follows` (0201-F2 decision 0002). CSharp declaration scope RESOLVED, 2026-10-17: XML-documentable types and members with role-specific placements and no implicit member bindings (0201-F2 decision 0003). Multiple-target form RESOLVED, 2026-10-17: repeat relationship elements with one complete target each and scalar identity fields (0201-F2 decision 0005). Strict XML validation RESOLVED, 2026-10-17: defined role attributes, empty content and supported placements; diagnose invalid elements, exclude invalid relationships/defaults, and preserve unrelated XML (0201-F2 decision 0006). Duplicate handling RESOLVED, 2026-10-17: count once by symbol, role and resolved target, diagnose duplicates, and offer invoked redundant-copy removal while preserving distinct roles and symbols (0201-F2 decision 0007). Code-fix safety RESOLVED, 2026-10-17: invoked deterministic repairs, explicit existing-record selection for target choices, no guessed or destructive corrections, and Fix All without target propagation (0201-F2 decision 0008). Diagnostic/version policy remains in OQ-9. | Owner with design |
| OQ-3  | What is the common reference form for Feature specifications, claims and repository- or Feature-scoped ADRs, including repositories without an epic tier? Which additional record kinds does "other such things" include? RESOLVED, 2026-10-17: claim references use separate `epic`, `feature` and `claim` fields, with `epic` optional (0201-F1 decision 0002). ADR ownership RESOLVED, 2026-10-17: an `adr` reference without `feature` selects repository ownership; with `feature`, it selects that Feature, with optional `epic`, no inherited default and no cross-scope search (0201-F1 decision 0003). F1 decision 0005 limits checked targets to claims and ADRs; Feature/epic identities provide scope and additional record kinds are deferred.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     | Owner             |
| OQ-4  | How do Gherkin Feature-level and Scenario-level bindings interact with existing local claim tags? Are bindings inherited, repeated or qualified explicitly? RESOLVED for behavior, 2026-10-17: F4 decision 0001 uses Feature scope by default, complete explicit Scenario/Outline overrides without merging, and local claim tags in effective scope. Exact tag/configuration encoding remains technical design.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                | Owner             |
| OQ-5  | Which claims require implementation, scenario and test bindings, and which code requires a claim binding? How are shared infrastructure, generated code, test code and exemptions treated? RESOLVED, 2026-10-17: 0201-F6 decision 0003 requires valid test bindings for non-exempt current claims in selected Feature scope; implementation/scenario requirements are opt-in. Validate explicit bindings in analyzed projects; require missing authored bindings only in selected enforcement scopes. Generated code requires no authored bindings; shared infrastructure has no blanket every-method rule. Exemptions require reasons and remain visible as exempt, never covered. Configuration encoding remains open in OQ-8 / 0201-F6 OQ-1.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 | Owner             |
| OQ-6  | How are scenario-to-test bindings established, and which inputs and aggregation boundary prove completeness across projects and compilations? PARTIALLY RESOLVED, 2026-10-18: 0201-F6 decision 0004 requires current inputs for every declared project/target-framework combination, source/assembly/configuration identity matching and visibly stale/mismatched artifacts excluded from current discovery/execution. F3 § 7 now defines the version-one envelope, deterministic input fingerprints, stable method/case identities, and explicit discovery/result states under decision 0001. F4 decision 0001 maps scenario-to-case generator/runner metadata through the common contract; its producer-specific metadata extraction remains technical design.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                | Design            |
| OQ-7  | Which document and ADR lifecycle states are valid binding targets, and what happens when a referenced claim is withdrawn or an ADR is superseded? RESOLVED, 2026-10-17: 0201-F1 decision 0004 defines eligible current claims and accepted ADRs, with resolving historical links retained but excluded from current evidence. Diagnostic policy remains in OQ-9.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                | Owner             |
| OQ-8  | How are the pinned manifest and specification records supplied to Roslyn analysis, and how are missing or invalid inputs diagnosed consistently in editor and build? RESOLVED for input behavior, 2026-10-17: 0201-F6 decision 0004 requires build discovery of actual consumer record contents supplied through AdditionalFiles, with offline analysis, editor/build consistency, invalidation and default errors for missing/invalid/incompatible required inputs. Analysis is unavailable/incomplete rather than clean. Detailed discovery/configuration encoding remains in 0201-F6 OQ-1.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   | Design            |
| OQ-9  | Which diagnostic namespace, severities, suppression rules and versioning policy apply to analyzer findings and reconciliation gaps? PARTIALLY RESOLVED, 2026-10-17: analyzer findings use stable `SPCT####` IDs starting at `SPCT0001`; retired IDs are never reused. Analyzer rules follow analyzer package releases, schema contract changes retain schema-version rules, and the pinned checker vocabulary remains unchanged (0201-F2 decision 0009). Severity RESOLVED, 2026-10-17: warnings for binding defects and missing required bindings within selected enforcement scope; information for valid duplicates; consumer per-rule overrides including error promotion (0201-F6 decision 0001). Suppression/reporting controls RESOLVED, 2026-10-17: use standard Roslyn configuration, pragmas and SuppressMessage with evidence validity, required-link completeness and declared scope unchanged (0201-F6 decision 0002). Input failure severity RESOLVED, 2026-10-17: errors by default for missing/invalid/incompatible required inputs when enabled (0201-F6 decision 0004). Reconciliation-gap presentation remains in OQ-11.                                                                                                                                                                                                                                                                                                                                                                                                                     | Owner with design |
| OQ-10 | Does adoption reuse or extract the consumer's existing claim bridge, and which repository retains ownership of its convention analyzers? RESOLVED, 2026-10-17: F4 decision 0001 reuses the consumer claim bridge through the common evidence contract, maps generator/runner scenario metadata, keeps convention analyzers consumer-owned and keeps Reqnroll integration optional.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | Owner             |
| OQ-11 | Where is the reconciled evidence presented, and does it extend the existing report contract or have its own surface? RESOLVED for product surface, 2026-10-17: 0201-F5 decision 0001, revised 2026-10-19, selects specht evidence with human summary and a separate versioned JSON contract, preserves the checker report and consumes supplied normal-workflow artifacts without launching tests. Detailed report schema and CLI options/exit codes remain technical design.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   | Owner with design |
| OQ-12 | Which nested and partial type rules govern authoring defaults? RESOLVED, 2026-10-17: type context is authoring-only and complete explicit identity travels with a move. 0201-F2 decision 0004 selects the nearest enclosing annotated type, whole-default replacement at inner types, shared single or identical partial defaults, diagnostics and no automatic materialization for conflicting partial defaults, and no base-type default inheritance.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         | Owner with design |

## Out of this epic

| Item                                                                            | Boundary                                                                                                                                                |
| ------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Proving arbitrary code behavior or architectural conformance from an annotation | Tests and review remain the evidence; this epic checks bindings and their completeness under a declared policy.                                         |
| Generic coding-convention analyzers                                             | No transfer of the consumer's convention analyzer scope is implied; OQ-10 addresses ownership.                                                          |
| Replacing the Markdown specification checker                                    | Epic `0001` continues to own repository-wide specification validation.                                                                                  |
| Creating claims or auto-approving specifications from code                      | The specification-first artifact chain remains authoritative.                                                                                           |
| Code fixes that invent references or edit specification records                 | Invoked repairs use existing records and the code-fix safety contract in 0201-F2 decision 0008; they never invent claims or edit specification records. |
| Runtime enforcement or remote traceability services                             | The requested capability is static, deterministic and offline.                                                                                          |
| Production analyzer delivery from this decomposition                            | The isolated binding spike supplies evidence; each production Feature starts with an agreed specification and scenarios before delivery items are cut.  |

## Project layout

Owner review, 2026-10-17: adopt the sibling project layout demonstrated by
[Airframe](https://github.com/RocketSurgeonsGuild/Airframe/tree/main/src/CodeAnalysis).
The project identities and folder names are deliberate:

| Folder              | Project identity      | Responsibility                                                                                            |
| ------------------- | --------------------- | --------------------------------------------------------------------------------------------------------- |
| `src/analyzers/`    | `Specht.Analysis`     | Binding model and read-only Roslyn diagnostics; no code-fix Workspaces dependency.                        |
| `src/codefixes/`    | `Specht.Codefixes`    | Invoked authoring repairs; references the analyzer project.                                               |
| `src/codeanalysis/` | `Specht.CodeAnalysis` | Deployment project; packages the analyzer and code-fix assemblies rather than duplicating their behavior. |

The planned project files are `analyzers.csproj`, `codefixes.csproj` and
`codeanalysis.csproj` in those folders, following the reference's naming. The
analyzer and code-fix projects are non-packable; CodeAnalysis owns deployment of
this capability. `src/tool` continues to own the existing dotnet-tool package.
A future package must include both component assemblies and the editor/build
assets it needs; a deployment project must not ship an empty package or leak its
own build output as a consumer runtime library.

[ADR-0018](../../.spec/adr/0018-lowercase-analyzer-project-home.md)
records this choice and its costs. Features remain vertical agreements rather
than a one-to-one project list: F2 specifies diagnostics in Analyzers and the
repairs in CodeFixes, with its agreement beside CodeFixes. F1, F3, F4 and F5 sit
in the relevant analyzer project slices; F6 sits beside the deployment project.
No duplicate specification or new shared-core assembly is introduced.

This revision moves the agreements and their acceptance links; it does not
scaffold empty production assemblies. The standalone experiment remains outside
the solution. Supported Roslyn target frameworks, dependency versions, package
installation/versioning, input transport and remaining diagnostic policy still need
the owning Feature agreements before implementation. The folder decision does
not waive ADR-0014's engine/host migration gate.

## Placement and next step

This epic lives at `epics/0201-spec-code-analysis/epic.md`, where the existing
epic discovery glob finds it. Feature specifications live beside their adopted
project homes as linked above.

The 2026-10-17 owner direction moves this work into implementation planning.
Start with 0238; 0242 can proceed independently. Core analyzer diagnostics,
code fixes and package delivery precede runner collection and trace reporting.
The analyzer components target netstandard2.0 alongside the existing modern
engine/tool targets; an engine port is not a prerequisite. The trace command
consumes exported evidence and cannot use the analyzer directly. Required test
outcomes must pass by default under F5 decision 0002.

## Implementation work items

| Item                                                                                     | Work                                      | Prerequisites    |
| ---------------------------------------------------------------------------------------- | ----------------------------------------- | ---------------- |
| [0238](../../src/analyzers/Bindings/.issue/0238-binding-identity.yml)                    | Binding identity and record resolution    | None; start here |
| [0239](../../src/codefixes/.issue/0239-xml-binding-diagnostics.yml)                      | XML binding diagnostics                   | 0238, 0242       |
| [0240](../../src/codefixes/.issue/0240-type-authoring-defaults.yml)                      | Nested and partial type defaults          | 0239             |
| [0241](../../src/codefixes/.issue/0241-binding-code-fixes.yml)                           | Safe invoked code fixes                   | 0240             |
| [0242](../../src/codeanalysis/.issue/0242-analysis-policy-inputs.yml)                    | Analysis configuration and input policy   | None             |
| [0243](../../src/codeanalysis/.issue/0243-codeanalysis-package-delivery.yml)             | Package and editor/build delivery         | 0241, 0242       |
| [0244](../../src/analyzers/TestEvidence/.issue/0244-test-evidence-collection.yml)        | Framework-independent evidence collection | 0243             |
| [0245](../../src/analyzers/Scenarios/.issue/0245-optional-scenario-bridge.yml)           | Optional scenario bridge                  | 0244             |
| [0246](../../src/analyzers/Reconciliation/.issue/0246-trace-artifact-reconciliation.yml) | CLI artifact reconciliation               | 0244             |

All 108 claim IDs are assigned exactly once across these items. The 0201-F7
claims and 0201-F3 B-010 to B-019, added 2026-10-23, have no item yet. Each item owns
its delivery state and cites its Feature's claims; none is marked implemented.
