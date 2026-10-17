---
title: "Spike 0236: comparing XML binding contracts"
description: "Executable evaluation plan and findings for inherited, explicit and hybrid specification bindings"
type: investigation
---

# XML binding comparison

This investigation answers epic `0201`'s evaluation cases. On 2026-10-17 the
owner adopted its hardened hybrid model and requested the non-Reqnroll extension.
The exact public XML and diagnostic contracts remain open in the Feature drafts. Its standalone projects are outside the shipped solution and NuGet
packages. The prototype uses the pinned SDK's Roslyn host in an executable;
it is not a distributable analyzer package or an IDE compatibility test.

## Adopted model

Use the **hybrid as authoring assistance with explicit member identities as the
checked contract**. A type's stable Feature ID can narrow code-fix choices; the
fix writes the selected Feature, optional epic and claim onto the method. An
inherited-only test binding is an error until expanded. A complete member
binding can target another Feature without inheriting part of the default.

This combines contextual authoring with portable evidence. Keeping implicit
scope inheritance in C repeats A's silent-retargeting defect. If the extra
scope/default policy is not worth maintaining, B is the smaller safe fallback.
The owner selected this model after the initial spike. The six decomposed
Feature agreements remain draft; this decision does not approve their remaining
public-contract choices.

## Trial contracts fixed before execution

| Candidate    | Scope                                                 | Member binding                                                      | Qualification                                                                                           |
| ------------ | ----------------------------------------------------- | ------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------- |
| A: inherited | `<spec path="src/alpha/.spec/README.md" />` on a type | `<requirement>B-001</requirement>`                                  | The path selects one specification; its frontmatter owns identity                                       |
| B: explicit  | None required                                         | `<verifies feature="F1" epic="0001" claim="B-001" />`               | Separate local Feature and optional epic fields                                                         |
| C: hybrid    | `<spec feature="F1" epic="0001" />` on a type         | `<verifies claim="B-001" />`, or a complete explicit member binding | A complete member identity overrides the type; missing scope uses the nearest annotated containing type |

Separate `feature` and `epic` fields isolate qualification from local-ID regexes;
the combined `0001-F1` encoding in the epic remains an alternative. All three
use the existing manifest parser, Markdown/claim model and claim-tag parser.
Scope is lexical, including containing nested types; base-class inheritance is
excluded. Distinct scopes on partial declarations conflict. Repeated member
tags can bind distinct claims; duplicates and ambiguous identities are errors.

For C, additionally test an invoked code fix that expands an inherited binding
to explicit member IDs _before_ a move. This is a remediation probe, not a
fourth candidate and not permission to claim an analyzer can recover lost intent
after a move. Explicit bindings to wrong-but-existing claims are negative
controls for every candidate.

## Evaluation oracles

Write intended target identities separately from the source comments. A
resolving but different target is a mismatch even if Roslyn reports no error.
Positive controls must resolve; invalid inputs must diagnose at their source.
Code-fix checks apply actual Roslyn code actions, reanalyze the edited document
and compare preserved documentation. Fixture paths and emitted evidence are
repository-relative. No performance or human reading-time measurements are
claimed.

| Epic case                        | Experiment                                                                                                                                                            |
| -------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Cohesive and mixed-Feature types | Twelve members in one Feature; a single class mixing two Features with repeated local claim IDs; count scope mentions, types and XML characters                       |
| Member move                      | Move an unchanged binding between two scopes that both declare `B-001`; compare against the independent intended target, then repeat after materialization            |
| Specification move               | Change the specification's path without changing its frontmatter identity and reanalyze unchanged source                                                              |
| Consumer grammars                | Default and custom IDs, optional epics, duplicated identities and bad regexes                                                                                         |
| Binding diagnostics and repairs  | Missing scope/claim, malformed XML, unknown and malformed IDs, duplicate and multiple claims, nested/partial scopes, explicit overrides and preservation during fixes |
| Scenario/test chain              | Generate and execute synthetic Reqnroll scenarios with a shared step and a two-row outline; inspect generated test metadata and record incomplete project scope       |

## Observed findings

The initial three-leg experiment passed **484 assertions across 83 observations** and
**four Reqnroll test invocations with zero skips**, using Roslyn `5.12.0.0` from
the pinned .NET 11 preview SDK. The engine baseline is commit `47fc3ce5`; the
experiment source digest is
`d56ed6bb49031087b7274d51ddad67cd95849ed7da13ec008e8a1bcdf07b2286`.

The runner's assertions deliberately include expected counterexamples. A green
spike run means those failures were reproduced, not that all candidate contracts
are safe. Source comments and intended target identities are separate inputs.

| Case                                                                                    | A: inherited path                                                                                     | B: explicit IDs                                     | C: hybrid                                                                  |
| --------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------- | --------------------------------------------------- | -------------------------------------------------------------------------- |
| Twelve cohesive bound methods                                                           | One scope, 1,006 XML characters                                                                       | Twelve explicit member identities, 1,188 characters | One scope, 926 characters before expansion                                 |
| One class spanning two Features                                                         | Resolves the second member to the wrong existing claim; splitting into two classes resolves correctly | Both intended targets resolve                       | Explicit override resolves both intended targets                           |
| Move a member between scopes sharing `B-001`                                            | No diagnostic; target changes from `0001/F1/B-001` to `0001/F2/B-001`                                 | Original target preserved                           | Short form silently retargets; expanded form preserves the original target |
| Move a specification without changing identity                                          | Old path no longer resolves                                                                           | Reference survives                                  | Reference survives                                                         |
| Default/custom grammars, with/without epics                                             | Resolves all four valid combinations                                                                  | Same                                                | Same                                                                       |
| Duplicate specification identities at different paths                                   | A path selects one record; a repository identity check is still required                              | Ambiguous ID diagnoses                              | Ambiguous ID diagnoses                                                     |
| Fix a missing binding in a scoped class                                                 | Two existing-record choices                                                                           | Three choices across the fixture's Features         | Two scoped choices; applied fix persists explicit IDs                      |
| Fix a missing binding without type scope                                                | No safe local-reference fix offered                                                                   | Three explicit choices                              | Three explicit choices                                                     |
| Missing inputs, invalid regex, malformed XML/claim, unknown claim and duplicate binding | Diagnosed                                                                                             | Diagnosed                                           | Diagnosed                                                                  |
| Generated scenario/test chain                                                           | Requires the common bridge                                                                            | Same                                                | Same                                                                       |

XML character counts include identical summaries and delimiters in this
synthetic fixture. They measure text volume, not writing time, readability or
runtime performance. Materializing C removes its storage saving and adds the
optional type declaration to B's persisted references. Its value is contextual
selection, not shorter checked XML. No claim is selected automatically when more
than one declared target is available.

Nested and partial type probes exercise A and C's nearest lexical scope and
conflicting partial declarations. C's complete member override retains its own
target after moving under a different default. B and C also reject malformed
Feature IDs, unknown Features and the wrong owning epic. Distinct repeated
bindings can verify several claims; duplicate bindings diagnose at the method
identifier. Applied fixes preserve existing summary and remarks content and
reanalyze cleanly.

### Why the materialization policy matters

The hardened C probe reports an error (`SPK009`, an experimental identifier)
for an inherited-only test binding. Its actual `CodeFixProvider` supplies one
materialization action after the target resolves unambiguously. Applying the
Roslyn action writes explicit IDs, clears the error and preserves the intended
target after the method moves. The default remains an authoring hint and never
silently reinterprets an already explicit member binding.

A fix invoked _after_ a move cannot recover the original intended Feature from
a valid local claim alone. Every candidate also accepts a deliberately wrong
but existing explicit target: static reference validity cannot prove developer
intent or assertion quality. Review and behavioral evidence remain necessary.

### The scenario bridge is independent of XML placement

Two actual Reqnroll projects generate and execute four test invocations with no
skips: alpha has one ordinary scenario and two outline rows, and beta has one
scenario reusing the local claim ID `B-001`. All three alpha cases use the same
handwritten step methods. No step documentation is treated as scenario coverage.

`ScenarioBridge` reads Reqnroll's generated Cucumber messages and generated test
syntax. Pickle indexes connect test methods/inline-data rows to scenario AST IDs
and example-row locations. Qualified Feature tags and claim tags resolve through
the catalog; generated category metadata is checked against the messages. No
scenario title or generated method name is used as a matching key. Those names
are retained only for presenting the observed link.

The bridge finds all four invocations. Supplying only the alpha project yields
three invocations, an explicit missing beta project and no evidence for
`0001/F2/B-001`; the declared scope remains incomplete. The project set is an
explicit synthetic input, not repository project discovery.

Default Reqnroll generation emits **zero** proposed XML bindings. A generator
adapter or an agreed metadata bridge must still establish the generated-test
binding contract. This spike inspects genuine generation and runs its tests;
it does not pretend that handwritten XML on shared steps closes that gap. The
current `pickleIndex` encoding is observed for the installed Reqnroll version,
not a promised stable public API across versions. Reqnroll's documented
[decorator extension](https://docs.reqnroll.net/latest/extend/decorators.html) is
an alternative to evaluate when that agreement is written.

## Reproduction and evidence

From the repository root:

```sh
python3 epics/0201-spec-code-analysis/spikes/0236/run-spike.py
```

The driver restores only from the local package cache with auditing disabled,
builds four isolated projects, runs both Reqnroll projects and the direct
xUnit/Playwright suite, and runs the Roslyn harness. Pass `--without-reqnroll` to
restore and build only the prototype and direct-test project and skip the scenario
bridge entirely. A clone must first populate the repository's package cache.
No test or analyzer contacts the network. The pinned SDK supplies Roslyn;
central package versions supply the test and engine dependencies. The spike pins
Microsoft.Playwright.Xunit.v3 1.56.0 to use the available Chromium revision 1194,
not as a production dependency recommendation. Before offline reproduction,
restore that project from NuGet and install its matching Chromium browser with
Playwright's documented browser installer; the driver does neither automatically.
The package must be cached and the matching browser preinstalled.

The driver writes `.artifacts/0236/bindings.json`, `run-manifest.json` and logs.
The manifest records the git revision, a SHA-256 of experiment source inputs,
exact commands with repository-relative paths, exit codes, assertion totals and
observation totals. The binding evidence records diagnostics and method
locations, intended/actual targets, annotation counts and the generated-test
links. These are generated outputs and stay gitignored.

## Limits and decisions still owed

- This is a headless, single-SDK-host experiment on one local platform. It proves
  no IDE support, distributable analyzer target, package dependency layout,
  incremental editor behavior, performance result or second-platform result.
- The executable hosts both diagnostics and fixes for convenience. A production
  analyzer and code-fix assembly need their own supported targets and packaging;
  no architecture or benchmark gate is waived by this prototype.
- The epic's Analyzer testing direction selects
  `Rocket.Surgery.Extensions.Testing.SourceGenerators` with direct assertions
  for production tests and excludes Verify snapshots. This standalone mechanical
  harness does not replace that direction or require a snapshot dependency.
- Synthetic Markdown contains the frontmatter and claims needed by the catalog,
  not a fully approved twelve-section Feature. Existing specification validation
  remains the upstream authority, including duplicate-identity checks.
- The extended prototype recognizes Fact-derived attributes using semantic base
  types and compiles against real xUnit and Playwright assemblies. It does not
  settle all test frameworks, assembly spoofing, implementation/ADR binding roles,
  lifecycle policy, generic XML forms, `inheritdoc` or generated-code policy.
- The materialization probe supports the trial's single-line XML forms and has
  no Fix All provider. Formatter preservation, malformed scope diagnostics and
  all code-fix edge cases need the production agreement and tests.
- Reconciliation checks the declared synthetic project set and generated
  metadata. It does not ingest arbitrary test-result formats, generate XML
  through a Reqnroll plugin, prove assertion quality or discover all projects.
- No human usability study or maintenance-time measurement was run. The proposal
  prioritizes the observed correctness boundary, then contextual code-fix
  selection and implementation complexity; it assigns no numerical value score.

OQ-2 still must agree the tags, roles and safe public repair forms; explicit
member identity for checked evidence is adopted. OQ-3
must agree qualified identity encoding. OQ-6 must choose the scenario/test
adapter and completeness boundary. OQ-8 must specify analyzer input delivery and
invalidations. OQ-12 is resolved for authoring-only defaults and complete member overrides;
nested/partial authoring conflicts remain open. Prototype details do not settle
those remaining choices by implication.

## Extension: framework-independent tests

Owner direction, 2026-10-17: adopt the hardened hybrid and extend this experiment
without Reqnroll. The new framework project must restore with no Reqnroll
transitive dependency and execute real xUnit facts, InlineData and MemberData
theories, and Playwright .NET PageTest facts and theories. Browser fixtures use
SetContentAsync with synthetic local markup and do not contact a website.

The oracle requires eight bound methods to yield eleven discovered cases,
including two distinct methods with the same display name. One case is an
intentional skip; a separately selected fact is intentionally failed as an
outcome control. Expected runner IDs and type/method metadata, not display-name
matching, connect discovery and results. Mutation cases remove a theory binding,
leave an inherited-only binding, and corrupt a target; each must produce its
expected diagnostic at the method. Removing a whole project's evidence must
leave coverage incomplete. The initial exact-Fact recognition is expected to
miss a missing Theory binding; record that red result before broadening semantic
recognition. These are spike obligations, not Covered production claims.

### Observed extension results

The no-Reqnroll run passed 477 Roslyn assertions and 82 observations, then
correlated **eight bound methods to eleven runner-discovered cases**. Ten cases
passed, including the Playwright PageTest fact and two theory rows in real
headless Chromium; one intentional skip remained skipped. A separately selected
fact intentionally failed and the driver required exit 1 and its failed event.
The restored dependency graph contains zero Reqnroll packages.

The old exact-Fact analyzer missed a missing InlineData theory binding; the new
red test failed with zero diagnostics where SPK001 was expected. Semantic
FactAttribute base-chain recognition fixed it. The green run now asserts missing
bindings on a real fact, a real theory and a Playwright theory, plus inherited-only
and unresolved bindings on a theory. Counts and method source spans are asserted.

The xUnit full discovery listing pre-enumerates theories. Automated runner events
carry the same TestCaseUniqueID, plus class/method metadata, so results correlate
without display-name matching. Two methods deliberately share a display name and
remain separate. XML result IDs are per-run IDs and are unsuitable as discovery
join keys; the adapter probe therefore uses automated events. This does not prove
all runtime data or overload cases: dynamic non-pre-enumerated theories, arbitrary
MemberData payloads, overloaded names and stale assemblies remain production
adapter questions in 0201-F3.

The control that removes all references to F2's claim reports that unreferenced
claim by enumerating the declared required set. Removing one case's outcome leaves
a missing-result gap; omitting the selected project leaves scope incomplete.
These are explicit synthetic reconciliation inputs, not repository-wide project
discovery or an adopted coverage-policy default. The source-side missing-method
checks and the claim-side missing-evidence checks demonstrate the two ends; they
cannot infer whether a wrong but resolving claim ID matches an assertion's intent.

`framework-evidence.json` retains the eleven ID-based links and negative controls.
`framework-discovery.log`, `framework-events.log` and `framework-negative.log`
retain sanitized real runner data. The no-Reqnroll and full run manifests identify
source inputs and commands; no generated evidence is committed. Framework support
is xUnit v3 plus Playwright .NET's xUnit v3 adapter on this platform, not a claim
that every .NET or JavaScript test runner is implemented.
