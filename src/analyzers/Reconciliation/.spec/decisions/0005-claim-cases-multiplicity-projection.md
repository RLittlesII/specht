---
title: "Decision 0005: claims[].cases is a multiplicity projection"
description: "Why the report keeps bare case-ID strings, repeated per project, with identity on the cases rows"
type: decision
---

# Decision 0005: claims[].cases is a multiplicity projection

**Date:** 2026-10-24
**Decided by:** Repository owner (item 0265 finding M4); recorded by spec-author

## The call

1. `claims[].cases` holds one bare case-ID string per `{project, targetFramework,
caseId}` key, ordered by project, target framework and case ID. A case ID
   shared by two projects appears twice, the same string repeated.
2. The entry names no project. Identity of each entry is on the matching `cases`
   rows, which carry `project` and `targetFramework`.
3. `formatVersion` stays 1. No object-valued entry is introduced.

## Why

Existing readers take each entry as the bare case ID; changing its type is a
report-version change with no consumer asking for it. The repeated string is the
count of keys, which is all the projection promises.

## What the test must prove

For two projects whose artifacts hold the same case ID bound to one claim, the
claim's `cases` lists that string twice, ordered by project then target
framework, and the `cases` array holds two rows with that `caseId`, one per
project; `formatVersion` is 1.

## Reversal

None.
