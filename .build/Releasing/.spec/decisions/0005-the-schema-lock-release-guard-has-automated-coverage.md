---
title: "Decision 0005: the schema-lock release guard has automated coverage"
description: "B-013 is proven by automated tests as well as hand runs, narrowing the release Feature's records-only boundary."
type: decision
---

# Decision 0005: the schema-lock release guard has automated coverage

**Date:** 2026-10-13
**Decided by:** the repository owner

## Decision

Asked whether to add automated coverage and amend § 5 row 9, or retain the
records-only rule and amend the work item's test criterion, the owner chose:
"Add automated coverage".

B-013 has unit coverage of the lock-membership decision and integration
coverage of the release guard. Its § 9 row names the tests. § 5 row 9 no
longer excludes tests of this third tag check. The earlier claims and
decision 0002 retain their existing record-based proof.

## Rejected

Closing B-013 on hand-run records with a missing test. The automated tests
must catch an unlocked older version even when the newest version is locked,
and must not write the lock or verify its content digest.
