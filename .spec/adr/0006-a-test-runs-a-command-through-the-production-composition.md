---
title: "ADR-0006: A test runs a command through the production composition"
description: "Rejected by the owner, 2026-10-09; superseded in intent by ADR-0007. Item 0099's spike. An integration test runs a specht command through a CommandAppTester that one AutoFixtureBase<T> fixture builds fresh from the host's own composition - services and configuration lifted out of Program.cs into one public type - replacing only the seam the test names. The acceptance tier keeps launching the built tool. Rejected, each against the code or the package: the hand-wired helper per class, a shared xunit fixture, the generated [AutoFixture] over the tester, a Rocket Surgery test host, constructing the command directly, and invoking Program.cs's entry point."
type: adr
---

# ADR-0006: A test runs a command through the production composition

## Status

rejected - 2026-10-09, by the owner, in session. The owner rejected the
direction this record starts from: the services and configuration do not move
into one central public list (question 1), and the `SpechtAppFixture` that
`Replace`s registrations after the fact rests on that list, so the whole
proposal falls with it; the other questions depended on the same direction.
The record started from the command tester and moved the composition as a side
effect. Superseded in intent by
[ADR-0007](0007-each-part-registers-itself-and-one-factory-composes-the-host.md).
Everything below is kept as history.

Proposed 2026-10-09. Decider: the owner. Item `0099`
(`.issue/0099-command-test-harness.yml`), cut when the owner rejected the
test wiring on pull request #4
([review comment](https://github.com/RLittlesII/specht/pull/4#discussion_r4225019006),
on `test/specht.tests/CheckCommand.Integration.Tests.cs`: "I'm not a fan of
this approach. Create an item on a separate branch to track how we should do
this."). ADR-0001 and ADR-0003 each leave this question to item `0099` by name.

## Context

### What the tests do today (`0dab519`)

- **Integration tier.** `CheckCommand.Integration.Tests.cs` runs every case
  through a private static `Check(run, params args)` helper: a new
  `ServiceCollection`, one `AddSingleton(run)`, a `TypeRegistrar` over it, a
  `CommandAppTester` over an 80-column `TestConsole`, and
  `SetDefaultCommand<CheckCommand>()`. The real-engine cases pass
  `SpecCheckRunner.Run` as `run`. The helper restates the host's composition:
  the registration `Program.cs` makes and the default command it sets.
- **Acceptance tier.** `test/specht.acceptance/Check/CheckSteps.cs` does **not**
  hand-wire a registrar. Item `0026` (pull request #4) did; item `0027`
  (pull request #17, `80a5414`) replaced it with `Launch`, which starts
  `dotnet specht.tool.dll` as a process, because `CheckCommand.Fail` writes to
  the static `Console.Error` (`CheckCommand.cs`) and the command tester's
  `TestConsole` captures stdout only. It is the one test that runs
  `Program.cs`'s composition as shipped (ADR-0003, Context table).
  `specht.acceptance.csproj` still references `Spectre.Console.Cli.Testing`,
  which no step uses.

### What is coming

- More commands, each one more line in the host's configuration: `init`
  (`0001-F4`), `upgrade` (`0001-F7`), `--explain` on the check (`0001-F3`,
  item `0040`, exit `4`), `--help` (item `0029`), the summary (item `0028`).
- ADR-0001 stage D (item `0107`): the engine resolves from the container
  through one `AddSpechtEngine()`, which `Program.cs` calls once. ADR-0001's
  rejected option 4 names this item's cost exactly: without a shared entry
  point "the list is written in the host and again in every test that composes
  the real engine". Item `0107` depends on this item.
- ADR-0003's owed test, which resolves every registered command type from the
  host's composition. It needs the host's composition reachable from a test.

### Facts measured for this record

Each was run against the real types on a throwaway branch and then removed;
none is committed.

1. **A `ServiceCollection` handed to a `TypeRegistrar` is single-use**, as
   ADR-0003 states: one collection under one `CommandAppTester` held 1
   registration, 18 after one `Run`, 35 after a second. Spectre registers its
   configuration, the command, its settings and the console into it on every
   run.
2. **`CommandAppTester` implements no interface**, and `IConfigurator` has no
   `SetDefaultCommand` (its members: `SetHelpProvider`, `Settings`,
   `AddExample`, `AddCommand`, `AddDelegate`, `AddAsyncDelegate`,
   `AddBranch`). `CommandApp.SetDefaultCommand<T>()` and
   `CommandAppTester.SetDefaultCommand<T>()` are separate members, so no one
   method can configure both.
3. **The generated `[AutoFixture(typeof(CommandAppTester))]`** (Rocket Surgery
   AutoFixtures `10.0.6`) emits
   `private ITypeRegistrar _registrar = Substitute.For<ITypeRegistrar>();`
   and fails with `CS0103: The name 'Substitute' does not exist`: the
   generator defaults every interface-typed constructor parameter to an
   NSubstitute substitute. Its `AutoFixtureBase<TFixture>` base, which the
   package emits into the test assembly, has no such default. It exposes
   `With<TField>(ref TField, TField)` and leaves `Build()` and the implicit
   conversion to the fixture.
4. **Rocket Surgery publishes no Spectre.Console.Cli test host** (NuGet,
   2026-10-09):
   - `Rocket.Surgery.CommandLine` `15.0.0` depends on
     `Microsoft.Extensions.Hosting` `10.0.3`, `Rocket.Surgery.Hosting`,
     `Rocket.Surgery.Conventions` and `Spectre.Console.Cli` `0.53.1`. Its 13
     public types are host-builder extensions (`ConfigureCommandLine`,
     `ConfigureCommandApp`, `SetDefaultCommand`, `RunConsoleAppAsync`) and
     convention delegates. No tester.
   - `Rocket.Surgery.Extensions.Testing` `10.0.6` depends on `DryIoc.dll`
     `6.0.0-preview-09`, `DryIoc.Microsoft.DependencyInjection`
     `8.0.0-preview-04`, `Serilog` and `System.Reactive` `7.0.0-rc.1`. Its
     public types are `LoggerTest`, `RocketSurgeryTestContext`,
     `DryIocExtensions` and `ILoggingTestContext`. No Spectre type.
   - `Rocket.Surgery.Extensions.Testing.XUnit3` `10.0.6`: `XUnitTestContext`
     and `XUnitDefaults`. `Rocket.Surgery.Extensions.Testing.Fixtures`
     `10.0.6`: `ITestFixtureBuilder` and one `With` extension.
5. **The command is constructible without the parser.** `CommandContext` has
   a public constructor and `CheckCommand.ExecuteAsync` is public, so
   `new CheckCommand(console, run).ExecuteAsync(context, new() { Strict = true }, ct)`
   compiles and runs. It needs a hand-written `IRemainingArguments`.
6. **The parser carries behaviour no claim states.** Through the host's
   configuration, `specht --no-such-option` exits `0`: Spectre parks an
   unknown option in the remaining arguments unless strict parsing is on. No
   claim in `0001-F2` or `0001-F3` covers an unknown option. This record does
   not decide it; it is routed to `spec-author` (Consequences). It is evidence
   that a test which skips the parser cannot see what the parser does.

### The constraints at stake

- `0001-F2` C-7: the command is tested through Spectre's command tester against
  synthetic trees under a temporary root.
- `0001-F2` C-2: exit codes are returned, never thrown. A missing registration
  surfaces at run time (ADR-0003, Costs).
- ADR-0003: one `BuildServiceProvider()` call site (`TypeRegistrar.Build`), a
  single-use collection, singleton host registrations, a hand-written service
  list, and no generic host or third-party container.
- `specht-conventions` `references/testing.md`: a test through the command
  tester is Integration; exactly one `Tier` trait per class; test data from
  Rocket Surgery AutoFixtures; cases of one shape are one `[Theory]`; no
  mocking library; a class owns its own harness; repository-relative paths.
- Lesson 0002: what the owner put in place is extended, not replaced. Here
  that means `Program.cs` as the hand-written composition root, Spectre's
  tester, and AutoFixtures.

## Decision drivers

1. **A test cannot drift from what ships.** The services and commands a test
   runs are the host's, read from the same code `Program.cs` calls. A new
   command or `AddSpechtEngine()` reaches every test with no test edit.
2. **The parser stays in the integration tier**, as `0001-F2` C-7 requires.
   Binding, defaults, `--help`, `--explain` and unknown input are Spectre's,
   and only a run through the tester sees them.
3. **A test names only the seam it replaces.** One builder call per seam, and
   nothing else about composition in a test.
4. **The owner's libraries, through their extension points**: Microsoft DI's
   own `Replace`, Spectre's own tester, AutoFixtures' own base class. No new
   package.
5. **Each test gets a fresh app**: ADR-0003's single-use collection, measured
   (fact 1).

## Considered options

1. **The hand-wired helper per test class (status quo).** Each command test
   class keeps a private static helper that builds a `ServiceCollection`, a
   `TypeRegistrar` and a `CommandAppTester`. Pro: everything a test runs is
   visible in its file. Con: the owner rejected it on pull request #4. It
   restates the host's composition in each class: the default command, every
   `AddCommand` line `init` and `upgrade` add, and after stage D the
   `AddSpechtEngine()` call, which ADR-0001 option 4 names as the cost. A
   command missing from a helper, or one the host drops, passes in tests and
   fails in the shipped tool.

2. **Reuse the production composition and replace one seam.** Lift
   `Program.cs`'s two lists into one public type in `src/specht.tool`: the
   service registrations and the `IConfigurator` configuration. `Program.cs`
   calls both, and so does a test, which then `Replace`s the seam it names.
   Pro: drivers 1 and 2. `Replace` is in
   `Microsoft.Extensions.DependencyInjection.Abstractions`, already in the
   graph. Con: `Program.cs` stops holding the list itself, which changes what
   the owner put in place (`dotnet-tool` `references/vertical-slice.md`: "the
   composition root - a hand-written list, nothing else"), so it needs the
   owner's approval under lesson 0002. The default command is written twice
   (fact 2). On its own it says nothing about where the test's composition
   code lives, so every test class still writes those lines (option 1's cost
   in a smaller form).

3. **A shared xunit class or collection fixture that owns the registrar, the
   console and the tester.** Pro: composition written once per class or
   collection. Con: rejected on fact 1. A tester shared across tests shares one
   collection that grows by 17 registrations a run, which ADR-0003 forbids.
   Each test hands a different runner, so the shared instance would be mutated
   per test. That is order-dependent state, the opposite of a fixture.
   Rebuilding inside the fixture per test is option 5 with xunit's lifetime
   wrapped around it for no gain.

4. **The generated `[AutoFixture(typeof(CommandAppTester))]` partial.** Pro: the
   owner's chosen generator, one line of declaration. Con: rejected on fact 3.
   It does not compile without NSubstitute, and NSubstitute is the mocking
   library `testing.md` rules out. With it added, the fixture still sets no
   default command and no configuration, so every test writes both.

5. **Option 2's composition, built by one hand-written `AutoFixtureBase<T>`
   fixture.** Option 2's public type, plus a `SpechtAppFixture : AutoFixtureBase<SpechtAppFixture>`
   in `test/specht.tests`. Its implicit conversion builds a fresh
   `CommandAppTester` on every use: a new collection from the host's
   registrations, each seam the test set `Replace`d, the host's configuration
   and default command applied, and an 80-column `TestConsole`. Pro: drivers 1
   to 5. A test reads `CommandAppTester app = new SpechtAppFixture().WithRunner(_ => report);`,
   the same shape as `SpecCheckReportFixture` and `SpecViolationFixture`. It is
   built fresh and fire-and-forget, as the AutoFixtures model requires. Con:
   option 2's cost (owner approval for the move out of `Program.cs`, and the
   default command written twice). Also one typed `With*` and one field per
   seam, which is code to write each time a seam is earned.

6. **A Rocket Surgery test host.** Pro: the owner's library family. Con:
   rejected on fact 4. None exists for Spectre.Console.Cli.
   `Rocket.Surgery.CommandLine` is a production hosting model on
   `Microsoft.Extensions.Hosting`, which ADR-0003 option 2 rejected (the host's
   configuration providers read environment variables, against `0001-F1` C-3),
   and it pins an older Spectre.Console.Cli than the `0.57.2` this repository
   pins. `Rocket.Surgery.Extensions.Testing` brings DryIoc, a third-party
   container ADR-0003 option 6 rejected, in preview builds. The part of
   Rocket Surgery that fits is the one already referenced:
   AutoFixtures' `AutoFixtureBase<T>`, which is option 5.

7. **Construct the command directly; leave the parser to acceptance.** Pro:
   no registrar, no tester. A command's fold is reachable with a
   `TestConsole` and a delegate (fact 5). Con: rejected. It contradicts
   `0001-F2` C-7, so it needs that constraint amended. The parser's work
   disappears from the integration tier: `--root`'s default, `--strict`'s
   binding, `Validate()`, `--help` (item `0029`), `--explain`'s option and
   its exit `4` (item `0040`), and fact 6's unknown option. The acceptance
   tier cannot take that work: Reqnroll covers the claims' scenarios, and
   `testing.md` says a green scenario does not relieve the mechanism beneath
   it of coverage. Each test also hand-writes an `IRemainingArguments`. A
   command decision that deserves a unit test is moved out of the command by
   the implementer (`testing.md`, "A test through Spectre's command tester is
   Integration"). This record does not change that.

8. **Invoke `Program.cs`'s entry point in process.** The strongest form of
   "the test runs exactly what ships": call
   `typeof(CheckCommand).Assembly.EntryPoint` with the arguments. Con:
   rejected. Top-level statements give no way to replace a seam or pass a
   `TestConsole`. Output reaches the real `Console`, and capturing it means
   `Console.SetOut`, which is process-wide and races with xunit's parallel
   classes. Running what ships exactly is what the acceptance tier already
   does, out of process, where stdout, stderr and the exit code are the
   shell's.

## Decision

Proposed: option 5 for the integration tier. The acceptance tier keeps
launching the built tool.

**The host's composition gets one public home in `src/specht.tool`.** One
static type (placeholder name `SpechtApp`) holds the two hand-written lists
that `Program.cs` holds today:

```csharp
public static class SpechtApp
{
    public static IServiceCollection AddSpechtTool(this IServiceCollection services) =>
        services.AddSingleton<Func<string, SpecCheckReport>>(SpecCheckRunner.Run);

    public static void Configure(IConfigurator config) => config.SetApplicationName("specht");
}
```

```csharp
var app = new CommandApp(new TypeRegistrar(new ServiceCollection().AddSpechtTool()));
app.SetDefaultCommand<CheckCommand>();
app.Configure(SpechtApp.Configure);

return await app.RunAsync(args);
```

Every later service goes into `AddSpechtTool()`: `AddSpechtEngine()` from
item `0107`, and anything a command takes by constructor. Every later command
goes into `Configure` (`AddCommand<InitCommand>("init")`, `upgrade`). Both are
still hand-written lists with no scanning, as ADR-0003 requires. They move one
file over.

**One fixture builds the app a test runs.** In `test/specht.tests`, beside the
other fixtures:

```csharp
internal sealed class SpechtAppFixture : AutoFixtureBase<SpechtAppFixture>
{
    public SpechtAppFixture WithRunner(Func<string, SpecCheckReport> run) => With(ref _run, run);

    public SpechtAppFixture WithConsole(TestConsole console) => With(ref _console, console);

    public static implicit operator CommandAppTester(SpechtAppFixture fixture) => fixture.Build();

    private CommandAppTester Build()
    {
        var services = new ServiceCollection().AddSpechtTool();
        if (_run is not null)
        {
            services.Replace(ServiceDescriptor.Singleton(_run));
        }

        var app = new CommandAppTester(new TypeRegistrar(services), console: _console);
        app.SetDefaultCommand<CheckCommand>();
        app.Configure(SpechtApp.Configure);
        return app;
    }

    private Func<string, SpecCheckReport>? _run;
    private TestConsole _console = new TestConsole().Width(80);
}
```

A test:

```csharp
[Theory]
[MemberData(nameof(Verdicts))]
public void AReportOfSeverities_WhenChecked_ShouldExitWithTheClaimedCode(SpecSeverity[] severities, string[] args, int expected)
{
    // Given
    SpecCheckReport report = new SpecCheckReportFixture().WithViolations(
        [.. severities.Select(static severity => (SpecViolation)new SpecViolationFixture().WithSeverity(severity))]);
    CommandAppTester app = new SpechtAppFixture().WithRunner(_ => report);

    // When
    var result = app.Run(args);

    // Then
    result.ExitCode.Should().Be(expected);
}
```

The rules this follows:

- **A test that runs a command builds its app from `SpechtAppFixture`.** It
  never builds a `ServiceCollection`, a `TypeRegistrar` or a
  `CommandAppTester` itself. It is Integration tier, as `testing.md` already
  says of the command tester.
- **A test replaces only the seams it names**, one typed `With*` each.
  Anything left unset is the host's own registration, so
  `new SpechtAppFixture()` runs the real engine. A single-service seam is
  `Replace`d. A set seam (the rule set, after stage D) is `RemoveAll<T>()`
  and then `Add`. A seam gets its `With*` when the first test needs it, not
  before. Today that is the runner and the console.
- **Every conversion builds a fresh app** (fact 1). The fixture holds no
  tester and no collection, only the values a test set. No xunit class or
  collection fixture owns a tester.
- **The acceptance tier launches the built tool** through `CheckSteps.Launch`
  and composes nothing. The shell's stdout, stderr and exit code are the
  contract its scenarios state, and stderr is out of the tester's reach.
  `Launch` moves to a type shared by step classes when a second step class
  (`init`'s or `upgrade`'s) needs it, not before.

How it holds for what is coming:

| Command or change     | Integration test                                                                                                                                     | Acceptance                   |
| --------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------- |
| `check` (`0001-F2`)   | `new SpechtAppFixture().WithRunner(...)` or over a `SpecTree`; the private `Check` helper goes                                                       | `Launch`, unchanged          |
| `--help` (`0029`)     | `app.Run("--help")`; Spectre renders it from the configuration `Configure` holds                                                                     | as its scenarios state       |
| `--explain` (`0040`)  | `app.Run("--explain", "SPEC031")` and `app.Run("--explain", "SPEC999")` for exit `4`; no new seam                                                    | `Launch`                     |
| `init` (`0001-F4`)    | `app.Run("init", "--root", tree.Root)`; the command joins through `Configure`, no test edit; `WithFileSystem` if the command takes `IFileSystem`     | `Launch`, shared once needed |
| `upgrade` (`0001-F7`) | as `init`                                                                                                                                            | as `init`                    |
| Stage D, item `0107`  | `AddSpechtTool()` calls `AddSpechtEngine()`; every real-engine test composes it with no edit; `WithFileSystem(MockFileSystem)` replaces the engine's | unchanged                    |
| ADR-0003's owed test  | one theory over the configured command names, each run through `new SpechtAppFixture()`, asserting no `-1`                                           | -                            |

## Consequences

Buys:

- No test restates the host. A command added to `Configure`, or a service
  added to `AddSpechtTool()`, is in every command test on the next build, and
  a command the host drops fails its tests.
- `0001-F2` C-7 holds as written, with the parser in the loop.
- Stage D lands without touching a command test's composition. ADR-0001 option
  4's "written again in every test" cost is retired.
- No new package. `Replace` and `RemoveAll` come from
  `Microsoft.Extensions.DependencyInjection.Abstractions`, already in the
  graph. `AutoFixtureBase<T>` is emitted by the AutoFixtures package already
  referenced.
- The harness reads like the test-data fixtures the owner asked for on pull
  request #4.

Costs:

- **`Program.cs` stops being the list.** It calls `AddSpechtTool()` and
  `Configure`, and the lists live in `SpechtApp`. That changes `dotnet-tool`
  `references/vertical-slice.md` ("`Program.cs` ... a hand-written list,
  nothing else") and ADR-0001's "`Program.cs` stays the composition root and
  calls it once". Under lesson 0002 the owner approves it before any code
  moves (question 1). `SpechtApp` is public because the test project is a
  separate assembly. ADR-0001 already rejected `InternalsVisibleTo` as the way
  to reach engine types from tests (Decision, constructibility), and this
  record does not reopen it for the host.
- **The default command is written twice**, once in `Program.cs` and once in
  the fixture (fact 2). The guard is the acceptance tier: every check scenario
  runs the built tool with no command name, so a host whose default changes
  fails them.
- **A seam costs a field and a `With*` on the fixture.** That is deliberate
  (driver 3), and a generic `With<TService>` was considered and not chosen
  (question 4).
- **The parser's unclaimed behaviour becomes visible**, starting with fact 6.
  That is a gain, but it is work: `spec-author` decides whether an unknown
  option is an error before a test asserts anything about it.
- **Judgement, labelled.** Using `AutoFixtureBase<T>` to build an app under
  test, not a value handed to the code under test, stretches "test data via
  AutoFixtures" (`testing.md`). It is the package's own model ("the fixture
  is a builder that produces a SUT"), but whether it fits the owner's rule is
  the owner's call (question 2).

Work on acceptance, under item `0099`'s remaining acceptance criteria and not
before:

- `SpechtApp` in `src/specht.tool`, with `Program.cs` calling it. This is an
  exemption-class refactor: the same registrations, the same default command,
  and the acceptance scenarios green.
- `SpechtAppFixture` in `test/specht.tests`. `CheckCommand.Integration.Tests.cs`
  moves onto it, and the private `Check` helper is deleted. The discovered
  test count is unchanged.
- `CheckSteps` already composes nothing. Its acceptance criterion is met by
  this record's acceptance-tier rule. The unused `Spectre.Console.Cli.Testing`
  reference in `specht.acceptance.csproj` is removed if the owner agrees
  (question 3).
- `specht-conventions` `references/testing.md` states the rule, with the
  Never-add item "a `ServiceCollection`, `TypeRegistrar` or `CommandAppTester`
  built in a test outside `SpechtAppFixture`".
- `dotnet-tool` `references/vertical-slice.md` names `SpechtApp` as where the
  lists live.

Routed elsewhere, not decided here:

- `spec-author`: fact 6. `specht --no-such-option` exits `0` and no claim
  covers it. Whether strict parsing is the intended behaviour is a claim to
  write or a § 5 row.
- `implementer`: `CheckCommand.Fail` writes to the static `Console.Error`, so
  a stderr message cannot be asserted in process. This is why the acceptance
  tier stays out of process. An injected error writer would let the
  integration tier assert it, and that is a design call for the command, not
  for this record.

Questions the owner answers to accept:

1. Do the services and the configuration move out of `Program.cs` into one
   public `SpechtApp` (name open) in `src/specht.tool`, with `Program.cs`
   calling it? This amends `dotnet-tool` § Vertical Slice and ADR-0001's
   "`Program.cs` stays the composition root".
2. Is a hand-written `AutoFixtureBase<T>` fixture that builds the
   `CommandAppTester` an acceptable use of AutoFixtures, or should the harness
   be a plain builder type under the same rules?
3. Does the acceptance tier stay a process launch of the built tool, with no
   in-process composition in step definitions? And does the unused
   `Spectre.Console.Cli.Testing` reference leave `specht.acceptance`?
4. One typed `With*` per seam (chosen), or one generic `With<TService>(TService)`
   that `Replace`s any registration? The generic form needs a list of
   descriptors on the fixture, which the AutoFixtures guidance ("no state
   fields on the fixture beyond what `AutoFixtureBase<T>` generates") rules
   against.
