---
title: "Lesson 0010: A rule is linked from the role that acts on it"
description: "Tests on PR #208 built their data with private helpers and inline member data, rules the owner first gave on PR #4; the testing reference held both, and neither the contract that writes tests nor the one that reviews them carried them to the point of action."
type: lesson
kind: process
---

# Lesson 0010: A rule is linked from the role that acts on it

**Date:** 2026-10-15
**Kind:** process

## Symptom

Pull request #208 (item `0037`, `0001-F3` B-009, B-010, B-011 and B-026)
added `test/tests/Report/SpecReportDocument.Expected.Unit.Tests.cs`. Its tests
built their data with private hand-rolled helpers - `Valid`, `Without`,
`Model`, `Feature`, `Enum` - and fed their theories from inline `[MemberData]`
properties. Two `spec-reviewer` rounds approved them. The repository owner's
review 5542419251 (2026-10-15) asked "AutoFixture? xUnit Class Data?" on the
file and summed up "Still haven't learned this lesson?".

The same two rules were first given on pull request #4 (2026-10-08): test data
from Rocket Surgery AutoFixtures, and cases of one shape as one theory over
xunit class or member data.

## Root cause

The rule existed. `specht-conventions` § Testing named the AutoFixtures as the
source of test data and listed "a hand-rolled factory method for test data an
AutoFixture can build" under `Never add`. Neither contract that acts on a test
carried it to the point of action, so the recurrence passed two reviews.

- `test-writer` step 5 says no "shared helper until a second test actually
  needs it" and "arrange inline where inline is readable". It never named the
  AutoFixtures or class data, and a private helper with several call sites
  reads as the exception it permits.
- `spec-reviewer` looked for a missing `Tier` trait and a weakened test, but
  nothing sent it to the testing reference's `Never add` list; "the skills
  covering the surfaces the diff touches" was left to memory.

The testing reference also left the theory's data source open - "over
`[MemberData]` or `[ClassData]`" - so the inline member data broke no written
rule.

## Spec delta

None. No Feature specification changes and `specht`'s behaviour is untouched.
The tests in pull request #208 are reworked by `test-writer` on the same
branch.

## Claim

None - a process lesson proves nothing about the tool. Review against the
`Never add` list, now a step of the `spec-reviewer` contract, is what enforces
it.

## Skill

[`specht-conventions` § Testing](../../.agents/skills/specht-conventions/references/testing.md)
keeps the rule. Its one changed sentence makes a `[ClassData]` type the default
source of theory rows, not an inline `[MemberData]` property - the owner's
direction in review 5542419251.

The two contracts link the rule rather than restate it:

- [`test-writer`](../../.agents/roles/test-writer.md) step 5 sends test data
  and theory rows to `specht-conventions` § Testing and its `Never add` list,
  and says a private data helper is not the exception the step allows.
- [`spec-reviewer`](../../.agents/roles/spec-reviewer.md) § "Look for" gains a
  test diff read entry by entry against that `Never add` list, routed to
  `test-writer`.
