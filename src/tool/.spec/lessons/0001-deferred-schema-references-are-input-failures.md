---
title: "Lesson 0001: Deferred schema references are input failures"
description: "A local schema can compile while retaining an unresolved reference, so controlled input failures must cover evaluation too"
type: lesson
kind: product
---

# Lesson 0001: Deferred schema references are input failures

**Date:** 2026-10-14

**Kind:** product

## Symptom

A disk Feature schema containing `{"$ref":"#/missing"}` compiled, but checking a
Feature exited `255` and wrote an error with a randomly generated schema URI to
stdout. No schema file was named on stderr.

## Root cause

JsonSchema.Net defers some reference resolution until evaluation. The original
B-105 stated only compilation failure, and the loader could not translate an
exception that arose later in a rule.

## Spec delta

`0001-F7` B-105 includes a reference that cannot resolve when the selected schema
is used. B-106 and B-107 still require a root-relative schema file diagnostic on
stderr and empty stdout. The shared frontmatter evaluation boundary translates
that deferred exception without changing valid reference semantics.

## Claim

- `0001-F7` B-105 to B-107 — the invalid disk schema scenario includes an unresolved local reference; unit and built-command regressions exercise discovered documents against that schema.
