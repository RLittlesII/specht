---
title: "Decision 0003: CSharp declaration scope"
description: "Support XML-documentable types and members with role-specific placements and no implicit member evidence"
type: decision
---

# Decision 0003: CSharp declaration scope

**Date:** 2026-10-17
**Decided by:** Repository owner

## The call

Use CSharp's XML-documentable type and member surface, with these placements:

| XML role     | Supported declarations                    |
| ------------ | ----------------------------------------- |
| `spec`       | Types, supplying authoring defaults only. |
| `verifies`   | Recognized test methods only.             |
| `implements` | XML-documentable types and members.       |
| `follows`    | XML-documentable types and members.       |

Types include classes, records, structs, interfaces, enums and delegates.
Members include constructors, methods, properties, indexers, fields and events,
and other XML-documentable members such as operators, conversions, finalizers
and enum members. The rule follows that documentable surface; the examples are
not a methods-only whitelist.

An explicit type binding describes the type itself. It never supplies an
implicit binding to any member. The type's authoring-default role remains
separate from its explicit claim or ADR relationships. Every checked target
retains its own complete identity under 0201-F1 decisions 0002 and 0003.

This decision does not extend support to other languages, namespaces, assembly
or local code declarations outside that type/member surface. Recognition of
test methods remains owned by 0201-F3. Enforcement scope, generated-code policy,
nested and partial default rules, multiple-target XML forms and diagnostic
severity remain in their existing open questions.

## Why

Claims and architectural decisions can concern construction, stored state,
properties and a type as a whole. Supporting their actual declarations keeps
those relationships explicit without forcing documentation onto an unrelated
method.

The language's [documentation-comment specification](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments)
provides the declaration boundary. The analyzer still owns the role and target
validation; compiler acceptance of a custom XML element is not a valid binding
verdict.

## Rejected

- Methods only for the first release: reduces initial syntax coverage, but leaves
  constructor, property and type relationships without their own declarations.
- Let a type binding cover its members: saves repeated IDs but creates unchecked
  evidence and permits silent retargeting when members move.
- Permit every role on every declaration: simpler placement rules but allows
  non-test declarations to claim test verification and confuses default context
  with evidence.

The accepted cost is broader declaration coverage in analysis and invoked fixes,
with tests across the supported type and member kinds.

## Affects

0201-F2 B-001, B-005, B-012, B-013 and OQ-1; epic 0201 OQ-2. Nested and partial
default selection remains open in 0201-F2 OQ-3 / epic OQ-12.

## Reversal

None.
