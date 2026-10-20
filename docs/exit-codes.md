---
title: "specht exit codes"
description: "What each exit code of the specht command means"
type: reference
---

# Exit codes

| Exit | Meaning                                                                                                                  |
| ---- | ------------------------------------------------------------------------------------------------------------------------ |
| `0`  | Clean                                                                                                                    |
| `1`  | Violations (any violation, under `--strict`)                                                                             |
| `2`  | Missing root or manifest, invalid frontmatter-command usage, or a version required without an interactive terminal       |
| `3`  | Invalid manifest or init source options, or an upstream schema fetch or hash check failed                                |
| `4`  | Something named on the command line, such as a schema version, rule id, frontmatter kind or document file, was not found |

For `specht evidence`, `1` means a missing required link or nonpassing required
case, `2` means required evidence is missing, stale or incompatible, `3` means
the analysis configuration is invalid, and `4` means an explicitly named evidence
path or run was not found, or the caller-named report could not be written. Its `0` requires current inputs, required links and
passing outcomes.
