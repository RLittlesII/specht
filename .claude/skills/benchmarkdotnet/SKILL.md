---
name: benchmarkdotnet
description:
  Measuring .NET code with BenchmarkDotNet - benchmark classes, parameters, setup, jobs and
  toolchains, the memory diagnoser, exporters, the command line, how to read the summary, and the
  measurement traps that make a number meaningless. Use when writing, running or reading a
  benchmark, or judging whether a performance finding can be trusted.
---

# BenchmarkDotNet

BenchmarkDotNet turns a method into a measurement: it builds the benchmark in
Release, runs it in a separate process, warms it up, runs it until the result is
statistically stable, and prints a summary table. The library owns the timing;
the benchmark owns only **what** is measured.

Sourced from [benchmarkdotnet.org](https://benchmarkdotnet.org/) - the
[overview](https://benchmarkdotnet.org/articles/overview.html),
[good practices](https://benchmarkdotnet.org/articles/guides/good-practices.html),
[setup and cleanup](https://benchmarkdotnet.org/articles/features/setup-and-cleanup.html),
[parameterization](https://benchmarkdotnet.org/articles/features/parameterization.html),
[toolchains](https://benchmarkdotnet.org/articles/configs/toolchains.html),
[diagnosers](https://benchmarkdotnet.org/articles/configs/diagnosers.html) and
[console arguments](https://benchmarkdotnet.org/articles/guides/console-args.html).
Read the page before relying on a surface this file only names.

Where `specht` has decided something, the line is marked **specht:**. Where
benchmarks live and run in this repository is
[`specht-conventions` § Benchmarking](../specht-conventions/references/benchmarking.md);
it is not restated here.

## Running

- `BenchmarkRunner.Run<T>()` runs one class.
- `BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args)` runs
  what the command line selects. It is the entry point for a project with
  more than one class, because it gives `--filter`.
- Release only. BenchmarkDotNet refuses a non-optimised build; a Debug result
  is 10-100x off and is never a finding. No debugger attached.

## Writing a benchmark

| Attribute                                 | Does                                                                             |
| ----------------------------------------- | -------------------------------------------------------------------------------- |
| `[Benchmark]`                             | measures the method                                                              |
| `[Benchmark(Baseline = true)]`            | the reference the `Ratio` column divides by - one per class (or per category)    |
| `[Params(...)]`                           | a run per value on a public field or property; values are compile-time constants |
| `[ParamsSource(nameof(X))]`               | values from a public method or property returning `IEnumerable`                  |
| `[ParamsAllValues]`                       | every value of a `bool` or a non-`[Flags]` enum                                  |
| `[Arguments(...)]` / `[ArgumentsSource]`  | method arguments; building them is not measured                                  |
| `[GlobalSetup]` / `[GlobalCleanup]`       | once per benchmark method, outside the measurement                               |
| `[IterationSetup]` / `[IterationCleanup]` | before and after every iteration - macrobenchmarks only, see the traps           |
| `[MemoryDiagnoser]`                       | adds `Allocated` and GC columns; not on by default                               |
| `[ShortRunJob]`, `[SimpleJob(...)]`       | fewer iterations, or a chosen runtime                                            |
| `[InProcess]`                             | runs in the host process - see Toolchains                                        |

Setup attributes take `Target`/`Targets` to apply to named methods only. The
order is global setup, then (iteration setup, benchmark, iteration cleanup)
repeated, then global cleanup.

A scaling question is `[Params]` over the input size. A comparison is a
`Baseline` and its alternative in one class, so `Ratio` reads directly.

**specht:** `[MemoryDiagnoser]` is on for every benchmark - allocation is in
the benchmark scope the owner set (item 0108).

## Toolchains

- **Default:** a generated console project per benchmark, built and run in its
  own process. Isolation: no JIT state or GC heap carries between benchmarks.
  It is what a published finding uses.
- **`InProcessEmitToolchain`** (`[InProcess]`) and **`InProcessNoEmitToolchain`:**
  no generated project, so it is fast, but nothing is isolated and **it cannot
  measure startup**. The docs recommend it for quick iteration and the default
  toolchain for results that are published.

## The command line

With `BenchmarkSwitcher`:

| Argument                         | Does                                                   |
| -------------------------------- | ------------------------------------------------------ |
| `--filter` / `-f '<glob>'`       | select by full name, e.g. `-f '*Discovery*'`           |
| `--list flat` / `--list tree`    | list without running                                   |
| `--job Dry\|Short\|Medium\|Long` | `Dry` checks that it runs; `Short` for a quick look    |
| `--memory` / `-m`                | memory diagnoser from the command line                 |
| `--exporters json markdown`      | the result files written                               |
| `--artifacts <dir>`              | where results go; default `BenchmarkDotNet.Artifacts/` |
| `--inProcess` / `-i`             | in-process toolchain                                   |
| `--join`                         | one summary for everything the filters selected        |

`--job Dry` is the cheap proof a benchmark compiles, runs and returns - the
form a build uses to keep benchmarks from rotting without paying for a
measurement.

JSON suits a later machine reader; Markdown suits a person.

## Reading the summary

| Column      | Means                                                           |
| ----------- | --------------------------------------------------------------- |
| `Mean`      | average time per operation                                      |
| `Error`     | half the 99.9% confidence interval                              |
| `StdDev`    | spread of the measurements                                      |
| `Ratio`     | `Mean` over the baseline's `Mean`                               |
| `Allocated` | managed bytes allocated per operation                           |
| `Gen0`      | gen-0 collections per 1000 operations (`Gen1`, `Gen2` likewise) |

The memory diagnoser runs a separate pass, so it does not disturb `Mean`, and is
about 99.5% accurate on allocated bytes under the default or short job.

A finding states the environment the summary's header prints - runtime, OS,
CPU. Results do not transfer between environments; a number from a laptop is not
a number for CI.

`Error` larger than the difference between two means is no difference.

## Traps

- **Dead-code elimination.** A result nobody uses can be removed by the JIT, and
  the benchmark measures nothing. Return the result.
- **Lazy sequences.** Returning an unenumerated `IEnumerable<T>` measures
  building the iterator, not running it. Materialise it inside the benchmark
  (`ToList()`, `Count()`) when the work is the enumeration.
- **Setup inside the measurement.** Building the fixture, resolving the
  container or reading the disk inside `[Benchmark]` measures that, not the call.
  Do it in `[GlobalSetup]`; `[Arguments]` and `[Params]` construction is already
  outside.
- **`[IterationSetup]` on a microbenchmark.** It forces `InvocationCount=1` and
  `UnrollFactor=1`; the docs warn it spoils results under about 100 ms. Use it
  only when each operation is long and must start from fresh state.
- **Resolution cost against steady-state cost.** Resolving a graph from a
  `ServiceProvider` and calling what it resolved are two costs. Benchmark them
  apart, or the first hides inside the second.
- **Async.** A method returning `Task` or `ValueTask` is awaited by the harness.
  Blocking on `.Result` inside the benchmark measures the block too.
- **The disk.** A benchmark that reads files measures the file system and its
  cache as much as the code. Say so in the finding, or read into memory in
  setup and measure the in-memory path.
- **Noise.** Other processes, power saving, a laptop on battery, an antivirus
  scan and a shared CI runner all move the numbers. A shared runner's timings
  are a record, not a comparison.

## Never add

- An `Assert`, or any threshold, in a benchmark - a benchmark measures, a test
  asserts. **specht:** nothing fails a build on a timing.
- A `Stopwatch` loop beside or instead of BenchmarkDotNet.
- A result from a Debug build or with a debugger attached.
- A benchmark that depends on the network or the wall clock.
- `[InProcess]` on a benchmark that measures startup.
- A package version in the `.csproj`. **specht:** versions are central, in
  `Directory.Packages.props`.
