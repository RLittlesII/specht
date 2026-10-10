---
title: "Decision 0005: The claim tag case setting is how a tag reads"
description: "A manifest may declare that claim tags are read ignoring letter case; that is where SPEC021 looks, within C-7, because what the rule asserts - every claim tag resolves to a claim in the claims section - does not change"
type: decision
---

# Decision 0005: The claim tag case setting is how a tag reads

**Date:** 2026-10-09
**Decided by:** spec-author, on the owner's direction of 2026-10-09 that a
consumer's claim tags be as configurable as the manifest allows. The owner chose
the tag form and the case setting; the C-7 judgement below is spec-author's and
is the owner's to overturn.

## The call

The manifest carries two values for claim tags: a tag form, literal text around
one `{claim}` placeholder (B-036), and a case setting saying whether a tag is
read in exact case or ignoring letter case (B-037). Both are within C-7: each
says how a tag in the companion reads, and neither changes what `SPEC021`
asserts. The limit is C-12: the case setting reaches the reading of a tag and
nothing else, so a claim id in § 3 and § 9 is still matched by the claim grammar
as written.

Ignoring case is an ordinal, culture-invariant fold: the same tag resolves to
the same claim on every machine and under every locale, which the determinism
invariant requires (brief § 9, C-12).

The default manifest declares `@{claim}` and exact case, which is what the
engine does today, so the baseline verdicts hold (C-4, B-016).

## Why

C-7 separates where a rule looks and what a marker says, which the manifest
names, from what the rule asserts, which is the engine's. `SPEC021` asserts one
thing: a claim tag in the companion resolves to a claim in § 3. Under either
case setting that assertion is the same sentence, and the same tag either
resolves or is reported. What the setting changes is which text in the
companion counts as the tag for `B-001`, the same kind of fact as the tag form
and the claim grammar, both already the manifest's.

C-7's own examples of logic are a different contiguity rule and a different
symmetry rule: each would change which trees a rule passes with the reading held
still. The case setting holds the assertion still and changes the reading.

The setting cannot be had through the claim grammar. An inline case option in
the grammar would also make `b-001` a well-formed claim id in § 3, and a tag
would still be compared with a claim exactly, so `@b-001` would still not
resolve to `B-001`.

## Rejected

- **Opening an open question and writing no claim.** The owner had already
  chosen the case setting; only its fit with C-7 was unjudged. Cost of not
  asking: if the owner reads C-7 more strictly, B-037 is withdrawn and its id
  retired.
- **Case-insensitive claim ids everywhere.** That changes what `SPEC030` and
  `SPEC031` accept and how § 9 pairs with § 3, which is rule logic (C-7, § 5
  row 5). Cost: a consumer whose § 3 mixes case is still told so.
- **A general comparison key** naming a culture or a normalisation. Nothing
  asked for it, and each further option is one more reading to keep
  reproducible. Cost: letter case is the only fold a consumer can declare.
- **A regular expression as the tag form.** It would be a second claim
  expression, which C-2 rules out. Cost: a form is literal text and one
  placeholder, nothing more.

Cost of the call: where a consumer's claim grammar admits two claim ids that
differ only in case, a tag read ignoring case resolves to either, and `SPEC021`
is satisfied by one.

## Affects

B-036, B-037, B-038, C-12; read against C-2, C-4, C-7 and § 5 row 5. OQ-8 and
OQ-9 stay open under it.

## Reversal

None.
