---
title: "Decision 0008: the layouts are named epics and features, and the file-shape keys are lists"
description: "The default manifest names its two layouts epics and features in place of legacy and coLocated, and taskFiles, epicFiles and companionFiles each take a list of globs of which a file need match only one; an empty list is an invalid manifest"
type: decision
---

# Decision 0008: the layouts are named epics and features, and the file-shape keys are lists

**Date:** 2026-10-09
**Decided by:** the repository owner, in review of pull request #78 (item `0005`) - two inline comments on `.spec/schema/spec-structure.schema.json`, each followed by one question put to the owner; recorded by spec-author

## The call

Two calls, each amending [decision 0004](0004-the-discovery-inputs-are-five-top-level-manifest-keys.md).

**1. The layout names.** The owner's review comment: "Can we coin these
concepts `epics` and `features`?" The option the owner chose: "epics /
features".

| Glob                 | Name until now | Name from now |
| -------------------- | -------------- | ------------- |
| `epics/**/spec.md`   | `legacy`       | `epics`       |
| `**/.spec/README.md` | `coLocated`    | `features`    |

The default manifest declares the `epics` layout first and the `features`
layout second, the order decision 0004 gave them.

**2. The file-shape keys.** The owner's review comment: "named
`companionFiles` but it's not an array." The option the owner chose: "All three
become arrays (Recommended)".

| Key              | Shape                     | Default                |
| ---------------- | ------------------------- | ---------------------- |
| `taskFiles`      | A list of file-name globs | `["{task}-*.md"]`      |
| `epicFiles`      | A list of globs           | `["epics/**/epic.md"]` |
| `companionFiles` | A list of file-name globs | `["*.feature"]`        |

- A file matching any entry of a list counts.
- An empty list is rejected: the manifest is invalid and the tool exits `3`.
- Each default is the single glob decision 0004 gave the key, as a list of
  one.

## Why

What the owner was shown with each question, and accepted:

- A layout's name is public. It is the report's `layout` value, which
  `docs/schema/report.schema.json` describes, and it is what the summary
  prints (B-009).
- `SPEC011`'s interim under [decision 0005](0005-spec011-selects-the-legacy-layout-by-name-until-path-identity-is-declared.md)
  selects its layout by name. That name is now `epics`.
- `**/.spec/README.md` also matches an epic's specification found beside code.
  This repository's `src/specht/.spec/README.md` is epic `0001`, and it reports
  layout `features`.
- The sibling key `epicFiles` means `epic.md` files. It does not mean the
  `epics` layout.
- The empty-list rejection was the rule proposed in the option the owner
  chose.

Both calls change what a manifest written to decision 0004 means. They are
part of schema version `0.1.0`, which is unpublished (`0001-F7` decisions 0002
and 0005), so neither needs a new version.

## Rejected

**Keeping `legacy` and `coLocated`.** Decision 0004 chose them because they
were what the summary and the report printed, so the default manifest changed
no output. Cost of rejecting: under the default manifest the summary line and
the report's `layout` values now read `epics` and `features`, and every
expected output that spells the old names changes with them.

**Keeping the three keys as single globs.** Cost of rejecting: a key holding a
bare string is no longer the declared shape (§ 11 OQ-14), and the manifest
gains one more rejection (`0001-F5` B-041).

## Affects

- B-001: the default layouts are named `epics` and `features`.
- B-003: the three keys are lists, and each default is a list of one.
- B-012 (new): a file matching any entry of a list is discovered.
- `0001-F5` B-041 (new): an empty list is rejected. The rejection is
  `0001-F5`'s, as the exclusion entry's is (`0001-F5` B-021, § 5 row 2);
  `0001-F5` B-022 and B-023 carry the exit code and the output.
- B-009: unchanged. Its default output changes with the names.
- Decision 0004: the two default layout names, the Shape and Default cells of
  the three file-shape rows, and the Why line on `legacy` and `coLocated` are
  superseded. The five keys, their being flat and top-level, `layouts` being
  replaced whole and ordered (C-7), `{task}` and the glob dialect (C-6) stand.
- Decision 0005: the interim stands; the name it selects is `epics`.
- Decision 0007: stands. The repository-root `README.md` block omits the five
  keys and names no layout.
- § 5 row 7; § 11 OQ-4, OQ-5 and OQ-11, each amended in place; OQ-12, OQ-13
  and OQ-14, opened.
- `0001-F1` B-002 and B-003, `0001-F5` OQ-7, and the scenarios of `0001-F1`,
  `0001-F2` and `0001-F3` that named a layout: reworded to the new names.
- Not decided here: whether an empty `layouts` or `exclusions` list is also
  rejected (OQ-12); what a repeated entry, or a file two entries match, does
  (OQ-13); what a bare string or an entry that is not a glob does (OQ-14).

## Reversal

None.
