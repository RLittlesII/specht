---
title: "Native Gherkin claim input"
description: "Configure tagged Gherkin as the claim catalogue and collect test evidence from a clean checkout"
type: reference
---

# Native Gherkin claim input

`Specht.CodeAnalysis` can read claims directly from tagged `.feature` files.
Set `codeAnalysis.records.claimSource` to `"gherkin"` in
`.specht/manifest.json`; leaving it absent keeps the Markdown claims table.
Native input needs neither Reqnroll nor scenario records.

## Select the claim files

This manifest selects Feature `arithmetic` for a test project, without an epic
tier or a Feature metadata record. Adjust the project path and target framework
to the project that installs [Specht.CodeAnalysis](../src/codeanalysis/README.md).
Paths are relative to the repository root.

```json
{
  "schemaVersion": "0.2.0",
  "identifiers": {
    "feature": "^[a-z]+$",
    "claim": "^REQ-FIX-[0-9]{3}$"
  },
  "markers": {
    "approved": "approved",
    "withdrawn": "Withdrawn",
    "superseded": "Superseded"
  },
  "claim": { "tag": "@{claim}", "tagIgnoreCase": false },
  "codeAnalysis": {
    "mappings": {
      "/schemaVersion": "/schemaVersion",
      "/identifiers": "/identifiers",
      "/markers": "/markers",
      "/claim": "/claim"
    },
    "projects": [
      {
        "path": "test/arithmetic.tests.csproj",
        "targetFrameworks": ["net10.0"]
      }
    ],
    "features": [{ "feature": "arithmetic" }],
    "enforcement": [
      {
        "project": "test/arithmetic.tests.csproj",
        "include": ["test/**/*.cs"],
        "exclude": [],
        "role": "verifies",
        "declarationKinds": ["testMethod"]
      }
    ],
    "records": {
      "claimSource": "gherkin",
      "claimFileSets": [
        {
          "glob": ".spec/features/arithmetic/*.feature",
          "feature": "arithmetic"
        }
      ],
      "plannedTags": ["@ignore"],
      "withdrawnTags": ["@withdrawn"],
      "supersededTags": ["@superseded"]
    },
    "scenarioScopes": { "featureTag": "@spec.feature:{feature}" }
  }
}
```

Each `claimFileSets` entry maps a root-relative `glob` to one Feature identity.
It must match delivered scenario inputs; every selected Feature needs a set.
Use one set per Feature identity, and keep sets from matching the same file.
Add `epic` to the file set and Feature selection exactly when
`identifiers.epic` declares an epic grammar; omit it otherwise. Identities must
satisfy their configured grammars. With `scenarioScopes.featureTag` configured,
a Feature-level tag such as `@spec.feature:arithmetic` must agree with the mapping.
The same agreement applies to a configured `scenarioScopes.epicTag`.

The package delivers a central `.spec/features/**/*.feature` tree to every
selected project by default. A glob in `claimFileSets` selects from those inputs;
it does not cause MSBuild to deliver files. For a different tree, set this
property in `Directory.Build.props` so it is available before package imports:

```xml
<PropertyGroup>
  <SpechtRoot>$(MSBuildThisFileDirectory)</SpechtRoot>
  <SpechtScenarioInputs>$(SpechtRoot)/specifications/**/*.feature</SpechtScenarioInputs>
</PropertyGroup>
```

Set the file-set glob to the corresponding root-relative path. This property
replaces the default scenario input globs; include any other needed trees in
its semicolon-separated value.

A file set may add `record`, for example `src/arithmetic/.spec/README.md`, to take
the Feature lifecycle from that record's `spec_status` frontmatter key
(or `records.featureLifecycleKey`). The file must be delivered as a `feature`
input and validate against the supplied Feature schema. It needs no claims table;
its table, if present, is ignored. Without `record`, the Feature is current.

## Author claims and planned cases

Put this in `.spec/features/arithmetic/arithmetic.feature`:

```gherkin
@spec.feature:arithmetic
Feature: Arithmetic

  @REQ-FIX-001
  Scenario Outline: Add two operands
    Given operands <a> and <b>
    Then their sum is <sum>

    Examples:
      | a | b | sum |
      | 1 | 2 | 3   |

    @ignore
    Examples: Planned cases
      | a | b | sum |
      | 2 | 2 | 4   |
```

A claim tag matches the existing `claim.tag` form and `identifiers.claim`
grammar. Here `@REQ-FIX-001` declares `REQ-FIX-001`, exactly as captured by
`{claim}`. Declare `claim.tag` explicitly and map its object through `/claim`;
the analyzer does not fill it from the schema pin. Other tags, including a tag
whose captured text fails the grammar, are ordinary tags that declare no claim.
Only claim tags honour `claim.tagIgnoreCase`.

A Feature, Rule, Scenario or Scenario Outline can declare a claim. A Rule's
scenarios inherit its claim without redeclaring it; an Outline declares one
claim across its example rows. Place claim tags on those declaring elements;
an Examples block cannot declare a claim. The reader accepts English keywords
only; omit the language header or use `# language: en`.

`records.plannedTags` defaults to `["@ignore"]`; `[]` plans nothing. A matching
tag on a Feature, Rule or scenario plans its cases; on an Examples block it
plans only that block's rows. Planned cases count as neither implemented nor
passing evidence. Their claim stays declared and its required links stay required.
In the example, only the second Examples block is planned.

A claim is Active unless its declaring element carries a configured lifecycle
tag. `records.withdrawnTags` and `records.supersededTags` both default to `[]`;
the manifest above opts into `@withdrawn` and `@superseded`. Superseded takes
precedence when both appear. These lists and `plannedTags` use exact,
case-sensitive `@`-led tags; inherited lifecycle tags do not change a claim's state.

## Build and collect evidence from a clean checkout

For a selected xUnit v2/VSTest project with an authored `verifies` relationship,
mandatory enforcement and warnings as errors, run from the repository root:

```sh
dotnet build test/arithmetic.tests.csproj
dotnet test test/arithmetic.tests.csproj --no-build --list-tests
dotnet test test/arithmetic.tests.csproj --no-build
specht evidence test/obj/specht-test-evidence
```

The first build reads the native claim catalogue and enforces relationship
shape, grammar and target resolution. With no current evidence, test-method
recognition is provisional: mandatory `testMethod`/`verifies` enforcement waits,
so evidence can be collected without a suppression. Other enforcement continues.
The build also exports compilation evidence and writes the runner configuration.
Discovery writes `test/obj/specht-test-evidence/arithmetic.tests-net10.0.json`
with discovered tests but no results; the run updates it with results and selection.
Keep project, framework, configuration, sources and policy consistent across steps.

Supply that artifact to the subsequent build in the test project's body:

```xml
<ItemGroup>
  <SpechtTestEvidence Include="obj/specht-test-evidence/$(MSBuildProjectName)-$(TargetFramework).json" />
</ItemGroup>
```

Rebuild with the same `dotnet build` command for authoritative test recognition
and mandatory `testMethod`/`verifies` enforcement. The project's own artifact is
not included automatically. If it no longer matches the compilation, recognition
stays provisional; rebuild, discover and run again to refresh it.

`specht evidence` is the reconciliation gate for current required links and
passing outcomes. Before evidence exists it reports incomplete and names the
build, discovery and run steps; a successful first build alone does not prove a
claim passed. The native catalogue is exported with the evidence so reconciliation
can resolve the claims. No claims-table adapter is generated.

For diagnostic conditions and remedies, see [SPCT0001](../src/codeanalysis/docs/SPCT0001.md),
[SPCT0010](../src/codeanalysis/docs/SPCT0010.md),
[SPCT0011](../src/codeanalysis/docs/SPCT0011.md) and
[SPCT0012](../src/codeanalysis/docs/SPCT0012.md).
