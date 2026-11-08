---
title: "Lesson 0017: A source link stays within the record that permits it"
description: "A source link was added that no record permits, because the change copied a precedent without reading the scope its record states"
type: lesson
kind: process
---

# Lesson 0017: A source link stays within the record that permits it

**Date:** 2026-11-08
**Kind:** process

Recorded at the coordinating session's request. The rule below is that
session's, not the owner's; the owner's words are quoted only in 0201-F5
decision 0009.

## Symptom

Commit `e2acacf0`, item 0409's fix for review round 1 finding 3, added
`<Compile Include="../specht/Model/RepositoryPaths.cs" Link="Text/RepositoryPaths.cs" />`
to `src/analyzers/analyzers.csproj`. No record permits that source link, and
item 0409's review ruled it not permitted.

## Root cause

The change copied the precedent of the 13 files the same project already links
under `Text/` without reading the scope the record behind them states.
[0201-F7 decision 0004](../../src/analyzers/Bindings/Gherkin/.spec/decisions/0004-the-gherkin-claim-reader-lives-in-the-engine.md)
admits the Gherkin claim reader beside the four text readers "and no other
source link", as the one exception to the owner's PR #114 rule against MSBuild
source links across projects. A precedent carries its record's scope, not a
general permission.

## Spec delta

[0201-F5 decision 0009](../../src/analyzers/Reconciliation/.spec/decisions/0009-two-path-readings-held-to-one-list-of-cases.md)
records the owner's ruling: the link is reverted, the analyzer keeps
`AnalysisPaths` and `BindingProvenanceGuard`, `specht evidence` gets its own
normaliser and matcher, and the same list of cases in `test/tests` and in
`test/analysis.tests` holds the two to one reading. 0201-F5 B-038 states the
parity; § 5 rows 10 and 11 exclude shared path code and the exclusions
difference.

## Claim

0201-F5 B-038 — § 9

## Skill

`specht-conventions` `references/coding.md` § "Never add" lists a source link
or project reference no record permits, or wider than the scope its record
states: before adding one, read that record and stay within its scope.
