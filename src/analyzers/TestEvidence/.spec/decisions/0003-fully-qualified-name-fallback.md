---
title: "Decision 0003: FullyQualifiedName fallback"
description: "When the VSTest adapter supplies no managed method properties, the logger matches a case by FullyQualifiedName only when declaring type and method name are unique"
type: decision
---

# Decision 0003: FullyQualifiedName fallback

**Date:** 2026-10-23
**Decided by:** Repository owner ("Add the fallback")

## The call

When xunit.runner.visualstudio 2.x supplies no `ManagedType` or `ManagedMethod`
property on a test case, the VSTest logger matches the case by its
FullyQualifiedName: declaring type plus method name, only when that pair is
unique among the compiler-exported methods. xUnit1024 forbids overloaded test
methods, so a unique pair names one method. An ambiguous or unmatched name
stays unresolved. The display name is never used.

## Why

Adapter 2.x is what xUnit v2 consumers run, and it omits the managed method
properties. Without a fallback every case under it is unresolved and B-010 and
B-011 cannot be met for those consumers. Declaring type plus method name is a
stable identity when it is unique, so the match stays within C-5.

## Rejected

- Require adapter 3.x; discovery unavailable under 2.x.

## Affects

B-004, B-010, B-011; C-5; ADR-0001.

## Reversal

None.
