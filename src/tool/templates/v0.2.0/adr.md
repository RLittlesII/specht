---
title: "ADR-{{NNNN}}: {{decision_title}}"
description: "{{one_line_summary}}"
type: adr
---

<!-- An ADR records a durable technical or architecture decision: a new rule
     about the system, a reversed one, or a technology choice with a rejected
     alternative.

     NOT an ADR: a process or workflow rule (that is a skill or a convention),
     an interface detail such as wording, or a product/scope call (that is a
     decision record in decisions/).

     Location is chosen by BLAST RADIUS, not by who made the call:
       - whole repo        → .spec/adr/, numbered repo-wide from 0001
       - one Feature       → <feature>/.spec/adr/, numbered per Feature from 0001

     AN ACCEPTED ADR IS IMMUTABLE. Only its status changes. A new ADR supersedes
     it entirely; never amend or delete an accepted record — a typo stays, because
     the record is history. -->

# ADR-{{NNNN}}: {{decision_title}}

## Status

{{proposed | accepted | superseded by ADR-NNNN | deprecated | rejected}}

## Context

<!-- The facts and constraints that force a decision. Cite the claims and
     constraints at stake as `<epic>-F<n> B-00n` / `C-<n>`. -->

{{context}}

## Decision drivers

<!-- Optional. The factors that decide it, in priority order. -->

{{drivers}}

## Considered options

<!-- Required. An ADR with one option is a note, not a decision. -->

1. {{option_1}}
2. {{option_2}}

## Decision

{{decision}}

## Consequences

<!-- Both directions. What this buys, and what it costs. -->

{{consequences}}
