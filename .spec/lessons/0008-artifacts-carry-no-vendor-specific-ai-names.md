---
title: "Lesson 0008: Artifacts carry no vendor-specific AI names"
description: "Keep vendor branding out of authored artifacts, including attribution and review records"
type: lesson
kind: process
---

# Lesson 0008: Artifacts carry no vendor-specific AI names

**Date:** 2026-10-13
**Kind:** process

## Symptom

Item 0209's work-item decisions, specification sign-off, architecture record and
published delivery report included vendor-specific AI attribution. The owner
rejected it and stated: "No vendor specific AI names in any artifacts."

## Root cause

Delivery guidance asked for contributor handoffs and attribution without stating
that functional responsibility does not authorize vendor branding. Attribution
was repeated across artifacts instead of keeping the decisions and evidence
focused on the work. The owner's preference should have governed every artifact.

## Spec delta

No product behaviour changes. Remove vendor-specific attribution from tracked
records and the existing delivery report. Preserve decisions, dates, evidence,
functional ownership and the distinction between self-review and independent
approval. Record the permanent rule in `AGENTS.md` and the delivery method.

## Claim

None. This is a process correction in item 0209's review round, not a new product
claim. Verification scans authored prose and attribution fields for prohibited
names and checks formatting and specification structure.

## Skill

[`deliver-change`](../../.claude/skills/deliver-change/SKILL.md) prohibits
vendor-specific AI names in every artifact. Handoffs retain work, findings and
responsibility; functional roles distinguish multiple contributions without
vendor attribution, and a single contributor needs no authorship byline.

`AGENTS.md` applies the rule to documentation, items, decisions, sign-off, reports,
comments and commit messages. The same prohibition applies when amending existing
artifacts; replacing a branded byline with another branded label is not a remedy.
