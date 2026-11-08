---
title: "Decision 0023: An optional frontmatter field a patch adds is an opt-in addition"
description: "The owner's choice of 2026-11-08, made twice by selecting option A: an optional frontmatter field a patch adds to a shipped frontmatter schema is an opt-in addition under C-11, and B-154's same report excludes a tree that sets such a field the lower version rejects, so `0001-F5`'s claim prefix stays in schema 0.3.1"
type: decision
---

# Decision 0023: An optional frontmatter field a patch adds is an opt-in addition

**Date:** 2026-11-08
**Decided by:** the repository owner, selecting option A in the session
delivering item `0352` and again in the coordinating session; each option's
text is the wording of the session that wrote it, not the owner's; the
amendment wording is spec-author's; recorded by spec-author

## The call

1. **In the session delivering item `0352`**, the owner selected the option
   "A: amend B-154, keep 0.3.1". That session put option A as:

   > A (recommended): keep 0.3.1 as built. The owner amends B-154, and C-11's
   > opt-in clause, so that an optional field added to a shipped frontmatter
   > schema counts as an opt-in addition. That would also cover a tree the
   > lower version rejects only because that field is present. This matches
   > the owner's own 'not required is not breaking' basis for B-154.

   The other option was "B: move B-076 to B-080 to schema 0.4.0."

2. **In the coordinating session**, the owner selected "A: amend B-154
   (Recommended)". That session wrote its text:

   > An optional frontmatter field counts as an opt-in patch addition, even
   > when a tree that 0.3.0 rejected for writing that field now fails with a
   > different error. Matches your rule (non-breaking goes in a patch) and
   > decision 0022's "not required is not breaking". Roots pinned to 0.3.0
   > with pinPolicy latest-patch get prefixes without moving their pin, but
   > only trees that write claim_prefix see any change. 0.3.1 stays as built.

3. **Consequences, as the coordinating session's option states them.** A
   root pinned to `0.3.0` under `pinPolicy` `latest-patch` (B-053) gets
   `0001-F5`'s claim prefixes without moving its pin. Only a tree that writes
   `claim_prefix` sees any change. `0.3.1` stays as built, and `0001-F5`
   B-076 to B-080 stay in it.

4. **The amendments, in spec-author's wording.** C-11's opt-in addition also
   covers an optional frontmatter field that the lower version's shipped
   frontmatter schemas do not define, with every verdict it adds reached
   only when a document sets that field. B-154's same report excludes a tree
   in which a document sets such a field and the lower version rejects it. A
   `@B-154 @boundary` scenario states the `claim_prefix` case.

## Why

Both options the owner selected rest on decision 0022's rule, in the owner's
words there: "Additions are not breaking as long as they are not required."

Spec-author's reading, not stated by the owner:

- B-154's exception covers every tree that sets the field and that the lower
  version rejects for it, whatever else the tree holds. Option A's "rejects
  only because that field is present" would leave a tree that also holds
  another fault under the same-report guarantee, though writing the field
  changes its report as well; the coordinating session's option says only
  trees that write the field see any change.
- Neither option speaks to a root whose disk-selected Feature schema already
  permits the field, so that the lower version accepts a tree setting it.
  That case is OQ-76 and is not answered here.

## Rejected

- **Option B: move B-076 to B-080 to schema `0.4.0`.**

## Affects

- C-11 amended, with its Rules Out.
- B-154 amended, with a `@B-154 @boundary` scenario; its § 9 row is
  test-writer's.
- OQ-76 raised.
- `0001-F5` A-7 and `0001-F5` decision 0023 cite this decision for the
  patch.

## Reversal

None.
