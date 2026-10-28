---
title: "Lesson 0016: Ask before building a copy of live state"
description: "A copy of state whose authority lives elsewhere, and a test of a document's own text, are built only on the owner's yes"
type: lesson
kind: process
---

# Lesson 0016: Ask before building a copy of live state

**Date:** 2026-10-28
**Kind:** process

## Symptom

Item `0090` (`0055-F8`, the repository settings checklist) was delivered as
pull request #600 without asking the owner first. It added a 92-line
checklist copying the live GitHub ruleset and merge settings into
`.github/settings/checklist.md`, a 189-line test class that parsed that
Markdown and asserted it said what it said, and 108 lines of specification
§§ 6 to 9 and § 12. The owner rejected and closed it on 2026-10-28: a
checked-in copy of live settings goes out of sync the moment a setting
changes on GitHub, and it is maintenance with no payoff.

## Root cause

The deliverer treated an approved specification and a ready item as a
mandate and built the work, through three role passes, without asking
whether it was wanted. The specification's own C-3 and decision 0001 already
conceded that nothing catches drift between the copy and GitHub. A copy of
state whose authority lives elsewhere, and tests that only assert a
document's own text, cost upkeep and prove nothing.

## Spec delta

None in this change. Whether `0055-F8` and items `0089`, `0090` and `0091`
are retired is the owner's decision, not yet made.

## Claim

None - a process lesson proves nothing about the tool. The deliverer asking
the owner, under the rule in § Skill, is what enforces it.

## Skill

`deliver-change` § "The shape of it" step 4 now says to ask the owner before
building a copy of state whose authority lives elsewhere, or a test that only
asserts a document's own content, even when an approved specification or a
ready item calls for it, and to build only on a yes. Its `Never add` list
carries the matching entry.
