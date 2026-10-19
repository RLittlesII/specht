---
title: Specht.CodeAnalysis diagnostic reference
description: Published diagnostic IDs, severities, and links to their explanations
type: reference
---

# Specht.CodeAnalysis diagnostics

The package publishes these diagnostics. Each page describes the current
reporting behavior and available code fix. Severity can be changed or suppressed
with standard Roslyn diagnostic controls; suppression does not change the
underlying analysis verdict. See the [package guide](../README.md) for input
configuration.

| ID                      | Title                                                   | Default severity | Code fix    |
| ----------------------- | ------------------------------------------------------- | ---------------- | ----------- |
| [SPCT0001](SPCT0001.md) | Consumer analysis input unavailable                     | Error            | No          |
| [SPCT0002](SPCT0002.md) | Reserved XML shape, placement or identity is incomplete | Warning          | Conditional |
| [SPCT0003](SPCT0003.md) | Identifier violates the consumer grammar                | Warning          | Conditional |
| [SPCT0004](SPCT0004.md) | Referenced target is absent                             | Warning          | Conditional |
| [SPCT0005](SPCT0005.md) | Referenced identity is ambiguous                        | Warning          | Conditional |
| [SPCT0006](SPCT0006.md) | Resolved target is ineligible                           | Warning          | Conditional |
| [SPCT0007](SPCT0007.md) | Required authored relationship is missing               | Warning          | Conditional |
| [SPCT0008](SPCT0008.md) | Partial type authoring defaults conflict                | Warning          | No          |
| [SPCT0009](SPCT0009.md) | Duplicate valid relationship                            | Info             | Conditional |
