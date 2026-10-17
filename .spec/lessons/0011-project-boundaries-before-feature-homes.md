---
title: "Lesson 0011: Project boundaries precede Feature homes"
description: "Co-located specifications should follow the adopted project layout without turning each Feature into a new project boundary"
type: lesson
kind: process
---

# Lesson 0011: Project boundaries precede Feature homes

**Date:** 2026-10-17
**Kind:** process

## Symptom

Owner review of PR 225 rejected the six src/analysis area folders and specified
Analyzers, CodeFixes and deployment CodeAnalysis instead.

## Root cause

The decomposition treated co-location as permission to choose project homes
independently of analyzer/code-fix/deployment responsibilities. Calling those
folders provisional left the architectural ownership invisible to the reviewer.

## Spec delta

ADR-0016 adopts the three sibling project homes and exact identities. Epic 0201
and its six agreements now use them; references and literal acceptance links
move together. Feature and claim IDs stay unchanged. No production behavior or
empty build project is introduced by the correction.

## Claim

No new product claim. The correction preserves 0201-F1 C-3 (framework-independent
identity), 0201-F2 C-3 (analysis versus invoked edits) and 0201-F6 C-2 (independent
Markdown checking). Verification checks the adopted locations, unchanged claim
sets, resolving links and discovery of all 32 draft scenarios on both targets.

## Skill

Updated specht-conventions layout guidance: locate agreements within the adopted
project homes; a Feature can span projects without duplicating its specification.
The companion owns the repository's exact paths and links ADR-0016 for identities
and dependency direction.
