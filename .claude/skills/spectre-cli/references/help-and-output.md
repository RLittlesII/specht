---
title: Help & Output
description: Help text customization, HelpProviderStyles, custom IHelpProvider, and injecting IAnsiConsole for testable output.
type: reference
---

# Help & Output

Spectre.Console.Cli generates `-h`/`--help` output from your command definitions automatically —
never write manual help strings or hand-parse `--help`.

## Application Identity & Examples

```csharp
var app = new CommandApp<DeployCommand>();

app.Configure(config =>
{
    config.SetApplicationName("myapp");
    config.SetApplicationVersion("1.2.0");

    config.AddExample("production");
    config.AddExample("staging", "--force");
    config.AddExample("dev", "--dry-run", "--verbose");
});
```

## Styling Help Output

Customize colors and formatting via `HelpProviderStyles`:

```csharp
config.Settings.HelpProviderStyles = new HelpProviderStyle
{
    Description = new DescriptionStyle { Header = "bold blue" },
    Options = new OptionStyle { RequiredOption = "bold red" },
    Arguments = new ArgumentStyle { RequiredArgument = "bold green" },
};
```

Available style classes: `DescriptionStyle`, `ArgumentStyle`, `OptionStyle`, `CommandStyle`,
`ExampleStyle`.

Set it to `null` for plain, unstyled text — needed for accessibility, or when output is piped to a
file:

```csharp
config.Settings.HelpProviderStyles = null;
```

## Custom Help Providers

Implement `IHelpProvider` for full control, or extend the built-in `HelpProvider` for selective
overrides:

```csharp
app.Configure(config =>
{
    config.SetHelpProvider(new CustomHelpProvider(config.Settings));
});
```

Reach for a custom provider only when styling knobs above aren't enough — most help customization
needs are covered by `HelpProviderStyles` and `[Description]`/`WithExample()` alone.

## `IAnsiConsole` Injection

Inject `IAnsiConsole` into commands instead of calling static `AnsiConsole.*` members:

```csharp
public sealed class GreetCommand : Command<GreetCommand.Settings>
{
    private readonly IAnsiConsole _console;

    public GreetCommand(IAnsiConsole console) => _console = console;

    public override int Execute(CommandContext context, Settings settings)
    {
        _console.MarkupLine($"[green]Hello, {settings.Name}![/]");
        return 0;
    }
}
```

This is what allows `CommandAppTester`/`TestConsole` to capture and assert on rendered output — see
[Testing](testing.md). A command that only ever calls static `AnsiConsole` methods cannot have its
output verified in a unit test.
