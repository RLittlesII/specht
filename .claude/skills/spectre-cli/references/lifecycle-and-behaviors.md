---
title: Lifecycle & Built-ins
description: The app.Run() execution pipeline stage by stage, built-in cli commands, and the design philosophy behind the conventions.
type: reference
---

# Lifecycle & Built-ins

## Design Philosophy: Convention Over Configuration

Spectre.Console.Cli is deliberately opinionated about CLI structure, aligned to conventions users
already know from other CLI tools:

- Option naming follows Unix conventions (`-v` short, `--version` long).
- `-h`/`--help` generation is automatic — never hand-built.
- Standard parsing rules handle flag grouping and value-assignment formats without configuration.

Input (a `CommandSettings` class) and execution logic (a `Command<T>`/`AsyncCommand<T>` class) are
always separate, which is what makes commands trivial to unit test — construct a `Settings`
instance directly and call the command, no argv parsing required in the test.

## Execution Pipeline

When `app.Run(args)` (or `RunAsync`) is invoked, stages run in this order:

1. **Parsing and command resolution** — tokenizes `args`, matches against the configured command
   tree. `-h`/`-?`/`--help` short-circuits here: processing stops, help renders, exit code `0`.
2. **Settings binding** — a `CommandSettings` instance is created and populated from parsed values,
   resolved through DI if configured. Type conversion happens at this stage.
3. **Settings validation** — `settings.Validate()` runs. This is the first validation checkpoint.
4. **Interceptors (before)** — every registered `ICommandInterceptor.Intercept` runs.
5. **Command validation** — `command.Validate(context, settings)` runs, separately from settings
   validation, allowing context-aware checks that settings alone can't express.
6. **Command execution** — `Execute`/`ExecuteAsync` runs, returning the exit code.
7. **Interceptors (after)** — every registered `ICommandInterceptor.InterceptResult` runs, with the
   exit code passed by `ref` so interceptors can rewrite it.
8. **Error handling and return** — unhandled exceptions return `-1` by default (or run through
   `SetExceptionHandler`, or propagate — see [Async & Errors](async-and-errors.md)).

Knowing this order matters when debugging _why_ a check didn't fire: a `Validate()` failure on
settings never reaches command execution, interceptors, or `command.Validate()` at all.

## Built-in `cli` Commands

The framework registers a hidden `cli` branch automatically:

- **`myapp cli version`** / **`myapp --version`** / **`myapp -v`** — prints library/app version.
  Requires `SetApplicationVersion(...)` to enable the `-v`/`--version` global shortcuts.
- **`myapp cli explain`** — diagnostic tree view of the entire CLI configuration (commands, options,
  arguments), with `--detailed` and `--hidden` flags for deeper output. Useful for debugging why a
  command or option isn't showing up as expected.
- **`myapp cli xmldoc`** — machine-readable XML documentation (parameters, validators, type
  converters, hierarchy) for automated tooling.
- **`myapp cli opencli`** / **`myapp --help-dump-opencli`** — generates an OpenCli
  0.1-draft-compatible specification document for CLI tool interoperability.

`-h`/`--help` itself works on every command automatically, rendering descriptions, usage syntax,
arguments, options with defaults, subcommands, and any configured examples.
