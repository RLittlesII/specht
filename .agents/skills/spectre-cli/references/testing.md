---
title: Testing
description: CommandAppTester, TestConsole, and asserting parsed settings and exit codes.
type: reference
---

# Testing

## Prerequisite: Inject `IAnsiConsole`

`CommandAppTester` can only capture output from commands that receive `IAnsiConsole` through DI
rather than calling static `AnsiConsole.*` members. See [Help & Output](help-and-output.md) — this
is a design decision to make upfront, not a test-time workaround.

```csharp
public class GreetCommand : Command<GreetCommand.Settings>
{
    private readonly IAnsiConsole _console;

    public GreetCommand(IAnsiConsole console) => _console = console;
}
```

## `CommandAppTester`

Install the testing package:

```bash
dotnet add package Spectre.Console.Cli.Testing
```

Run a command in-memory and assert on exit code and rendered output:

```csharp
[Fact]
public void ValidArguments_WhenRunningCommand_ShouldSucceedAndRenderOutput()
{
    // Given
    var app = new CommandAppTester();
    app.SetDefaultCommand<YourCommand>();

    // When
    var result = app.Run("arg1", "arg2");

    // Then
    result.ExitCode.Should().Be(0);
    result.Output.Should().Contain("expected text");
}
```

## Asserting Parsed Settings

`CommandAppTester.Run(...)` exposes the bound `Settings` instance for direct assertions, separate
from exit code and output:

```csharp
// Then
var settings = result.Settings as YourCommand.Settings;
settings!.PropertyName.Should().Be("expectedValue");
```

Use this to verify parsing/binding behavior (argument order, option aliasing, default values)
independently of the command's execution logic.

## Interactive Prompts — `TestConsole`

For commands that prompt for input, use `TestConsole` to script the interaction:

```csharp
var console = new TestConsole();
console.Profile.Capabilities.Interactive = true;
console.Input.PushTextWithEnter("user input");

var app = new CommandAppTester(console: console);
var result = app.Run();
```

`PushTextWithEnter` simulates typed input followed by Enter; `PushKey` simulates individual keys
(arrow navigation, Enter, Escape) for menu-style prompts.
