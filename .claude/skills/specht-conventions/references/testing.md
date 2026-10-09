---
title: Testing in specht
description: The three test tiers, where tests live and why not beside the code, the tier trait guard, the Reqnroll wiring, and what a spec-tree fixture is
type: reference
---

# Testing

Extends [`test-from-scenarios`](../../test-from-scenarios/SKILL.md).

## Three tiers

| Tier        | Where                                                        | Selected by                     |
| ----------- | ------------------------------------------------------------ | ------------------------------- |
| Unit        | `test/specht.tests/**/<Thing>.Unit.Tests.cs`                 | `[Trait("Tier","Unit")]`        |
| Integration | `test/specht.tests/**/<Thing>.Integration.Tests.cs`          | `[Trait("Tier","Integration")]` |
| Acceptance  | `test/specht.acceptance`, Reqnroll over the `.feature` files | the project, no filter          |

`./build.sh UnitTest` and `IntegrationTest` run `--filter-trait "Tier=<X>"`,
xunit.v3's trait filter on Microsoft.Testing.Platform, over every project whose
name ends `.tests`, ignoring case. `AcceptanceTest` runs `test/specht.acceptance`
with no filter.

The line between the first two tiers is the file system: a unit test exercises
one decision, derivation or mapping in memory; an integration test touches the
disk or a process. In the engine, a unit test runs one rule or one reader over
documents built in memory, and an integration test runs `SpecCheckRunner` over
a `SpecTree` written to a temporary directory and asserts the report. The 56
tests extracted from `hooked` already draw it this way (brief § 3); follow them.

The tiers are defined by what is under test, not by where it lives. Code
outside `src/` - the build in `.build/` is the first - has the same unit tier,
in its own `*.Tests` project that references it. Until that project exists, its
mechanisms are `Partial` in § 9, never "no unit tier".

**A test through Spectre's command tester is Integration.** `CommandAppTester`
runs the parser, the type registrar, the settings binding and the command
together; however small the report it is handed, that is a composition of
components, not one unit. It goes in `<Command>.Integration.Tests.cs`. A unit
test of a command's decision needs the decision outside the command, which is
the implementer's design call, not a reason to label a pipeline test `Unit`.
There is no third, "UI", tier; one is the owner's to add.

**Reqnroll covers the acceptance tier only.** A green scenario does not relieve
the mechanism beneath it of unit coverage: a rule's decision, an id grammar, a
path relativization, a table-header match, an ordering, a file-set filter and an
argument a target builds each earn a unit test at the level they live at.

**Acceptance-only is the failure mode, not a tier choice.** Epic `0055` shipped
its build and CI with scenarios alone and a § 8 verdict that the build had "no
logic of its own", while `Build.cs` partitioned files by extension, skipped an
empty formatter and quoted arguments - [lesson 0003](../../../../.spec/lessons/0003-the-unit-tier-had-no-home.md).

## The stack

- **xunit.v3**, self-hosting on Microsoft.Testing Platform. There is no
  `Microsoft.NET.Test.Sdk` and no `xunit.runner.visualstudio`; `global.json` sets
  `"test": { "runner": "Microsoft.Testing.Platform" }`. `test/Directory.Build.props`
  sets `OutputType=Exe` explicitly because the package that used to set it is gone.
  `xunit.v3` is the product line; its package version is pinned in
  `Directory.Packages.props`.
- **AwesomeAssertions** for assertions — `.Should().Be(…)`. Not FluentAssertions,
  not Shouldly.
- **No mocking library.** A seam is a delegate or an injected interface. Reach
  for a delegate before reaching for a package.
- **`MockFileSystem`** from `TestableIO.System.IO.Abstractions.TestingHelpers`
  is the file-system fake for code that takes an injected
  `System.IO.Abstractions.IFileSystem` (the owner's direction, 2026-10-08; the
  manifest loader is the first, `0001-F5` § 8). It is a fake of an injected
  interface, not a mocking library. A test over it touches no disk, so it is
  `Unit`; give it repository-relative paths, as any fixture.
- **Rocket Surgery AutoFixtures** for test data —
  `Rocket.Surgery.Extensions.Testing.AutoFixtures`, a source generator. A value a
  test hands the code under test, such as a `SpecViolation` or a
  `SpecCheckReport`, comes from an `[AutoFixture(typeof(T))]` partial named
  `<T>Fixture`, not a hand-rolled `Report(...)` or `Violation(...)` helper. The
  generator handles a positional record. The fixture's constructor sets only the
  defaults that make the value valid, and a test overrides with `With*` only what
  it asserts on. Version 10.0.6 emits its `AutoFixtureBase` without a `#nullable`
  directive, so `specht.tests.csproj` carries `NoWarn` `CS8669`, the one
  diagnostic only generated code raises.
- `Microsoft.Testing.Extensions.CodeCoverage`; each test project writes one
  report, `.artifacts/coverage/<project>.coverage.cobertura.xml`, named by
  `TestingPlatformCommandLineArguments` in `test/Directory.Build.props`, and CI
  uploads the folder to Codecov. Keep `coverage` in the name: the uploader finds
  reports by its default name patterns, and a glob in its `files` input breaks
  on Windows (`0055-F3` § 7).

One behaviour worth carrying from `hooked`: an MTP test project that discovers
**zero** tests exits `8` and fails its tier. A new test project needs at least
one test per tier it runs in, or the build goes red for a reason the log does
not explain.

## Test naming

`Given_When_Should` as **one identifier**, with `// Given` / `// When` / `// Then`
comment blocks in the body:

```csharp
public void ClaimCitedTwiceInMatrix_WhenChecked_ShouldReportSpec031Once()
```

A § 9 row cites the test by name, so renaming it to suit an implementation breaks
the citation. Rename the claim's wording instead, or accept the name.

**Cases of one shape are one `[Theory]`.** Two facts that differ only in their
data — a warning with and without `--strict`, each exit code — are one theory
over `[MemberData]` or `[ClassData]`, a typed xunit.v3 `TheoryData<…>`, with the
name stating the behaviour every row shares. A helper one test class calls from
another is shared data or a fixture in the wrong place: a class owns its own
harness, and data two classes need is a `[ClassData]` type.

## Tests live in `test/`, not beside the code

Tests are **not** co-located with production code here. `src/specht` is one
assembly with one caller; a separate `test/specht.tests` project references it
directly and needs no `Compile Remove` / `Compile Include` dance to pull test
files back out of the product.

That dance is why co-location is not introduced casually: `hooked` once scoped
its re-include glob to one folder, and a `*.Tests.cs` dropped anywhere else was
removed from production by the unconditional exclude and never linked into the
test project — it _silently compiled nowhere and no test ran_, while the solution
built green and `dotnet test` reported success. A separate project cannot fail
that way. If co-location is ever proposed, that incident is the cost to weigh.

## The tier trait is enforced

`test/Shared/TestTierGovernanceTests.cs` (scaffold) is linked into every `*.Tests`
project by `test/Directory.Build.props` and reflects over the assembly, asserting
every class carrying a `[Fact]` or `[Theory]` declares exactly one
`[Trait("Tier","Unit"|"Integration")]`.

Without it, a class that forgets the trait drops out of **both** filtered tiers
while an unfiltered run still reports it passing. It is not linked into
`specht.acceptance` — Reqnroll-generated scenario classes do not use the
convention.

A new test project must end in `.Tests`, or it gets neither the tier filters nor
the guard.

## Reqnroll wiring, and why it is literal

`.feature` files live in `src/**/.spec/` beside the specification they belong
to, and are linked into `test/specht.acceptance` by a **literal
`ReqnrollFeatureFile` glob** with a `Link=` path, plus
`ReqnrollUseIntermediateOutputPathForCodeBehind=true`.

Two attempts in `hooked` to generalize such globs failed the same way: Reqnroll's
static metadata `ItemGroup Update` pass runs before any MSBuild `Target`, so
batched items produced zero features and `dotnet test` **silently discovered
zero tests**. Add a glob by writing it out, and verify the discovered test count
changed.

## Fixtures

A fixture is a specification tree, and it is **built, not copied**: `SpecTree`
in `test/specht.tests` constructs the files a test needs — frontmatter, sections,
tables, a companion `.feature`, child items — and writes them to a temporary
root the test owns. Every path inside a fixture, and every path in an expected
report, is relative to that root; an absolute path in either is the defect
brief § 9 names.

Never a copy of a real repository's `.spec/` tree, this one's included: a copied
tree carries every rule's happy path at once and pins the test to whatever that
tree happened to contain. No test touches the network, a live provider, or the
wall clock.

## Never add

- A test project whose name does not end `.Tests`.
- A test class with no `Tier` trait, or with both.
- A claim marked `Covered` by a scenario alone while the mechanism beneath it
  has no unit test, or a "no unit tier" verdict that names no code read.
- A `*.Tests.cs` file under `src/`.
- A batched or wildcarded `ReqnrollFeatureFile` glob.
- A mocking library, when a delegate seam would do.
- A hand-rolled file-system fake for code that takes an `IFileSystem`;
  `MockFileSystem` does it.
- A `Unit` trait on a test that runs a command through `CommandAppTester`.
- A hand-rolled factory method for test data an AutoFixture can build.
- Copied facts that differ only in their data, where a `[Theory]` would do.
- A test class calling another test class's static helper.
- `Thread.Sleep`, a retry loop, or an ambient clock read.
- A fixture copied from a real `.spec/` tree, or an absolute path in one.
