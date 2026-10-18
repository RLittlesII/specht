---
title: "Decision 0001: Analyzer tests use direct assertions"
description: "Reuse the Rocket Surgery test context builder and inspect diagnostic results directly, excluding Verify"
type: decision
---

# Decision 0001: Analyzer tests use direct assertions

**Date:** 2026-10-17
**Decided by:** Repository owner

## The call

Use `Rocket.Surgery.Extensions.Testing.SourceGenerators` for production analyzer
and generator tests, following the direct-assertion examples in
[Airframe Rsa1001Tests](https://github.com/RocketSurgeonsGuild/Airframe/blob/main/test/Analyzers.Tests/Diagnostics/Usage/Rsa1001Tests.cs).
Use the repository's assertion library. Assert diagnostic counts, identities,
severities and locations; apply fixes, assert the edited source and reanalyze.
Do not add Verify, Verify.Xunit, Verifier.Verify or snapshot files for this
capability. The isolated spike harness is not the production harness.

## Why

The test context builder provides useful Roslyn setup and observable results.
Direct assertions make the expected diagnostic and repaired behavior explicit.
A predicate over an empty diagnostic list must not count as a negative test.

## Rejected

Copying the example's Verify-based test or snapshot dependencies. The accepted
cost is writing explicit assertions for each behavior rather than approving a
serialized result snapshot.

## Affects

0201-F2 § 8; epic 0201 Analyzer testing direction. This chooses a test harness,
not a production package version or a binding syntax.

## Reversal

None.
