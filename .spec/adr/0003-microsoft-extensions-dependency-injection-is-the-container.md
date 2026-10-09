---
title: "ADR-0003: Microsoft.Extensions.DependencyInjection is the container"
description: "The host composes through plain Microsoft.Extensions.DependencyInjection behind Spectre's TypeRegistrar: a hand-written service list, one call site for the provider, no assembly scanning, no generic host, no third-party container and no compile-time generator; whether the engine composes from it is ADR-0001's question."
type: adr
---

# ADR-0003: Microsoft.Extensions.DependencyInjection is the container

## Status

accepted - 2026-10-08, by the owner. Asked for by the owner on 2026-10-08
("There should be an ADR for using MSFT DI"), as a record separate from
ADR-0001, and proposed on pull request #18.

## Context

ADR-0001 decides what the engine resolves from the container, and ADR-0002
leans on it for the rule-set seam. Neither decides which container, or why.
The only statement is a skill rule, `dotnet-tool` § Vertical Slice: "plain
`Microsoft.Extensions.DependencyInjection` through Spectre's `TypeRegistrar`
... no assembly scanning and no third-party container". A rule with no
decision behind it is reopened by the next reader who prefers another
container.

This record decides the host's container. Whether the engine composes from it
is ADR-0001's question, and this record stands whether ADR-0001 is accepted or
rejected.

What is in place, since item `0026` (pull request #4, commit `81c66bb`):

| Where                                                            | Today                                                                                                                                                                                                                                                |
| ---------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `src/specht.tool/Program.cs`                                     | `new ServiceCollection()`, one registration - `Func<string, SpecCheckReport>` bound to `SpecCheckRunner.Run` - then `new CommandApp(new TypeRegistrar(services))` and `SetDefaultCommand<CheckCommand>()`.                                           |
| `src/specht.tool/TypeRegistrar.cs`                               | Spectre's `ITypeRegistrar` over `IServiceCollection`. `Register`, `RegisterInstance` and `RegisterLazy` all add singletons; `Build` is the one call site of `BuildServiceProvider()`.                                                                |
| `src/specht.tool/TypeResolver.cs`                                | Spectre's `ITypeResolver` over the built `IServiceProvider`: `GetService`, and disposes the provider.                                                                                                                                                |
| `Features/Check/CheckCommand.cs`                                 | Takes `IAnsiConsole` and `Func<string, SpecCheckReport>` by constructor. It is not listed in `Program.cs`: Spectre registers the command type, its settings, its configuration and the console through the registrar when it runs.                   |
| `src/specht/SpecCheckRunner.cs:52-58`                            | The engine composes itself: `Discover()` finds every `ISpecRule` by reflection over the assembly and builds each with `Activator.CreateInstance`. No container.                                                                                      |
| `Directory.Packages.props`                                       | `Microsoft.Extensions.DependencyInjection` `10.0.12` in the `Cli` item group, its only version. No `.Abstractions` version; the `Engine` item group holds the engine's four. `CentralPackageTransitivePinningEnabled` is `true`.                     |
| `src/specht.tool/specht.tool.csproj`, `src/specht/specht.csproj` | The host references the package; the engine references none of it.                                                                                                                                                                                   |
| Tests                                                            | `CheckCommand.Integration.Tests.cs` registers a runner - a lambda or the real one - in its own `ServiceCollection`. `test/specht.acceptance/Check/CheckSteps.cs:72-90` launches the built tool, so it is the one test that runs `Program.cs`'s root. |

Spectre.Console.Cli takes no container. It constructs commands through the
`ITypeRegistrar`/`ITypeResolver` pair (`spectre-cli` § Dependency Injection),
so the host carries either a container behind that pair or a hand-written
resolver that does the container's job for each constructor.

The claims at stake:

- `0001-F2` A-1: `check` is the default command; `init` and `upgrade` are
  sibling commands, each one more constructor to resolve.
- `0001-F2` C-2: exit codes are returned, never thrown; an exception crossing
  the command boundary to Spectre's `-1` is the violation.
- `0001-F2` C-4: a command parses, calls the runner and folds; composition
  does not live in a command.
- `0001-F2` C-7: the command is tested through Spectre's command tester against
  synthetic trees.
- `0001-F1` C-3 (and B-006): the verdict is a function of the tree and the
  schema files; no environment variable, clock or machine name reaches it
  (brief § 9).

## Decision drivers

1. First-party and already in place: a package the repository pins once and
   the host already runs on, extended rather than replaced
   ([lesson 0002](../lessons/0002-extend-what-the-owner-put-in-place.md)).
2. The smallest dependency surface in the packed tool, and nothing in the
   composition that reads configuration from the machine (`0001-F1` C-3).
3. Testable through Spectre's command tester (`0001-F2` C-7), with a fake
   registered in place of a real service.
4. Explicit registration of services: the host's service list is one a
   reader can follow, and no service joins it by reflection over the
   assembly. Command and settings types are outside that list: Spectre
   registers them through the registrar and binds them by reflection, whatever
   the container.

## Considered options

1. **Microsoft.Extensions.DependencyInjection with a hand-written composition
   root.** What runs today. Pro: first-party, already pinned and referenced,
   the container every .NET library's `IServiceCollection` extension targets,
   and the one `spectre-cli` documents; the tests already compose through it.
   Con: resolution errors - a missing registration, a captive dependency -
   surface at run time, not at compile time; it is a package where option 2
   has none.
2. **Microsoft.Extensions.Hosting (`HostApplicationBuilder`), or a community
   Spectre-to-DI bridge package.** Pro: the generic host's familiar shape -
   logging, options, configuration - and a bridge someone else maintains.
   Con: rejected. The host's configuration providers read environment
   variables and `appsettings` files by default, which is a path for the
   machine to reach the verdict that `0001-F1` C-3 and B-006 forbid; it adds
   the hosting, configuration, logging and options packages to the packed
   tool for a host with one registration; a bridge package replaces two
   twenty-line classes with a dependency.
3. **Host-only Microsoft.Extensions.DependencyInjection, the engine kept
   static behind the `Func<string, SpecCheckReport>` seam for good.** The
   status quo below the host. Pro: no engine change; the host's choice is the
   same as option 1. Con: the engine stays untestable stage by stage, and its
   rule set stays reflection-built - the problem ADR-0001 is raised to solve.
   This is the live alternative if ADR-0001 is rejected, and this record's
   decision does not change under it.
4. **No container: wire by hand in `Program.cs` with a custom
   `ITypeResolver`.** The resolver switches on the requested type and
   constructs it. Pro: no package; every construction is visible and
   compile-checked. Con: the resolver is a hand-written container that grows
   a branch per command and per dependency, and Spectre also asks it for
   settings types and its own internals; a test needs a second resolver to
   substitute the runner; ADR-0001's engine registration list has nothing to
   register into.
5. **An `ActivatorUtilities`-only resolver.** The strongest form of "no
   container": a custom `ITypeResolver` that builds any requested type with
   `ActivatorUtilities.CreateInstance` over a small dictionary of instances.
   Pro: no branch per type; constructors resolve by signature. Con:
   `ActivatorUtilities` ships in the abstractions package, so the dependency
   is barely smaller; it has no lifetimes, no `IEnumerable<T>` of
   registrations (ADR-0001's rule set), and no disposal, each of which the
   resolver would grow by hand into option 1 without its tests.
6. **A third-party runtime container (Autofac, DryIoc).** Pro: richer
   features - decorators, modules, scanning, child scopes - and earlier
   diagnostics in some. Con: a new package and a new set of rules for a host
   with one registration; the features it adds are ones this repository rules
   out (scanning) or defers (decorators, ADR-0002); nothing here needs what
   option 1 lacks.
7. **A compile-time generator (Pure.DI, Jab).** Pro: a missing registration
   is a build error; no reflection at resolve time. Con: a source generator in
   the build and a second composition model beside `IServiceCollection`;
   Spectre still asks for types at run time, so a generated container still
   sits behind a `TypeResolver`, and Spectre binds commands and settings by
   reflection regardless; a library's `IServiceCollection` extension method is
   not consumed as written and its registrations are re-declared in the
   generator's model.
8. **Microsoft.Extensions.DependencyInjection with Scrutor assembly
   scanning.** Pro: a new service or rule registers itself; no list to keep.
   Con: a type joins the composition by being in the assembly, which is what
   the pinned rule vocabulary forbids for rules (`0001-F5` C-3, B-017;
   decision `0001-F2` 0001) and what ADR-0001 replaces with an explicit list;
   reflection over the assembly at startup; one more package. ADR-0002's
   earned move is decoration around `ISpecRule`, and that is done in plain
   Microsoft.Extensions.DependencyInjection by registering the decorators by
   hand; this option is reconsidered only if ADR-0002 option 3 is earned and
   hand registration proves too costly.

## Decision

Proposed: option 1, for the host's composition.

Unconditionally:

- The host's container is plain `Microsoft.Extensions.DependencyInjection`,
  bridged to Spectre by `TypeRegistrar` and `TypeResolver` in
  `src/specht.tool`. Only `specht.tool` references the implementation
  package.
- There is exactly one call site of `BuildServiceProvider()`: inside
  `TypeRegistrar.Build`. It runs once per `CommandApp` execution and once per
  composing test; nothing else builds a provider.
- A `ServiceCollection` handed to a `TypeRegistrar` is single-use: on each run
  Spectre registers its configuration, every command and settings type and
  the console into it through the registrar, so a second `CommandApp` over the
  same collection inherits the first one's registrations.
- Services are registered by hand in `Program.cs`. No assembly scanning, no
  generic host, no third-party container, no compile-time generator.
- Host registrations are singletons, and run data - the root, the settings'
  values - passes as arguments, never as a registration.
- The provider disposes what it creates, including the objects
  `RegisterLazy` builds; the objects Spectre hands to `RegisterInstance`
  belong to Spectre.
- Tests satisfy `0001-F2` C-7: the command runs through Spectre's command
  tester. How a test composes the registrar and the tester is item `0099`'s
  decision (`.issue/0099-command-test-harness.yml`, cut because the owner
  rejected the current wiring on pull request #4), not this record's.

If ADR-0001 is accepted:

- The engine may reference `Microsoft.Extensions.DependencyInjection.Abstractions`
  and nothing else of the container, for the `IServiceCollection` extension
  ADR-0001 gives it. Its version will be pinned in the `Engine` item group of
  `Directory.Packages.props`, not the `Cli` one, on two conditions: it moves
  with the implementation package's version, and
  `CentralPackageTransitivePinningEnabled` applies that pin across the whole
  graph, the host's transitive copy included.
- The engine's lifetimes are the ones ADR-0001 sets.

## Consequences

Buys:

- No new package and no new pattern for the host: `Program.cs` and the tests
  already compose this way, and ADR-0001's engine extension, if accepted,
  plugs into the same `IServiceCollection`.
- A fake is one `AddSingleton` in a test's collection.
- The host's service list stays readable, and no service or rule joins it by
  accident of being in the assembly. Command and settings types still join
  through Spectre's registration and reflection; that is the framework's, not
  this choice's.
- No configuration provider in the composition, so nothing about the machine
  enters through the container (`0001-F1` C-3).

Costs:

- Resolution fails at run time. A command whose constructor needs something
  unregistered compiles and throws when invoked; the exception crosses to
  Spectre's `-1`, which `0001-F2` C-2 forbids, and its rendered message and
  stack can carry absolute paths, which brief § 9 forbids. The mitigation is
  one test that resolves every registered command type from the host's
  composition. `ServiceProviderOptions.ValidateOnBuild` is the deferred
  alternative: it checks registered services at build time but not the
  command types Spectre registers on the run, so it does not replace the test.
- `TypeRegistrar` makes every registration Spectre asks for a singleton, so a
  command's dependencies live for the run; a scoped or transient service needs
  its own registration in the collection, not through Spectre.
- A missing compile-time check is accepted in exchange for no generator in the
  build; option 7 is the named move if run-time resolution errors start to
  escape the tests.

ADR-0001 builds on this record: it decides what the engine resolves; this one
decides the container the host resolves from. This record is the decision
behind the rule in `dotnet-tool` § Vertical Slice ("plain
`Microsoft.Extensions.DependencyInjection` through Spectre's `TypeRegistrar`
... no assembly scanning and no third-party container"). `spectre-cli`
§ Dependency Injection stays a library guide; this record marks it up with a
repository decision - the container is Microsoft.Extensions.DependencyInjection
here - and is not the decision behind its generic rule against building the
provider twice.

Work on acceptance:

- The host already matches the unconditional half; nothing changes in
  `src/specht.tool`.
- Owed: the test that resolves every registered command type (Costs, first
  bullet).
- ADR-0001's work, not this record's: replacing `SpecCheckRunner.cs:52-58`'s
  reflection discovery with the hand-written list, and the
  `Microsoft.Extensions.DependencyInjection.Abstractions` pin it needs, in the
  `Engine` item group.
- `dotnet-tool` § Vertical Slice cites this ADR in its next edit.
