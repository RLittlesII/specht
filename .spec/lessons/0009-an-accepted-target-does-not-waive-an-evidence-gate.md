---
title: "Lesson 0009: an accepted target does not waive an evidence gate"
description: "Schema commands pulled planned architecture into production before its benchmark gate"
type: lesson
kind: process
---

# Lesson 0009: an accepted target does not waive an evidence gate

**Date:** 2026-10-14
**Kind:** process

## Symptom

PR #185 added a handwritten mediator, model reader and global host factory to
schema list and pin. Its self-review approved that expanded scope without an
independent reviewer checking the migration prerequisites.

## Root cause

The implementation treated ADR-0010's accepted target as immediate execution
permission. Contributor instructions omitted the owner's benchmark cost-benefit
gate, and staged architecture work was pulled into a feature to follow the target.

## Spec delta

ADR-0014 records the clarified gate and the martinothamar/Mediator evaluation.
ADR-0010's status points to it; its accepted body is preserved. The feature uses
direct schema operations and leaves migration to its benchmark-gated items.

## Claim

The existing `0001-F7` B-041 to B-051, B-057 and B-058 remain the delivered
behavior, proved by their scenario, unit and CLI tests. The process correction
is proved by independent review against ADR-0014, not by a fabricated runtime
claim or an unsupported performance result.

## Skill

`coding-conventions` now requires checking execution gates and prerequisites
before introducing an accepted architecture. Its repository companion points
to ADR-0014 as the authority for this gate.
