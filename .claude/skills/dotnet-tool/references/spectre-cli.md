---
title: Spectre.Console.Cli
description: dotnet-tool-specific Spectre.Console.Cli patterns — CommandApp choice, the command as an entry point, and settings composition for vertical-slice tools.
type: reference
---

# Spectre.Console.Cli

For the full framework reference — `CommandOption`/`CommandArgument` attributes, `AddBranch`,
`Validate()`, `FlagValue<T>`, `ITypeRegistrar`/`ITypeResolver`, help customization, testing, and the
execution lifecycle — see the [spectre-cli](../../spectre-cli/SKILL.md) skill. This file covers only
what's specific to organizing a `dotnet tool`: which `CommandApp` shape to pick, keeping a command
an entry point, and composing settings across a vertical-slice tool.

## `CommandApp` vs `CommandApp<T>`

| Pattern                      | When to use                                        |
| ---------------------------- | -------------------------------------------------- |
| `CommandApp<TCommand>`       | Single-command tool — no subcommand routing needed |
| `CommandApp` + `Configure()` | Multi-command tool — commands and branches         |

## `CommandSettings` and the command as entry point

Nest the `Settings` class inside its command — never a separate file for a single command's
settings — and keep the command an entry point: parse, hand the work to the collaborator the slice
owns, fold the result into an exit code. Business logic does not live in the command:

```csharp
public sealed class IssueCreateCommand : AsyncCommand<IssueCreateCommand.Settings>
{
    public sealed class Settings : BaseSettings, IHaveTitle, IHaveBody
    {
        [CommandOption("--title <TITLE>")]
        [Description("The issue title.")]
        public required string Title { get; init; }

        [CommandOption("--body <BODY>")]
        [Description("The issue body (Markdown).")]
        public string? Body { get; init; }

        public override ValidationResult Validate()
        {
            if (string.IsNullOrWhiteSpace(Title))
                return ValidationResult.Error("--title is required.");
            return ValidationResult.Success();
        }
    }

    private readonly IIssueCatalog _issues;
    public IssueCreateCommand(IIssueCatalog issues) => _issues = issues;

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        await _issues.CreateAsync(settings.Title, settings.Body);
        return 0;
    }
}
```

## Settings Inheritance Patterns

### Inheritance hierarchy (single-domain tools)

Use when commands share a common set of global options (e.g., `--output-dir`, `--verbose`):

```csharp
public abstract class BaseSettings : CommandSettings
{
    [CommandOption("--output <PATH>")]
    public string OutputDir { get; init; } = ".";

    [CommandOption("--verbose")]
    public bool Verbose { get; init; }
}

public sealed class RunSettings : BaseSettings
{
    [CommandOption("--input <FILE>")]
    public required string Input { get; init; }
}
```

### Mixin interfaces (multi-domain tools)

Use when multiple unrelated commands share individual options, composing them via interfaces
instead of forcing every command through one inheritance chain:

```csharp
// Marker interfaces
public interface IHaveTitle { string Title { get; } }
public interface IHaveBody { string? Body { get; } }

// Shared base adds --repo
public class BaseSettings : CommandSettings
{
    [CommandOption("--repo <REPO>")]
    public required string Repo { get; init; }
}

// Command composes what it needs
public sealed class IssueCreateCommand : AsyncCommand<IssueCreateCommand.Settings>
{
    public sealed class Settings : BaseSettings, IHaveTitle, IHaveBody { ... }
}
```
