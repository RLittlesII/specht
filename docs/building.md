---
title: "Building specht"
description: "How to build, test and pack this repository"
type: reference
---

# Building

Requires the .NET SDK pinned in [`global.json`](../global.json).

```sh
dotnet tool restore
./build.sh             # compile and test
./build.sh Pack        # specht.tool.<version>.nupkg
./build.sh Benchmark --benchmark-job Short # every benchmark, Release
./build.sh Benchmark --benchmark-filter '*CheckBenchmarks*' --benchmark-job Short
./build.sh Specht   # run specht on this repository's own specifications
./build.sh LockSchemaVersion # record the newest schema version's digest once
./build.sh CutSchemaVersion --cut-version <major.minor.patch> # copy the newest schema version to a higher one
```

`LockSchemaVersion` adds the newest shipped schema version to
`src/tool/schema/versions.lock.json`. Commit the lock before publishing that
version. Each entry is one SHA-256 over the version's schemas, manifest,
templates and rule pages. Running the target again leaves an existing entry
and the lock's bytes unchanged, even if a locked file was edited.

`CutSchemaVersion` copies the newest shipped version's schemas, manifest,
templates and rule pages to `--cut-version`, rewriting only each schema's `$id`
and each rule page's schema version. It refuses, creating nothing, while a
shipped version is unlocked or when the version is not `major.minor.patch` and
higher than the newest. It writes no lock entry and nothing under `.specht/`.

`Benchmark` accepts optional `--benchmark-filter` (a BenchmarkDotNet full-name
glob) and `--benchmark-job` parameters. Without them it runs every benchmark
under the library's default job. Results are Markdown and JSON files under
`.artifacts/benchmarks/`. Measurements never fail a build; broken benchmarks do.
The default build runs no benchmark.

The self-check builds and packs this checkout's tool, installs its exact package
through an isolated local manifest under .artifacts/self-check/, and invokes it
against this tree. Each run replaces the installation and caches, including when
working edits retain the same computed package version. A preparation failure
fails the gate; no supporting published checker is required. The committed
published-consumer pin remains unchanged.

The root `dotnet tool run specht` command still selects that published consumer
pin. Use `./build.sh Specht` to validate the working checkout.

## C# cleanup

Roslynator analyzers, formatting analyzers and compiler-diagnostic code fixes are private development
packages, pinned in [`Directory.Packages.props`](../Directory.Packages.props)
and referenced through [`Directory.Build.props`](../Directory.Build.props).
The native SDK settings and analyzer severities live in [`.editorconfig`](../.editorconfig).
The SDK owns whitespace, braces, qualification, type keywords, naming,
initializers and null-handling preferences. Roslynator adds blank-line spacing
and the repository's single-line documentation summaries, alongside its existing
cleanup rules. The Roslynator
selection starts from [Airframe's ruleset](https://github.com/RocketSurgeonsGuild/Airframe/blob/2095ebe044c1a7af2d384f024868922c2f09ba8b/analyzers.ruleset),
with unrelated analyzer families excluded. Native rules replace the overlapping
RCS1001 brace and RCS1037 whitespace checks. RCS1259 replaces obsolete RCS1066;
upstream-disabled RCS1062 and nonexistent RCS1507 are omitted. Existing
accessibility and static-lambda conventions remain authoritative, and cleanup
makes no await-context changes. RCS1139 is disabled because traceability-only
`<implements>` comments are intentional and need no empty summary scaffolding.

The supplied native preferences retain this repository's LF endings,
file-scoped namespaces, modifier order, `var` preferences and single-line
expression-body policy. Constants and static readonly storage keep PascalCase;
test methods retain underscore-separated names. Collection-expression suggestions
require exact type matches. Namespace-folder matching, compiler-warning
downgrades and unrelated Serilog/XAML settings are excluded. Existing primary
constructors, explicit entry points and static lambdas remain valid.

```sh
dotnet restore specht.slnx
dotnet format specht.slnx --no-restore                     # apply cleanup
dotnet format specht.slnx --no-restore --include src/tool  # limit the files
./build.sh Format                                        # verify without writing
```

For a bulk cleanup that changes shared or multi-targeted files, format one project
and its highest target framework at a time (for example,
`dotnet format src/tool/tool.csproj --framework net11.0 --no-restore`). The pinned
SDK can produce conflicting edits when it fixes the same file through multiple
project contexts. Use the solution-wide verify-only gate after applying fixes.

The supplied suggestion/silent preferences remain advisory.
`dotnet format` applies enabled warning/error analyzer diagnostics that provide
batch fixes. Airframe's informational RCS1163 (unused parameters, no code fix)
and RCS1168 (parameter names) remain suggestions, outside the default format
gate. Review an individual suggestion in the IDE; a targeted explicit pass can
apply RCS1168 with `dotnet format analyzers specht.slnx --no-restore --severity info --diagnostics RCS1168`.

The separate `Roslynator.CodeFixes` package supplies IDE actions for compiler
diagnostics. It does not turn every compiler error or interactive refactoring
into an unattended `dotnet format` fix. Diagnostics without a batch fixer still
require a code change; successful formatting does not replace compilation.
The build, CI and staged-file hook continue to verify formatting without
rewriting files (`0055-F1` C-2).
