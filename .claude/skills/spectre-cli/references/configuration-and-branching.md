---
title: Configuration & Branching
description: CommandApp.Configure(), AddCommand, AddBranch, the default-command pattern, aliases, examples, and hiding commands/options.
type: reference
---

# Configuration & Branching

## Registering Commands

```csharp
var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("myapp");
    config.SetApplicationVersion("1.0.0");

    config.AddCommand<AddCommand>("add")
        .WithDescription("Add a new item.")
        .WithAlias("a")
        .WithExample("add", "todo.txt");

    config.AddCommand<RemoveCommand>("remove")
        .WithDescription("Remove an item.")
        .WithAlias("rm")
        .WithAlias("delete");
});
```

Chainable configuration methods:

- `WithDescription()` — short description shown in help.
- `WithAlias()` — alternative command name; chain for multiple aliases.
- `WithExample()` — usage example rendered in help output.

## Global Parsing Settings

`config.Settings` controls parsing behavior:

| Setting           | Purpose                                                                   |
| ----------------- | ------------------------------------------------------------------------- |
| `CaseSensitivity` | Whether commands/options are matched case-sensitively.                    |
| `StrictParsing`   | When `false`, unknown flags become remaining arguments instead of errors. |

## Default Command Pattern

For a single-command app, skip explicit registration entirely:

```csharp
var app = new CommandApp<ListCommand>();
```

## Branching (Grouped Subcommands)

Use `AddBranch` for hierarchical commands, mirroring patterns like `git remote add` /
`git remote remove`:

```csharp
config.AddBranch<RemoteSettings>("remote", remote =>
{
    remote.SetDescription("Manage remote repositories.");
    remote.AddCommand<RemoteAddCommand>("add");
    remote.AddCommand<RemoteRemoveCommand>("remove");
    remote.AddCommand<RemoteListCommand>("list");
});
```

Subcommand settings inherit the branch's settings class so shared options work both before and
after the subcommand name:

```csharp
public class RemoteSettings : CommandSettings
{
    [CommandOption("-v|--verbose")]
    public bool Verbose { get; init; }
}

public class RemoteAddSettings : RemoteSettings
{
    [CommandArgument(0, "<name>")]
    public required string Name { get; init; }
}
```

For grouping-only branches (no shared settings), use the non-generic overload — this also nests
arbitrarily deep:

```csharp
config.AddBranch("cloud", cloud =>
{
    cloud.AddBranch("storage", storage =>
    {
        storage.AddCommand<UploadCommand>("upload");
    });
});
```

Produces: `myapp cloud storage upload`.

## Hiding Commands & Options

Hidden items stay fully functional — they just don't appear in help. Use for internal tooling,
deprecated flags, or advanced options that would clutter the default help output.

**Hidden command:**

```csharp
config.AddCommand<DiagnosticsCommand>("diagnostics")
    .WithDescription("Internal diagnostics.")
    .IsHidden();
```

**Hidden option:**

```csharp
[CommandOption("--skip-hooks", IsHidden = true)]
[Description("Skip deployment hooks (internal use).")]
public bool SkipHooks { get; init; }
```

## Development-Only Validation

Gate stricter startup checks behind `#if DEBUG` so they never run in production:

```csharp
#if DEBUG
    config.PropagateExceptions();  // Full stack traces
    config.ValidateExamples();     // Verify registered examples parse correctly at startup
#endif
```

`ValidateExamples()` catches example/configuration drift immediately instead of surfacing as a
confusing runtime error later.
