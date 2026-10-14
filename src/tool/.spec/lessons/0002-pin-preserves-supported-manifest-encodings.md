---
title: "Lesson 0002: pin preserves supported manifest encodings"
description: "List and pin must accept the BOM encodings the existing check reads and preserve the original manifest bytes"
type: lesson
kind: product
---

# Lesson 0002: pin preserves supported manifest encodings

**Date:** 2026-10-14
**Kind:** product

## Symptom

The check accepted BOM-encoded UTF-16 and UTF-32 manifests, but schema list
and schema pin rejected the same files with exit 3.

## Root cause

The new operations decoded raw bytes as UTF-8. Preservation tests covered
only UTF-8, so the green suite missed valid inputs already accepted by the tool.

## Spec delta

B-045's byte-preservation scenario now covers UTF-8 and both byte orders of
BOM-encoded UTF-16 and UTF-32. List and pin retain the existing supported input
encodings; pin serializes only the changed token in that original encoding.

## Claim

- `0001-F7` B-041, B-043 and B-045: the encoding scenario and unit/process
  regressions accept the manifest and compare the exact expected bytes.
