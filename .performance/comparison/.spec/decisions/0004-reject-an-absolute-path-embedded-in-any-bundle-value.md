---
title: "Decision 0004: Reject an absolute path embedded in any bundle value"
description: "A report bundle whose JSON entries hold an absolute path inside a longer string value is rejected when built and when validated, never rewritten"
type: decision
---

# Decision 0004: Reject an absolute path embedded in any bundle value

**Date:** 2026-11-01
**Decided by:** owner ("Widen the check"), item `0213`

## The call

A report bundle is rejected, never rewritten, when any string value of its JSON
entries - generated or raw - contains an embedded absolute path. This holds when
the bundle is built, where nothing is written, and when it is validated.

A string value **contains an embedded absolute path** when, at the start of the
value or directly after whitespace or one of `=` `,` `;` `:` `(` `)` `[` `]`
`"` `'`, the text begins with any of:

1. `/`, one or more characters that are none of `/`, `\`, whitespace or those
   separators, then `/` - for example `/home/runner`;
2. `~/` or `~\`;
3. an ASCII letter, `:`, then `\` or `/` - for example `C:\` or `D:/`;
4. `\\` - a UNC path;
5. `file:`, compared ignoring ASCII case, then `/` - for example `file:///tmp`.

Property names are not values and are not examined.

| Value                                | Verdict                                            |
| ------------------------------------ | -------------------------------------------------- |
| `Root=/home/runner/work`             | embedded absolute path (rule 1, after `=`)         |
| `--artifacts /home/runner/out`       | embedded absolute path (rule 1, after a space)     |
| `Path=C:\agent\work`, `D:/a/specht`  | embedded absolute path (rule 3)                    |
| `read from file:///tmp/x.json`       | embedded absolute path (rule 5)                    |
| `\\server\share\x`, `~/x`            | embedded absolute path (rules 4 and 2)             |
| `ns/op`, `1/2`, `raw/current/x.json` | not a path: no boundary precedes the `/`           |
| `/p:Configuration=Release`           | not a path: `/p` has no second `/`                 |
| `https://example.com/a`              | not a path: the `/` after `:` has an empty segment |

The existing whole-value rule is unchanged: a raw export string value that is
wholly a rooted path - beginning with `/`, `~`, `\` or a drive letter - becomes
`[path removed]` and the file records `absolute-paths-removed`. The embedded
check applies to what remains, so a raw export value that is wholly a path is
removed and recorded, and one that only contains a path rejects the bundle. A
generated entry holding either form is rejected.

A report observation's `locator` that the agreed locator rule accepts - a JSON
pointer whose parent resolves inside the raw export its `sourceFile` cites - is
not a path and is exempt from both checks, as it already is from the
whole-value check. A locator that does not resolve stays rejected.

## Why

Brief § 9 forbids an absolute path in any output, and `0109-F5` C-8 requires
every reported path to be repository-relative. Checking only whole values let a
runner path inside a session invocation, a workload parameter or a reason reach
a durable release asset. A rewrite would alter recorded evidence without a
reliable boundary for where the path ends; rejecting keeps evidence exact and
makes the cause visible.

## Rejected

- **Check whole values only:** leaves `Root=/home/runner/…` and `file:///…` in
  a published bundle. Cost of rejection: a value that merely carries a path now
  fails the bundle.
- **Rewrite the embedded path:** guesses where the path ends and changes
  evidence the report cites. Cost of rejection: the producer of the value must
  stop emitting it.
- **Treat any `/` as a path:** rejects units, ratios, relative bundle paths and
  locators. Cost of rejection: the boundary rules above must be implemented
  exactly.

## Affects

`0109-F5` B-016, B-022; C-8, C-11; § 7 "Rendering and durable publication".

## Reversal

None.
