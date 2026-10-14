---
title: "Lesson 0006: A path compared as a string is normalized first"
description: "A test compared a path it built with Path.Combine against the paths a file system returned; a folder constant carried a forward slash, so the comparison failed on Windows alone, and the pull-request build, which runs ubuntu only, let it reach main."
type: lesson
kind: process
---

# Lesson 0006: A path compared as a string is normalized first

**Date:** 2026-10-12
**Kind:** process

## Symptom

The push build on `main` failed on `windows-latest` in the `Unit Test` step on
every push from `1286b6b` (item `0183`, pull request #135); the last green push
was `83d6f80`. Run 38212538483 is one such failure. `ubuntu-latest` passed
throughout. The repository owner reported it on 2026-10-12.

One test failed,
`InitWriterUnitTests.TheEmbeddedShippingCopy_WhenWrittenIntoABareRoot_ShouldWriteTheEightFilesAndNothingOutsideTheTwoFolders`
in `test/tests/Init/InitWriter.Unit.Tests.cs`:

```
Expected object to match (file.Contains("repo\.specht/schema\", Ordinal)
OrElse file.Contains("repo\.specht/templates\", Ordinal)), but found
"C:\repo\.specht\schema\epic.frontmatter.schema.json"
```

## Root cause

A test defect, not a product defect. The writer wrote the right files to the
right folders on Windows.

The test built its two expected prefixes as
`Path.Combine(Root, SpecTree.SchemaFolder) + Path.DirectorySeparatorChar`. Item
`0183` changed `SpecTree.SchemaFolder` and `SpecTree.TemplatesFolder`
(`test/tests/Shared/SpecTree.cs`) from one segment to `.specht/schema` and
`.specht/templates`. `Path.Combine` joins its arguments and keeps a `/` inside
one, so on Windows the prefix was `repo\.specht/schema\`.
`MockFileSystem.AllFiles` returns normalized paths, and an ordinal comparison of
the two never matches.

Every other `Path.Combine(Root, SpecTree.SchemaFolder)` in the tests hands its
result to a file-system API, which normalizes it. This test alone compared it as
a string. On Unix the separator is `/` either way, so nothing showed.

It reached `main` because the pull-request workflow, `.github/workflows/ci.yml`,
runs `ubuntu-latest` only. The Windows gates run on a push to `main`
([`0055-F2` decision 0003](../../.build/ContinuousIntegration/.spec/decisions/0003-run-the-windows-gates-on-main-only.md)),
so a Windows-only failure is first seen after the merge.

## Spec delta

No delta. No claim, scenario or `.feature` changes: the writer's behaviour is as
specified on both systems, and the expected value in one test was wrong. The
test now builds each prefix with `fileSystem.Path.GetFullPath` and asserts
`StartsWith`. Item
[`0192`](../../.issue/0192-windows-init-writer-folder-assertion.yml) records
the fix.

Whether the pull-request build runs Windows is the owner's call and is not
decided here; no item is cut for it.

## Claim

None - a process lesson proves nothing about the tool. The test named under
§ Symptom is the one that failed and now passes on both systems; it cites the
claims it already cited. Review is what enforces the rule under § Skill.

## Skill

[`specht-conventions` § Testing](../../.agents/skills/specht-conventions/references/testing.md)
§ Fixtures gains **"A path a test compares as a string is normalized first"**:
pass it through the file system it came from, `fileSystem.Path.GetFullPath`,
before the comparison, because the `SpecTree` folder constants carry `/`,
`Path.Combine` keeps it, and the pull-request build does not run Windows. Its
`Never add` gains the string comparison of a path built with `Path.Combine`.
