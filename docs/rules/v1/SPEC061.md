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

A Feature specification's `spec_status` is the approved value and a row of its sign-off table has the draft or the blocked marker somewhere in a cell. The manifest sets all four:

| What                 | Manifest key       | Default manifest  |
| -------------------- | ------------------ | ----------------- |
| The approved value   | `markers.approved` | `approved`        |
| The draft marker     | `markers.draft`    | 🟡                |
| The blocked marker   | `markers.blocked`  | 🔴                |
| The sign-off section | `roles.signOff`    | `## 12. Sign-off` |

Reported once per such row, at the row, in fixed text whatever the manifest gives:

`spec_status is 'approved' while § 12 carries a row that is not approved`

Every row of the section's first table is read, rows kept from earlier review rounds included.

## Rule description

An approved specification has no draft or blocked sign-off row. Under any other `spec_status`, such rows are not reported.

### Example violation

A specification with `spec_status: approved` and this § 12 row:

```markdown
| 6-9 | 🟡 | spec-reviewer | Waiting on the design section. |
```

```text
src/orders/.spec/README.md(89): error SPEC061: spec_status is 'approved' while § 12 carries a row that is not approved
```

### Corrected

```yaml
spec_status: in-review
```

## How to fix violations

- **The review is not finished.** Set `spec_status` back to `in-review`.
- **The review is finished.** The reviewer updates the row so it carries neither marker.
- **The row records an earlier round.** Move it out of the section's first table.
