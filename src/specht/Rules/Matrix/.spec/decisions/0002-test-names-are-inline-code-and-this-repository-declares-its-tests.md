---
title: "Decision 0002: test names are inline code, and this repository declares its tests"
description: "A Test cell's test names are its inline-code spans alone; a waiver is <approver>, <YYYY-MM-DD> in the Test cell; this repository's manifest declares tests and owners in the pull request that delivers the matrix rules"
type: decision
---

# Decision 0002: test names are inline code, and this repository declares its tests

**Date:** 2026-10-15
**Decided by:** the repository owner (2026-10-15); recorded by spec-author

## The call

1. A Test cell's test names are its inline-code spans, each split on `,` and
   `;`; text outside inline code is not a test name. Plain-text names are not
   adopted now. B-003 stands as written. This resolves OQ-1.
2. `<approver>, <YYYY-MM-DD>` in the Test cell is the form of a waiver. B-018
   stands as written. This resolves OQ-2.
3. In the pull request that delivers these rules, this repository's manifest,
   `.specht/manifest.json`, declares `"tests": { "files": ["test/**/*.cs"] }`
   and `owners` from `specht-conventions` § "Section ownership", with `owners`
   listing `test-writer` and `spec-author` for `9. Traceability Matrix`. The
   same pull request fixes every row the rules then report, `0001-F3` B-033's
   Test cell among them, which quotes `major.minor.patch` in inline code. This
   resolves OQ-3.

## Why

Recorded by spec-author from the session:

- Inline code marks a test name without guessing: a status marker, an
  approver or a sentence in plain text is never read as one.
- The waiver form uses the Test cell a waived row has no tests for, and names
  a Reviewer § 12 already records, so B-018 checks it against the tree.
- This repository is checked by the tool it ships (brief § 7); rules that do
  not run on its own tree until a later item would ship unproven here.

## Rejected

The owner was offered these and chose against them; the cost after each is
spec-author's reading:

- **Splitting plain text in a Test cell on `,` and `;` as names too.** A cell
  of prose would yield names that are not tests. Rejecting it means a
  consumer who writes test names in plain text has each such cell reported
  by `SPEC032` (B-006) until the names are in inline code; plain text stays
  open to a later opt-in.
- **A waiver form other than `<approver>, <YYYY-MM-DD>`.** Not an option the
  owner was offered: the question named only "a different form", and no one
  put one forward. A different form would need its own decision.
- **Turning the rules on in this repository in a later, separate item.** The
  delivering pull request carries the cost instead: it also fixes `0001-F3`
  B-033's Test cell and any other row the rules report on this tree.

## Affects

- B-003, B-006, B-018; OQ-1, OQ-2, OQ-3.
- `.specht/manifest.json` (`tests`, `owners`), in the pull request that
  delivers these rules, under `0001-F5` B-055 (`tests`) and B-059 (`owners`),
  both still Missing in that Feature's § 9; `0001-F3` B-033's § 9 row;
  `0101-F9` B-024, through `owners`.

## Reversal

None.

## Addendum - 2026-11-02

Recorded by spec-author while item `0334` built `SPEC032`. Call 3 stands; its
`tests.files` value is amended. With `SPEC032` built and `tests.files` as
`["test/**/*.cs"]`, `./build.sh Specht` reported 59 rows, 71 errors, in
`.github/mirror/.spec/README.md` (`0055-F9`), whose tests are Python under
`.github/mirror/tests/test_*.py`, which that glob does not select.

The owner was offered two options and answered, verbatim:

- "Add the .py glob (Recommended)"

As put to the owner, the option meant: (a) add
`.github/mirror/tests/test_*.py` to `tests.files`. In effect, this
repository's manifest declares
`"tests": { "files": ["test/**/*.cs", ".github/mirror/tests/test_*.py"] }`,
so `0055-F9`'s § 9 rows resolve their Python tests and no `0055-F9` row
changes. `owners` is unchanged.

### Rejected

- **(b) Keep `test/**/*.cs` alone and rewrite the 59 Test cells.** Spec-author's
  reading of the cost: the cells name tests that exist, so a rewrite would
  serve the glob rather than the matrix, and `0055-F9`'s tests would stay
  outside every declared file.

### Affects

- `.specht/manifest.json` (`tests.files`); OQ-3's resolution, amended in place.
- `0055-F9` § 9, through the files its rows now resolve against.

Spec-author's readings, not stated by the owner:

- The live manifest is embedded as the engine's default manifest, so the
  whole-check benchmark's tree carries the second glob too; that tree holds
  no `.github/mirror/tests/`, so the glob selects nothing there.
- `0001-F4` B-004's comparison does not reach `tests`: the newest embedded
  manifest carries no `tests` key, and keys only the live manifest carries
  are outside it.
