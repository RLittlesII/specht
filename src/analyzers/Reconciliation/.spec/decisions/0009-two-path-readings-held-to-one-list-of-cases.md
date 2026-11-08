---
title: "Decision 0009: Two path readings, held to one list of cases"
description: "The owner selected option A, two copies, tested, with a shared list of cases run against both; in the recorder's reading the RepositoryPaths source link is reverted, specht evidence gets its own normaliser and matcher, and one copy of the cases sits in each test project"
type: decision
---

# Decision 0009: Two path readings, held to one list of cases

**Date:** 2026-11-08
**Decided by:** Repository owner; recorded by spec-author

## The call

Commit `e2acacf0`, item 0409's fix for review round 1 finding 3, moved the
analyzer's `AnalysisPaths` and `BindingProvenanceGuard` into
`src/specht/Model/RepositoryPaths.cs` and linked that file into
`src/analyzers/analyzers.csproj` under `Text/`. Item 0409's review found the
link outside
[0201-F7 decision 0004](../../../Bindings/Gherkin/.spec/decisions/0004-the-gherkin-claim-reader-lives-in-the-engine.md),
which covers the Gherkin claim reader beside the four linked text readers "and
no other source link".

The owner's words, relayed by the coordinating session, verbatim:

<!-- prettier-ignore -->
- "Rules are rules for a reason.  Why did we violate it, and how can we correct it without breaking the rule?"

The owner then selected the option labelled "A: two copies, tested". The
option, as it was put to the owner: revert the move so the analyzer keeps its
own path code; `specht evidence` gets its own normaliser and matcher; a shared
list of cases, run against both, holds them to the same reading. That
description is the option's text, not the owner's words.

The rest of this record is the recorder's reading of that selection.

1. The `RepositoryPaths` source link is reverted.
2. The analyzer keeps `AnalysisPaths` and `BindingProvenanceGuard`.
3. `specht evidence` has its own normaliser and matcher. It reads a `./`, an
   empty segment, a backslash, `..` and a trailing `**` in a claim file set
   exactly as the analyzer does (B-038).
4. The same list of cases pins that parity in `test/tests` and in
   `test/analysis.tests`, one copy in each project. Fixtures are never shared or
   linked across test projects (`specht-conventions` `references/testing.md` §
   "Never add": a `<Compile Include>` of another project's source).
5. An acceptance scenario runs one claim file set through both the analyzer's
   native claim catalogue and `specht evidence` (B-038).
6. The exclusions difference stays, and stays pinned: the gate skips the
   manifest's `exclusions`, such as `bin/` and `obj/`, and the analyzer does
   not. B-035's excluded-directory case pins it (§ 8).
7. This refines 0201-F7 decision 0004 and does not widen it. The analyzer's
   source links stay the Gherkin claim reader and the four text readers.

## Why

On [PR #114](https://github.com/RLittlesII/specht/pull/114), 2026-10-11, the
owner ruled against MSBuild source links across projects; 0201-F7 decision
0004 quotes it and admits only its own links as the exception. Two copies of
one reading drift unless a test fails when they part, and one list of cases run
in each test project is that test.

## Rejected

This section is the recorder's reading. Of the owner's words it holds only
what "The call" quotes.

- Keeping the `RepositoryPaths` source link (commit `e2acacf0`): outside 0201-F7
  decision 0004's scope.
- Option B, put to the owner as the recommended option: extend 0201-F7
  decision 0004's link permission to `RepositoryPaths`. One implementation and
  no new assembly, at the cost of widening the PR #114 pattern by one file. The
  owner did not select it.
- Option C: narrow the glob contract so both sides reject non-canonical paths.
  Spec-reviewer raised it and advised against it; it was mentioned to the
  coordinating session, considered, and not put forward to the owner as an
  option.

## Affects

0201-F5 B-028, B-035, B-038, § 5 rows 10 and 11, § 7 (the claim file set path
paragraph), § 8, § 9; 0201-F7 decision 0004 (refined, unchanged);
`src/analyzers/analyzers.csproj`; `docs/native-gherkin-claims.md`; item 0409;
repo-wide lesson 0017.

## Reversal

None.
