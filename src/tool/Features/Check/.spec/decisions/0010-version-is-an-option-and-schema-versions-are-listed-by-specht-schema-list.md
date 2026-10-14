---
title: "Decision 0010: The tool's version is an option, and schema versions are listed by specht schema list"
description: "specht --version prints the version and commit the tool's assembly states; there is no version command; the shipped schema versions are listed by specht schema list, which marks the version a root pins; no manifest --version"
type: decision
---

# Decision 0010: The tool's version is an option, and schema versions are listed by specht schema list

**Date:** 2026-10-11
**Decided by:** the repository owner; B-041 to B-044, the "one line" and "nothing else" of B-040, and the open questions are spec-author's derivation

## The call

The owner first asked for one `version` command printing the tool version, the
git hash and the schema version, then withdrew it, 2026-10-11:

> I just realized I shouldn't do this. `specht --version` should print the tool
> version `specht schema --version` should print schema version infor (all
> available) `specht manifest --version` should print the manifest schema
> version?

Asked to choose on each surface, the owner picked, 2026-10-11. These are
choices among options put to the owner, not the owner's prose:

1. `specht --version` prints the version and the commit as one string, the
   assembly's informational version, such as `0.1.59+dbc060d079`.
2. The schema versions the tool ships are listed by `specht schema list`, not
   by `specht schema --version`.
3. The schema version a root's manifest pins is shown by marking it in that
   list when the root has a manifest; the list is plain otherwise. There is no
   `specht manifest --version`.
4. This change builds `specht --version` only. Choices 2 and 3 are recorded in
   the specification for later items.

## Why

- **Choice 1.** The string is the one B-030 already holds each assembly to
  state. No other reason was recorded.
- **Choice 2**, as put to the owner and accepted: `--version` on a subcommand
  reads as the program's version, and an option that lists is odd;
  `specht schema list` leaves room for the pin command under the same `schema`
  branch.
- **Choice 3**, as put to the owner and accepted: a boolean
  `manifest --version` would take the name `0001-F9` OQ-3 may want for a
  `--version <x.y.z>` value.
- **Choice 4.** No reason was recorded.

## What is not decided

- The `-v` short form the command-line library gives the option (OQ-7).
- `--version` after a command name or beside another option (OQ-8).
- How the mark looks, what the list does when the pin cannot be read, and
  which version is marked under a pin policy (`0001-F7` OQ-48 to OQ-50).
- The pin command's name, and where it sits (`0001-F7` OQ-11). The room under
  the `schema` branch is a reason offered with choice 2, not a ruling.

## Rejected

**One `version` command printing the tool version, the commit and the schema
version.** The owner's first ask, withdrawn by the owner. Cost of rejecting:
the tool's version and the schema versions are two invocations.

**`specht schema --version` listing the shipped schema versions.** The
owner's first wording. Cost of rejecting: a command name, `list`, under a
`schema` branch that does not exist yet.

**`specht manifest --version` printing the pinned schema version.** The
owner's first wording, put as a question. Cost of rejecting: no invocation
prints the pinned version alone; a caller reads the mark in the list, or
`schemaVersion` in what `specht manifest` prints (`0001-F9` B-006).

## Affects

- B-040 to B-044, added. § 2 row 7. § 5 row 22 withdrawn, rows 24 to 27
  added. OQ-7 and OQ-8 raised.
- `check.feature`: `@B-040` to `@B-044`.
- `0001-F7`: B-041 and B-042 amended, B-084 and B-085 added; OQ-11 and OQ-19
  resolved in part; OQ-48 to OQ-50 raised.
- `0001-F9`: § 5 row 8.
- `0055-F5`: § 5 row 3 withdrawn.
- `0109-F4`: § 5 row 8 and decision 0002 annotated; OQ-3 raised.

## Reversal

None.

## Pin command naming resolved 2026-10-14

The repository owner named and placed the command as `specht schema pin <version>`
([`0001-F7` decision 0005, command-name addendum](../../../../.spec/decisions/0005-a-schema-version-is-semver-pinned-exactly-moved-by-pin.md)).
The pin command's name and placement, listed above as undecided on 2026-10-11,
are now agreed. The list marker, unreadable-pin behaviour and pin-policy
questions remain open.
