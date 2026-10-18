---
title: "Decision 0001: Diagnostic severity defaults"
description: "Use warnings for binding defects and missing required bindings, information for duplicates and configurable per-rule severity"
type: decision
---

# Decision 0001: Diagnostic severity defaults

**Date:** 2026-10-17
**Decided by:** Repository owner

## The call

Use these defaults for the adopted `SPCT####` analyzer diagnostics:

| Findings                                                                           | Default severity |
| ---------------------------------------------------------------------------------- | ---------------- |
| Invalid or incomplete bindings, including malformed XML and unsupported placements | Warning          |
| Unresolved or ambiguous targets                                                    | Warning          |
| Lifecycle-ineligible targets                                                       | Warning          |
| Conflicting authoring defaults                                                     | Warning          |
| Missing required bindings within the selected enforcement scope                    | Warning          |
| Duplicate valid relationships                                                      | Information      |

Consumers can adjust severity per rule, including promotion to error. These are
reporting defaults; they do not change reference resolution, lifecycle
eligibility, evidence validity or authoring-default selection.

This policy does not choose which claims require links or which code must carry
bindings. Those remain epic OQ-5's enforcement-scope decisions. It does not
require bindings outside the selected scope, nor make an absent optional type
default a defect. Missing or invalid analysis-input behavior remains with the
input contract; its severity is not selected by this table.

The configuration mechanism, suppression and exclusion policies remain open in
OQ-2 / epic OQ-9. Reconciliation's output contract remains with 0201-F5 / epic
OQ-11; this decision does not invent a Roslyn location or report surface for
repository-wide gaps.

## Why

Warnings make traceability defects visible while allowing consumers to choose
stricter enforcement per rule. Valid duplicates still retain one relationship,
so their cleanup starts at information severity. Required-binding warnings
apply only where the consumer's selected enforcement policy requires a binding.

## Rejected

- Errors by default: imposes stricter enforcement immediately during adoption.
- Information for every finding: gives broken or missing required links the same
  default prominence as redundant but valid documentation.
- Warnings for valid duplicates: increases the prominence of cleanup even though
  analysis already retains the valid relationship.
- Fixed severities with no per-rule override: prevents consumers from choosing
  their own enforcement level during gradual adoption.

The accepted cost is consumer severity configuration when a repository wants
stricter or looser enforcement than these defaults.

## Affects

0201-F6 B-005 through B-007 and OQ-2; epic 0201 OQ-9's severity portion.
0201-F2 decision 0009 continues to own diagnostic identity/versioning. Exact
finding-to-ID assignments, input failure severity, suppression, exclusions and
configuration transport retain their existing owners and open questions.

## Reversal

None.
