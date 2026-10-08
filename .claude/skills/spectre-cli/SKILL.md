---
name: spectre-cli
description:
  Building command-line applications with Spectre.Console.Cli — commands, settings, validation,
  branching, help customization, dependency injection, testing, and the convention-over-configuration
  execution pipeline. Use when scaffolding CLI commands, wiring options/arguments, customizing help
  output, testing CLI parsing, or reasoning about command lifecycle and exit codes.
---

# Spectre.Console.Cli

Spectre.Console.Cli is a convention-over-configuration framework for building command-line
interfaces in .NET. A command separates input (a `CommandSettings` class) from execution logic
(a `Command<T>`/`AsyncCommand<T>` class); the framework owns parsing, validation, help generation,
and DI wiring.

For `dotnet tool` packaging and vertical-slice organization on top of this framework, see
[dotnet-tool](../dotnet-tool/SKILL.md).

## Index

- [Commands & Settings](references/commands-and-settings.md): `CommandArgument`/`CommandOption`
  attributes, required options, `Validate()`, flag values, dictionary/lookup options, custom type
  converters.
- [Configuration & Branching](references/configuration-and-branching.md): `CommandApp.Configure()`,
  `AddCommand`, `AddBranch`, the default-command pattern, aliases, examples, hidden commands/options.
- [Help & Output](references/help-and-output.md): help text customization, `HelpProviderStyles`,
  custom `IHelpProvider`, injecting `IAnsiConsole` for testable output.
- [Async & Errors](references/async-and-errors.md): `AsyncCommand<T>`, cancellation,
  `SetExceptionHandler`, `PropagateExceptions`, exit codes, `ICommandInterceptor`.
- [Dependency Injection](references/dependency-injection.md): the `ITypeRegistrar`/`ITypeResolver`
  bridge to `Microsoft.Extensions.DependencyInjection`.
- [Testing](references/testing.md): `CommandAppTester`, `TestConsole`, asserting parsed settings and
  exit codes.
- [Lifecycle & Built-ins](references/lifecycle-and-behaviors.md): the `app.Run()` execution pipeline
  stage by stage, built-in `cli` commands, and the design philosophy behind the conventions.

## Quick Start

**Single-command app:**

```csharp
var app = new CommandApp<RunCommand>();
return app.Run(args);
```

**Multi-command app with a branch:**

```csharp
var app = new CommandApp();
app.Configure(config =>
{
    config.SetApplicationName("mytool");
    config.AddCommand<DeployCommand>("deploy").WithDescription("Deploy the application.");
    config.AddBranch("issue", issue =>
    {
        issue.SetDescription("Manage issues.");
        issue.AddCommand<IssueListCommand>("list");
        issue.AddCommand<IssueCreateCommand>("create");
    });
});
return app.Run(args);
```

**A command:**

```csharp
public sealed class DeployCommand : AsyncCommand<DeployCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<environment>")]
        [Description("Target environment.")]
        public required string Environment { get; init; }

        [CommandOption("-f|--force")]
        [Description("Skip confirmation prompts.")]
        public bool Force { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        // ...
        return 0;
    }
}
```

## Mandatory Requirements

- **USE** `AsyncCommand<TSettings>` for every command — never `Command<TSettings>` when the command
  performs I/O or business logic.
- **NEST** the `Settings` class inside its command — never a standalone file for a single command's
  settings.
- **INJECT** `IAnsiConsole` into commands instead of calling static `AnsiConsole.*` members — this is
  what makes output capturable by `CommandAppTester`/`TestConsole`.
- **OVERRIDE** `Validate()` on `CommandSettings` for cross-field constraints; use
  `isRequired: true` on `[CommandOption]` for single-option requirements — do not reimplement
  required-ness checks by hand.
- **USE** `AddBranch` for grouped subcommands (e.g. `tool issue list`) — never hand-roll subcommand
  routing by inspecting `args` directly.
- **PREFER** `SetExceptionHandler` for centralized exception-to-exit-code mapping; reach for
  `PropagateExceptions()` only when the host app needs its own `try`/`catch` around `app.Run()`.
- **RETURN** semantic exit codes (`0` success, a distinct non-zero value per error category) — never
  let an exception cross the command boundary unhandled.
- **NEVER** write manual help text or hand-parse `-h`/`--help` — customize via `HelpProviderStyles`
  or a custom `IHelpProvider`.

## Edge Cases

- An optional flag with an optional value (`--port` present vs. `--port 8080` vs. absent) needs
  `FlagValue<T>` with a bracketed template (`--port [PORT]`) — a nullable `int?` cannot distinguish
  "present, no value" from "absent."
- A shared option must work both before and after a subcommand name (`tool -v issue list` and
  `tool issue list -v`) — make the subcommand's `Settings` inherit the branch's `Settings`.
- Passing unmatched arguments through to a wrapped external process — read `context.Remaining.Raw`,
  not `context.Arguments`.
- `SetExceptionHandler` and `PropagateExceptions()` are mutually exclusive — enabling
  `PropagateExceptions()` stops the configured exception handler from firing; pick one strategy per
  app.
