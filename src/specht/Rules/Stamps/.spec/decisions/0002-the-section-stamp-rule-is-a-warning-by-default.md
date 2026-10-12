---
title: "Decision 0002: the section stamp rule is a warning by default"
description: "The rule of 0101-F9 has warning severity unless the manifest says otherwise, fails a run only under --strict, and can be raised to an error or disabled through the manifest's rule settings; error by default was not chosen"
type: decision
---

# Decision 0002: the section stamp rule is a warning by default

**Date:** 2026-10-11
**Decided by:** the repository owner, in the wording of the request (2026-10-11); recorded by spec-author

## The call

The rule has warning severity unless the manifest says otherwise. A
repository raises it to an error or disables it through `0001-F5`'s rule
settings; the rule adds no switch of its own.

## Why

Stated by the owner, in the request of 2026-10-11:

- "can we add a SPEC warning for
  `<!-- last written by: spec-author, 2026-10-11 (row 4) -->` if the
  `last written by` becomes an array, flag it!"

The level was not put to the owner as a question; "warning" is the request's
own word, and no option was selected for it.

The author's inference, not stated by the owner:

- This is what `0101-F1` decision 0001 decided for the epic's style and
  ordering rules, which its Reversal note narrowed to `0101-F1` to
  `0101-F4`. That record does not bind this Feature, so this one is made
  for it.
- A stamp that lists its history leaves the specification complete and every
  citation sound. It costs the reader, as a row out of order does, and
  breaks no contract, as an id declared twice does.
- A warning is printed and counted and fails the run only under `--strict`
  (`0001-F2` B-003, B-004; `0001-F5` B-010).

## Rejected

**Error by default.** Not put to the owner as an option; the owner's request
names a warning. The cost is the author's assessment: a repository that
wants the stamp's form enforced sets the rule to error or runs `--strict`,
and this repository's 35 listed stamps do not fail its own check on the day
it pins the version (`0101-F9` A-5).

## Affects

- `0101-F9` B-013, B-014, B-015; C-8.
- `0001-F5` B-010, B-011, B-044: relied on, unchanged.
- `0101-F1` decision 0001: unchanged; its scope stays `0101-F1` to
  `0101-F4`.

## Reversal

None.
