---
title: "Lesson 0003: Policy rejections reached the consumer as the generic message"
description: "An enforcement entry without exclude, and a verifies scope with a non-testMethod kind, reported only the generic SPCT0001 text, naming no field"
type: lesson
kind: product
---

# Lesson 0003: Policy rejections reached the consumer as the generic message

**Date:** 2026-11-11
**Kind:** product

## Symptom

A consumer walkthrough on the published `0.6.1-preview` packages of
`Specht.CodeAnalysis` saw one SPCT0001 on `.specht/manifest.json`, "Required
consumer input is missing, invalid or incompatible with its explicit
mappings.", in two cases: a `codeAnalysis.enforcement` entry that omitted
`exclude`, and a `verifies` entry whose `declarationKinds` named a kind other
than `testMethod`. Neither message said which field was wrong.

## Root cause

`AnalysisPolicyReader` read `exclude` as a required array and threw on its
absence, and it rejected a `verifies` entry's kinds in one combined check with
the unsupported role and the unsupported kind. Both threw an
`ArgumentException` with its own text, and the catch-all in
`AnalysisInputReader` replaced every such text with the generic message.
Only the required-case scope, thrown as a `FormatException` and caught
separately, reached the consumer with its own message (B-026).

The specification left both open. § 7's `enforcement` row listed `exclude`
without saying it was required, its example always carried `"exclude": []`,
and no claim said what an input failure's message tells the consumer: B-003
and B-017 promise a reported failure and unavailable analysis, which the
generic message satisfies.

## Spec delta

B-028 reads an `enforcement` entry with no `exclude` as excluding no source.
B-029 makes a policy rejection report a message naming the rejected field and
its fault in place of the generic text, and C-4 keeps paths and consumer values
out of it. § 5 row 5 keeps the generic text for every other input failure.
[Decision 0008](../decisions/0008-policy-rejections-name-their-field.md)
records the owner's two answers and the message texts.

## Claim

- [0201-F6 B-028 and B-029](../README.md#9-traceability-matrix) — § 9.
