---
title: "Decision 0008: Policy rejections name their field, and an absent exclude excludes nothing"
description: "An enforcement entry may omit exclude, and SPCT0001 names the policy field and fault the analysis rejects in place of the generic text"
type: decision
---

# Decision 0008: Policy rejections name their field, and an absent exclude excludes nothing

**Date:** 2026-11-11
**Decided by:** Repository owner; message texts drafted by spec-author

## The call

The repository owner answered two questions asked during item 0434.

Asked "codeAnalysis.enforcement[] entry without `exclude` currently fails with
the generic SPCT0001 message. Which behaviour should the spec state?", the
owner chose "Absent = empty (Recommended)", whose option read: "Missing
`exclude` means no exclusions; the entry is accepted. `include` stays
required, so scope stays explicit (C-3)." Rejected: "Stay required, name it".

Asked "How should SPCT0001 carry the specific reason for an invalid
codeAnalysis policy (e.g. a `verifies` scope with `declarationKinds` other
than `testMethod`)?", the owner chose "Specific message (Recommended)", whose
option read: "Replace the generic text with a message naming the field and
fault, as B-026 already does for `requiredCaseScope`. Applies to
policy-reader failures; other inputs keep the generic text." Rejected:
"Generic + property", "Generic + appended reason".

So:

- An `enforcement` entry with no `exclude` is read as excluding no source
  (B-028). `include` remains required and nonempty.
- A policy that projects through its mappings but holds a field value the
  analysis does not accept reports SPCT0001 with a message naming that field
  and its fault, in place of the generic text (B-029). The message names a
  field by its normalized analysis name, the name the mappings project to, and
  holds no path and no consumer value (C-4).
- Every other input failure keeps the generic text, `Required consumer input
is missing, invalid or incompatible with its explicit mappings.` (§ 5 row 5).
  B-026's required-case scope message and 0201-F7's Gherkin input messages
  stand.

The texts below were drafted by spec-author, not by the owner. They are the
agreed messages for the faults the owner's question named:

| Fault in one `enforcement` entry                                          | Message                                                                        |
| ------------------------------------------------------------------------- | ------------------------------------------------------------------------------ |
| `role` absent, or not `verifies`, `implements` or `follows`               | `Enforcement role must be verifies, implements or follows.`                    |
| `declarationKinds` absent or empty                                        | `Enforcement declarationKinds must name at least one declaration kind.`        |
| `declarationKinds` names a kind outside 0201-F2's declaration list        | `Enforcement declarationKinds names an unsupported declaration kind.`          |
| `role` is `verifies` and `declarationKinds` names other than `testMethod` | `Enforcement declarationKinds may name only testMethod when role is verifies.` |
| `include` absent or empty                                                 | `Enforcement include must select at least one source glob.`                    |

Any other policy rejection names its normalized field the same way, the
containing field first and the rejected field after it, then the fault; the
exact sentence is the implementer's, recorded in § 7.

## Why

A consumer walkthrough on the published `0.6.1-preview` packages hit both
faults and saw only the generic text, with nothing saying which field to
change ([lesson 0003](../lessons/0003-policy-rejections-reached-the-consumer-as-the-generic-message.md)).
The example policy in § 7 always carried `"exclude": []`, so the table's
silence read as optional, and a consumer omitting it was rejected for a field
that changes nothing when empty.

## Ruled out

- Keeping `exclude` required and naming it: the owner's rejected option.
- The generic text with the field as a diagnostic property, or with the
  reason appended: the owner's rejected options.
- A specific message for every input failure: the owner limited it to the
  policy reader.
- Echoing the consumer's value or a regular-expression engine's own message:
  a consumer value or a path in a diagnostic is ruled out by C-4 and brief § 9.
