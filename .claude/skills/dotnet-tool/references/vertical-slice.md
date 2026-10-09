---
title: Vertical Slice Organization
description: Feature-first folder structure for .NET tools — cohesion over technical layering — and how a command folder joins the one host in this repository.
type: reference
---

# Vertical Slice Organization

## In this repository

There is one host, `src/specht.tool`, and one slice per Spectre command:

```
src/specht.tool/
  Program.cs                    the composition root - a hand-written list, nothing else
  Features/
    Check/                      `specht [--root] [--report] [--strict] [--json]` - the default command
      CheckCommand.cs           command class + nested Settings
    Init/                       `specht init` - writes schema and templates, never overwrites
      InitCommand.cs
    Upgrade/                    `specht upgrade` - moves the manifest to the next schemaVersion
      UpgradeCommand.cs
  schema/v1/                    embedded - the shipping copy of the schema set
  templates/v1/                 embedded - the shipping copy of the templates
```

The engine — discovery, the frontmatter reader, the schema loader, the Markdig
document model, the rules, the runner and the report — is the shared concern
beneath every command and lives in `src/specht`, not in a slice. A command
parses, calls `SpecCheckRunner`, folds the report into an exit code, and that is
the whole command. `--explain SPEC031` lives in `Check/` until a second caller
earns it a folder.

Tests are **not** co-located here: they live in `test/specht.tests` and
`test/specht.acceptance`. Why, and the tiers, are in
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
- **The composition root is a hand-written list.** `Program.cs` registers
  services explicitly, in plain `Microsoft.Extensions.DependencyInjection`
  through Spectre's `TypeRegistrar`. Commands are not in that list: they are
  configured on Spectre's `IConfigurator` with `AddCommand`/`AddBranch` (or
  `SetDefaultCommand`), and Spectre registers each command and settings type
  through the registrar when it runs. There is no assembly scanning and no
  third-party container (ADR-0003). Once ADR-0001's stage D lands, the
  engine's services arrive through one `AddSpechtEngine()` call in that list,
  in place of a line per engine service — owner-approved under lesson 0002.
  Adding a command means adding one line to the configuration — that is the
  whole cost, and it is deliberate: the root is readable, ordering is
  explicit, and a command cannot join the host by accident of being in the
  assembly.
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
