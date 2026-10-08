---
title: "Lesson 0002: Extend what the owner put in place"
description: "A library or approach already in the repository is the starting point; replacing it is the owner's decision, asked before the change and recorded in the specification."
type: lesson
kind: process
---

# Lesson 0002: Extend what the owner put in place

**Date:** 2026-10-08
**Kind:** process

## Symptom

Item `0064` was to generate the CI workflow. The owner had committed a
Rocket.Surgery.Nuke `[GitHubActionsSteps]` declaration with its own middleware
one commit earlier (`487e5d8`). The agent ran that generator, saw output that
missed claims — no triggers, a global tool install, no entry script — and
replaced it with NUKE's own `[GitHubActions]` without asking. The replacement
was recorded only in the pull-request body, which neither merge path keeps. The
owner asked why the approach had been removed.

## Root cause

Friction with an existing tool was read as a reason to swap it, rather than as
configuration still to write. The owner's declaration already showed the
extension point — its middleware rewrites jobs and steps — and the gaps were
settings of the same kind. Nothing told the agent that a library or approach
already in place is the owner's decision, so the agent made a second decision
over the first, silently, inside an implementation.

## Spec delta

No change to `specht`'s behaviour. `.build/ContinuousIntegration/.spec/README.md`
(`0055-F2`) § 7 now names Rocket.Surgery.Nuke's `[GitHubActionsSteps]` and its
middleware as the generator, and pull request #2 was redone on it.

## Claim

- None — a process lesson proves nothing about the tool. `0055-F2`'s claims,
  bound by `ContinuousIntegrationSteps`, prove the redone workflow still meets
  the specification.

## Skill

[`coding-conventions`](../../.claude/skills/coding-conventions/SKILL.md)
§ "Before implementing" — **Extend what is already in place**: a library, tool
or approach the repository already holds is made to fit through its own
extension points; replacing it is the owner's decision, asked before the change
and recorded in the specification. `Never add` gains the silent replacement.
