---
title: "Decision 0005: B-012 is proven without a report path"
description: "B-012 is proven without a report path; the owner later permits the command-level read-only proof through the existing setup while the report exemption stays in 0001-F3"
type: decision
---

# Decision 0005: B-012 is proven without a report path

**Date:** 2026-10-09
**Decided by:** the repository owner, in session; recorded by spec-author

## The call

B-012 is proven in two halves:

- **Here:** the `@B-012` scenario runs the check with no report path, launching
  the built tool, and asserts no file under the root was created, modified or
  deleted.
- **In `0001-F3`:** the exemption - the file `--report` names is the only write
  under the root - is proven with B-022, B-024 and `0001-F3` C-7, the claims
  that build that file.

The command-tester test C-7 calls for waits on the test harness
[ADR-0007](../../../../../.spec/adr/0007-each-part-registers-itself-and-one-factory-composes-the-host.md)
decides. B-012's text is unchanged.

## Why

- `--report` is not built, so a scenario that names a report path cannot pass
  until `0001-F3` lands, and B-012's no-write half would wait on it for nothing.
- A command-tester test now would need a private helper over Spectre's command
  tester, which the owner rejected on PR #4.

## Rejected

**One scenario with a report path, as first written.** Proves both halves at
once. Cost of rejecting: the exemption is asserted in `0001-F3`'s tests, not
in this Feature's.

**A private `Check` helper over the command tester.** Rejected on PR #4. Cost
of rejecting: until the harness exists, B-012 is proven only at the acceptance
tier.

## Affects

- `check.feature` `@B-012`: runs with no report path.
- § 5 row 10.
- `0001-F3` C-7: its proof also proves B-012's exemption.

## Reversal

2026-10-27, the repository owner permitted B-012's read-only test through the
existing command setup, without starting ADR-0010's architecture migration.
The harness prerequisite above is reversed for this test only. The built-tool
scenario remains, the command integration test uses the existing setup, and
`0001-F3` still owns the report-path exemption. ADR-0014's benchmark gate and
the rest of ADR-0007's migration remain unchanged.
