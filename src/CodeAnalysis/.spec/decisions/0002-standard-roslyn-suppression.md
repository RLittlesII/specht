---
title: "Decision 0002: Standard Roslyn suppression"
description: "Honor native diagnostic controls while preserving evidence validity, missing links and declared scope"
type: decision
---

# Decision 0002: Standard Roslyn suppression

**Date:** 2026-10-17
**Decided by:** Repository owner

## The call

Use standard Roslyn diagnostic controls for the `SPCT####` analyzer findings:
AnalyzerConfig severity settings, including `.editorconfig`, `#pragma warning`
and `SuppressMessage` suppressions. Honor those controls through the normal host
mechanisms rather than introducing a custom suppression XML element or parallel
suppression configuration. Decision 0001's per-rule severity overrides use these
standard reporting controls.

Suppression affects diagnostic visibility only. It does not:

- Make an invalid binding valid or make an ineligible target eligible.
- Fill a missing required evidence link or make incomplete coverage complete.
- Change the declared analysis scope or create an exclusion from it.

Evidence validity and required-link completeness are evaluated from the binding,
record and policy inputs, independently of whether a diagnostic is displayed.
A valid relationship retains its meaning when a diagnostic is hidden, and a
broken or absent relationship cannot become evidence through suppression.

Exclusions and required-link policies remain separate decisions under epic
OQ-5. This policy does not select a reconciliation report layout, require a
second suppression ledger or expand scope beyond the projects and declarations
the consumer selected. Missing/invalid input behavior and its diagnostic
severity remain with the input contract.

The native controls are described in Microsoft's [suppression documentation](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/suppress-warnings)
and [analyzer configuration documentation](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-options).

## Why

Consumers already have mechanisms for configuring and suppressing Roslyn
findings. Reusing them avoids another suppression language. Separating reporting
from evidence ensures a quiet diagnostic stream cannot certify an invalid
binding, fill a missing link or imply a different coverage scope.

## Rejected

- Add a custom suppression XML element or parallel configuration: adds another
  system for consumers and maintainers to learn and keep consistent.
- Treat a suppressed diagnostic as successful evidence: hides the defect by
  changing the traceability verdict rather than its presentation.
- Treat suppression as a scope exclusion: changes the declared coverage boundary
  without the explicit scope policy that owns that decision.
- Ignore standard Roslyn suppression: prevents consumers from managing analyzer
  findings through their existing tooling.

The accepted cost is keeping evidence computation independent of diagnostic
visibility, including when a rule is disabled through analyzer configuration.

## Affects

0201-F6 B-008 through B-011 and OQ-1/OQ-2; epic 0201 OQ-9's reporting-control
portion. Required-link completeness remains owned by 0201-F5, while exclusion
and enforcement policy remain open under epic OQ-5.

## Reversal

None.
