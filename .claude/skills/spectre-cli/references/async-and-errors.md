---
title: Async & Errors
description: AsyncCommand<T>, cancellation, SetExceptionHandler, PropagateExceptions, exit codes, and ICommandInterceptor.
type: reference
---

# Async & Errors

## `AsyncCommand<TSettings>`

Use `AsyncCommand<TSettings>` for every command that does I/O or business logic — never the
synchronous `Command<TSettings>`.

```csharp
internal sealed class FetchCommand : AsyncCommand<FetchCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<url>")]
        public required string Url { get; init; }
    }

    public override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetStringAsync(settings.Url, cancellationToken);
        return 0;
    }
}
```

## Cancellation

Wire `Console.CancelKeyPress` (Ctrl+C) to a `CancellationTokenSource` and run through `RunAsync`,
not `Run`:

```csharp
var cancellationTokenSource = new CancellationTokenSource();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cancellationTokenSource.Cancel();
};

var app = new CommandApp<FetchCommand>();
return await app.RunAsync(args, cancellationTokenSource.Token);
```

The framework passes the token straight through to `ExecuteAsync` — forward it to every awaited
call. Catch `OperationCanceledException` where a graceful cancellation message or a specific exit
code is needed.

## Exception Handling

**Default behavior**: unhandled exceptions are caught, a user-friendly message is printed, and the
process exits with `-1`.

**`SetExceptionHandler`** — centralized exception-to-exit-code mapping, the recommended default for
most apps:

```csharp
app.Configure(config =>
{
    config.SetExceptionHandler((ex, resolver) =>
    {
        AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths);

        return ex switch
        {
            InvalidOperationException => 2,
            FileNotFoundException => 3,
            _ => 1
        };
    });
});
```

`resolver` (`ITypeResolver`) is `null` when the exception occurred during parsing, before DI is
available.

**In this repository** no handler is set: the command catches the engine's typed input failures
around the runner call and returns the `ExitCodes` constant, because `CommandAppTester` composes its
own app and never reaches a handler set in `Program.cs` — `0001-F2` C-2 and § 7 of the
[Check specification](../../../../src/specht.tool/Features/Check/.spec/README.md).

**`PropagateExceptions()`** — re-throws instead, for apps that need their own `try`/`catch` around
`app.Run()` (layered error handling, custom logging integration, exception-specific properties):

```csharp
app.Configure(config => config.PropagateExceptions());

try
{
    return app.Run(args);
}
catch (FileNotFoundException ex)
{
    AnsiConsole.MarkupLine($"[red]File not found:[/] {ex.FileName}");
    return 3;
}
```

**These two are mutually exclusive.** Enabling `PropagateExceptions()` means the exception handler
configured via `SetExceptionHandler` no longer runs — exceptions go straight to your `catch` blocks.
Pick one strategy per app; don't configure both expecting layered behavior.

## `ICommandInterceptor`

For cross-cutting concerns (logging, timing, auth checks) that apply to every command without
duplicating code in each one:

```csharp
public sealed class TimingInterceptor : ICommandInterceptor
{
    private Stopwatch? _stopwatch;

    public void Intercept(CommandContext context, CommandSettings settings)
    {
        _stopwatch = Stopwatch.StartNew();
        Console.WriteLine($"Starting command: {context.Name}");
    }

    public void InterceptResult(CommandContext context, CommandSettings settings, ref int result)
    {
        _stopwatch?.Stop();
        Console.WriteLine($"Command completed in {_stopwatch?.ElapsedMilliseconds}ms (exit code: {result})");
    }
}
```

```csharp
app.Configure(config =>
{
    config.SetInterceptor(new TimingInterceptor());
    config.AddCommand<ProcessCommand>("process");
});
```

`Intercept` runs after settings validation and before command execution; `InterceptResult` runs
after command execution and can rewrite the exit code via the `ref int result` parameter. The same
interceptor instance handles both calls, so instance state (like the `Stopwatch` above) can flow
between them.
