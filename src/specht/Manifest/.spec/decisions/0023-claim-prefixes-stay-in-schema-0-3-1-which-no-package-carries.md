---
title: "Decision 0023: claim prefixes stay in schema 0.3.1, which no package carries"
description: "The owner's rule of November 8 2026: a schema version that has not gone out in a NuGet package is changed in place, and one that has takes 0.4.0 for a breaking change or 0.3.2 for a non-breaking one; 0.3.1 is in no package, so B-076 to B-080 stay in it"
type: decision
---

# Decision 0023: claim prefixes stay in schema 0.3.1, which no package carries

**Date:** 2026-11-08
**Decided by:** the repository owner, relayed through the coordinating session; the application to item `0352` follows from the owner's rule and the verified fact below; recorded by spec-author

## The call

The owner's words, verbatim:

> has 0.3.1 gone out in a package to nuget? That is the rule. If it has. And
> it's a breaking change, 0.4.0; non breaking 0.3.2.

1. **The rule.** Whether a schema version takes a change in place depends on
   whether that version has gone out in a NuGet package. If it has, a
   breaking change goes to `0.4.0` and a non-breaking one to `0.3.2`.
2. **The application to B-076 to B-080.** nuget.org lists `specht.tool`
   `0.4.2-preview`, `0.5.11-preview` and `0.5.12-preview`, checked
   2026-11-08. Tag `v0.5.12-preview` (`3d3ee293`) carries no schema `0.3.1`,
   and commit `c67ef3e9`, which created `0.3.1`, is not its ancestor. `0.3.1`
   has not gone out in a package, so the change stays in `0.3.1` as built,
   relocked under `0001-F7` C-17. Neither branch of the rule applies.

## Why

The owner's reasoning is the publication test the words state: whether the
version has gone out in a NuGet package.

## Rejected

No alternative was put to the owner. A-7 named `0.3.1` on spec-author's reading of its
own fallback, called a `0001-F7` C-11 patch. This decision replaces that
reading as the reason for the version choice.

## Affects

A-7; B-076 to B-080; B-049; `0001-F7` C-1, C-11, C-17; item `0352`.

Left unanswered, spec-author's note: the ruling does not say whether a
`0.3.0` tree whose disk-selected Feature schema permits `claim_prefix`, which
`0.3.1` gives prefix meaning (the `@B-049` boundary scenario), makes the change
breaking. This record does not answer it.

## Reversal

None.
