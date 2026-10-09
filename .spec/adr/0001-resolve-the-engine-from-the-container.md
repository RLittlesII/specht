---
title: "ADR-0001: Resolve the engine from the container, behind interfaces only at its seams"
description: "The engine's static pipeline becomes instance classes resolved from Microsoft.Extensions.DependencyInjection, with an interface only where a substitution exists today - the file system, the rule set, discovery's two modes and the runner the command calls - so each stage can be tested without a real tree."
type: adr
---

# ADR-0001: Resolve the engine from the container, behind interfaces only at its seams

## Status

proposed - raised by the owner on pull request #13
([review comment](https://github.com/RLittlesII/specht/pull/13#discussion_r4226016028),
on `src/specht/SpecCheckRunner.cs`). Acceptance is the owner's.

## Context

The owner's comment: "All these classes are starting to feel like they should
be resolved from a container so they can be tested properly. We have access to
MSFT DI. We can inject interfaces."

The engine is `hooked`'s, copied and renamed (brief § 3, § 9), and it composes
itself with statics and `new`:

| Stage                                    | Today                                                                                                                                                                                      |
| ---------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `SpecCheckRunner`                        | `static class`. `Run(root)` calls `SpecModel.Load`, finds the rules by reflection and `Activator.CreateInstance`, collects and sorts. `WriteReport` writes with static `File`/`Directory`. |
| `SpecModel.Load`                         | `new FileSystem()` for the schemas, then static `SpecDiscovery`, `SpecDocument.Parse`, `FrontmatterReader.Read` and `Directory.EnumerateFiles`.                                            |
| `SpecDiscovery`                          | `static class` over static `Directory`/`File`.                                                                                                                                             |
| `FrontmatterReader`, `FeatureFileReader` | `static class`es over `File.ReadAllLines`.                                                                                                                                                 |
| `SpecDocument.Parse`                     | static factory over `File.ReadAllText`.                                                                                                                                                    |
| `SpecSchemas`, `SpecManifest`            | static `Load(IFileSystem, root)` - the one place `System.IO.Abstractions` is already threaded through (`0001-F5`, item `0011`).                                                            |
| `ISpecRule`                              | the one interface: eight implementations under `Rules/`, parameterless, stateless, `Evaluate(SpecModel)`.                                                                                  |

The host already uses the container. `Program.cs` builds a `ServiceCollection`
and hands it to Spectre through `TypeRegistrar`/`TypeResolver`
(`dotnet-tool` § Vertical Slice: a hand-written registration list, no assembly
scanning, no third-party container). The engine reaches the host as one
registration, `Func<string, SpecCheckReport>` bound to `SpecCheckRunner.Run`,
which `CheckCommand` takes by constructor and its tests replace with a lambda.
`Microsoft.Extensions.DependencyInjection` is in `Directory.Packages.props`
under `Cli` and referenced by `src/specht.tool` only; the engine references
JsonSchema.Net, YamlDotNet, Markdig and the `System.IO.Abstractions` wrappers,
and the test project already references its `TestingHelpers`
(`MockFileSystem`, used by `SpecManifest.Unit.Tests.cs`).

What that makes hard to test, as `0001-F3` § 8 reports to the implementer:

- Every stage below the manifest reads the real disk, so every engine test
  above a single rule builds a temporary tree (`SpecTree`), and no stage can
  be unit-tested alone.
- The engine takes no clock and no environment input. `0001-F3` B-006 is
  proved by inspecting content, and its two scenarios can vary the clock, the
  user and the machine name only for a launched tool (item `0035`).
- The root-relative mapping `0001-F3` B-021 rests on was three private copies;
  it is now one public member, `SpecDiscovery.Relative` (`0001-F3` § 7), but a
  static one.

Seams that exist or are cut and waiting:

- **The file system** - `MockFileSystem` in tests today; `IFileSystem` already
  enters through `SpecManifest`/`SpecSchemas`.
- **The rule set** - eight `ISpecRule`s today; per-rule disable and severity
  (`0001-F5` B-010, B-011, item `0014`) act on it.
- **Discovery's two modes** - git-backed and pruned walk (`0001-F6` B-005,
  B-006, items `0007`, `0008`), one contract (`0001-F6` C-3, B-007).
- **The runner the command calls** - a test double in
  `CheckCommand.Integration.Tests.cs` today.

What any change here must preserve: no verdict changes (`0001-F1` C-9), the
manifest is accepted whole before any rule runs (`0001-F5` C-5), no rule is
loaded from anything but the pinned vocabulary (`0001-F5` C-3, B-017; decision
`0001-F2` 0001), and the engine is changed under a test, never rewritten
(brief § 9).

## Decision drivers

1. A stage can be tested alone, without a tree on disk.
2. No verdict, order or message changes (`0001-F1` C-9).
3. An interface only where a substitution exists today
   (`coding-conventions` § Design); a reader with one implementation is
   substituted through the file system, not behind its own interface.
4. Extend what is in place: the `TypeRegistrar` bridge, plain
   `Microsoft.Extensions.DependencyInjection`, `IFileSystem`
   ([lesson 0002](../lessons/0002-extend-what-the-owner-put-in-place.md)).
5. The composition stays a readable, explicit list.

## Considered options

1. **Keep the statics; thread `IFileSystem` through as a parameter.** Each
   static `Read`/`Parse`/`Find` gains an `IFileSystem` argument, as
   `SpecManifest.Load` has. Cost: smallest diff; no container in the engine;
   tests substitute the file system but nothing else; the rule set stays
   reflection-built and cannot be substituted or re-graded from outside; every
   caller passes the file system by hand, and discovery's second mode lands as
   a branch inside a static class.
2. **Instance classes resolved from the container, an interface at every
   class.** `ISpecDiscovery`, `IFrontmatterReader`, `ISpecDocumentParser`,
   `ISpecModelLoader`, `ISpecCheckRunner`, ... Cost: eight or more interfaces
   with one implementation each - the `IFooService` shape `dotnet-tool`
   § Vertical Slice rules out; tests mock the readers instead of feeding them a
   file, which tests the mock; every signature lives twice.
3. **Instance classes resolved from the container, an interface only at the
   four seams above.** The readers and the model loader become sealed instance
   classes taking `IFileSystem` by constructor and are registered by their
   concrete type; `ISpecRule` is registered once per rule and injected as
   `IEnumerable<ISpecRule>`; discovery gets one interface when its second mode
   lands (items `0007`, `0008`), not before; the runner is an injected class
   whose `Run(root)` the host binds - the `Func<string, SpecCheckReport>`
   seam stays until a second operation earns an interface. The root stays a
   method argument: it is run data, not a service. Cost: every engine file
   changes shape, one stage per pull request under C-9; the engine gains
   `Microsoft.Extensions.DependencyInjection.Abstractions` (a new central
   version beside the `Cli` one); the call sites of `SpecCheckRunner.Run` in
   tests and step definitions (`SpecTree`, `CheckSteps`, `ManifestSteps`,
   `ReportSteps`, `CheckCommand.Integration.Tests.cs`) change to resolve it.
4. **Option 3 without the abstractions package**: the engine's constructors
   take their dependencies but the engine never names the container;
   `Program.cs` lists every engine registration itself. Cost: the list is
   written three times - `Program.cs`, `CheckSteps`, and the integration
   tests' composition - and drifts; a rule added to the assembly and missed in
   one copy runs in one place and not the other.

## Decision

Proposed: option 3.

- The engine exposes one `IServiceCollection` extension that lists every
  engine registration by hand - each rule by name, no scanning - and the host,
  the acceptance steps and the integration tests compose through it.
  `Program.cs` stays the composition root and calls it once.
- Rule discovery by reflection is replaced by that list. A unit test asserts
  every `ISpecRule` in the assembly is registered, so the "add a file and
  nothing else" property `ISpecRule`'s summary promises becomes "add a file and
  a line, or a test fails"; a new rule is a schema version anyway (brief § 4).
- Lifetimes: singletons; every registered type is stateless. `SpecModel` and
  `SpecSchemas` are built per run and never registered.
- `IFileSystem` is the substitution for every reader; no reader gets its own
  interface.
- No clock is registered: the engine reads none (`0001-F3` B-006). A clock or
  environment seam for B-006's scenarios belongs to the host (item `0035`).

## Consequences

Buys:

- A rule, a reader or discovery can be unit-tested against a `MockFileSystem`,
  and the runner against a fixed rule set, without a temporary tree.
- Per-rule settings (item `0014`) and discovery's second mode (items `0007`,
  `0008`) land as registrations and constructor arguments, not as branches in
  static classes.
- One composition list, shared by the host and every test that runs the real
  engine.

Costs:

- A shape change across the copied engine. It is done one stage per pull
  request, each with the baseline report reproduced (`0001-F1` B-004, C-9),
  and it collides with items `0007`, `0008`, `0010`, `0014` and `0016`, which
  edit the same files - sequence it before them or inside them, not beside.
- A new package reference in the engine and a new central version.
- `ISpecRule`'s summary and `dotnet-tool` § Vertical Slice ("each engine
  service explicitly" in `Program.cs`) are amended to name the engine's
  extension.
- `0001-F3` § 7 and the engine's § 7 (`0001-F1`) redraw the pipeline as
  resolved types.

Work on acceptance: items cut on acceptance, one per stage - file system and
readers; discovery; model loader and runner with the explicit rule list; host
and test composition - each citing this ADR and `0001-F1` C-9.
