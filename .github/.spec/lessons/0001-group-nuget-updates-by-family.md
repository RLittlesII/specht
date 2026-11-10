---
title: "Lesson 0001: Group NuGet updates by family"
description: "A repository-wide NuGet group mixed unrelated changes in a single pull request"
type: lesson
kind: product
---

# Lesson 0001: Group NuGet updates by family

**Date:** 2026-11-09

## Symptom

The hosted run opened [PR #809](https://github.com/RLittlesII/specht/pull/809)
with 18 updates spanning the compiler, test frameworks, dependency injection,
the MCP SDK and other libraries.

## Root cause

B-001 specified one pull request across the entire central package file.
The Renovate rule matched the file and update type without a package family.

## Spec delta

Related minor and patch updates share a pull request within each configured
NuGet family. Packages without a family get individual pull requests.

## Claim

`0055-F4` B-001, § 9 of the [dependency updates specification](../README.md).
