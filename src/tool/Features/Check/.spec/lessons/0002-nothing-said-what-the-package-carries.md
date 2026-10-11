---
title: "Lesson 0002: Nothing said what the package carries"
description: "The packed tool shipped every specification, scenario and work-item file under src/tool, because three solution-wide declarations made them content of every project and no claim said what the package holds"
type: lesson
kind: product
---

# Lesson 0002: Nothing said what the package carries

**Date:** 2026-10-11
**Kind:** product

## Symptom

A pack of commit `dbc060d` holds 168 files, and 134 of them are this
repository's working documents: under `content/` and again under
`contentFiles/any/net10.0/`, 23 Markdown files from `.spec/` folders, 5
`.feature` files and 39 `.issue/` items. The largest, the specification of
`0001-F7`, is 311,008 bytes, twice. `test-writer` found it while binding the
package scenarios.

## Root cause

`Directory.Build.props` declares `**\.spec\*.feature`, `**\.spec\**\*.md` and
`**\.issue\**\*.yml` as content of every project. Commit `254aabc` added the
three lines and records no reason; no project file or build target reads the
items by name. A pack includes a project's content, and `src/tool` is the one
project that packs (C-5).

The specification never said what the package holds. `0055-F1` B-009 counts
the packages, `0055-F5` B-005 reads the version, and until this pull request
no claim opened the package at all.

## Spec delta

B-033 added: the packed package carries no specification, scenario or
work-item file. § 5 row 21 leaves how those files are declared to the
`implementer`.

## Claim

- B-033 — `Missing` until the item that fixes it lands; § 9 names the test.
