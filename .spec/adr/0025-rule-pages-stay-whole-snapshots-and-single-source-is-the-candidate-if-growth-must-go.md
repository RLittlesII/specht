---
title: "ADR-0025: Rule pages stay whole snapshots; single source is the candidate if growth must go"
description: "Item 0220's proposal. Five ways to source per-version rule pages were weighed, two of them on unmerged trial branches. Proposed: keep a whole snapshot per schema version for now; take single source with generated versions if the owner wants the growth gone; do not take history reconstruction or rule metadata as data. The record adopts nothing."
type: adr
---

# ADR-0025: Rule pages stay whole snapshots; single source is the candidate if growth must go

## Status

proposed - 2026-10-31. Decider: the repository owner. Item `0220`
([`.issue/0220-rule-page-versioning-spike.yml`](../../.issue/0220-rule-page-versioning-spike.yml)),
which quotes the owner's request of 2026-10-14. The record adopts nothing,
amends no claim and edits no accepted record; the one question it leaves the
owner is `0001-F7` OQ-69.

## Context

### How to read the evidence

- **Read from a commit**: file counts, byte counts, code and specification
  text. Baseline is `ffa0c8e9` (`main`, 2026-10-31).
- **Measured by a trial**: a build or test result, produced by a command run on
  a trial branch's commit and not re-run for this record. The branches are
  never merged (item `0220`) and may be deleted once the owner rules, so the
  facts that matter are quoted here.
- **Judgement** or **inference**: labelled where it stands.

| Trial                | Branch                     | Commit     | Candidate |
| -------------------- | -------------------------- | ---------- | --------- |
| Single source        | `0220/trial-single-source` | `3c29778d` | (b)       |
| Rebuild from history | `0220/trial-reconstruct`   | `9aa4a937` | (e)       |

Commits are abbreviated; the branch names are the stable handle.

### What the repository holds

Read from `ffa0c8e9`:

| Folder                           | Files | Bytes  | gzip -9 |
| -------------------------------- | ----- | ------ | ------- |
| `docs/rules/v0.1.0`              | 21    | 42,378 | 11,342  |
| `docs/rules/v0.2.0`              | 23    | 45,651 | 12,449  |
| `docs/rules/v0.2.1`              | 23    | 45,651 | 12,434  |
| schemas, each version            | 3     | 9,794  | ~2,420  |
| manifest, `v0.1.0`               | 1     | 1,827  | 1,262   |
| manifest, `v0.2.x`               | 1     | 2,714  | ~1,470  |
| templates, `v0.1.0` and `v0.2.0` | 4     | 12,928 | ~5,790  |
| templates, `v0.2.1`              | 4     | 7,112  | 3,478   |

- `git ls-files docs/rules` is 67 files, 133,680 bytes. A version's whole
  shipping set is 31 files; its pages are about two thirds of the bytes.
- Pages that differ from the previous version beyond the schema-version row:
  `0.1.0` to `0.2.0`, `SPEC021` changed and `SPEC034` and `SPEC035` are new, so
  20 of 21 old pages are unchanged; `0.2.0` to `0.2.1`, none of 23. The last cut
  copied 23 files for no change of text.
- A page is 23.4% non-prose (frontmatter, heading, metadata table: 10,687 of
  45,651 bytes at `v0.2.1`) and 76.6% prose.
- `src/specht/specht.csproj` embeds `docs/rules/v*/*.md` as
  `rules/v<version>/<file>`. `SchemaVersions` loads the pages eagerly per
  version; the vocabulary per version is a switch in C#, not read from the
  pages. `RuleEntriesReader` reads each rule's family and default severity out
  of the page's metadata table.
- `.build/SchemaVersionLock.cs` names `docs/rules` in `ShippingTrees` and
  hashes the ordered relative paths and bytes of `<tree>/v<version>` on disk.
  `.build/SchemaVersionCut.cs` copies each tree's newest folder and rewrites the
  `$id` of the schemas and the schema-version row of the pages.
- Every workflow checks out with `fetch-depth: '0'`; `nbgv` fails in a shallow
  clone (`0055-F5` C-2).
- `README.md:198`, `docs/cli.md:54`, `docs/what-it-checks.md:11` and
  `src/tool/README.md:31` link to `docs/rules/v0.1.0`.

Baseline tests, run at `ffa0c8e9`: the rule page unit tests, 272 passed; the
version lock and numbering integration tests, 6 passed.

### The constraints at stake

- One copy per version: `0001-F7` § 5 row 26, B-096 to B-099 and decision
  0007 (each version's folders are a whole snapshot); `0001-F3` C-6 and
  decision 0003 (the rule's text lives once, in the engine, as a page the host
  shows).
- Locked digest: `0001-F7` B-089 and C-1, with its
  [ADR-0002](../../src/tool/.spec/adr/0002-schema-locks-are-independent-of-tool-versions.md)
  (a digest over the files of a version, verified without Git history); the
  lock records `0.1.0`, `0.2.0` and `0.2.1`.
- Offline: brief § 9 and `AGENTS.md` § Invariants; `0001-F3` C-6 and B-019
  (`--explain` prints an embedded page).

### Where the item's text is stale

Corrections read at `ffa0c8e9`, not decisions:

- The item names `v0.1.0`, `v0.1.1` and `v0.2.0` at `73ff7ab`. The folders are
  `v0.1.0`, `v0.2.0` and `v0.2.1`; `0001-F7` decision 0013 stopped shipping
  `0.1.1`.
- The item says `--explain` is specified and not built. `0001-F3` B-019 is
  Covered.
- The item says items `0189` and `0044` may wait on this spike. Both are done
  (`0189` closed 2026-10-28, `0044` 2026-10-29), so the cut already copies every
  page and (b) and (e) amend built code and its tests.
- The item says the templates are byte-identical across the three folders.
  `v0.1.0` and `v0.2.0` are identical (4 files, 12,928 bytes); `v0.2.1` differs
  in all four (7,112 bytes).

A defect found in passing: the id `0220` is held twice, by this item and by
`.performance/benchmarks/Startup/.issue/0220-cold-start-baseline-bundled-framework.yml`
(done).

## Decision drivers

1. A published version stays immutable and its digest stays verifiable
   (`0001-F7` C-1, B-089).
2. The check and `--explain` stay offline and deterministic (brief § 9).
3. A build works in the places the project builds today, including a clone
   that has no history.
4. The repository does not grow by a whole set of pages for a cut that changes
   no text.
5. A change reverses as few accepted records as the gain justifies.

## Considered options

The trials added no option beyond the item's (a) to (e).

### (a) A whole snapshot per version - the baseline

- **Cost**: each cut adds 23 files, 45,651 bytes (12.4 KB compressed); the
  `0.2.0` to `0.2.1` cut added them for no text change. Review sees 23 new files
  of which at most a few carry a change.
- **Digest**: untouched. The lock verifies the folders as tracked; no history
  is read.
- **Offline**: untouched; the pages are embedded at build.
- **One copy per version**: this is the constraint, held as written.
- **Cut and page-only patch** (`0001-F7` B-096 to B-100, B-088): as built; a
  patch is an edit to the working folder.
- **Build**: no addition. A shallow clone builds.

### (b) One source per rule, versions generated at build

The trial (`0220/trial-single-source`, `3c29778d`): `docs/rules/source/` holds
one file per rule text (23 current texts plus `SPEC021@0.1.0.md`, the one text
that differs) and a `rules.json` giving the version list and, per rule, the
version ranges and file. A 28-line Python script writes
`docs/rules/v<version>/` (gitignored); a target in `specht.csproj` runs it
before `AssignTargetPaths` and embeds the output.

- **Growth** (measured): 25 tracked files, 50,148 bytes, against 67 and
  133,680. A cut adds one line to `rules.json`, none for the `0.2.0` to `0.2.1`
  step; a rule whose text changes adds one file holding the old text; a new rule
  adds one file.
- **Digest** (measured): generated output equals `ffa0c8e9` for `v0.1.0`,
  `v0.2.0` and `v0.2.1` (`diff -r`), in the working tree and in a `--depth 1`
  clone. The recorded digests are unchanged and match. The 6 lock and numbering
  tests pass unedited. Verification needs no historical checkout.
- **Offline** (measured): pages embedded at build as before; run time
  unchanged. The 272 rule page tests pass unedited from deleted generated
  folders, so `0001-F3` B-031 to B-033 hold.
- **One copy per version**: breaks it. Row 26 excludes a range or delta between
  version folders; decision 0007 accepted the snapshot as the cost.
- **Cut and page-only patch**: the cut no longer copies `docs/rules`; it adds a
  line to `rules.json`. `SchemaVersionCut` tests assert the copy and rewrite
  of pages (B-096, B-098, B-099), so they are edited, not run unedited. A
  page-only patch is one variant file and one line.
- **Build**: adds `python3` to `dotnet build`; `AGENTS.md` needs it for
  `.agents/install.py`, not for the build. A C# generator or an inline MSBuild
  task avoids it and was not trialled. Works in a shallow clone.
- **Traps met** (measured):
  - `CS1508`, a duplicate resource name: an unqualified `%(Filename)` batched
    across every `EmbeddedResource`, templates included. Fixed with a separate
    item type; a static glob cannot see generated files, so the embedding moved
    into a target.
  - The pre-commit Format step rejected all 24 source files: Prettier reflows
    the `| Schema version | {{schemaVersion}} |` row to the token's width. The
    trial branch was committed with `--no-verify`. Formatting the sources
    changes the padding, so the generated bytes and the locked digest, unless
    the token is exactly the version's width or the folder is excluded.
  - `dotnet build specht.csproj` now writes into `docs/rules/`.
  - Editing a source text changes every locked version its range covers; only
    the digest check catches it, not the review diff.
  - The rule pages stop being browsable on the repository host, the newest
    included; decision 0003 gave that as a reason for the page. The four links
    above would point at a gitignored path.
- **Not run**: `AcceptanceTest`, `Pack`, CI on Windows.

### (c) Rule metadata as data, rendering `--explain` and the site

No trial; read at `ffa0c8e9`. The non-prose head is 23.4% of a page, so data
shrinks each copy by at most about a quarter and a copy per version remains for
every text that changes. `RuleEntriesReader` already reads the page as the data
source for family and severity, so (c) inverts a built reader; the manifest
already carries a severity per rule. It reopens `0001-F3` decision 0003's
second rejected option, whose named cost stands: a family and severity typed by
hand can disagree with the engine, and B-031 and B-033 check the id and version,
not those.

- **Digest**: depends on the generated bytes; not measured.
- **Offline**: held if embedded.
- **One copy per version**: broken for metadata, kept for prose.

### (d) The site builds versioned pages from Git history

No trial. Alone it removes nothing from the tree: `--explain` embeds every
shipped version's text offline (`0001-F3` C-6, brief § 9), so the tool's build
needs the pages from somewhere, tracked or rebuilt. That makes (d) alone leave
the embedded set unsourced, and with it sourced the option is (e) with the
rebuild also feeding the site. Findings for (e) apply to it.

### (e) Newest pages tracked, locked versions rebuilt from history

The trial (`0220/trial-reconstruct`, `9aa4a937`): `git rm` of `docs/rules/v0.1.0`
and `v0.2.0`; `v0.2.1` stays. A 15-line shell script restores each locked
version whose folder is absent: `git rev-list -1 HEAD -- docs/rules/v<version>`
finds the commit that removed it and `git archive <that commit>^` restores it.
No commit is pinned. Two gitignore lines; the same csproj target shape as (b).

- **Growth** (measured): 23 tracked files, 45,651 bytes, constant; each cut adds
  a folder and removes the previous newest.
- **Digest** (measured): the restored folders equal `ffa0c8e9`; the 6 lock and
  numbering tests and the 272 page tests pass unedited, but only after the
  history step runs. Verification needs history, which ADR-0002 chose a content
  digest to avoid.
- **Offline** (measured): as (b), once restored.
- **One copy per version**: broken for locked versions, which no longer sit in
  the tree.
- **Cut and page-only patch**: copy the newest to the new number, then delete
  the previous newest. B-100 (no file outside the named version's folders is
  created, modified or deleted) fails as worded; B-096 and B-099 hold for the new
  folder. Every new version also needs a `.gitignore` line, or its restored
  folder shows as untracked.
- **Build** (measured): a full clone (`git clone --no-local`) builds. A shallow
  clone (`--depth 1`, one commit) fails with `no commit holds
docs/rules/v0.1.0`. CI is `fetch-depth: '0'` throughout, but a source archive
  or a contributor's shallow clone cannot build. Needs `bash`, `tar` and `git`
  on the Windows runner.
- **Pinning a commit per version does not work** with squash merge: the PR
  branch's commit does not survive on `main`, and a cut's own PR cannot name the
  squash commit it becomes. The trial searches history instead, which depends on
  `main` never being rewritten; one rewrite on 2026-10-08 was reported to the
  spike and cannot be checked from the tree.
- The pages of locked versions stop being browsable on the host; the four links
  to `v0.1.0` break.

## The site (criterion 6)

`0002-F2` B-003 holds the site unstarted, and nothing was started. Under (a) the
tracked folders are the source; under (b) the generated `docs/rules/v*/` tree is
a ready content source that a static site tool takes as it is (inference). Neither
adds to `0002-F2` OQ-1 (hosting) or OQ-2 (framework). Under (e) and (d) the
site's deploy job needs a full-history checkout, which feeds OQ-1.

## Templates (criterion 7)

`src/tool/templates/v*` is four files per version, copied by the same cut.
`v0.1.0` and `v0.2.0` are identical and `v0.2.1` differs. (b)'s model fits only
a file identical across versions, as a range per file; templates are
consumer-facing (`init` writes them), carry no version row and are rewritten
rarely. Recorded, not acted on: no option here changes them.

## Decision

Proposed. Each part is a judgement resting on the measurements above; the owner
decides.

- **Keep (a) now - judgement.** The cost is 23 files and about 12 KB compressed
  per cut, paid by the repository and not by the tool's behaviour. (a) keeps
  B-096 to B-100, decision 0003's host-browsable page and the offline lock, with
  no new build dependency.
- **If the owner wants the growth gone, take (b) - judgement.** It is the only
  option that keeps verification free of history and the recorded digests
  unchanged. Its preconditions: the Format exclusion for the source folder, a C#
  generator in place of Python, and an owner ruling that `0001-F7` § 5 row 26
  and decision 0003's host-browsability give way.
- **(e), (d) and (c) are not recommended - judgement.** (e) and (d) make the
  build depend on history, which fails in a shallow clone and a source archive,
  and a commit pin does not survive a squash merge. (c) removes at most a
  quarter of each copy and reopens a rejected option for it.
- **Revisit when `0002-F2` B-003 is lifted.** The site needs a generated
  versioned tree either way, and that is when (b)'s generator costs least.

## Consequences

Under (a), nothing changes: no specification text is owed and no item is cut.
The repository keeps growing by 23 files a cut, in the cases where a cut changes
no page.

Under (b) or (e), the specification text that would change; each is owed, none
is made here, and an accepted record is answered by a new one:

- `0001-F3` ([`src/specht/Report/.spec/README.md`](../../src/specht/Report/.spec/README.md)):
  A-4 (location and "ships embedded"); decision 0003 (location,
  host-browsability), which needs a reversal entry. B-031 to B-033 keep their
  wording and pass unedited. C-6 stands, and (b) states "lives once" more
  literally. C-8 stands: a generated page is still a function of the rule and
  its version. § 5 rows 9 and 10 stand.
- `0001-F7` ([`src/tool/.spec/README.md`](../../src/tool/.spec/README.md)): B-086
  (the folder in each tree is generated or restored); B-088 (a page-only patch);
  B-089 (the digest input is the generated or restored bytes); B-096, B-098 and
  B-099 (the cut); B-100 (fails as worded under (e); the cut touches
  `rules.json` under (b)); B-097 is untouched; C-16; § 5 row 26 ((b) is a range or
  delta between versions, (e) a fallback to history); decisions 0007 and 0009
  (the accepted cost, "a tool carrying each version's files"); and
  [ADR-0002](../../src/tool/.spec/adr/0002-schema-locks-are-independent-of-tool-versions.md)
  (the digest input and "without a historical checkout", which holds under (b)
  and fails under (e)).
- The links in `README.md`, `docs/cli.md`, `docs/what-it-checks.md` and
  `src/tool/README.md` to `docs/rules/v0.1.0`.
- Built code and tests: `.build/SchemaVersionCut.cs`,
  `.build/SchemaVersionLock.cs` (`ShippingTrees`), `src/specht/specht.csproj`,
  `SchemaVersionCut.Unit.Tests`, `SchemaVersionCut.Integration.Tests` and
  `VersioningSteps.Cut.cs`.

Untouched by every option: a locked version's content (`0001-F7` C-1), the page
shape (`0001-F3` B-032), and the set of versions a tool ships (decision 0007).

The question this record leaves the owner is `0001-F7` OQ-69: which option the
tool takes for per-version rule pages. A follow-up item is cut only after the
owner rules.
