---
title: Shared assistant resources
description: Install repository-owned skills and role contracts into both project-local assistants from one tracked source
type: guide
---

# Shared assistant resources

Edit skills in `skills/` and role contracts in `roles/`. These tracked sources
serve both assistants. `AGENTS.md` owns repository instructions; the root
`CLAUDE.md` imports it.

Install [Skillfile](https://github.com/eljulians/skillfile) (tested with 1.7.1)
and Python 3.11 or later, then run from the repository root:

```sh
python3 .agents/install.py
```

This validates and installs the tracked `Skillfile` with `--update` into `.claude/skills/`,
`.claude/agents/` and `.codex/skills/`, then generates `.codex/agents/*.toml`.
Skillfile 1.7.1 skips Codex agents, so the adapter supplies their native
registrations. Each registration gets its name and description from the shared
contract and instructs the agent to read that contract before acting. No model,
permissions or delegation policy is set here. Codex also discovers the shared
`.agents/skills/` sources directly.

`--update` refreshes existing copies; plain `skillfile install` skips them in
1.7.1. The default manifest contains only local entries, so this refresh makes
no network requests.

Run the same command after changing a source or adding a local entry to
`Skillfile`. Repeated installation is supported. Installed copies are gitignored;
edit the shared sources. Start a new assistant session after installation to
refresh discovery. Skillfile copies include each skill's references and scripts.

The default manifest has only local entries and needs no private repository or
network access once Skillfile is installed. `Skillfile.lock` is empty until a
remote dependency is deliberately added. Optional private guides remain outside
the required setup; keep personal dependencies in a separate manifest and use
global targets for personal installation. Avoid names that collide with this
repository's local skills. Existing unrelated tool-local resources are retained.

Verify installation, including a fresh directory and repeated setup:

```sh
python3 .agents/test_install.py
```

The adapter and its verification use only Python's standard library and the
installed Skillfile executable. Product build and test commands remain in
`AGENTS.md`.
