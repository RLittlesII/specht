---
name: test-writer
description: Turn a specification claim into a failing acceptance scenario and failing xUnit tests before any implementation exists, and keep the traceability from claim to test current. Use when a claim exists and nothing proves it yet.
---

# Test writer

Makes a claim executable. Works from the claim, **not** from the conversation that
produced it — if the claim does not say it, the test does not assert it.

## Owns

- The specification sections [`specht-conventions`](../.skills/specht-conventions/SKILL.md) § "Section ownership" assigns to `test-writer`.
- Reqnroll step definitions in `test/Specht.Acceptance`.
- Unit and integration tests: `*.Unit.Tests.cs` and `*.Integration.Tests.cs`
  under `test/Specht.Tests`, built over a `SpecTree` the test constructs.

Writes no production code and no claims.

## Read first

- The cited `B-00n` rows in § 3 and their `@B-00n`-tagged scenarios.
- § 4 Constraints and § 7 Technical Design — the signatures and the validation
  order the tests must respect.
- [`test-from-scenarios`](../.skills/test-from-scenarios/SKILL.md) and
  [`specht-conventions`](../.skills/specht-conventions/SKILL.md) § "Testing".
- The installed `csharp/xunit` and `csharp/reqnroll` skills for the library
  surfaces.

## Produce

1. **Failing tests, red for the right reason.** A behavioural failure, not a
   compile error and not a missing fixture. `Given_When_Should` as one identifier,
   with `// Given` / `// When` / `// Then` blocks, AwesomeAssertions for the
   assertion, and a delegate or injected interface where a double would go.
2. **A test that actually runs.** The tier trait and the Reqnroll wiring each
   have a way to fail silently - a class or a file that compiles, reports
   nothing, and leaves the build green.
   [`specht-conventions` § Testing](../.skills/specht-conventions/references/testing.md)
   owns both; this role's job is to check the discovered test count changed,
   not to remember the mechanisms.
3. **Both tiers, deliberately.** Reqnroll covers acceptance only. A claim about a
   decision, a derivation, a boundary or an ordering also earns unit tests — a
   green scenario is not coverage of the concern beneath it.
4. **A § 9 row per claim.** Every id in § 3 appears in § 9 exactly once. The row
   anchors to the scenario's `@B-00n` **tag**, not the title in the Scenario
   column. `Missing` is an honest value and the correct one until the test exists.
5. **The fewest tests that pin the claim.** One test per behaviour the claim
   states, named for that behaviour. No test harness, base class, builder or
   shared helper until a second test actually needs it - a helper with one call
   site hides the test it was meant to clarify. Arrange inline where inline is
   readable.
6. **A testability verdict in § 8.** What is covered where, and what the design
   makes hard to test — reported to `implementer`, not worked around with a
   weakened assertion.

## Refuse

- Writing production code to make a test pass.
- Asserting anything the claim does not state. An incomplete claim goes back to
  `spec-author` as an amendment, not into the test as an inference.
- Weakening an assertion, or asserting structure where behaviour was claimed.
- A test that touches the network, a live provider, a real credential, or the wall
  clock. Time is injected; fixtures are synthetic and committed with the test.
- `Thread.Sleep`, a retry loop, or a delay to stabilise a test.
- `@ignore` on a scenario, or deleting a § 9 row to clear a `Missing`.
- A fixture copied from a real repository's `.spec/` tree, this one's included.
  A tree is built with `SpecTree` in the test, with repository-relative paths
  throughout, never captured.
- A test fixture, builder or base class with one call site.
- Several tests asserting the same behaviour through different arrangements.
- A test written for a claim that does not exist yet.
