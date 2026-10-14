---
title: "Building specht"
description: "How to build, test and pack this repository"
type: reference
---

# Building

Requires the .NET SDK pinned in [`global.json`](../global.json).

```sh
./build.sh             # compile and test
./build.sh Pack        # specht.tool.<version>.nupkg
./build.sh Benchmark --benchmark-job Short # every benchmark, Release
./build.sh Benchmark --benchmark-filter '*CheckBenchmarks*' --benchmark-job Short
./build.sh Specht   # run specht on this repository's own specifications
./build.sh LockSchemaVersion # record the newest schema version's digest once
```

`LockSchemaVersion` adds the newest shipped schema version to
`src/tool/schema/versions.lock.json`. Commit the lock before publishing that
version. Each entry is one SHA-256 over the version's schemas, manifest,
templates and rule pages. Running the target again leaves an existing entry
and the lock's bytes unchanged, even if a locked file was edited.

`Benchmark` accepts optional `--benchmark-filter` (a BenchmarkDotNet full-name
glob) and `--benchmark-job` parameters. Without them it runs every benchmark
under the library's default job. Results are Markdown and JSON files under
`.artifacts/benchmarks/`. Measurements never fail a build; broken benchmarks do.
The default build runs no benchmark.
