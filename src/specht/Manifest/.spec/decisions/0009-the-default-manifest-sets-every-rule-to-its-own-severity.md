---
title: "Decision 0009: the default manifest sets every rule to its own severity"
description: "Supersedes the addendum to decision 0008 where it took the rules key out of the default manifest: the default manifest carries a rules map setting every rule id of its version to the rule's own severity, and it is what specht init writes; a manifest that names no level for a rule still keeps the rule's own severity"
type: decision
---

# Decision 0009: the default manifest sets every rule to its own severity

**Date:** 2026-10-11
**Decided by:** the repository owner, answering the first of three questions on the need for `0001-F4` decision 0002; recorded by `spec-author`

## The call

The default manifest carries a `rules` key that names every rule id of its
version, each at the level of the rule's own severity. It is the manifest
`specht init` writes (`0001-F4` B-024, B-025).

This supersedes two statements of decision 0008: call 2's "The default
manifest declares `"rules": {}`", and its addendum's "A-4 leaves the `rules`
key out of the default". Everything else in decision 0008 stands.

B-044 stands as written: a manifest that sets no level for a rule id keeps
the rule's own severity, whatever the default manifest sets. The engine fills
no level from a default, so this repository lowering a rule in its own
manifest still reaches no consumer.

## Why

- The owner wants `init` to give the user every default (`0001-F4`
  decision 0002). The rule levels were the one part of the configuration the
  written manifest did not show.
- The addendum removed `rules` from the default for one reason: this
  repository's manifest was the default and sets a level of its own. B-044
  already answers that, and `0001-F4` B-004 leaves `rules` out of the
  live-copy comparison by name.
- Each level in the default is the rule's own, so the default still
  reproduces the baseline verdicts (B-016, C-4).

## Rejected

**Keep `rules` out of the default**, as the addendum had it. Cost of
rejecting: a manifest `init` wrote pins every level, so a later change of a
rule's default severity does not reach that consumer; the owner accepted
this.

**Fill a level the manifest leaves out from the default manifest**, making
B-044 a case of B-019. Cost of rejecting: none; it would hand a level set in
whichever manifest serves as the default to every consumer.

## Affects

- § 2 A-4: amended.
- Decision 0008: amended under Reversal.
- B-044, B-019, B-016, C-4: unchanged.
- `0001-F4` B-024 to B-026, B-004; `0001-F4` decision 0002.
- § 7, "The default manifest has one copy" and "The shipping copies": they
  describe the code before this ruling; the `implementer` owns them
  (`0001-F4` OQ-5).

## Reversal

None.
