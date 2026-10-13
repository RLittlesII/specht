---
title: "Decision {{NNNN}}: {{decision_title}}"
description: "{{one_line_summary}}"
type: decision
---

<!-- Copy to <feature>/.spec/decisions/{{NNNN}}-{{decision-slug}}.md and add a
     row to that Feature's § 3 or § 11 where it changed one.

     A decision record is a PRODUCT or SCOPE call — what will and will not be
     built. An ADR is about the code's structure. If it changes what the tool
     does, it is a decision; if it changes how the code is shaped, it is an ADR.

     Numbered per Feature from 0001, append only. A reversed decision keeps its
     original reasoning — the reversal is appended below, never an overwrite,
     so a later reader cannot rediscover the rejected option as a gap to fill. -->

# Decision {{NNNN}}: {{decision_title}}

**Date:** {{date}}
**Decided by:** {{who}}

## The call

<!-- What was decided, in the plainest available words. -->

{{the_call}}

## Why

<!-- The reasoning AT THE TIME. Do not update this later. -->

{{why}}

## Rejected

<!-- What was turned down and what turning it down costs. This is the half that
     stops the same idea being re-proposed as a gap. -->

{{rejected}}

## Affects

<!-- The claims, constraints or sections this changes:
     `B-00n`, `C-<n>`, `§ 5 row 4`. -->

{{affects}}

## Reversal

<!-- Only if this decision was later reneged or redirected. Append; never edit
     the sections above. -->

{{reversal}}
