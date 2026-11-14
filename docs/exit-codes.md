---
title: "specht exit codes"
description: "What each exit code of the specht command means"
type: reference
---

# Exit codes

| Exit | Meaning                                                                                                                                                                                                                                                                                                                |
| ---- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `0`  | Clean                                                                                                                                                                                                                                                                                                                  |
| `1`  | Violations (any violation, under `--strict`)                                                                                                                                                                                                                                                                           |
| `2`  | Missing root or manifest, invalid frontmatter-command usage, `--feature` on an `adr` or `lesson` mint or none on a per-Feature one, or a version required without an interactive terminal                                                                                                                              |
| `3`  | Invalid manifest or init source options, an upstream schema fetch or hash check failed, a `mint` entry that opts in `epic`, `feature` or `task`, a `prefix` in an `ids` record's `mint`, or one that names a strategy and gives no id, Snowflake's clock reading earlier included                                      |
| `4`  | Something named on the command line, such as a schema version, rule id, frontmatter kind, document file or a type with no `mint` entry or `ids.<type>.mint`, or whose entry names no strategy, or a `mint --feature` folder holding no Feature's `.spec/`, was not found, or a `facts` path selecting no specification |

For `specht evidence`, `1` means a missing required link or nonpassing required
case, `2` means required evidence is missing, stale or incompatible, `3` means
the analysis configuration is invalid, and `4` means an explicitly named evidence
path or run was not found, or the caller-named report could not be written. Its `0` requires current inputs, required links and
passing outcomes.
