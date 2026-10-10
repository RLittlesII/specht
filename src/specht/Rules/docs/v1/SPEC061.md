---
title: "SPEC061: An approved specification has no unapproved sign-off row"
description: A specification whose spec_status is approved still has a draft or blocked row in its sign-off table.
type: rule
---

# SPEC061: An approved specification has no unapproved sign-off row

## Metadata

| Property         | Value    |
| ---------------- | -------- |
| Rule ID          | SPEC061  |
| Family           | Approval |
| Default severity | Error    |
| Schema version   | 1        |

## Cause

A Feature specification's frontmatter has `spec_status: approved`, and a row of its `## 12. Sign-off` table carries a draft or a blocked marker. The rule reports once for each such row, at the row's line, with the message:

`spec_status is 'approved' while § 12 carries a row that is not approved`

The rule applies only when `spec_status` is exactly `approved`. A specification with any other status may carry draft and blocked rows and is never reported.

A row is not approved when any of its cells contains one of two markers:

| Marker | Meaning |
| ------ | ------- |
| 🟡     | Draft   |
| 🔴     | Blocked |

The marker may be anywhere in the cell, alone or beside other text, and in any column. A row with neither marker passes, whatever else it holds; the rule does not look for the approved marker, 🟢.

Every row of the table is read. The rule does not tell a row that records an earlier review round from the row that records the latest one: a blocked row kept as history is reported like any other. The table read is the first table of § 12, and a specification with no § 12 is not reported here.

## Rule description

A specification's approval is stated twice. § 12 holds the reviewer's sign-off, section by section; the frontmatter's `spec_status` is the one word other tools and people read. The frontmatter is set to `approved` only when every sign-off row is. This rule compares the two, so that the word cannot run ahead of the review it summarises.

The comparison runs one way. A specification whose rows are all approved but whose `spec_status` is still `in-review` is not reported.

### Example violation

A specification with `spec_status: approved` in its frontmatter and this sign-off:

```markdown
## 12. Sign-off

| Section | Status | Reviewer      | Note                           |
| ------- | ------ | ------------- | ------------------------------ |
| 1-5     | 🟢     | spec-reviewer | Approved.                      |
| 6-9     | 🟡     | spec-reviewer | Waiting on the design section. |
```

```text
src/orders/.spec/README.md(90): error SPEC061: spec_status is 'approved' while § 12 carries a row that is not approved
```

### Corrected

The review of sections 6 to 9 has not finished, so the frontmatter says so:

```yaml
spec_status: in-review
```

## How to fix violations

Which fix applies depends on which of the two statements is true.

- **The review is not finished.** Set `spec_status` back to `in-review`. It returns to `approved` when the reviewer has approved every row.
- **The review is finished and the row is stale.** The reviewer updates the row to the approved marker. The author of the specification does not change a sign-off row to clear this line; the row is the reviewer's record.
- **The row records an earlier round.** The rule reads it as it reads any row. Keep the first table of § 12 to the current sign-off of each section, and move the record of earlier rounds to prose or to a second table below it, which is not read.
