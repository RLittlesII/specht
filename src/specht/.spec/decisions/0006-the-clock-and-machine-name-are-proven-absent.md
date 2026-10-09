---
title: "Decision 0006: the clock and the machine name are proven absent, not varied"
description: "B-006's clock and machine name are proven by a test that the engine assembly references neither, not by an injected seam; the root location, an environment variable and the locale are varied against the golden report; amended the same day: the schema rule evaluates under the invariant culture, the guard's one exemption"
type: decision
---

# Decision 0006: the clock and the machine name are proven absent, not varied

**Date:** 2026-10-09
**Decided by:** the owner, during item 0024; recorded by spec-author

Amended 2026-10-09, by the owner during item 0024, after the test of call 2
failed: under `tr-TR`, JsonSchema.Net 9.4.0 lowercased a `SPEC002` message's
expected type to `"ınteger"` ([lesson 0001](../lessons/0001-the-locale-reached-a-schema-message.md)).
Call 3 no longer holds as written. The engine changes once:
`FrontmatterSchemaRule` evaluates each schema through a private
`EvaluateInvariant`, which sets `CultureInfo.CurrentCulture` to the invariant
culture for the `Evaluate` call alone and restores it in a `finally`. The
reference guard of call 1 carries exactly one exemption,
`CultureInfo.CurrentCulture` in `specht.Rules.FrontmatterSchemaRule.EvaluateInvariant`,
pinned to that site by a test. C-9 is amended to bind under the invariant
culture. Rejected with it: rebuilding the type message in the engine, which
departs from how the copied engine produces messages (C-2); deferring to a bug
item; narrowing B-006; setting `EvaluationOptions.Culture` alone, which does
not reach the lowercasing; upgrading, as no newer release exists.

## The call

1. **The clock and the machine name are proven by reference absence.** A
   unit test reads the engine assembly's metadata with
   `System.Reflection.Metadata`, which is in-box, and fails on any reference
   to:
   - `Environment.MachineName`, `Environment.GetEnvironmentVariable`,
     `Environment.GetEnvironmentVariables`, `Environment.CurrentDirectory`;
   - `DateTime.Now`, `DateTime.UtcNow`, `DateTime.Today`;
   - `DateTimeOffset.Now`, `DateTimeOffset.UtcNow`;
   - `TimeProvider`, `Stopwatch`, `TimeZoneInfo.Local`;
   - `CultureInfo.CurrentCulture`, `CultureInfo.CurrentUICulture`.
2. **What a test can vary, it varies.** An integration test runs the engine on
   the baseline tree at a different, nested root, under the `tr-TR` culture and
   with an extra environment variable set, and compares both reports with the
   golden report of decision 0004.
3. **No engine file changes** (C-9), superseded by the amendment above. No
   package is added (C-7).

## Why

- The engine (`src/specht`) references no `DateTime`, `DateTimeOffset`,
  `TimeProvider`, `Stopwatch` or `Environment` member, and its comparisons are
  ordinal. Its one `CultureInfo` reference is the exempt site the amendment
  adds, which sets the invariant culture rather than reading the caller's.
- A test cannot vary the machine name, and cannot vary a clock the engine
  never reads. Only a reference the engine holds can carry either into the
  verdict, so proving the reference absent is the falsifiable form.

## Rejected

- **An injected `TimeProvider` or machine-name abstraction:** it has no
  production reader, so varying it passes whatever the engine does, and a
  direct `DateTime.Now` or `Environment.MachineName` call bypasses it. .NET has
  no in-box machine-name abstraction, so it would be a new engine type, which
  C-9 rules out.
- **Running the packed tool as a child process:** still cannot vary the
  machine name, is slower, and needs the packed tool.

## Affects

`B-006` (scenario amended; claim unchanged); `C-9` (amended); § 7; § 8; item 0024; lesson 0001.

## Reversal

When the engine first needs the time or the machine name, the indirection is
earned: that change injects it, and B-006's proof varies it.
