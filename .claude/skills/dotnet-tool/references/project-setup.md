---
title: Project Setup
description: Configuring a .NET console app as a packaged dotnet tool, and what src/specht.tool sets versus what the repository root already sets.
type: reference
---

# Project Setup

## In this repository

`src/specht.tool/specht.tool.csproj` is the tool project. It sets **only** what
is specific to being a packed tool; everything else comes from the root:

- `PackAsTool`, `ToolCommandName=specht`, `PackageId=specht.tool`, and
  `IsPackable=true` — overriding the repository-wide `IsPackable=false` default
  in `Directory.Build.props` in this one project.
- `EmbeddedResource` for `schema/v1/**` and `templates/v1/**` — the versioned
  shipping copies that `specht init` writes into a consumer's `.spec/` and that
  the checker validates against (README § 3, § 6). The tool embeds every schema
  version it knows.
- A `ProjectReference` to `src/specht`, the engine library.
- The framework, nullability and strictness properties the template below
  repeats are already set once in `Directory.Build.props`. A `.csproj` here sets
  only what is specific to that project.
- **No `Version` attribute on any `PackageReference`.** Versions live in
  `Directory.Packages.props`. The pinning advice further down this file is about
  a repository that does not use central package management; this one does.

Adding `Spectre.Console.Cli` therefore means one `PackageVersion` entry in
`Directory.Packages.props` and one versionless `PackageReference` in
`specht.tool.csproj`.

## Required `.csproj` Properties

Three properties are mandatory to turn a console app into a dotnet tool:

```xml
<PackAsTool>true</PackAsTool>
<ToolCommandName>specht</ToolCommandName>
<PackageId>specht.tool</PackageId>
```

| Property          | Purpose                                                       |
| ----------------- | ------------------------------------------------------------- |
| `PackAsTool`      | Tells the SDK to produce a tool NuGet package                 |
| `ToolCommandName` | The command users type at the terminal — omit file extensions |
| `PackageId`       | The NuGet package ID users pass to `dotnet tool install`      |

**Naming convention**: `PackageId` uses `<org>.<toolname>` or `<product>.Tool`.
**Avoid** file extensions in `ToolCommandName` — the tool is installed as an app host.

## Full `.csproj` Template

The generic shape. In this repository the first `PropertyGroup`'s framework,
nullable and implicit-usings lines are already in `Directory.Build.props` and
are not repeated.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>MyOrg.MyTool</RootNamespace>

    <PackAsTool>true</PackAsTool>
    <ToolCommandName>mytool</ToolCommandName>
    <PackageId>MyOrg.MyTool</PackageId>
    <PackageOutputPath>./nupkg</PackageOutputPath>

    <Description>One-line description shown in dotnet tool list.</Description>
    <Authors>Your Name</Authors>
    <PackageTags>dotnet-tool;mytool</PackageTags>
  </PropertyGroup>

  <ItemGroup>
    <!-- versions live in Directory.Packages.props -->
    <PackageReference Include="Spectre.Console.Cli" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" />
  </ItemGroup>
</Project>
```

## Shared `Directory.Build.props`

Place at the repository root or `src/` directory to apply globally:

```xml
<PropertyGroup>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <LangVersion>latest</LangVersion>
</PropertyGroup>
```

## `Directory.Build.targets` — Auto-Inject Analyzers

When using Roslyn analyzers (e.g., the `cli.generator` source generator), auto-inject them
for all `PackAsTool` projects from a shared `Directory.Build.targets`:

```xml
<ItemGroup Condition="'$(PackAsTool)' == 'true'">
  <CompilerVisibleProperty Include="RootNamespace" />
  <CompilerVisibleProperty Include="ToolCommandName" />
  <ProjectReference Include="$(MSBuildThisFileDirectory)cli.generator\cli.generator.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

Nothing Roslyn is in scope for this tool (README § 2); the pattern is kept as
the generic recipe only.

## Versioning

**Option 1 — Static version** (simple tools):

```xml
<Version>1.0.0</Version>
```

**Option 2 — `Nerdbank.GitVersioning`** (preferred for multi-tool repos):

```xml
<!-- version in Directory.Packages.props -->
<PackageReference Include="Nerdbank.GitVersioning" PrivateAssets="all" />
```

Driven by `version.json` at the repo root. Git tags and commit height generate the version automatically.

**Option 3 — Override from CI tag** (any version source):

```bash
dotnet pack -c Release -p:Version=${GITHUB_REF_NAME#v}
```

Which of these this repository uses is build state, not a rule: read
`.build/Build.cs`.

## `PackageOutputPath` Convention

Set it once, under the build's artifact root:

```xml
<PackageOutputPath>$(MSBuildThisFileDirectory)../../.artifacts/nupkg</PackageOutputPath>
```

Prevents the default `bin/Release/` output from mixing tool packages with build
artifacts, and keeps everything the build produces under the gitignored
`.artifacts/`.
