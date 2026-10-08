---
title: Conventions
description: Exit codes, why this tool has no secret, AnsiConsole markup, stderr, repository-relative paths, and the central-package and zero-warning rules.
type: reference
---

# Conventions

## Exit Codes

Return semantic exit codes from `ExecuteAsync`. Spectre passes them through to the shell.
Non-zero codes make CI pipelines fail correctly when the tool encounters an error.

Define a static class — never use magic numbers inline:

```csharp
public static class ExitCodes
{
    public const int Success = 0;
    public const int Failure = 1;
    public const int MissingInput = 2;
    public const int InvalidConfiguration = 3;
    public const int ExternalServiceError = 4;
    public const int Unauthorized = 5;
    public const int NotFound = 6;
}
```

Return the appropriate code from `ExecuteAsync`:

```csharp
public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
{
    if (!File.Exists(settings.Input))
    {
        return ExitCodes.MissingInput;
    }

    await _runner.RunAsync(settings.Input);
    return ExitCodes.Success;
}
```

`specht`'s codes are decided (README § 5) and the pre-commit hook and CI depend
on them:

| Code | Means |
| ---- | ----- |
| `0`  | clean |
| `1`  | violations — any violation at all under `--strict`, otherwise at least one error |
| `2`  | the root or the manifest (`.spec/schema/spec-structure.schema.json`) was not found |
| `3`  | the manifest is invalid |

`--json` and `--report` change what is written, never the exit code.

## Secrets

**Never** accept an API key, token or signing secret as a `--flag` option — it
lands in shell history and in process lists.

This tool needs no credential. It reads a repository on disk and writes a report;
it is **deterministic and offline** and never calls GitHub. A command that would
need a secret is a design question for the specification, not a `--flag` — and
the environment-variable pattern below is the generic answer should one ever be
decided.

Use `Environment.GetEnvironmentVariable` with a null-guard throw:

```csharp
var apiKey = Environment.GetEnvironmentVariable("MYORG_API_KEY")
    ?? throw new InvalidOperationException(
        "MYORG_API_KEY is not set. Export it before running this tool.");
```

Register in DI during startup:

```csharp
var apiKey = Environment.GetEnvironmentVariable("MYORG_API_KEY")
    ?? throw new InvalidOperationException("MYORG_API_KEY is not set.");
services.AddSingleton(new ApiKeyCredential(apiKey));
```

Document required environment variables in the command `[Description]` string.

## Repository-relative paths

Every path the tool emits — a diagnostic line, a `--json` document, a `--report`
file, a log line, an exception message — is relative to the root it was given.
An absolute path anywhere is a defect (README § 9): it differs per machine, so a
report is not reproducible and a test fixture is not portable. Resolve to
absolute paths internally if you must; relativize before anything leaves the
process.

## AnsiConsole Markup

Use `AnsiConsole.MarkupLine(...)` for all terminal output. Follow this color convention:

| Use case                          | Markup            |
| --------------------------------- | ----------------- |
| Success, positive state           | `[green]text[/]`  |
| Informational (IDs, paths, names) | `[blue]text[/]`   |
| Timestamps, secondary info        | `[grey]text[/]`   |
| Warnings                          | `[yellow]text[/]` |
| Errors                            | `[red]text[/]`    |

```csharp
AnsiConsole.MarkupLine($"[green]Created[/] issue [blue]#{issue.Number}[/]: {issue.Title}");
AnsiConsole.MarkupLine($"[yellow]Warning:[/] no issues found in [blue]{settings.Repo}[/]");
AnsiConsole.MarkupLine($"[red]Error:[/] {err.Message}");
```

The violation stream is the exception: it is MSBuild-shaped plain text
(`<path>(<line>): error SPEC031: …`) so GitHub annotates it on the diff, and it
carries no markup. Markup is for the summary lines around it.

## Error Output

Use `AnsiConsole` for human-readable error display. For machine-readable output (CI log parsing,
piped scripts), write to stderr:

```csharp
Console.Error.WriteLine($"Error: {err.Message}");
```

**Rule**: Never write error messages to stdout. Consumers pipe stdout; errors on stdout corrupt the output stream.
Under `--json` that rule is load-bearing: stdout is the report document and nothing else.

## Dependency Version Pinning

Versions are central: a `PackageVersion` in `Directory.Packages.props` and a
versionless `PackageReference` in the project, added in the same change. A
`Version` attribute in a `.csproj` is a violation, and
`CentralPackageTransitivePinningEnabled` is on.

Floating ranges of the `[0.3.*, 0.4.0)` shape are what the central file exists
to avoid — it pins exact versions, with the reason beside any pin that has one.
Renovate proposes the bumps.

## `TreatWarningsAsErrors`

Set in `Directory.Build.props`, together with `EnforceCodeStyleInBuild`, which
is what turns the `.editorconfig` severities into real build failures rather
than suggestions. All code compiles with zero warnings. A suppression carries
its reason on the adjacent line.

`.build/.build.csproj` is the single exemption, so a Nuke or SDK bump cannot
block the build orchestrator from compiling. `src/Specht` and `src/Specht.Tool`
get no such exemption.

## Secrets Never in Source

- No API keys in `.csproj`, `appsettings.json`, or any committed file.
- No secrets in `[CommandOption]` definitions — shell history exposure.
- No secret or token in a log line or an exception message, including at debug
  level.
- A test uses an invented value, committed beside it. Never a real credential.
