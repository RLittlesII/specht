---
title: "ADR-0011: Tool configuration lives in .specht/, and specifications in .spec/"
description: "The owner's decision of 2026-10-11. The files the tool writes and reads as its own configuration - the manifest, the three frontmatter schemas and the templates the tool ships - move from <root>/.spec/schema/ and <root>/.spec/templates/ to <root>/.specht/schema/ and <root>/.specht/templates/. The move is hard: the tool reads only .specht/, a root whose manifest is still at the old path exits 2 with a message naming the new one, and there is no fallback read and no migration command. Specifications, their .feature files and their records stay under .spec/. Supersedes the path in 0001-F5 A-1, C-6 and § 5 row 4 and in 0001-F2 A-3, and every claim and constraint that names the location is amended in place."
type: adr
---

# ADR-0011: Tool configuration lives in `.specht/`, and specifications in `.spec/`

## Status

accepted - 2026-10-11, by the owner, in session; recorded by spec-author. The
owner made the three decisions under Decision. Four points were not put to the
owner. Each is listed under Open points with a default, and each default
stands unless the owner overrules it in review.

Amended 2026-10-11, by the owner, in session, after this record was accepted:
the default of open point 1 is overruled. The manifest is
`<root>/.specht/manifest.json`, and the three frontmatter schemas are
`feature.schema.json`, `task.schema.json` and `epic.schema.json` under
`<root>/.specht/schema/frontmatter/`. The templates folder and its file names
are unchanged. Decisions 1 and 2 are read with those names; the retired path
Decision 2 names is unchanged, and no path used between this record and the
amendment is a retired one, since none shipped. Open points 2 to 4 stand. The
body below is unedited. Recorded in
[`0001-F5` decision 0011](../../src/specht/Manifest/.spec/decisions/0011-the-manifest-is-manifest-json-and-the-frontmatter-schemas-sit-in-their-own-folder.md).

## Context

Read from commit `60de077` when this record was written.

- **One folder holds two kinds of file.** A root's `.spec/` holds what its
  authors write - the brief, ADRs, lessons - and, under `schema/` and
  `templates/`, what the tool writes: the manifest
  `spec-structure.schema.json`, the three frontmatter schemas and the four
  templates. `init` creates those eight files and `upgrade` rewrites them
  (`0001-F4` B-018, `0001-F7` B-005).
- **The location is written as a literal in the engine and the tool**:
  `src/specht/Manifest/SpecManifest.cs:21` and `:48`,
  `src/specht/Model/SpecSchemas.cs:76` and
  `src/tool/Features/Init/InitWriter.cs:85`.
- **The specifications fix it.** `0001-F5` A-1 says the manifest "keeps its
  file name and place", C-6 reads it at
  `<root>/.spec/schema/spec-structure.schema.json`, and § 5 row 4 puts renaming
  it out of scope. `0001-F2` A-3 gives the same path. `0001-F1` C-8, `0001-F4`
  C-3 and `0001-F7` C-2 and C-4 name the two folders.
- **Nothing is published.** The repository has no git tag, `README.md` says
  "Not published yet", and schema version `0.1.0` is unpublished (`0001-F7`
  decision 0006). The only trees that hold the old folders are this
  repository's and `hooked`'s.

## Decision drivers

1. A reader of a root can tell the tool's files from the authors' by the
   folder they are in.
2. The tool reads its configuration from one place.
3. A move costs least before the first release.

## Considered options

What moves:

1. **The schema folder and the templates folder.** Chosen.
2. **The schema folder only.** Not chosen: the templates are tool-owned files
   `upgrade` rewrites, and would stay among the authors' files.
3. **The manifest only.** Not chosen: the frontmatter schemas `init` writes
   beside it would stay behind, and the schema set would sit in two folders.

How the move is made:

4. **A hard move: the tool reads only the new location.** Chosen.
5. **Read both, prefer the new.** Not chosen: the tool would carry a second
   read path for good, to serve trees that do not exist outside this owner's
   hands.
6. **The tool migrates a tree.** Not chosen: a command that moves files in a
   consumer's tree is a new write (brief § 9), built for two repositories.

## Decision

1. **Tool configuration lives in `<root>/.specht/`.** The manifest and the
   three frontmatter schemas are under `<root>/.specht/schema/`, and the
   templates the tool ships under `<root>/.specht/templates/`. `init` writes
   there, `upgrade` rewrites there, and the check reads there.
2. **The move is hard.** The tool reads nothing under `<root>/.spec/schema/` or
   `<root>/.spec/templates/`. A root with a manifest at
   `.spec/schema/spec-structure.schema.json` and none at
   `.specht/schema/spec-structure.schema.json` exits `2`, and the message names
   the new path and says the manifest at the old path is not read (`0001-F2`
   B-037, B-038). A root with a manifest at both is checked with the one under
   `.specht/` alone (`0001-F2` B-039). No command moves the files.
3. **Authored content stays in `.spec/`.** Specifications, `.feature` files,
   `decisions/`, `adr/`, `lessons/` and `.spec/brief.md` do not move.

### What this supersedes

| Record           | Superseded                                                                 | Stands                                                                    |
| ---------------- | -------------------------------------------------------------------------- | ------------------------------------------------------------------------- |
| `0001-F5` A-1    | "keeps its file name and place": the place.                                | The file name; that the manifest gains keys with the change that reads it |
| `0001-F5` C-6    | The path `<root>/.spec/schema/spec-structure.schema.json`.                 | Always read from disk; no embedded fallback; found nowhere else           |
| `0001-F5` § 5 #4 | That the manifest file is not moved.                                       | That it is not renamed                                                    |
| `0001-F2` A-3    | The path.                                                                  | The file name; that renaming it is not that Feature's                     |
| brief § 2, 5, 6  | `.spec/schema/` and `.spec/templates/` as where the tool writes and reads. | Everything else                                                           |

Each row is amended in place with its id kept. So is every other claim,
constraint and assumption that names either folder: in `0001-F1`, `0001-F3`,
`0001-F4`, `0001-F5`, `0001-F7`, `0101-F5` and `0101-F7`. `0001-F7` B-077,
B-080 and B-082, added after commit `60de077`, are amended with them.

`0055-F2` B-012 is amended too, with its § 5 row 11. It let a pull request
that changes only `*.md` files outside any `.spec/` folder skip the build; a
`*.md` file under a `.specht/` folder no longer counts as one, because the
shipped templates are such files and `0001-F4` B-004 is proven by the build.

No earlier ADR is superseded. A decision record or lesson that names
`.spec/schema/` or `.spec/templates/` is unedited: none of them decided the
location, and each is read with this record.

## Consequences

Gained:

- `.spec/` holds only what authors write, and `.specht/` only what the tool
  writes; `init` and `upgrade` write under one folder that holds nothing else.
- The engine reads its configuration from one path, with no second branch.

Paid:

- Every Feature claim and constraint naming the location is amended, and each
  test that asserts the old path is re-pointed by the build that moves the
  literals. Until then those tests pass against the old path.
- This repository's own `.spec/schema/` and the four shipped templates under
  `.spec/templates/` move in that build, with `AGENTS.md`, `README.md`,
  `docs/` and the skills that name them.
- The continuous-integration filter that skips a documentation-only pull
  request excepts `.specht/` as it excepts `.spec/` (`0055-F2` B-012), in the
  same build.
- A consumer whose tree holds the old folders - `hooked` - moves two folders by
  hand. Until it does, the check exits `2` and says where the manifest goes.
- `init` under a root that still holds the old folders writes a fresh set under
  `.specht/` and leaves the old one as it was (`0001-F4` B-028). A consumer who
  runs `init` before moving its manifest then holds two, and the check reads
  the fresh one.

## Open points

Not decided by the owner. Each default stands unless the owner overrules it in
review.

1. **Sub-folder and file names.** Default: kept. `schema/`,
   `spec-structure.schema.json`, the three `*.frontmatter.schema.json` names
   and `templates/*.md` are unchanged; only the parent folder changes.
2. **`.spec/templates/item.yml`.** Default: it stays where it is. Its own
   header says specht does not ship it; it is this repository's tracker schema,
   not tool configuration.
3. **The default manifest's `/.spec` exclusion.** Default: it stays, since the
   root `.spec/` still holds the brief, ADRs and lessons, and the owner
   rejected narrowing it (`0101-F7` decision 0004). No `.specht` exclusion is
   added: no default layout glob matches inside it.
4. **`schemaVersion`.** Default: unchanged. The manifest's content is the same
   and `0.1.0` is unpublished.
