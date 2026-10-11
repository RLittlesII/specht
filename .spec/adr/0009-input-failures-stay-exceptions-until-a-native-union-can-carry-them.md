---
title: "ADR-0009: Input failures stay exceptions until a native C# union can carry them"
description: "Item 0151's proposal. One slice of the check - the manifest load to the command's catch ladder - was rewritten four ways on unmerged branches with stderr and exit codes frozen: language-ext v5 beta, Dunet, and C# 15 union types on net11.0 and on net10.0 with a polyfill. All four build and pass. Proposed: reject language-ext, Dunet as a bridge, and the polyfill; take the native union on net11.0 as the direction, gated on general availability, on the build running on .NET 11, and on the owner moving specht.tool off the long-term-support target. Until all three hold, exceptions stay."
type: adr
---

# ADR-0009: Input failures stay exceptions until a native C# union can carry them

## Status

proposed - 2026-10-11. Decider: the repository owner. Item `0151`
(`.issue/0151-railway-error-handling.yml`), which quotes the owner's request
of 2026-10-11 and records the owner's two decisions on how the spike was run.
The record closes on three questions for the owner.

## Context

### How to read the evidence

Three kinds of statement appear below, and each is marked:

- **Read from a commit** when this record was written: every code quotation,
  every `git diff --shortstat` figure, every count of lines, call sites and
  assertions. Baseline is `fcaa0b8`.
- **Measured by a trial**: a build result, a compiler diagnostic, a package
  size or a timing. Each was produced by a command run on the named trial
  commit and was not re-run for this record. Release dates and support terms
  are as the trials recorded them from the publishers.
- **Judgement** or **inference**: labelled where it stands.

The trial branches are never merged (item `0151`, decision 1) and may be
deleted once the owner rules, so the decisive code is quoted here. A
quotation omits XML documentation comments and the members a trial left as
they were, and changes nothing else.

| Trial                             | Branch                    | Commit                                     |
| --------------------------------- | ------------------------- | ------------------------------------------ |
| language-ext `5.0.0-beta-78`      | `0151/trial-language-ext` | `d6b04d9bcb5e0ba9441dc66ccc9d8554d66bff6b` |
| Dunet `1.16.2`                    | `0151/trial-dunet`        | `727d5f92a8a0d942c28372af12fba67930b74072` |
| C# 15 union, `net11.0`            | `0151/trial-union`        | `2d96ed0bdcd24a88ec492f5f84c77d93a75e2104` |
| C# 15 union, `net10.0` + polyfill | `0151/trial-union`        | `79bcfef7e214f2cb2e798fc3c90a56a31218747a` |

### What the code is today

Read from `fcaa0b8`:

- **Eleven `throw` sites and seven `catch` clauses under `src/`.** Seven of
  the throws are in `src/specht/Manifest/SpecManifest.cs`, two in
  `src/specht/Versioning/SchemaVersions.cs`, one in
  `src/specht/Model/SpecSchemas.cs` and one in
  `src/tool/Features/Init/InitWriter.cs`. Four of the catches are the ladder
  in `src/tool/Features/Check/CheckCommand.cs:31-46`.
- **Four sealed exception types with no shared base**:
  `SpechtRootNotFoundException`, `SpechtManifestNotFoundException`,
  `SpechtManifestUnreadableException` and `SpechtManifestException`, each
  deriving from `Exception`.
- **One filter relabels by type, not by cause.** `SpecManifest.cs:59`:

  ```csharp
  catch (Exception exception) when (exception is JsonException or InvalidOperationException or FormatException)
  {
      throw new SpechtManifestUnreadableException(exception);
  }
  ```

  The shape check is implicit: `AsObject()`, `AsArray()`, `GetValue<string>()`
  and the private `Present` guard throw into that one filter. A genuine defect
  that throws `InvalidOperationException` or `FormatException` anywhere under
  `Read` takes the same path and is reported as an unreadable manifest.

- **The specification and the code disagree on the count.** `0001-F2` § 7
  says the command "catches exactly those three types"; `CheckCommand` catches
  four. The fourth is `SpechtManifestException`, `0001-F5` B-022 and B-023.
  § 7 is stale today and is owed a delta whichever way the owner rules.
- **Nothing makes the compiler check the ladder.** The four catch clauses
  cover the four types because someone wrote four. A fifth exception type
  would compile, cross the command, and reach Spectre's default handler,
  which `0001-F2` C-2 rules out.

Violations are already values; only input failures throw.

### The slice every trial rewrote

Identical for each, with stderr text and exit codes frozen and no `.feature`
file edited: `SpecManifest.Load` and what it calls, `SchemaVersions.Select`,
the vocabulary check in `SpecSchemas`, the passing frames `SpecModel.Load` and
`SpechtRunner.Run`, the `Func<string, SpechtReport>` registration in
`src/tool/Program.cs`, and the four-clause catch ladder in `CheckCommand`.
`init` is outside it, so `SpechtRootNotFoundException` stays in every trial
and the other three exception types are deleted.

### The toolchain

Measured by the trial, and from Microsoft's published schedule as the trial
recorded it: .NET 11 RC 1 is SDK `11.0.100-rc.1.26425.128`, released
2026-09-08 with a go-live licence. General availability is 2026-11-10.
.NET 11 is a standard-term-support release, supported 2026-11-10 to
2028-11-09; .NET 10 is long-term. C# 15 is the default language version for
`net11.0`. The SDK was installed in an isolated user folder for the trial;
`global.json` on `main` is unchanged.

### The constraints at stake

- `0001-F2` C-2 and C-4, and `0001-F2` § 7.
- `0001-F1` C-7 (the engine's package list) and `0001-F1` B-005.
- `0001-F5` B-022 and B-023.
- ADR-0002, whose Context names the short-circuit as
  `SpechtManifestException`.
- Untouched by every option: the fault isolation around a rule that throws
  (`0001-F1` B-012 to B-016, `0001-F1` C-11, ADR-0005 (c)). That catches a
  defect, not an expected input failure.

## Decision drivers

1. **A failure the command does not handle fails the build**, not the run
   (`0001-F2` C-2).
2. **What crosses from engine to command is the engine's own type**
   (`0001-F2` C-4).
3. **A dependency is paid for by what it does here** (`0001-F1` C-7), in
   package size and start-up as well as in an ADR.
4. **The change is made once.** A step that is re-edited when the language
   feature ships is two migrations.
5. **A supported toolchain.** The tool is installed on every consumer's
   machine and CI.

## Considered options

### 1. Keep exceptions

What runs today (Context). It costs nothing and it works: every acceptance
scenario passes. Its three defects are facts read from `fcaa0b8`: the
over-broad filter, the unchecked ladder, and the `<exception>` documentation
on the public loaders as the only statement of what a caller must catch. The
first needs no union to fix (question 3).

### 2. language-ext `5.0.0-beta-78`

The trial on `0151/trial-language-ext` at `d6b04d9`. The failure type:

```csharp
public sealed record SpechtFailure(SpechtFailureKind Kind, string Message);

public enum SpechtFailureKind
{
    RootNotFound,
    ManifestNotFound,
    ManifestUnreadable,
    ManifestRejected,
}
```

The command:

```csharp
public sealed class CheckCommand(IAnsiConsole console, Func<string, Either<SpechtFailure, SpechtReport>> run)
    : AsyncCommand<CheckCommand.Settings>
{
    public override Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken) =>
        Task.FromResult(
            run(Path.GetFullPath(settings.Root)).Match(
                Left: failure => Fail(failure, settings.Root),
                Right: report => Print(report, settings)));

    private static int Fail(SpechtFailure failure, string root)
    {
        Console.Error.WriteLine(
            failure.Kind is SpechtFailureKind.RootNotFound ? $"specht: '{root}' is not a directory." : $"specht: {failure.Message}");

#pragma warning disable CS8524 // An unnamed kind is a defect; a named kind left out still fails the build as CS8509.
        return failure.Kind switch
        {
            SpechtFailureKind.RootNotFound => ExitCodes.MissingInput,
            SpechtFailureKind.ManifestNotFound => ExitCodes.MissingInput,
            SpechtFailureKind.ManifestUnreadable => ExitCodes.InvalidManifest,
            SpechtFailureKind.ManifestRejected => ExitCodes.InvalidManifest,
        };
#pragma warning restore CS8524
    }
}
```

Facts:

- **The exhaustiveness is the enum's.** Measured: a fifth enum member left
  out of the switch fails the build with CS8509, naming the member. That is
  the C# compiler on an enum switch; `Either` contributes nothing to it, and
  the switch needs a `#pragma warning disable CS8524` to build at all.
  `Fin<A>` was rejected inside the trial because its failure is the open
  `Error` hierarchy, which the compiler cannot check for coverage.
- **A library type is in the host.** Read from the commit: `using LanguageExt;`
  in `src/tool/Program.cs` and `CheckCommand.cs`, and `Either<,>` in the
  registration and in the command's constructor.
- **`Validation` made the accumulating check longer**, shown under "The two
  hard places".
- **One assembly of 5,327,872 bytes.** `LanguageExt.Core` brings no
  transitive package; the `.nupkg` grows from 2,171,792 to 3,765,181 bytes
  (73%), and start-up by about 56 ms (11%).
- **Defects met**, measured: `using LanguageExt;` makes `HashSet<>` ambiguous
  with `System.Collections.Generic` (CS0104). The commit carries the
  workaround in three engine files: `SpecManifest.cs` writes
  `System.Collections.Generic.HashSet<string>`, and `SpechtRunner.cs` and
  `SchemaVersions.cs` omit the `using` and write `LanguageExt.Either<,>` in
  full. Implicit conversion into `Either` does not apply to interface types.
  `Validation`'s semigroup requirement is resolved at run time, not compile
  time; the package's XML documentation says a `TypeLoadException` results.
- **It is a beta.** As the trial recorded from the package's listing and its
  author: the types used were last reshaped by the breaking beta-55
  (2025-11-12); beta-77 is dated 2025-12-30 and beta-78 2026-10-09; there is
  no stable v5; and the author wrote on 2026-07-15 that he may hold v5 until
  C# unions ship.

### 3. Dunet `1.16.2`

The trial on `0151/trial-dunet` at `727d5f9`. The failure types:

```csharp
[Union(EnableImplicitConversions = false)]
public partial record InputFailure
{
    public partial record RootNotFound;

    public partial record ManifestNotFound
    {
        public string Message => $"{SpecManifest.RelativePath}: there is no manifest at the manifest path.";
    }

    public partial record ManifestUnreadable
    {
        public string Message => $"{SpecManifest.RelativePath}: the manifest does not parse into the manifest's shape.";
    }

    public partial record ManifestRejected(string Message);
}

[Union]
public partial record Result<T>
{
    public partial record Ok(T Value);

    public partial record Failed(InputFailure Failure);
}
```

The command:

```csharp
public sealed class CheckCommand(IAnsiConsole console, Func<string, Result<SpechtReport>> run) : AsyncCommand<CheckCommand.Settings>
{
    public override Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken) =>
        run(Path.GetFullPath(settings.Root)).Match(
            loaded => Print(loaded.Value, settings),
            failed => failed.Failure.Match(
                _ => Fail(ExitCodes.MissingInput, $"specht: '{settings.Root}' is not a directory."),
                static missing => Fail(ExitCodes.MissingInput, $"specht: {missing.Message}"),
                static unreadable => Fail(ExitCodes.InvalidManifest, $"specht: {unreadable.Message}"),
                static rejected => Fail(ExitCodes.InvalidManifest, $"specht: {rejected.Message}")));
}
```

Facts:

- **A fifth case fails the build, and neither diagnostic names it.**
  Measured: CS1501 through the generated `Match` (an arity error), CS8509
  through a `switch`.
- **No library type or `using` in `src/tool`**, only generated members
  (`Match`). Read from the commit.
- **It is not build-time only.** Measured: since 1.11.0 `specht.dll` carries
  an assembly reference to `Dunet`, and the package ships an 11,776-byte
  `Dunet.dll`. With `PrivateAssets="all"` a complete `switch` in `src/tool`
  fails CS8509, because the analyzer suppression that makes the switch
  exhaustive does not reach that project, and reading the type's attributes
  by reflection throws `FileNotFoundException`.
- **Two defaults had to be refused or accepted.** By default it would
  generate an implicit `string` to `InputFailure` conversion, disabled above
  with `EnableImplicitConversions = false`. The engine's public surface gains
  two unused generated `*MatchExtensions` classes.
- **It gives no bind.** Read from the commit: eight
  `static failed => failed.Failure` arms that only pass a failure up, in six
  methods across `SpecManifest.cs`, `SpecSchemas.cs`, `SpecModel.cs` and
  `SpechtRunner.cs`.
- **Leaving it later is a second migration.** Inference; C# 15 was not
  compiled on that branch: about 20 engine sites and 80 test sites, because
  the generated `Match`, `UnwrapOk` and nested case names do not exist
  natively.

### 4. C# 15 union types on `net11.0`

The trial on `0151/trial-union` at `2d96ed0`. The failure types, each in its
own file:

```csharp
public closed record InputFailure;

public sealed record RootNotFound : InputFailure;

public sealed record ManifestNotFound : InputFailure
{
    public string Message => $"{SpecManifest.RelativePath}: there is no manifest at the manifest path.";
}

public sealed record ManifestUnreadable : InputFailure
{
    public string Message => $"{SpecManifest.RelativePath}: the manifest does not parse into the manifest's shape.";
}

public sealed record ManifestRejected(string Message) : InputFailure;

public union Outcome<T>(T, InputFailure);
```

The command:

```csharp
public sealed class CheckCommand(IAnsiConsole console, Func<string, Outcome<SpechtReport>> run) : AsyncCommand<CheckCommand.Settings>
{
    public override Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken) =>
        Task.FromResult(
            run(Path.GetFullPath(settings.Root)) switch
            {
                SpechtReport report => Print(report, settings),
                RootNotFound => Fail(ExitCodes.MissingInput, $"specht: '{settings.Root}' is not a directory."),
                ManifestNotFound missing => Fail(ExitCodes.MissingInput, $"specht: {missing.Message}"),
                ManifestUnreadable unreadable => Fail(ExitCodes.InvalidManifest, $"specht: {unreadable.Message}"),
                ManifestRejected rejected => Fail(ExitCodes.InvalidManifest, $"specht: {rejected.Message}"),
            });
}
```

A passing frame, `SpechtRunner.Run`:

```csharp
public static Outcome<SpechtReport> Run(string root, SchemaVersions versions) =>
    SpecModel.Load(root, versions) switch
    {
        SpecModel model => Evaluate(model, SpecRules.All),
        InputFailure failure => failure,
    };
```

Facts:

- **A fifth case fails the build and the diagnostic names it.** Measured:
  CS8509 naming the missing case, across the assembly boundary from
  `specht.dll` to `specht.tool.dll`, with no pragma, no package and no
  suppression.
- **What crosses is the engine's own.** Read from the commit: `src/tool`
  names `Outcome<SpechtReport>` and the four case types, all declared in
  `src/specht/`. No package is added, so `0001-F1` C-7 is untouched.
- **The build needed two workarounds.** Read from the commit and measured:
  NUKE 10.1.0 cannot run on the .NET 11 runtime
  (`System.TypeInitializationException` from `Nuke.Common.NukeBuild`, exit
  134). `.build` inherits the root `Directory.Build.props`, so it was pinned
  back with `<TargetFramework>net10.0</TargetFramework>` in
  `.build/.build.csproj`. NUKE's MSBuild locator then exports
  `MSBUILD_EXE_PATH`, `MSBuildExtensionsPath` and `MSBuildSDKsPath` pointing
  at SDK 10, giving NETSDK1045; `SpechtBuild.OnBuildInitialized` clears the
  three.
- **Limits met**, measured: an exhaustive `switch` _statement_ over a union
  still fails CS0161, which matches the symptom of Roslyn issue #85265 and
  was not confirmed to be it; switch expressions are fine. Unions do not
  flatten when nested (CS8121) and a case does not convert through two unions
  (CS0029), which is why the trial used one closed hierarchy inside one
  generic union. `dotnet format` handles `union` and `closed` normally. No
  analyzer raised a diagnostic.
- **A struct case boxes.** Inference from the documented lowering to
  `object? Value`: `Outcome<SemanticVersion>` boxes the struct.
- **It gives no bind either.** Each passing frame is a two-arm switch, as
  above.

### 5. C# 15 union types on `net10.0` with a polyfill

The trial on `0151/trial-union` at `79bcfef`, one commit on top of option 4
and touching no test. Read from the commit: the target returns to `net10.0`
and a new project, `src/polyfill` (assembly `specht.polyfill`), declares three
public types in `System.Runtime.CompilerServices`:

```csharp
public interface IUnion
{
    object? Value { get; }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class IsClosedTypeAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class UnionAttribute : Attribute;
```

Facts, measured:

- It needs the RC compiler regardless of the target.
- It needs three types, not the two Microsoft's page lists: without
  `IsClosedTypeAttribute` the build fails CS0656 and CS0518.
- Declared internal inside `specht.dll` the build passes and the acceptance
  scenario for `0001-F1` B-005 fails, because the engine assembly then
  carries a `System.Runtime.CompilerServices` namespace. The committed form
  is therefore a separate public assembly shipping `System.*` types.
- Microsoft documents a language version newer than the target framework's
  as unsupported.

### Not trialled, and excluded

- **A hand-written result type on C# 14.** Not trialled, by the owner's
  choice (item `0151`, decision 2). It is not rejected here; question 2
  reopens it.
- **language-ext 4.4.9.** Excluded by the item's scope: the last stable
  release, from 2024, with ten package dependencies.

## What the trials measured

### Build, tests and behaviour

Measured on each of the four trial commits: `./build.sh` exit 0; unit
421/421, integration 148/148, acceptance 254 total with 91 passed, 163
skipped and 0 failed - the same as the baseline. `./build.sh Format` exit 0
on each. Each trial compared stdout, stderr and exit code against a tool
built from `fcaa0b8` over synthetic roots and found no difference:
language-ext over 26 roots, Dunet over 32, the union over 73 for each of its
two commits.

### The eight measures

| Measure                           | language-ext v5 beta                                                                                                           | Dunet                                                                                          | C# 15 union, `net11.0`                                                                  | C# 15 union, `net10.0` + polyfill     |
| --------------------------------- | ------------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------- | ------------------------------------- |
| Failure type                      | `Either<SpechtFailure, T>`; `SpechtFailure(SpechtFailureKind Kind, string Message)` over a four-member enum                    | generated `InputFailure` (4 cases) and `Result<T>` (`Ok`, `Failed`)                            | `closed record InputFailure` with 4 sealed records; `union Outcome<T>(T, InputFailure)` | same as `net11.0`                     |
| `git diff --shortstat` `-- src`   | 13 files, +285 -275                                                                                                            | 13 files, +358 -233                                                                            | 15 files, +333 -213                                                                     | 20 files, +365 -213 (cumulative)      |
| `git diff --shortstat` `-- test`  | 17 files, +145 -126                                                                                                            | 17 files, +153 -121                                                                            | 17 files, +126 -126                                                                     | same; the commit touches no test      |
| `SpecManifest.cs`, from 276 lines | 287                                                                                                                            | 379                                                                                            | 368                                                                                     | 368                                   |
| Fifth failure left unhandled      | build fails, CS8509, names the enum member - from the enum, not the library; the switch needs `#pragma warning disable CS8524` | build fails: CS1501 through generated `Match`, CS8509 through `switch`; neither names the case | build fails, CS8509, names the missing case, across the assembly boundary; no pragma    | same as `net11.0`                     |
| Library type in `src/tool`        | yes: `using LanguageExt;` and `Either<,>` in `Program.cs` and the `CheckCommand` constructor                                   | no type or `using`; only generated members (`Match`)                                           | `Outcome<SpechtReport>` and the case type names; the engine's own types                 | same                                  |
| Packages added                    | `LanguageExt.Core`, no transitive packages                                                                                     | `Dunet`, which ships a runtime `Dunet.dll`                                                     | none                                                                                    | none; one new project, `src/polyfill` |
| `.nupkg` from `./build.sh Pack`   | 3,765,181 bytes                                                                                                                | 2,203,892 bytes                                                                                | 2,146,430 bytes                                                                         | 2,185,819 bytes                       |
| Largest added assembly            | `LanguageExt.Core.dll`, 5,327,872 bytes                                                                                        | `Dunet.dll`, 11,776 bytes                                                                      | none                                                                                    | `specht.polyfill.dll`, 5,120 bytes    |

The two `--shortstat` rows and the line counts are read from the commits
against `fcaa0b8`. The rest is measured by the trials. The baseline `.nupkg`
from `./build.sh Pack` at `ff62ba9`, whose sources are `fcaa0b8`'s, is
2,171,792 bytes; the baseline `specht.dll` is 197,632 bytes (Debug).

### The cost common to all three

These came with every candidate, so they are the price of returning a value,
not of any library. Read from the commits unless marked:

- **21 throw assertions rewritten**: 16 in `SpecManifest.Unit.Tests.cs`, 3 in
  `SpecSchemas.Unit.Tests.cs`, 1 in `SchemaVersions.Unit.Tests.cs` and 1 in
  `SpecManifest.Integration.Tests.cs`. The last of them, before:

  ```csharp
  var run = () => tree.Run();

  run.Should().Throw<SpechtManifestException>().Which.Message.Should().Contain("glossary");
  ```

  After, with language-ext:

  ```csharp
  var failure = (SpechtFailure)SpechtRunner.Run(tree.Root);

  failure.Kind.Should().Be(SpechtFailureKind.ManifestRejected);
  failure.Message.Should().Contain("glossary");
  ```

  After, with Dunet:

  ```csharp
  var run = SpechtRunner.Run(tree.Root);

  run.Should().BeOfType<Result<SpechtReport>.Failed>()
      .Which.Failure.Should().BeOfType<InputFailure.ManifestRejected>().Which.Message.Should().Contain("glossary");
  ```

  After, with the C# 15 union:

  ```csharp
  var run = SpechtRunner.Run(tree.Root);

  run.Value.Should().BeOfType<ManifestRejected>().Which.Message.Should().Contain("glossary");
  ```

- **58 success-path call sites in tests needed a mechanical unwrap**, because
  every caller of a changed signature stopped compiling: 58 casts for
  language-ext, 58 `.UnwrapOk().Value` for Dunet, 58 `(T)….Value!` for the
  union.
- **One more cast in each.** A test lambda returning `SpechtReportFixture`
  needs `(SpechtReport)new SpechtReportFixture()`, because two user-defined
  conversions do not chain (CS0029, measured).
- **3 `load.Should().NotThrow()` assertions would pass vacuously.** Two in
  `SpecManifest.Unit.Tests.cs` and one in `SpecSchemas.Unit.Tests.cs` keep
  compiling after the change and assert nothing unless rewritten. Measured:
  no diagnostic warns of it. The trials rewrote all three.
- **1 assertion dropped.** The unreadable-manifest failure no longer carries
  the parser's inner exception
  (`thrown.InnerException.Should().NotBeNull()`). It was never printed.
- **One `catch` remains on the path in every trial.** `JsonNode.Parse` has no
  non-throwing form. Dunet and the union catch `JsonException` alone;
  language-ext hides it in `Try.lift`, which catches `Exception` unfiltered.
- **Three exception types deleted**; `SpechtRootNotFoundException` stays for
  `init`.

### Start-up

Measured once for all five builds: each tool published with
`dotnet publish -c Release`, run through the same `dotnet` host, 40
interleaved runs after one warm-up, wall-clock milliseconds, as median / min
/ p90.

| Build                       | Check this repository's own tree (30 features, 21 rules, exit 1) | `--root` a missing directory (exit 2) |
| --------------------------- | ---------------------------------------------------------------- | ------------------------------------- |
| baseline, `fcaa0b8`         | 485.6 / 480.2 / 493.0                                            | 120.1 / 117.5 / 122.6                 |
| language-ext                | 541.4 / 532.8 / 553.7                                            | 133.3 / 129.9 / 136.3                 |
| Dunet                       | 488.5 / 482.7 / 502.6                                            | 118.6 / 114.7 / 122.8                 |
| union, `net10.0` + polyfill | 487.1 / 477.9 / 493.6                                            | 119.3 / 115.5 / 121.2                 |
| union, `net11.0`            | 491.7 / 485.0 / 501.6                                            | 118.2 / 114.4 / 119.9                 |

This is coarse wall-clock timing on one machine, not a BenchmarkDotNet
measurement. Only the language-ext difference stands clear of the noise:
about 56 ms (11%) on the check and 13 ms on the failure path. Published
folder size: baseline 4,060 KB, Dunet 4,132, language-ext 9,292, union
`net10.0` 4,088, union `net11.0` 3,940.

### The two hard places

**The check that collects every fault and fails once (`SpecManifest.Check`).**
The original was already an accumulation: a LINQ chain of `Concat`s into a
list, then one throw. Read from the commits:

- Dunet and the union leave the chain untouched and change its tail to one
  expression. The union's:

  ```csharp
  return faults.Count > 0 ? new ManifestRejected($"{RelativePath}: {string.Join("; ", faults)}.") : structure;
  ```

- language-ext with `Validation<Seq<string>, Unit>` is longer: a `Faults(...)`
  wrapper around each of the seven groups, joined with `&`, then

  ```csharp
  .ToEither()
  .MapLeft(static faults => new SpechtFailure(SpechtFailureKind.ManifestRejected, $"{RelativePath}: {string.Join("; ", faults)}."))
  .Map(_ => structure);
  ```

  Measured by the trial: the detour through `ToEither` is needed because
  `MapFail` needs a `Monoid` target. The trial's own judgement is that the
  plain form would be shorter.

**The malformed-shape path.** Every trial had to rewrite every reader
(`Strings`, `Tables`, `Named`, `Levels`, `Filled`, `Discovery`, `Layout`),
because the shape check stops being an exception. Read from the commits:

- language-ext: the readers return `Either` and one LINQ query in `Read`
  composes them; `SpecManifest.cs` grows by 11 lines. The awkward part is the
  traversal, `toSeq(array).Traverse(read).As().Map(static items => items.ToList())`.
- Dunet and the union: neither gives a bind, so the readers return nullable
  and `Read` is a chain of `is not { }` tests. `SpecManifest.cs` grows by 103
  lines with Dunet and 92 with the union. The union needed `Read` split into
  `Read` and `Pinned` around the `schemaVersion` result.
- In all three the order in which failures win had to be kept by hand to
  hold stderr identical. `Read` tests `sections`, `tables` and `identifiers`,
  then `schemaVersion`, then the rest, because that is the order today's
  throws happen to fall in.
- One reader keeps its throwing accessors in all three: `IdentityFaults`
  still calls `AsArray()`, `AsObject()` and `GetValue<string>()`. Inference
  from reading the code: it is reached only after `Discovery` has accepted
  the same nodes, so it cannot throw, but nothing in the types says so.

## Decision

Proposed. Each part is a judgement resting on the measurements above; the
owner decides.

### (a) Reject language-ext for this engine - judgement

On these facts: the exhaustiveness came from an enum and a pragma, not from
the library (driver 1); the one feature it was expected to help with,
`Validation`, made the accumulating check longer; it puts `Either<,>` into
the host (driver 2); it adds a 5.3 MB assembly, 73% to the package and about
11% to start-up (driver 3); it is a beta whose used surface broke eleven
months ago and whose author may hold the release for C# unions (driver 5);
and `0001-F1` C-7 would need an ADR to carry all of that for four failure
cases. What it does better than the others is the reader composition: a
bind, and 11 added lines where they added about 100.

### (b) Reject Dunet as a bridge - judgement

It works today and is light: 11,776 bytes, no start-up cost outside the
noise, no library type in the host. On these facts it is still not proposed:
its `switch` exhaustiveness is an analyzer suppression that does not cross a
project boundary unless its runtime assembly ships; neither of its
diagnostics names the missing case; and the later move to a native union
re-edits roughly the same hundred sites (inference). That is two migrations
to save a wait that ends on 2026-11-10 (driver 4).

### (c) Reject the `net10.0` polyfill - judgement

On two facts: Microsoft documents the configuration as unsupported, and it
ships a public assembly declaring `System.*` types (driver 5).

### (d) Propose the native union on `net11.0` as the direction, not yet as a change - judgement

It gave the strongest check: the diagnostic names the missing case, across
assemblies, with no package and no suppression. It gave the flattest
command, and no run-time cost that stands clear of the noise. It is gated on
three things this spike cannot settle:

1. **General availability**, 2026-11-10.
2. **A NUKE that runs on the .NET 11 runtime**, or the two build workarounds
   accepted as they stand in the trial commit.
3. **The owner's decision on the target.** Whether `specht.tool` moves from
   `net10.0`, long-term support, to `net11.0`, standard-term support. A
   `dotnet tool` targeting `net11.0` needs that runtime on every consumer's
   machine and CI.

**Until all three hold, exceptions stay** (option 1), and no code on `main`
changes under this record.

### The size of the prize

Four failure cases today. The gain is a compiler-checked fold, the
over-broad filter gone, and a shape the unbuilt exit-`4` commands
(`--explain`, the pin command, `upgrade`) can extend with new cases safely.
The price, read from the union trial's commit: a net 120 more lines under
`src/`, 92 of them in the manifest readers; 21 assertions rewritten; 58
unwraps; and the lost inner exception. Judgement: neither the gain nor the
price is large.

## Consequences

Buys, if the owner accepts the direction and the three gates hold:

- A fifth input failure the command does not fold is a build error that
  names it, so `0001-F2` C-2 is held by the compiler instead of by a count of
  catch clauses.
- The filter at `SpecManifest.cs:59` is gone: a defect that throws
  `InvalidOperationException` is no longer reported as an unreadable
  manifest.
- What a loader can fail with is in its return type, not in `<exception>`
  documentation.
- No package, so `0001-F1` C-7 is untouched.
- The exit-`4` commands add a case to a closed hierarchy and every fold that
  misses it fails to compile.

Costs:

- The manifest readers grow by about 90 lines and lose the implicit shape
  check; the order in which failures win is kept by hand.
- Neither the union nor Dunet gives a bind; every passing frame is a two-arm
  switch.
- 21 assertions, 58 unwraps and one cast in tests, and three `NotThrow`
  assertions that must be rewritten by hand because nothing flags them.
- The unreadable-manifest failure loses the parser's inner exception.
- The tool's target leaves long-term support, and every consumer needs the
  .NET 11 runtime.
- The build carries two NUKE workarounds until NUKE runs on .NET 11.
- One `catch` stays, around `JsonNode.Parse`.

If the owner does not accept the direction, the cost is what stands today:
the unchecked ladder and the filter, the second of which question 3 can
still remove.

Specification deltas owed if the owner accepts the direction. Each is owed,
none is made here:

- **`0001-F2` § 7**, rewritten: input failures are values the command folds.
  Its three-versus-four count is wrong today and is owed a fix now,
  whichever way the owner rules.
- **`0001-F2` C-2**, its "caught at the command" wording.
- **ADR-0002**, whose Context says the short-circuit is
  `SpechtManifestException`. It is accepted and so immutable; its status
  records the sentence as superseded by this record.
- **The `<exception>` documentation contract** on the engine's public
  loaders, which becomes `<returns>`.
- **Untouched**: `0001-F1` C-7 by the native union, and ADR-0005 (c)'s fault
  isolation around a throwing rule by every option.

Work on acceptance is its own item, depending on `0151`, cut after the owner
rules (item `0151`, out of scope row 1).

A finding outside the decision. `src/tool/tool.csproj` declares
`<PackageId>tool</PackageId>`, read from `fcaa0b8`, where `AGENTS.md` says
the package is `specht.tool`. Inference, since `Pack` was not re-run for this
record: `./build.sh Pack` therefore names the file `tool.<version>.nupkg`. No
trial changed the project file.

The questions this record closes on, for the owner:

1. **The target.** Does `specht.tool` move to `net11.0` after general
   availability, and if so when?

2. **If not.** Is the question closed - keep exceptions - or reopened with
   the hand-written C# 14 result type that was not trialled? Inference: the
   trials suggest the type design carries over to it unchanged - one closed
   four-case failure and one generic two-case result - with the
   exhaustiveness check being what a hand-written type would have to supply.

3. **Two defects on `main` today that need no union.** The filter at
   `SpecManifest.cs:59` relabels a genuine `InvalidOperationException` or
   `FormatException` as "manifest unreadable", and `0001-F2` § 7 names three
   caught types where the command catches four. Should each be filed as its
   own item?
