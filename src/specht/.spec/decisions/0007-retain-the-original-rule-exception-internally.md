---
title: "Decision 0007: Retain the original rule exception internally"
description: "A fault violation keeps its original exception for later internal diagnostics while every public report and diagnostic remains sanitized"
type: decision
---

# Decision 0007: Retain the original rule exception internally

**Date:** 2026-10-27
**Decided by:** the repository owner, confirming the proposal while addressing
review 5675863896 on PR 587.

## The call

The fault violation retains the original exception object as internal metadata
(B-017), replacing the boolean provenance marker. Its presence identifies a
fault for filtering and severity grading. Record equality includes the exception
object's identity; a record copy keeps that same object. The projected report
and diagnostic text continue to carry only the existing fields and the safe
rule id / exception-type message (B-013, B-016).

## Why

A boolean loses the original exception, its stack and associated details.
Keeping the exception permits later internal diagnostics without inventing
another failure representation or changing the output contract today.

## Rejected

- Keep only the boolean: loses the failure that the owner asked to retain.
- Copy only the exception type or message: loses the original object and stack.
- Introduce a separate failure wrapper: no current information beyond the
  exception needs a second representation.
- Publish the exception in diagnostics or JSON: leaks machine-specific details
  and changes the report shape, contrary to B-016 and `0001-F3`'s contract.

## Affects

`0001-F1` B-017, § 7 to § 9, and the provenance implementation of B-015.
No logging command, report field, schema version or exception policy is added.

## Reversal

None.
