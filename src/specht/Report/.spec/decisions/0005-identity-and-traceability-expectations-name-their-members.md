---
title: "Decision 0005: Identity and traceability expectations name their members"
description: "The named expected members for identity, companion, claim and matrix violations, including all declared claim counts"
type: decision
---

# Decision 0005: Identity and traceability expectations name their members

**Date:** 2026-10-16
**Decided by:** the repository owner, answering item 0038's contract question; recorded by spec-author.

## The call

B-012 to B-015 name the members the existing rules add to `expected`:

| Rules                     | Members                                                                       |
| ------------------------- | ----------------------------------------------------------------------------- |
| SPEC011, SPEC012, SPEC043 | `form`, `value`; `firstDeclaration` for a reused identity                     |
| SPEC020                   | `companionCount`, `1`                                                         |
| SPEC021                   | `claims`, distinct valid declared ids in declaration order                    |
| SPEC030                   | `grammar`; `firstDeclarationLine` only for a duplicate                        |
| SPEC031                   | `requiredRows`, `1`; `rowsByClaim`, every valid declared id and its row count |

The owner selected every declared claim in `rowsByClaim`, rather than only the offending claim. Missing rows have count zero; an undeclared matrix id is not added to this map. Each violation owns its own JSON object. `expected` remains an open object in the published report schema, as decision 0004 specifies.

The identity form and extraction are defined in § 7; the choice does not add identity validation or change any guard, diagnostic, count or order. The first declaration is the first Feature in model order, the order already used by the rule; its path is root-relative. The model's existing claim derivation supplies valid ids and duplicate declaration lines.

## Rejected

- A matrix map holding only the offending id: smaller, but the owner chose one complete count map so the reader can assess every declared claim in the same expectation.
- Repeating a message as `expected`: violates C-2's structured-data contract.
- Changing rule verdicts or their diagnostic lines: outside this report-only change; the baseline remains the guard.
- Closing the `expected` object or editing shipped rule pages: neither is required for these additive report members.

## Affects

- `0001-F3` B-012 to B-015, their scenarios, design and evidence.
- Item 0038; no new rule, schema version or architecture migration.
