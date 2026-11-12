---
title: Walkthrough
description: One small repository from init through the check, the analyzer, test evidence and the MCP server
type: reference
---

# Walkthrough

One Feature, `F1` Checkout, with two claims, checked by `specht.tool`, linked
to C# by `Specht.CodeAnalysis`, and queried through `specht.mcp`. Every output
below is from a real run of the `0.6.1-preview` packages on the .NET 11 SDK.

## 1. Install and init

```sh
dotnet new tool-manifest --output .config
dotnet tool install --local specht.tool --prerelease
dotnet specht init --all
```

`init --all` writes `.specht/manifest.json`, the frontmatter schemas and the
templates.

## 2. Write a specification

Copy `.specht/templates/feature.md` to `src/checkout/.spec/README.md`. Without
epics, delete its `epic` line. Fill in the sections; § 3 holds the claims:

```markdown
| ID    | Claim                                                  | Source      | Status |
| ----- | ------------------------------------------------------ | ----------- | ------ |
| B-001 | Given a cart, its total is the sum of its line prices. | User need 1 | Active |
| B-002 | Given an empty cart, its total is zero.                | User need 1 | Active |
```

Put `checkout.feature` beside it, one tagged scenario per claim:

```gherkin
Feature: Checkout

  @B-001
  Scenario: Total sums the lines
    Given a cart with lines priced 2 and 3
    Then its total is 5

  @B-002
  Scenario: Empty cart totals zero
    Given an empty cart
    Then its total is 0
```

## 3. Check

With only `B-001` in the § 9 matrix:

```text
$ dotnet specht
src/checkout/.spec/README.md(50): error SPEC4004: claim 'B-002' has no row in § 9 - every § 3 claim appears there exactly once, with 'Missing' as its test until one exists [B-002]
specifications: epics 0, features 1
items: 0
rules evaluated: 23
errors: 1, warnings: 0
```

Exit `1`. Add the row:

```markdown
| Claim ID | Scenario               | Test    | Status  |
| -------- | ---------------------- | ------- | ------- |
| B-001    | Total sums the lines   | Missing | Missing |
| B-002    | Empty cart totals zero | Missing | Missing |
```

```text
$ dotnet specht
specifications: epics 0, features 1
items: 0
rules evaluated: 23
errors: 0, warnings: 0
```

Exit `0`. `Missing` is allowed until the specification's `status` is `done`.

## 4. Add the analyzer

Reference the package in each project it checks. It reads `///` elements, so
the project must generate documentation:

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);CS1591</NoWarn>
</PropertyGroup>

<ItemGroup>
  <PackageReference Include="Specht.CodeAnalysis" Version="0.6.1-preview" PrivateAssets="all" />
</ItemGroup>
```

Without `GenerateDocumentationFile`, the compiler reads `///` as an ordinary
comment and the analyzer reports `SPCT0013` on each one.

Add `codeAnalysis` to `.specht/manifest.json`. This one requires an
`implements` link on every method in `src/checkout` and a `verifies` link on
every test method in `test/checkout.tests`:

```json
"codeAnalysis": {
  "mappings": {
    "/schemaVersion": "/schemaVersion",
    "/identifiers": "/identifiers",
    "/claimsSection": "/roles/claims",
    "/markers": "/markers",
    "/policy": "/codeAnalysis"
  },
  "projects": [
    { "path": "src/checkout/checkout.csproj", "targetFrameworks": ["net10.0"] },
    { "path": "test/checkout.tests/checkout.tests.csproj", "targetFrameworks": ["net10.0"] }
  ],
  "features": [{ "feature": "F1" }],
  "enforcement": [
    {
      "project": "src/checkout/checkout.csproj",
      "include": ["src/checkout/**/*.cs"],
      "exclude": [],
      "role": "implements",
      "declarationKinds": ["method"]
    },
    {
      "project": "test/checkout.tests/checkout.tests.csproj",
      "include": ["test/checkout.tests/**/*.cs"],
      "exclude": [],
      "role": "verifies",
      "declarationKinds": ["testMethod"]
    }
  ]
}
```

`exclude` is required, even when empty, and a `verifies` scope takes only
`testMethod`. Either mistake reports `SPCT0001` on the manifest.

## 5. Link the code

With no links, the build reports each selected method:

```text
src/checkout/Cart.cs(7,17): warning SPCT0007: M:Checkout.Cart.Add(System.Decimal) requires implements
src/checkout/Cart.cs(9,20): warning SPCT0007: M:Checkout.Cart.Total requires implements
```

Link each one to its claim:

```csharp
/// <implements feature="F1" claim="B-001" />
public void Add(decimal price) => _lines.Add(price);

/// <implements feature="F1" claim="B-001" />
/// <implements feature="F1" claim="B-003" />
public decimal Total() => _lines.Sum();
```

`B-003` is not in the specification:

```text
src/checkout/Cart.cs(11,34): warning SPCT0004: feature="F1" claim="B-003"
```

Change it to `B-002` and the build is clean.

## 6. Link the tests and collect evidence

Link one test, leave the other:

```csharp
/// <verifies feature="F1" claim="B-001" />
[Fact]
public void Total_sums_the_lines() { ... }

[Fact]
public void Empty_cart_totals_zero() => Assert.Equal(0, new Cart().Total());
```

The first build is provisional: with no evidence yet, it cannot tell a test
method from any other method, so it requires no `verifies`. Run the tests to
write the evidence, and supply it to the build:

```xml
<ItemGroup>
  <SpechtTestEvidence Include="obj/specht-test-evidence/*.json" />
</ItemGroup>
```

```sh
dotnet test test/checkout.tests --list-tests
dotnet test test/checkout.tests
dotnet build test/checkout.tests
```

```text
test/checkout.tests/CartTests.cs(18,17): warning SPCT0007: M:Checkout.Tests.CartTests.Empty_cart_totals_zero requires verifies
```

`specht evidence` reconciles the test results against the claims:

```text
$ dotnet specht evidence test/checkout.tests/obj/specht-test-evidence
claims: 2, covered: 1, exempt: 0
inputs: current 9, stale 0, unavailable 0
bindings: resolved 1, unresolved 0, invalid 0, ambiguous 0, ineligible 0
links: satisfied 1, missing 1, exempt 0, unavailable 0
passed: 2, failed: 0, skipped: 0, notRun: 0, aborted: 0, absent: 0
findings: 1
src/checkout/.spec/README.md: missingRequiredLink: Required test evidence is missing.
```

Exit `1`. Add `/// <verifies feature="F1" claim="B-002" />` to the second
test, then rerun both `dotnet test` commands. The source changed, so the old
evidence no longer applies.

```text
$ dotnet specht evidence test/checkout.tests/obj/specht-test-evidence
claims: 2, covered: 2, exempt: 0
inputs: current 9, stale 0, unavailable 0
bindings: resolved 2, unresolved 0, invalid 0, ambiguous 0, ineligible 0
links: satisfied 2, missing 0, exempt 0, unavailable 0
passed: 2, failed: 0, skipped: 0, notRun: 0, aborted: 0, absent: 0
findings: 0
```

Exit `0`. Without `--list-tests` first, the report has a `deferredCases`
finding and exits `2`.

## 7. Ask through MCP

```sh
dotnet tool install --local specht.mcp --prerelease
```

Client entry:

```json
{
  "mcpServers": {
    "specht": { "command": "dotnet", "args": ["specht-mcp", "--root", "."] }
  }
}
```

The server lists `check`, `explain`, `rules` and `manifest`. `check` returns
the same report as `dotnet specht --json`:

```json
{
  "schemaVersion": "0.3.1",
  "schemaSource": "embedded",
  "layouts": [
    { "layout": "epics", "specificationCount": 0 },
    { "layout": "features", "specificationCount": 1 }
  ],
  "itemCount": 0,
  "rulesEvaluated": 23,
  "errorCount": 0,
  "warningCount": 0,
  "violations": []
}
```

## Next

[Configuration](configuration.md), [Command line](cli.md),
[analyzer diagnostics](../src/codeanalysis/docs/README.md),
[native Gherkin claims](native-gherkin-claims.md), [MCP](mcp.md).
