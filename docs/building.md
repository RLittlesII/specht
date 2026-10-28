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
./build.sh CutSchemaVersion --cut-version 0.3.0 # copy the newest schema version to 0.3.0
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
