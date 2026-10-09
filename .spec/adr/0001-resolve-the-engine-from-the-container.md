---
title: "ADR-0001: Resolve the engine from the container, behind interfaces only at its seams"
description: "The engine's model becomes constructible in memory first; then its static pipeline becomes instance classes registered by one hand-written AddSpechtEngine() extension and resolved from the container ADR-0003 chooses, with an interface only where a substitution exists today - the file system, the rule set, discovery's two modes and the runner the command calls. Staged behind the committed and guarded baseline report."
type: adr
---

# ADR-0001: Resolve the engine from the container, behind interfaces only at its seams

## Status

accepted - 2026-10-08, by the owner. Raised by the owner on pull request #13
([review comment](https://github.com/RLittlesII/specht/pull/13#discussion_r4226016028),
on `src/specht/SpecCheckRunner.cs`), and revised against the-architect's
assessment on pull request #20.

Partially superseded by [ADR-0004](0004-per-rule-settings-are-selection-and-a-map.md), 2026-10-09: the Consequences cost bullet on item `0014` (per-rule settings are selection and a severity map inside `Evaluate`, not one step over collected violations). Stage D's shape is made concrete by [ADR-0005](0005-typed-injected-stages-not-a-chain.md). The rest stands.

Amended 2026-10-09, by the owner on pull request #29 ([review comment](https://github.com/RLittlesII/specht/pull/29#discussion_r4231043583), on `src/specht/FrontmatterReader.cs`): `FrontmatterReader` moves from stage D to stage A, as a sealed instance class taking `IFileSystem`, in item `0104`.

## Context

The owner's comment: "All these classes are starting to feel like they should
be resolved from a container so they can be tested properly. We have access to
MSFT DI. We can inject interfaces."

The engine is `hooked`'s, copied and renamed (brief § 3, § 9), and it composes
itself with statics and `new`:

| Stage                                    | Today                                                                                                                                                                                                                                                                                                                   |
| ---------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `SpecCheckRunner`                        | `static class`. `Run(root)` calls `SpecModel.Load`, finds the rules by reflection and `Activator.CreateInstance` (`SpecCheckRunner.cs:52-58`), evaluates, collects and sorts. `WriteReport` writes with static `File`/`Directory`; its body is `0001-F3`'s (§ 7, `Report/.spec/README.md:175`) and no command calls it. |
| `SpecModel.Load`                         | `new FileSystem()` for the schemas (`SpecModel.cs:47`), then static `SpecDiscovery`, `SpecDocument.Parse`, `FrontmatterReader.Read`, `Directory.EnumerateFiles` for the `.feature` files (`:56`), `Directory.Exists` for `epics/` (`:73`) and a second `Directory.EnumerateFiles` for `epic.md` (`:75`).                |
| `SpecDiscovery`                          | `static class` over static `Directory`/`File`.                                                                                                                                                                                                                                                                          |
| `FrontmatterReader`, `FeatureFileReader` | `static class`es over `File.ReadAllLines`.                                                                                                                                                                                                                                                                              |
| `SpecDocument.Parse`                     | static factory over `File.ReadAllText`, taking `(absolutePath, relativePath)`.                                                                                                                                                                                                                                          |
| `SpecSchemas`, `SpecManifest`            | static `Load(IFileSystem, root)` - the one place `System.IO.Abstractions` is already threaded through (`0001-F5`, item `0011`).                                                                                                                                                                                         |
| `ISpecRule`                              | the one interface: eight implementations under `Rules/`, parameterless, stateless, `Evaluate(SpecModel)`. `FeatureFileRule` reads the disk during `Evaluate`: `FeatureFileReader.ReadTags(path)` (`Rules/FeatureFileRule.cs:41`).                                                                                       |

**The container is not what blocks testability; constructor visibility is.**
A rule's only input is a `SpecModel`, and no test outside the engine can build
one: `SpecModel`'s constructor is private (`SpecModel.cs:8`), so is
`SpecDocument`'s (`SpecDocument.cs:19`), `FeatureSpec`'s and `ChildItem`'s
are `internal` (`FeatureSpec.cs:8`, `ChildItem.cs:6`), and no assembly carries
`InternalsVisibleTo`. The only way to a model is `SpecModel.Load` over a tree
on disk. No container change alone makes a rule or the runner unit-testable:
resolving `SpecCheckRunner` from a provider still hands it a model only the
disk can produce.

The measured cost: `test/specht.tests/SpecCheckRunner.Unit.Tests.cs` (24
facts) and `SpecCheckRunner.Violations.Unit.Tests.cs` (26 facts) are tagged
`Tier=Unit`, and every one of those 50 facts writes a temporary tree through
`SpecTree`. `specht-conventions` `references/testing.md:23-27` puts that on
the integration side of the line - "an integration test runs
`SpecCheckRunner` over a `SpecTree` written to a temporary directory". The
tiers are wrong because the model cannot be built in memory.

The host already uses the container, and which container is ADR-0003's
decision: plain `Microsoft.Extensions.DependencyInjection` behind
`TypeRegistrar`, a single provider call site, a single-use collection,
singletons, run data as arguments, and disposal by the provider. The engine
reaches the host as one registration, `Func<string, SpecCheckReport>` bound to
`SpecCheckRunner.Run`, which `CheckCommand` takes by constructor and its tests
replace with a lambda. The engine references JsonSchema.Net, YamlDotNet,
Markdig and the `System.IO.Abstractions` wrappers, and none of the container.

What the specifications report as hard to test:

- `0001-F5` § 8 (`Manifest/.spec/README.md:169`): the runner finds its rules
  by reflection and takes none injected, so "no rule ran" is observable only
  as "no report"; and the runner takes no `IFileSystem`, so B-018's ordering
  has no unit-tier proof.
- `0001-F3` § 8 (`Report/.spec/README.md:197`): the engine takes no clock and
  no environment input, so `0001-F3` B-006's scenarios can vary the clock, the
  user and the machine name only for a launched tool (item `0035`).
- The root-relative mapping `0001-F3` B-021 rests on was three private copies;
  it is now one public member, `SpecDiscovery.Relative` (`0001-F3` § 7,
  `Report/.spec/README.md:177`), unit-tested as a static (§ 8, `:189`).

The position this record supersedes: `0001-F5` § 6 and § 7
(`Manifest/.spec/README.md:129`, `:147-149`) put `IFileSystem` in
`SpecManifest.Load` and `SpecSchemas.Load` only, and defer threading it
through the runner, the model, discovery and the readers because it is "a
change across the copied engine with no claim asking for it". Accepting this
record is that ask; the § 7 text is owed an amendment.

Seams that exist or are cut and waiting:

- **The file system** - `MockFileSystem` in tests today; `IFileSystem` already
  enters through `SpecManifest`/`SpecSchemas`.
- **The rule set** - eight `ISpecRule`s today; per-rule disable and severity
  (`0001-F5` B-010, B-011, item `0014`) act on their output.
- **Discovery's two modes** - git-backed and pruned walk (`0001-F6` B-005,
  B-006, items `0007`, `0008`), one contract (`0001-F6` C-3, B-007). Git mode
  runs a child process (`0001-F6` decision 0002, git is an executable), which
  `IFileSystem` does not cover; what happens when `git ls-files` fails is
  `0001-F6` OQ-2.
- **The runner the command calls** - a test double in
  `CheckCommand.Integration.Tests.cs` today.

What any change here must preserve: no verdict changes (`0001-F1` C-9), the
manifest is accepted whole before any rule runs (`0001-F5` C-5), no rule is
loaded from anything but the pinned vocabulary (`0001-F1` C-5, `0001-F5` C-3,
B-017; decision `0001-F2` 0001), and the engine is changed under a test, never
rewritten (brief § 9). The proof C-9 names exists: `0001-F1` B-004 compares
the engine's verdicts on a synthetic baseline tree
(`test/specht.tests/Baseline/BaselineTree.cs`, `0001-F1` C-10) with the golden
report the engine gave on it at commit `e7dba24`
(`test/specht.tests/Baseline/engine-e7dba24.json`; `0001-F1` decision 0004),
in `SpecCheckRunner.Baseline.Integration.Tests.cs`. Item `0022` delivered it
and is done. Guarding the same verdicts under the default manifest
(`0001-F5` B-016) is item `0012`, still open; and the order the report is
sorted in has no unit-tier proof, so B-004 is `Partial` in `0001-F1` § 9
(`src/specht/.spec/README.md:153`) until item `0103` exposes it.

And a constraint the decision collides with: `0001-F1` C-7
(`src/specht/.spec/README.md:85`) names the engine's dependencies as Markdig,
YamlDotNet and JsonSchema.Net; four ship today (with the
`System.IO.Abstractions` wrappers, `specht-conventions`
`references/coding.md:76-78`), and this record adds a fifth.

## Decision drivers

1. A stage can be tested alone, without a tree on disk.
2. No verdict, order or message changes (`0001-F1` C-9), proven against the
   committed baseline, not argued.
3. An interface only where a substitution exists today
   (`coding-conventions` § Design); a reader with one implementation is
   substituted through the file system, not behind its own interface.
4. Extend what is in place: the container ADR-0003 records, `IFileSystem`
   ([lesson 0002](../lessons/0002-extend-what-the-owner-put-in-place.md)).
5. The composition stays a readable, explicit list, with one entry point.

## Considered options

The do-nothing baseline is ADR-0003 option 3: the host composes from the
container and the engine stays static behind the `Func<string,
SpecCheckReport>` seam. It is what holds if this record is rejected.

1. **Keep the statics; thread `IFileSystem` through as a parameter.** Each
   static `Read`/`Parse`/`Find` gains an `IFileSystem` argument, as
   `SpecManifest.Load` has. Cost: smallest diff; no container in the engine;
   tests substitute the file system but nothing else; the model stays
   unconstructible, so rules are still tested through a tree; the rule set
   stays reflection-built and cannot be substituted from outside; discovery's
   second mode lands as a branch inside a static class.
2. **Instance classes resolved from the container, an interface at every
   class.** `ISpecDiscovery`, `IFrontmatterReader`, `ISpecDocumentParser`,
   `ISpecModelLoader`, `ISpecCheckRunner`, ... Cost: eight or more interfaces
   with one implementation each - the `IFooService` shape `dotnet-tool`
   § Vertical Slice rules out; tests mock the readers instead of feeding them
   a file, which tests the mock; every signature lives twice; and the model
   is still unconstructible.
3. **Constructible model, then instance classes resolved from the container,
   an interface only at the four seams above.** The model types get public
   constructors; the runner splits into evaluation over a model and a run over
   a root; the rule set becomes a hand-written list; then the readers and the
   loader become sealed instance classes taking `IFileSystem` by constructor,
   registered by concrete type through one `AddSpechtEngine()` extension.
   Cost: every engine file changes shape, one stage per pull request under
   C-9; the engine gains `Microsoft.Extensions.DependencyInjection.Abstractions`,
   a fifth dependency against `0001-F1` C-7; the call sites of
   `SpecCheckRunner.Run` in tests and step definitions change.
4. **Option 3 without the abstractions package**: the engine's constructors
   take their dependencies but the engine never names the container;
   `Program.cs` lists every engine registration itself. Cost: the list is
   written in the host and again in every test that composes the real engine,
   and drifts; a rule added to the assembly and missed in one copy runs in one
   place and not the other.
5. **Make the model constructible; keep the composition static.** Public
   constructors on the model, `SpecDocument.Parse` over text, the runner split
   into `Evaluate(SpecModel)` and `Run(root)`, all still static. Pro: the
   strongest missing option - it fixes what actually blocks unit tests (the
   Context's constructor finding) with no package and no container, and it
   re-tiers the 50 facts on its own. Cost: the readers and discovery still
   read the disk through statics, so the loader has no unit-tier proof and
   discovery's second mode still lands as a branch. Its parts become stages A
   and B of the decision rather than the whole answer.
6. **The engine exposes the rule list as data; the host registers it.** A
   static `SpecRules.All` (or equivalent) lists every rule; the host and the
   tests register its members, and the engine takes no package. Pro: no fifth
   dependency, `0001-F1` C-7 stands unamended. Cost: rejected - the rules are
   one of several registrations (readers, loader, discovery, runner), so the
   host still writes the rest of the list itself (option 4's drift); the owner
   chose one composition entry point, `AddSpechtEngine()`, and accepts the C-7
   amendment it costs (owner decision, 2026-10-08).

## Decision

Proposed: option 3, in this order.

- **The model is constructible first.** `SpecModel`, `SpecDocument`,
  `FeatureSpec` and `ChildItem` get public constructors or named factories, so
  a test builds a model in memory. `SpecDocument` parses from text:
  `SpecDocument.Parse(text, relativePath)`. The current
  `Parse(absolutePath, relativePath)` has the same parameter types, so it is
  replaced, not overloaded: the loader reads the text and passes it in.
  `InternalsVisibleTo` is rejected: it makes the engine testable only from
  this repository's test assembly, keeps the model unbuildable for anyone
  else, and hides a public type's construction behind an assembly attribute
  (owner decision, 2026-10-08).
- **The runner splits** into `Evaluate(SpecModel)` - every rule, collect,
  sort, count - and `Run(root)`, which loads and evaluates. An
  `ISpecModelLoader` interface is taken only if that split is refused.
- **Rule discovery by reflection is replaced** by a hand-written list
  (`SpecCheckRunner.cs:52-58`). A unit test asserts every `ISpecRule` in the
  assembly is registered, so "add a file and nothing else" becomes "add a file
  and a line, or a test fails"; a new rule is a schema version anyway
  (brief § 4).
- **One composition entry point.** The engine exposes one
  `IServiceCollection` extension, `AddSpechtEngine()`, that lists every engine
  registration by hand - each rule by name, no scanning. `Program.cs` stays
  the composition root and calls it once; removing the per-service engine list
  from `Program.cs` (`dotnet-tool` `references/vertical-slice.md:52-58`) was
  asked for and approved by the owner (2026-10-08), as lesson 0002 requires
  before changing what the owner put in place. Nothing else in
  `src/specht.tool` changes. How tests compose the host is item `0099`'s
  decision (`.issue/0099-command-test-harness.yml`), not this record's.
- **The package.** The engine references
  `Microsoft.Extensions.DependencyInjection.Abstractions` and nothing else of
  the container. Its version is pinned in the `Engine` item group of
  `Directory.Packages.props`, on ADR-0003's two conditions: it moves with the
  implementation package's version, and transitive pinning applies it
  graph-wide.
- **`IFileSystem` is the substitution for every reader**, the parser's input,
  the walk and the loader; no reader gets its own interface.
- **Discovery gets one interface, `ISpecDiscovery`, and a git-listing seam**
  when its second mode lands, inside items `0007` and `0008`, not before. The
  git-listing seam exists because git mode runs a child process
  (`0001-F6` B-005; decision 0002), which `IFileSystem` does not cover, and
  because `0001-F6` OQ-2's failure path needs a test that can make
  `git ls-files` fail.
- **The host half is ADR-0003's**: the container, the single provider call
  site, the single-use collection, singleton host registrations, run data as
  arguments, disposal by the provider. This record does not restate it.
- **Lifetimes.** Engine registrations are singletons; every registered type is
  stateless. `SpecModel` and `SpecSchemas` are built per run and never
  registered, and the root stays a method argument. Two process-wide statics
  remain shared state regardless of lifetime: `SpecManifest.Defaults`
  (`SpecManifest.cs:83`, the embedded default manifest, read once) and
  `SpecSchemas.Options` (`SpecSchemas.cs:26`). `SpecSchemas.Load` keeps its
  per-load `SchemaRegistry` (`SpecSchemas.cs:45-56`), so two loads in one
  process do not collide. No engine type is `IDisposable`; that changes if git
  mode holds a `Process`, and the registration then follows ADR-0003's
  disposal rule.
- **The statics that stay**, because each is a pure function or a constant
  with no I/O to substitute:
  - `SpecDiscovery.Relative` - the one root-relative mapping;
    `SpecDiscovery.Unit.Tests.cs:30` and `0001-F3` § 8 call it as a static.
  - `SpecDiscovery.IsItemFileName` and `SpecDiscovery.ExcludedDirectories` -
    a name predicate and a constant list.
  - `SpecManifest.RelativePath` - a constant.
  - `SpecDocument.Pipeline` - the one immutable Markdig pipeline.
  - `FeatureFileReader.ClaimTag` - a compiled regular expression.
  - `SpecReportDocument.From` - a mapping from a report to its document.
- **No clock is registered**: the engine reads none (`0001-F3` B-006). A clock
  or environment seam for B-006's scenarios belongs to the host (item `0035`).

## Consequences

Buys:

- A rule, a reader or discovery can be unit-tested against an in-memory model
  or a `MockFileSystem`, and the runner's ordering and counts against a fixed
  set of fake rules, without a temporary tree; the 50 mis-tiered facts move to
  `Integration`.
- Discovery's second mode (items `0007`, `0008`) lands as a registration and a
  constructor argument, not a branch in a static class.
- One composition list, `AddSpechtEngine()`, shared by the host and every test
  that runs the real engine.
- The explicit rule list strengthens `0001-F1` C-5 and `0001-F5` C-3, B-017:
  the rules that run are the ones named, not whatever the assembly holds.
  `ISpecRule.cs:4-6`'s summary ("discovered by reflection over this assembly,
  so adding a rule means adding a file and nothing else") contradicts those
  constraints today and is rewritten.

Costs:

- A shape change across the copied engine, one stage per pull request, each
  proven against the committed baseline. **The proof (`0001-F1` C-9) is the
  baseline report and its guard: the migration is blocked on the baseline
  being committed (item `0022`, done: the golden report and its test) and
  guarded under the default manifest (item `0012`, open). No substitute proof
  is accepted** (owner decision, 2026-10-08).
- Per-rule settings (item `0014`) do not become decorators: per ADR-0002
  (`0002-no-chain-of-responsibility-for-the-check.md:99-100`), disable and
  re-grade are one step in the runner over the collected violations, and a
  decorator around `ISpecRule` stays deferred until ADR-0002 names it earned.
- A fifth engine dependency and a new central version.
- The rules and records this record amends, each owed on acceptance and none
  edited by it:
  - `0001-F1` C-7 (`src/specht/.spec/README.md:85`) and § 7's "the three C-7
    names" (`:124`), with a `0001-F1` § 10 delta recording why.
  - `specht-conventions` `references/coding.md:76-78` (the engine's
    dependencies) and `:85` ("migrate as later items touch them, not in a
    sweep") - acceptance authorizes this staged migration as the exception.
  - `dotnet-tool` `references/vertical-slice.md:52-58` ("each engine service
    explicitly" in `Program.cs`) names `AddSpechtEngine()`.
  - `0001-F5` § 7 (`Manifest/.spec/README.md:147-149`) and § 6 (`:129`), the
    deferral this record supersedes.
  - `ISpecRule.cs:4-6`'s summary.
  - `0001-F1` § 9 B-004 and B-010 (`src/specht/.spec/README.md:153`, `:159`)
    name `SpecCheckRunnerViolationsUnitTests`, which stage D re-tiers.
  - `0001-F3` § 7 and `0001-F1` § 7 redraw the pipeline as resolved types.
- It collides with items `0007`, `0008`, `0010`, `0014` and `0016`, which edit
  the same files; the edges below sequence it.

Work on acceptance: items cut on acceptance, one per stage, each citing this
ADR and `0001-F1` C-9.

| Stage | What                                                                                                                                                                                                                                                               | Proof                                                                                                                                                                                                                                            | Edges                                                                                                                    |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------ |
| 0     | Item `0012`: the baseline verdicts guarded under the default manifest (`0001-F5` B-016). Item `0022`'s half - the synthetic tree and the golden report, committed and tested - is done.                                                                            | `0012`'s own claim, against the golden report `0022` committed.                                                                                                                                                                                  | Every later stage depends on `0012`.                                                                                     |
| A     | Constructibility: public constructors or factories on `SpecModel`, `SpecDocument`, `FeatureSpec`, `ChildItem`; `SpecDocument.Parse(text, relativePath)`; `FrontmatterReader` a sealed instance class taking `IFileSystem`. No DI, no package.                      | New in-memory unit tests; `MockFileSystem` unit tests for `FrontmatterReader`; the 50 `SpecTree` facts stay green; `SpecCheckRunner.Baseline.Integration.Tests.cs` green against `engine-e7dba24.json`, field for field, in order; `0012` green. | Depends on `0012`.                                                                                                       |
| B     | Split `SpecCheckRunner` into `Evaluate(SpecModel)` and `Run(root)`, still static, building on the order function item `0103` exposes rather than a second one.                                                                                                     | The golden-report test and `0012` green; `0103`'s ordering unit test, reused; a unit test of counts and `ReportedIds` with fake rules.                                                                                                           | Depends on A and on item `0103`. Item `0014` depends on B.                                                               |
| C     | The hand-written rule list replaces reflection (`SpecCheckRunner.cs:52-58`), with the assembly-coverage test.                                                                                                                                                      | The golden-report test and `0012` green; the rule-id set and the `ReportedIds` sum unchanged.                                                                                                                                                    | Depends on B.                                                                                                            |
| D     | Instance classes; `IFileSystem` through the remaining readers, the parser's caller, the walk and the loader; `AddSpechtEngine()`; host composition; the two `SpecCheckRunner` `Unit` files re-tiered to `Integration`, with the `0001-F1` § 9 rows that name them. | The golden-report test and `0012` green; `MockFileSystem` unit tests per reader and the loader.                                                                                                                                                  | Depends on C, on item `0099`, and on the `0001-F1` C-7 amendment. Lands before `0016`. Items `0007`, `0008` depend on D. |
| E     | `ISpecDiscovery` and the git-listing seam, inside items `0007` and `0008`.                                                                                                                                                                                         | Item `0009`'s mode parity (`0001-F6` B-007, C-3); the golden-report test green.                                                                                                                                                                  | Depends on D.                                                                                                            |
