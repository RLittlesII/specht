---
title: "Decision 0007: Default scenario inputs cover a central feature tree"
description: "The package delivers a root .spec/features tree as scenario inputs without a consumer build property"
type: decision
---

# Decision 0007: Default scenario inputs cover a central feature tree

**Date:** 2026-10-23
**Decided by:** Repository owner

## The call

The default scenario input glob keeps `**/.spec/*.feature` and
`epics/**/*.feature` and adds `.spec/features/**/*.feature` under the root
(B-027). A consumer keeping claims in a tree elsewhere still sets the
`SpechtScenarioInputs` build property; a narrowed property still replaces the
default.

## Why

Consumer request #256 keeps Gherkin in a central `.spec/features` tree. The old
default did not match it, so the consumer had to know a build property to get any
claims, and the omission showed only as an input failure (0201-F7 B-006). One
more default glob removes that setup step and changes no existing consumer's
delivered set.

## Ruled out

- A broader glob such as `**/*.feature`: it delivers test-project feature files
  that are not specifications.
- Reading the tree from the manifest's claim file sets: delivery stays MSBuild's,
  with policy in the manifest (decision 0005).
