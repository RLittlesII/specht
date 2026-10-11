---
title: Vertical Slice Organization
description: Feature-first folder structure for .NET tools — cohesion over technical layering — and how a command folder joins the one host in this repository.
type: reference
---

# Vertical Slice Organization

## In this repository

There is one host, `src/tool`, and one slice per Spectre command:

```
src/tool/
  Program.cs                    the composition root - a hand-written list, nothing else, until
                                item 0144 moves the list into the factory (ADR-0007)
  Features/
    Check/                      `specht [--root] [--report] [--strict] [--json]` - the default command
      CheckCommand.cs           command class + nested Settings
    Init/                       `specht init` - writes schema and templates, never overwrites
      InitCommand.cs
    Upgrade/                    `specht upgrade` - brings the schema and template files to the pinned
                                schemaVersion and never moves the pin (0001-F7 decision 0005)
      UpgradeCommand.cs
  schema/v1/                    embedded - the shipping copy of the schema set
  templates/v1/                 embedded - the shipping copy of the templates
```

The engine is the shared concern beneath every command and lives in
`src/specht`, sliced the same way — one folder per Feature, and a named folder
for what more than one Feature reads:

```
src/specht/
  SpechtRunner.cs               the runner, its report and the root-not-found exception -
  SpechtReport.cs               the only .cs at the project root, namespace `Specht`
  SpechtRootNotFoundException.cs
  .spec/                        the engine Feature's specification (0001-F1)
  Model/                        shared - what every rule reads: the spec model and its loader, the Markdig
                                document, the frontmatter reader, the schema loader, the root-relative path
                                mapping, the violation and its severity
  Discovery/                    Feature 0001-F6 - finding specifications, layouts and locations
  Manifest/                     Feature 0001-F5 - the manifest, the structure it declares, its exceptions
  Report/                       Feature 0001-F3 - the report document
  Versioning/                   shared - the engine half of 0001-F7, whose home is `src/tool`:
                                the schema versions. No .spec/ here.
  Composition/                  shared - `AddSpechtEngine()`, the engine's one registration extension
                                (ADR-0001 stage D). No .spec/ here.
  Rules/                        shared - `ISpecRule`, the rules, the feature-file reader
    <Name>/.spec/               a rule Feature's specification
```

A command parses, calls `SpechtRunner`, folds the report into an exit code, and
that is the whole command. `--explain SPEC031` lives in `Check/` until a second
caller earns it a folder.

Tests are **not** co-located here: they live in `test/tests` and
`test/acceptance`. Why, the tiers, and the folders inside `test/tests` are in
[`specht-conventions` § Testing](../../specht-conventions/references/testing.md).

### The rules that make it hold

- **Every folder is earned.** A slice with one command is one folder and one
  file. A `Domain/` or `Settings/` subfolder appears when a second file needs
  it, not before — [`coding-conventions`](../../coding-conventions/SKILL.md)
  § "Design" applies to folders too.
- **One file per operation, named for the operation.** The command and its
  nested `Settings` in one file. Not a `CheckCommand.cs` and a
  `CheckCommandSettings.cs` apart.
- **No `Services/` folder.** A grab-bag `IFooService` is what this layout
  prevents. Name the behaviour and put it in the slice that owns it, or in the
  engine if two commands need it.
- **Shared types sit at the `Features/` root only once a second slice uses
  them.** A `Features/`-root type with one consumer belongs in that consumer.
- **An engine file goes in a folder, never at the project root.** A file one
  Feature owns goes in that Feature's folder. A type more than one Feature
  reads goes in a named folder — `Model/`, `Versioning/`, `Rules/`,
  `Composition/`. The root of
  `src/specht` holds the runner, its report and the root-not-found exception,
  and nothing joins them.
- **Namespaces follow folders.** `Model/` is `Specht.Model`; the root is
  `Specht`. A file that moves folder changes namespace with it.
- **Each part registers itself, and one factory composes the host.** The
  decided direction
  ([ADR-0007](../../../../.spec/adr/0007-each-part-registers-itself-and-one-factory-composes-the-host.md),
  accepted 2026-10-10): each command slice carries its own registration
  beside its command - its services on `IServiceCollection` and its
  `AddCommand` on `IConfigurator` - one static factory lists the parts in one
  fluent chain, and `Program.cs` only calls the factory. A test builds the
  same graph by calling the same factory. Item `0144` builds it; until it
  lands, `Program.cs` still holds the list, and a new command adds its lines
  there.
- **Every list is hand-written.** Services are registered explicitly, in
  plain `Microsoft.Extensions.DependencyInjection` through Spectre's
  `TypeRegistrar`. Commands are not services: they are configured on
  Spectre's `IConfigurator` with `AddCommand`/`AddBranch` (or
  `SetDefaultCommand`), and Spectre registers each command and settings type
  through the registrar when it runs. There is no assembly scanning and no
  third-party container (ADR-0003). The engine's services arrive through
  one `AddSpechtEngine()` call in the composition (ADR-0001 stage D, item
  `0107`), in place of a line per engine service — owner-approved under
  lesson 0002. The extension lives in `src/specht/Composition/` and is
  itself a hand-written list. Adding a command means adding its registration and one link
  in the composition — that is the whole cost, and it is deliberate: the
  composition is readable, ordering is explicit, and a command cannot join
  the host by accident of being in the assembly.
- **The slice's README slot is the specification.** A Feature's `.spec/README.md`
  is the twelve-section specification, sitting beside the code it specifies. Do
  not add a second, informal `README.md` beside it.

## Core Rule

**Organize by feature/domain, not by technical type.**

Put cohesive things together. A command, its settings, and the behaviour it invokes all belong in
the same feature folder. Do not scatter them across top-level technical folders.

## What NOT to Do

```
❌  src/mytool/
      Commands/
        IssueListCommand.cs
        IssueCreateCommand.cs
        PrCreateCommand.cs
      Settings/
        IssueListSettings.cs
        IssueCreateSettings.cs
      Services/
        IssueService.cs
        PrService.cs
```

This layout forces you to open multiple folders to understand one feature, and
it promotes ad-hoc `IXxxService` abstractions that answer to nothing.

## Preferred Layout

```
✅  src/mytool/
      Features/
        Issues/
          IssueListCommand.cs       ← command class + nested Settings
          IssueCreateCommand.cs
          IssueCloseCommand.cs
          Domain/
            List.cs                 ← one read operation
            Create.cs               ← one write operation
            Close.cs
        PullRequests/
          PrCreateCommand.cs
          PrGetCommand.cs
          Domain/
            Create.cs
            Get.cs
        Labels/
          LabelListCommand.cs
          Domain/
            List.cs
        Settings/                   ← shared base classes and mixin interfaces ONLY
          BaseSettings.cs
          IHaveTitle.cs
          IHaveBody.cs
          IHaveMilestoneNumber.cs
```

## Rules

1. **Each command file contains its own nested `Settings` class.** No separate settings file per command.
2. **Each feature folder owns its `Domain/` subfolder** when it has operations of its own. One operation per file, next to the command that invokes it.
3. **`Settings/` at the feature group level is for shared types only** — base classes and mixin interfaces used across multiple commands. It is not a dumping ground for every settings class.
4. **Commands dispatch; they do not compute.** A command parses, hands the work
   to whatever sits below it, and folds the result into an exit code. Here, what
   sits below is the engine in `src/specht`.
5. **No `Services/` folder.** A grab-bag `IFooService` is the smell this layout
   exists to prevent. Name the behaviour, put it in the slice that owns it, and
   add an abstraction only when a second caller or a substitution exists today -
   see [`coding-conventions`](../../coding-conventions/SKILL.md) § "Design".

## Single-Domain Tools

For tools with a single domain (no multi-noun branching), a flat layout is acceptable:

```
src/mytool/
  Cli/
    BaseSettings.cs
    RunCommand.cs
    ValidateCommand.cs
    Domain/
      Run.cs
      Validate.cs
```

`specht` is single-domain and uses the `Features/<Command>/` form above instead,
because `init` and `upgrade` write files while `check` never does — three
commands with different invariants read better as three folders than as one
flat `Cli/`.

## Registering Branches in `Program.cs`

The branch names in `AddBranch(...)` should mirror the feature folder names:

```csharp
config.AddBranch("issue", issue =>       // → Features/Issues/
{
    issue.AddCommand<IssueListCommand>("list");
    issue.AddCommand<IssueCreateCommand>("create");
});

config.AddBranch("pr", pr =>             // → Features/PullRequests/
{
    pr.AddCommand<PrCreateCommand>("create");
    pr.AddCommand<PrGetCommand>("get");
});
```

`specht` has no branches: `check` is the default command, and `init` and
`upgrade` are registered flat with `AddCommand`.
