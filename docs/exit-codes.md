---
title: "specht exit codes"
description: "What each exit code of the specht command means"
type: reference
---

# Exit codes

| Exit | Meaning                                                               |
| ---- | --------------------------------------------------------------------- |
| `0`  | Clean                                                                 |
| `1`  | Violations (any violation, under `--strict`)                          |
| `2`  | Missing root or manifest                                              |
| `3`  | Invalid manifest                                                      |
| `4`  | Something named on the command line, such as a rule id, was not found |
