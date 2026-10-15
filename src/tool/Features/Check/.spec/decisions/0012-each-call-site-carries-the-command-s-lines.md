---
title: "Decision 0012: Each call site carries the command's lines"
description: "B-010 holds when the shell, the Specht target and the pre-commit hook carry the command's lines unchanged and in order, a log prefix and a call site's own lines allowed; a root checked through the target carries this repository's source (A-4); B-010 and B-011 are tested, outside the build's no-tests decision"
type: decision
---

# Decision 0012: Each call site carries the command's lines

**Date:** 2026-10-14
**Decided by:** the repository owner, 2026-10-14, answering the implementer's
design pass for item `0031` (§ 7, "Three call sites and the self-check");
recorded by spec-author. The `@B-011` scenario's wording is spec-author's.

## The call

1. **B-010 means the command's lines as each call site carries them.** A shell
   invocation, this repository's `Specht` target and the pre-commit hook, each
   pointed at one root, carry the lines the command prints unchanged and in
   order, and exit with the same code. A log prefix on each line, and a call
   site's own lines around them, are allowed. B-010 is amended in place; its id
   and its exit-code half are unchanged. No wiring changes.
2. **B-010 and B-011 are tested.** Both are this Feature's claims, and both
   acceptance scenarios, `@B-010` and `@B-011`, are written and bound. The
   owner's decision of 2026-10-08 that the build has no tests,
   [`.build/.spec/README.md`](../../../../../../.build/.spec/README.md)
   (`0055-F1`) § 8, does not cover them. That decision's text is unchanged, and
   `0055-F1`'s own claims stay untested by it.
3. **A root checked through the `Specht` target carries this repository's
   source (A-4).** While the target runs the tool as A-2 quotes, it builds the
   tool from `src/tool` under the root it checks, so B-010 holds for such a
   root. No wiring change separates where the tool is built from what it
   checks.
4. **The `@B-011` scenario reads "this repository's own tree"**, as B-011 does,
   where it read "this repository's tree at one commit" (spec-author). B-011's
   text is unchanged.

## Why

- **Only the shell prints the command's lines bare.** The `Specht` target logs
  each line of the command's stdout through NUKE's logger behind a
  `HH:mm:ss [DBG] ` prefix, between its banner and its target table; the hook
  adds its rejection and bypass lines, and Husky its exit line. With the prefix
  removed, the target's lines were the bare command's, byte for byte and in
  order, 156 of them, and every call site exited with the command's code (§ 7,
  measured 2026-10-14). As written, "print the same lines" held at no call site
  but the shell, so no test could pass it without changing the wiring.
- **What B-010 protects is the verdict.** Must-1 asks that a hook, a CI step and
  an agent see the same answer: the same violations, in the same order, and the
  same code. A prefix and framing lines change none of that.
- **The claims are the check's, not the build's.** `0055-F1` § 8 governs the
  build's own claims, and stops `build.feature` from running. B-010 and B-011
  are claims of the check command about how its verdict reaches its call sites.
  Under that decision they would stay `Missing` in § 9 for good, and item
  `0031`, cut to prove them, could not reach done. C-7 already names the
  self-check as the one test that reads this repository's real tree.
- **The root limitation follows from the wiring the owner kept.** The target
  checks `.` under NUKE's root and builds `src/tool` there, so a root without
  this repository's source fails at the build, with a violation's code, before
  any check runs. The hook never meets it: its root is an export of the whole
  index. Stated as an assumption beside A-2, it holds while the target builds
  the tool from source, and is revisited with A-2 when B-014 replaces that.
  An assumption, not a § 5 row: § 5 says what will not be built and why, and
  this is a precondition of B-010's Given, not a capability ruled out.
- **"At one commit" is not what the self-check reads.** The self-check runs
  `./build.sh Specht` and the tool on the checkout as it stands, which may hold
  uncommitted edits; both runs read the same tree, which is all B-011 needs.
  The scenario now says what the claim says.

## Rejected

- **B-010 as written, literal identity of output.** Needs the target to pass
  the command's stdout through without its logger and the hook to print nothing
  of its own: a wiring change the owner did not make. Cost of rejecting: a
  reader of the target's log sees each line behind a prefix.
- **Leaving B-010 and B-011 untested under the build's no-tests decision.**
  Two claims of this Feature with no test, and item `0031` never done. Cost of
  rejecting: an acceptance scenario launches `build.sh` and the hook, with cold
  builds of the build and the tool, on every platform but Windows (§ 7).
- **Separating where the target builds the tool from the root it checks.**
  Would let a root of specifications alone go through the target. A wiring
  change the owner did not make. Cost of rejecting: B-010's root carries this
  repository's source around its specification tree.
- **Recording the root limitation as a § 5 row.** Reasoned above. Cost of
  rejecting: none.
- **Keeping "at one commit" and exporting a commit for the self-check.** The
  root it would check is not the repository root B-011 names (§ 7). Cost of
  rejecting: none.

## Affects

- B-010 (Amended): its lines half; its Source gains the owner, 2026-10-14,
  this decision and A-4.
- A-4 (added).
- B-011: text unchanged; its `@B-011` scenario's Given reworded.
- The `@B-010` scenario: its Then, and a Given for A-4's root.
- C-7 and C-8: unchanged; C-7's self-check is the `@B-011` test, and C-8's
  gating from `0001-F5`'s arrival still applies to both claims.
- `.build/.spec/README.md` (`0055-F1`) § 8, "The build has no tests": unchanged,
  and scoped out for B-010 and B-011.
- § 10. Item `0031`.

## Reversal

None.
