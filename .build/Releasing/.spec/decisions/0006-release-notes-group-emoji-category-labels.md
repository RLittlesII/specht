---
title: "Decision 0006: release notes group emoji category labels"
description: "GitHub groups generated release notes by PR category labels, with emojis and a rocket for Features."
type: decision
---

# Decision 0006: release notes group emoji category labels

**Date:** 2026-10-13
**Decided by:** the repository owner

The owner requested labels on every merged pull request so GitHub's generated
release notes group changes into useful categories, then requested emojis in
the labels and a rocket for Features.

B-006 retains GitHub's generated notes and gains category grouping. The
category vocabulary and ordering live in [`.github/release.yml`](../../../../.github/release.yml).
The contributor convention requires one category per pull request, chosen by
its delivered result. Specifications and benchmarks have their own categories
so an agreement or a measurement is not presented as a shipped capability.
A final catch-all retains unmatched pull requests; nothing is excluded.
The existing plain `dependencies` label remains supported, including Renovate's
specified label (`0055-F4 B-011`), alongside its emoji category label.

This changes release metadata only. The local `.issue/` tracker still owns
delivery state. Existing release bodies and package assets are not rewritten,
and this work creates no tag or release. The configuration takes effect in a
release from a commit containing it; labels alone do not update published notes.

Validation follows B-006's existing record-based proof: inspect the YAML,
verify GitHub label coverage, and request a generated-notes preview without
publishing. Automated workflow tests remain outside § 5 row 9.
