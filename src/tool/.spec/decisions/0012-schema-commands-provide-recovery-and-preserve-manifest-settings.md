---
title: "Decision 0012: schema commands provide recovery and preserve manifest settings"
description: "Listing failures provide a recovery path; pin validates inputs in order, repairs the current pin and preserves settings on downgrade; non-interactive selection needs a version"
type: decision
---

# Decision 0012: schema commands provide recovery and preserve manifest settings

**Date:** 2026-10-14
**Decided by:** the repository owner, while requesting implementation

The owner accepted root and non-version manifest validation before target
validation, repair of an unsupported or malformed current pin, and preservation
of newer settings on downgrade. The check may reject those settings after the
pin moves down; pin does not remove or rewrite them.

With no version and no interactive terminal, pin writes nothing, prints a hint
to use `specht schema pin <version>` and exits `2`. Rejected: exit `4`, which
remains the unshipped or malformed target's failure.

For listing, the proposed mark was the suffix ` (pinned)`, and an unreadable or
unsupported pin was proposed to exit `3` with no list on stdout. The owner
asked whether the exit leaves the user a way to recover. The implementation
therefore pairs the failure with the problem, shipped versions and a recovery
hint on stderr. An unsupported or malformed pin can be repaired through pin;
invalid JSON or other invalid settings require editing the manifest first.
The suffix from the walkthrough is retained. Rejected: failing without recovery
information, or silently treating an invalid manifest as absent.

Pin validates settings under the current shipped version when available, or
the newest shipped version when the current pin cannot be used. This is the
implementation's interpretation of validating non-version settings while
repairing only the pin. Pin changes no setting other than `schemaVersion`.

The prompt title, initial highlight and successful pin confirmation are
presentation choices: the selector offers the catalog in ascending order, and
success names the selected version and `.specht/manifest.json`. A supplied
version never needs a prompt. Cancellation behavior and floating policies are
not expanded in this change.

OQ-14, OQ-17, OQ-20, OQ-23, OQ-48 and OQ-49 are resolved. B-041 and B-084 are
amended; B-108 to B-114 and their scenarios record the failure and recovery
boundaries. Existing IDs are retained.
