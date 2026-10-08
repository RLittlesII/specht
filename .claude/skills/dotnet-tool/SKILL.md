---
name: dotnet-tool
description:
  Rules, concepts, and best practices for building, packaging, and distributing .NET tools
  using Spectre.Console.Cli. Use when scaffolding or reviewing a new dotnet tool project, adding commands
  to an existing tool, or advising on packaging and distribution.
---

# .NET Tool Skill

A .NET tool is a NuGet package that installs as a CLI command via `dotnet tool install`.
This skill covers the full lifecycle: project setup, CLI structure with Spectre.Console.Cli,
vertical-slice folder organization, and packaging and distribution.

## What applies to `specht`

All of it. `specht` **is** a packed dotnet tool: `src/specht.tool` carries
`PackAsTool`, `ToolCommandName=specht` and `PackageId=specht.tool`, `./build.sh Pack`
produces the package, and a consumer installs it through a local tool manifest
(`.config/dotnet-tools.json`, restored by `dotnet tool restore`). This repository
installs itself the same way, so its own build and pre-commit hook call the tool
exactly as a consumer would — never through a project reference (README § 7).

One thing is decided and worth stating once:

| Topic                     | Status here                                                                                                                                                                                                                                                                                                                            |
| ------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| What sits below a command | A command parses its settings, calls the engine in `src/specht` (`SpecCheckRunner`), and folds the report into an exit code — `0` clean, `1` violations, `2` missing root or manifest, `3` invalid manifest, `4` a named thing not found. No mediator, no `LanguageExt`; the engine is one library and the command is its only caller. |

## Index

- [Project Setup](references/project-setup.md): `.csproj` configuration, `PackAsTool`, `ToolCommandName`, `Directory.Build` patterns.
- [Spectre.Cli](references/spectre-cli.md): `CommandApp`, `AsyncCommand`, `CommandSettings`, branches, validation, and DI wiring. For the full framework reference (flag values, dictionary/lookup options, custom type converters, interceptors, help styling, testing, execution lifecycle), see the [spectre-cli](../spectre-cli/SKILL.md) skill.
- [Vertical Slice](references/vertical-slice.md): Feature-first folder organization — cohesion over technical layering, and how a command folder joins the host here.
- [Installation & Distribution](references/installation-and-distribution.md): Local tool manifest, NuGet.org, and the CI workflow — this repository's path.
- [Conventions](references/conventions.md): Exit codes, `AnsiConsole` markup, error output, and the central-package and zero-warning rules.

## Quick Start

**`.csproj`** — three properties turn a console app into a dotnet tool:

```xml
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <TargetFramework>net10.0</TargetFramework>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
  <PackAsTool>true</PackAsTool>
  <ToolCommandName>mytool</ToolCommandName>
  <PackageId>MyOrg.MyTool</PackageId>
</PropertyGroup>

<ItemGroup>
  <!-- no Version attribute: the version lives in Directory.Packages.props -->
  <PackageReference Include="Spectre.Console.Cli" />
</ItemGroup>
```

**`Program.cs`** — wire DI and commands:

```csharp
var services = new ServiceCollection();
// each command folder registers what it needs - references/vertical-slice.md owns how

var app = new CommandApp(new TypeRegistrar(services));
app.Configure(config =>
{
    config.SetApplicationName("mytool");
    config.AddBranch("issue", issue =>
    {
        issue.SetDescription("Manage issues.");
        issue.AddCommand<IssueListCommand>("list");
        issue.AddCommand<IssueCreateCommand>("create");
    });
});
return app.Run(args);
```

**Command with nested Settings:**

```csharp
public sealed class IssueListCommand : AsyncCommand<IssueListCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("--repo <REPO>")]
        [Description("The repository name.")]
        public required string Repo { get; init; }
    }

    private readonly IIssueCatalog _issues;
    private readonly IAnsiConsole _console;

    public IssueListCommand(IIssueCatalog issues, IAnsiConsole console)
    {
        _issues = issues;
        _console = console;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var issues = await _issues.ListAsync(settings.Repo);
        // render issues through _console
        return 0;
    }
}
```

## Mandatory Requirements

- **USE** `Spectre.Console.Cli` for all CLI parsing — not `System.CommandLine`, `McMaster.Extensions.CommandLineUtils`, or manual `args` parsing.
- **INJECT** `IAnsiConsole` rather than calling static `AnsiConsole.*` — that is what makes output assertable from a test. Several examples in this skill use the static form for brevity; prefer the injected one in real commands.
- **ORGANIZE** by feature/domain (vertical slice) — never create top-level `Commands/`, `Settings/`, or `Services/` folders. In this repository a slice is `src/specht.tool/Features/<Command>/`; [Vertical Slice](references/vertical-slice.md) owns that rule.
- **PACK** the tool. `PackAsTool=true` in `src/specht.tool`; `./build.sh Pack` produces `specht.tool.<version>.nupkg`; consumers install it from NuGet.org through a local tool manifest. The repository-wide `IsPackable=false` default is overridden in that one project and nowhere else.
- **KEEP** commands as entry points — parse, dispatch, fold a result into an exit code. Business logic does not live in a command.
- **NEVER** accept a secret as a `--flag` option — it lands in shell history and process lists. This tool needs none; see [Conventions](references/conventions.md) § "Secrets".
- **ALWAYS** implement `Validate()` on `CommandSettings` when options have cross-field constraints.
- **USE** `AsyncCommand<TSettings>` as the base for every command — synchronous commands are not permitted.
- **RETURN** semantic exit codes — `0` for success, non-zero constants for each error category.
- **NEVER** write an absolute path to stdout, stderr, a report or a log line. Every path the tool emits is relative to the root it was given (README § 9).
