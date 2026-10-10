---
title: "Decision 0003: rule pages are the rule text"
description: "Each rule id has one Markdown page per schema version, embedded in the engine, and that page is the full text C-6 names and --explain prints; a docs folder --explain does not read and pages generated from C# descriptors were turned down"
type: decision
---

# Decision 0003: rule pages are the rule text

**Date:** 2026-10-09
**Decided by:** the repository owner, asking for a documentation page per `SPEC###` rule (2026-10-09); recorded by spec-author

## The call

Every rule id in a schema version's vocabulary has one Markdown page for that
version, embedded in the engine at `src/specht/Rules/docs/v<n>/SPEC###.md`. The
page is the rule's full text: the text C-6 says lives once, and the text
`--explain` prints (B-019). `0001-F3` owns the pages. This change delivers the
pages and a check that they are complete; `--explain` stays where it was.

A page has the shape of an analyzer rule page:

- frontmatter: `title` (`SPEC031: <title>`), `description`, `type: rule`;
- `# SPEC031: <title>`;
- `## Metadata`, a Property and Value table with the rows Rule ID, Family,
  Default severity and Schema version;
- `## Cause`, what the rule checks;
- `## Rule description`, what it expects, with `### Example violation` - a
  snippet and the line the tool prints - and `### Corrected`;
- `## How to fix violations`.

Not decided: the owner named the folder `v<n>`, which is `v1` today, as the
embedded schema folder is. What that segment is for a `major.minor.patch`
version is `0001-F7` OQ-22; that the page folder takes the same answer is the
author's inference (A-4).

## Why

The first reason is the owner's; the rest are the author's reading of the call.

- C-6 already says a rule's full text lives once, in the engine, versioned with
  the schema. A page per version in the engine is that text, so C-6 stands as
  written and nothing is catalogued twice.
- A rule's text is prose with examples. Markdown holds it as written, and a
  reader on the repository host and a reader of `--explain` see the same page.
- The page shape is one readers of analyzer documentation already know: a
  violating example beside the corrected one answers what need 6 asks.
- The pages are files, so a completeness check is a comparison of two lists:
  the version's vocabulary and the pages beside it (B-031).

## Rejected

**Standalone pages under `docs/` that `--explain` does not read.** Written for
people, apart from the engine.

- It is the second catalogue C-6 rules out: two texts for one rule, and nothing
  that makes them agree.
- Cost of rejecting: the pages sit under `src/`, not where a reader looks for
  documentation first. Taken.

**Rule text in C# descriptors, with the pages generated from them.** One
descriptor per rule carrying title, cause and examples; a generator writes the
Markdown.

- It needs a descriptor type and a generator before the first page exists, and
  neither is asked for by any claim.
- Cost of rejecting: a page's Rule ID, Family and Schema version are typed by
  hand and can disagree with the engine. B-031 and B-033 check the id and the
  version; nothing checks the family or the default severity. Taken.

**A new Feature, or `0002-F1`, as the owner of the pages.** Documentation as a
deliverable is epic `0002`'s.

- The pages are the text `--explain` prints, and `--explain` is this Feature's;
  a second owner would split one text across two agreements.
- Cost of rejecting: epic `0002`'s "Out of this epic" table now points here for
  the rule pages. Taken.

**"Enabled by default", "Code fix" and "When to suppress" from the reference
shape.** The three parts of an analyzer page this one leaves out.

- The tool applies no fix (§ 5 row 1) and has no suppression; severity and
  disable are `0001-F5` B-010 and B-011, unbuilt.
- Cost of rejecting: a page says nothing on turning a rule down or off until
  those claims are delivered and a later decision adds the part. Taken.

## Affects

- `0001-F3` § 2 need 6, A-4, A-5; § 3 B-031, B-032, B-033; § 4 C-8; § 5 rows 8
  to 11; § 11 OQ-3. C-6 and B-019 are unchanged.
- Epic `0002`, "Out of this epic": the rule pages are `0001-F3`'s.

## Reversal

**2026-10-09, redirected by the repository owner**, reviewing the pages on the
pull request that delivered them. The sections above are as written.

- **Location.** The owner: "I was thinking these would live at root `docs/`".
  A page's file now sits at `docs/rules/v<n>/SPEC###.md`, `docs/rules/v1/`
  today, not under `src/specht/Rules/docs/`. The `rules/v<n>/` sub-path and
  keeping the version segment are the author's reading, not the owner's words:
  the text is versioned with the schema (C-6), and `docs/schema/` is the
  precedent for a published file under `docs/`.
- **Terseness.** The owner: "These are more verbose than I'd like can we make
  them terse?!". A page states each condition the rule reports and its
  message, one minimal example and the fix, and carries no background (§ 5
  row 12). The page shape is unchanged.
- **Unchanged.** The pages are the rule text, the engine embeds them - now
  from `docs/rules/` - `--explain` prints them, and `0001-F3` owns them. The
  first Rejected entry stands as written: what it turned down is a page
  `--explain` does not read, and its stated cost, pages away from where a
  reader looks first, is no longer paid.
- **Affects.** `0001-F3` A-4 (amended), § 5 row 12 (added). C-6, B-019 and
  B-031 to B-033 are unchanged. `0001-F7` item `0126` names the new folder.
  Epic `0002` and `0002-F1` § 5 row 4 say the pages under `docs/` are
  `0001-F3`'s.
