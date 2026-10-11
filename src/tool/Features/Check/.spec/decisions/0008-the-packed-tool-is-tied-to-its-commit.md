---
title: "Decision 0008: The packed tool is tied to its commit"
description: "Each assembly in the package names the commit it was built from, its debug information travels in the package and maps its sources to the repository at that commit, and a build made in continuous integration names no directory of the machine that made it"
type: decision
---

# Decision 0008: The packed tool is tied to its commit

**Date:** 2026-10-11
**Decided by:** the repository owner ruled on OQ-3; the three claims, the constraint and what is left unclaimed are spec-author's derivation from that ruling

## The call

The owner, 2026-10-11, on OQ-3:

> OQ-3 source link and deterministic CI build => yes. this will tie a version
> of my binary to a git commit? Yes please!

The purpose is the owner's: a version of the binary is tied to a git commit.
Three claims are written to it, each about the assemblies this repository
builds that the package carries - the tool's and the engine's:

- **B-030, the binary names its commit.** Each assembly states a version that
  names the commit it was built from.
- **B-031, the binary's sources are found at that commit.** The debug
  information the package carries for each assembly maps its source files to
  `https://github.com/RLittlesII/specht` at that commit.
- **B-032, a build made in continuous integration names no directory of the
  machine that made it**, in an assembly or in its debug information.
- **C-11, the debug information travels inside the package**, as a file
  beside each assembly.

Not claimed (OQ-6): that the package rebuilds byte for byte on another
machine, and that the file a tag pushes is itself read.

## Why

- **B-030 is the claim the purpose names.** B-019 ties the package to a
  commit; an installed tool is no longer a package, and the assembly is what
  is left. A pack of commit `dbc060d` on 2026-10-11 showed both assemblies
  already state `0.1.59+dbc060d079`. It holds today and nothing held it,
  which is how the package id was lost (lesson 0001).
- **B-031 is what "source link" adds to it:** from the commit to the lines.
  The same pack showed both debug files already map to the repository at the
  full commit id.
- **B-032 is what "deterministic" can be seen as.** In that pack, made on a
  developer's machine, each assembly and each debug file names the directory
  of the clone it was built in. A build told it is a continuous-integration
  build replaces that directory with a neutral root. It is also the
  repository's own rule that no output carries an absolute path (brief § 9).
- **A file beside the assembly, not embedded.** It is what the SDK already
  packs for a tool, so nothing changes to deliver C-11, and a test reads one
  known place. GitHub Packages is documented as taking no symbol package, and
  the release pushes one file (`0055-F6` C-4), so the debug information has
  to be in the package to be anywhere. That documentation was not checked
  against the feed here.

## What a test reads, and what stays unproven

- **B-030.** The version each of the two assemblies records, read from the
  assembly inside the package, against the commit checked out.
- **B-031.** The source mapping in each of the two debug files inside the
  package: its address names the repository and the commit checked out.
  Unproven: the address comes from the clone's `origin`, so the claim holds
  only in a clone of the repository.
- **B-032.** On a developer's machine the ordinary build does not meet the
  Given, so the test builds the tool with the signal a runner gives, packs
  that build, and searches the two assemblies and the two debug files for the
  directory it built in. On a runner the tier's own build already meets the
  Given. Unproven: that the runner gives the signal - a test that sets it
  proves the build obeys it; and that the file a tag pushes was built so,
  which nothing reads.

## Rejected

**Debug information embedded in the assembly.** One file fewer. Rejected: a
setting to change for no gain to the owner's purpose. Cost of rejecting: a
debug file can be separated from its assembly after install.

**B-032 for every build.** A test would need no special build. Rejected: a
developer's own build then names no directory of the developer's machine,
which is what a debugger on that machine uses to find the source. Cost of
rejecting: the test has to make a build the way a runner does.

**A claim that the package rebuilds byte for byte.** The strongest reading of
"deterministic". Rejected for now: OQ-6. It needs two builds on two machines
to fail. Cost of rejecting: nothing shows two machines agree.

**Leaving B-030 unclaimed because it already holds.** Rejected: lesson 0001.

## Affects

- B-030 to B-032, added. C-11, added.
- § 5 row 14 amended, row 16 withdrawn, row 20 added.
- OQ-3 resolved; OQ-6 raised.
- `check.feature`: `@B-030`, `@B-031`, `@B-032`.
- Decision 0007: reversed in part.

## Reversal

None.
