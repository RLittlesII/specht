---
title: "ADR-0007: Each part registers itself, and one factory composes the host and every test"
description: "Item 0099's decision, on the owner's direction of 2026-10-09. One static factory builds the specht app as one fluent chain; Program.cs only calls it. The engine composes itself through AddSpechtEngine() and each command slice through its own registration, so the factory lists parts and never their internals. A test builds the same graph by calling the same factory, with what only the caller knows - the file system, the console - passed in as parameters, and never replaces a registration after the fact. Supersedes the rejected ADR-0006 in intent."
type: adr
---

# ADR-0007: Each part registers itself, and one factory composes the host and every test

## Status

accepted - 2026-10-10, by the owner, in session, with the two questions this
record closes on answered below. Item `0099`
(`.issue/0099-command-test-harness.yml`). Written on the owner's direction,
given in session on 2026-10-09 when the owner rejected
[ADR-0006](0006-a-test-runs-a-command-through-the-production-composition.md).

## Context

### Why item `0099` exists

On pull request #4 the owner rejected the command tests' wiring
([review comment](https://github.com/RLittlesII/specht/pull/4#discussion_r4225019006),
on `test/specht.tests/CheckCommand.Integration.Tests.cs`: "I'm not a fan of
this approach."). The complaint is that each test restates the composition
root. It is still true on `main` (`cdb0876`):

- `CheckCommand.Integration.Tests.cs` runs every case through a private static
  `Check` helper that builds a `ServiceCollection`, a `TypeRegistrar` and a
  `CommandAppTester`, and sets `CheckCommand` as the default command.
- `InitCommand.Integration.Tests.cs`
  (`NoRootOption_WhenInitRuns_ShouldWriteUnderTheWorkingDirectory`) builds its
  own `ServiceCollection` with an `InitWriter`, its own `CommandAppTester`, and
  its own `AddCommand<InitCommand>("init")`.
- `src/specht.tool/Program.cs` holds the list those tests copy: the runner
  delegate bound to `SpecCheckRunner.Run`, an `InitWriter` over the shipping
  copy and `new FileSystem()`, the default command, and the `init` command.

### Why ADR-0006 was rejected

ADR-0006 started from the command tester and moved the composition as a side
effect: it lifted `Program.cs`'s two lists into one central public type and
had a test fixture `Replace` registrations after the fact. The owner rejected
that premise - the services and configuration do not move into a central list -
and with it the fixture and every question that depended on it. The tester is
a consumer of a composition, not the thing to design.

### The owner's two examples

The direction is modelled on two of the owner's apps (both files read on
`main`, 2026-10-09):

- **Transporter**
  ([`src/Gui/MauiProgram.cs`](https://github.com/RLittlesII/Transporter/blob/main/src/Gui/MauiProgram.cs)):
  `MauiProgram.CreateMauiApp()` builds the app as one fluent chain. Each part
  composes itself through its own extension (`.AddTransponder(...)`,
  `.AddFleetTrackingActors(...)`). The caller passes what only it knows, a
  `SynchronizationContextScheduler`, beside its configuration. Its own comment states the testing
  consequence: "Everything but the page is composed in Transponder, where a
  test builds the same graph".
- **Rx.Tracker**
  ([`src/UI/MauiProgram.cs`](https://github.com/RLittlesII/Rx.Tracker/blob/main/src/UI/MauiProgram.cs)):
  the same factory shape, with parts joining through
  `RegisterModule<MainModule>()` and the platform's own services entering as an
  `IPlatformRegistrations` parameter.

The owner, on this direction: "This feels like the direction that would be
available had we started with injectable services!"

### What it depends on

ADR-0001's stages make the engine injectable: `0104` (done), `0105`, `0106`,
then `0107`, which gives the engine its one composition entry point,
`AddSpechtEngine()`. Until `0107` lands the engine is static: the command
receives `SpechtRunner.Run` as a `Func<string, SpechtReport>`.

### The Spectre fact

Read from the package's XML documentation (`Spectre.Console.Cli` and
`Spectre.Console.Cli.Testing` `0.57.2`):

- `CommandApp` has a constructor over an `ITypeRegistrar`,
  `Configure(Action<IConfigurator>)`, `SetDefaultCommand<T>()` and
  `RunAsync(IEnumerable<string>, CancellationToken)` returning an exit code.
- `CommandAppTester` has a constructor over
  `(ITypeRegistrar, CommandAppTesterSettings, TestConsole)`,
  `Configure(Action<IConfigurator>)`, `SetDefaultCommand<T>(string, object)`,
  and `Run(string[])` returning a `CommandAppResult` (`ExitCode`, `Output`,
  `Context`, `Settings`).
- `IConfigurator` has `AddCommand`, `AddBranch`, `AddDelegate`,
  `AddAsyncDelegate`, `AddExample`, `SetHelpProvider` and `Settings`, and no
  `SetDefaultCommand`. ADR-0006 fact 2 measured that `CommandAppTester`
  implements no interface.

So the host's app and the tester share their **inputs** - a registrar and a
configure step - but not a run interface, and the default command is set on
each separately.

### The constraints at stake

- `0001-F2` C-7: the command is tested through Spectre's command tester
  against synthetic trees under a temporary root.
- ADR-0003: Microsoft.Extensions.DependencyInjection behind Spectre's
  `TypeRegistrar`, hand-written registrations with no scanning, singletons,
  and a `ServiceCollection` that is single-use under a registrar (ADR-0006
  fact 1: 1, then 18, then 35 registrations over two runs).
- ADR-0001: one hand-written `AddSpechtEngine()`, called once.
- Brief § 9 and AGENTS.md: no absolute path in any output or fixture, and no
  write into a consumer's tree outside `init`, `upgrade` and `--report`.

## Decision drivers

1. **A test cannot restate the composition**, because it builds the graph by
   calling the code the host calls.
2. **A part owns its own internals.** The host lists parts; what a part
   registers is written once, beside the part.
3. **What only the caller knows is an argument**, not a registration replaced
   afterwards.
4. **The owner's libraries through their own extension points**: Microsoft
   DI's `IServiceCollection` extensions, Spectre's `IConfigurator` and tester.
   No new package.

## Considered options

1. **A private helper per test class (today).** Each class builds its own
   `ServiceCollection`, `TypeRegistrar` and `CommandAppTester`. Rejected by
   the owner on pull request #4: every class restates `Program.cs`, and a
   command or service the host adds or drops does not reach the tests.
2. **ADR-0006: a central public list and a fixture that `Replace`s.** Lift
   `Program.cs`'s services and configuration into one public type and have
   a `SpechtAppFixture` replace seams after composing it. Rejected by the
   owner, 2026-10-09: the composition does not move into a central list, and
   replacing registrations after the fact is built on that list.
3. **Each part registers itself; one factory composes the host and every
   test.** The owner's direction, below.

## Decision

Option 3, accepted by the owner on 2026-10-10.

1. **One static factory builds the app.** As `MauiProgram.CreateMauiApp()`
   does, one fluent chain composes the services and the configuration.
   `Program.cs`, the entry point, only calls it.
2. **Each part composes itself through its own extension.** The engine
   through `AddSpechtEngine()` (item `0107`, ADR-0001). Each command slice
   through its own registration, beside the command in its folder: its
   services on `IServiceCollection` and its `AddCommand` on `IConfigurator`.
   The factory lists the parts and never states their internals. Each part is
   still a hand-written list with no scanning (ADR-0003).
3. **A test builds the same graph by calling the same factory.** No
   after-the-fact `Replace`, and no `ServiceCollection`, `TypeRegistrar` or
   `CommandAppTester` assembled by hand in a test. Every call builds a fresh
   collection, which ADR-0003's single-use registrar requires.
4. **What only the caller knows enters as a factory parameter.** For specht
   the caller inputs are the file system (the host passes `new FileSystem()`,
   a test a `MockFileSystem` or a temporary root) and the console (the host
   leaves Spectre's, a test passes a `TestConsole`), and nothing else. The
   runner is not one: a check test runs the real engine over a synthetic tree
   and never hands the factory a runner (question 1).

**The factory's product is the registrar and the configure step**, the inputs
`CommandApp` and `CommandAppTester` share. The host wraps them in `CommandApp`
and a test in `CommandAppTester`. `Spectre.Console.Cli.Testing` is a test
package and `src/tool` is the one project that packs, so the wrap into the
tester lives once in the test tree, beside the fixtures. It is the only place
a `CommandAppTester` is constructed, it sets the default command, and it is
part of the factory for the testing rule below (question 2).

**Until `0107` lands**, the slices register what exists today: the check slice
still registers the runner delegate bound to `SpechtRunner.Run`, and the init
slice registers its `InitWriter` over the shipping copy and the file system it
is given. A check test reaches that runner through a synthetic tree, not a
canned report, and registers none of its own. When `0107` lands, the factory
adds `AddSpechtEngine()` to its chain and the check slice stops registering
the runner; no test changes its composition.

A non-binding sketch. Every name is a placeholder; the owner has asked for the
shape, not for these names:

```csharp
public static class SpechtProgram
{
    public static (ITypeRegistrar Registrar, Action<IConfigurator> Configure) Create(IFileSystem fileSystem) =>
        (new TypeRegistrar(new ServiceCollection()
                .AddCheck()
                .AddInit(fileSystem)),
         static config => config.AddInit());

    public static CommandApp CreateApp(IFileSystem fileSystem)
    {
        var (registrar, configure) = Create(fileSystem);
        var app = new CommandApp(registrar);
        app.SetDefaultCommand<CheckCommand>();
        app.Configure(configure);
        return app;
    }
}
```

```csharp
return await SpechtProgram.CreateApp(new FileSystem()).RunAsync(args);
```

The acceptance tier is unchanged: its steps launch the built tool through
`Tool.Launch`, so they run `Program.cs`, and through it the factory, exactly as
shipped, and compose nothing.

## Consequences

Buys:

- No test restates the host. A part added to the factory's chain, or a
  registration added inside a part, reaches every command test on the next
  build; a part the host drops fails its tests.
- `0107` lands without editing a command test's composition. ADR-0001 option
  4's cost - the list "written in the host and again in every test" - is
  retired.
- `0001-F2` C-7 holds as written: tests still run through Spectre's tester,
  with the parser in the loop.
- A slice's registration sits beside its command, so a new command (`upgrade`,
  `0001-F7`) is one folder plus one link in the factory's chain.
- No new package.

Costs:

- **`Program.cs` stops holding the list.** It calls the factory, and the
  factory calls `AddSpechtEngine()` once. On acceptance, ADR-0001's status
  records that its "`Program.cs` stays the composition root and calls it once"
  is superseded by this record, ADR-0003's status records the same for
  "Services are registered by hand in `Program.cs`", and `dotnet-tool`
  `references/vertical-slice.md` ("The composition root is a hand-written
  list. `Program.cs` registers services explicitly") is updated to name the
  factory and the per-slice registrations. Each list is still hand-written.
- **The default command is written twice**, in the host's wrap and the test
  tree's wrap, because `IConfigurator` has no `SetDefaultCommand`. The guard is
  the acceptance tier: every check scenario runs the built tool with no command
  name.
- **The factory is public**, because the test projects are separate
  assemblies. ADR-0001 already rejected `InternalsVisibleTo` for reaching
  engine types from tests, and this record does not reopen it.
- **A seam a test needs is a factory parameter.** A parameter is added when a
  test or the host first needs it, not before (`coding-conventions`
  § "Design").

The testing rule, written into `specht-conventions` `references/testing.md` on
acceptance. It binds from item `0144`, which builds the factory:

- A test that runs a command gets its registrar and configuration from the
  factory, wrapped by the one test-tree wrap.
- A check test supplies a synthetic tree, never a canned report.
- Never add: a `ServiceCollection`, `TypeRegistrar` or `CommandAppTester`
  assembled in a test outside the factory, or a runner passed into the
  factory.

Work on acceptance, as its own item, `0144`
(`.issue/0144-compose-the-host-through-one-factory.yml`), depending on `0099`,
not before: the factory and the per-slice registrations, `Program.cs` calling
the factory, the `CheckCommand` and `InitCommand` integration tests moved onto
it, the check's fold tests moved onto synthetic trees with the real engine,
the private `Check` helper and the hand-built `CommandAppTester`s removed, the
duplicated private statics in the `ShippingCopy` tests and `InitSteps`
removed, and the unused `Spectre.Console.Cli.Testing` reference dropped from
`test/acceptance`.

The questions this record closed on, each answered by the owner in session on
2026-10-10:

1. **The check's runner, before and after `0107`.** The check's fold tests hand
   the command a canned report today (`_ => report`). Is the runner a factory
   parameter - the host passes the engine's, a test a canned one - or do those
   tests run the real engine over a synthetic tree, so the file system is the
   only caller input and the runner is never one? ADR-0001 lists "the runner
   the command calls" as a substitution that exists today.

   Resolved 2026-10-10, by the owner: tests run the real engine over a
   synthetic tree. The file system and the console are the only caller
   inputs, and the runner is never a factory parameter.

2. **The one test-tree wrap.** Since the tester cannot be built in `src/tool`
   without shipping a test package, is one test-side wrap of the factory's
   product - the only `new CommandAppTester` in the tree, setting the default
   command - what "built by calling the same factory" means here?

   Resolved 2026-10-10, by the owner: yes. One test-tree wrap of the factory's
   product is the only `new CommandAppTester` in the tree, and it sets the
   default command.
