---
title: "ADR-0001: Required-case scope expression"
description: "A project's required-case scope is a Cucumber-style tag expression, evaluated by one parser over a case's categories"
type: adr
---

# ADR-0001: Required-case scope expression

## Status

Proposed — 2026-10-23, `implementer`, from item 0258. It gives the grammar to
what [0201-F3 decision 0002](../../../analyzers/TestEvidence/.spec/decisions/0002-required-case-scope-per-project.md)
decides. Section 7 states the key and grammar in full. It no longer depends on
the shared library the owner withdrew on 2026-10-23; the expression lives in the
analyzer instead.

## Context

0201-F6 B-026 and 0201-F3 B-025 name a tag expression over case categories with
the operators `not`, `and`, `or` and parentheses. The analyzer policy reader
validates it and the collector applies it to each case; a differing reading
would give two answers to "which keys does this project own". `specht evidence`
needs the answer, not the expression.

## Decision

The key is `requiredCaseScope` on each `codeAnalysis.projects` entry, a string.
The grammar is the Cucumber tag-expression grammar restricted to what the
decision names: lowercase `not`, `and` and `or`, parentheses, and `@tag` operands,
with `not` binding tighter than `and` and `and` tighter than `or`. A tag is
compared ordinally and case-sensitively with a category written without its `@`.
An absent key owns every case. An empty, malformed or unbalanced expression is an
input failure at the manifest. `CaseScope`, in the analyzer's
`src/analyzers/Policy/`, parses and evaluates it; the collector, which
references the analyzer, records each case's result in the test-evidence
artifact, and `specht evidence` reads that result and never parses the
expression. A changed expression changes the manifest, so the artifact's
`policyFingerprint` makes the recorded result stale.

## Rejected

- A VSTest filter string (`Category!=visual`): the run selection is deliberately
  not the scope (decision 0002).
- A case-insensitive match: VSTest filters fold case, Cucumber tag expressions
  do not, and an ownership test must not depend on the runner.
- Escapes, a tag-name wildcard or a `xor`: no claim needs them.
- Evaluating the expression again in `specht evidence`: a second parser, and the
  tool cannot reference the analyzer (0201-F5 § 7).
- A scope per target framework: a project's cases are the same in each; the
  key sits on the project entry that already lists the frameworks.

## Reversal

None.
