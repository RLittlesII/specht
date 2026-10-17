---
title: "Spike 0236: comparing XML binding contracts"
description: "Executable evaluation plan and findings for inherited, explicit and hybrid specification bindings"
type: investigation
---

# XML binding comparison

This investigation answers epic `0201`'s evaluation cases. It adopts no public
contract. Its standalone projects are outside the shipped solution and NuGet
packages. The prototype uses the pinned SDK's Roslyn host in an executable;
it is not a distributable analyzer package or an IDE compatibility test.

## Proposed selection

Use the **hybrid as authoring assistance with explicit member identities as the
checked contract**. A type's stable Feature ID can narrow code-fix choices; the
fix writes the selected Feature, optional epic and claim onto the method. An
inherited-only test binding is an error until expanded. A complete member
binding can target another Feature without inheriting part of the default.

This combines contextual authoring with portable evidence. Keeping implicit
scope inheritance in C repeats A's silent-retargeting defect. If the extra
scope/default policy is not worth maintaining, B is the smaller safe fallback.
The spike proposes this selection; no Feature agreement or open question is
approved by it.

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

The final experiment run passed **484 assertions across 83 observations** and
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
builds the three isolated projects, runs both Reqnroll projects, and runs the
Roslyn harness. A clone must first populate the repository's package cache.
No test or analyzer contacts the network. The pinned SDK supplies Roslyn;
existing central package versions supply Reqnroll and the engine dependencies.

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
- The prototype recognizes synthetic xUnit facts. It does not settle all test
  frameworks, inherited test attributes, implementation/ADR binding roles,
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

OQ-2 must agree the tags, roles, required explicit evidence and repairs. OQ-3
must agree qualified identity encoding. OQ-6 must choose the scenario/test
adapter and completeness boundary. OQ-8 must specify analyzer input delivery and
invalidations. OQ-12 must decide whether type defaults are authoring hints only,
and how nested/partial scopes and member overrides interact. The proposed safe
hybrid answers none of those by implication.
